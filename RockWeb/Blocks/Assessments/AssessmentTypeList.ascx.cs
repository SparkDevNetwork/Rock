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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Assessments
{
    [DisplayName( "Assessment Type List" )]
    [Category( "Assessments" )]
    [Description( "Shows a list of all Assessment Types." )]

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Category = AttributeCategory.LinkedPages,
        Order = 1 )]

    #endregion

    [Rock.SystemGuid.BlockTypeGuid( "00A86827-1E0C-4F47-8A6F-82581FA75CED" )]
    public partial class AssessmentTypeList : RockBlock, ISecondaryBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
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
            public const string AssessmentTypeId = "AssessmentTypeId";
        }

        #endregion

        #region User Preference Keys

        /// <summary>
        /// Keys to use for Filter Settings
        /// </summary>
        private static class FilterSettingName
        {
            public const string Title = "Title";
            public const string RequiresRequest = "Requires Request";
            public const string IsActive = "Is Active";
        }

        #endregion

        #region Constructors

        public AssessmentTypeList()
        {
            this.ItemSystemType = typeof( AssessmentType );

            this.IsSecondaryBlock = true;

            this.ShowItemAdd = true;
            this.ShowItemDelete = true;

            this.PreventSystemItemDelete = true;

            this.ShowItemSecurity = true;
            this.ShowItemReorder = true;

            this.LogExceptionNotifications = true;
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
        /// Gets or sets the ListPanel control that contains the used to display the list of entities.
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

            if ( !this.IsPostBack )
            {
                ShowBlockDetail();
            }
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
            OnConfigureListGrid( this.ListGridControl );

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

            e.Value = OnGetFilterValueDescription( e.Key );
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
        private void gList_Edit( object sender, RowEventArgs e )
        {
            OnNavigateToDetailPage( e.RowKeyId );
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
                    ListFilterControl.PreferenceKeyPrefix = OnGetUserPreferenceKeyPrefix();
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
            ListGridControl.RowItemText = _entityType.FriendlyName;
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
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            bool canAdministrate = IsUserAuthorized( Authorization.ADMINISTRATE );

            ListGridControl.Actions.ShowAdd = this.ShowItemAdd && canAddEditDelete;

            if ( canAddEditDelete )
            {
                ListGridControl.RowSelected += gList_Edit;
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
            ListGridControl.IsDeleteEnabled = this.ShowItemDelete && canAddEditDelete;

            if ( this.ShowItemDelete
                 && canAddEditDelete )
            {
                _deleteColumn = ListGridControl.ColumnsOfType<DeleteField>().FirstOrDefault();

                if ( _deleteColumn == null )
                {
                    _deleteColumn = new DeleteField();

                    _deleteColumn.Click += gList_Delete;

                    ListGridControl.Columns.Add( _deleteColumn );
                }
            }

            // Perform additional customization of the list grid if required.
            OnConfigureListGrid( this.ListGridControl );
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
            if ( Page.IsPostBack )
            {
                return;
            }

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
            ListFilterControl.DeleteFilterPreferences();

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
                settings[key] = ListFilterControl.GetFilterPreference( key );
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
                ListFilterControl.SetFilterPreference( kvp.Key, kvp.Value );
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

            var items = OnGetListDataSource( dataContext, settings, ListGridControl.SortProperty );

            ListGridControl.DataSource = items;

            ListGridControl.DataBind();

            // The Grid has built-in functionality to prevent deletion of system-protected items if the Data Source implements an "IsSystem" flag.
            // If this option is enforced, verify that the data source is correctly configured to support this feature.
            if ( this.ShowItemDelete
                 && this.PreventSystemItemDelete
                 && items != null )
            {
                var enumerator = ( ( IEnumerable ) items ).GetEnumerator();

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
                StatusNotificationControl = this.ControlsOfTypeRecursive<NotificationBox>().FirstOrDefault();
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
        /// Show a fatal error indicating that the user does not have permission to access this content.
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

        #endregion

        #region Custom Methods (AssessmentTypeList)

        /// <summary>
        /// Apply custom configuration settings to the list grid.
        /// </summary>
        /// <param name="listGrid"></param>
        /// <returns></returns>
        private bool OnConfigureListGrid( Grid listGrid )
        {
            return true;
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
        /// Set the required configuration parameters to determine how this block will operate.
        /// Any required parameters will be verified after this method is completed.
        /// </summary>
        private void OnConfigureBlock()
        {
            this.ListPanelControl = pnlList;
            this.ListGridControl = gList;
            this.ListFilterControl = rFilter;
            this.ModalAlertControl = mdAlert;

            this.ShowItemAdd = true;
            this.ShowItemDelete = true;
            this.ShowItemSecurity = true;
            this.ShowItemReorder = false;
        }

        /// <summary>
        /// Get the name of the URL query string parameter used to pass the entity Id property to a linked page.
        /// </summary>
        /// <returns></returns>
        private string OnGetUrlQueryParameterNameEntityId()
        {
            return PageParameterKey.AssessmentTypeId;
        }

        /// <summary>
        /// Apply the filter settings to the filter controls.
        /// </summary>
        /// <param name="settingsKeyValueMap"></param>
        private void OnApplyFilterSettings( Dictionary<string, string> settingsKeyValueMap )
        {
            txbTitleFilter.Text = settingsKeyValueMap[FilterSettingName.Title];
            ddlRequiresRequestFilter.SetValue( settingsKeyValueMap[FilterSettingName.RequiresRequest] );
            ddlIsActiveFilter.SetValue( settingsKeyValueMap[FilterSettingName.IsActive] );
        }

        /// <summary>
        /// Get a key/value map of current filter settings to be saved.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> OnStoreFilterSettings()
        {
            var settings = new Dictionary<string, string>();

            settings[FilterSettingName.Title] = txbTitleFilter.Text;
            settings[FilterSettingName.RequiresRequest] = ddlRequiresRequestFilter.SelectedValue;
            settings[FilterSettingName.IsActive] = ddlIsActiveFilter.SelectedValue;

            return settings;
        }

        /// <summary>
        /// Gets the user-friendly description for a filter field setting.
        /// </summary>
        /// <param name="filterSettingName"></param>
        /// <returns></returns>
        private string OnGetFilterValueDescription( string filterSettingName )
        {
            if ( filterSettingName == FilterSettingName.Title )
            {
                return string.Format( "Contains \"{0}\"", txbTitleFilter.Text );
            }
            else if ( filterSettingName == FilterSettingName.RequiresRequest )
            {
                return ddlRequiresRequestFilter.SelectedValue;
            }
            else if ( filterSettingName == FilterSettingName.IsActive )
            {
                return ddlIsActiveFilter.SelectedValue;
            }

            return string.Empty;
        }

        /// <summary>
        /// Get the data source for the list after applying the specified filter settings.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="filterSettingsKeyValueMap"></param>
        /// <returns></returns>
        private object OnGetListDataSource( RockContext dataContext, Dictionary<string, string> filterSettingsKeyValueMap, SortProperty sortProperty )
        {
            var assessmentService = new AssessmentTypeService( dataContext );

            var assessmentTypesQry = assessmentService.Queryable();

            // Filter by: Title
            var name = filterSettingsKeyValueMap[FilterSettingName.Title].ToStringSafe();

            if ( !string.IsNullOrWhiteSpace( name ) )
            {
                assessmentTypesQry = assessmentTypesQry.Where( a => a.Title.Contains( name ) );
            }

            // Filter by: Requires Request
            var requiresRequest = rFilter.GetFilterPreference( FilterSettingName.RequiresRequest ).AsBooleanOrNull();

            if ( requiresRequest.HasValue )
            {
                assessmentTypesQry = assessmentTypesQry.Where( a => a.RequiresRequest == requiresRequest.Value );
            }

            // Filter by: Is Active
            var isActive = rFilter.GetFilterPreference( FilterSettingName.IsActive ).AsBooleanOrNull();

            if ( isActive.HasValue )
            {
                assessmentTypesQry = assessmentTypesQry.Where( a => a.IsActive == isActive.Value );
            }

            // Apply Sorting.
            if ( sortProperty != null )
            {
                assessmentTypesQry = assessmentTypesQry.Sort( sortProperty );
            }
            else
            {
                assessmentTypesQry = assessmentTypesQry.OrderBy( b => b.Title );
            }

            // Retrieve the Assessment Type data models and create corresponding view models to display in the grid.
            var assessmentTypes = assessmentTypesQry
                .Select( x => new AssessmentTypeListItemViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    RequiresRequest = x.RequiresRequest,
                    IsActive = x.IsActive,
                    IsSystem = x.IsSystem
                } )
                .ToList();

            return assessmentTypes;
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
            throw new NotImplementedException( "Reordering of list items failed." );
        }

        /// <summary>
        /// Delete the entities corresponding to list items that are deleted.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="entityId"></param>
        /// <returns></returns>
        private bool OnDeleteEntity( RockContext dataContext, int entityId )
        {
            var assessmentTypeService = new AssessmentTypeService( dataContext );

            var assessmentType = assessmentTypeService.Get( entityId );

            if ( assessmentType == null )
            {
                ShowAlert( "This item could not be found." );
                return false;
            }

            string errorMessage;

            if ( !assessmentTypeService.CanDelete( assessmentType, out errorMessage ) )
            {
                ShowAlert( errorMessage, ModalAlertType.Warning );
                return false;
            }

            assessmentTypeService.Delete( assessmentType );

            return true;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Represents an entry in the list of items shown by this block.
        /// </summary>
        public class AssessmentTypeListItemViewModel
        {
            public int Id { get; set; }
            public bool IsSystem { get; set; }
            public bool IsActive { get; set; }

            public string Title { get; set; }
            public bool RequiresRequest { get; set; }
        }

        #endregion
    }
}