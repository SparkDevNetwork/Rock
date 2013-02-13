using System;
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
    [ContextAware( typeof( Group ) )]
    [DetailPage]
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
            gGroupMembers.Actions.AddClick += gGroupMembers_AddClick;
            gGroupMembers.Actions.IsAddEnabled = true;
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
                Group group = this.ContextEntity<Group>();
                if ( group != null )
                {
                    hfGroupId.Value = group.Id.ToString();
                    BindGroupMembersGrid();
                }
                else
                {
                    pnlGroupMembers.Visible = false;
                }
            }
        }

        #endregion

        #region dimmable

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
                GroupMember groupMember = groupMemberService.Get( (int)e.RowKeyValue );
                if ( groupMember != null )
                {
                    string errorMessage;
                    if ( !groupMemberService.CanDelete( groupMember, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    groupMemberService.Delete( groupMember, CurrentPersonId );
                    groupMemberService.Save( groupMember, CurrentPersonId );
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
            NavigateToDetailPage( "groupMemberId", 0, "groupId", hfGroupId.ValueAsInt() );
        }

        /// <summary>
        /// Handles the Edit event of the gGroupMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroupMembers_Edit( object sender, RowEventArgs e )
        {
            NavigateToDetailPage( "groupMemberId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Binds the group members grid.
        /// </summary>
        protected void BindGroupMembersGrid()
        {
            pnlGroupMembers.Visible = false;

            var group = this.ContextEntity<Group>();

            if ( group == null )
            {
                return;
            }

            int groupId = group.Id;
            hfGroupId.SetValue( group.Id );

            pnlGroupMembers.Visible = true;

            GroupMemberService groupMemberService = new GroupMemberService();

            var qry = groupMemberService.Queryable().Where( a => a.GroupId.Equals( groupId ) );

            SortProperty sortProperty = gGroupMembers.SortProperty;

            if ( sortProperty != null )
            {
                gGroupMembers.DataSource = qry.Sort( sortProperty ).ToList();
            }
            else
            {
                gGroupMembers.DataSource = qry.OrderBy( a => a.Person.LastName ).ThenBy( a => a.Person.NickName ).ThenBy( a => a.Person.GivenName ).ToList();
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
    }
}