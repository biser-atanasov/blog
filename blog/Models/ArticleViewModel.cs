using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace blog.Models
{
    public class ArticleViewModel
    {
        [Key]
        public int Id { get; set; }
      
        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime Date { get; set; }

        public string AuthorId { get; set; }

        [Required(ErrorMessage = "Themes or Tags are required")]
        public string CatAndTags { get; set; }
    }
}