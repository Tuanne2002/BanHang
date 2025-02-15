using BanHangWeb.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BanHangWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize]
	[Route("Admin/Dashboard")]
	public class DashboardController : Controller
	{
		private readonly DataContext _dataContext;
		private readonly IWebHostEnvironment _environment;
		public DashboardController(DataContext dataContext, IWebHostEnvironment environment)
		{
			_dataContext = dataContext;
			_environment = environment;
		}

        public IActionResult Index()
        {
            var count_product = _dataContext.Products.Count();
            var count_order = _dataContext.Orders.Count();
            var count_category = _dataContext.Categories.Count();
            var count_user = _dataContext.Users.Count();
            ViewBag.CountProduct = count_product;
            ViewBag.CountOrder = count_order;
            ViewBag.CountCategory = count_category;
            ViewBag.CountUser = count_user;
            return View();
        }
		[HttpPost]
		public async Task<IActionResult> GetChartData()
		{
			var data = _dataContext.StatisticalModels
				.Select(s => new
				{
					date = s.DateCreated.ToString("yyyy-MM-dd"),
					sold = s.Sold,
					quantity = s.Quantity,
					revenue = s.Revenue,
					profit = s.Profit
				})
				.ToList();

			return Json(data);
		}
	}
}
