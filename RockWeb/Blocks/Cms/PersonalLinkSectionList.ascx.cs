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

using System;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.ComponentModel;
using Rock.Security;
using Rock.Web.Cache;
using System.Collections.Generic;
using System.Data.Entity;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Personal Link Section List" )]
    [Category( "CMS" )]
    [Description( "Lists personal link section in the system." )]

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    [BooleanField(
        "Shared Section",
        Description = "When enabled, only shared sections will be displayed.",
        Key = AttributeKey.SharedSection,
        Order = 1 )]

    #endregion Block Attributes
    public partial class PersonalLinkSectionList : RockBlock, ICustomGridColumns
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string SharedSection = "SharedSection";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string SectionId = "SectionId";
        }

        #endregion PageParameterKey

        #region UserPreferenceKeys

        /// <summary>
        /// Keys to use for UserPreferences
        /// </summary>
        protected static class UserPreferenceKey
        {
            /// <summary>
            /// The Name
            /// </summary>
            public const string Name = "Name";
        }

        #endregion UserPreferanceKeys

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;

            gfFilter.ApplyFilterClick += gfFilter_ApplyFilterClick;
            gfFilter.ClearFilterClick += gfFilter_ClearFilterClick;
            gfFilter.DisplayFilterValue += gfFilter_DisplayFilterValue;

            gSectionList.DataKeyNames = new string[] { "Id" };
            gSectionList.Actions.ShowAdd = true;
            gSectionList.IsDeleteEnabled = true;
            gSectionList.RowSelected += gSectionList_Edit;
            gSectionList.Actions.AddClick += gSectionList_Add;
            gSectionList.GridReorder += gSectionList_GridReorder;
            gSectionList.GridRebind += gSectionList_GridRebind;
            gSectionList.RowDataBound += gSectionList_RowDataBound;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                if ( CurrentPersonAliasId.HasValue )
                {
                    BindGrid();
                }
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events 

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gSectionList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSectionList_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.SectionId, 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gSectionList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSectionList_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.SectionId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gSectionList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSectionList_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var personalLinkSectionService = new PersonalLinkSectionService( rockContext );
            var personalLinkSection = personalLinkSectionService.Get( e.RowKeyId );

            if ( personalLinkSection != null )
            {
                string errorMessage;
                if ( !personalLinkSectionService.CanDelete( personalLinkSection, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                personalLinkSectionService.Delete( personalLinkSection );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gSectionList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gSectionList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var personalLinkSection = ( PersonalLinkSectionViewModel ) e.Row.DataItem;
            if ( personalLinkSection == null )
            {
                return;
            }
        
            // disable delete button
            var deleteField = gSectionList.ColumnsOfType<DeleteField>().FirstOrDefault();
            var deleteFieldIndex = gSectionList.Columns.IndexOf( deleteField );
            var deleteButtonCell = ( ( DataControlFieldCell ) e.Row.Cells[deleteFieldIndex] ).Controls[0];
            deleteButtonCell.Visible = personalLinkSection.CanEdit;
        }

        /// <summary>
        /// Handles the GridRebind event of the gSectionList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gSectionList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the gSectionList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gSectionList_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var personalLinkSections = GetPersonalLinkSections( rockContext );
            var personalLinkSectionOrders = GetPersonalLinkSectionOrders( rockContext );
            var personalLinkSectionViewModels = GetOrderedPersonalLinkSection( personalLinkSections, personalLinkSectionOrders );

            // first get all the section for which there is no corresponding order entry.
            var sectionWithNoOrder = personalLinkSectionViewModels
                .Where( a => !a.OrderId.HasValue );

            if ( sectionWithNoOrder.Any() )
            {
                var newPersonalLinkSectionOrder = sectionWithNoOrder
                    .Select( a => new PersonalLinkSectionOrder
                    {
                        SectionId = a.Id,
                        Order = 0,
                        PersonAliasId = CurrentPersonAliasId.Value
                    } ).ToList();
                new PersonalLinkSectionOrderService( rockContext ).AddRange( newPersonalLinkSectionOrder );
                rockContext.SaveChanges();
                personalLinkSectionOrders = GetPersonalLinkSectionOrders( rockContext );
            }

            var orderedSectionIds = GetOrderedPersonalLinkSection( personalLinkSections, personalLinkSectionOrders ).Select( a => a.OrderId ).ToList();
            personalLinkSectionOrders = personalLinkSectionOrders.OrderBy( a => orderedSectionIds.IndexOf( a.Id ) ).ToList();

            new PersonalLinkSectionOrderService( rockContext ).Reorder( personalLinkSectionOrders, e.OldIndex, e.NewIndex );
            rockContext.SaveChanges();

            BindGrid();
        }

        /// <summary>
        /// Displays the text of the gfFilter control
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfFilter.SaveUserPreference( UserPreferenceKey.Name, txtSectionName.Text );
            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfFilter_ClearFilterClick( object sender, EventArgs e )
        {
            gfFilter.DeleteUserPreferences();
            BindFilter();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            txtSectionName.Text = gfFilter.GetUserPreference( UserPreferenceKey.Name );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var personalLinkSections = GetPersonalLinkSections( rockContext );
            var personalLinkSectionOrders = GetPersonalLinkSectionOrders( rockContext );
            gSectionList.EntityTypeId = EntityTypeCache.GetId<PersonalLinkSection>();
            gSectionList.DataSource = GetOrderedPersonalLinkSection( personalLinkSections, personalLinkSectionOrders );
            gSectionList.DataBind();
        }

        private IEnumerable<PersonalLinkSectionViewModel> GetOrderedPersonalLinkSection( List<PersonalLinkSection> personalLinkSections, List<PersonalLinkSectionOrder> personalLinkSectionOrders )
        {
            return personalLinkSections
                 .Select( a => new PersonalLinkSectionViewModel
                 {
                     Id = a.Id,
                     Name = a.Name,
                     LinkCount = a.PersonalLinks.Count(),
                     IsShared = a.IsShared,
                     Order = personalLinkSectionOrders.Where( b => b.SectionId == a.Id ).Select( b => b.Order ).DefaultIfEmpty().FirstOrDefault(),
                     OrderId = personalLinkSectionOrders.Where( b => b.SectionId == a.Id ).Select( b => (int?) b.Id ).FirstOrDefault(),
                     CanEdit = !a.IsShared || ( a.IsShared && a.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                 } )
                 .OrderBy( a => a.Order )
                 .ThenBy( a => a.Name )
                 .ToList();
        }

        /// <summary>
        /// Gets the personal link sections.
        /// </summary>
        /// <returns></returns>
        private List<PersonalLinkSection> GetPersonalLinkSections( RockContext rockContext )
        {
            rockContext = rockContext ?? new RockContext();
            var personalLinkSections = new List<PersonalLinkSection>();
            var isShared = GetAttributeValue( AttributeKey.SharedSection ).AsBoolean();

            var qry = new PersonalLinkSectionService( rockContext )
                .Queryable()
                .Include( a => a.PersonalLinks )
                .AsNoTracking();

            if ( isShared )
            {
                qry = qry.Where( a => a.IsShared );
            }
            else
            {
                qry = qry.Where( a => a.IsShared || a.PersonAliasId == CurrentPersonAliasId.Value );
            }

            // Filter by: Name
            var name = gfFilter.GetUserPreference( UserPreferenceKey.Name ).ToStringSafe();

            if ( !string.IsNullOrWhiteSpace( name ) )
            {
                qry = qry.Where( a => a.Name.Contains( name ) );
            }

            foreach ( var personalLinkSection in qry.ToList() )
            {
                var isViewable = !personalLinkSection.IsShared || ( personalLinkSection.IsShared && personalLinkSection.IsAuthorized( Authorization.VIEW, CurrentPerson ) );
                if ( isViewable )
                {
                    personalLinkSections.Add( personalLinkSection );
                }
            }

            return personalLinkSections;
        }

        /// <summary>
        /// Gets the personal link section orders.
        /// </summary>
        /// <returns></returns>
        private List<PersonalLinkSectionOrder> GetPersonalLinkSectionOrders( RockContext rockContext )
        {
            rockContext = rockContext ?? new RockContext();

            var qry = new PersonalLinkSectionOrderService( rockContext )
                .Queryable();

            qry = qry.Where( a => a.PersonAliasId == CurrentPersonAliasId );

            return qry.ToList();
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// 
        /// </summary>
        public class PersonalLinkSectionViewModel
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public bool IsShared { get; set; }

            public int LinkCount { get; set; }

            public int Order { get; set; }

            public int? OrderId { get; set; }

            public bool CanEdit { get; set; }
        }

        #endregion Helper Classes
    }
}