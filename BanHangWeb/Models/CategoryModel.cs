using System.ComponentModel.DataAnnotations;

namespace BanHangWeb.Models
{
    public class CategoryModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage ="Yêu cầu nhập Tên Danh mục")]
        public string Name { get; set; }
        [Required(ErrorMessage ="Yêu cầu nhập mô tả Danh mục")]
        public string Description { get; set; }
        
        public string Slug { get; set; }//lấy name tạo code
        public int Status { get; set; }

    }
}
