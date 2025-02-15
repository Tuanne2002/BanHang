using BanHangWeb.Models;
using BanHangWeb.Models.ViewModels;
using BanHangWeb.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BanHangWeb.Controllers
{
    public class ProductController : Controller
    {
		private readonly DataContext _dataContext;
        
        public IActionResult Index()
        {
            return View();
        }
        public ProductController(DataContext context)
        {
            _dataContext = context;
            
        }
        public async Task<IActionResult>  Details(long Id )
        {
            if(Id == null)
            {
                return RedirectToAction("Index");
            }
			var productsById = _dataContext.Products
                .Include(p =>p.Ratings)
                .Where(p => p.Id == Id).FirstOrDefault();

            var relatedProducts = await _dataContext.Products
                .Where(p=>p.CategoryId == productsById.CategoryId && p.Id != productsById.Id)
                .Take(4)
                .ToListAsync();
            ViewBag.RelatedProducts = relatedProducts;

            var viewModel = new ProductDetailsViewModel
            {
                ProductDetails = productsById
                
            };

			return View(viewModel);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            var products = await _dataContext.Products
                .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
                .ToListAsync();
            ViewBag.Keyword = searchTerm;
            return View(products);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CommentProduct(RatingModel rating)
        {
            if (ModelState.IsValid)
            {
                var ratingEntity = new RatingModel
                {
                    ProductId = rating.ProductId,
                    Name = rating.Name,
                    Email = rating.Email,
                    Comment = rating.Comment,
                    Star = rating.Star
                };
                _dataContext.Ratings.Add(ratingEntity);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Thêm đánh giá sản phẩm thành công";

                return Redirect(Request.Headers["Referer"]);
            }
            else
            {
                TempData["error"] = "Có vài thứ đang bị lỗi";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values) {
                    foreach (var error in value.Errors) { 
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n ", errors);
                return RedirectToAction("Details", new { id = rating.ProductId });
            }

            return Redirect(Request.Headers["Referer"]);
        }
        //public async Task<IActionResult> Search(string searchTerm)
        //{
        //    try
        //    {
        //        // Gọi API
        //        var response = await _httpClient.GetAsync($"https://dummyjson.com/products/search?q={Uri.EscapeDataString(searchTerm)}");

        //        List<ProductModel> combinedProducts = new List<ProductModel>();

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var content = await response.Content.ReadAsStringAsync();
        //            var apiResult = JsonSerializer.Deserialize<ApiProductSearchResult>(content, new JsonSerializerOptions
        //            {
        //                PropertyNameCaseInsensitive = true
        //            });

        //            // Chuyển đổi sản phẩm từ API
        //            var apiProducts = apiResult?.Products.Select(apiProduct => new ProductModel
        //            {
        //                Id = apiProduct.Id,
        //                Name = apiProduct.Title,
        //                Description = apiProduct.Description,
        //                Price = apiProduct.Price,
        //                Image = apiProduct.Thumbnail,

        //                // Tìm category và brand mặc định
        //                Category = _dataContext.Categories.FirstOrDefault(),
        //                Brand = _dataContext.Brands.FirstOrDefault(),

        //                BrandId = _dataContext.Brands.First().Id,
        //                CategoryId = _dataContext.Categories.First().Id
        //            }).ToList() ?? new List<ProductModel>();

        //            // Tìm kiếm trong local database
        //            var localProducts = await _dataContext.Products
        //                .Include(p => p.Category)
        //                .Include(p => p.Brand)
        //                .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
        //                .ToListAsync();

        //            // Kết hợp kết quả
        //            combinedProducts = apiProducts.Concat(localProducts).Distinct().ToList();
        //        }
        //        else
        //        {
        //            // Nếu API gặp lỗi, chỉ tìm kiếm trong cơ sở dữ liệu local
        //            combinedProducts = await _dataContext.Products
        //                .Include(p => p.Category)
        //                .Include(p => p.Brand)
        //                .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
        //                .ToListAsync();
        //        }

        //        ViewBag.Keyword = searchTerm;
        //        return View(combinedProducts);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Ghi log lỗi
        //        Console.WriteLine($"Lỗi tìm kiếm: {ex.Message}");
        //        ModelState.AddModelError("", "Có lỗi xảy ra khi tìm kiếm: " + ex.Message);
        //        return View(new List<ProductModel>());
        //    }
        //}

        //// Giữ nguyên các model API như trước
        //public class ApiProductSearchResult
        //{
        //    [JsonPropertyName("products")]
        //    public List<ApiProductModel> Products { get; set; }
        //}

        //public class ApiProductModel
        //{
        //    [JsonPropertyName("id")]
        //    public long Id { get; set; }

        //    [JsonPropertyName("title")]
        //    public string Title { get; set; }

        //    [JsonPropertyName("description")]
        //    public string Description { get; set; }

        //    [JsonPropertyName("price")]
        //    public decimal Price { get; set; }

        //    [JsonPropertyName("thumbnail")]
        //    public string Thumbnail { get; set; }
        //}
    }
}
