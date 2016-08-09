using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;

namespace RockWeb.Plugins.church_ccv.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Attendance Spreadsheet Tool" )]
    [Category( "CCV > Reporting" )]
    [Description( "Helps create the weekly CCV Attendance Spreadsheet" )]
    [TextField( "AttendanceTypes", Category = "CustomSetting")]
    public partial class AttendanceSpreadSheetTool : RockBlockCustomSettings
    {
        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gList.GridRebind += gList_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            BuildGroupTypesUI();

            if ( !Page.IsPostBack )
            {
                LoadDropDowns();
                
                BindGrid();
            }
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        public void LoadDropDowns()
        {
            var rockContext = new RockContext();

            clbCampuses.Items.Clear();
            var noCampusListItem = new ListItem();
            noCampusListItem.Text = "<span title='Include records that are not associated with a campus'>No Campus</span>";
            noCampusListItem.Value = "null";
            clbCampuses.Items.Add( noCampusListItem );
            foreach ( var campus in CampusCache.All().OrderBy( a => a.Name ) )
            {
                var listItem = new ListItem();
                listItem.Text = campus.Name;
                listItem.Value = campus.Id.ToString();
                clbCampuses.Items.Add( listItem );
            }
        }

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            // TODO
        }

        /// <summary>
        /// Builds the group types UI
        /// </summary>
        private void BuildGroupTypesUI()
        {
            var groupTypes = this.GetSelectedGroupTypes();
            if ( groupTypes.Any() )
            {
                nbGroupTypeWarning.Visible = false;

                // only add each grouptype/group once in case they are a child of multiple parents
                _addedGroupTypeIds = new List<int>();
                _addedGroupIds = new List<int>();
                rptGroupTypes.DataSource = groupTypes.ToList();
                rptGroupTypes.DataBind();
            }
            else
            {
                nbGroupTypeWarning.Text = "Please select a check-in type.";
                nbGroupTypeWarning.Visible = true;
            }
        }

        /// <summary>
        /// Gets the type of the selected template group (Check-In Type)
        /// </summary>
        /// <returns></returns>
        private List<GroupType> GetSelectedGroupTypes()
        {
            var rockContext = new RockContext();
            var result = new List<GroupType>();
            var attendanceTypes = this.GetAttributeValue( "AttendanceTypes" ).SplitDelimitedValues().AsIntegerList();

            foreach ( var groupTypeId in attendanceTypes )
            {
                var groupTypes = new GroupTypeService( rockContext )
                        .GetChildGroupTypes( groupTypeId )
                        .OrderBy( a => a.Order )
                        .ThenBy( a => a.Name )
                        .ToList();

                result.AddRange( groupTypes );
            }

            return result;
        }

        // list of grouptype ids that have already been rendered (in case a group type has multiple parents )
        private List<int> _addedGroupTypeIds;

        private List<int> _addedGroupIds;

        /// <summary>
        /// Adds the group type controls.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="pnlGroupTypes">The PNL group types.</param>
        private void AddGroupTypeControls( GroupType groupType, HtmlGenericContainer liGroupTypeItem, RockContext rockContext )
        {
            if ( !_addedGroupTypeIds.Contains( groupType.Id ) )
            {
                _addedGroupTypeIds.Add( groupType.Id );

                if ( groupType.Groups.Any() )
                {
                    bool showGroupAncestry = GetAttributeValue( "ShowGroupAncestry" ).AsBoolean( true );

                    var groupService = new GroupService( rockContext );

                    var cblGroupTypeGroups = new RockCheckBoxList { ID = "cblGroupTypeGroups" + groupType.Id };

                    cblGroupTypeGroups.Label = groupType.Name;
                    cblGroupTypeGroups.Items.Clear();

                    // limit to Groups that don't have a Parent, or the ParentGroup is a different grouptype so we don't end up with infinite recursion
                    foreach ( var group in groupType.Groups
                        .Where( g => !g.ParentGroupId.HasValue || ( g.ParentGroup.GroupTypeId != groupType.Id ) )
                        .OrderBy( a => a.Order )
                        .ThenBy( a => a.Name )
                        .ToList() )
                    {
                        AddGroupControls( group, cblGroupTypeGroups, groupService, showGroupAncestry );
                    }

                    liGroupTypeItem.Controls.Add( cblGroupTypeGroups );
                }
                else
                {
                    if ( groupType.ChildGroupTypes.Any() )
                    {
                        liGroupTypeItem.Controls.Add( new Label { Text = groupType.Name, ID = "lbl" + groupType.Name } );
                    }
                }

                if ( groupType.ChildGroupTypes.Any() )
                {
                    var ulGroupTypeList = new HtmlGenericContainer( "ul", "list-unstyled" );

                    liGroupTypeItem.Controls.Add( ulGroupTypeList );
                    foreach ( var childGroupType in groupType.ChildGroupTypes.OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
                    {
                        var liChildGroupTypeItem = new HtmlGenericContainer( "li" );
                        liChildGroupTypeItem.ID = "liGroupTypeItem" + childGroupType.Id;
                        ulGroupTypeList.Controls.Add( liChildGroupTypeItem );
                        AddGroupTypeControls( childGroupType, liChildGroupTypeItem, rockContext );
                    }
                }
            }
        }

        protected void rptGroupTypes_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var groupType = e.Item.DataItem as GroupType;

                var liGroupTypeItem = new HtmlGenericContainer( "li", "rocktree-item rocktree-folder" );
                liGroupTypeItem.ID = "liGroupTypeItem" + groupType.Id;
                e.Item.Controls.Add( liGroupTypeItem );

                var rockContext = new RockContext();
                AddGroupTypeControls( groupType, liGroupTypeItem, rockContext );
            }
        }

        /// <summary>
        /// Adds the group controls.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="checkBoxList">The check box list.</param>
        /// <param name="service">The service.</param>
        /// <param name="showGroupAncestry">if set to <c>true</c> [show group ancestry].</param>
        private void AddGroupControls( Group group, RockCheckBoxList checkBoxList, GroupService service, bool showGroupAncestry )
        {
            // Only show groups that actually have a schedule
            if ( group != null )
            {
                if ( !_addedGroupIds.Contains( group.Id ) )
                {
                    _addedGroupIds.Add( group.Id );
                    if ( group.ScheduleId.HasValue || group.GroupLocations.Any( l => l.Schedules.Any() ) )
                    {
                        string displayName = showGroupAncestry ? service.GroupAncestorPathName( group.Id ) : group.Name;
                        checkBoxList.Items.Add( new ListItem( displayName, group.Id.ToString() ) );
                    }

                    if ( group.Groups != null )
                    {
                        foreach ( var childGroup in group.Groups
                            .OrderBy( a => a.Order )
                            .ThenBy( a => a.Name )
                            .ToList() )
                        {
                            AddGroupControls( childGroup, checkBoxList, service, showGroupAncestry );
                        }
                    }
                }
            }
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
            //
        }

        /// <summary>
        /// Handles the GridRebind event of the gPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            RockContext rockContext = new RockContext();
            //
        }

        #endregion

        protected void gList_RowDataBound( object sender, GridViewRowEventArgs e )
        {

        }

        protected void btnUpdate_Click( object sender, EventArgs e )
        {

        }

        protected void cblAttendanceTypes_SelectedIndexChanged( object sender, EventArgs e )
        {
            BuildGroupTypesUI();
        }
    }
}