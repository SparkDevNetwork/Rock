//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.PersonProfile.Badge
{
    /// <summary>
    /// Person Status Badge
    /// </summary>
    [Description( "Person Status Badge" )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata("ComponentName", "Person Status")]
    public class PersonStatus : TextBadge
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
                defaults["Order"] = "0";
                return defaults;
            }
        }

        /// <summary>
        /// Gets the type of the badge.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override BadgeType GetBadgeType( Person person )
        {
            return BadgeType.Success;
        }

        /// <summary>
        /// Gets the text.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override string GetText(Person person )
        {
            if ( Person != null )
            {
                return Person.PersonStatusValueId.DefinedValue();
            }

            return string.Empty;
        } 
    }
}
