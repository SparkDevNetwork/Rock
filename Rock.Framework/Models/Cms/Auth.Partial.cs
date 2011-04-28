using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Caching;
using System.Text;

namespace Rock.Models.Cms
{
    public partial class Auth
    {
        public override bool DefaultAuthorization( string action )
        {
            return false;
        }
    }
}
