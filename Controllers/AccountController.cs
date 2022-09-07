using MailKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Text;
using System.Reflection.Metadata.Ecma335;
using UserManagement.MailHelpers;
using UserManagement.MailService;
using UserManagement.Models;
using UserManagement.Models.ViewModels;

namespace UserManagement.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IMailSender _mailSender;
        private readonly ILogger<AccountController> _logger;
        public AccountController(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            IMailSender mailSender,
            ILogger<AccountController>logger
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mailSender = mailSender;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnurl = null)
        {
            ViewData["ReturnUrl"] = returnurl;
            
            RegisterViewModel registerViewModel = new RegisterViewModel();
            return View(registerViewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnurl = null)
        {
            ViewData["ReturnUrl"] = returnurl;
            LoginViewModel loginViewModel = new LoginViewModel();
            return View(loginViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel, string returnurl=null)
        {
            ViewData["ReturnUrl"] = returnurl;
            returnurl = returnurl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = registerViewModel.Email,
                    Email = registerViewModel.Email,
                    Name = registerViewModel.Name,
                };

                var result = await _userManager.CreateAsync(user, registerViewModel.Password);
                if (result.Succeeded)
                {
                    //Generate token for email confirmation
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    //generate confirmation lin with URL
                    var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account", new { userId = user.Id, token = token }, Request.Scheme);
                    //_logger.Log(LogLevel.Warning, confirmationLink);

                    await _mailSender.SendEmailAsync(
                       registerViewModel.Email,
                       "Confirm you Account-Identity Manager",
                       "Please activate your Account by clicking here: <a href= \"" + confirmationLink + "\">Link</a>"
                    );

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    //return LocalRedirect(nameof(Login));
                    return RedirectToAction(nameof(ConfirmView));
                }
                AddErrors(result);

            }

            return View(registerViewModel);
            
        }

        [HttpGet]
        public  async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if(userId == null || token == null)
            {
                ModelState.AddModelError("Error", "Invalid userid or token");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if(user == null)
            {
                return NotFound($"The user id={userId} is invalid");
            }
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return View();
            }
            return View("Error");
        }

        [HttpGet]
        public IActionResult ConfirmView()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult>Login(LoginViewModel loginViewModel, string returnurl=null)
        {
            ViewData["ReturnUrl"] = returnurl;
            returnurl = returnurl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(loginViewModel.Email);
                if(user != null && !user.EmailConfirmed && (await _userManager.CheckPasswordAsync(user, loginViewModel.Password)))
                {
                    ModelState.AddModelError(string.Empty, "Email not confirmed");
                    return View(loginViewModel);
                }

                //Otherwise proceed to login
                var result = await _signInManager.PasswordSignInAsync(
                    loginViewModel.Email,
                    loginViewModel.Password, loginViewModel.RememberMe,
                    lockoutOnFailure: true
                    );

                if (result.Succeeded)
                {
                    return LocalRedirect(returnurl);
                }
                if (result.IsLockedOut)
                {
                    return View("Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt");
                    return View(loginViewModel);
                }
            }

            return View(loginViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

       

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            if(token == null || email == null)
            {
                ModelState.AddModelError("", "Invalid password reset token");
            }
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel rmodel)
        {
            if (!ModelState.IsValid)
            {
                return View(rmodel);
            }
            else
            {
                var user = await _userManager.FindByEmailAsync(rmodel.Email);
                if (user == null)
                {
                    return RedirectToAction(nameof(ResetPasswordConfirmation));
                }
                var result = await _userManager.ResetPasswordAsync(user, rmodel.Token, rmodel.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(ResetPasswordConfirmation));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if(user == null)
            {
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }
            else
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                
                var callBack = Url.Action(nameof(ResetPassword), "Account", new {email = model.Email, token = token }, Request.Scheme);

                /*
                                await _mailSender.SendEmailAsync(model.Email, "Reset Password - Identity Manager",
                                     "Please reset your password by clicking here: <a href= \"" + callBack + "\">Link</a>");*/
                await _mailSender.SendEmailAsync(
                    model.Email,
                    "Password-Reset Link",
                    "Please reset your password by clicking here: <a href= \"" + callBack + "\">Link</a>"
                 );

                
                
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

        }

        private void AddErrors(IdentityResult result)
        {
            foreach(var error in result.Errors)
            {
                ModelState.AddModelError(String.Empty, error.Description);
            }
        }
    }
}
