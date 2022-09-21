using MainMusicStore.DataAccess.IMainRepository;
using MainMusicStore.Models;
using MainMusicStore.Models.DbModels;
using MainMusicStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;

namespace MainMusicStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.product.GetAll(includeProperties: "Category,CoverType");

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                var shoppingCount = _unitOfWork.shoppingCart.GetAll(a => a.ApplicationUserId == claim.Value).ToList().Count();
                HttpContext.Session.SetInt32(ProjectConstant.shoppingCart, shoppingCount);
            }
            return View(productList);
        }
        [HttpGet]
        public IActionResult Details(int id)
        {
            var product = _unitOfWork.product.GetfirstOrDefault(p => p.Id == id, "Category,CoverType");
            ShoppingCart cart = new ShoppingCart
            {
                Product = product,
                ProductId = product.Id
            };
            return View(cart);
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart cartObj)
        {
            cartObj.Id = 0;
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                cartObj.ApplicationUserId = claim.Value;
                ShoppingCart fromDb = _unitOfWork.shoppingCart.GetfirstOrDefault(
                    s=>s.ApplicationUserId == cartObj.ApplicationUserId && s.ProductId == cartObj.ProductId,
                    "Product");
                if(fromDb== null)
                {
                    //add
                    _unitOfWork.shoppingCart.Add(cartObj);
                }
                else
                {
                    //update
                    fromDb.Count += cartObj.Count;
                }
                _unitOfWork.Save();
                var shoppingCount = _unitOfWork.shoppingCart.GetAll(a => a.ApplicationUserId == cartObj.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32(ProjectConstant.shoppingCart,shoppingCount);

                return RedirectToAction(nameof(Index));
            }
            else
            {
                var product = _unitOfWork.product.GetfirstOrDefault(p => p.Id == cartObj.ProductId, "Category,CoverType");
                ShoppingCart cart = new ShoppingCart
                {
                    Product = product,
                    ProductId = product.Id
                };
                return View(cart);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
