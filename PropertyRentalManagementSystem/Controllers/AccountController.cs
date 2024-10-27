using PropertyRentalManagementSystem.ViewModels;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PropertyRentalManagementSystem.Models;
using System.Web.Security;
using System;

namespace PropertyRentalManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private PropertyRentalManagementDBEntities db = new PropertyRentalManagementDBEntities();

        // GET: Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Authenticate the user
                var user = db.Users.FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);
                if (user != null)
                {
                    // Get role name
                    var roleName = db.Roles.FirstOrDefault(r => r.RoleId == user.RoleId)?.RoleName;

                    // Check if the role is valid and assign session values
                    if (roleName == "Property Owner" || roleName == "Administrator" || roleName == "Property Manager")
                    {
                        // Set session variables
                        Session["UserName"] = user.Email;
                        Session["UserId"] = user.UserId;
                        Session["RoleId"] = user.RoleId;
                        Session["RoleName"] = roleName;

                        // Set authentication cookie
                        FormsAuthentication.SetAuthCookie(user.Email, false);

                        // Redirect to appropriate dashboard based on role
                        if (roleName == "Property Owner" || roleName == "Administrator")
                        {
                            return RedirectToAction("Index", "OwnersAdministratorsDashboard");
                        }
                        else if (roleName == "Property Manager")
                        {
                            return RedirectToAction("Index", "PropertyManagerDashboard");
                        }
                    }
                    else
                    {
                        // Role not recognized, clear session and display error
                        Session.Clear();
                        ModelState.AddModelError("", "Access Denied. Invalid role assigned.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                }
            }

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]  // This validates the token sent by the form
        public ActionResult Logout()
        {
            // Clear session and authentication
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();

            // Redirect to home page after logout
            return RedirectToAction("Index", "Home");
        }


        // GET: Account/SignUp
        [HttpGet]
        public ActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignUp(SignUpViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (db.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email is already in use.");
                    return View(model);
                }

                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Password = model.Password,
                    Phone = model.Phone,
                    RoleId = 4  // RoleId for Tenant
                };

                db.Users.Add(user);
                db.SaveChanges();

               
                ViewBag.Message = "Sign-up successful! You can now log in.";
                return View(); 
            }

            ViewBag.Message = "Sign-up failed. Please check the details and try again.";
            return View(model);
        }

    }
}
