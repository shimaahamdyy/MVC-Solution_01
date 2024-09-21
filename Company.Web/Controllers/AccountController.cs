﻿using Company.Data.Models;
using Company.Service.Helper;
using Company.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Company.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        

        public AccountController(UserManager<ApplicationUser> userManager , SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        #region SignUp

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel input)
        {

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {

                    UserName = input.Email.Split("@")[0],
                    Email = input.Email,
                    FirstName = input.FirstName,
                    LastName = input.LastName,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, input.Password);

                if (result.Succeeded)
                    return RedirectToAction("SignIn");

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

            }


            return View(input);
        }

        #endregion


        #region Login

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel input)
        {
            
            if(ModelState.IsValid)
            {

                var user = await _userManager.FindByEmailAsync(input.Email);

                if(user is not null)
                {

                    if(await _userManager.CheckPasswordAsync(user, input.Password))
                    {

                        var result = await _signInManager.PasswordSignInAsync(user, input.Password, input.RememberMe, true);

                        if (result.Succeeded)
                            return RedirectToAction("Index", "Home");

                    }

                }


                ModelState.AddModelError("", "Incorrect Email Or Password");
                return View(input);

            }

            return View(input);

        }


        #endregion


        #region SignOut
        public new async Task<IActionResult> SignOut()
        {

            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");

        }
        #endregion


        #region ForgetPassword

        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel input)
        {

            if(ModelState.IsValid)
            {

                var user = await _userManager.FindByEmailAsync(input.Email);

                if(user is not null)
                {

                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                    var url = Url.Action("ResetPassword", "Account", new { Email = input.Email, Token = token }, Request.Scheme);

                    var email = new Email
                    {

                        Body = url,
                        Subject = "ResetPassword",
                        To = input.Email

                    };

                    EmailSettings.SendEmail(email);

                    return RedirectToAction("CheckYourInbox");


                }


            }

            return View(input);

        }


        public IActionResult CheckYourInbox()
        {

            return View();
        }


        #endregion


        #region ResetPassword

        public IActionResult ResetPassword(string Email , string Token)
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel input)
        {

            if(ModelState.IsValid)
            {

                var user = await _userManager.FindByEmailAsync(input.Email);

                if(user is not null)
                {

                    var result = await _userManager.ResetPasswordAsync(user, input.Token, input.Password);

                    if (result.Succeeded)
                        return RedirectToAction(nameof(Login));

                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);

                }

            }

            return View(input);


        }

        #endregion


    }
}
