using MainMusicStore.Data;
using MainMusicStore.DataAccess.IMainRepository;
using MainMusicStore.Models.DbModels;
using MainMusicStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace MainMusicStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = ProjectConstant.Role_Admin)]
    public class UserController : Controller
    {
        //private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;
        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }

        #region ACTION
        public IActionResult Index()
        {
            return View();
        }
        #endregion

        #region API CALIS
        public IActionResult GetAll()
        {
            var userList = _db.ApplicationUsers.Include(c => c.Company).ToList();
            var userRole = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();
            foreach (var user in userList)
            {
                var roleId = userRole.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;

                if (user.Company == null)
                {
                    user.Company = new Company()
                    {
                        Name = string.Empty
                    };
                }
            }
            return Json(new { data = userList });
        }
        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var data = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if(data== null)
            {
                return Json(new { success = false, message = "Error while locking or unlocking" });
            }
            if(data.LockoutEnd!= null && data.LockoutEnd>DateTime.Now)
            {
                data.LockoutEnd = DateTime.Now;
            }
            else
            {
                data.LockoutEnd = DateTime.Now.AddYears(10);
            }
            _db.SaveChanges();
            return Json(new { success = true, message = "Operation Successfully" });
        }
        #endregion
    }
}
