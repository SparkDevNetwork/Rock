//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Rock.Model;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MarketingCampaignAudienceDto : IDto
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get
            {
                if ( _name == null )
                {
                    var audience = DefinedValue.Read( this.AudienceTypeValueId );
                    if ( audience != null )
                    {
                        _name = audience.Name;
                    }
                }
                return _name ?? string.Empty;
            }
        }
        private string _name;
    }
}