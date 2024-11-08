// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;
using System.Data.Entity;

namespace RockWeb.Blocks.Groups
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Group Archived List" )]
    [Category( "Utility" )]
    [Description( "Lists Groups that have been archived" )]
    [Rock.SystemGuid.BlockTypeGuid( "AD5B3A8A-2111-4FC4-A026-51EEB4929CBA" )]
    public partial class GroupArchivedList : RockBlock, ICustomGridColumns
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

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
            gList.DataKeyNames = new string[] { "Id" };
            gList.GridRebind += gList_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

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

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        /// <summary>
        /// Handles the GridRebind event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnUnarchive control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void btnUnarchive_Click( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                GroupService groupService = new GroupService( rockContext );
                var archivedGroup = groupService.Get( e.RowKeyId );
                archivedGroup.IsArchived = false;
                archivedGroup.ArchivedByPersonAliasId = null;
                archivedGroup.ArchivedDateTime = null;
                rockContext.SaveChanges();
                BindGrid();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            RockContext rockContext = new RockContext();
            GroupService groupService = new GroupService( rockContext );

            // sample query to display a few people
            var qry = groupService.GetArchived();

            int? groupTypeIdFilter = gfList.GetFilterPreference( "Group Type" ).AsIntegerOrNull();
            if ( groupTypeIdFilter.HasValue )
            {
                qry = qry.Where( a => a.GroupTypeId == groupTypeIdFilter.Value );
            }

            string groupName = gfList.GetFilterPreference( "Group Name" );
            if (!string.IsNullOrWhiteSpace( groupName ) )
            {
                qry = qry.Where( a => a.Name.Contains( groupName ) );
            }

            SortProperty sortProperty = gList.SortProperty ?? new SortProperty { Direction = SortDirection.Descending, Property = "ArchivedDateTime" };

            qry = qry.Sort( sortProperty );

            gList.SetLinqDataSource( qry );
            gList.DataBind();
        }

        #endregion

        #region Grid

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            // limit GroupType picker to group types that have groups that are archived
            gtpGroupTypeFilter.GroupTypes = new GroupTypeService( new RockContext() ).AsNoFilter().Where(a => a.Groups.Any(x => x.IsArchived)).OrderBy( a => a.Name).AsNoTracking().ToList();

            gtpGroupTypeFilter.SetValue( gfList.GetFilterPreference( "Group Type" ).AsIntegerOrNull() );
            tbNameFilter.Text = gfList.GetFilterPreference( "Group Name" );
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfList_ClearFilterClick( object sender, EventArgs e )
        {
            gfList.DeleteFilterPreferences();
            BindFilter();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfList_ApplyFilterClick( object sender, EventArgs e )
        {
            gfList.SetFilterPreference( "Group Type", gtpGroupTypeFilter.SelectedValue );
            gfList.SetFilterPreference( "Group Name", tbNameFilter.Text );
            BindGrid();
        }

        /// <summary>
        /// Gfs the list display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfList_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Group Type":

                    int? groupTypeId = e.Value.AsIntegerOrNull();
                    if ( groupTypeId.HasValue )
                    {
                        var groupType = GroupTypeCache.Get( groupTypeId.Value );
                        if ( groupType != null )
                        {
                            e.Value = groupType.Name;
                        }
                    }

                    break;
            }
        }

        #endregion
    }
}