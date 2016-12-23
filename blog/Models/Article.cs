using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using blog.Models.IdentityModels;

namespace blog.Models
{
    public class Article
    {
        private ICollection<CatAndTag> catAndTags;

        public Article()
        {
            this.catAndTags = new HashSet<CatAndTag>();
        }

        public Article(ArticleViewModel model)
        {
            this.AuthorId = model.AuthorId;
            this.Date = DateTime.Now;
            this.Title = model.Title;
            this.Content = model.Content;
            this.catAndTags = new HashSet<CatAndTag>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime Date { get; set; }

        [ForeignKey("Author")]
        public string AuthorId { get; set; }

        public virtual ApplicationUser Author { get; set; }

        public virtual ICollection<CatAndTag> CatAndTags
        {
            get { return this.catAndTags; }
            set { this.catAndTags = value; }
        }

        public bool IsAuthor(string name)
        {
            return this.Author.UserName.Equals(name);
        }
    }
}