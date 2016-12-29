using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace blog.Models
{
    public class CatAndTag
    {
        private ICollection<Article> articles;

        public CatAndTag()
        {
            this.articles = new HashSet<Article>();
        }

        [Key]
        public int Id { get; set; }

        //[Required]
        //[Index(IsUnique = true)]
        [MaxLength(100)]
        public string Name { get; set; }

        public virtual ICollection<Article> Articles
        {
            get { return this.articles; }
            set { this.articles = value; }
        }
    }
}