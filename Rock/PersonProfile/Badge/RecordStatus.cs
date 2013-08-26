//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.PersonProfile.Badge
{
    /// <summary>
    /// Record Status Badge
    /// </summary>
    [Description( "Record Status Badge" )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Record Status" )]
    public class RecordStatus : TextBadge
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
                defaults["Order"] = "2";
                return defaults;
            }
        }

        /// <summary>
        /// Gets the type of the badge.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override string GetBadgeType( Person person )
        {
            return "Important";
        }

        /// <summary>
        /// Gets the text.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override string GetText( Person person )
        {
            if ( Person != null )
            {
                // Show record status only if it's not 'Active'
                if ( Person.RecordStatusValueId.HasValue )
                {
                    var recordStatusValue = DefinedValueCache.Read( Person.RecordStatusValueId.Value );
                    if ( string.Compare( recordStatusValue.Guid.ToString(), Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE, true ) != 0 )
                    {
                        return recordStatusValue.Name;
                    }
                }
            }

            return string.Empty;
        }

    }
}
