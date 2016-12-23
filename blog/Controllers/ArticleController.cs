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
        public ActionResult List(string id)
        {
            using (var database = new BlogDbContext())
            {
                var articles = new List<blog.Models.Article>();

                //показване на всички статии
                if (id == null)
                {
                    articles = database.Articles
                       .Include(a => a.Author)
                       .Include(a => a.CatAndTags)
                       .ToList();
                }
                else
                //търсене на статии по зададен критерий
                {
                    int Id = 0;
                    var isNumber = int.TryParse(id, out Id);

                    if (isNumber)
                    {
                        // tag or theme (number Id)
                        articles = database.Articles
                        .Include(a => a.CatAndTags)
                        .Where(a=> a.CatAndTags.Any(c => c.Id==Id))
                        .Include(a => a.Author)
                        .ToList();
                    }
                    else
                    {
                        //author
                        articles = database.Articles
                       .Include(a => a.Author)
                       .Where(a => a.Author.Id == id)
                       .Include(a => a.CatAndTags)
                       .ToList();
                    }
                };

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
                    .Include(a => a.CatAndTags)
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
        public ActionResult Create(ArticleViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {
                    //get author id
                    model.AuthorId = database.Users
                        .Where(u => u.UserName == this.User.Identity.Name)
                        .First()
                        .Id;

                    var article = new Article(model);

                    //set cats and tags
                    SetAtricleCatAndTags(model, database, article);

                    //save articles in Db
                    database.Articles.Add(article);
                    database.SaveChanges();

                    return RedirectToAction("Index");
                }
            }

            return View(model);
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
                    .Include(a => a.CatAndTags)
                    .First();

                if (!IsUserAuthorizedToEdit(аrticle))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                ViewBag.CatAndTagsString = string.Join(", ", аrticle.CatAndTags.Select(t => t.Name));

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
                    .Include(a => a.CatAndTags)
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
                model.CatAndTags = string.Join(", ", аrticle.CatAndTags.Select(a => a.Name));

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

                    if (!IsUserAuthorizedToEdit(article))
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                    }

                    article.Title = model.Title;
                    article.Content = model.Content;
                    article.Date = DateTime.Now;

                    SetAtricleCatAndTags(model, database, article);

                    database.Entry(article).State = EntityState.Modified;
                    database.SaveChanges();

                    return RedirectToAction("Index");
                }
            }

            return View(model);
        }

        private static void SetAtricleCatAndTags(ArticleViewModel model, BlogDbContext database, Article article)
        {
            string[] delimiter = new string[] { ",", " " };
            var cattags = model.CatAndTags
                .Split(delimiter, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.ToLower())
                .Distinct();

            article.CatAndTags.Clear();

            foreach (var cattag in cattags)
            {
                CatAndTag catAndTag = database.CatAndTags.FirstOrDefault(t => t.Name.Equals(cattag));

                if (catAndTag == null)
                {
                    catAndTag = new CatAndTag() { Name = cattag };
                }

                article.CatAndTags.Add(catAndTag);
            }
        }

        private bool IsUserAuthorizedToEdit(Article article)
        {
            bool isAdmin = this.User.IsInRole("Admin");
            var name = this.User.Identity.Name;
            bool isAuthor = article.IsAuthor(name);

            return isAdmin || isAuthor;
        }
    }
}