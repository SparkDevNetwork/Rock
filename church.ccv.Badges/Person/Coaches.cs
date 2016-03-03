using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;


namespace church.ccv.Badges.Person
{
    /// <summary>
    /// Coaches Badge
    /// </summary>
    [Description( "Displays the coaches for the specific person." )]
    [Export( typeof( Rock.PersonProfile.BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Coaches" )]

    [TextField( "Group Types", "List of group types to display." )]
    [TextField( "Label", "Label to display badge.", true, "Coach" )]
    class Coaches : Rock.PersonProfile.BadgeComponent
    {
        public override void Render( PersonBadgeCache badge, HtmlTextWriter writer )
        {
            var rockContext = new RockContext();

            var label = GetAttributeValue( badge, "Label" );
            var groupTypes = GetAttributeValue( badge, "GroupTypes" );
            var groupTypesIds = groupTypes.Split( ',' ).AsIntegerList();

            var groups = new GroupMemberService( rockContext ).Queryable()
                                                .Where( m => groupTypesIds.Contains( m.Group.GroupTypeId )
                                                    && m.GroupMemberStatus != GroupMemberStatus.Inactive
                                                    && m.Group.IsActive != false
                                                    && m.PersonId == Person.Id
                                                    && m.GroupRole.IsLeader != true )
                                                .Select( m => m.Group.Id )
                                                .ToList();

            List<string> groupCoaches = new GroupMemberService( rockContext ).Queryable()
                                                .Where( m => groups.Contains( m.Group.Id )
                                                   && m.GroupMemberStatus != GroupMemberStatus.Inactive
                                                   && m.Group.IsActive != false
                                                   && m.GroupRole.IsLeader == true )
                                                .Select( m => m.Person.NickName + " " + m.Person.LastName )
                                                .ToList();
            if ( groupCoaches.Any() )
            {
                writer.Write(
                    @"<script>
                        $(document).ready(function(){ $('[data-toggle=tooltip]').tooltip() });
                    </script>

                    <style>
                        .label-coach {
                            background-color: #ee7624;
                        }
                    </style>

                    <span data-toggle='tooltip' title='" + string.Join( ",", groupCoaches ) +
                    "' class='label label-coach'>" + label + "</span>" );
            }
        }
    }
}
