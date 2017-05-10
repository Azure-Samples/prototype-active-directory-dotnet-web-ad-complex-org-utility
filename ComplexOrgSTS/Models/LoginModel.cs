using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ComplexOrgSTS.Models
{
    public class LoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string AuthMethod { get; set; }
        public bool Kmsi { get; set; }
        public string realm { get; set; }
        public string wtrealm { get; set; }
        public string wctx { get; set; }
        public string wct { get; set; }
        public string wreply { get; set; }
    }
}