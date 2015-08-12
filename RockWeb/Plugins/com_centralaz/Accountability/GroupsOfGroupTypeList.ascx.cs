using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.Accountability
{
    [DisplayName( "Groups of Group Type List" )]
    [Category( "com_centralaz > Accountability" )]
    [Description( "Lists all groups for the configured group types." )]

    [LinkedPage( "Detail Page", "", true, "", "", 0 )]
    [BooleanField( "Limit to Security Role Groups", "Any groups can be flagged as a security group (even if they're not a security role).  Should the list of groups be limited to these groups?", false, "", 2 )]
    [BooleanField( "Display Description Column", "Should the Description column be displayed?", true, "", 5 )]
    [BooleanField( "Display Active Status Column", "Should the Active Status column be displayed?", false, "", 6 )]
    [BooleanField( "Display System Column", "Should the System column be displayed?", true, "", 6 )]
    [BooleanField( "Display Filter", "Should filter be displayed to allow filtering by group type?", false, "", 7 )]
    [ContextAware]
    public partial class GroupsOfGroupTypeList : Rock.Web.UI.RockBlock, Rock.Web.UI.ISecondaryBlock
    {
        public int _groupTypesCount = 0;

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gfSettings.Visible = ( GetAttributeValue( "DisplayFilter" ) ?? "false" ).AsBoolean();
            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;

            gGroups.DataKeyNames = new string[] { "id" };
            gGroups.Actions.ShowAdd = true;
            gGroups.Actions.AddClick += gGroups_Add;
            gGroups.GridRebind += gGroups_GridRebind;

            BindFilter();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            if ( ddlActiveFilter.SelectedValue == "all" )
            {
                gfSettings.SaveUserPreference( "Active Status", string.Empty );
            }
            else
            {
                gfSettings.SaveUserPreference( "Active Status", ddlActiveFilter.SelectedValue );
            }

            BindGrid();
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
                case "Group Type":

                    int id = e.Value.AsInteger();

                    var groupType = GroupTypeCache.Read( id );
                    if ( groupType != null )
                    {
                        e.Value = groupType.Name;
                    }

                    break;
            }
        }

        /// <summary>
        /// Handles the Add event of the gGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroups_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "GroupId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroups_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "GroupId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroups_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            GroupService groupService = new GroupService( rockContext );
            AuthService authService = new AuthService( rockContext );
            Group group = groupService.Get( e.RowKeyId );

            if ( group != null )
            {
                if ( !group.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                {
                    mdGridWarning.Show( "You are not authorized to delete this group", ModalAlertType.Information );
                    return;
                }

                string errorMessage;
                if ( !groupService.CanDelete( group, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                bool isSecurityRoleGroup = group.IsSecurityRole || group.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() );
                if ( isSecurityRoleGroup )
                {
                    Rock.Security.Role.Flush( group.Id );
                    foreach ( var auth in authService.Queryable().Where( a => a.GroupId == group.Id ).ToList() )
                    {
                        authService.Delete( auth );
                    }
                }

                groupService.Delete( group );

                rockContext.SaveChanges();

                if ( isSecurityRoleGroup )
                {
                    Rock.Security.Authorization.Flush();
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gGroups_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {

            // Set the Active Status
            var itemActiveStatus = ddlActiveFilter.Items.FindByValue( gfSettings.GetUserPreference( "Active Status" ) );
            if ( itemActiveStatus != null )
            {
                itemActiveStatus.Selected = true;
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            // Find all the Group Types
            int? groupTypeId = PageParameter( "groupTypeId" ).AsIntegerOrNull();

            var rockContext = new RockContext();

            SortProperty sortProperty = gGroups.SortProperty;
            if ( sortProperty == null )
            {
                sortProperty = new SortProperty( new GridViewSortEventArgs( "GroupTypeOrder, GroupTypeName, GroupOrder, Name", SortDirection.Ascending ) );
            }

            bool onlySecurityGroups = GetAttributeValue( "LimittoSecurityRoleGroups" ).AsBoolean();
            bool showDescriptionColumn = GetAttributeValue( "DisplayDescriptionColumn" ).AsBoolean();
            bool showActiveStatusColumn = GetAttributeValue( "DisplayActiveStatusColumn" ).AsBoolean();
            bool showSystemColumn = GetAttributeValue( "DisplaySystemColumn" ).AsBoolean();

            if ( !showDescriptionColumn )
            {
                gGroups.TooltipField = "Description";
            }

            Dictionary<string, BoundField> boundFields = gGroups.Columns.OfType<BoundField>().ToDictionary( a => a.DataField );
            boundFields["Description"].Visible = showDescriptionColumn;

            Dictionary<string, BoolField> boolFields = gGroups.Columns.OfType<BoolField>().ToDictionary( a => a.DataField );
            boolFields["IsActive"].Visible = showActiveStatusColumn;
            boolFields["IsSystem"].Visible = showSystemColumn;

            // Person context will exist if used on a person detail page
            int personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;
            if ( ContextTypesRequired.Any( e => e.Id == personEntityTypeId ) )
            {
                var personContext = ContextEntity<Person>();
                if ( personContext != null )
                {
                    boundFields["GroupRole"].Visible = true;
                    boundFields["DateAdded"].Visible = true;
                    boundFields["MemberCount"].Visible = false;

                    gGroups.Actions.ShowAdd = false;
                    gGroups.IsDeleteEnabled = false;
                    gGroups.Columns.OfType<DeleteField>().ToList().ForEach( f => f.Visible = false );

                    var qry = new GroupMemberService( rockContext ).Queryable( true )
                        .Where( m =>
                            m.PersonId == personContext.Id &&
                            ( groupTypeId == m.Group.GroupTypeId ) &&
                            ( !onlySecurityGroups || m.Group.IsSecurityRole ) );

                    // Filter by active/inactive
                    if ( ddlActiveFilter.SelectedIndex > -1 )
                    {
                        if ( ddlActiveFilter.SelectedValue == "inactive" )
                        {
                            qry = qry.Where( a => a.Group.IsActive == false );
                        }
                        else if ( ddlActiveFilter.SelectedValue == "active" )
                        {
                            qry = qry.Where( a => a.Group.IsActive == true );
                        }
                    }

                    gGroups.DataSource = qry
                        .Select( m => new
                        {
                            Id = m.Group.Id,
                            Name = m.Group.Name,
                            GroupOrder = m.Group.Order,
                            GroupTypeOrder = m.Group.GroupType.Order,
                            Description = m.Group.Description,
                            IsSystem = m.Group.IsSystem,
                            GroupRole = m.GroupRole.Name,
                            DateAdded = m.CreatedDateTime,
                            IsActive = m.Group.IsActive,
                            MemberCount = 0
                        } )
                        .Sort( sortProperty )
                        .ToList();
                }
            }
            else
            {
                bool canEdit = IsUserAuthorized( Authorization.EDIT );
                gGroups.Actions.ShowAdd = canEdit;
                gGroups.IsDeleteEnabled = canEdit;

                boundFields["GroupRole"].Visible = false;
                boundFields["DateAdded"].Visible = false;
                boundFields["MemberCount"].Visible = true;

                var qry = new GroupService( rockContext ).Queryable()
                    .Where( g =>
                        ( groupTypeId == g.GroupTypeId ) &&
                        ( !onlySecurityGroups || g.IsSecurityRole ) );

                // Filter by active/inactive
                if ( ddlActiveFilter.SelectedIndex > -1 )
                {
                    if ( ddlActiveFilter.SelectedValue == "inactive" )
                    {
                        qry = qry.Where( a => a.IsActive == false );
                    }
                    else if ( ddlActiveFilter.SelectedValue == "active" )
                    {
                        qry = qry.Where( a => a.IsActive == true );
                    }
                }

                gGroups.DataSource = qry.Select( g => new
                {
                    Id = g.Id,
                    Name = g.Name,
                    GroupTypeName = g.GroupType.Name,
                    GroupOrder = g.Order,
                    GroupTypeOrder = g.GroupType.Order,
                    Description = g.Description,
                    IsSystem = g.IsSystem,
                    IsActive = g.IsActive,
                    GroupRole = string.Empty,
                    DateAdded = DateTime.MinValue,
                    MemberCount = g.Members.Count()
                } )
                    .Sort( sortProperty )
                    .ToList();
            }

            gGroups.DataBind();
        }

        #endregion

        /// <summary>
        /// Determines the visibility of the block.
        /// </summary>
        /// <param name="visible">The boolean that determines whether the block is visible or not</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }
    }
}