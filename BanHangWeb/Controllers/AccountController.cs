using BanHangWeb.Areas.Admin.Repository;
using BanHangWeb.Models;
using BanHangWeb.Models.ViewModels;
using BanHangWeb.Repository;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BanHangWeb.Controllers
{
	public class AccountController : Controller
	{
		private UserManager<AppUserModel> _userManager;
		private SignInManager<AppUserModel> _signInManager;
		private readonly IEmailSender _emailSender;
		private readonly DataContext _dataContext;

		public IActionResult Login(string returnUrl)
		{
			return View(new LoginViewModel { ReturnUrl = returnUrl });
		}
		public async Task<IActionResult> NewPass(AppUserModel user, string token)
		{
			var checkuser = await _userManager.Users
				.Where(u => u.Email == user.Email)
				.Where(u => u.Token == user.Token).FirstOrDefaultAsync();

			if (checkuser != null)
			{
				ViewBag.Email = checkuser.Email;
				ViewBag.Token = token;
			}
			else
			{
				TempData["error"] = "Không tìm thấy email hoặc token sai !!!";
				return RedirectToAction("ForgetPass", "Account");
			}
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> UpdateNewPassword(AppUserModel user, string token)
		{
			var checkuser = await _userManager.Users
				.Where(u => u.Email == user.Email)
				.Where(u => u.Token == user.Token).FirstOrDefaultAsync();

			if (checkuser != null)
			{
				// Update user with new password and token
				string newtoken = Guid.NewGuid().ToString();

				// Hash the new password
				var passwordHasher = new PasswordHasher<AppUserModel>();
				var passwordHash = passwordHasher.HashPassword(checkuser, user.PasswordHash);

				checkuser.PasswordHash = passwordHash;
				checkuser.Token = newtoken;

				await _userManager.UpdateAsync(checkuser);
				TempData["success"] = "Mật khẩu được thay đổi thành công!!!";
				return RedirectToAction("Login", "Account");
			}
			else
			{
				TempData["error"] = "Không tìm thấy email hoặc token sai !!!";
				return RedirectToAction("ForgetPass", "Account");
			}


		}

		public async Task<IActionResult> ForgetPass(string returnUrl)
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> SendMailForgotPass(AppUserModel user)
		{
			var checkMail = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

			if (checkMail == null)
			{
				TempData["error"] = "Không tìm thấy Email !!!";
				return RedirectToAction("ForgetPass", "Account");
			}
			else
			{
				string token = Guid.NewGuid().ToString();
				//update token to user
				checkMail.Token = token;
				_dataContext.Update(checkMail);
				await _dataContext.SaveChangesAsync();
				var receiver = checkMail.Email;
				var subject = "Đổi mật khẩu cho người" + checkMail.Email;
				var message = "Nhấn vào link để đổi mật khẩu: " +
					$"<a href='{Request.Scheme}://{Request.Host}/Account/NewPass?email={checkMail.Email}&token={token}'>Nhấn vào đây</a>";

				await _emailSender.SendEmailAsync(receiver, subject, message);
			}

			TempData["success"] = "Đã gửi email tới email mà bạn đăng ký để thay đổi mật khẩu !!!";

			return RedirectToAction("ForgetPass", "Account");
		}

		[HttpPost]
		public async Task<IActionResult> Login(LoginViewModel loginVM)
		{
			if (ModelState.IsValid)
			{
				Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(loginVM.Username, loginVM.Password, false, false);
				if (result.Succeeded)
				{
					return Redirect(loginVM.ReturnUrl ?? "/");
				}
				ModelState.AddModelError("", "Sai username hoặc password");
			}
			return View(loginVM);
		}
		public IActionResult Create()
		{
			return View();
		}

		public async Task<IActionResult> History()
		{
			if ((bool)!User.Identity?.IsAuthenticated)
			{
				return RedirectToAction("Login", "Account");
			}

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var userEmail = User.FindFirstValue(ClaimTypes.Email);

			var Orders = await _dataContext.Orders
				.Where(od => od.UserName == userEmail).OrderByDescending(od => od.Id).ToListAsync();

			ViewBag.UserEmail = userEmail;

			return View(Orders);
		}

		public async Task<IActionResult> CancelOrder(string ordercode)
		{
			if ((bool)!User.Identity?.IsAuthenticated)
			{
				// User is not logged in, redirect to login
				return RedirectToAction("Login", "Account");
			}
			try
			{
				var order = await _dataContext.Orders.Where(o => o.OrderCode == ordercode).FirstAsync();
				order.Status = 3;
				_dataContext.Update(order);
				await _dataContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				return BadRequest("An error occurred while canceling the order.");
			}

			return RedirectToAction("History", "Account");
		}

		public AccountController(IEmailSender emailSender, SignInManager<AppUserModel> signInManager,
			UserManager<AppUserModel> userManagerm, DataContext context)
		{
			_signInManager = signInManager;
			_userManager = userManagerm;
			_emailSender = emailSender;
			_dataContext = context;
		}

		[HttpPost]
		public async Task<IActionResult> Create(UserModel user)
		{
			if (ModelState.IsValid)
			{
				AppUserModel newUser = new AppUserModel { UserName = user.Username, Email = user.Email };
				IdentityResult result = await _userManager.CreateAsync(newUser, user.Password);
				if (result.Succeeded)
				{
					TempData["success"] = "Tạo tài khoản thành công!!!";
					return Redirect("/account/login");
				}
				foreach (IdentityError error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
			}
			return View();
		}

		public async Task<IActionResult> UpdateAccount()
		{
			if ((bool)!User.Identity?.IsAuthenticated)
			{
				return RedirectToAction("Login", "Account");
			}
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var userEmail = User.FindFirstValue(ClaimTypes.Email);

			//lấy người dùng qua email
			var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
			if (user == null)
			{
				return NotFound();
			}
			return View(user);
		}
		[HttpPost]
		public async Task<IActionResult> UpdateInfoAccount(AppUserModel user)
		{

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


			//lấy người dùng qua email
			var userById = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
			if (userById == null)
			{
				return NotFound();
			}
			else
			{
				var passwordHasher = new PasswordHasher<AppUserModel>();
				var passwordHash = passwordHasher.HashPassword(userById, user.PasswordHash);

				userById.PasswordHash = passwordHash;
				//hash the new pass
				_dataContext.Update(userById);
				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Cập nhật thông tin mới thành công!!!";
			}
			return RedirectToAction("UpdateAccount", "Account");
		}
		public async Task<IActionResult> Logout(string returnUrl = "/")
		{
			await _signInManager.SignOutAsync();
			return Redirect(returnUrl);
		}
		public async Task LoginByGoogle()
		{
			await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
				new AuthenticationProperties
				{
					RedirectUri = Url.Action("GoogleResponse")
				});
		}
		public async Task<IActionResult> GoogleResponse()
		{
			var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
			if (!result.Succeeded)
			{
				return RedirectToAction("Login");
			}

			var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claim => new
			{
				claim.Issuer,
				claim.OriginalIssuer,
				claim.Type,
				claim.Value
			});
			//return Json(claims);
			var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
			string UserName = email.Split('@')[0];
			//kiểm tra user có tồn tại không 
			var existingUser = await _userManager.FindByEmailAsync(email);
			if (existingUser == null)
			{
				//nếu user không tồn tại trong db thì tạo user mới với pw mặc định là 1-9
				var passwordHasher = new PasswordHasher<AppUserModel>();
				var hashedPassword = passwordHasher.HashPassword(null, "123456789");
				//khai báo tạo user mới
				var newUser = new AppUserModel { UserName = UserName, Email = email };
				newUser.PasswordHash = hashedPassword;
				var createUserResult = await _userManager.CreateAsync(newUser);
				if (!createUserResult.Succeeded)
				{
					TempData["error"] = "Đăng ký tài khoản thất bại. Vui lòng đăng ký lại!!!";
					return RedirectToAction("Login", "Account");
				}
				else
				{
					//tạo thành công thì đăng nhập ngay
					await _signInManager.SignInAsync(newUser, isPersistent: false);
					TempData["error"] = "Đăng ký tài khoản thành công!!!";
					return RedirectToAction("Index", "Home");
				}
			}
			else
			{
				//còn user đã tồn tawait _signInManager.SignInAsync(newUser, isPersistent: false);");
				await _signInManager.SignInAsync(existingUser, isPersistent: false);
			}
			return RedirectToAction("Login", "Account");
		}
	}
}
