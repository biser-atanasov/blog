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
    [Authorize(Roles = "Admin")]
    public class CatAndTagController : Controller
    {
        private BlogDbContext db = new BlogDbContext();

        //GET: CatAndTag
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        // GET: CatAndTag/List
        [HttpGet]
        public ActionResult List()
        {
            var tags = db.CatAndTags.ToList();

            return View(tags);
        }

        // POST: CatAndTag/List
        [HttpPost]
        [ActionName("List")]
        [ValidateAntiForgeryToken]
        public ActionResult ListSaved([Bind(Include = "Id,Name")] List<CatAndTag> tags)
        {
            var dbTags = db.CatAndTags.ToList();
            for (int i = 0; i < tags.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(tags[i].Name)) db.Entry(dbTags[i]).State = EntityState.Deleted;
                else dbTags[i].Name = tags[i].Name;
            }

            db.SaveChanges();

            return RedirectToAction("List");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}