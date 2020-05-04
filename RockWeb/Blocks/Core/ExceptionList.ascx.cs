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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// Exception List Block
    /// </summary>
    [DisplayName( "Exception List" )]
    [Category( "Core" )]
    [Description( "Lists all exceptions." )]

    #region Block Attributes

    [IntegerField(
        "Summary Count Days",
        Key = AttributeKey.SummaryCountDays,
        Description = "Summary field for exceptions that have occurred within the last x days. Default value is 7.",
        IsRequired = false,
        DefaultIntegerValue = 7,
        Order = 1 )]
    [DefinedValueField(
        Rock.SystemGuid.DefinedType.CHART_STYLES,
        Key = AttributeKey.ChartStyle,
        Name = "Chart Style",
        Order = 2 )]
    [BooleanField(
        "Show Legend",
        Key = AttributeKey.ShowLegend,
        DefaultBooleanValue = true,
        Order = 3 )]
    [CustomDropdownListField(
        "Legend Position",
        "Select the position of the Legend (corner)",
        "ne,nw,se,sw",
        Key = AttributeKey.LegendPosition,
        IsRequired = false,
        DefaultValue = "ne",
        Order = 4 )]
    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Category = AttributeCategory.LinkedPages,
        Order = 5 )]

    #endregion

    public partial class ExceptionList : RockBlock, ICustomGridColumns
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string SummaryCountDays = "SummaryCountDays";
            public const string ChartStyle = "ChartStyle";
            public const string ShowLegend = "ShowLegend";
            public const string LegendPosition = "LegendPosition";
        }

        #endregion

        #region Attribute Categories

        /// <summary>
        /// Keys to use for Block Attribute Categories
        /// </summary>
        private static class AttributeCategory
        {
            public const string LinkedPages = "Linked Pages";
        }

        #endregion

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string EntityId = "ExceptionId";
        }

        #endregion

        #region User Preference Keys

        /// <summary>
        /// Keys to use for Filter Settings
        /// </summary>
        private static class FilterSettingName
        {
            public const string Site = "Site";
            public const string Page = "Page";
            public const string User = "User";
            public const string DateRange = "Date Range";
            public const string ExceptionType = "Exception Type";
        }

        #endregion

        #region Constructors

        public ExceptionList()
        {
            this.ItemSystemType = typeof( ExceptionLog );

            this.AllowItemSelect = false;
            this.ShowItemAdd = false;
            this.ShowItemDelete = false;

            this.ItemFriendlyName = "Exception Type";
        }

        #endregion

        #region Common Code (EntityList)

        /// <summary>
        /// Code in this region can be reused for any block that shows a simple list of entities.
        /// </summary>

        #region Private Variables

        private RockContext _dataContext = null;
        private bool _blockContextIsValid = false;
        private ReorderField _reorderColumn = null;
        private DeleteField _deleteColumn = null;
        private SecurityField _securityColumn = null;
        private string _blockTitle = "Entity List";
        private EntityTypeCache _entityType = null;

        #endregion

        #region Properties

        /// <summary>
        /// Specifies the action that is triggered when an item is selected in the list.
        /// </summary>
        public bool AllowItemSelect { get; set; }

        /// <summary>
        /// Determines if list items flagged as system-protected are prevented from deletion.
        /// If set to True, objects provided as the data source for the list must implement an IsSystem property.
        /// </summary>
        public bool PreventSystemItemDelete { get; set; }

        /// <summary>
        /// Show a delete action for items in the list?
        /// </summary>
        public bool ShowItemDelete { get; set; }

        /// <summary>
        /// Show a security action for items in the list?
        /// </summary>
        public bool ShowItemSecurity { get; set; }

        /// <summary>
        /// Show an add action for the list?
        /// </summary>
        public bool ShowItemAdd { get; set; }

        /// <summary>
        /// Show reorder handles for items in the list?
        /// </summary>
        public bool ShowItemReorder { get; set; }

        /// <summary>
        /// Write handled block-level exception notifications to the Rock exception log?
        /// </summary>
        public bool LogExceptionNotifications { get; set; }

        /// <summary>
        /// Gets or sets the ListPanel control that contains the grid used to display the list of entities.
        /// This control is the top-level container for all content displayed by the block other than block-level status messages.
        /// </summary
        public Panel ListPanelControl { get; set; }

        /// <summary>
        /// Gets or sets the Grid control used to display the list of entities.
        /// </summary>
        public Grid ListGridControl { get; set; }

        /// <summary>
        /// Gets or sets the Filter control that contains the filter fields available to the user.
        /// </summary>
        public GridFilter ListFilterControl { get; set; }

        /// <summary>
        /// Gets or sets the ModalAlert control that shows page-level notifications to the user.
        /// </summary>
        public ModalAlert ModalAlertControl { get; set; }

        /// <summary>
        /// A NotificationBox control that is used to display a block-level status message.
        /// </summary>
        public NotificationBox StatusNotificationControl { get; set; }

        /// <summary>
        /// Gets or sets the System.Type of the items in the list.
        /// </summary>
        public Type ItemSystemType { get; set; }

        /// <summary>
        /// Gets or sets a user-friendly name for a generic list item.
        /// </summary>
        /// <remarks>
        ///  If not specified, the friendly name of the list item Entity Type will be used.
        /// </remarks>
        public string ItemFriendlyName { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            InitializeBlock();
            InitializeBlockConfigurationChangeHandler();
            InitializeBlockNotifications();

            _blockContextIsValid = InitializeBlockContext();

            if ( !_blockContextIsValid )
            {
                return;
            }

            InitializeFilter();
            InitializeGrid();

            // Perform configuration of non-standard controls for this block.
            OnInitializeCustomActions();

            // Perform operations that are specific to this block implementation.
            OnInitializeBlock( this.IsPostBack );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !_blockContextIsValid )
            {
                return;
            }

            // Perform operations that are specific to this block implementation.
            OnLoadBlock( this.IsPostBack );
        }

        #endregion

        #region Control Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Block_BlockUpdated( object sender, EventArgs e )
        {
            _blockContextIsValid = InitializeBlockContext();

            if ( !_blockContextIsValid )
            {
                return;
            }

            // Apply block settings to the list grid.
            OnInitializeListGrid( this.ListGridControl );

            // Reload the list data.
            BindGrid();
        }

        #endregion

        #region Filter Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            ApplyGridFilter();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void rFilter_ClearFilterClick( object sender, EventArgs e )
        {
            ClearGridFilter();
        }

        /// <summary>
        /// ts the filter display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            SaveFilterSettings();

            e.Value = OnFormatFilterValueDescription( e.Key, e.Value );
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gList_Add( object sender, EventArgs e )
        {
            OnNavigateToDetailPage( 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        private void gList_RowSelected( object sender, RowEventArgs e )
        {
            if ( this.AllowItemSelect )
            {
                OnItemSelectedCustomAction( e.RowKeyId );
            }
        }

        /// <summary>
        /// Handles the Delete event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        private void gList_Delete( object sender, RowEventArgs e )
        {
            DeleteEntity( e.RowKeyId );
        }

        /// <summary>
        /// Handles the GridReorder event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs" /> instance containing the event data.</param>
        private void gList_GridReorder( object sender, GridReorderEventArgs e )
        {
            MoveItem( e.OldIndex, e.NewIndex );
        }

        /// <summary>
        /// Handles the GridRebind event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Perform required initialization tasks to configure the block and validate the user-provided settings.
        /// </summary>
        private void InitializeBlock()
        {
            // Store the Entity Type displayed by the list.
            _entityType = EntityTypeCache.Get( this.ItemSystemType );

            if ( _entityType == null )
            {
                throw new Exception( "Entity Type information for this block could not be retrieved." );
            }

            // Set the default block title for this entity type.
            _blockTitle = string.Format( "{0} List", _entityType.FriendlyName );

            // Perform block-specific initialization tasks.
            OnConfigureBlock();

            // Verify essential properties have been assigned.
            if ( this.ListGridControl == null )
            {
                throw new Exception( "ListGrid property must be initialized." );
            }

            if ( this.ListPanelControl == null )
            {
                throw new Exception( "ListPanel property must be initialized." );
            }

            if ( this.ModalAlertControl == null )
            {
                throw new Exception( "NotificationDialog property must be initialized." );
            }
        }

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
        /// Initialize handlers for block configuration change events.
        /// </summary>
        private void InitializeBlockConfigurationChangeHandler()
        {
            // Handle the Block Settings change notification.
            this.BlockUpdated += Block_BlockUpdated;

            var triggerPanel = GetMainUpdatePanel();

            AddConfigurationUpdateTrigger( triggerPanel );
        }

        /// <summary>
        /// Initialize the filter for the main list.
        /// </summary>
        private void InitializeFilter()
        {
            if ( ListFilterControl == null )
            {
                return;
            }

            // If this is a full page load, initialize the filter control and load the filter values.
            if ( !Page.IsPostBack )
            {
                var keyPrefix = OnGetUserPreferenceKeyPrefix();

                if ( !string.IsNullOrWhiteSpace( keyPrefix ) )
                {
                    ListFilterControl.UserPreferenceKeyPrefix = OnGetUserPreferenceKeyPrefix();
                }

                BindFilter();
            }

            // Hook up the filter event handlers.
            ListFilterControl.ApplyFilterClick += rFilter_ApplyFilterClick;
            ListFilterControl.DisplayFilterValue += rFilter_DisplayFilterValue;
            ListFilterControl.ClearFilterClick += rFilter_ClearFilterClick;
        }

        /// <summary>
        /// Set the properties of the main grid.
        /// </summary>
        private void InitializeGrid()
        {
            ListGridControl.DataKeyNames = new string[] { "Id" };

            if ( string.IsNullOrWhiteSpace( this.ItemFriendlyName ) )
            {
                ListGridControl.RowItemText = _entityType.FriendlyName;
            }
            else
            {
                ListGridControl.RowItemText = this.ItemFriendlyName;
            }

            ListGridControl.ExportSource = ExcelExportSource.DataSource;

            ListGridControl.HideDeleteButtonForIsSystem = this.PreventSystemItemDelete;

            ListGridControl.Actions.AddClick += gList_Add;
            ListGridControl.GridRebind += gList_GridRebind;

            ListGridControl.AllowSorting = !this.ShowItemReorder;

            // Show Reorder handle
            if ( this.ShowItemReorder )
            {
                _reorderColumn = new ReorderField();

                ListGridControl.Columns.Insert( 0, _reorderColumn );

                ListGridControl.GridReorder += gList_GridReorder;
            }

            // Verify block authorization
            bool canEdit = IsUserAuthorized( Authorization.EDIT );
            bool canAdministrate = IsUserAuthorized( Authorization.ADMINISTRATE );

            ListGridControl.Actions.ShowAdd = this.ShowItemAdd && canEdit;

            // Set item click event status.
            ListGridControl.RowClickEnabled = this.AllowItemSelect;

            if ( this.AllowItemSelect )
            {
                ListGridControl.RowSelected += gList_RowSelected;
            }

            // Show Security button
            if ( this.ShowItemSecurity
                 && canAdministrate )
            {
                _securityColumn = ListGridControl.ColumnsOfType<SecurityField>().FirstOrDefault();

                if ( _securityColumn == null )
                {
                    _securityColumn = new SecurityField();

                    ListGridControl.Columns.Add( _securityColumn );
                }

                _securityColumn.EntityTypeId = _entityType.Id;
            }

            // Show Delete button
            ListGridControl.IsDeleteEnabled = this.ShowItemDelete && canEdit;

            if ( this.ShowItemDelete
                 && canEdit )
            {
                _deleteColumn = ListGridControl.ColumnsOfType<DeleteField>().FirstOrDefault();

                if ( _deleteColumn == null )
                {
                    _deleteColumn = new DeleteField();

                    _deleteColumn.Click += gList_Delete;

                    ListGridControl.Columns.Add( _deleteColumn );
                }
            }

            // Perform additional customisation of the list grid if required.
            OnInitializeListGrid( this.ListGridControl );
        }

        /// <summary>
        /// Initialize the essential context in which this block is operating.
        /// </summary>
        /// <returns>True, if the block context is valid.</returns>
        private bool InitializeBlockContext()
        {
            ResetBlockNotification();

            OnInitializeBlockContext();

            // Check for View permissions.
            if ( !IsUserAuthorized( Authorization.VIEW ) )
            {
                ShowNotificationViewUnauthorized();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Show the block content.
        /// </summary>
        private void ShowBlockDetail()
        {
            BindGrid();
        }

        /// <summary>
        /// Set the ordinal position of an item in the list.
        /// </summary>
        /// <param name="oldIndex"></param>
        /// <param name="newIndex"></param>
        private void MoveItem( int oldIndex, int newIndex )
        {
            bool success = OnReorderStepType( oldIndex, newIndex );

            if ( success )
            {
                BindGrid();
            }
        }

        private void ApplyGridFilter()
        {
            SaveFilterSettings();

            BindGrid();
        }

        /// <summary>
        /// Clear the filter fields for the grid.
        /// </summary>
        private void ClearGridFilter()
        {
            ListFilterControl.DeleteUserPreferences();

            BindFilter();
        }

        /// <summary>
        /// Delete the specified Step Type.
        /// </summary>
        /// <param name="stepTypeId"></param>
        private void DeleteEntity( int entityId )
        {
            var dataContext = GetDataContext();

            bool success = OnDeleteEntity( dataContext, entityId );

            if ( !success )
            {
                return;
            }

            dataContext.SaveChanges();

            BindGrid();
        }

        /// <summary>
        /// Binds data to the filter controls.
        /// </summary>
        private void BindFilter()
        {
            if ( ListFilterControl == null )
            {
                return;
            }

            // Get the key/value map with the current values.
            var settings = OnStoreFilterSettings();

            if ( settings == null )
            {
                return;
            }

            // Overwrite the map with the settings stored in the user preferences.
            foreach ( var key in settings.Keys.ToList() )
            {
                settings[key] = ListFilterControl.GetUserPreference( key );
            }

            // Apply the map to update the filter controls.
            OnApplyFilterSettings( settings );
        }

        /// <summary>
        /// Save the current filter settings.
        /// </summary>
        private void SaveFilterSettings()
        {
            var settings = OnStoreFilterSettings();

            if ( settings == null )
            {
                return;
            }

            foreach ( var kvp in settings )
            {
                ListFilterControl.SaveUserPreference( kvp.Key, kvp.Value );
            }
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

            var dataContext = GetDataContext();

            var settings = OnStoreFilterSettings();

            OnPopulateListItems( dataContext, this.ListGridControl, settings, ListGridControl.SortProperty );

            var dataSource = ListGridControl.DataSource;

            // The Grid has built-in functionality to prevent deletion of system-protected items if the Data Source implements an "IsSystem" flag.
            // If this option is enforced, verify that the data source is correctly configured to support this feature.
            if ( this.ShowItemDelete
                 && this.PreventSystemItemDelete
                 && dataSource != null )
            {
                var enumerator = ( ( IEnumerable ) dataSource ).GetEnumerator();

                if ( enumerator.MoveNext() )
                {
                    var item = enumerator.Current;

                    if ( item != null )
                    {
                        var pi = item.GetType().GetProperty( "IsSystem" );

                        if ( pi == null )
                        {
                            // Show a non-fatal configuration error.
                            ShowNotificationError( "Configuration Error: Data Source should implement property \"IsSystem\"." );
                        }
                    }
                }
            }
        }

        private IQueryable<T> GetQueryableAsGeneric<T>( IQueryable query )
        {
            return query.Cast<T>();
        }

        #endregion

        #region Block Notifications and Alerts

        /// <summary>
        /// Set up the mechanism for showing block-level notification messages.
        /// </summary>
        private void InitializeBlockNotifications()
        {
            // If a notification control has not been provided, find the first one in the block.
            if ( StatusNotificationControl == null )
            {
                StatusNotificationControl = this.RockPage.ControlsOfTypeRecursive<NotificationBox>().FirstOrDefault();
            }

            if ( StatusNotificationControl == null )
            {
                throw new Exception( "NotificationControl not found." );
            }

            if ( ListPanelControl == null )
            {
                throw new ArgumentNullException( "ListPanel control not found." );
            }

            // Verify that the notification control is not a child of the detail container.
            // This would cause the notification to be hidden when the content is disallowed.
            var invalidParent = StatusNotificationControl.FindFirstParentWhere( x => x.ID == ListPanelControl.ID );

            if ( invalidParent != null )
            {
                throw new Exception( "NotificationControl cannot be a child of DetailContainerControl." );
            }

            // Set the initial state of the controls.
            ResetBlockNotification();
        }

        /// <summary>
        /// Show a notification message for the block.
        /// </summary>
        /// <param name="notificationControl"></param>
        /// <param name="message"></param>
        /// <param name="notificationType"></param>
        private void ShowNotification( string message, NotificationBoxType notificationType = NotificationBoxType.Info, bool hideBlockContent = false )
        {
            StatusNotificationControl.Text = message;
            StatusNotificationControl.NotificationBoxType = notificationType;

            StatusNotificationControl.Visible = true;
            ListPanelControl.Visible = !hideBlockContent;
        }

        /// <summary>
        /// Reset the notification message for the block.
        /// </summary>
        private void ResetBlockNotification()
        {
            StatusNotificationControl.Visible = false;
            ListPanelControl.Visible = true;
        }

        /// <summary>
        /// Show a block-level error notification.
        /// </summary>
        /// <param name="message"></param>
        private void ShowNotificationError( string message )
        {
            ShowNotification( message, NotificationBoxType.Danger );
        }

        /// <summary>
        /// Show a block-level exception notification. 
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="writeToLog"></param>
        private void ShowNotificationException( Exception ex )
        {
            ShowNotification( ex.Message, NotificationBoxType.Danger );

            if ( this.LogExceptionNotifications )
            {
                LogException( ex );
            }
        }

        /// <summary>
        /// Show a block-level success notification. 
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="writeToLog"></param>
        private void ShowNotificationSuccess( string message )
        {
            ShowNotification( message, NotificationBoxType.Success );
        }

        /// <summary>
        /// Show a fatal error that prevents the block content from being displayed.
        /// </summary>
        /// <param name="message"></param>
        private void ShowNotificationFatalError( string message )
        {
            ShowNotification( message, NotificationBoxType.Danger, true );
        }

        /// <summary>
        /// Show a fatal error indicating that the user does not have permision to access this content.
        /// </summary>
        private void ShowNotificationViewUnauthorized()
        {
            ShowNotification( "Sorry, you are not authorized to view this content.", NotificationBoxType.Danger, true );
        }

        /// <summary>
        /// Show a fatal error indicating that there is no content available in this block for the current context settings.
        /// </summary>
        private void ShowNotificationEmptyContent()
        {
            ShowNotification( "There is no content to show in this context.", NotificationBoxType.Info, true );
        }

        /// <summary>
        /// Show a notification that edit mode is not allowed.
        /// </summary>
        /// <param name="itemFriendlyName"></param>
        private void ShowNotificationEditModeDisallowed()
        {
            ShowNotification( EditModeMessage.ReadOnlyEditActionNotAllowed( _entityType.FriendlyName ), NotificationBoxType.Info, false );
        }

        /// <summary>
        /// Show an alert message that requires user acknowledgement to continue.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="alertType"></param>
        private void ShowAlert( string message, ModalAlertType alertType )
        {
            ModalAlertControl.Show( message, alertType );
        }

        /// <summary>
        /// Show an informational alert message that requires user acknowledgement to continue.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="alertType"></param>
        private void ShowAlert( string message )
        {
            ModalAlertControl.Show( message, ModalAlertType.Information );
        }

        #endregion

        #region ISecondaryBlock Implementation

        /// <summary>
        /// Indicates if this block should behave as a secondary block that responds to the state of a primary block on the same page.
        /// </summary>
        public bool IsSecondaryBlock { get; set; }

        /// <summary>
        /// Sets the visibility of this block in response to a directive from a primary block.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            if ( !IsSecondaryBlock )
            {
                return;
            }

            var panel = GetMainUpdatePanel();

            if ( panel != null )
            {
                panel.Visible = visible;
            }
        }

        #endregion

        #region Default Implementation

        ///
        /// These default method implementations may be replaced by block-specific methods to override default functionality.
        ///

        /// <summary>
        /// Get the name of the URL query string parameter used to pass the entity Id property to a linked page.
        /// </summary>
        /// <returns></returns>
        private string OnGetUrlQueryParameterNameEntityId()
        {
            return PageParameterKey.EntityId;
        }

        /// <summary>
        /// Load data into the block controls.
        /// </summary>
        /// <param name="isPostBack"></param>
        protected void OnLoadBlock( bool isPostBack )
        {
            if ( !isPostBack )
            {
                ShowBlockDetail();
            }
        }

        /// <summary>
        /// Initialize block configuration that depends on settings from the request query string, view state, or other environmental sources.
        /// </summary>
        /// <returns>True, if the block context is valid.</returns>
        private bool OnInitializeBlockContext()
        {
            return true;
        }

        /// <summary>
        /// Get a custom prefix for the key used to store filter settings in user preferences.
        /// </summary>
        /// <returns>An alphanumeric prefix, or empty if not used.</returns>
        private string OnGetUserPreferenceKeyPrefix()
        {
            return string.Empty;
        }

        /// <summary>
        /// Handles the item selected event if a custom behavior is required.
        /// </summary>
        /// <param name="itemId"></param>
        private void OnItemSelectedCustomAction( int itemId )
        {
            // Not required.
        }

        /// <summary>
        /// Provide custom navigation for the Detail Page displayed when an item is selected.
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        private bool OnNavigateToDetailPage( int itemId )
        {
            var entityIdParameterName = OnGetUrlQueryParameterNameEntityId();

            if ( itemId == 0 )
            {
                NavigateToLinkedPage( AttributeKey.DetailPage );
            }
            else
            {
                NavigateToLinkedPage( AttributeKey.DetailPage, entityIdParameterName, itemId );
            }

            return true;
        }

        /// <summary>
        /// Get the main update panel for the block.
        /// </summary>
        /// <returns></returns>
        private UpdatePanel GetMainUpdatePanel()
        {
            // TODO: Improve the efficiency of this search with a "FirstControlOfType" method?
            var panel = this.RockPage.ControlsOfTypeRecursive<UpdatePanel>().FirstOrDefault();

            return panel;
        }

        /// <summary>
        /// Apply changes to the reordering of list items to the corresponding entities.
        /// An implementation of this method is only required if reordering is enabled for this list.
        /// </summary>
        /// <param name="oldIndex"></param>
        /// <param name="newIndex"></param>
        /// <returns>Null if this operation is not implemented.</returns>
        private bool OnReorderStepType( int oldIndex, int newIndex )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete the entities corresponding to list items that are deleted.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="entityId"></param>
        /// <returns></returns>
        private bool OnDeleteEntity( RockContext dataContext, int entityId )
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region Custom Methods (ExceptionList)

        /// <summary>
        /// Create and initialize the block controls.
        /// </summary>
        protected void OnInitializeBlock( bool isPostBack )
        {
            InitializeCharts();
        }

        /// <summary>
        /// Apply custom configuration settings to the list grid.
        /// </summary>
        /// <param name="listGrid"></param>
        /// <returns></returns>
        private void OnInitializeListGrid( Grid listGrid )
        {
            listGrid.RowClickEnabled = false;
        }

        /// <summary>
        /// Configure non-standard controls for this block.
        /// </summary>
        /// <param name="listGrid"></param>
        /// <returns></returns>
        private void OnInitializeCustomActions()
        {
            // Only show clear exceptions button if user has edit rights
            btnClearExceptions.Visible = IsUserAuthorized( Authorization.EDIT );
        }

        /// <summary>
        /// Set the required configuration parameters to determine how this block will operate.
        /// Any required parameters will be verified after this method is completed.
        /// </summary>
        private void OnConfigureBlock()
        {
            this.StatusNotificationControl = nbBlockStatus;
            this.ListPanelControl = pnlList;
            this.ListGridControl = gExceptionList;
            this.ListFilterControl = fExceptionList;
            this.ModalAlertControl = mdAlert;

            this.ShowItemAdd = false;
            this.ShowItemDelete = false;
            this.ShowItemSecurity = false;
            this.ShowItemReorder = false;
        }

        /// <summary>
        /// Apply the filter settings to the filter controls.
        /// </summary>
        /// <param name="settingsKeyValueMap"></param>
        private void OnApplyFilterSettings( Dictionary<string, string> settingsKeyValueMap )
        {
            BindSitesFilter();

            var dataContext = GetDataContext();

            // Site
            int siteId = settingsKeyValueMap.GetValueOrDefault( FilterSettingName.Site, string.Empty ).AsInteger();

            if ( siteId != 0 )
            {
                if ( ddlSite.Items.FindByValue( siteId.ToString() ) != null )
                {
                    ddlSite.SelectedValue = siteId.ToString();
                }
            }

            // Page
            int pageId = settingsKeyValueMap.GetValueOrDefault( FilterSettingName.Page, string.Empty ).AsInteger();

            if ( pageId != 0 )
            {
                var pageService = new PageService( dataContext );

                var page = pageService.Get( pageId );

                ppPage.SetValue( page );
            }

            // User
            int userPersonId = settingsKeyValueMap.GetValueOrDefault( FilterSettingName.User, string.Empty ).AsInteger();

            if ( userPersonId != 0 )
            {
                var personService = new PersonService( dataContext );

                var person = personService.Get( userPersonId );

                ppUser.SetValue( person );
            }

            txtType.Text = settingsKeyValueMap.GetValueOrNull( FilterSettingName.ExceptionType );

            sdpDateRange.DelimitedValues = settingsKeyValueMap.GetValueOrNull( FilterSettingName.DateRange );
        }

        /// <summary>
        /// Get a key/value map of current filter settings to be saved.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> OnStoreFilterSettings()
        {
            var settings = new Dictionary<string, string>();

            if ( ddlSite.SelectedValue != All.IdValue )
            {
                settings[FilterSettingName.Site] = ddlSite.SelectedValue;
            }

            settings[FilterSettingName.User] = ppUser.PersonId.ToStringSafe();
            settings[FilterSettingName.Page] = ppPage.SelectedValueAsInt( true ).ToStringSafe();
            settings[FilterSettingName.ExceptionType] = txtType.Text;
            settings[FilterSettingName.DateRange] = sdpDateRange.DelimitedValues;

            return settings;
        }

        /// <summary>
        /// Gets the user-friendly description for a filter field setting.
        /// </summary>
        /// <param name="filterName"></param>
        /// <returns></returns>
        private string OnFormatFilterValueDescription( string filterName, string value )
        {
            if ( filterName == FilterSettingName.Site )
            {
                int siteId;
                if ( int.TryParse( value, out siteId ) )
                {
                    var site = SiteCache.Get( siteId );
                    if ( site != null )
                    {
                        value = site.Name;
                    }
                }
            }
            else if ( filterName == FilterSettingName.Page )
            {
                int pageId;
                if ( int.TryParse( value, out pageId ) )
                {
                    var page = PageCache.Get( pageId );
                    if ( page != null )
                    {
                        value = page.InternalName;
                    }
                }
            }
            else if ( filterName == FilterSettingName.User )
            {
                int userPersonId;
                if ( int.TryParse( value, out userPersonId ) )
                {
                    var personService = new PersonService( _dataContext );
                    var user = personService.Get( userPersonId );
                    if ( user != null )
                    {
                        value = user.FullName;
                    }
                }
            }
            else if ( filterName == FilterSettingName.ExceptionType )
            {
                value = string.Format( "Contains \"{0}\"", value );
            }
            else if ( filterName == FilterSettingName.DateRange )
            {
                value = SlidingDateRangePicker.FormatDelimitedValues( value );
            }

            return value;
        }

        /// <summary>
        /// Get the data source for the list after applying the specified filter settings.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="filterSettingsKeyValueMap"></param>
        /// <returns></returns>
        private void OnPopulateListItems( RockContext dataContext, Grid listControl, Dictionary<string, string> filterSettingsKeyValueMap, SortProperty sortProperty )
        {
            // Get the summary count attribute.
            int summaryCountDays = Convert.ToInt32( GetAttributeValue( AttributeKey.SummaryCountDays ) );

            var subsetCountField = gExceptionList.ColumnsOfType<RockBoundField>().FirstOrDefault( a => a.DataField == "SubsetCount" );

            if ( subsetCountField != null )
            {
                // Set the header text for the subset/summary field.
                subsetCountField.HeaderText = string.Format( "Last {0} days", summaryCountDays );
            }

            // Get the subset/summary date.
            DateTime minSummaryCountDate = RockDateTime.Now.Date.AddDays( -( summaryCountDays ) );

            // Construct the query...
            var exceptionService = new ExceptionLogService( dataContext );

            var filterQuery = exceptionService.Queryable();

            // Filter by: SiteId
            int siteId = filterSettingsKeyValueMap.GetValueOrDefault( FilterSettingName.Site, string.Empty ).AsInteger();

            if ( siteId != 0 )
            {
                filterQuery = filterQuery.Where( e => e.SiteId == siteId );
            }

            // Filter by: PageId
            int pageId = filterSettingsKeyValueMap.GetValueOrDefault( FilterSettingName.Page, string.Empty ).AsInteger();

            if ( pageId != 0 )
            {
                filterQuery = filterQuery.Where( e => e.PageId == pageId );
            }

            // Filter by: PersonId
            int userPersonID = filterSettingsKeyValueMap.GetValueOrDefault( FilterSettingName.User, string.Empty ).AsInteger();

            if ( userPersonID != 0 )
            {
                filterQuery = filterQuery.Where( e => e.CreatedByPersonAlias != null && e.CreatedByPersonAlias.PersonId == userPersonID );
            }

            // Filter by: Exception Type
            var exceptionType = filterSettingsKeyValueMap.GetValueOrDefault( FilterSettingName.ExceptionType, string.Empty );

            if ( !string.IsNullOrEmpty( exceptionType ) )
            {
                filterQuery = filterQuery.Where( e => e.ExceptionType != null && e.ExceptionType.Contains( exceptionType ) );
            }

            // Filter by: Date Range
            var dateRangeSettings = filterSettingsKeyValueMap.GetValueOrDefault( FilterSettingName.DateRange, string.Empty );

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( dateRangeSettings );

            if ( dateRange.Start.HasValue )
            {
                filterQuery = filterQuery.Where( e => e.CreatedDateTime.HasValue && e.CreatedDateTime.Value >= dateRange.Start.Value );
            }
            if ( dateRange.End.HasValue )
            {
                filterQuery = filterQuery.Where( e => e.CreatedDateTime.HasValue && e.CreatedDateTime.Value < dateRange.End.Value );
            }

            // Exclude all inner exceptions.
            filterQuery = exceptionService.FilterByOutermost( filterQuery );

            // Select data for the list items.
            var listItemQuery = filterQuery
                                    .GroupBy( e => new
                                    {
                                        Type = e.ExceptionType,
                                        Description = e.Description.Substring( 0, ExceptionLogService.DescriptionGroupingPrefixLength )
                                    } )
                                    .Select( eg => new ExceptionLogViewModel
                                    {
                                        Id = eg.Max( e => e.Id ),
                                        ExceptionTypeName = eg.Key.Type,
                                        Description = eg.FirstOrDefault( e => e.Id == eg.Max( e2 => e2.Id ) ).Description,
                                        LastExceptionDate = eg.Max( e => e.CreatedDateTime ),
                                        TotalCount = eg.Count(),
                                        SubsetCount = eg.Count( e => e.CreatedDateTime.HasValue && e.CreatedDateTime.Value >= minSummaryCountDate )
                                    } );

            // Apply sort parameters.
            if ( gExceptionList.SortProperty != null )
            {
                listItemQuery = listItemQuery.Sort( gExceptionList.SortProperty );
            }
            else
            {
                listItemQuery = listItemQuery.OrderByDescending( e => e.LastExceptionDate );
            }

            // Get the query results.
            var exceptions = listItemQuery.ToList();

            // Abbreviate the Exception Type Name to reduce the list width.
            foreach ( var exception in exceptions )
            {
                var periodIndex = exception.ExceptionTypeName.LastIndexOf( "." );

                if ( periodIndex > 0 )
                {
                    exception.ExceptionTypeName = exception.ExceptionTypeName.Substring( periodIndex + 1 );
                }
            }

            // Populate the grid.
            gExceptionList.DataSource = exceptions;

            gExceptionList.DataBind();

            // Update the chart.
            InitializeCharts();
        }

        /// <summary>
        /// Configure the chart controls.
        /// </summary>
        private void InitializeCharts()
        {
            lcExceptions.Options.legend = lcExceptions.Options.legend ?? new Legend();
            lcExceptions.Options.legend.show = GetAttributeValue( AttributeKey.ShowLegend ).AsBooleanOrNull();
            lcExceptions.Options.legend.position = GetAttributeValue( AttributeKey.LegendPosition );
            lcExceptions.Options.SetChartStyle( GetChartStyle() );

            bcExceptions.Options.legend = bcExceptions.Options.legend ?? new Legend();
            bcExceptions.Options.legend.show = GetAttributeValue( AttributeKey.ShowLegend ).AsBooleanOrNull();
            bcExceptions.Options.legend.position = GetAttributeValue( AttributeKey.LegendPosition );
            bcExceptions.Options.xaxis = new AxisOptions { mode = AxisMode.categories, tickLength = 0 };
            bcExceptions.Options.series.bars.barWidth = 0.6;
            bcExceptions.Options.series.bars.align = "center";

            // Set chart style after setting options so they are not overwritten.
            bcExceptions.Options.SetChartStyle( GetChartStyle() );

            bcExceptions.TooltipFormatter = @"
function(item) {
    var itemDate = new Date(item.series.chartData[item.dataIndex].DateTimeStamp);
    var dateText = itemDate.toLocaleDateString();
    var seriesLabel = item.series.label || ( item.series.labels ? item.series.labels[item.dataIndex] : null );
    var pointValue = item.series.chartData[item.dataIndex].YValue || item.series.chartData[item.dataIndex].YValueTotal || '-';
    return dateText + '<br />' + seriesLabel + ': ' + pointValue;
}
";

            // Select the type of graph to show.
            // If there is only one X-axis datapoint, show a barchart.
            var dataContext = GetDataContext();

            var exceptionLogService = new ExceptionLogService( dataContext );

            var exceptionListCount = exceptionLogService.Queryable()
                .Where( x => x.HasInnerException == false && x.CreatedDateTime != null )
                .GroupBy( x => DbFunctions.TruncateTime( x.CreatedDateTime.Value ) )
                .Count();


            var showBarchart = ( exceptionListCount == 1 );

            bcExceptions.Visible = showBarchart;
            lcExceptions.Visible = !showBarchart;
        }

        /// <summary>
        /// Gets the chart style.
        /// </summary>
        /// <value>
        /// The chart style.
        /// </value>
        private ChartStyle GetChartStyle()
        {
            var chartStyleDefinedValueGuid = GetAttributeValue( AttributeKey.ChartStyle ).AsGuidOrNull();

            if ( chartStyleDefinedValueGuid.HasValue )
            {
                var rockContext = GetDataContext();

                var definedValue = new DefinedValueService( rockContext ).Get( chartStyleDefinedValueGuid.Value );

                if ( definedValue != null )
                {
                    try
                    {
                        definedValue.LoadAttributes( rockContext );

                        return ChartStyle.CreateFromJson( definedValue.Value, definedValue.GetAttributeValue( AttributeKey.ChartStyle ) );
                    }
                    catch
                    {
                        // intentionally ignore and default to basic style
                    }
                }
            }

            return new ChartStyle();
        }

        /// <summary>
        /// Bind the sites filter selection list.
        /// </summary>
        private void BindSitesFilter()
        {
            var dataContext = GetDataContext();

            var siteService = new SiteService( dataContext );

            ddlSite.DataTextField = "Name";
            ddlSite.DataValueField = "Id";
            ddlSite.DataSource = siteService.Queryable().OrderBy( s => s.Name ).ToList();
            ddlSite.DataBind();

            ddlSite.Items.Insert( 0, new ListItem( All.Text, All.IdValue ) );
        }

        /// <summary>
        /// Clears the exception log.
        /// </summary>
        private void ClearExceptionLog()
        {
            var dataContext = GetDataContext();

            var exceptionService = new ExceptionLogService( dataContext );

            exceptionService.TruncateLog();
        }

        #region Control Events

        /// <summary>
        /// Handles the Click event of the lbShowDetail control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gExceptionList_ShowDetail( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.EntityId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the Click event of the btnClearExceptions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnClearExceptions_Click( object sender, EventArgs e )
        {
            ClearExceptionLog();

            BindGrid();
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Represents an item in the Exception List.
        /// </summary>
        private class ExceptionLogViewModel : RockDynamic
        {
            public int Id { get; set; }
            public string ExceptionTypeName { get; set; }
            public string Description { get; set; }
            public DateTime? LastExceptionDate { get; set; }
            public int TotalCount { get; set; }
            public int SubsetCount { get; set; }
        }

        #endregion

        #endregion

        /// <summary>
        /// Handles the RowDataBound event of the gExceptionList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gExceptionList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                ExceptionLogViewModel exceptionLogViewModel = e.Row.DataItem as ExceptionLogViewModel;
                if ( exceptionLogViewModel == null )
                {
                    return;
                }

                var lDescription = e.Row.FindControl( "lDescription" ) as Literal;
                if ( lDescription != null )
                {
                    var descriptionAsHtml = exceptionLogViewModel.Description.ConvertCrLfToHtmlBr();
                    if ( descriptionAsHtml.Length > 255 )
                    {
                        // if over 255 chars, limit the height to 100px so we don't get a giant summary displayed in the grid
                        // Also, we don't want to use .Truncate(255) since that could break any html that is in the LastStatusMessageAsHtml
                        lDescription.Text = string.Format( "<div style='max-height:100px;overflow:hidden'>{0}</div>", descriptionAsHtml );
                    }
                    else
                    {
                        lDescription.Text = descriptionAsHtml;
                    }
                }
            }
        }
    }
}