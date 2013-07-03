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
    [GroupRoleField( "", "Group Type/Role Filter", "The Group Type and role to display other members from.", false, "" )]
    [BooleanField("Show Role", "Should the member's role be displayed with their name")]
    [BooleanField("Create Group", "Should group be created if a group/role cannot be found for the current person.", true)]
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
            if ( Person != null && Person.Id > 0 )
            {
                Group group = null;

                Guid roleGuid = Guid.Empty;
                if ( Guid.TryParse( GetAttributeValue( "GroupType/RoleFilter" ), out roleGuid ) )
                {
                    using ( new Rock.Data.UnitOfWorkScope() )
                    {
                        var memberService = new GroupMemberService();
                        group = memberService.Queryable()
                            .Where( m =>
                                m.PersonId == Person.Id &&
                                m.GroupRole.Guid == roleGuid
                            )
                            .Select( m => m.Group )
                            .FirstOrDefault();

                        if ( group == null && bool.Parse( GetAttributeValue( "CreateGroup" ) ) )
                        {
                            var role = new GroupRoleService().Get( roleGuid );
                            if ( role != null && role.GroupTypeId.HasValue )
                            {
                                var groupMember = new GroupMember();
                                groupMember.PersonId = Person.Id;
                                groupMember.GroupRoleId = role.Id;

                                group = new Group();
                                group.Name = role.GroupType.Name;
                                group.GroupTypeId = role.GroupTypeId.Value;
                                group.Members.Add( groupMember );

                                var groupService = new GroupService();
                                groupService.Add( group, CurrentPersonId );
                                groupService.Save( group, CurrentPersonId );

                                group = groupService.Get( group.Id );
                            }
                        }

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

                                rGroupMembers.DataSource = new GroupMemberService().GetByGroupId( group.Id )
                                    .Where( m => m.PersonId != Person.Id )
                                    .OrderBy( m => m.Person.LastName )
                                    .ThenBy( m => m.Person.FirstName )
                                    .Take( 50 )
                                    .ToList();
                                rGroupMembers.DataBind();
                            }
                        }
                    }
                }
            }
        }
    }
}