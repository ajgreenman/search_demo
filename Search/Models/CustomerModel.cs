using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Search.Models
{
    public class CustomerModel
    {
        public int CustomerAccountPK { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
    }
}