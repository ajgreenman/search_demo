using SolrNet.Attributes;

namespace Search.Models
{
    public class HvtDocument
    {
        [SolrUniqueKey("id")]
        public int Id { get; set; }

        [SolrField("year_i")]
        public int Year { get; set; }

        [SolrField("make_id_i")]
        public int MakeId { get; set; }

        [SolrField("make_name")]
        public string MakeName { get; set; }

        [SolrField("model_id_i")]
        public int ModelId { get; set; }

        [SolrField("model_name")]
        public string ModelName { get; set; }

        [SolrField("vehicle_display_name")]
        public string DisplayName { get; set; }

        [SolrField("make_model")]
        public string MakeModel { get; set; }

        public string HvtLink { get; set; }
    }
}