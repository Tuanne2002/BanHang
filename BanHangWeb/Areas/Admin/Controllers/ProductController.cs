using BanHangWeb.Models;
using BanHangWeb.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace BanHangWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    [Route("Admin/[controller]")]

    [Authorize]
    public class ProductController : Controller
	{
		private readonly DataContext _dataContext;
		private readonly IWebHostEnvironment _webHostEnvironment;

		//PhongVu
		//private readonly PhongVuScraper _phongVuScraper;
		public ProductController(DataContext context, IWebHostEnvironment webHostEnvironment) 
		{
			_dataContext = context;
			_webHostEnvironment = webHostEnvironment;
			//_phongVuScraper = phongVuScraper;
		}
        //public async Task<IActionResult> Index()
        //{

        //	return View(await _dataContext.Products.OrderByDescending(p=>p.Id).Include(p => p.Category).Include(p => p.Brand).ToListAsync());
        //}

        [HttpGet("Index")]
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<ProductModel> product = _dataContext.Products.OrderByDescending(p => p.Id).Include(p => p.Category).Include(p => p.Brand).ToList(); //33 datas


            const int pageSize = 10; //10 items/trang

            if (pg < 1) //page < 1;
            {
                pg = 1; //page ==1
            }
            int recsCount = product.Count(); //33 items;

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize; //(3 - 1) * 10; 

            //category.Skip(20).Take(10).ToList()

            var data = product.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            return View(data);
        }
        [HttpGet("Create")]
        public IActionResult Create()
        {
			ViewBag.Categories = new SelectList(_dataContext.Categories,"Id","Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
            return View();
        }
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel product)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name",product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name",product.BrandId);

			if (ModelState.IsValid) {
				product.Slug = product.Name.Replace(" ", "-");
				var slug = await _dataContext.Products.FirstOrDefaultAsync(p=> p.Slug == product.Slug);
				if (slug != null) {
					ModelState.AddModelError("", "Sản phẩm đã có trong database");
					return View(product);
				}
				
				if(product.ImageUpload != null)
				{
					string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath,"media/products");
					string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
					string filePath = Path.Combine(uploadsDir, imageName);

					FileStream fs = new FileStream(filePath, FileMode.Create);
					await product.ImageUpload.CopyToAsync(fs);
					fs.Close();
					product.Image = imageName;
				}
				
				_dataContext.Add(product);
				await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm sản phẩm thành công!!!";
				return RedirectToAction("Index");
            }
			else
			{
				TempData["error"] = "Model đang bị lỗi!!!";
				List<string> errors = new List<string>();
				foreach (var value in ModelState.Values) {
					foreach (var error in value.Errors) {
						errors.Add(error.ErrorMessage);
					}
				}
				string errorMessage = string.Join("\n", errors);
				return BadRequest(errorMessage);
			}
			return View(product);
        }

        [HttpGet("Edit/{Id}")]
        public async Task<IActionResult> Edit(long Id)
		{
			ProductModel product = await _dataContext.Products.FindAsync(Id);
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            return View(product);
		}
        [HttpPost("Edit/{Id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long Id,ProductModel product)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            var existed_product = _dataContext.Products.Find(product.Id);

            if (ModelState.IsValid)
            {
                product.Slug = product.Name.Replace(" ", "-");
                var slug = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Sản phẩm đã có trong database");
                    return View(product);
                }

                if (product.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    //xoa anh cu
                    string oldfilePath = Path.Combine(uploadsDir, existed_product.Image);

                    try
                    {
                        if (System.IO.File.Exists(oldfilePath))
                        {
                            System.IO.File.Delete(oldfilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "An error occurred while deleting the product image.");
                    }

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    existed_product.Image = imageName;
                    
                }

                existed_product.Name = product.Name;
                existed_product.Description = product.Description;
                existed_product.Price = product.Price;
                existed_product.CategoryId = product.CategoryId;
                existed_product.BrandId = product.BrandId;

                _dataContext.Update(existed_product);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật sản phẩm thành công!!!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Model đang bị lỗi!!!";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
            return View(product);
        }
        [HttpGet("Delete/{Id}")]
        public async Task<IActionResult> Delete(long Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);

            if (product == null)
            {
                return NotFound();
            }

            string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
            string oldfilePath = Path.Combine(uploadsDir, product.Image);

            try
            {
                if (System.IO.File.Exists(oldfilePath))
                {
                    System.IO.File.Delete(oldfilePath);
                }
            }
            catch (Exception ex) {
                ModelState.AddModelError("", "An error occurred while deleting the product image.");
            }
            
            _dataContext.Products.Remove(product);
            await _dataContext.SaveChangesAsync();
            TempData["error"] = "Sản phẩm đã xóa!!!";
            return RedirectToAction("Index","Product");
        }

        [HttpGet("AddQuantity/{Id}")]
        public async Task<IActionResult> AddQuantity(int Id)
        {
            var  productbyquantity = await _dataContext.ProductQuantities.Where(x => x.ProductId == Id).ToListAsync();
            ViewBag.ProductQuantity = productbyquantity;
            ViewBag.Id = Id;
            return View();
        }

        [HttpPost("StoreProductQuantity")]
        [ValidateAntiForgeryToken]
        public IActionResult StoreProductQuantity(ProductQuantityModel productQuantityModel)
        {
            var product = _dataContext.Products.Find(productQuantityModel.ProductId);

            if(product == null) { return NotFound(); }
            product.Quantity += productQuantityModel.Quantity;

            productQuantityModel.Quantity = productQuantityModel.Quantity;
            productQuantityModel.ProductId = productQuantityModel.ProductId;
            productQuantityModel.DateCreated = DateTime.Now;

            _dataContext.Add(productQuantityModel);
            _dataContext.SaveChangesAsync();
            productQuantityModel.DateCreated = DateTime.Now;
            _dataContext.Add(productQuantityModel);

       //     // Tính tổng số lượng từ bảng ProductQuantities và cập nhật vào bảng Products
       //     product.Quantity = await _dataContext.ProductQuantities
       //.Where(pq => pq.ProductId == productQuantityModel.ProductId)
       //.SumAsync(pq => (int?)pq.Quantity) ?? 0;
       //     _dataContext.Update(product);
       //     await _dataContext.SaveChangesAsync();
       
            TempData["success"] = "Thêm số lượng sản phẩm thành công";
            return RedirectToAction("AddQuantity", "Product", new { Id = productQuantityModel.ProductId });
        }

        //[HttpGet]
        //[Route("Scrape")]
        //public async Task<IActionResult> ScrapeProducts()
        //{
        //    string urlPhongVu = "https://phongvu.vn/laptop-chinh-hang";
        //    var products = await _phongVuScraper.GetLaptopProducts(urlPhongVu);

        //    if (products != null)
        //    {
        //        //mapping Category and Brand
        //        foreach (var product in products)
        //        {
        //            //Map category
        //            var category = _dataContext.Categories.FirstOrDefault(x => x.Name.Contains("Laptop"));
        //            if (category != null)
        //            {
        //                product.CategoryId = category.Id;
        //            }
        //            //Map brand
        //            if (product.Name.Contains("Apple"))
        //            {
        //                var brand = _dataContext.Brands.FirstOrDefault(x => x.Name.Contains("Apple"));
        //                if (brand != null)
        //                {
        //                    product.BrandId = brand.Id;
        //                }
        //            }
        //            else if (product.Name.Contains("Asus"))
        //            {
        //                var brand = _dataContext.Brands.FirstOrDefault(x => x.Name.Contains("Asus"));
        //                if (brand != null)
        //                {
        //                    product.BrandId = brand.Id;
        //                }
        //            }
        //            else if (product.Name.Contains("Acer"))
        //            {
        //                var brand = _dataContext.Brands.FirstOrDefault(x => x.Name.Contains("Acer"));
        //                if (brand != null)
        //                {
        //                    product.BrandId = brand.Id;
        //                }
        //            }
        //            else if (product.Name.Contains("Lenovo"))
        //            {
        //                var brand = _dataContext.Brands.FirstOrDefault(x => x.Name.Contains("Lenovo"));
        //                if (brand != null)
        //                {
        //                    product.BrandId = brand.Id;
        //                }
        //            }
        //            else
        //            {
        //                var brand = _dataContext.Brands.FirstOrDefault(x => x.Name.Contains("Khác"));
        //                if (brand != null)
        //                {
        //                    product.BrandId = brand.Id;
        //                }
        //            }

        //            //add the categories and brands for view
        //            var categories = _dataContext.Categories.ToList();
        //            var brands = _dataContext.Brands.ToList();
        //            ViewBag.Categories = categories;
        //            ViewBag.Brands = brands;

        //        }
        //        return View(products);
        //    }
        //    else
        //    {
        //        TempData["error"] = "Không có sản phẩm từ Phong Vũ";
        //        return RedirectToAction("Index", "Product");
        //    }
        //}
    }
}
