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
using Rock.Data;
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
        protected Guid ownerRoleGuid = Guid.Empty;
        protected bool IsKnownRelationships = false;

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( Guid.TryParse( GetAttributeValue( "GroupType/RoleFilter" ), out ownerRoleGuid ) )
            {
                IsKnownRelationships = ownerRoleGuid.Equals(new Guid(Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER));
            }

            rGroupMembers.ItemCommand += rGroupMembers_ItemCommand;
            modalAddPerson.SaveClick += modalAddPerson_SaveClick;

            string script = @"
    $('a.remove-relationship').click(function(){
        return confirm('Are you sure you want to remove this relationship?');
    });
";
            ScriptManager.RegisterStartupScript( rGroupMembers, rGroupMembers.GetType(), "ConfirmRemoveRelationship", script, true );
        }

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

        protected void lbAdd_Click( object sender, EventArgs e )
        {
            ShowModal( null, null );
        }

        void rGroupMembers_ItemCommand( object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e )
        {
            int groupMemberId = int.MinValue;
            if ( int.TryParse( e.CommandArgument.ToString(), out groupMemberId ) )
            {
                var service = new GroupMemberService();
                var groupMember = service.Get( groupMemberId );
                if ( groupMember != null )
                {
                    if ( e.CommandName == "EditRole" )
                    {
                        ShowModal(groupMember.PersonId, groupMember.GroupRoleId);
                    }

                    else if ( e.CommandName == "RemoveRole" )
                    {
                        if ( IsKnownRelationships )
                        {
                            var inverseGroupMember = service.GetInverseRelationship( groupMember, false, CurrentPersonId );
                            if ( inverseGroupMember != null )
                            {
                                service.Delete( inverseGroupMember, CurrentPersonId );
                            }
                        }

                        service.Delete( groupMember, CurrentPersonId );
                        service.Save( groupMember, CurrentPersonId );

                        BindData();
                    }
                }
            }
        }

        void modalAddPerson_SaveClick( object sender, EventArgs e )
        {
            int? personId = ppPerson.PersonId;
            int? roleId = grpRole.GroupRoleId;
            if ( personId.HasValue && roleId.HasValue )
            {
                using ( new UnitOfWorkScope() )
                {
                    var memberService = new GroupMemberService();
                    var group = memberService.Queryable()
                        .Where( m =>
                            m.PersonId == Person.Id &&
                            m.GroupRole.Guid == ownerRoleGuid
                        )
                        .Select( m => m.Group )
                        .FirstOrDefault();

                    if (group != null)
                    {
                        var groupMember = memberService.Queryable()
                            .Where( m => 
                                m.GroupId == group.Id &&
                                m.PersonId == personId.Value &&
                                m.GroupRoleId == roleId.Value)
                            .FirstOrDefault();

                        if (groupMember == null)
                        {
                            groupMember = new GroupMember();
                            groupMember.GroupId = group.Id;
                            groupMember.PersonId = personId.Value;
                            groupMember.GroupRoleId = roleId.Value;
                            memberService.Add(groupMember, CurrentPersonId);
                        }

                        memberService.Save(groupMember, CurrentPersonId);
                        if (IsKnownRelationships)
                        {
                            var inverseGroupMember = memberService.GetInverseRelationship(
                                groupMember, bool.Parse( GetAttributeValue( "CreateGroup" ) ), CurrentPersonId );
                            if (inverseGroupMember != null)
                            {
                                memberService.Save(inverseGroupMember, CurrentPersonId);
                            }
                        }
                    }
                }
            }

            BindData();
        }

        /// <summary>
        /// Binds the data.
        /// </summary>
        private void BindData()
        {
            if ( Person != null && Person.Id > 0 )
            {
                if ( ownerRoleGuid != Guid.Empty )
                {
                    using ( new UnitOfWorkScope() )
                    {
                        var memberService = new GroupMemberService();
                        var group = memberService.Queryable()
                            .Where( m =>
                                m.PersonId == Person.Id &&
                                m.GroupRole.Guid == ownerRoleGuid
                            )
                            .Select( m => m.Group )
                            .FirstOrDefault();

                        if ( group == null && bool.Parse( GetAttributeValue( "CreateGroup" ) ) )
                        {
                            var role = new GroupRoleService().Get( ownerRoleGuid );
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

        private void ShowModal(int? personId, int? roleId)
        {
            Guid roleGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( "GroupType/RoleFilter" ), out roleGuid ) )
            {
                grpRole.GroupTypeId = new GroupRoleService().Queryable()
                    .Where( r => r.Guid == roleGuid )
                    .Select( r => r.GroupTypeId )
                    .FirstOrDefault();

            }

            ppPerson.PersonId = personId;
            grpRole.GroupRoleId = roleId;

            modalAddPerson.Show();
        }
    }
}