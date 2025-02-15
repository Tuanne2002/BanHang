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
    public class ShippingController : Controller
	{
        private readonly DataContext _dataContext;

        public ShippingController(DataContext context)
        {
            _dataContext = context;
        }
        
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
		{
            var shippingList = await _dataContext.Shippings.ToListAsync();
            ViewBag.Shippings = shippingList;
			return View();
		}
        [HttpPost("StoreShipping")]
        public async Task<IActionResult> StoreShipping(ShippingModel shippingModel, string phuong, string quan, string tinh, decimal price)
        {
            shippingModel.ThanhPho = tinh;
            shippingModel.Huyen = quan;
            shippingModel.Xa = phuong;
            shippingModel.Price = price;

            try
            {
                var exitstingShipping = await _dataContext.Shippings
                    .AnyAsync(x => x.ThanhPho == tinh && x.Huyen == quan && x.Xa == phuong);

                if (exitstingShipping)
                {
                    return Ok(new { duplicate = true, message = "Dữ liệu bị trùng!!!" });
                }
                _dataContext.Shippings.Add(shippingModel);
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Thêm giá vận chuyển thành công!!!" });
            }
            catch (Exception ) {
                return StatusCode(500,"An error occurred while adding shipping");
            }
        }
        [HttpGet("Delete/{Id}")]
        public async Task<IActionResult> Delete(int Id)
        {
            ShippingModel shipping = await _dataContext.Shippings.FindAsync(Id);
            _dataContext.Shippings.Remove(shipping);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Shipping đã được xóa thành công";
            return RedirectToAction("Index","Shipping");
        }
    }
}
