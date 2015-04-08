// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// Renders the related members of a group (typically used for the Relationships group and the Peer Network group)
    /// </summary>
    [DisplayName( "Relationships" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Allows you to view relationships of a particular person." )]

    [GroupRoleField( "", "Group Type/Role Filter", "The Group Type and role to display other members from.", false, "" )]
    [BooleanField("Show Role", "Should the member's role be displayed with their name")]
    [BooleanField("Create Group", "Should group be created if a group/role cannot be found for the current person.", true)]
    public partial class Relationships : Rock.Web.UI.PersonBlock
    {
        protected bool CanEdit = false;
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

            lbAdd.Visible = IsKnownRelationships;

            rGroupMembers.ItemDataBound += rGroupMembers_ItemDataBound;
            rGroupMembers.ItemCommand += rGroupMembers_ItemCommand;

            modalAddPerson.SaveClick += modalAddPerson_SaveClick;
            modalAddPerson.OnCancelScript = string.Format( "$('#{0}').val('');", hfRoleId.ClientID );

            CanEdit = IsUserAuthorized( Authorization.EDIT );
            lbAdd.Visible = CanEdit;

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

            if ( Person != null && Person.Id != 0 )
            {
                upRelationships.Visible = true;

                bool.TryParse( GetAttributeValue( "ShowRole" ), out ShowRole );

                if ( !Page.IsPostBack )
                {
                    BindData();
                }
                else
                {
                    ShowDialog();
                }
            }
            else
            {
                upRelationships.Visible = false;
            }
        }

        protected void lbAdd_Click( object sender, EventArgs e )
        {
            if ( CanEdit )
            {
                ShowModal( null, null, null );
            }
        }

        void rGroupMembers_ItemDataBound( object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e )
        {
            var lbEdit = e.Item.FindControl( "lbEdit" ) as LinkButton;
            var lbRemove = e.Item.FindControl( "lbRemove" ) as LinkButton;

            if ( lbEdit != null && lbRemove != null)
            {
                lbEdit.Visible = CanEdit;
                lbRemove.Visible = CanEdit;
            }
        }


        void rGroupMembers_ItemCommand( object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e )
        {
            if ( CanEdit )
            {
                int groupMemberId = int.MinValue;
                if ( int.TryParse( e.CommandArgument.ToString(), out groupMemberId ) )
                {
                    var rockContext = new RockContext();
                    var service = new GroupMemberService( rockContext );
                    var groupMember = service.Get( groupMemberId );
                    if ( groupMember != null )
                    {
                        if ( e.CommandName == "EditRole" )
                        {
                            ShowModal( groupMember.Person, groupMember.GroupRoleId, groupMemberId );
                        }

                        else if ( e.CommandName == "RemoveRole" )
                        {
                            if ( IsKnownRelationships )
                            {
                                var inverseGroupMember = service.GetInverseRelationship( groupMember, false, CurrentPersonAlias );
                                if ( inverseGroupMember != null )
                                {
                                    service.Delete( inverseGroupMember );
                                }
                            }

                            service.Delete( groupMember );

                            rockContext.SaveChanges();

                            BindData();
                        }
                    }
                }
            }
        }

        void modalAddPerson_SaveClick( object sender, EventArgs e )
        {
            if ( CanEdit )
            {
                if ( ppPerson.PersonId.HasValue )
                {
                    int? roleId = grpRole.GroupRoleId;
                    if ( roleId.HasValue )
                    {
                        var rockContext = new RockContext();
                        var memberService = new GroupMemberService( rockContext );

                        var group = memberService.Queryable()
                            .Where( m =>
                                m.PersonId == Person.Id &&
                                m.GroupRole.Guid == ownerRoleGuid
                            )
                            .Select( m => m.Group )
                            .FirstOrDefault();

                        if ( group != null )
                        {
                            GroupMember groupMember = null;
                            int? groupMemberId = hfRoleId.Value.AsIntegerOrNull();
                            if ( groupMemberId.HasValue )
                            {
                                groupMember = memberService.Queryable()
                                .Where( m => m.Id == groupMemberId.Value )
                                .FirstOrDefault();
                            }

                            if ( groupMember == null )
                            {
                                groupMember = new GroupMember();
                                groupMember.GroupId = group.Id;
                                memberService.Add( groupMember );
                            }

                            GroupMember formerInverseGroupMember = null;
                            if ( IsKnownRelationships )
                            {
                                formerInverseGroupMember = memberService.GetInverseRelationship( groupMember, false, CurrentPersonAlias );
                            }

                            groupMember.PersonId = ppPerson.PersonId.Value;
                            groupMember.GroupRoleId = roleId.Value;

                            rockContext.SaveChanges();

                            if ( IsKnownRelationships )
                            {
                                var inverseGroupMember = memberService.GetInverseRelationship(
                                    groupMember, bool.Parse( GetAttributeValue( "CreateGroup" ) ), CurrentPersonAlias );
                                if ( inverseGroupMember != null )
                                {
                                    rockContext.SaveChanges();
                                    if ( formerInverseGroupMember != null && formerInverseGroupMember.Id != inverseGroupMember.Id )
                                    {
                                        memberService.Delete( formerInverseGroupMember );
                                        rockContext.SaveChanges();
                                    }
                                }
                            }
                        }

                    }

                }

                HideDialog();

                BindData();
            }
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
                    using ( var rockContext = new RockContext() )
                    {
                        var memberService = new GroupMemberService( rockContext );
                        var group = memberService.Queryable()
                            .Where( m =>
                                m.PersonId == Person.Id &&
                                m.GroupRole.Guid == ownerRoleGuid
                            )
                            .Select( m => m.Group )
                            .FirstOrDefault();

                        if ( group == null && bool.Parse( GetAttributeValue( "CreateGroup" ) ) )
                        {
                            var role = new GroupTypeRoleService( rockContext ).Get( ownerRoleGuid );
                            if ( role != null && role.GroupTypeId.HasValue )
                            {
                                var groupMember = new GroupMember();
                                groupMember.PersonId = Person.Id;
                                groupMember.GroupRoleId = role.Id;

                                group = new Group();
                                group.Name = role.GroupType.Name;
                                group.GroupTypeId = role.GroupTypeId.Value;
                                group.Members.Add( groupMember );

                                var groupService = new GroupService( rockContext );
                                groupService.Add( group );
                                rockContext.SaveChanges();

                                group = groupService.Get( group.Id );
                            }
                        }

                        if ( group != null )
                        {
                            if ( group.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                            {
                                phGroupTypeIcon.Controls.Clear();
                                if ( !string.IsNullOrWhiteSpace( group.GroupType.IconCssClass ) )
                                {
                                    phGroupTypeIcon.Controls.Add(
                                        new LiteralControl(
                                            string.Format( "<i class='{0}'></i>", group.GroupType.IconCssClass ) ) );
                                }

                                lGroupName.Text = group.Name.Pluralize();

                                phEditActions.Visible = group.IsAuthorized( Authorization.EDIT, CurrentPerson );

                                // TODO: How many implied relationships should be displayed

                                rGroupMembers.DataSource = new GroupMemberService( rockContext ).GetByGroupId( group.Id )
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

        private void ShowModal( Person person, int? roleId, int? groupMemberId  )
        {
            Guid roleGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( "GroupType/RoleFilter" ), out roleGuid ) )
            {
                var groupTypeRoleService = new GroupTypeRoleService( new RockContext() );
                var role = groupTypeRoleService.Get( ownerRoleGuid );
                grpRole.ExcludeGroupRoles.Add( role.Id );

                grpRole.GroupTypeId = groupTypeRoleService.Queryable()
                    .Where( r => r.Guid == roleGuid )
                    .Select( r => r.GroupTypeId )
                    .FirstOrDefault();

            }
            grpRole.GroupRoleId = roleId;
            ppPerson.SetValue( person );

            ShowDialog( groupMemberId ?? 0, true );
        }

        private void ShowDialog( int roleId, bool setValues = false )
        {
            hfRoleId.Value = roleId.ToString();
            ShowDialog( setValues );
        }


        private void ShowDialog( bool setValues = false )
        {
            if ( !string.IsNullOrWhiteSpace( hfRoleId.Value ) )
            {
                modalAddPerson.Show();
            }
        }

        private void HideDialog()
        {
            modalAddPerson.Hide();
            hfRoleId.Value = string.Empty;
        }
    }
}