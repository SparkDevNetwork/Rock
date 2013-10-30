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
        #region Private Variables

        private Group _group = null;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // if this block has a specific GroupId set, use that, otherwise, determine it from the PageParameters
            int groupId = GetAttributeValue( "Group" ).AsInteger() ?? 0;
            if ( groupId == 0 )
            {
                groupId = PageParameter( "groupId" ).AsInteger() ?? 0;
                if ( groupId != 0 )
                {
                    _group = new GroupService().Get( groupId );

                    rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

                    gGroupMembers.DataKeyNames = new string[] { "Id" };
                    gGroupMembers.CommunicateMergeFields = new List<string> { "GroupRole.Name" };
                    gGroupMembers.PersonIdField = "PersonId";
                    gGroupMembers.Actions.AddClick += gGroupMembers_AddClick;
                    gGroupMembers.Actions.ShowAdd = true;
                    gGroupMembers.IsDeleteEnabled = true;
                    gGroupMembers.GridRebind += gGroupMembers_GridRebind;
                    gGroupMembers.RowItemText = _group.GroupType.GroupTerm + " " + _group.GroupType.GroupMemberTerm;
                }
            }

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
                BindFilter();

                tbFirstName.Text = rFilter.GetUserPreference( "First Name" );
                tbLastName.Text = rFilter.GetUserPreference( "Last Name" );
                cblRole.SetValues( rFilter.GetUserPreference( "Role" ).Split( ';' ).ToList() );
                cblStatus.SetValues( rFilter.GetUserPreference( "Status" ).Split( ';' ).ToList() );

                BindGroupMembersGrid();
            }
        }

        #endregion

        #region GroupMembers Grid

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "First Name", tbFirstName.Text );
            rFilter.SaveUserPreference( "Last Name", tbLastName.Text );
            rFilter.SaveUserPreference( "Role", GetCheckBoxListValues( cblRole ) );
            rFilter.SaveUserPreference( "Status", GetCheckBoxListValues( cblStatus ) );

            BindGroupMembersGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "First Name":
                case "Last Name":
                    break;
                case "Role":
                    e.Value = ResolveValues( e.Value, cblRole );
                    break;
                case "Status":
                    e.Value = ResolveValues( e.Value, cblStatus );
                    break;
                default:
                    e.Value = string.Empty;
                    break;
            }

        }

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
            NavigateToLinkedPage( "DetailPage", "groupMemberId", 0, "groupId", _group.Id );
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

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            if ( _group != null )
            {
                cblRole.DataSource = _group.GroupType.Roles.OrderBy( a => a.Order ).ToList();
                cblRole.DataBind();
            }

            cblStatus.BindToEnum( typeof( GroupMemberStatus ) );
        }

        /// <summary>
        /// Binds the group members grid.
        /// </summary>
        protected void BindGroupMembersGrid()
        {
            if ( _group != null )
            {
                pnlGroupMembers.Visible = true;

                using ( new UnitOfWorkScope() )
                {
                    lHeading.Text = string.Format( "{0} {1}", _group.GroupType.GroupTerm, _group.GroupType.GroupMemberTerm.Pluralize() );

                    if ( _group.GroupType.Roles.Any() )
                    {
                        nbRoleWarning.Visible = false;
                        rFilter.Visible = true;
                        gGroupMembers.Visible = true;

                        // Remove attribute columns
                        foreach ( var column in gGroupMembers.Columns.OfType<AttributeField>().ToList() )
                        {
                            gGroupMembers.Columns.Remove( column );
                        }

                        // Add attribute columns
                        int entityTypeId = new GroupMember().TypeId;
                        string groupQualifier = _group.Id.ToString();
                        string groupTypeQualifier = _group.GroupTypeId.ToString();
                        foreach ( var attribute in new AttributeService().Queryable()
                            .Where( a =>
                                a.EntityTypeId == entityTypeId &&
                                a.IsGridColumn &&
                                ( a.EntityTypeQualifierColumn.Equals( "GroupId", StringComparison.OrdinalIgnoreCase ) && a.EntityTypeQualifierValue.Equals( groupQualifier ) ||
                                 a.EntityTypeQualifierColumn.Equals( "GroupTypeId", StringComparison.OrdinalIgnoreCase ) && a.EntityTypeQualifierValue.Equals( groupTypeQualifier ) )
                                 )
                            .OrderBy( a => a.Order )
                            .ThenBy( a => a.Name ) )
                        {
                            string dataFieldExpression = attribute.Key;
                            bool columnExists = gGroupMembers.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                            if ( !columnExists )
                            {
                                AttributeField boundField = new AttributeField();
                                boundField.DataField = dataFieldExpression;
                                boundField.HeaderText = attribute.Name;
                                boundField.SortExpression = string.Empty;
                                int insertPos = gGroupMembers.Columns.IndexOf( gGroupMembers.Columns.OfType<DeleteField>().First() );
                                gGroupMembers.Columns.Insert( insertPos, boundField );
                            }

                        }

                        GroupMemberService groupMemberService = new GroupMemberService();
                        var qry = groupMemberService.Queryable( "Person,GroupRole" )
                            .Where( m => m.GroupId == _group.Id );

                        // Filter by First Name
                        string firstName = rFilter.GetUserPreference( "First Name" );
                        if ( !string.IsNullOrWhiteSpace( firstName ) )
                        {
                            qry = qry.Where( m => m.Person.FirstName.StartsWith( firstName ) );
                        }

                        // Filter by Last Name
                        string lastName = rFilter.GetUserPreference( "Last Name" );
                        if ( !string.IsNullOrWhiteSpace( lastName ) )
                        {
                            qry = qry.Where( m => m.Person.LastName.StartsWith( lastName ) );
                        }

                        // Filter by role
                        var roles = new List<int>();
                        foreach ( string role in rFilter.GetUserPreference( "Role" ).Split( ';' ) )
                        {
                            if ( !string.IsNullOrWhiteSpace( role ) )
                            {
                                int roleId = int.MinValue;
                                if ( int.TryParse( role, out roleId ) )
                                {
                                    roles.Add( roleId );
                                }
                            }
                        }
                        if ( roles.Any() )
                        {
                            qry = qry.Where( m => roles.Contains( m.GroupRoleId ) );
                        }

                        // Filter by Sttus
                        var statuses = new List<GroupMemberStatus>();
                        foreach ( string status in rFilter.GetUserPreference( "Status" ).Split( ';' ) )
                        {
                            if ( !string.IsNullOrWhiteSpace( status ) )
                            {
                                statuses.Add( status.ConvertToEnum<GroupMemberStatus>() );
                            }
                        }
                        if ( statuses.Any() )
                        {
                            qry = qry.Where( m => statuses.Contains( m.GroupMemberStatus ) );
                        }

                        SortProperty sortProperty = gGroupMembers.SortProperty;

                        if ( sortProperty != null )
                        {
                            gGroupMembers.DataSource = qry.Sort( sortProperty ).ToList();
                        }
                        else
                        {
                            gGroupMembers.DataSource = qry.OrderBy( a => a.Person.LastName ).ThenBy( a => a.Person.FirstName ).ToList();
                        }

                        gGroupMembers.DataBind();
                    }
                    else
                    {
                        nbRoleWarning.Text = string.Format( "{0} cannot be added to this {1} because the '{2}' group type does not have any roles defined.",
                            _group.GroupType.GroupMemberTerm.Pluralize(), _group.GroupType.GroupTerm, _group.GroupType.Name );
                        nbRoleWarning.Visible = true;
                        rFilter.Visible = false;
                        gGroupMembers.Visible = false;
                    }
                }
            }
            else
            {
                pnlGroupMembers.Visible = false;
            }

        }

        /// <summary>
        /// Gets the check box list values by evaluating the posted form values for each input item in the rendered checkbox list.  
        /// This is required because of a bug in ASP.NET that results in the Selected property for CheckBoxList items to not be
        /// set correctly on a postback.
        /// </summary>
        /// <param name="checkBoxList">The check box list.</param>
        /// <returns></returns>
        private string GetCheckBoxListValues( System.Web.UI.WebControls.CheckBoxList checkBoxList )
        {
            var selectedItems = new List<string>();

            for(int i = 0; i < checkBoxList.Items.Count; i++)
            {
                string value = Request.Form[checkBoxList.UniqueID + "$" + i.ToString()];
                if (value != null)
                {
                    checkBoxList.Items[i].Selected = true;
                    selectedItems.Add( value );
                }
                else
                {
                    checkBoxList.Items[i].Selected = false;
                }
            }

            return selectedItems.AsDelimited(";");
        }

        /// <summary>
        /// Resolves the values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="listControl">The list control.</param>
        /// <returns></returns>
        private string ResolveValues( string values, System.Web.UI.WebControls.CheckBoxList checkBoxList )
        {
            var resolvedValues = new List<string>();

            foreach ( string value in values.Split( ';' ) )
            {
                var item = checkBoxList.Items.FindByValue( value );
                if ( item != null )
                {
                    resolvedValues.Add( item.Text );
                }
            }

            return resolvedValues.AsDelimited( ", " );
        }

        #endregion


        #region IDimmableBlock

        /// <summary>
        /// Sets the dimmed.
        /// </summary>
        /// <param name="dimmed">if set to <c>true</c> [dimmed].</param>
        public void SetDimmed( bool dimmed )
        {
            pnlContent.Visible = !dimmed;
        }

        #endregion
    }
}