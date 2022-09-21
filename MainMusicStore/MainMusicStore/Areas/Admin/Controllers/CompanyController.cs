using MainMusicStore.DataAccess.IMainRepository;
using MainMusicStore.Models.DbModels;
using MainMusicStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainMusicStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ProjectConstant.Role_Admin + "," + ProjectConstant.Role_Employee)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region ACTION
        public IActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// Create and update operation HttpGet
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            Company company = new Company();
            if (id == null)
            {
                //Create İşlemi
                return View(company);
            }
            else
            {
                company = _unitOfWork.company.Get((int)id);
                if (company != null)
                {
                    return View(company);
                }
                else
                {
                    return NotFound();
                }
            }
        }
        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    //Create Operation
                    _unitOfWork.company.Add(company);
                }
                else
                {
                    //Update Operation
                    _unitOfWork.company.Update(company);
                }
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(company);
        }

        #endregion

        #region API CALIS
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.company.GetAll();
            return Json(new { data = allObj });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var deleteData = _unitOfWork.company.Get(id);
            if (deleteData == null)
            {
                return Json(new { success = false, message = "Data Not Found" });
            }
            _unitOfWork.company.Remove(deleteData);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Opreration Successfully" });
        }
        #endregion
    }
}
