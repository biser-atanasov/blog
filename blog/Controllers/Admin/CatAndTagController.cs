using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using blog.Models;

namespace blog.Controllers
{
    public class CatAndTagController : Controller
    {
        //GET: CatAndTag
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        // GET: CatAndTag/List
        public ActionResult List(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                var cattags = database.CatAndTags.ToList();

                return View(cattags);
            }
        }
    }
}