﻿using BanHangWeb.Models;
using BanHangWeb.Models.ViewModels;
using BanHangWeb.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BanHangWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext _dataContext;
		private UserManager<AppUserModel> _userManager;

		private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, DataContext context, UserManager<AppUserModel> userManagerm)
        {
            _logger = logger;
            _dataContext = context;
			_userManager = userManagerm;
		}

        public IActionResult Index()
        {
            var products = _dataContext.Products.Include("Category").Include("Brand").ToList();
            var sliders = _dataContext.Sliders.Where(s=>s.Status==1).ToList();
            ViewBag.Sliders = sliders;
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statuscode)
        {
            if (statuscode == 404) {
                return View("NotFound");
            }
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Contact()
        {
            return View();
        }
        public async Task<IActionResult> Compare()
        {
            var compare_product = await (from c in _dataContext.Compares
                                         join p in _dataContext.Products on c.ProductId equals p.Id
                                         join u in _dataContext.Users on c.UserId equals u.Id
                                         select new CompareViewModel { User = u, Product = p, Compare = c })
                                  .ToListAsync();
            return View(compare_product);
        }

        public async Task<IActionResult> DeleteWishList(int Id)
        {
            WishlistModel wishlist = await _dataContext.Wishlists.FindAsync(Id);
            _dataContext.Wishlists.Remove(wishlist);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Yêu Thích đã xóa!!!";
            return RedirectToAction("WishList","Home");
        }
        public async Task<IActionResult> DeleteCompare(int Id)
        {
            CompareModel compare = await _dataContext.Compares.FindAsync(Id);
            _dataContext.Compares.Remove(compare);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Sản Phẩm so sánh đã xóa!!!";
            return RedirectToAction("Compare", "Home");
        }
        public async Task<IActionResult> WishList()
        {
            var wishlist_product = await (from w in _dataContext.Wishlists
                                          join p in _dataContext.Products on w.ProductId equals p.Id
                                          join u in _dataContext.Users on w.UserId equals u.Id
                                          select  new WishListViewModel{User = u, Product = p, Wishlist =w })
                                          .ToListAsync();
            return View(wishlist_product);
        }

		[HttpPost]
		public async Task<IActionResult> AddWishList(long Id)
		{
            var user = await _userManager.GetUserAsync(User);

            var wishlistProduct = new WishlistModel
            {
                ProductId = Id,
                UserId = user.Id
            };
            _dataContext.Wishlists.Add(wishlistProduct);

            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Thêm sản phẩm yêu thích thành công!!!" });
            }
            catch (Exception) {
                return StatusCode(500, "Lỗi khi thêm sản phẩm yêu thích");
            }
			
		}
		
		[HttpPost]
		public async Task<IActionResult> AddCompare(long Id)
		{
			var user = await _userManager.GetUserAsync(User);

			var compareProduct = new CompareModel
			{
				ProductId = Id,
				UserId = user.Id
			};
			_dataContext.Compares.Add(compareProduct);

			try
			{
				await _dataContext.SaveChangesAsync();
				return Ok(new { success = true, message = "Thêm so sánh snar phẩm thành công!!!" });
			}
			catch (Exception)
			{
				return StatusCode(500, "Lỗi khi thêm sản phẩm yêu thích");
			}

		}

    }
}
