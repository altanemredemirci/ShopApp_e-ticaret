using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using ShopApp.BLL.Abstract;
using ShopApp.WebUI.Extensions;
using ShopApp.WebUI.Identity;
using ShopApp.WebUI.Models;

namespace ShopApp.WebUI.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class AccountController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private IEmailSender _emailSender;
        private ICartService _cartService;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, ICartService cartService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _cartService = cartService;
        }

        public IActionResult Register()
        {
            return View(new RegisterModel());
        }

        [HttpPost]        
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                //generate token
               
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Action("ConfirmEmail", "account", new
                {
                    userId =user.Id,
                    token = code
                });

                //send mail
                await _emailSender.SendEmailAsync(model.Email, "Hesabınızı Onaylayınız",$"Lütfen Email hesabınızı onaylamak için linke <a href='https://localhost:44305{callbackUrl}'> tıklayınız. </a>");

                TempData.Put("message", new ResultMessage()
                {
                    Title = "Hesap Onayı",
                    Message = "Eposta adresinize gelen link ile hesabınızı onaylayınız.",
                    Css = "warning"
                });

                return RedirectToAction("login", "account");
            }

            ModelState.AddModelError("", "Bilinmeyen hata oluştu lütfen tekrar deneyiniz.");
            return View(model);
        }

        public IActionResult Login(string ReturnUrl = null)
        {
            return View(new LoginModel()
            {
                ReturnUrl = ReturnUrl
            });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                var username = await _userManager.FindByNameAsync(model.Email);
            }
            
            //if (username == null)
            //{
            //    ModelState.AddModelError("", "Bu email ile daha önce hesap oluşturulmamış.");
            //    return View(model);
            //}

            if(!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError("", "Lütfen hesabınızı email ile onaylayınız");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, true, false);

            if (result.Succeeded)
            {
                return Redirect(model.ReturnUrl ?? "~/");
            }

            ModelState.AddModelError("", "Email adresi veya Şifre Yanlış");

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            TempData.Put("message", new ResultMessage()
            {
                Title = "Oturum Kapatıldı.",
                Message = "Hesabınız güvenli bir şekilde sonlandırıldı",
                Css = "warning"
            });

            return Redirect("~/");
        }
        
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if(userId==null || token == null)
            {
                TempData.Put("message", new ResultMessage()
                {
                    Title = "Hesap Onayı",
                    Message = "Hesap onayı için bilgileriniz yanlış.",
                    Css = "danger"
                });

                return Redirect("~/");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if(user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    //create cart object
                    _cartService.InitializeCart(user.Id);

                    TempData.Put("message", new ResultMessage()
                    {
                        Title = "Hesap Onayı",
                        Message = "Hesabınız başarıyla onaylanmıştır.",
                        Css = "success"
                    });

                    return RedirectToAction("Login");
                }
            }

            TempData.Put("message", new ResultMessage()
            {
                Title = "Hesap Onayı",
                Message = "Hesabınız onaylanamadı.",
                Css = "danger"
            });

            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                TempData.Put("message", new ResultMessage()
                {
                    Title = "Forgot Password",
                    Message = "Bilgileriniz Hatalı",
                    Css = "danger"
                });

                return View();
            }

            var user = await _userManager.FindByEmailAsync(Email);

            if(user == null)
            {
                TempData.Put("message", new ResultMessage()
                {
                    Title = "Forgot Password",
                    Message = "Eposta adresi ile bir kullanıcı bulunmadı",
                    Css = "danger"
                });

                return View();
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

            var callbackUrl = Url.Action("ResetPassword", "Account", new
            {
                token = code
            });

            //send email

            await _emailSender.SendEmailAsync(Email, "Reset Password", $"Parolanızı yenilemek için linke <a href='https://localhost:44305{callbackUrl}'> tıklayınız. </a>");

            TempData.Put("message", new ResultMessage()
            {
                Title = "Forgot Password",
                Message = "Parola yenilemek için hesabınıza mail gönderildi.",
                Css = "warning"
            });

            return RedirectToAction("Login", "Account");
        }

        public IActionResult ResetPassword(string token)
        {
            if(token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new ResetPasswordModel { Token = token };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if(user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }

        public IActionResult Accessdenied()
        {
            return View();
        }
    }
}