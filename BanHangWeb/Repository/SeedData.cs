using BanHangWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BanHangWeb.Repository
{
	public class SeedData
	{
		public static void SeedingData(DataContext _context)
		{
			_context.Database.Migrate();
			if (!_context.Products.Any())
			{
				CategoryModel macbook = new CategoryModel {Name="Macbook",Slug="macbook",Description="Macbook is Large Product in th word",Status=1};
				CategoryModel pc = new CategoryModel { Name = "Pc", Slug = "pc", Description = "PC is Large Product in th word", Status = 1 };

				BrandModel apple = new BrandModel { Name = "Apple", Slug = "apple", Description = "Apple is Large Brand in th word", Status = 1 };
				BrandModel samsung = new BrandModel { Name = "Samsung", Slug = "samsung", Description = "Samsung is Large Brand in th word", Status = 1 };
				_context.Products.AddRange(
					new ProductModel { Name = "Macbook", Slug = "macbook", Description = "Macbook is best", Image = "1.jpg", Category = macbook,Brand = apple, Price = 1100 },
					new ProductModel { Name = "PC", Slug = "pc", Description = "PC is best", Image = "1.jpg", Category = pc,Brand = samsung, Price = 1100 }

				);
				_context.SaveChanges();
			}
		}
	}
}
