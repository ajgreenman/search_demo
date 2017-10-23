using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Search.Models
{
    public class ArticleModel
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Author { get; set; }
        public DateTime Date { get; set; }
        public string Url { get; set; }
        public string Thumbnail { get; set; }
    }
}