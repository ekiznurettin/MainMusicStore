using Dapper;
using MainMusicStore.DataAccess.IMainRepository;
using MainMusicStore.Models.DbModels;
using MainMusicStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainMusicStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ProjectConstant.Role_Admin)]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CoverTypeController(IUnitOfWork unitOfWork)
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
            CoverType coverType = new CoverType();
            if(id== null)
            {
                //Create İşlemi
                return View(coverType);
            }
            else
            {
                var parameter = new DynamicParameters();
                parameter.Add("@Id",id);
                coverType = _unitOfWork.sp_call.OneRecord<CoverType>(ProjectConstant.Proc_CoverType_Get, parameter);
                //coverType = _unitOfWork.coverType.Get((int)id);
                if (coverType != null)
                {
                    return View(coverType);
                }
                else
                {
                    return NotFound();
                }
            }
        }
        [HttpPost]
        public IActionResult Upsert(CoverType coverType)
        {
            if (ModelState.IsValid)
            {
                var parameter = new DynamicParameters();
                parameter.Add("@Name", coverType.Name);
                if (coverType.Id == 0)
                {
                    //Create Operation
                    _unitOfWork.coverType.Add(coverType);
                    //_unitOfWork.sp_call.Execute(ProjectConstant.Proc_CoverType_Create, parameter);
                }
                else
                {
                    //Update Operation
                   // parameter.Add("@Id", coverType.Id);
                    _unitOfWork.coverType.Update(coverType);
                    //_unitOfWork.sp_call.Execute(ProjectConstant.Proc_CoverType_Update, parameter);
                }
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(coverType);
        }

        #endregion

        #region API CALIS
        public IActionResult GetAll()
        {
            // var allObj = _unitOfWork.category.GetAll();
            var allCoverTypes = _unitOfWork.sp_call.List<CoverType>(ProjectConstant.Proc_CoverType_GetAll, null);
            return Json(new { data = allCoverTypes });
        } 
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            //var deleteData = _unitOfWork.category.Get(id);
            //if(deleteData== null)
            //{
            //    return Json(new { success = false, message = "Data Not Found" });
            //}
            //_unitOfWork.category.Remove(deleteData);
            //_unitOfWork.Save();
            //return Json(new { success = true, message = "Delete Opreration Successfully" });
            var parameter = new DynamicParameters();
            parameter.Add("@Id", id);
            var deleteData = _unitOfWork.sp_call.OneRecord<CoverType>(ProjectConstant.Proc_CoverType_Get, parameter);
            if (deleteData == null)
            {
                return Json(new { success = false, message = "Data Not Found" });
            }
            _unitOfWork.sp_call.Execute(ProjectConstant.Proc_CoverType_Delete,parameter);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Opreration Successfully" });
        }
        #endregion
    }
}
