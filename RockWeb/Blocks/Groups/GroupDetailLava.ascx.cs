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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Entity;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Security;

namespace RockWeb.Blocks.Groups
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Group Detail Lava" )]
    [Category( "Groups" )]
    [Description( "Presents the details of a group using Lava" )]
    [BooleanField("Enable Debug", "Shows the fields available to merge in lava.", false)]
    [CodeEditorField( "Lava Template", "The lava template to use to format the group details.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, true, "{% include '~~/Assets/Lava/GroupDetail.lava' %}" )]
    [LinkedPage("Person Detail Page", "Page to link to for more information on a group member.", false)]
    [LinkedPage( "Group Member Add Page", "Page to use for adding a new group member. If no page is provided the built in group member edit panel will be used. This panel allows the individual to search the database.", false )]
    public partial class GroupDetailLava : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties
        public bool IsEditingGroup
        {
            get
            {
                return ViewState["IsEditingGroup"] as bool? ?? false;
            }
            set
            {
                ViewState["IsEditingGroup"] = value;
            }
        }

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            /*if ( IsEditingGroup == true )
            {
                Group group = new GroupService(new RockContext()).Get( groupId );
                group.LoadAttributes();

                phAttributes.Controls.Clear();
                Rock.Attribute.Helper.AddEditControls( group, phAttributes, false, BlockValidationGroup );
            }*/
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            
            //if ( !Page.IsPostBack )
            //{
                RouteAction();
            //}
            /*else
            {
                var rockContext = new RockContext();
                GroupService groupService = new GroupService( rockContext );

                int groupId = -1;
                if ( !string.IsNullOrWhiteSpace( PageParameter( "GroupId" ) ) )
                {
                    groupId = Convert.ToInt32( PageParameter( "GroupId" ) );
                }
            }*/
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            RouteAction();
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            GroupService groupService = new GroupService( rockContext );

            int groupId = -1;
            if ( !string.IsNullOrWhiteSpace( PageParameter( "GroupId" ) ) )
            {
                groupId = Convert.ToInt32( PageParameter( "GroupId" ) );
            }

            Group group = groupService.Get(groupId);

            if ( group != null && group.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                group.Name = tbName.Text;
                group.Description = tbDescription.Text;
                group.IsActive = cbIsActive.Checked;

                // set attributes
                group.LoadAttributes( rockContext );
                Rock.Attribute.Helper.GetEditValues( phAttributes, group );

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    group.SaveAttributeValues( rockContext );
                } );
            }

            this.IsEditingGroup = false;

            Response.Redirect( CreateCancelLink() );
        }

        protected void lbCancel_Click( object sender, EventArgs e )
        {
            Response.Redirect( CreateCancelLink() );
        }

        protected void btnSaveGroupMember_Click( object sender, EventArgs e )
        {
            int groupId = 0;
            int.TryParse( PageParameter( "GroupId" ), out groupId );
            
            int groupMemberId = 0;
            if ( int.TryParse( PageParameter( "GroupMemberId" ), out groupMemberId ) );
            
            var rockContext = new RockContext();
            GroupMemberService groupMemberService = new GroupMemberService( rockContext );

            GroupTypeRole role = new GroupTypeRoleService( rockContext ).Get( ddlGroupRole.SelectedValueAsInt() ?? 0 );

            var groupMember = groupMemberService.Get( groupMemberId );

            if ( groupMemberId.Equals( 0 ) )
            {
                groupMember = new GroupMember { Id = 0 };
                groupMember.GroupId = groupId;

                // check to see if the person is alread a member of the gorup/role
                var existingGroupMember = groupMemberService.GetByGroupIdAndPersonIdAndGroupRoleId(
                    groupId, ppGroupMemberPerson.SelectedValue ?? 0, ddlGroupRole.SelectedValueAsId() ?? 0 );

                if ( existingGroupMember != null )
                {
                    // if so, don't add and show error message
                    var person = new PersonService( rockContext ).Get( (int)ppGroupMemberPerson.PersonId );

                    nbGroupMemberErrorMessage.Title = "Person Already In Group";
                    nbGroupMemberErrorMessage.Text = string.Format(
                        "{0} already belongs to the {1} role for this {2}, and cannot be added again with the same role.",
                        person.FullName,
                        ddlGroupRole.SelectedItem.Text,
                        role.GroupType.GroupTerm,
                        RockPage.PageId,
                        existingGroupMember.Id );
                    return;
                }
            }

            groupMember.PersonId = ppGroupMemberPerson.PersonId.Value;
            groupMember.GroupRoleId = role.Id;
            groupMember.GroupMemberStatus = rblStatus.SelectedValueAsEnum<GroupMemberStatus>();

            groupMember.LoadAttributes();

            Rock.Attribute.Helper.GetEditValues( phAttributes, groupMember );

            // using WrapTransaction because there are two Saves
            rockContext.WrapTransaction( () =>
            {
                if ( groupMember.Id.Equals( 0 ) )
                {
                    groupMemberService.Add( groupMember );
                }

                rockContext.SaveChanges();
                groupMember.SaveAttributeValues( rockContext );
            } );

            Group group = new GroupService( rockContext ).Get( groupMember.GroupId );
            if ( group.IsSecurityRole || group.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() ) )
            {
                Rock.Security.Role.Flush( group.Id );
                Rock.Security.Authorization.Flush();
            }
            
            Response.Redirect( CreateCancelLink() );
        }
        protected void btnCancelGroupMember_Click( object sender, EventArgs e )
        {
            Response.Redirect( CreateCancelLink() );
        }

        #endregion

        #region Methods

        // todo delete
        private string CreateCancelLink()
        {
            if ( Request.Url.Port == 80 || Request.Url.Port == 443 )
            {
                return string.Format( "{0}://{1}{2}?GroupId={3}", Request.Url.Scheme, Request.Url.Host, Request.Url.LocalPath, PageParameter( "GroupId" ) );
                
            }
            else
            {
                return string.Format( "{0}://{1}:{2}{3}?GroupId={4}", Request.Url.Scheme, Request.Url.Host, Request.Url.Port.ToString(), Request.Url.LocalPath, PageParameter( "GroupId" ) );
            }
        }

        private void RouteAction()
        {

            if ( Request.Form["__EVENTARGUMENT"] != null )
            {

                string[] eventArgs = Request.Form["__EVENTARGUMENT"].Split( '^' );

                if ( eventArgs.Length == 2 )
                {
                    string action = eventArgs[0];
                    string parameters = eventArgs[1];

                    int argument = 0;
                    int.TryParse( parameters, out argument );

                    switch ( action )
                    {
                        case "DeleteGroupMember":
                            DeleteGroupMember( argument );
                            pnlGroupEdit.Visible = false;
                            pnlGroupView.Visible = true;
                            pnlEditGroupMember.Visible = false;
                            DisplayContent();
                            break;
                    }
                }
            }

            

            
            int groupMemberId = 0;

            switch ( PageParameter( "Action" ) )
            {
                case "EditGroup":
                    pnlGroupEdit.Visible = true;
                    pnlGroupView.Visible = false;
                    pnlEditGroupMember.Visible = false;
                    DisplayEditGroup();
                    break;
                case "AddGroupMember":
                    AddGroupMember();
                    break;
                case "EditGroupMember":
                    if ( int.TryParse( PageParameter( "GroupMemberId" ), out groupMemberId ) )
                    {
                        DisplayEditGroupMember( groupMemberId );
                    }
                    break;
                default:
                    pnlGroupEdit.Visible = false;
                    pnlGroupView.Visible = true;
                    pnlEditGroupMember.Visible = false;
                    DisplayContent();
                    break;
            }
        }

        private void AddGroupMember()
        {
            int groupId = 0;
            int.TryParse( PageParameter( "GroupId" ), out groupId );

            var personAddPage = GetAttributeValue( "GroupMemberAddPage" );

            if ( personAddPage == string.Empty )
            {
                DisplayEditGroupMember( 0 );
            }
            else
            {
                // redirect to the add page provided
                var group = new GroupService( new RockContext() ).Get( groupId );
                if ( group != null )
                {
                    var queryParams = new Dictionary<string, string>();
                    queryParams.Add( "GroupId", group.Id.ToString() );
                    queryParams.Add( "GroupName", group.Name );
                    NavigateToLinkedPage( "GroupMemberAddPage", queryParams );
                }
            }
        }

        private void DisplayEditGroupMember(int groupMemberId)
        {
            pnlGroupEdit.Visible = false;
            pnlGroupView.Visible = true;
            pnlEditGroupMember.Visible = true;
            
            int groupId = 0;
            int.TryParse( PageParameter( "GroupId" ), out groupId );
            
            RockContext rockContext = new RockContext();
            GroupMemberService groupMemberService = new GroupMemberService(rockContext);

            var groupMember = groupMemberService.Get(groupMemberId);

            if ( groupMember == null )
            {
                groupMember = new GroupMember { Id = 0 };
                groupMember.GroupId = groupId;
                groupMember.Group = new GroupService( rockContext ).Get( groupMember.GroupId );
                groupMember.GroupRoleId = groupMember.Group.GroupType.DefaultGroupRoleId ?? 0;
                groupMember.GroupMemberStatus = GroupMemberStatus.Active;
            }

            // load dropdowns
            LoadGroupMemberDropDowns( groupId );

            // set values
            ppGroupMemberPerson.SetValue( groupMember.Person );
            ddlGroupRole.SetValue( groupMember.GroupRoleId );
            rblStatus.SetValue( (int)groupMember.GroupMemberStatus );

            // set attributes
            groupMember.LoadAttributes();
            Rock.Attribute.Helper.AddEditControls( groupMember, phGroupMemberAttributes, true, "", true );
        }

        private void LoadGroupMemberDropDowns(int groupId)
        {
            Group group = new GroupService( new RockContext() ).Get( groupId );
            if ( group != null )
            {
                ddlGroupRole.DataSource = group.GroupType.Roles.OrderBy( a => a.Order ).ToList();
                ddlGroupRole.DataBind();
            }

            rblStatus.BindToEnum<GroupMemberStatus>();
        }
        
        private void DisplayContent() {

            int groupId = -1;
            
            if ( !string.IsNullOrWhiteSpace( PageParameter( "GroupId" ) ) )
            {
                groupId = Convert.ToInt32( PageParameter( "GroupId" ) );
            }

            if ( groupId != -1 )
            {
                RockContext rockContext = new RockContext();
                GroupService groupService = new GroupService( rockContext );

                bool enableDebug = GetAttributeValue( "EnableDebug" ).AsBoolean();

                var qry = groupService
                    .Queryable( "GroupLocations,Members,Members.Person" )
                    .Where( g => g.Id == groupId );
                
                if ( !enableDebug )
                {
                    qry = qry.AsNoTracking();
                }
                var group = qry.FirstOrDefault();
                
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Group", group );

                // add linked pages
                Dictionary<string, object> linkedPages = new Dictionary<string, object>();
                linkedPages.Add( "PersonDetailPage", LinkedPageUrl( "PersonDetailPage", null ) );
                mergeFields.Add( "LinkedPages", linkedPages );

                // add collection of allowed security actions
                Dictionary<string, object> securityActions = new Dictionary<string, object>();
                securityActions.Add( "View", group.IsAuthorized( Authorization.VIEW, CurrentPerson ) );
                securityActions.Add( "Edit", group.IsAuthorized( Authorization.EDIT, CurrentPerson ) );
                securityActions.Add( "Administrate", group.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) );
                mergeFields.Add( "AllowedActions", securityActions );

                mergeFields.Add( "CurrentPerson", CurrentPerson );
                var globalAttributeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( CurrentPerson );
                globalAttributeFields.ToList().ForEach( d => mergeFields.Add( d.Key, d.Value ) );

                string template = GetAttributeValue( "LavaTemplate" );

                // show debug info
                if ( enableDebug && IsUserAuthorized( Authorization.EDIT ) )
                {
                    string postbackCommands = @"<h5>Available Postback Commands</h5>
                                                    <ul>
                                                        <li><strong>EditGroup:</strong> Shows a panel for modifing group info. Expects a group id. <code>{{ Group.Id | Postback:'EditGroup' }}</code></li>
                                                        <li><strong>AddGroupMember:</strong> Shows a panel for adding group info. Does not require input. <code>{{ '' | Postback:'AddGroupMember' }}</code></li>
                                                        <li><strong>EditGroupMember:</strong> Shows a panel for modifing group info. Expects a group member id. <code>{{ member.Id | Postback:'EditGroupMember' }}</code></li>
                                                        <li><strong>DeleteGroupMember:</strong> Deletes a group member. Expects a group member id. <code>{{ member.Id | Postback:'DeleteGroupMember' }}</code></li>
                                                    </ul>";
                    
                    lDebug.Visible = true;
                    lDebug.Text = mergeFields.lavaDebugInfo( null, "", postbackCommands );
                }

                lContent.Text = template.ResolveMergeFields( mergeFields ).ResolveClientIds( upnlContent.ClientID);
            }
            else
            {
                lContent.Text = "<div class='alert alert-warning'>No group was available from the querystring.</div>";
            }

        }

        private void DeleteGroupMember( int groupMemberId )
        {
            RockContext rockContext = new RockContext();
            GroupMemberService groupMemberService = new GroupMemberService( rockContext );

            var groupMember = groupMemberService.Get( groupMemberId );
            if ( groupMember != null )
            {
                groupMemberService.Delete( groupMember );
            }

            rockContext.SaveChanges();
        }

        private void DisplayEditGroup()
        {
            this.IsEditingGroup = true;
            
            int groupId = -1;
            if ( !string.IsNullOrWhiteSpace( PageParameter( "GroupId" ) ) )
            {
                groupId = Convert.ToInt32( PageParameter( "GroupId" ) );
            }

            if ( groupId != -1 )
            {
                
                    RockContext rockContext = new RockContext();
                    GroupService groupService = new GroupService( rockContext );

                    var qry = groupService
                            .Queryable( "GroupLocations" )
                            .Where( g => g.Id == groupId );

                    var group = qry.FirstOrDefault();

                    if ( group.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                    {
                        tbName.Text = group.Name;
                        tbDescription.Text = group.Description;
                        cbIsActive.Checked = group.IsActive;

                        group.LoadAttributes();
                        phAttributes.Controls.Clear();
                        Rock.Attribute.Helper.AddEditControls( group, phAttributes, true, BlockValidationGroup );
                    }
                    else
                    {
                        lContent.Text = "<div class='alert alert-warning'>You do not have permission to edit this group.</div>";
                    }
            }
            else
            {
                lContent.Text = "<div class='alert alert-warning'>No group was available from the querystring.</div>";
            }
        }

        #endregion
    }
}