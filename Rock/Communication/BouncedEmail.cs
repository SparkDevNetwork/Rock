using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Communication
{
    public class BouncedEmail
    {
        public string Status { get; set; }
        public DateTime Created { get; set; }
        public string Reason { get; set; }
        public string Email { get; set; }
    }
}