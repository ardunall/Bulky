using BulkyWeb2.Data;
using BulkyWeb2.Models;
using BulkyWeb2.Repository.IRepository;
using BulkyWeb2.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace BulkyWeb2.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment )
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;

		}
        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(inculdeProperties:"Category").ToList();
           
            return View(objProductList);

        }

        public IActionResult Upsert(int? id)
        {
			ProductVM productVm = new()
			{
				CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
				{
					Text = u.Name,
					Value = u.Id.ToString(),


				}),
				Product = new Product()

			};


            if(id == null | id == 0)
            {
                //cretae
				return View(productVm);

			}
            else
            {
                //update
                productVm.Product = _unitOfWork.Product.Get(u=>u.Id==id);
                return View(productVm);

            }


		}
        [HttpPost]
        public IActionResult Upsert(ProductVM vm, IFormFile? file)
        {

            if (ModelState.IsValid & file != null)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if(file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, "images", "product");

                    // Ensure the directory exists
                    if (!Directory.Exists(productPath))
                    {
                        Directory.CreateDirectory(productPath);
                    }

                    if(!string.IsNullOrEmpty(vm.Product.ImageUrl))
                    {
                        var Oldimagespath = Path.Combine(wwwRootPath, vm.Product.ImageUrl.TrimStart('/'));
                    
                        if(System.IO.File.Exists(Oldimagespath))
                        {
                            System.IO.File.Delete(Oldimagespath);
                        }
                    }

                    string filePath = Path.Combine(productPath, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    vm.Product.ImageUrl = "/images/product/" + fileName;


				}

                if(vm.Product.Id==0)

                {
                    _unitOfWork.Product.Add(vm.Product);


                }

                else 
                { 
                    _unitOfWork.Product.Update(vm.Product);
                }

                _unitOfWork.Save();
                return RedirectToAction("index");
            }

            else 
            {
				vm.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),


                });
                return View(vm);

			};
			
		}

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll(int id)
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(inculdeProperties: "Category").ToList();
            return Json(new { data = objProductList }); 
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productdelete = _unitOfWork.Product.Get(u=>u.Id == id);
            if (productdelete == null)
            {
                return Json(new { success  = false , message = "Error while deleting"});
            }

            var Oldimagespath = Path.Combine(_webHostEnvironment.WebRootPath, productdelete.ImageUrl.TrimStart('/'));

            if (System.IO.File.Exists(Oldimagespath))
            {

                System.IO.File.Delete(Oldimagespath);
            }

            _unitOfWork.Product.Remove(productdelete);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });

        }


        #endregion


    }

}
