using BanHangWeb.Areas.Admin.Repository;
using BanHangWeb.Models;
using BanHangWeb.Repository;
using BanHangWeb.Services.Momo;
using BanHangWeb.Services.Vnpay;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;

namespace BanHangWeb.Controllers
{
	public class CheckoutController : Controller
	{
		
		private readonly DataContext _dataContext;
		private readonly IEmailSender _emailSender;
		private readonly IMomoService _momoService;
		private readonly IVnPayService _vnPayService;

		public CheckoutController(IEmailSender emailSender,DataContext context, IMomoService momoService, IVnPayService vnPayService)
		{
			_dataContext = context;
			_emailSender = emailSender;
			_momoService = momoService;
			_vnPayService = vnPayService;
		}
		public async Task<IActionResult> Checkout(string OrderId)
		{
			var userEmail = User.FindFirstValue(ClaimTypes.Email);
			if (userEmail == null) {
				return RedirectToAction("Login", "Account");
			}
			else
			{
				var ordercode = Guid.NewGuid().ToString();
				var orderItem = new OrderModel();
				orderItem.OrderCode = ordercode;
				//Nhận shipping từ côkies
				var shippingPriceCookie = Request.Cookies["ShippingPrice"];
				decimal shippingPrice = 0;
				//Nhận coupon từ cookies
				var coupon_code = Request.Cookies["CouponTitle"];

				if (shippingPriceCookie != null)
				{
					var shippingPriceJson = shippingPriceCookie;
					shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
				}
				orderItem.ShippingCost = shippingPrice;
				orderItem.CouponCode = coupon_code;
				orderItem.UserName = userEmail;
				if (OrderId != null)
				{
					orderItem.PaymentMethod = OrderId;
				}
				else
				{
					orderItem.PaymentMethod = "COD";
				}
				orderItem.Status = 1;
				orderItem.CreatedDate = DateTime.Now;
				_dataContext.Add(orderItem);
				_dataContext.SaveChanges();
				List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

				foreach (var cart in cartItems) { 
					var orderdetails = new OrderDetails();
					orderdetails.UserName = userEmail;
					orderdetails.OrderCode = ordercode;
					orderdetails.ProductId = cart.ProductId;
					orderdetails.Price = cart.Price;
					orderdetails.Quantity = cart.Quantity;
					//cập nhật số lựng
					var product  = await _dataContext.Products.Where(p =>p.Id==cart.ProductId).FirstAsync();
					product.Quantity -= cart.Quantity;
					product.Sold += cart.Quantity;
					_dataContext.Update(product);
					_dataContext.Add(orderdetails);
					_dataContext.SaveChanges();
				}
				HttpContext.Session.Remove("Cart");
				//Send Email
				
                
                // Tạo nội dung email
                var sb = new StringBuilder();
                sb.Append("<p>Đặt hàng thành công, trải nghiệm dịch vụ nhé.</p>");
                sb.Append("<p>Cảm ơn bạn đã đặt hàng! Dưới đây là thông tin đơn hàng của bạn:</p>");
                sb.Append("<table border='1' cellpadding='5' cellspacing='0' style='width:100%; border-collapse: collapse;'>");
                sb.Append("<tr><th>Tên sản phẩm</th><th>Số lượng</th><th>Giá</th><th>Tổng</th></tr>");

                decimal totalAmount = 0;
                foreach (var cart in cartItems)
                {
                    var product = await _dataContext.Products.FindAsync(cart.ProductId);
                    if (product == null)
                    {
                        continue;
                    }

                    decimal totalPrice = cart.Quantity * cart.Price;
                    totalAmount += totalPrice;

                    sb.Append($"<tr><td>{product.Name}</td><td>{cart.Quantity}</td><td>{cart.Price:C}</td><td>{totalPrice:C}</td></tr>");
                }

                sb.Append($"<tr><td colspan='3' style='text-align:right;'><strong>Tổng cộng:</strong></td><td><strong>{totalAmount:C}</strong></td></tr>");
                sb.Append("</table>");
                sb.Append("<p>Chúng tôi sẽ liên hệ với bạn trong thời gian sớm nhất.</p>");

                var message = sb.ToString();

                // Gửi email
                var receiver = userEmail;
                var subject = "Đặt hàng thành công.";
                await _emailSender.SendEmailAsync(receiver, subject, message);


                await _emailSender.SendEmailAsync(receiver, subject, message);
                TempData["success"] = "Checkout thành công, vui lòng chờ đơn hàng!!!";
				return RedirectToAction("History", "Account");
			}
			return View();
		}

        [HttpGet]
        public async Task<IActionResult> PaymentCallBack(MomoInfoModel model)
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            var requestQuery = HttpContext.Request.Query;
            if (requestQuery["resultCode"] != 0) // giao dịch ko thành công
            {
                var newMomoInsert = new MomoInfoModel
                {
                    OrderId = requestQuery["orderId"],
                    FullName = User.FindFirstValue(ClaimTypes.Email),
                    Amount = decimal.Parse(requestQuery["Amount"]),
                    OrderInfo = requestQuery["orderInfo"],
                    DatePaid = DateTime.Now
                };

                _dataContext.Add(newMomoInsert);
                await _dataContext.SaveChangesAsync();
				//tiến hành đặt đơn hàng khi thanh toán momo thành công
				await Checkout(requestQuery["orderId"]);
            }
            else
            {
                TempData["success"] = "Đã hủy giao dịch Momo.";
                return RedirectToAction("Index", "Cart");
            }

            // var checkoutResult = await Checkout(requestQuery["orderId"]);
            return View(response);
        }
		[HttpGet]
		public IActionResult PaymentCallbackVnpay()
		{
			var response = _vnPayService.PaymentExecute(Request.Query);

			return Json(response);
		}
	}
}
