﻿using BanHangWeb.Models;
using BanHangWeb.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BanHangWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    [Route("Admin/[controller]")]
    public class ContactController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ContactController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpGet("Index")]
        public IActionResult Index()
        {
            var contact = _dataContext.Contact.ToList();
            return View(contact);
        }
        [HttpGet("Edit")]

        public async Task<IActionResult> Edit()
        {
            ContactModel contact = await _dataContext.Contact.FirstOrDefaultAsync();
            return View(contact);
        }

        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ContactModel contact)
        {

            var existed_contact = _dataContext.Contact.FirstOrDefault();

            if (ModelState.IsValid)
            {
                
               

                if (contact.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/logo");
                    string imageName = Guid.NewGuid().ToString() + "_" + contact.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    
                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await contact.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    existed_contact.LogoImg = imageName;

                }

                existed_contact.Name = contact.Name;
                existed_contact.Description = contact.Description;
                existed_contact.Phone =   contact.Phone;
                existed_contact.Map = contact.Map;
                existed_contact.Email = contact.Email;

                _dataContext.Update(existed_contact);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật địa chỉ thành công!!!";
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
            return View(contact);
        }
    }
}
