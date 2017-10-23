using System;
using System.Collections.Generic;
using SolrNet.Attributes;

namespace Search.Models
{
    public class RawDocumentModel
    {
        [SolrField("title_t")]
        public IEnumerable<string> Title { get; set; }

        [SolrField("content_t")]
        public IEnumerable<string> ArticleContent { get; set; }

        [SolrField("summary_t")]
        public IEnumerable<string> Summary { get; set; }

        [SolrField("release_date_tdt")]
        public DateTime ReleaseDate { get; set; }

        [SolrField("newstype_t")]
        public string NewsType { get; set; }

        [SolrField("itemurl_t")]
        public IEnumerable<string> StoryUrl { get; set; }

        [SolrField("activethumbimageurl_t")]
        public IEnumerable<string> ThumbImageUrl { get; set; }

        [SolrField("featured_b")]
        public bool Featured { get; set; }

        [SolrField("authorname_t")]
        public IEnumerable<string> AuthorName { get; set; }

        [SolrField("authordisplayname_t")]
        public IEnumerable<string> AuthorDisplayName { get; set; }

        [SolrField("authorurl_t")]
        public string[] AuthorUrl { get; set; }

        [SolrField("keywordlabels_sm")]
        public string[] KeywordLabels { get; set; }

        [SolrField("keywordnames_sm")]
        public IEnumerable<string> KeywordNames { get; set; }

        [SolrField("categorylabel_t")]
        public IEnumerable<string> CategoryLabel { get; set; }

        [SolrField("categoryname_t")]
        public IEnumerable<string> CategoryName { get; set; }

        [SolrField("storytype_t")]
        public IEnumerable<string> StoryType { get; set; }

        [SolrField("video_id_t")]
        public IEnumerable<string> VideoID { get; set; }
    }
}