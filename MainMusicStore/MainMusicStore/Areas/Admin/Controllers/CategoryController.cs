using MainMusicStore.DataAccess.IMainRepository;
using MainMusicStore.Models.DbModels;
using MainMusicStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainMusicStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =ProjectConstant.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
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
            Category category = new Category();
            if(id== null)
            {
                //Create İşlemi
                return View(category);
            }
            else
            {
                category = _unitOfWork.category.Get((int)id);
                if (category != null)
                {
                    return View(category);
                }
                else
                {
                    return NotFound();
                }
            }
        }
        [HttpPost]
        public IActionResult Upsert(Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.Id == 0)
                {
                    //Create Operation
                    _unitOfWork.category.Add(category);
                }
                else
                {
                    //Update Operation
                    _unitOfWork.category.Update(category);
                }
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        #endregion

        #region API CALIS
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.category.GetAll();
            return Json(new { data = allObj });
        } 
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var deleteData = _unitOfWork.category.Get(id);
            if(deleteData== null)
            {
                return Json(new { success = false, message = "Data Not Found" });
            }
            _unitOfWork.category.Remove(deleteData);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Opreration Successfully" });
        }
        #endregion
    }
}
