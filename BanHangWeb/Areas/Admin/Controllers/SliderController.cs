using BanHangWeb.Models;
using BanHangWeb.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BanHangWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/Slider")]
	[Authorize(Roles = "Admin")]
	public class SliderController : Controller
	{
		private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public SliderController(DataContext context, IWebHostEnvironment webHostEnvironment) {
			_dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
		{
			return View(await _dataContext.Sliders.OrderByDescending(p=>p.Id).ToListAsync());
		}
        [HttpGet("Create")]
        public IActionResult Create() {
			return View();
		}
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SliderModel slider)
        {
            

            if (ModelState.IsValid)
            {
                if (slider.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders");
                    string imageName = Guid.NewGuid().ToString() + "_" + slider.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await slider.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    slider.Image = imageName;
                }

                _dataContext.Add(slider);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm slider thành công!!!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Model đang bị lỗi!!!";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
            return View(slider);
        }
        [HttpGet("Edit/{Id}")]
        public async Task<IActionResult> Edit(int Id)
        {
            SliderModel slider = await _dataContext.Sliders.FindAsync(Id);
            
            return View(slider);
        }
        [HttpPost("Edit/{Id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, SliderModel slider)
        {
           

            var existed_slider = _dataContext.Sliders.Find(slider.Id);

            if (ModelState.IsValid)
            {


                if (slider.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders");
                    string imageName = Guid.NewGuid().ToString() + "_" + slider.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath,FileMode.Create);
                    await slider.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    existed_slider.Image = imageName;

                    //xoa anh cu
                    //string oldfilePath = Path.Combine(uploadsDir, existed_slider.Image);


                }
                existed_slider.Name = slider.Name;
                existed_slider.Description = slider.Description;
                existed_slider.Status = slider.Status;
          

                _dataContext.Update(existed_slider);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật slider thành công!!!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Model đang bị lỗi!!!";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
            return View(slider);
        }
        [HttpGet("Delete/{Id}")]
        public async Task<IActionResult> Delete(int Id)
        {
            SliderModel slider = await _dataContext.Sliders.FindAsync(Id);

            if (slider == null)
            {
                return NotFound();
            }

            string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/sliders");
            string oldfilePath = Path.Combine(uploadsDir, slider.Image);

            try
            {
                if (System.IO.File.Exists(oldfilePath))
                {
                    System.IO.File.Delete(oldfilePath);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while deleting the product image.");
            }

            _dataContext.Sliders.Remove(slider);
            await _dataContext.SaveChangesAsync();
            TempData["error"] = "Slider đã xóa!!!";
            return RedirectToAction("Index");
        }
    }
}
