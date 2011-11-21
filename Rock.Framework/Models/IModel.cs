using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Models
{
    public interface IModel : Rock.Cms.Security.ISecured
    {
        Guid Guid { get; set; }
    }
}