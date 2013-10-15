//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;

using Rock.Model;

namespace Rock.PersonProfile.Badge
{
    /// <summary>
    /// Baptized Badge
    /// </summary>
    [Description( "Baptized Badge" )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Baptized" )]
    public class Baptized : IconBadge
    {
        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override System.Collections.Generic.Dictionary<string, string> AttributeValueDefaults
        {
            get
            {
                var defaults = base.AttributeValueDefaults;
                defaults["Order"] = "8";
                return defaults;
            }
        }

        /// <summary>
        /// Gets the tool tip text.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override string GetToolTipText( Person person )
        {
            var attributeValue = person.GetAttributeValue("BaptismDate");
            if (!string.IsNullOrWhiteSpace(attributeValue))
            {
                var date = DateTime.MinValue;
                if (DateTime.TryParse(attributeValue, out date) && date > DateTime.MinValue)
                {
                    return "Baptized on " + date.ToShortDateString();
                }
            }

            return "No Baptism Date";
        }

        /// <summary>
        /// Gets the icon path.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override string GetIconPath( Person person )
        {
            var attributeValue = person.GetAttributeValue( "BaptismDate" );
            if ( !string.IsNullOrWhiteSpace( attributeValue ) )
            {
                var date = DateTime.MinValue;
                if ( DateTime.TryParse( attributeValue, out date ) && date > DateTime.MinValue )
                {
                    return Path.Combine( System.Web.VirtualPathUtility.ToAbsolute( "~" ), "Assets/Images/bap-e.png" );
                }
            }

            return Path.Combine( System.Web.VirtualPathUtility.ToAbsolute( "~" ), "Assets/Images/bap-d.png" );
        }
    }
}
