using System.ComponentModel.DataAnnotations;

namespace BanHangWeb.Models.ViewModels
{
	public class LoginViewModel
	{
		public int Id { get; set; }
		[Required(ErrorMessage = "Hãy nhập User")]
		public string Username { get; set; }
		
		[DataType(DataType.Password), Required(ErrorMessage = "Hãy nhập Pass")]
		public string Password { get; set; }
		public string ReturnUrl { get; set; }
	}
}
