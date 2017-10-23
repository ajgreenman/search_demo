using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Search.Extensions;
using Search.Models;
using SolrNet;
using SolrNet.Commands.Parameters;
using SolrNet.Impl;
using SortOrder = SolrNet.SortOrder;

namespace Search.Controllers
{
    public class HomeController : Controller
    {
        private static readonly string NewsTemplateId = "{EE12347D-23AE-4EC3-A006-122366B4D809}";
        private static readonly string ArticlesStartingPoint = "{BE4748BE-B51C-42D2-B19F-DFC61B899947}";

        private static readonly string TemplatesImplementedField = "_templatesimplemented_sm";
        private static readonly string ItemPathField = "_path";

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string searchText)
        {
            return View(FindArticleResults(searchText));
        }

        [HttpGet]
        public ActionResult Vehicles()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Vehicles(string searchText)
        {
            return View(FindVehicleResults(searchText));
        }

        [HttpPost]
        public JsonResult Suggestions(string searchText)
        {
            var solr = new SolrConnection(SolrConfig.VehicleInformationIndex);
            var parameters = new Dictionary<string, string>
            {
                {"q", searchText},
                {"wt", "json"}
            };

            var response = solr.Get("/suggest", parameters);

            var suggestion = JsonConvert.DeserializeObject<SuggestionModel>(response);
            if (suggestion.Spellcheck == null)
            {
                return null;
            }

            var details = JsonConvert.DeserializeObject<SuggestionDetailModel>(suggestion.Spellcheck.Suggestions.ToList()[1].ToString());
            details.SearchText = searchText;

            return Json(details);
        }

        private HvtSearchModel FindVehicleResults(string searchText)
        {
            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<HvtDocument>>();

            var queryOptions = new QueryOptions
            {
                OrderBy = { new SortOrder("vehicle_display_name") },
                Rows = 10
            };

            var documents = solr.Query(GetVehicleQuery(searchText), queryOptions);

            foreach (var vehicle in documents)
            {
                vehicle.HvtLink = vehicle.DisplayName.Replace(' ', '-');
            }

            return new HvtSearchModel
            {
                SearchText = searchText,
                Vehicles = documents
            };
        }

        private ArticleSearchModel FindArticleResults(string searchText)
        {
            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<RawDocumentModel>>();

            var queryOptions = new QueryOptions
            {
                FilterQueries = new List<ISolrQuery>
                {
                    GetTemplateFilterQuery(),
                    GetPathFilterQuery()
                },
                Facet = new FacetParameters
                {
                    Queries = new[]
                    {
                        new SolrFacetFieldQuery("categorylabel_t"),
                        new SolrFacetFieldQuery("keywordlabels_sm")
                    }
                },
                OrderBy = {new SortOrder("score", Order.DESC)},
                Rows = 10
            };

            var documents = solr.Query(GetArticleQuery(searchText), queryOptions);
            var articles = new List<ArticleModel>();
            foreach (var document in documents)
            {
                var article = new ArticleModel
                {
                    Title = ParseSolrList(document.Title),
                    Summary = GetSummary(document.Summary, document.ArticleContent),
                    Author = ParseSolrList(document.AuthorDisplayName),
                    Date = document.ReleaseDate,
                    Url = ParseSolrList(document.StoryUrl),
                    Thumbnail = ParseSolrList(document.ThumbImageUrl)
                };

                articles.Add(article);
            }

            return new ArticleSearchModel
            {
                SearchText = searchText,
                Articles = articles,
                Categories = documents.FacetFields["categorylabel_t"],
                Keywords = documents.FacetFields["keywordlabels_sm"]
            };
        }

        private ISolrQuery GetVehicleQuery(string searchText)
        {
            var query = "(make_model:\"" + searchText + "\") ";
            return new LocalParams { { "defType", "edismax" } } + new SolrQuery(query);
        }

        private ISolrQuery GetArticleQuery(string searchText)
        {
            var titleQuery = "(title_t:\"" + searchText + "\")^1.2 ";
            var summaryQuery = "(summary_t:\"" + searchText + "\")^1.0 ";
            var contentQuery = "(content_t:\"" + searchText + "\")^0.5 ";
            var authorQuery = "(authordisplayname_t:\"" + searchText + "\")^1.0 ";
            var dateQuery = "recip(ms(NOW,release_date_tdt),3.16e-11,1,1)";
            return new LocalParams { { "boost b", dateQuery }, { "tie", "1.0" }, { "defType", "edismax" } } + new SolrQuery(titleQuery + summaryQuery + contentQuery + authorQuery);
        }

        private ISolrQuery GetTemplateFilterQuery()
        {
            return new SolrQuery(TemplatesImplementedField + ":(" + NewsTemplateId.ToSolrReadyString() + ")");
        }

        private ISolrQuery GetPathFilterQuery()
        {
            return new SolrQuery(ItemPathField + ":(" + ArticlesStartingPoint.ToSolrReadyString() + ")");
        }

        public static string GetSummary(IEnumerable<string> summaries, IEnumerable<string> contents)
        {
            var summary = summaries?.FirstOrDefault();
            var content = contents?.FirstOrDefault();

            return GetSummary(summary, content);
        }

        // Returns the summary if present otherwise the content up to the first punctuation mark (. ! or ?).
        public static string GetSummary(string summary, string content)
        {
            if (!string.IsNullOrEmpty(summary))
            {
                return summary;
            }

            if (string.IsNullOrEmpty(content))
            {
                return "";
            }

            // We only want the summary to be up to the first piece of punctuation.
            var firstPeriod = content.IndexOfAny(new[] { '.', '!', '?' });
            var length = firstPeriod > 0 && content.Length > firstPeriod ? firstPeriod + 1 : content.Length;

            // If the length of our summary is greater than 160, then we will shorten it so it's not too big.
            var finalLength = length > 160 ? 160 : length;
            var newSummary = content.Substring(0, finalLength);
            if (finalLength != length)
            {
                newSummary += "..."; // This means we're cutting the summary short, so add the ellipses.
            }

            return newSummary;
        }

        // Returns the first value of a list or an empty string if none are present.
        private static string ParseSolrList(IEnumerable<string> values)
        {
            if (values == null)
            {
                return "";
            }

            var list = values.ToList();
            return list.Any() ? list.First() : "";
        }
    }
}