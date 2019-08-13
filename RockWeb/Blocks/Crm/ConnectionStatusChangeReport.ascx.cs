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
using System.Text;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Chart;
using Rock.Constants;
using Rock.Crm.ConnectionStatusChangeReport;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm
{
    [DisplayName( "Connection Status Changes" )]
    [Category( "Connection" )]
    [Description( "Shows changes of Connection Status for people within a specific period." )]

    #region Block Attributes

    [LinkedPage(
        "Person Detail Page",
        Key = AttributeKey.DetailPage,
        Category = AttributeCategory.LinkedPages,
        Order = 0 )]

    #endregion

    public partial class ConnectionStatusChangeReport : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        protected static class AttributeKey
        {
            public const string DetailPage = "PersonDetailPage";
        }

        #endregion

        #region Attribute Categories

        /// <summary>
        /// Keys to use for Block Attribute Categories
        /// </summary>
        protected static class AttributeCategory
        {
            public const string LinkedPages = "Linked Pages";
        }

        #endregion

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        protected static class PageParameterKey
        {
            public const string CampusId = "campusId";
            public const string Period = "period";
            public const string FromStatusId = "fromStatusId";
            public const string ToStatusId = "toStatusId";
            public const string ShowResults = "showResults";
        }

        #endregion

        #region Constants

        private const string _urlValuesListDelimiter = ",";

        #endregion

        #region Private Variables

        private bool _showResults;
        private ReportDataViewModel _report;
        private List<StatusChangeViewModel> _changeEvents;

        #endregion

        #region Public Properties and Methods

        #endregion

        #region Control Methods

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            _report = ViewState["Report"] as ReportDataViewModel;

            _changeEvents = ViewState["Changes"] as List<StatusChangeViewModel>;
        }

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.RegisterScript();

            this.ListPanelControl = pnlResults;

            //this.InitializeBlockNotifications();

            this.InitializeGrid();
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            string eventTarget = this.Page.Request.Params["__EVENTTARGET"] ?? string.Empty;

            ConnectionStatusChangeReportSettings settings;

            // Get report settings from page and query parameters
            settings = this.GetReportSettingsFromPage();

            if ( this.IsPostBack )
            {
                if ( eventTarget.StartsWith( gChanges.UniqueID ) )
                {
                    // Postback is from Grid, so we must be in Results mode.
                    _showResults = true;
                }
            }
            else
            {
                this.ApplyUrlParametersToReportSettings( settings );
            }

            var dataContext = this.GetDataContext();

            BuildPage( dataContext, settings );
        }

        protected override object SaveViewState()
        {
            ViewState["Report"] = _report;
            ViewState["Changes"] = _changeEvents;

            return base.SaveViewState();
        }

        #endregion

        #region Events

        protected void lbApplyFilter_Click( object sender, EventArgs e )
        {
            var settings = this.GetReportSettingsFromPage();

            var queryParams = GetUrlQueryParametersFromSettings( settings, true );

            this.NavigateToCurrentPage( queryParams );
        }


        #endregion

        #region Internal Methods

        /// <summary>
        /// Populate a drop-down list with Connection Types.
        /// </summary>
        /// <param name="control"></param>
        private void InitializeConnectionTypesList( RockDropDownList control )
        {
            var definedValues = this.GetConnectionTypesList();

            control.Items.Clear();

            control.Items.Add( new ListItem( string.Empty ) );

            foreach ( var item in definedValues )
            {
                control.Items.Add( new ListItem( item.Value, item.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Get the list of available Connection Types.
        /// </summary>
        /// <returns></returns>
        private List<DefinedValueCache> GetConnectionTypesList()
        {
            return DefinedTypeCache.GetOrThrow( "Person/Connection Status", Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() ).DefinedValues;
        }

        /// <summary>
        /// Get a collection of Url Query String parameters representing the current report settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="setToReportView"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetUrlQueryParametersFromSettings( ConnectionStatusChangeReportSettings settings, bool setToReportView )
        {
            var queryParams = new Dictionary<string, string>();

            // Campus
            if ( settings.CampusId.GetValueOrDefault( 0 ) != 0 )
            {
                queryParams.Add( PageParameterKey.CampusId, settings.CampusId.ToString() );
            }

            // Period.
            if ( settings.ReportPeriod.Range != TimePeriodRangeSpecifier.Current
                 && settings.ReportPeriod.TimeUnit != TimePeriodUnitSpecifier.Year )
            {
                // Only set the period parameter if it is not the default setting.
                queryParams.Add( PageParameterKey.Period, settings.ReportPeriod.ToDelimitedString( _urlValuesListDelimiter ) );
            }

            // From Connection Status
            if ( settings.FromConnectionStatusId.GetValueOrDefault( 0 ) != 0 )
            {
                queryParams.Add( PageParameterKey.FromStatusId, settings.FromConnectionStatusId.ToString() );
            }

            // To Connection Status
            if ( settings.ToConnectionStatusId.GetValueOrDefault( 0 ) != 0 )
            {
                queryParams.Add( PageParameterKey.ToStatusId, settings.ToConnectionStatusId.ToString() );
            }

            // Show Settings?
            if ( setToReportView )
            {
                queryParams.Add( PageParameterKey.ShowResults, "true" );
            }

            return queryParams;
        }

        /// <summary>
        /// Initialize the results grid.
        /// </summary>
        private void InitializeGrid()
        {
            gChanges.DataKeyNames = new string[] { "Id" };
            gChanges.Actions.ShowAdd = false;
            gChanges.GridRebind += gChanges_GridRebind;
            gChanges.RowDataBound += gChanges_RowDataBound;
            gChanges.PersonIdField = "PersonId";

            gChanges.EntityTypeId = EntityTypeCache.GetId<Person>();

            gChanges.RowItemText = "Connection Status Change";
        }

        /// <summary>
        /// Constructs the web page.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="settings"></param>
        protected void BuildPage( RockContext context, ConnectionStatusChangeReportSettings settings )
        {
            _showResults = PageParameter( PageParameterKey.ShowResults ).AsBoolean( false );

            // Apply settings to page controls.
            cpCampus.SetValue( settings.CampusId );

            drpDateRange.DelimitedValues = settings.ReportPeriod.ToDelimitedString( "|" );

            InitializeConnectionTypesList( ddlFromConnectionStatus );
            InitializeConnectionTypesList( ddlToConnectionStatus );

            ddlFromConnectionStatus.SelectedValue = settings.FromConnectionStatusId.ToStringSafe();
            ddlToConnectionStatus.SelectedValue = settings.ToConnectionStatusId.ToStringSafe();

            if ( _showResults
                 && _report == null )
            {
                // If this is an initial page load, run the Report.
                // Otherwise, it will be loaded from ViewState.
                this.LoadReport();
            }

            pnlResults.Visible = _showResults;

            BindReport();
        }

        /// <summary>
        /// Populate a report settings data object from the page control values.
        /// </summary>
        /// <returns></returns>
        private ConnectionStatusChangeReportSettings GetReportSettingsFromPage()
        {
            var settings = new ConnectionStatusChangeReportSettings();

            settings.CampusId = cpCampus.SelectedValue.AsInteger();

            // Parse the date range settings string supplied by the DateRange control .
            settings.ReportPeriod.FromDelimitedString( drpDateRange.DelimitedValues, "|" );

            settings.FromConnectionStatusId = ddlFromConnectionStatus.SelectedValueAsInt();
            settings.ToConnectionStatusId = ddlToConnectionStatus.SelectedValueAsInt();

            return settings;
        }

        /// <summary>
        /// Apply the Url Query Parameters to a report settings data object.
        /// </summary>
        /// <param name="settings"></param>
        private void ApplyUrlParametersToReportSettings( ConnectionStatusChangeReportSettings settings )
        {
            // Campus
            var campusId = PageParameter( PageParameterKey.CampusId ).AsIntegerOrNull();

            if ( campusId != null )
            {
                settings.CampusId = campusId.Value;
            }

            // Report Period
            var period = PageParameter( PageParameterKey.Period );

            if ( !string.IsNullOrWhiteSpace( period ) )
            {
                settings.ReportPeriod.FromDelimitedString( period, _urlValuesListDelimiter );
            }
            else
            {
                // If a period is not specified, default to Current Year.
                settings.ReportPeriod.SetToCurrentPeriod( TimePeriodUnitSpecifier.Year );
            }

            // From Status
            var fromStatusId = PageParameter( PageParameterKey.FromStatusId ).AsIntegerOrNull();

            if ( fromStatusId != null )
            {
                settings.FromConnectionStatusId = fromStatusId;

            }

            // To Status
            var toStatusId = PageParameter( PageParameterKey.ToStatusId ).AsIntegerOrNull();

            if ( toStatusId != null )
            {
                settings.ToConnectionStatusId = toStatusId;

            }
        }

        /// <summary>
        /// Create the report data.
        /// </summary>
        private void LoadReport()
        {
            var settings = GetReportSettingsFromPage();

            // Get an instance of the report builder.
            var dataContext = this.GetDataContext();

            var reportService = new ConnectionStatusChangeReportBuilder( dataContext, settings );

            var report = reportService.CreateReport();

            // Create the report data to be presented.
            _report = new ReportDataViewModel();

            _report.EndDate = report.EndDate;
            _report.StartDate = report.StartDate;

            // Get the set of changes that represent activity during the reporting period.
            _changeEvents = new List<StatusChangeViewModel>();

            var modelsAdded = GetChangeEventViewModels( report.ChangeEvents );

            _changeEvents.AddRange( modelsAdded );

            BindReport();
        }

        private List<StatusChangeViewModel> GetChangeEventViewModels( List<ConnectionStatusChangeEventInfo> personList )
        {
            var vmList = new List<StatusChangeViewModel>();

            foreach ( var info in personList )
            {
                var vm = new StatusChangeViewModel();

                vm.Id = info.PersonId;

                vm.FirstName = info.FirstName;
                vm.LastName = info.LastName;
                vm.NameLastFirst = info.LastName + ", " + info.FirstName;

                vm.PersonId = info.PersonId;

                vm.DateChanged = info.EventDate;

                vm.PhotoUrl = Person.GetPersonPhotoUrl( info.PersonId );

                vm.OriginalStatus = info.OldConnectionStatusName;
                vm.UpdatedStatus = info.NewConnectionStatusName;

                vm.ChangedBy = info.CreatedBy;

                vmList.Add( vm );
            }

            return vmList;
        }

        private void BindReport()
        {
            bool showReport = ( _report != null );

            if ( showReport )
            {
                BindMembershipChangesGrid();
            }
        }

        /// <summary>
        /// Register any JavaScript code required for client-side execution.
        /// </summary>
        protected void RegisterScript()
        {
            RockPage.AddScriptLink( ResolveRockUrl( "~/Scripts/jquery.lazyload.min.js" ) );
        }

        void gChanges_GridRebind( object sender, EventArgs e )
        {
            BindMembershipChangesGrid();
        }

        private void BindMembershipChangesGrid()
        {
            var sortProperty = gChanges.SortProperty;

            List<StatusChangeViewModel> groupMembersList = null;

            if ( sortProperty != null )
            {
                try
                {
                    groupMembersList = _changeEvents.AsQueryable().Sort( sortProperty ).ToList();
                }
                catch ( ArgumentException )
                {
                    // If the sort property is invalid, return the unsorted list.
                    groupMembersList = _changeEvents;
                }
            }
            else
            {
                groupMembersList = _changeEvents;
            }

            gChanges.DataSource = groupMembersList;

            gChanges.DataBind();
        }

        void gChanges_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            var listItem = e.Row.DataItem as StatusChangeViewModel;

            if ( listItem == null )
            {
                return;
            }

            if ( !listItem.IsActive )
            {
                e.Row.AddCssClass( "inactive" );
            }

            if ( listItem.IsDeceased )
            {
                e.Row.AddCssClass( "deceased" );
            }

            var lPerson = e.Row.FindControl( "lPerson" ) as Literal;

            var sbPersonDetails = new StringBuilder();

            sbPersonDetails.Append( string.Format( "<div class=\"photo-round photo-round-sm pull-left\" data-original=\"{0}&w=100\" style=\"background-image: url('{1}');\"></div>", listItem.PhotoUrl, ResolveUrl( "~/Assets/Images/person-no-photo-male.svg" ) ) );
            sbPersonDetails.Append( "<div class=\"pull-left margin-l-sm\">" );
            sbPersonDetails.Append( listItem.NameLastFirst );
            sbPersonDetails.Append( "</div>" );

            lPerson.Text = sbPersonDetails.ToString();
        }

        private void AddSummaryItem( List<SummaryData> result, DateTime dateTime, string seriesName, decimal? value )
        {
            var summaryItem = new SummaryData()
            {
                DateTimeStamp = dateTime.ToJavascriptMilliseconds(),
                DateTime = dateTime,
                SeriesName = seriesName,
                YValue = value
            };

            result.Add( summaryItem );
        }

        /// <summary>
        /// Gets the chart style.
        /// </summary>
        /// <value>
        /// The chart style.
        /// </value>
        private ChartStyle ChartStyle
        {
            get
            {
                Guid? chartStyleDefinedValueGuid = this.GetAttributeValue( "ChartStyle" ).AsGuidOrNull();
                if ( chartStyleDefinedValueGuid.HasValue )
                {
                    var rockContext = new Rock.Data.RockContext();
                    var definedValue = new DefinedValueService( rockContext ).Get( chartStyleDefinedValueGuid.Value );
                    if ( definedValue != null )
                    {
                        try
                        {
                            definedValue.LoadAttributes( rockContext );
                            return ChartStyle.CreateFromJson( definedValue.Value, definedValue.GetAttributeValue( "ChartStyle" ) );
                        }
                        catch
                        {
                            // intentionally ignore and default to basic style
                        }
                    }
                }

                return new ChartStyle();
            }
        }

        protected void lnkSettings_OnServerClick( object sender, EventArgs e )
        {
            _report = null;
            _changeEvents = null;

            var settings = this.GetReportSettingsFromPage();

            var queryParams = GetUrlQueryParametersFromSettings( settings, false );

            this.NavigateToCurrentPage( queryParams );
        }

        protected void gEvents_OnRowSelected( object sender, RowEventArgs e )
        {
            var membership = _changeEvents.FirstOrDefault( x => x.Id == e.RowKeyId );

            if ( membership == null )
            {
                return;
            }

            NavigateToLinkedPage( "PersonDetailPage", "PersonId", membership.PersonId );
        }

        #endregion

        #region Common Code (EntityList)

        /// <summary>
        /// Code in this region can be reused for any block that shows a simple list of entities.
        /// </summary>

        #region Private Variables

        private RockContext _dataContext = null;
        //private bool _blockContextIsValid = false;
        //private ReorderField _reorderColumn = null;
        //private DeleteField _deleteColumn = null;
        //private SecurityField _securityColumn = null;
        //private string _blockTitle = "Entity List";
        //private EntityTypeCache _entityType = null;

        #endregion

        #region Properties

        /// <summary>
        /// Determines if list items flagged as system-protected are prevented from deletion.
        /// If set to True, objects provided as the data source for the list must implement an IsSystem property.
        /// </summary>
        //public bool PreventSystemItemDelete { get; set; }

        /// <summary>
        /// Show a delete action for items in the list?
        /// </summary>
        //public bool ShowItemDelete { get; set; }

        /// <summary>
        /// Show a security action for items in the list?
        /// </summary>
        //public bool ShowItemSecurity { get; set; }

        /// <summary>
        /// Show an add action for the list?
        /// </summary>
        //public bool ShowItemAdd { get; set; }

        /// <summary>
        /// Show reorder handles for items in the list?
        /// </summary>
        //public bool ShowItemReorder { get; set; }

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

        /// <summary>
        /// Retrieve a singleton data context for data operations in this block.
        /// </summary>
        /// <returns></returns>
        protected RockContext GetDataContext()
        {
            if ( _dataContext == null )
            {
                _dataContext = new RockContext();
            }

            return _dataContext;
        }

        //#region Block Notifications and Alerts

        ///// <summary>
        ///// Set up the mechanism for showing block-level notification messages.
        ///// </summary>
        //private void InitializeBlockNotifications()
        //{
        //    // If a notification control has not been provided, find the first one in the block.
        //    if ( StatusNotificationControl == null )
        //    {
        //        StatusNotificationControl = this.ControlsOfTypeRecursive<NotificationBox>().FirstOrDefault();
        //    }

        //    if ( StatusNotificationControl == null )
        //    {
        //        throw new Exception( "NotificationControl not found." );
        //    }

        //    if ( ListPanelControl == null )
        //    {
        //        throw new ArgumentNullException( "ListPanel control not found." );
        //    }

        //    // Verify that the notification control is not a child of the detail container.
        //    // This would cause the notification to be hidden when the content is disallowed.
        //    var invalidParent = StatusNotificationControl.FindFirstParentWhere( x => x.ID == ListPanelControl.ID );

        //    if ( invalidParent != null )
        //    {
        //        throw new Exception( "NotificationControl cannot be a child of DetailContainerControl." );
        //    }

        //    // Set the initial state of the controls.
        //    ResetBlockNotification();
        //}

        ///// <summary>
        ///// Show a notification message for the block.
        ///// </summary>
        ///// <param name="notificationControl"></param>
        ///// <param name="message"></param>
        ///// <param name="notificationType"></param>
        //private void ShowNotification( string message, NotificationBoxType notificationType = NotificationBoxType.Info, bool hideBlockContent = false )
        //{
        //    StatusNotificationControl.Text = message;
        //    StatusNotificationControl.NotificationBoxType = notificationType;

        //    StatusNotificationControl.Visible = true;
        //    ListPanelControl.Visible = !hideBlockContent;
        //}

        ///// <summary>
        ///// Reset the notification message for the block.
        ///// </summary>
        //private void ResetBlockNotification()
        //{
        //    StatusNotificationControl.Visible = false;
        //    ListPanelControl.Visible = true;
        //}

        ///// <summary>
        ///// Show a block-level error notification.
        ///// </summary>
        ///// <param name="message"></param>
        //private void ShowNotificationError( string message )
        //{
        //    ShowNotification( message, NotificationBoxType.Danger );
        //}

        ///// <summary>
        ///// Show a block-level exception notification. 
        ///// </summary>
        ///// <param name="ex"></param>
        ///// <param name="writeToLog"></param>
        //private void ShowNotificationException( Exception ex )
        //{
        //    ShowNotification( ex.Message, NotificationBoxType.Danger );

        //    if ( this.LogExceptionNotifications )
        //    {
        //        LogException( ex );
        //    }
        //}

        ///// <summary>
        ///// Show a block-level success notification. 
        ///// </summary>
        ///// <param name="ex"></param>
        ///// <param name="writeToLog"></param>
        //private void ShowNotificationSuccess( string message )
        //{
        //    ShowNotification( message, NotificationBoxType.Success );
        //}

        ///// <summary>
        ///// Show a fatal error that prevents the block content from being displayed.
        ///// </summary>
        ///// <param name="message"></param>
        //private void ShowNotificationFatalError( string message )
        //{
        //    ShowNotification( message, NotificationBoxType.Danger, true );
        //}

        ///// <summary>
        ///// Show a fatal error indicating that the user does not have permision to access this content.
        ///// </summary>
        //private void ShowNotificationViewUnauthorized()
        //{
        //    ShowNotification( "Sorry, you are not authorized to view this content.", NotificationBoxType.Danger, true );
        //}

        ///// <summary>
        ///// Show a fatal error indicating that there is no content available in this block for the current context settings.
        ///// </summary>
        //private void ShowNotificationEmptyContent()
        //{
        //    ShowNotification( "There is no content to show in this context.", NotificationBoxType.Info, true );
        //}

        ///// <summary>
        ///// Show a notification that edit mode is not allowed.
        ///// </summary>
        ///// <param name="itemFriendlyName"></param>
        //private void ShowNotificationEditModeDisallowed()
        //{
        //    ShowNotification( EditModeMessage.ReadOnlyEditActionNotAllowed( _entityType.FriendlyName ), NotificationBoxType.Info, false );
        //}

        ///// <summary>
        ///// Show an alert message that requires user acknowledgement to continue.
        ///// </summary>
        ///// <param name="message"></param>
        ///// <param name="alertType"></param>
        //private void ShowAlert( string message, ModalAlertType alertType )
        //{
        //    ModalAlertControl.Show( message, alertType );
        //}

        ///// <summary>
        ///// Show an informational alert message that requires user acknowledgement to continue.
        ///// </summary>
        ///// <param name="message"></param>
        ///// <param name="alertType"></param>
        //private void ShowAlert( string message )
        //{
        //    ModalAlertControl.Show( message, ModalAlertType.Information );
        //}

        //#endregion

        #endregion

        #region Support Classes

        /// <summary>
        /// A summary of a connection status change event.
        /// </summary>
        [Serializable]
        public class StatusChangeViewModel
        {
            public int Id { get; set; }

            public int PersonId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string NameLastFirst { get; set; }
            public string OriginalStatus { get; set; }
            public string UpdatedStatus { get; set; }
            public string PhotoUrl { get; set; }
            public bool IsActive { get; set; }
            public bool IsDeceased { get; set; }

            public DateTime? DateChanged { get; set; }
            public string ChangedBy { get; set; }

            public override string ToString()
            {
                return string.Format( "{0} {1} [{2}:{3} --> {4}]", FirstName, LastName, DateChanged, OriginalStatus, UpdatedStatus );
            }
        }

        /// <summary>
        /// A model containing the data needed for presenting the CampusMembershipActivityReport page.
        /// </summary>
        [Serializable]
        public class ReportDataViewModel
        {
            public string SettingsDescription { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string PeriodDescription { get; set; }
        }

        #endregion
    }
}
