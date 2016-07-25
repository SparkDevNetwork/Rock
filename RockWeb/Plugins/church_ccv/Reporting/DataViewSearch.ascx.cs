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

namespace RockWeb.Plugins.church_ccv.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "DataView Search" )]
    [Category( "CCV > Reporting" )]
    [Description( "Shows the results of a DataView search" )]

    [LinkedPage( "DataView Page" )]
    public partial class DataViewSearch : RockBlock
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

            if ( !Page.IsPostBack )
            {
                BindGrid();
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
            DataViewService dataViewService = new DataViewService( rockContext );

            string dataViewName = PageParameter( "DataViewName" );

            var qry = dataViewService.Queryable().Where( a => a.Name.Contains( dataViewName ) );

            if ( qry.Count() == 1 )
            {
                var dataView = qry.First();
                var qryParams = new Dictionary<string, string>();
                qryParams.Add( "DataViewId", dataView.Id.ToString() );
                qryParams.Add( "ExpandedIds", CategoryExpandedIds( CategoryCache.Read( dataView.CategoryId ?? 0 ) ));
                NavigateToLinkedPage( "DataViewPage", qryParams );
            }
            else
            {
                gList.DataSource = qry.ToList();
                gList.DataBind();
            }
        }

        #endregion

        /// <summary>
        /// Categories the name of the ancestor path.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        private string CategoryAncestorPathName( CategoryCache category )
        {
            var path = category.Name;
            var parentCategory = category.ParentCategory;
            while ( parentCategory != null )
            {
                path = parentCategory.Name + " > " + path;
                parentCategory = parentCategory.ParentCategory;
            }

            return path;
        }

        /// <summary>
        /// Categories the expanded ids.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        private string CategoryExpandedIds( CategoryCache category )
        {
            var path = "C" + category.Id.ToString();
            var parentCategory = category.ParentCategory;
            while ( parentCategory != null )
            {
                path = "C" + parentCategory.Id.ToString() + "," + path;
                parentCategory = parentCategory.ParentCategory;
            }

            return path;
        }

        /// <summary>
        /// Handles the RowDataBound event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            DataView dataView = e.Row.DataItem as DataView;
            var lDataViewPath = e.Row.ControlsOfTypeRecursive<Literal>().FirstOrDefault( a => a.ID == "lDataViewPath" );
            if ( dataView != null && lDataViewPath != null && dataView.CategoryId.HasValue )
            {

                var category = CategoryCache.Read( dataView.CategoryId.Value );
                lDataViewPath.Text = CategoryAncestorPathName( category );
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_RowSelected( object sender, RowEventArgs e )
        {
            RockContext rockContext = new RockContext();
            DataViewService dataViewService = new DataViewService( rockContext );
            var dataView = dataViewService.Get( e.RowKeyId );

            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "DataViewId", dataView.Id.ToString() );
            qryParams.Add( "ExpandedIds", CategoryExpandedIds( CategoryCache.Read( dataView.CategoryId ?? 0 ) ) );
            NavigateToLinkedPage( "DataViewPage", qryParams );
        }
    }
}