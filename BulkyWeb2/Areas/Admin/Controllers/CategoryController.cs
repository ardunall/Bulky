using BulkyWeb2.Data;
using BulkyWeb2.Models;
using BulkyWeb2.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BulkyWeb2.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Category> objCategoryList = _unitOfWork.Category.GetAll().ToList();
            return View(objCategoryList);
        }

        public IActionResult Create()
        {
            return View();

        }
        [HttpPost]
        public IActionResult Create(Category category)
        {

            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(category);
                _unitOfWork.Save();
                TempData["success"] = "basarlı sekilde olusuuruldu";
                return RedirectToAction("index");
            }
            return View();
        }


        public IActionResult Edit(int? id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            Category? categoryFromdb = _unitOfWork.Category.Get(u => u.Id == id);
            if (categoryFromdb == null)
            {
                return NotFound();
            }
            return View(categoryFromdb);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(category);
                _unitOfWork.Save();
                TempData["success"] = "basarlı sekilde guncellendi";

                return RedirectToAction("index");
            }
            return View();
        }



        public IActionResult Delete(int? id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            Category? categoryFromdb = _unitOfWork.Category.Get(u => u.Id == id);
            if (categoryFromdb == null)
            {
                return NotFound();
            }
            return View(categoryFromdb);

        }


        [HttpPost]
        public IActionResult Delete(Category category)
        {

            _unitOfWork.Category.Remove(category);
            _unitOfWork.Save();
            return RedirectToAction("index");

        }


        public IActionResult Yemek()
        { 
        
                return View();
        }


    }
}
