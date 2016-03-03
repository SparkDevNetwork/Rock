
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.PersonProfile.Badge
{
    /// <summary>
    /// Campus with Leader Badge
    /// </summary>
    [Description( "Campus with Leader Badge" )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Campus with Leader" )]
    public class CampusWithLeader : HighlightLabelBadge
    {

        /// <summary>
        /// Gets the badge label
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override HighlightLabel GetLabel( Person person )
        {
            if ( ParentPersonBlock != null )
            {
                // Campus is associated with the family group(s) person belongs to.
                var families = ParentPersonBlock.PersonGroups( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
                if ( families != null )
                {
                    var rockContext = new RockContext();

                    var label = new HighlightLabel();
                    label.LabelType = LabelType.Campus;

                    var campusNames = new List<string>();
                    var campusLeaders = new List<Person>();

                    foreach ( int campusId in families
                        .Where( g => g.CampusId.HasValue )
                        .Select( g => g.CampusId )
                        .Distinct()
                        .ToList() )
                    {
                        var campus = Rock.Web.Cache.CampusCache.Read( campusId );

                        campusNames.Add( campus.Name );
                        campusLeaders.Add( new PersonAliasService( rockContext ).GetPerson( (int)campus.LeaderPersonAliasId ) );
                    }
                        

                    label.Text = campusNames.ToList().AsDelimited( ", " );
                    label.ToolTip = campusLeaders.Select( p => p.NickName + " " + p.LastName ).ToList().AsDelimited( ", " );

                    return label;
                }
            }

            return null;

        }

    }
}
