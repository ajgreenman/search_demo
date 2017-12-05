using Search.Models;
using SolrNet;

namespace Search
{
    public static class SolrConfig
    {
        public static string VehicleInformationIndex = "http://vdcsolrdv01:8983/solr/vehicle_information_index";
        public static string DrivetrainCustomerIndex = "http://vdcsolrdv01:8983/solr/drivetrain_customer_index";

        public static void RegisterSolr()
        {
            Startup.Init<RawDocumentModel>("http://vdcsolrmo01:8983/solr/sitecore_web_index");
            Startup.Init<HvtDocument>(VehicleInformationIndex);
            Startup.Init<CustomerDocument>(DrivetrainCustomerIndex);
        }
    }
}