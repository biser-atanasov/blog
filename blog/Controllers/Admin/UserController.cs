using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using blog.Models;
using blog.Models.IdentityModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace blog.Controllers
{
    [Authorize(Roles ="Admin")]
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            return View("List");
        }

        //Get User/List
        public ActionResult List()
        {
            using (var database = new BlogDbContext())
            {
                var users = database.Users.ToList();

                var admins = GetAdminUserNames(users, database);
                ViewBag.Admins = admins;

                return View(users);
            }
        }

        //Get User/Edit
        public ActionResult Edit(string id)
        {
            //Validate Id
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                //Get user
                var user = database.Users
                    .Where(u => u.Id == id)
                    .First();

                //Check is exist
                if (user == null)
                {
                    return HttpNotFound();
                }
                //Create view model
                var viewModel = new EditUserViewModel();
                viewModel.User = user;
                viewModel.Roles = GetUserRoles(user, database);

                //Pass the model to the view
                return View(viewModel);
            }
        }

        //POST User/Edit
        [HttpPost]
        public ActionResult Edit(string id, EditUserViewModel viewModel)
        {
            //Check is model valid
            if (ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {
                    //Get user from db
                    var user = database.Users.FirstOrDefault(u => u.Id == id);

                    //Check exist
                    if (user == null)
                    {
                        return HttpNotFound();
                    }

                    //Change pswd, if not empty
                    if (!string.IsNullOrEmpty(viewModel.Password))
                    {
                        var hasher = new PasswordHasher();
                        var passwordHash = hasher.HashPassword(viewModel.Password);
                        user.PasswordHash = passwordHash;
                    }

                    //Set user props                  
                    user.Email = viewModel.User.Email;
                    user.UserName = user.Email;
                    user.FullName = viewModel.User.FullName;
                    this.SetUserRoles(viewModel, user, database);

                    //Save changes
                    database.Entry(user).State = EntityState.Modified;
                    database.SaveChanges();

                    return RedirectToAction("List");
                }
            }

            return View(viewModel);
        }

        //Get User/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                //Get user
                var user = database.Users
                    .Where(u => u.Id.Equals(id))
                    .First();

                //Check existence
                if (user == null)
                {
                    return HttpNotFound();
                }

                return View(user);
            }
        }

        //POST User/Delete
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                //get user
                var user = database.Users
                    .Where(u => u.Id == id)
                    .First();

                //get articles
                var userArticles = database.Articles
                        .Where(a => a.Author.Id == user.Id);

                //delete articles
                foreach (var article in userArticles)
                {
                    database.Articles.Remove(article);
                }

                //delete user and save changes
                database.Users.Remove(user);
                database.SaveChanges();
            }

            return RedirectToAction("List");
        }

        private void SetUserRoles(EditUserViewModel model, ApplicationUser user, BlogDbContext db)
        {
            var userManager = Request
                .GetOwinContext()
                .GetUserManager<ApplicationUserManager>();

            foreach (var role in model.Roles)
            {
                if (role.IsSelected)
                {
                    userManager.AddToRole(user.Id, role.Name);
                }
                else if (!role.IsSelected)
                {
                    userManager.RemoveFromRole(user.Id, role.Name);
                }
            }
        }

        private List<Role> GetUserRoles(ApplicationUser user, BlogDbContext db)
        {
            //Create user manager
            var userManager = Request
                .GetOwinContext()
                .GetUserManager<ApplicationUserManager>();

            //Get All application roles
            var roles = db.Roles
                .Select(r => r.Name)
                .OrderBy(r => r)
                .ToList();

            //For each apllication role, check if the user has it
            var userRoles = new List<Role>();

            foreach (var roleName in roles)
            {
                var role = new Role { Name = roleName };

                if (userManager.IsInRole(user.Id, roleName))
                {
                    role.IsSelected = true;
                }

                userRoles.Add(role);
            }

            //Return a list with all roles
            return userRoles;
        }

        private HashSet<string> GetAdminUserNames(List<ApplicationUser> users, BlogDbContext context)
        {
            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));

            var admins = new HashSet<string>();

            foreach (var user in users)
            {
                if (userManager.IsInRole(user.Id, "Admin"))
                {
                    admins.Add(user.UserName);
                }
            }
            return admins;
        }
    }
}