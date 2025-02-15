using BanHangWeb.Models;
using BanHangWeb.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BanHangWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize]
	[Route("Admin/[controller]")]
	public class CouponController : Controller
	{

		private readonly DataContext _dataContext;
		public CouponController(DataContext dataContext) {
			_dataContext = dataContext;
		}

		public async Task<IActionResult> Index()
		{
            var coupon_list = await _dataContext.Coupons.ToListAsync();
            ViewBag.Coupons = coupon_list;
			return View();
		}
        [HttpPost("Create")]

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CouponModel coupon)
        {

            if (ModelState.IsValid)
            {
                



                _dataContext.Add(coupon);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm coupon thành công!!!";
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
            return View(coupon);
        }
    }
}
