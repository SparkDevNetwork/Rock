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
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;

namespace RockWeb.Blocks.Steps
{
    [DisplayName( "Step Program List" )]
    [Category( "Steps" )]
    [Description( "Shows a list of all step programs." )]

    #region Block Attributes

    [CategoryField(
        "Categories",
        Key = AttributeKey.Categories,
        Description = "If block should only display Step Programs from specific categories, select the categories here.",
        AllowMultiple = true,
        EntityTypeName = "Rock.Model.StepProgram",
        IsRequired = false,
        Order = 1 )]

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Category = AttributeCategory.LinkedPages,
        Order = 2 )]

    #endregion Block Attributes

    public partial class StepProgramList : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string Categories = "Categories";
            public const string DetailPage = "DetailPage";
        }

        #endregion Attribute Keys

        #region Attribute Categories

        /// <summary>
        /// Keys to use for Block Attribute Categories
        /// </summary>
        private static class AttributeCategory
        {
            public const string LinkedPages = "Linked Pages";
        }

        #endregion Attribute Keys

        #region Base Control Methods

        private List<Guid> _categoryGuids = null;
        private bool _showFilter = true;
        private bool _showCategoryColumn = true;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.ApplyBlockSettings();

            // Initialize Filter
            if ( _showFilter )
            {
                if ( !Page.IsPostBack )
                {
                    BindFilter();
                }

                rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
                rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;
            }

            InitializeGrid();

            // Set up Block Settings change notification.
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upStepProgramList );
        }

        /// <summary>
        /// Set the properties of the main grid.
        /// </summary>
        private void InitializeGrid()
        {
            // Initialize Grid
            gStepProgram.DataKeyNames = new string[] { "Id" };
            gStepProgram.Actions.AddClick += gStepProgram_Add;
            gStepProgram.GridReorder += gStepProgram_GridReorder;
            gStepProgram.GridRebind += gStepProgram_GridRebind;
            gStepProgram.RowSelected += gStepProgram_Edit;
            gStepProgram.RowItemText = "Step Program";

            // Initialize Grid: Secured actions
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            bool canAdministrate = IsUserAuthorized( Authorization.ADMINISTRATE );

            gStepProgram.Actions.ShowAdd = canAddEditDelete;
            gStepProgram.IsDeleteEnabled = canAddEditDelete;

            var reorderField = gStepProgram.ColumnsOfType<ReorderField>().FirstOrDefault();

            if ( reorderField != null )
            {
                reorderField.Visible = canAddEditDelete;
            }

            var securityField = gStepProgram.ColumnsOfType<SecurityField>().FirstOrDefault();

            if ( securityField != null )
            {
                securityField.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.StepProgram ) ).Id;
                securityField.Visible = canAdministrate;
            }
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

        #endregion Base Control Methods

        #region Control Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            this.ApplyBlockSettings();

            BindGrid();
        }

        #endregion Control Events

        #region Filter Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            int? categoryId = cpCategory.SelectedValueAsInt();

            rFilter.SaveUserPreference( "Category", categoryId.HasValue ? categoryId.Value.ToString() : string.Empty );
            rFilter.SaveUserPreference( "Active", ddlActiveFilter.SelectedValue );

            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilter_ClearFilterClick( object sender, EventArgs e )
        {
            rFilter.DeleteUserPreferences();

            BindFilter();
        }

        /// <summary>
        /// ts the filter display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( e.Key == "Category" )
            {
                int? categoryId = e.Value.AsIntegerOrNull();
                if ( categoryId.HasValue )
                {
                    var category = CategoryCache.Get( categoryId.Value );
                    if ( category != null )
                    {
                        e.Value = category.Name;
                    }
                }
                else
                {
                    e.Value = string.Empty;
                }
            }
            else if ( e.Key == "Active" )
            {
                e.Value = ddlActiveFilter.SelectedValue;
            }
            else
            {
                e.Value = string.Empty;
            }
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gStepProgram control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gStepProgram_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "ProgramId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gStepProgram control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gStepProgram_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "ProgramId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gStepProgram control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gStepProgram_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();

            var stepProgramService = new StepProgramService( rockContext );

            var stepProgram = stepProgramService.Get( e.RowKeyId );

            if ( stepProgram == null )
            {
                mdGridWarning.Show( "This item could not be found.", ModalAlertType.Information );
                return;
            }

            string errorMessage;

            if ( !stepProgramService.CanDelete( stepProgram, out errorMessage ) )
            {
                mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                return;
            }

            stepProgramService.Delete( stepProgram );

            rockContext.SaveChanges();

            BindGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the gStepProgram control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs" /> instance containing the event data.</param>
        void gStepProgram_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var service = new StepProgramService( rockContext );
            var stepPrograms = service.Queryable().OrderBy( b => b.Order );

            service.Reorder( stepPrograms.ToList(), e.OldIndex, e.NewIndex );
            rockContext.SaveChanges();

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gStepProgram control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gStepProgram_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion Grid Events

        #region Internal Methods

        /// <summary>
        /// Apply block configuration settings.
        /// </summary>
        private void ApplyBlockSettings()
        {
            // Get Block Settings
            _categoryGuids = GetAttributeValue( AttributeKey.Categories ).SplitDelimitedValues().AsGuidList();

            // If the block is constrained to show specific categories, hide the filter.
            _showFilter = !_categoryGuids.Any();

            rFilter.Visible = _showFilter;

            // If the block is constrained to a single category, hide the Category list column.
            _showCategoryColumn = _categoryGuids.Count != 1;

            gStepProgram.ColumnsOfType<RockBoundField>().Where( c => c.DataField == "Category" ).First().Visible = _showCategoryColumn;
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            var categoryId = rFilter.GetUserPreference( "Category" ).AsIntegerOrNull();
            if ( categoryId > 0 )
            {
                cpCategory.SetValue( categoryId );
            }
            else
            {
                cpCategory.SetValue( null );
            }

            ddlActiveFilter.SetValue( rFilter.GetUserPreference( "Active" ) );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var dataContext = new RockContext();

            var stepProgramsQry = new StepProgramService( dataContext )
                .Queryable();

            // Filter by: Category
            if ( _categoryGuids.Any() )
            {
                stepProgramsQry = stepProgramsQry.Where( a => a.Category != null && _categoryGuids.Contains( a.Category.Guid ) );
            }
            else
            {
                var categoryId = rFilter.GetUserPreference( "Category" ).AsIntegerOrNull();

                if ( categoryId.HasValue && categoryId > 0 )
                {
                    stepProgramsQry = stepProgramsQry.Where( a => a.CategoryId == categoryId.Value );
                }
            }

            // Filter by: Active
            var activeFilter = rFilter.GetUserPreference( "Active" ).ToLower();

            switch ( activeFilter )
            {
                case "active":
                    stepProgramsQry = stepProgramsQry.Where( a => a.IsActive );
                    break;
                case "inactive":
                    stepProgramsQry = stepProgramsQry.Where( a => !a.IsActive );
                    break;
            }

            // Sort by: Order
            stepProgramsQry = stepProgramsQry.OrderBy( b => b.Order );

            // Retrieve the Step Program data models and create corresponding view models to display in the grid.
            var stepService = new StepService( dataContext );

            var completedStepsQry = stepService.Queryable().Where( x => x.StepStatus != null && x.StepStatus.IsCompleteStatus );

            var stepPrograms = stepProgramsQry.Select( x =>
                new StepProgramListItemViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    IconCssClass = x.IconCssClass,
                    Category = x.Category.Name,
                    StepTypeCount = x.StepTypes.Count,
                    StepCompletedCount = completedStepsQry.Count( y => y.StepType.StepProgramId == x.Id )
                } )
                .ToList();

            gStepProgram.DataSource = stepPrograms;

            gStepProgram.DataBind();
        }

        #endregion Internal Methods

        #region Helper Classes

        /// <summary>
        /// Represents an entry in the list of Step Programs shown on this page.
        /// </summary>
        public class StepProgramListItemViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string IconCssClass { get; set; }
            public string Category { get; set; }
            public int StepTypeCount { get; set; }
            public int StepCompletedCount { get; set; }
        }

        #endregion Helper Classes
    }
}