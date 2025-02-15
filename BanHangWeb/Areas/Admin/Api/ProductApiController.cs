using BanHangWeb.Models;
using BanHangWeb.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BanHangWeb.Areas.Admin.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductApiController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ProblemDetailsFactory _problemDetailsFactory;

        public ProductApiController(DataContext context, IWebHostEnvironment webHostEnvironment, ProblemDetailsFactory problemDetailsFactory)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
            _problemDetailsFactory = problemDetailsFactory;
        }

        /// <summary>
        /// Lấy danh sách tất cả sản phẩm (có phân trang)
        /// </summary>
        /// <param name="pg">Số trang</param>
        /// <param name="pageSize">Số lượng sản phẩm trên mỗi trang</param>
        /// <returns>Danh sách các sản phẩm</returns>
        /// <response code="200">Trả về danh sách sản phẩm thành công</response>
        /// <response code="500">Lỗi server</response>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResult<ProductModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<PaginatedResult<ProductModel>>> GetProducts(int pg = 1, int pageSize = 10)
        {
            try
            {
                var query = _dataContext.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .OrderByDescending(p => p.Id);

                int recsCount = await query.CountAsync();
                if (pg < 1) { pg = 1; }

                var pager = new Paginate(recsCount, pg, pageSize);
                int recSkip = (pg - 1) * pageSize;

                var data = await query.Skip(recSkip).Take(pager.PageSize).ToListAsync();

                var result = new PaginatedResult<ProductModel>
                {
                    Data = data,
                    Pager = pager
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                var problemDetails = _problemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: (int)HttpStatusCode.InternalServerError, detail: ex.Message);
                return new ObjectResult(problemDetails) { StatusCode = (int)HttpStatusCode.InternalServerError };
            }
        }


        /// <summary>
        /// Lấy thông tin chi tiết một sản phẩm
        /// </summary>
        /// <param name="id">ID của sản phẩm</param>
        /// <returns>Thông tin sản phẩm</returns>
        /// <response code="200">Trả về sản phẩm thành công</response>
        /// <response code="404">Không tìm thấy sản phẩm</response>
        /// <response code="500">Lỗi server</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<ProductModel>> GetProduct(long id)
        {
            try
            {
                var product = await _dataContext.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    return NotFound();
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                var problemDetails = _problemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: (int)HttpStatusCode.InternalServerError, detail: ex.Message);
                return new ObjectResult(problemDetails) { StatusCode = (int)HttpStatusCode.InternalServerError };
            }
        }


        /// <summary>
        /// Tạo mới sản phẩm
        /// </summary>
        /// <param name="product">Thông tin sản phẩm</param>
        /// <returns>Thông tin sản phẩm vừa tạo</returns>
        /// <response code="201">Trả về sản phẩm vừa tạo thành công</response>
        /// <response code="400">Thông tin sản phẩm không hợp lệ</response>
        /// <response code="401">Không có quyền truy cập</response>
        /// <response code="500">Lỗi server</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ProductModel), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<ProductModel>> CreateProduct([FromForm] ProductModel product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                product.Slug = product.Name.Replace(" ", "-");
                var existingProduct = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug);
                if (existingProduct != null)
                {
                    var problemDetails = _problemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: (int)HttpStatusCode.BadRequest, title: "Sản phẩm đã tồn tại");
                    return new ObjectResult(problemDetails) { StatusCode = (int)HttpStatusCode.BadRequest };
                }

                if (product.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");

                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    if (filePath.Length > 260)
                    {
                        var problemDetails = _problemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: (int)HttpStatusCode.BadRequest, title: "Đường dẫn file quá dài");
                        return new ObjectResult(problemDetails) { StatusCode = (int)HttpStatusCode.BadRequest };

                    }
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageUpload.CopyToAsync(fileStream);
                    }

                    product.Image = imageName;
                }

                _dataContext.Products.Add(product);
                await _dataContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                var problemDetails = _problemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: (int)HttpStatusCode.InternalServerError, detail: ex.Message);
                return new ObjectResult(problemDetails) { StatusCode = (int)HttpStatusCode.InternalServerError };
            }
        }

        /// <summary>
        /// Cập nhật thông tin sản phẩm
        /// </summary>
        /// <param name="id">ID của sản phẩm cần cập nhật</param>
        /// <param name="product">Thông tin sản phẩm cần cập nhật</param>
        /// <returns>Không trả về nội dung</returns>
        /// <response code="204">Cập nhật sản phẩm thành công</response>
        /// <response code="400">Id không khớp với Id sản phẩm</response>
        /// <response code="404">Không tìm thấy sản phẩm</response>
        /// <response code="401">Không có quyền truy cập</response>
        /// <response code="500">Lỗi server</response>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateProduct(long id, [FromForm] ProductModel product)
        {
            if (id != product.Id)
            {
                var problemDetails = _problemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: (int)HttpStatusCode.BadRequest, title: "Id không khớp với Id sản phẩm");
                return new ObjectResult(problemDetails) { StatusCode = (int)HttpStatusCode.BadRequest };
            }

            try
            {
                var existingProduct = await _dataContext.Products.FindAsync(id);
                if (existingProduct == null)
                {
                    return NotFound();
                }
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.BrandId = product.BrandId;
                existingProduct.Slug = product.Name.Replace(" ", "-");
                var slug = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug && p.Id != product.Id);

                if (slug != null)
                {
                    var problemDetails = _problemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: (int)HttpStatusCode.BadRequest, title: "Sản phẩm đã tồn tại");
                    return new ObjectResult(problemDetails) { StatusCode = (int)HttpStatusCode.BadRequest };
                }


                if (product.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);
                    if (filePath.Length > 260)
                    {
                        var problemDetails = _problemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: (int)HttpStatusCode.BadRequest, title: "Đường dẫn file quá dài");
                        return new ObjectResult(problemDetails) { StatusCode = (int)HttpStatusCode.BadRequest };
                    }
                    if (!string.IsNullOrEmpty(existingProduct.Image))
                    {
                        string oldFilePath = Path.Combine(uploadsDir, existingProduct.Image);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }

                    }
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageUpload.CopyToAsync(fileStream);
                    }
                    existingProduct.Image = imageName;
                }

                await _dataContext.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                var problemDetails = _problemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: (int)HttpStatusCode.InternalServerError, detail: ex.Message);
                return new ObjectResult(problemDetails) { StatusCode = (int)HttpStatusCode.InternalServerError };
            }
        }

        /// <summary>
        /// Xóa sản phẩm
        /// </summary>
        /// <param name="id">ID của sản phẩm cần xóa</param>
        /// <returns>Không trả về nội dung</returns>
        /// <response code="204">Xóa sản phẩm thành công</response>
        /// <response code="404">Không tìm thấy sản phẩm</response>
        /// <response code="401">Không có quyền truy cập</response>
        ///  <response code="500">Lỗi server</response>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteProduct(long id)
        {
            try
            {
                var product = await _dataContext.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                if (!string.IsNullOrEmpty(product.Image))
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    string filePath = Path.Combine(uploadsDir, product.Image);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                _dataContext.Products.Remove(product);
                await _dataContext.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                var problemDetails = _problemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: (int)HttpStatusCode.InternalServerError, detail: ex.Message);
                return new ObjectResult(problemDetails) { StatusCode = (int)HttpStatusCode.InternalServerError };
            }
        }

        /// <summary>
        /// Thêm số lượng cho sản phẩm
        /// </summary>
        /// <param name="productQuantityModel"></param>
        /// <returns>Thông tin số lượng sản phẩm vừa thêm</returns>
        /// <response code="201">Trả về sản phẩm vừa thêm số lượng thành công</response>
        /// <response code="404">Không tìm thấy sản phẩm</response>
        /// <response code="500">Lỗi server</response>
        [HttpPost("add-quantity")]
        [Authorize]
        [ProducesResponseType(typeof(ProductQuantityModel), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<ProductQuantityModel>> AddQuantityProduct(ProductQuantityModel productQuantityModel)
        {
            try
            {
                var product = await _dataContext.Products.FindAsync(productQuantityModel.ProductId);
                if (product == null)
                {
                    return NotFound();
                }
                product.Quantity += productQuantityModel.Quantity;
                productQuantityModel.DateCreated = DateTime.Now;
                _dataContext.Add(productQuantityModel);
                await _dataContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productQuantityModel);
            }
            catch (Exception ex)
            {
                var problemDetails = _problemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: (int)HttpStatusCode.InternalServerError, detail: ex.Message);
                return new ObjectResult(problemDetails) { StatusCode = (int)HttpStatusCode.InternalServerError };
            }
        }

        private bool ProductExists(long id)
        {
            return _dataContext.Products.Any(e => e.Id == id);
        }

        // Helper class for pagination
        public class PaginatedResult<T>
        {
            public List<T> Data { get; set; } = new List<T>();
            public Paginate Pager { get; set; } = new Paginate(0, 1, 10);
        }
    }
}