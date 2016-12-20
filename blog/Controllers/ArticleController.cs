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
    [ValidateInput(false)]
    public class ArticleController : Controller
    {
        // GET: Article
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        // GET: Article/List
        public ActionResult List()
        {
            using (var database = new BlogDbContext())
            {
                var articles = database.Articles
                    .Include(a => a.Author)
                    .ToList();

                foreach (var article in articles)
                {
                    article.Content = Left(article.Content, 500);
                }

                return View(articles);
            }
        }

        // GET: Article/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                var аrticle = database.Articles
                    .Where(a => a.Id == id)
                    .Include(a => a.Author)
                    .First();

                if (аrticle == null)
                {
                    return HttpNotFound();
                }

                return View(аrticle);
            }
        }

        // GET: POST: Article/Create
        [Authorize]
        public ActionResult Create(Article article)
        {
            if (ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {
                    //get author id
                    var authorId = database.Users
                        .Where(u => u.UserName == this.User.Identity.Name)
                        .First()
                        .Id;

                    //set articles author
                    article.AuthorId = authorId;

                    //save articles in Db
                    article.Content = article.Content.Substring(0, article.Content.Length-4);
                    database.Articles.Add(article);
                    database.SaveChanges();

                    return RedirectToAction("Index");
                }
            }

            return View(article);
        }

        // GET: Article/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                var аrticle = database.Articles
                    .Where(a => a.Id == id)
                    .Include(a => a.Author)
                    .First();

                if (!IsUserAuthorizedToEdit(аrticle))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                if (аrticle == null)
                {
                    return HttpNotFound();
                }

                return View(аrticle);
            }
        }

        // POST: Article/Delete
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                var аrticle = database.Articles
                    .Where(a => a.Id == id)
                    .Include(a => a.Author)
                    .First();

                if (аrticle == null)
                {
                    return HttpNotFound();
                }

                database.Articles.Remove(аrticle);
                database.SaveChanges();


                return RedirectToAction("Index");
            }
        }

        // GET: Article/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                var аrticle = database.Articles
                    .Where(a => a.Id == id)
                    .First();

                if (!IsUserAuthorizedToEdit(аrticle))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                if (аrticle == null)
                {
                    return HttpNotFound();
                }

                var model = new ArticleViewModel();
                model.Id = аrticle.Id;
                model.Title = аrticle.Title;
                model.Content = аrticle.Content;

                return View(model);
            }
        }

        // POST: Article/Edit
        [HttpPost]
        public ActionResult Edit(ArticleViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {
                    var article = database.Articles
                        .FirstOrDefault(a => a.Id == model.Id);

                    article.Title = model.Title;
                    article.Content = model.Content;

                    if (!IsUserAuthorizedToEdit(article))
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                    }

                    database.Entry(article).State = EntityState.Modified;
                    database.SaveChanges();

                    return RedirectToAction("Index");
                }
            }

            return View(model);
        }

        private bool IsUserAuthorizedToEdit(Article article)
        {
            bool isAdmin = this.User.IsInRole("Admin");
            var name = this.User.Identity.Name;
            bool isAuthor = article.IsAuthor(name);

            return isAdmin || isAuthor;
        }

        private string Left(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength) return value;

            value = value.Substring(0, maxLength);
            value = value.Substring(0, value.LastIndexOf(" "));

            return string.Concat("  ", value, " ...");
        }

    }
}