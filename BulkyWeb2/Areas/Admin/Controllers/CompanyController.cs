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

    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

		}
        public IActionResult Index()
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
           
            return View(objCompanyList);

        }

        public IActionResult Upsert(int? id)
        {
		
            if(id == null | id == 0)
            {
                //cretae
				return View(new Company());

			}
            else
            {
                //update
                Company companyObj = _unitOfWork.Company.Get(u=>u.Id==id);
                return View(companyObj);

            }


		}
        [HttpPost]
        public IActionResult Upsert(Company companyObj)
        {
            if (ModelState.IsValid)
            {
    
                if (companyObj.Id == 0)

                {
                    _unitOfWork.Company.Add(companyObj);


                }

                else
                {
                    _unitOfWork.Company.Update(companyObj);
                }

                _unitOfWork.Save();
                return RedirectToAction("index");
            }

            else
            {
               
                return View(companyObj);

            };


        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll(int id)
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
            return Json(new { data = objCompanyList }); 
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var companydelete = _unitOfWork.Company.Get(u=>u.Id == id);
            if (companydelete == null)
            {
                return Json(new { success  = false , message = "Error while deleting"});
            }

   

            _unitOfWork.Company.Remove(companydelete);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });

        }


        #endregion


    }

}
