using System.Collections.Generic;
using System.ComponentModel;

namespace Search.Models
{
    public class SearchModel
    {
        [DisplayName("Search")]
        public string SearchText { get; set; }
    }

    public class ArticleSearchModel : SearchModel
    {
        public IEnumerable<ArticleModel> Articles { get; set; }
        public ICollection<KeyValuePair<string, int>> Categories { get; set; }
        public ICollection<KeyValuePair<string, int>> Keywords { get; set; }
    }

    public class HvtSearchModel : SearchModel
    {
        public IEnumerable<HvtDocument> Vehicles { get; set; }
    }
}