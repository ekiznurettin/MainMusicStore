using MainMusicStore.DataAccess.IMainRepository;
using MainMusicStore.Models.DbModels;
using MainMusicStore.Models.ViewModels;
using MainMusicStore.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace MainMusicStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _userManager = userManager;
        }
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVM = new ShoppingCartVM
            {
                OrderHeader = new OrderHeader(),
                ListCart = _unitOfWork.shoppingCart.GetAll(u => u.ApplicationUserId == claims.Value, includeProperties: "Product")
            };
            ShoppingCartVM.OrderHeader.OrderTotal = 0;
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.applicationUser.GetfirstOrDefault(u => u.Id == claims.Value, includeProperties: "Company");
            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = ProjectConstant.GetPriceBaseOnQuentity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                cart.Product.Description = ProjectConstant.ConvertToRawHtml(cart.Product.Description);
                if (cart.Product.Description.Length > 50)
                {
                    cart.Product.Description = cart.Product.Description.Substring(0, 49) + "...";
                }
            }
            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.applicationUser.GetfirstOrDefault(u => u.Id == claims.Value);
            if (user == null)
                ModelState.AddModelError(string.Empty, "Verification email is emty");


            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = code },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            ModelState.AddModelError(string.Empty, "Verification email Send, Pleace check your email");
            return RedirectToAction("Index");
        }
        public IActionResult Plus(int id)
        {
            var cart = _unitOfWork.shoppingCart.GetfirstOrDefault(x => x.Id == id, "Product");
            if (cart == null)
                return Json(false);
            //return RedirectToAction("Index");
            cart.Count += 1;
            cart.Price = ProjectConstant.GetPriceBaseOnQuentity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
            _unitOfWork.Save();
            //var allShoppingCart = _unitOfWork.shoppingCart.GetAll();
            return Json(true);
            //return RedirectToAction("Index"); 
        }
        public IActionResult Minus(int cartId)
        {
            var cart = _unitOfWork.shoppingCart.GetfirstOrDefault(x => x.Id == cartId, "Product");
            if (cart.Count == 1)
            {
                var count = _unitOfWork.shoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
                _unitOfWork.shoppingCart.Remove(cart);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(ProjectConstant.shoppingCart, count - 1);
            }
            else
            {
                cart.Count -= 1;
                cart.Price = ProjectConstant.GetPriceBaseOnQuentity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                _unitOfWork.Save();
            }

            return RedirectToAction("Index");
        }
        public IActionResult Remove(int cartId)
        {
            var cart = _unitOfWork.shoppingCart.GetfirstOrDefault(x => x.Id == cartId, "Product");
            var count = _unitOfWork.shoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
            _unitOfWork.shoppingCart.Remove(cart);
            _unitOfWork.Save();
            HttpContext.Session.SetInt32(ProjectConstant.shoppingCart, count - 1);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVM = new ShoppingCartVM
            {
                OrderHeader = new OrderHeader(),
                ListCart = _unitOfWork.shoppingCart.GetAll(u => u.ApplicationUserId == claims.Value, includeProperties: "Product")
            };
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.applicationUser.GetfirstOrDefault(u => u.Id == claims.Value, includeProperties: "Company");
            foreach (var item in ShoppingCartVM.ListCart)
            {
                item.Price = ProjectConstant.GetPriceBaseOnQuentity(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);
            }
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostaCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostaCode;
            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public IActionResult SummaryPost(string stripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.applicationUser.GetfirstOrDefault(a => a.Id == claims.Value, includeProperties: "Company");
            ShoppingCartVM.ListCart = _unitOfWork.shoppingCart.GetAll(s => s.ApplicationUserId == claims.Value, includeProperties: "Product");
            ShoppingCartVM.OrderHeader.PaymentStatus = ProjectConstant.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderStatus = ProjectConstant.OrderStatusPending;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claims.Value;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            _unitOfWork.orderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            List<OrderDetails> orderDetailsList = new List<OrderDetails>();
            foreach (var orderDetail in ShoppingCartVM.ListCart)
            {
                orderDetail.Price = ProjectConstant.GetPriceBaseOnQuentity(orderDetail.Count, orderDetail.Product.Price, orderDetail.Product.Price50, orderDetail.Product.Price100);
                OrderDetails orderDetails = new OrderDetails()
                {
                    ProductId = orderDetail.ProductId,
                    OrderId = ShoppingCartVM.OrderHeader.Id,
                    Price = orderDetail.Price,
                    Count = orderDetail.Count
                };
                ShoppingCartVM.OrderHeader.OrderTotal += orderDetails.Count * orderDetails.Price;
                _unitOfWork.orderDetails.Add(orderDetails);
            }
            _unitOfWork.shoppingCart.RemoveRange(ShoppingCartVM.ListCart);

            HttpContext.Session.SetInt32(ProjectConstant.shoppingCart, 0);

            if (stripeToken == null)
            {
                ShoppingCartVM.OrderHeader.PaymentDate = DateTime.Now.AddDays(30);
                ShoppingCartVM.OrderHeader.PaymentStatus = ProjectConstant.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = ProjectConstant.OrderStatusApproved;
            }
            else
            {
                var options = new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(ShoppingCartVM.OrderHeader.OrderTotal * 100),
                    Currency = "usd",
                    Description = "Order Id:" + ShoppingCartVM.OrderHeader.Id,
                    Source = stripeToken
                };
                var service = new ChargeService();
                Charge charge = service.Create(options);
                if (charge.BalanceTransactionId == null)
                    ShoppingCartVM.OrderHeader.PaymentStatus = ProjectConstant.PaymentStatusRejected;
                else
                    ShoppingCartVM.OrderHeader.TransactionId = charge.BalanceTransactionId;
                if (charge.Status.ToLower() == "succeeded")
                {
                    ShoppingCartVM.OrderHeader.PaymentStatus = ProjectConstant.PaymentStatusApproved;
                    ShoppingCartVM.OrderHeader.OrderStatus = ProjectConstant.OrderStatusApproved;
                    ShoppingCartVM.OrderHeader.PaymentDate = DateTime.Now;
                }
            }
            _unitOfWork.Save();
            return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });

        }

        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }
    }
}
