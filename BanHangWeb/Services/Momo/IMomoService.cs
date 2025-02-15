using BanHangWeb.Models;
using BanHangWeb.Models.Momo;

namespace BanHangWeb.Services.Momo
{
	public interface IMomoService
	{
		Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfoModel model);

		MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);

	}
}
