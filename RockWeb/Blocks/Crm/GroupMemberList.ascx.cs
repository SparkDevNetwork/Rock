//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// 
    /// </summary>
    [GroupField( "Group", "Either pick a specific group or choose <none> to have group be determined by the groupId page parameter" )]
    [LinkedPage("Detail Page")]
    public partial class GroupMemberList : RockBlock, IDimmableBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gGroupMembers.DataKeyNames = new string[] { "Id" };
            gGroupMembers.CommunicateMergeFields = new List<string> { "GroupRole.Name" };
            gGroupMembers.PersonIdField = "PersonId";
            gGroupMembers.Actions.AddClick += gGroupMembers_AddClick;
            gGroupMembers.Actions.ShowAdd = true;
            gGroupMembers.IsDeleteEnabled = true;
            gGroupMembers.GridRebind += gGroupMembers_GridRebind;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindGroupMembersGrid();
            }
        }

        #endregion

        #region GroupMembers Grid

        /// <summary>
        /// Handles the Click event of the DeleteGroupMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs" /> instance containing the event data.</param>
        protected void DeleteGroupMember_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
            {
                GroupMemberService groupMemberService = new GroupMemberService();
                GroupMember groupMember = groupMemberService.Get( e.RowKeyId );
                if ( groupMember != null )
                {
                    string errorMessage;
                    if ( !groupMemberService.CanDelete( groupMember, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    int groupId = groupMember.GroupId;

                    groupMemberService.Delete( groupMember, CurrentPersonId );
                    groupMemberService.Save( groupMember, CurrentPersonId );

                    Group group = new GroupService().Get( groupId );
                    if ( group.IsSecurityRole )
                    {
                        // person removed from SecurityRole, Flush
                        Rock.Security.Authorization.Flush();
                    }
                }
            } );

            BindGroupMembersGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gGroupMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gGroupMembers_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "groupMemberId", 0, "groupId", hfGroupId.ValueAsInt() );
        }

        /// <summary>
        /// Handles the Edit event of the gGroupMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroupMembers_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "groupMemberId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Binds the group members grid.
        /// </summary>
        protected void BindGroupMembersGrid()
        {
            pnlGroupMembers.Visible = false;

            // if this block has a specific GroupId set, use that, otherwise, determine it from the PageParameters
            int groupId = GetAttributeValue( "Group" ).AsInteger() ?? 0;

            if ( groupId == 0 )
            {
                groupId = PageParameter( "groupId" ).AsInteger() ?? 0;

                if ( groupId == 0 )
                {
                    // quit if the groupId can't be determined
                    return;
                }
            }

            hfGroupId.SetValue( groupId );

            pnlGroupMembers.Visible = true;

            GroupMemberService groupMemberService = new GroupMemberService();

            var qry = groupMemberService.Queryable().Where( a => a.GroupId.Equals( groupId ) ).Select( a =>
                new
                {
                    a.Id,
                    PersonId = a.PersonId,
                    PersonFirstName = a.Person.FirstName,
                    PersonLastName = a.Person.LastName,
                    PersonFullNameLastFirst = a.Person.FullNameLastFirst,
                    GroupRoleName = a.GroupRole.Name,
                    a.GroupMemberStatus
                } ).AsQueryable();

            SortProperty sortProperty = gGroupMembers.SortProperty;

            if ( sortProperty != null )
            {
                gGroupMembers.DataSource = qry.Sort( sortProperty ).ToList();
            }
            else
            {
                gGroupMembers.DataSource = qry.OrderBy( a => a.PersonLastName ).ThenBy( a => a.PersonFirstName ).ToList();
            }

            gGroupMembers.DataBind();
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gGroupMembers_GridRebind( object sender, EventArgs e )
        {
            BindGroupMembersGrid();
        }

        #endregion

        #region IDimmableBlock

        /// <summary>
        /// Sets the dimmed.
        /// </summary>
        /// <param name="dimmed">if set to <c>true</c> [dimmed].</param>
        public void SetDimmed( bool dimmed )
        {
            pnlGroupMembers.Disabled = dimmed;
            gGroupMembers.Enabled = !dimmed;
        }

        #endregion
    }
}