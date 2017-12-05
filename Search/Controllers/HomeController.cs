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
        public ActionResult Index(string searchText, string keywordSelection)
        {
            if (searchText.Equals("index it"))
            {
                CreateCustomerIndex();
            }
            return View(FindArticleResults(searchText, keywordSelection));
        }

        [HttpGet]
        public ActionResult Vehicles()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Vehicles(string searchText)
        {
            if (searchText.Equals("index it"))
            {
                CreateVehicleIndex();
            }
            return View(FindVehicleResults(searchText));
        }

        [HttpGet]
        public ActionResult Customers()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Customers(string searchText)
        {
            return View(FindCustomerResults(searchText));
        }

        [HttpPost]
        public JsonResult Suggestions(string searchText)
        {
            var solr = new SolrConnection(SolrConfig.VehicleInformationIndex);
            var parameters = new Dictionary<string, string>
            {
                {"spellcheck.q", searchText},
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

        private void CreateVehicleIndex()
        {
            //var vehicleIndex = ServiceLocator.Current.GetInstance<ISolrOperations<HvtDocument>>();
            //vehicleIndex.Delete(new SolrQuery("*:*"));
            //vehicleIndex.Commit();

            //var db = new VehicleInformationDataContext();
            //var hvtDocs = from bv in db.BaseVehicles
            //              join y in db.Years on bv.YearID equals y.YearID
            //              join m in db.Makes on bv.MakeID equals m.MakeID
            //              join n in db.Models on bv.ModelID equals n.ModelID
            //              select new HvtDocument
            //              {
            //                  Id = bv.BaseVehicleID,
            //                  Year = y.YearID,
            //                  MakeId = m.MakeID,
            //                  MakeName = m.MakeName,
            //                  ModelId = n.ModelID,
            //                  ModelName = n.ModelName,
            //                  DisplayName = y.YearID + " " + m.MakeName + " " + n.ModelName,
            //                  MakeModel = m.MakeName + " " + n.ModelName
            //              };


            //foreach (var hvtDoc in hvtDocs)
            //{
            //    vehicleIndex.Add(hvtDoc);
            //}
        }

        private void CreateCustomerIndex()
        {
            //var drivetrainCustomerIndex = ServiceLocator.Current.GetInstance<ISolrOperations<CustomerDocument>>();

            //drivetrainCustomerIndex.Delete(new SolrQuery("*:*"));
            //drivetrainCustomerIndex.Commit();

            //var db = new DashboardAutoDataContext();
            //var customerDocs = from c in db.Customers
            //    join ce in db.CustomerEmails on c.CustomerPK equals ce.CustomerFK
            //    join cp in db.CustomerPhones on c.CustomerPK equals cp.CustomerFK
            //    where c.CustomerAccountFK < 200000
            //    where c.CustomerAccountFK > 100000
            //    select new CustomerDocument
            //    {
            //        CustomerAccountPK = c.CustomerAccountFK,
            //        FirstName = c.FirstName.Trim(),
            //        LastName = c.LastName.Trim(),
            //        DisplayName = new[] {c.FirstName.Trim() + " " + c.LastName.Trim()},
            //        EmailAddress = ce.EmailAddress.Trim(),
            //        PhoneNumber = cp.AreaCode.Trim() + "-" + cp.Prefix.Trim() + "-" + cp.Suffix.Trim()
            //    };

            //foreach (var doc in customerDocs)
            //{
            //    drivetrainCustomerIndex.Add(doc);
            //}
        }

        private CustomerSearchModel FindCustomerResults(string searchText)
        {
            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<CustomerDocument>>();

            var queryOptions = new QueryOptions
            {
                OrderBy = { new SortOrder("last_name_s", Order.ASC), new SortOrder("first_name_s", Order.ASC) },
                Rows = 10
            };

            var customers = new List<CustomerModel>();
            var documents = solr.Query(GetCustomerQuery(searchText), queryOptions);

            foreach (var document in documents)
            {
                var customer = new CustomerModel
                {
                    CustomerAccountPK = document.CustomerAccountPK,
                    FirstName = document.FirstName,
                    LastName = document.LastName,
                    DisplayName = ParseSolrList(document.DisplayName),
                    EmailAddress = document.EmailAddress,
                    PhoneNumber = document.PhoneNumber
                };

                customers.Add(customer);
            }

            return new CustomerSearchModel
            {
                SearchText = searchText,
                Customers = customers
            };
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

        private ArticleSearchModel FindArticleResults(string searchText, string keyword)
        {
            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<RawDocumentModel>>();

            var queryOptions = new QueryOptions
            {
                FilterQueries = new List<ISolrQuery>
                {
                    GetTemplateFilterQuery(),
                    GetPathFilterQuery(),
                    GetKeywordFilterQuery(keyword)
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

        private ISolrQuery GetCustomerQuery(string searchText)
        {
            string query;

            int pk;
            if (int.TryParse(searchText, out pk))
            {
                var pkQuery = "(customer_account_pk:\"" + pk + "\") ";
                query = pkQuery;
            }
            else
            {
                var nameQuery = "(display_name_t:\"" + searchText + "\") ";
                var emailQuery = "(email_address_s:\"" + searchText + "\") ";
                var phoneQuery = "(phone_number_s:\"" + searchText + "\") ";
                query = nameQuery + emailQuery + phoneQuery;
            }
            
            return new LocalParams { { "defType", "edismax" } } + new SolrQuery(query);
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

        private ISolrQuery GetKeywordFilterQuery(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return null;
            }

            return new SolrQuery("keywordlabels_sm:(\"" + keyword + "\")");
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