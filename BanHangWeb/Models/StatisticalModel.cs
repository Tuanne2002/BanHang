﻿namespace BanHangWeb.Models
{
	public class StatisticalModel
	{
		public int Id { get; set; }
		public int Quantity { get; set; } // so luong ban
		public int Sold { get; set; } // so luong don hang
		public int Revenue { get; set; } // doanh thu
		public int Profit { get; set; } // loi nhuan
		public DateTime DateCreated { get; set; } // ngay dat hang

	}
}
