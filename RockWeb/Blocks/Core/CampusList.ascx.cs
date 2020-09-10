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
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Campus List" )]
    [Category( "Core" )]
    [Description( "Displays a list of all campuses." )]

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage )]

    public partial class Campuses : RockBlock, ICustomGridColumns
    {
        public static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        #region fields

        /// <summary>
        /// Gets or sets the available attributes.
        /// </summary>
        /// <value>
        /// The available attributes.
        /// </value>
        public List<AttributeCache> AvailableAttributes { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gCampuses.DataKeyNames = new string[] { "Id" };
            gCampuses.Actions.AddClick += gCampuses_Add;
            gCampuses.GridRebind += gCampuses_GridRebind;
            gCampuses.GridReorder += gCampuses_GridReorder;
            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gCampuses.Actions.ShowAdd = canAddEditDelete;
            gCampuses.IsDeleteEnabled = canAddEditDelete;

            BindAttributes();
            AddDynamicControls();
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

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            AvailableAttributes = ViewState["AvailableAttributes"] as List<AttributeCache>;
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["AvailableAttributes"] = AvailableAttributes;

            return base.SaveViewState();
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gCampuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gCampuses_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "CampusId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gCampuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gCampuses_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "CampusId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gCampuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gCampuses_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            CampusService campusService = new CampusService( rockContext );
            Campus campus = campusService.Get( e.RowKeyId );
            if ( campus != null )
            {
                // Don't allow deleting the last campus
                if ( !campusService.Queryable().Where( c => c.Id != campus.Id ).Any() )
                {
                    mdGridWarning.Show( campus.Name + " is the only campus and cannot be deleted (Rock requires at least one campus).", ModalAlertType.Information );
                    return;
                }

                string errorMessage;
                if ( !campusService.CanDelete( campus, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                campusService.Delete( campus );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gCampuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gCampuses_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }


        /// <summary>
        /// Handles the GridReorder event of the gCampuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gCampuses_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var campuses = GetCampuses( rockContext ).ToList();
            if ( campuses != null )
            {
                new CampusService( rockContext ).Reorder( campuses, e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the attributes.
        /// </summary>
        private void BindAttributes()
        {
            // Parse the attribute filters 
            AvailableAttributes = new List<AttributeCache>();

            int entityTypeId = new Campus().TypeId;
            foreach ( var attributeModel in new AttributeService( new RockContext() ).Queryable()
                .Where( a =>
                    a.EntityTypeId == entityTypeId &&
                    a.IsGridColumn )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name ) )
            {
                AvailableAttributes.Add( AttributeCache.Get( attributeModel ) );
            }
        }

        /// <summary>
        /// Adds the attribute columns.
        /// </summary>
        private void AddDynamicControls()
        {
            // Remove attribute columns
            foreach ( var column in gCampuses.Columns.OfType<AttributeField>().ToList() )
            {
                gCampuses.Columns.Remove( column );
            }

            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    bool columnExists = gCampuses.Columns.OfType<AttributeField>().FirstOrDefault( a => a.AttributeId == attribute.Id ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = attribute.Key;
                        boundField.AttributeId = attribute.Id;
                        boundField.HeaderText = attribute.Name;

                        var attributeCache = Rock.Web.Cache.AttributeCache.Get( attribute.Id );
                        if ( attributeCache != null )
                        {
                            boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                        }

                        gCampuses.Columns.Add( boundField );
                    }
                }
            }

            // Add delete column
            var deleteField = new DeleteField();
            gCampuses.Columns.Add( deleteField );
            deleteField.Click += gCampuses_Delete;
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            gCampuses.DataSource = GetCampuses().ToList();
            gCampuses.DataBind();
        }

        private IQueryable<Campus> GetCampuses( RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            CampusService campusService = new CampusService( rockContext );
            return campusService.Queryable().OrderBy( s => s.Order );
        }

        #endregion
    }
}