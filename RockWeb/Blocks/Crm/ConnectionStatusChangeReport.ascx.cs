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
        Key = AttributeKey.PersonDetailPage,
        Category = AttributeCategory.LinkedPages,
        Order = 0 )]

    #endregion

    /// <summary>
    /// Shows changes of Connection Status for people within a specific period.
    /// </summary>
    [Rock.SystemGuid.BlockTypeGuid( "FE50DDE5-3D8C-47EC-817D-21348717AD38" )]
    public partial class ConnectionStatusChangeReport : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string PersonDetailPage = "PersonDetailPage";
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
            public const string CampusId = "CampusId";
            public const string Period = "Period";
            public const string FromStatusId = "FromStatusId";
            public const string ToStatusId = "ToStatusId";
            public const string ShowResults = "ShowResults";
        }

        #endregion

        #region Constants

        private const string _urlValuesListDelimiter = ",";
        private const int _MaxRecords = 100000;

        #endregion

        #region Private Variables

        private bool _showResults;
        private ReportDataViewModel _report;
        private List<StatusChangeViewModel> _changeEvents;

        #endregion

        #region Public Properties and Methods

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            _report = ViewState["Report"] as ReportDataViewModel;

            _changeEvents = ViewState["Changes"] as List<StatusChangeViewModel>;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.RegisterScript();

            this.ListPanelControl = pnlResults;

            this.InitializeGrid();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
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

            base.OnLoad( e );
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["Report"] = _report;
            ViewState["Changes"] = _changeEvents;

            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbApplyFilter button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
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
                 || settings.ReportPeriod.TimeUnit != TimePeriodUnitSpecifier.Year )
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

                var eventCount = _changeEvents.Count();

                if ( _changeEvents != null
                     && eventCount > _MaxRecords )
                {
                    _changeEvents.Clear();

                    nbNotice.NotificationBoxType = NotificationBoxType.Danger;
                    nbNotice.Title = "Report Failed.";
                    nbNotice.Text = string.Format( "The result set is too large ({0:#,###} records). Retry the report with a more restrictive filter.", eventCount );

                    nbNotice.Visible = true;

                    _showResults = false;
                }
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
        }

        /// <summary>
        /// Create the view models for the status change list.
        /// </summary>
        /// <param name="personList"></param>
        /// <returns></returns>
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

                /*
                 * Calculate the Age Classification here to prevent the database query that will otherwise be made when the PhotoUrl is retrieved.
                */
                AgeClassification ageClassification;

                if ( info.Age.HasValue
                     && info.Age.Value < 18 )
                {
                    ageClassification = AgeClassification.Child;
                }
                else
                {
                    ageClassification = AgeClassification.Adult;
                }

                vm.PhotoUrl = Person.GetPersonPhotoUrl( info.Initials, info.PhotoId, info.Age, info.Gender, info.RecordTypeValueId, ageClassification );

                vm.OriginalStatus = info.OldConnectionStatusName;
                vm.UpdatedStatus = info.NewConnectionStatusName;

                vm.ChangedBy = info.CreatedBy;

                vmList.Add( vm );
            }

            return vmList;
        }

        /// <summary>
        /// Bind the report data source to the page controls.
        /// </summary>
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

        /// <summary>
        /// Handles the GridRebind event of the gChanges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        void gChanges_GridRebind( object sender, EventArgs e )
        {
            BindMembershipChangesGrid();
        }

        /// <summary>
        /// Bind the data source to the membership changes grid control.
        /// </summary>
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

        /// <summary>
        /// Handles the RowDataBound event of the gChanges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles the Click event of the lnkSettings button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lnkSettings_OnServerClick( object sender, EventArgs e )
        {
            _report = null;
            _changeEvents = null;

            var settings = this.GetReportSettingsFromPage();

            var queryParams = GetUrlQueryParametersFromSettings( settings, false );

            this.NavigateToCurrentPage( queryParams );
        }

        /// <summary>
        /// Handles the OnRowSelected event of the lnkSettings button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gEvents_OnRowSelected( object sender, RowEventArgs e )
        {
            var membership = _changeEvents.FirstOrDefault( x => x.Id == e.RowKeyId );

            if ( membership == null )
            {
                return;
            }

            NavigateToLinkedPage( AttributeKey.PersonDetailPage, "PersonId", membership.PersonId );
        }

        #endregion

        #region Common Code (EntityList)

        /// <summary>
        /// Code in this region can be reused for any block that shows a simple list of entities.
        /// </summary>

        #region Private Variables

        private RockContext _dataContext = null;

        #endregion

        #region Properties

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
        /// A model containing the data needed for presenting the ConnectionStatusChangeReport page.
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