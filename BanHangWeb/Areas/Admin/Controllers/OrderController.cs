using BanHangWeb.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BanHangWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize]
	public class OrderController : Controller
	{
		

		private readonly DataContext _dataContext;
		public OrderController(DataContext context)
		{
			_dataContext = context;
		}
		public async Task<IActionResult> Index()
		{
			return View(await _dataContext.Orders.OrderByDescending(p => p.Id).ToListAsync());
		}
		[HttpGet]
		[Route("ViewOrder")]
		public async Task<IActionResult> ViewOrder(string ordercode)
		{
			var DetailsOrder = await _dataContext.OrderDetails.Include(od => od.Product).Where(od => od.OrderCode == ordercode).ToListAsync();
			//lấy shipping
			var shippingCost = _dataContext.Orders.Where(o=>o.OrderCode == ordercode).First();
			ViewBag.ShippingCost = shippingCost.ShippingCost;
			ViewBag.Status = shippingCost.Status;

			return View(DetailsOrder);
		}
		[HttpPost]
		[Route("UpdateOrder")]
		public async Task<IActionResult> UpdateOrder(string ordercode,int status)
		{
			var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);
			if (order == null)
			{
				return NotFound();
			}
			order.Status = status;
			try
			{
				await _dataContext.SaveChangesAsync();
				return Ok(new { success = true, message = "Order status updated successfully" });
			}
			catch (Exception ex) {
				return StatusCode(500, "An error occurred while updating the order status");
			}

		}

        [HttpGet]
        [Route("PaymentMomoInfo")]
        public async Task<IActionResult> PaymentMomoInfo(string orderId)
        {
            var momoInfo = await _dataContext.MomoOptions.FirstOrDefaultAsync(m => m.OrderId == orderId);

            if (momoInfo == null)
            {
                return NotFound();
            }

            return View(momoInfo);
        }


    }
}
