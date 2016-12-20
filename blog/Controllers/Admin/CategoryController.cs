using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using blog.Models;

namespace blog.Controllers
{
    public class CategoryController : Controller
    {
        //GET: Category
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        // GET: Category/List
        public ActionResult List()
        {
            using (var database = new BlogDbContext())
            {
                var categories = database.Categories.ToList();

                return View(categories);
            }
        }
    }
}