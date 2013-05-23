//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Model;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// Renders the related members of a group (typically used for the Relationships group and the Peer Network group)
    /// </summary>
    [GroupTypeField( "Group Type", "The type of group to display.  Any group of this type that person belongs to will be displayed", true )]
    // TODO: Make a group/role picker
    [TextField( "Group Role Filter", "Delimited list of group role guid's that if entered, will only show groups where selected person is one of the roles.", false, "" )]
    [BooleanField("Show Role", "Should the member's role be displayed with their name")]
    public partial class Relationships : Rock.Web.UI.PersonBlock
    {
        protected bool ShowRole = false;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            bool.TryParse( GetAttributeValue( "ShowRole" ), out ShowRole );

            BindData();
        }

        /// <summary>
        /// Binds the data.
        /// </summary>
        private void BindData()
        {
            Guid GroupTypeGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( "GroupType" ), out GroupTypeGuid ) )
            {
                var filterRoles = new List<Guid>();
                foreach ( string stringRoleGuid in GetAttributeValue( "GroupRoleFilter" ).SplitDelimitedValues() )
                {
                    Guid roleGuid = Guid.Empty;
                    if ( Guid.TryParse( stringRoleGuid, out roleGuid ) )
                    {
                        filterRoles.Add( roleGuid );
                    }
                }

                var memberService = new GroupMemberService();
                var group = memberService.Queryable()
                    .Where( m =>
                        m.PersonId == Person.Id &&
                        m.Group.GroupType.Guid == GroupTypeGuid &&
                        ( filterRoles.Count == 0 || filterRoles.Contains( m.GroupRole.Guid ) ) )
                    .Select( m => m.Group )
                    .FirstOrDefault();

                if ( group != null )
                {
                    if ( group.IsAuthorized( "View", CurrentPerson ) )
                    {
                        if ( !string.IsNullOrWhiteSpace( group.GroupType.IconCssClass ) )
                        {
                            phGroupTypeIcon.Controls.Add(
                                new LiteralControl(
                                    string.Format( "<i class='{0}'></i>", group.GroupType.IconCssClass ) ) );
                        }

                        lGroupName.Text = group.Name;

                        phEditActions.Visible = group.IsAuthorized( "Edit", CurrentPerson );

                        // TODO: How many implied relationships should be displayed

                        rGroupMembers.DataSource = new GroupMemberService().GetByGroupId(group.Id)
                            .Where( m => m.PersonId != Person.Id )
                            .OrderBy( m => m.Person.LastName )
                            .ThenBy( m => m.Person.FirstName )
                            .Take(50)
                            .ToList();
                        rGroupMembers.DataBind();
                    }
                }
            }
        }
    }
}