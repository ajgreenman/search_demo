using SolrNet.Attributes;

namespace Search.Models
{
    public class CustomerDocument
    {
        [SolrUniqueKey("customer_account_pk")]
        public int CustomerAccountPK { get; set; }

        [SolrField("first_name_s")]
        public string FirstName { get; set; }

        [SolrField("last_name_s")]
        public string LastName { get; set; }

        [SolrField("display_name_t")]
        public string[] DisplayName { get; set; }

        [SolrField("email_address_s")]
        public string EmailAddress { get; set; }

        [SolrField("phone_number_s")]
        public string PhoneNumber { get; set; }
    }
}