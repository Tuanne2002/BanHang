﻿using System.ComponentModel.DataAnnotations;

namespace BanHangWeb.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="Hãy nhập User")]
        public string Username { get; set; }
		[Required(ErrorMessage = "Hãy nhập Email"),EmailAddress]
        public string Email { get; set; }
        [DataType(DataType.Password),Required(ErrorMessage ="Hãy nhập Pass")]
		public string Password { get; set; }
    }
}
