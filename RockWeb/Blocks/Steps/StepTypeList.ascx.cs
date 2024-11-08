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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Steps
{
    [DisplayName( "Step Type List" )]
    [Category( "Steps" )]
    [Description( "Shows a list of all step types for a program." )]
    [ContextAware( typeof( Campus ) )]

    #region Block Attributes

    [StepProgramField(
        "Step Program",
        Key = AttributeKey.StepProgram,
        Description = "Display Step Types from a specified program. If none selected, the block will display the program from the current context.",
        IsRequired = false,
        Order = 1 )]
    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Category = AttributeCategory.LinkedPages,
        Order = 2 )]
    [LinkedPage(
        "Bulk Entry",
        Key = AttributeKey.BulkEntryPage,
        Description = "Linked page that allows for bulk entry of steps for a step type.",
        Category = AttributeCategory.LinkedPages,
        Order = 3 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "3EFB4302-9AB4-420F-A818-48B1B06AD109" )]
    public partial class StepTypeList : ContextEntityBlock, ISecondaryBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string StepProgram = "Programs";
            public const string DetailPage = "DetailPage";
            public const string BulkEntryPage = "BulkEntryPage";
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

        #endregion Attribute Categories

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string StepProgramId = "ProgramId";
            public const string StepTypeId = "StepTypeId";
        }

        #endregion Page Parameter Keys

        #region User Preference Keys

        /// <summary>
        /// Keys to use for Filter Settings
        /// </summary>
        private static class FilterSettingName
        {
            public const string Name = "Name";
            public const string AllowMultiple = "Allow Multiple";
            public const string SpansTime = "Spans Time";
            public const string ActiveStatus = "Active Status";
        }

        #endregion Page Parameter Keys

        #region Private Variables

        private StepProgram _program = null;
        private RockContext _dataContext = null;
        private bool _blockContextIsValid = false;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.InitializeBlockNotification( nbBlockStatus, pnlList );
            this.InitializeSettingsNotification( upMain );

            _blockContextIsValid = this.InitializeBlockContext();

            if ( !_blockContextIsValid )
            {
                return;
            }

            this.InitializeFilter();

            this.InitializeGrid();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !_blockContextIsValid )
            {
                return;
            }

            this.ShowBlockDetail();

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
            _blockContextIsValid = this.InitializeBlockContext();

            if ( !_blockContextIsValid )
            {
                return;
            }

            this.ConfigureGridFromBlockSettings();

            this.BindGrid();
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
            this.ApplyGridFilter();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilter_ClearFilterClick( object sender, EventArgs e )
        {
            this.ClearGridFilter();
        }

        /// <summary>
        /// ts the filter display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            this.SaveFilterSettings();

            e.Value = this.GetFilterValueDescription( e.Key );
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gStepType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gStepType_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.StepTypeId, 0, PageParameterKey.StepProgramId, _program.Id );
        }

        /// <summary>
        /// Handles the Edit event of the gStepType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gStepType_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.StepTypeId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the Edit event of the gStepType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gStepType_BulkEntry( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.BulkEntryPage, PageParameterKey.StepTypeId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gStepType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gStepType_Delete( object sender, RowEventArgs e )
        {
            this.DeleteStepType( e.RowKeyId );
        }

        /// <summary>
        /// Handles the GridReorder event of the gStepType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs" /> instance containing the event data.</param>
        void gStepType_GridReorder( object sender, GridReorderEventArgs e )
        {
            this.ReorderStepType( e.OldIndex, e.NewIndex );
        }

        /// <summary>
        /// Handles the GridRebind event of the gStepType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gStepType_GridRebind( object sender, EventArgs e )
        {
            this.BindGrid();
        }

        #endregion Grid Events

        #region Internal Methods

        /// <summary>
        /// Retrieve a singleton data context for data operations in this block.
        /// </summary>
        /// <returns></returns>
        private RockContext GetDataContext()
        {
            if ( _dataContext == null )
            {
                _dataContext = new RockContext();
            }

            return _dataContext;
        }

        /// <summary>
        /// Initialize handlers for block configuration changes.
        /// </summary>
        /// <param name="triggerPanel"></param>
        private void InitializeSettingsNotification( UpdatePanel triggerPanel )
        {
            // Set up Block Settings change notification.
            this.BlockUpdated += Block_BlockUpdated;

            this.AddConfigurationUpdateTrigger( triggerPanel );
        }

        /// <summary>
        /// Initialize the list filter.
        /// </summary>
        private void InitializeFilter()
        {
            if ( !Page.IsPostBack )
            {
                if ( _program != null )
                {
                    rFilter.PreferenceKeyPrefix = string.Format( "{0}-", _program.Id );
                }

                this.BindFilter();
            }

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;
        }

        /// <summary>
        /// Set the properties of the main grid.
        /// </summary>
        private void InitializeGrid()
        {
            if ( _program == null )
            {
                return;
            }

            gStepType.DataKeyNames = new string[] { "Id" };
            gStepType.Actions.AddClick += gStepType_Add;
            gStepType.GridReorder += gStepType_GridReorder;
            gStepType.GridRebind += gStepType_GridRebind;
            gStepType.RowSelected += gStepType_Edit;
            gStepType.RowItemText = "Step Type";
            gStepType.ExportSource = ExcelExportSource.DataSource;
            gStepType.ExportFilename = _program.Name;

            // Initialize Grid: Secured actions
            /*
             SK - 10/28/2021

             Block Authorization is removed after Step Type parent authority is set to Step Program.
             */
            bool canAddEditDelete = _program.IsAuthorized( Authorization.EDIT, this.CurrentPerson );
            bool canAdministrate = IsUserAuthorized( Authorization.ADMINISTRATE );

            gStepType.Actions.ShowAdd = canAddEditDelete;
            gStepType.IsDeleteEnabled = canAddEditDelete;

            var reorderField = gStepType.ColumnsOfType<ReorderField>().FirstOrDefault();

            if ( reorderField != null )
            {
                reorderField.Visible = canAddEditDelete;
            }

            var securityField = gStepType.ColumnsOfType<SecurityField>().FirstOrDefault();

            if ( securityField != null )
            {
                securityField.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.StepType ) ).Id;
                securityField.Visible = canAdministrate;
            }

            this.ConfigureGridFromBlockSettings();
        }

        /// <summary>
        /// Configure grid elements that are affected by the current block settings.
        /// </summary>
        private void ConfigureGridFromBlockSettings()
        {
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );

            // Set availability of Bulk Entry button.
            bool allowBulkEntry = canAddEditDelete && GetAttributeValue( AttributeKey.BulkEntryPage ).IsNotNullOrWhiteSpace();

            var bulkEntryColumn = gStepType.ColumnsOfType<LinkButtonField>().FirstOrDefault( a => a.ID == "lbBulkEntry" );
            if ( bulkEntryColumn != null )
            {
                bulkEntryColumn.Visible = allowBulkEntry;
            }
        }

        /// <summary>
        /// Initialize the essential context in which this block is operating.
        /// </summary>
        /// <returns>True, if the block context is valid.</returns>
        private bool InitializeBlockContext()
        {
            _program = null;

            // Try to load the Step Program from the cache.
            var programGuid = GetAttributeValue( AttributeKey.StepProgram ).AsGuid();

            int programId = 0;
            string sharedItemKey;

            // If a Step Program is specified in the block settings use it, otherwise use the PageParameters.
            if ( programGuid != Guid.Empty )
            {
                sharedItemKey = string.Format( "{0}:{1}", PageParameterKey.StepProgramId, programGuid );
            }
            else
            {
                programId = PageParameter( PageParameterKey.StepProgramId ).AsInteger();

                sharedItemKey = string.Format( "{0}:{1}", PageParameterKey.StepProgramId, programId );
            }

            if ( !string.IsNullOrEmpty( sharedItemKey ) )
            {
                _program = RockPage.GetSharedItem( sharedItemKey ) as StepProgram;
            }

            // Retrieve the program from the data store and cache for subsequent use.
            if ( _program == null )
            {
                var dataContext = this.GetDataContext();

                var stepProgramService = new StepProgramService( dataContext );

                if ( programGuid != Guid.Empty )
                {
                    _program = stepProgramService.Queryable().Where( g => g.Guid == programGuid ).FirstOrDefault();
                }
                else if ( programId != 0 )
                {
                    _program = stepProgramService.Queryable().Where( g => g.Id == programId ).FirstOrDefault();
                }

                if ( _program != null )
                {
                    RockPage.SaveSharedItem( sharedItemKey, _program );
                }
            }

            // Verify the Step Program is valid.
            if ( _program == null )
            {
                this.ShowNotification( "There are no Step Types available in this context.", NotificationBoxType.Info, true );
                return false;
            }

            // Check for View permissions.
            if ( !_program.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                this.ShowNotification( "Sorry, you are not authorized to view this content.", NotificationBoxType.Danger, true );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Show the block content.
        /// </summary>
        private void ShowBlockDetail()
        {
            if ( Page.IsPostBack )
            {
                return;
            }

            this.BindGrid();
        }

        /// <summary>
        /// Set the ordinal position of an item in the list.
        /// </summary>
        /// <param name="oldIndex"></param>
        /// <param name="newIndex"></param>
        private void ReorderStepType( int oldIndex, int newIndex )
        {
            if ( _program == null )
            {
                return;
            }

            var rockContext = this.GetDataContext();

            var service = new StepTypeService( rockContext );

            var stepTypes = service.Queryable()
                .Where( x => x.StepProgramId == _program.Id )
                .OrderBy( b => b.Order )
                .ToList();

            service.Reorder( stepTypes, oldIndex, newIndex );

            rockContext.SaveChanges();

            this.BindGrid();
        }

        /// <summary>
        /// Store the current filter field values and reload the grid using the stored filter.
        /// </summary>
        private void ApplyGridFilter()
        {
            this.SaveFilterSettings();

            this.BindGrid();
        }

        /// <summary>
        /// Clear the filter fields for the grid.
        /// </summary>
        private void ClearGridFilter()
        {
            rFilter.DeleteFilterPreferences();

            BindFilter();
        }

        /// <summary>
        /// Delete the specified Step Type.
        /// </summary>
        /// <param name="stepTypeId"></param>
        private void DeleteStepType( int stepTypeId )
        {
            var rockContext = this.GetDataContext();

            var stepTypeService = new StepTypeService( rockContext );

            var stepType = stepTypeService.Get( stepTypeId );

            if ( stepType == null )
            {
                mdGridWarning.Show( "This item could not be found.", ModalAlertType.Information );
                return;
            }

            string errorMessage;

            if ( !stepTypeService.CanDelete( stepType, out errorMessage ) )
            {
                mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                return;
            }

            stepTypeService.Delete( stepType );

            rockContext.SaveChanges();

            BindGrid();
        }

        /// <summary>
        /// Binds data to the filter controls.
        /// </summary>
        private void BindFilter()
        {
            txbNameFilter.Text = rFilter.GetFilterPreference( FilterSettingName.Name );
            ddlAllowMultipleFilter.SetValue( rFilter.GetFilterPreference( FilterSettingName.AllowMultiple ) );
            ddlHasDurationFilter.SetValue( rFilter.GetFilterPreference( FilterSettingName.SpansTime ) );
            ddlActiveFilter.SetValue( rFilter.GetFilterPreference( FilterSettingName.ActiveStatus ) );
        }

        /// <summary>
        /// Save the current filter settings.
        /// </summary>
        private void SaveFilterSettings()
        {
            rFilter.SetFilterPreference( FilterSettingName.Name, txbNameFilter.Text );
            rFilter.SetFilterPreference( FilterSettingName.AllowMultiple, ddlAllowMultipleFilter.SelectedValue );
            rFilter.SetFilterPreference( FilterSettingName.SpansTime, ddlHasDurationFilter.SelectedValue );
            rFilter.SetFilterPreference( FilterSettingName.ActiveStatus, ddlActiveFilter.SelectedValue );
        }

        /// <summary>
        /// Gets the user-friendly description for a filter field setting.
        /// </summary>
        /// <param name="filterSettingName"></param>
        /// <returns></returns>
        private string GetFilterValueDescription( string filterSettingName )
        {
            if ( filterSettingName == FilterSettingName.Name )
            {
                return string.Format( "Contains \"{0}\"", txbNameFilter.Text );
            }
            else if ( filterSettingName == FilterSettingName.AllowMultiple )
            {
                return ddlAllowMultipleFilter.SelectedValue;
            }
            else if ( filterSettingName == FilterSettingName.SpansTime )
            {
                return ddlHasDurationFilter.SelectedValue;
            }
            else if ( filterSettingName == FilterSettingName.ActiveStatus )
            {
                return ddlActiveFilter.SelectedValue;
            }

            return string.Empty;
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            if ( !_blockContextIsValid )
            {
                return;
            }

            if ( _program == null )
            {
                return;
            }

            var dataContext = this.GetDataContext();

            var stepTypesQry = new StepTypeService( dataContext )
                .Queryable();

            // Filter by: Step Programs
            stepTypesQry = stepTypesQry.Where( x => x.StepProgramId == _program.Id );

            // Filter by: Name
            var name = rFilter.GetFilterPreference( FilterSettingName.Name ).ToStringSafe();

            if ( !string.IsNullOrWhiteSpace( name ) )
            {
                stepTypesQry = stepTypesQry.Where( a => a.Name.Contains( name ) );
            }

            // Filter by: Allow Multiple
            var allowMultiple = rFilter.GetFilterPreference( FilterSettingName.AllowMultiple ).AsBooleanOrNull();

            if ( allowMultiple.HasValue )
            {
                stepTypesQry = stepTypesQry.Where( a => a.AllowMultiple == allowMultiple.Value );
            }

            // Filter by: Has Duration
            var hasDuration = rFilter.GetFilterPreference( FilterSettingName.SpansTime ).AsBooleanOrNull();

            if ( hasDuration.HasValue )
            {
                stepTypesQry = stepTypesQry.Where( a => a.HasEndDate == hasDuration.Value );
            }

            // Filter by: Active
            var activeFilter = rFilter.GetFilterPreference( FilterSettingName.ActiveStatus ).ToUpperInvariant();

            switch ( activeFilter )
            {
                case "ACTIVE":
                    stepTypesQry = stepTypesQry.Where( a => a.IsActive );
                    break;
                case "INACTIVE":
                    stepTypesQry = stepTypesQry.Where( a => !a.IsActive );
                    break;
            }

            // Sort by: Order, Id.
            stepTypesQry = stepTypesQry.OrderBy( b => b.Order ).ThenBy( b => b.Id );

            // Retrieve the Step Type data models and create corresponding view models to display in the grid.
            var stepService = new StepService( dataContext );

            var startedStepsQry = stepService.Queryable();
            var completedStepsQry = stepService.Queryable().Where( x => x.StepStatus != null && x.StepStatus.IsCompleteStatus );

            // Filter by CampusId
            var campusContext = GetCampusContextOrNull();
            if ( campusContext != null )
            {
                startedStepsQry = startedStepsQry.Where( s => s.CampusId == campusContext.Id );
                completedStepsQry = completedStepsQry.Where( s => s.CampusId == campusContext.Id );
            }

            var stepTypes = stepTypesQry.Select( x =>
                new StepTypeListItemViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    IconCssClass = x.IconCssClass,
                    AllowMultipleInstances = x.AllowMultiple,
                    HasDuration = x.HasEndDate,
                    StartedCount = startedStepsQry.Count( y => y.StepTypeId == x.Id ),
                    CompletedCount = completedStepsQry.Count( y => y.StepTypeId == x.Id )
                } )
                .ToList();

            gStepType.DataSource = stepTypes;

            gStepType.DataBind();
        }

        /// <summary>
        /// Gets the campus context, returns null if there is only no more than one active campus.
        /// This is to prevent to filtering out of Steps that are associated with currently inactive
        /// campuses or no campus at all.
        /// </summary>
        /// <returns></returns>
        private Campus GetCampusContextOrNull()
        {
            return ( CampusCache.All( false ).Count > 1 ) ? ContextEntity<Campus>() : null;
        }

        #endregion Internal Methods

        #region Block Notifications

        private NotificationBox _notificationControl;
        private Control _detailContainerControl;

        /// <summary>
        /// Initialize block-level notification message handlers for block configuration changes.
        /// </summary>
        /// <param name="triggerPanel"></param>
        private void InitializeBlockNotification( NotificationBox notificationControl, Control detailContainerControl )
        {
            _notificationControl = notificationControl;
            _detailContainerControl = detailContainerControl;

            this.ClearBlockNotification();
        }

        /// <summary>
        /// Reset the notification message for the block.
        /// </summary>
        public void ClearBlockNotification()
        {
            _notificationControl.Visible = false;
            _detailContainerControl.Visible = true;
        }

        /// <summary>
        /// Show a notification message for the block.
        /// </summary>
        /// <param name="notificationControl"></param>
        /// <param name="message"></param>
        /// <param name="notificationType"></param>
        public void ShowNotification( string message, NotificationBoxType notificationType = NotificationBoxType.Info, bool hideBlockContent = false )
        {
            _notificationControl.Text = message;
            _notificationControl.NotificationBoxType = notificationType;

            _notificationControl.Visible = true;
            _detailContainerControl.Visible = !hideBlockContent;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Represents an entry in the list of Step Programs shown on this page.
        /// </summary>
        public class StepTypeListItemViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string IconCssClass { get; set; }
            public bool AllowMultipleInstances { get; set; }
            public bool HasDuration { get; set; }
            public int StartedCount { get; set; }
            public int CompletedCount { get; set; }
        }

        #endregion Helper Classes

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visibility of this block in response to a directive from a primary block.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        #endregion
    }
}