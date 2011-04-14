using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Services;
using System.Data.Services.Common;
using System.Linq;
using System.Web;

namespace Rock.Models
{
    /// <summary>
    /// Base class that all models need to inherit from
    /// </summary>
    [DataServiceKey("Id")]
    [IgnoreProperties(new[] { "ParentAuthority", "SupportedActions", "AuthEntity" })]
    public abstract class Model :  Rock.Cms.Security.ISecured
    {
        // Note: The DataServiceKey attribute is part of the magic behind WCF Data Services. This allows
        // the service to interface with EF and fetch data.

        [Key]
        public int Id { get; set; }

        #region ISecured implementation

        [NotMapped]
        public virtual string AuthEntity { get { return string.Empty; } }

        [NotMapped]
        public virtual Rock.Cms.Security.ISecured ParentAuthority { get { return null; } }

        public virtual List<string> SupportedActions
        {
            get { return new List<string>() { "View", "Edit", "Secure" }; }
        }

        public virtual bool DefaultAuthorization (string action)
        {
            return action == "View";
        }

        #endregion
    }
}