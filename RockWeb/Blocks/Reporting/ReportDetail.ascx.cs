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

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// Displays the details of the given report.
    /// </summary>
    /// <seealso cref="Rock.Web.UI.RockBlock" />
    [DisplayName( "Report Detail" )]
    [Category( "Reporting" )]
    [Description( "Displays the details of the given report." )]

    [BooleanField( "Add Administrate Security to Item Creator",
        Key = AttributeKey.AddAdministrateSecurityToItemCreator,
        Description = "If enabled, the person who creates a new item will be granted 'Administrate' and 'Edit' security rights to the item.  This was the behavior in previous versions of Rock.  If disabled, the item creator will not be able to edit the report, its security settings, or possibly perform other functions without the Rock administrator settings up a role that is allowed to perform such functions.",
        DefaultBooleanValue = false,
        Order = 0 )]

    [IntegerField( "Database Timeout",
        Key = AttributeKey.DatabaseTimeout,
        Description = "The number of seconds to wait before reporting a database timeout.",
        IsRequired = false,
        DefaultIntegerValue = 180,
        Order = 1 )]

    [LinkedPage( "Data View Page",
        Key = AttributeKey.DataViewPage,
        Description = "The page to edit data views",
        IsRequired = true,
        Order = 2 )]

    [ReportField( "Report",
        Key = AttributeKey.Report,
        Description = "Select the report to present to the user.",
        IsRequired = false,
        Order = 3 )]

    [Rock.SystemGuid.BlockTypeGuid( "E431DBDF-5C65-45DC-ADC5-157A02045CCD" )]
    public partial class ReportDetail : RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string AddAdministrateSecurityToItemCreator = "AddAdministrateSecurityToItemCreator";
            public const string DatabaseTimeout = "DatabaseTimeout";
            public const string DataViewPage = "DataViewPage";
            public const string Report = "Report";
        }

        #endregion Attribute Keys

        #region PageParameterKey

        private static class PageParameterKey
        {
            public const string ParentCategoryId = "ParentCategoryId";
            public const string ReportId = "ReportId";
            public const string DataViewId = "DataViewId";
        }

        #endregion PageParameterKey

        #region Properties

        /// <summary>
        /// Gets or sets the report fields dictionary.
        /// </summary>
        /// <value>
        /// The report fields dictionary.
        /// </value>
        protected List<ReportFieldInfo> ReportFieldsDictionary
        {
            get
            {
                List<ReportFieldInfo> reportFieldsDictionary = ViewState["ReportFieldsDictionary"] as List<ReportFieldInfo>;
                return reportFieldsDictionary;
            }

            set
            {
                ViewState["ReportFieldsDictionary"] = value;
            }
        }

        private const string _ViewStateKeyShowResults = "ShowResults";
        private const string SHOW_RESULTS_PREFERENCE = "show-results";

        protected bool ShowResults
        {
            get
            {
                return ViewState[_ViewStateKeyShowResults].ToStringSafe().AsBoolean();
            }

            set
            {
                if ( this.ShowResults != value )
                {
                    ViewState[_ViewStateKeyShowResults] = value;

                    var preferences = GetBlockPersonPreferences();
                    preferences.SetValue( SHOW_RESULTS_PREFERENCE, value.ToString() );
                    preferences.Save();
                }

                pnlResultsGrid.Visible = this.ShowResults;

                if ( this.ShowResults )
                {
                    btnToggleResults.Text = "Hide Results <i class='fa fa-chevron-up'></i>";
                    btnToggleResults.ToolTip = "Hide Results";
                }
                else
                {
                    btnToggleResults.Text = "Show Results <i class='fa fa-chevron-down'></i>";
                    btnToggleResults.ToolTip = "Show Results";
                }

                if ( !this.ShowResults )
                {
                    return;
                }

                // Run the Report and show the results.
                var reportServiceReadOnly = new ReportService( new RockContextReadOnly() );

                var reportReadOnly = reportServiceReadOnly.Get( hfReportId.Value.AsInteger() );

                if ( reportReadOnly == null )
                {
                    return;
                }

                BindGrid( reportReadOnly, false );
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gReport.GridRebind += gReport_GridRebind;
            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", Report.FriendlyTypeName );
            btnSecurity.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Report ) ).Id;

            //// Set postback timeout and request-timeout to whatever the DatabaseTimeout is plus an extra 5 seconds so that page doesn't timeout before the database does
            int databaseTimeout = GetAttributeValue( AttributeKey.DatabaseTimeout ).AsIntegerOrNull() ?? 180;
            var sm = ScriptManager.GetCurrent( this.Page );
            if ( sm.AsyncPostBackTimeout < databaseTimeout + 5 )
            {
                sm.AsyncPostBackTimeout = databaseTimeout + 5;
                Server.ScriptTimeout = databaseTimeout + 5;
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
                var preferences = GetBlockPersonPreferences();

                ShowResults = preferences.GetValue( SHOW_RESULTS_PREFERENCE ).AsBoolean( true );

                var reportId = GetReportId();
                if ( reportId.HasValue )
                {
                    ShowDetail( reportId.Value, PageParameter( PageParameterKey.ParentCategoryId ).AsIntegerOrNull() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }

            if ( pnlEditDetails.Visible )
            {
                var rockContext = new RockContext();

                foreach ( var field in ReportFieldsDictionary )
                {
                    AddFieldPanelWidget( field.Guid, field.ReportFieldType, field.FieldSelection, true, rockContext );
                }
            }

            // handle sort events
            string postbackArgs = Request.Params["__EVENTARGUMENT"];
            if ( !string.IsNullOrWhiteSpace( postbackArgs ) )
            {
                string[] nameValue = postbackArgs.Split( new char[] { ':' } );
                if ( nameValue.Count() == 2 )
                {
                    string eventParam = nameValue[0];
                    if ( eventParam.Equals( "re-order-panel-widget" ) )
                    {
                        string[] values = nameValue[1].Split( new char[] { ';' } );
                        if ( values.Count() == 2 )
                        {
                            SortPanelWidgets( eventParam, values );
                        }
                    }
                }
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            if ( pnlEditDetails.Visible )
            {
                ConfigureKeyValueControls();
            }
        }

        /// <summary>
        /// Configures the key value controls (SortFields, MergeFields, CommunicationRecipientFields) based on the currently selected field widgets
        /// </summary>
        private void ConfigureKeyValueControls()
        {
            // rebuild the CustomKeys list based on the current field titles
            kvSortFields.CustomKeys = new Dictionary<string, string>();
            vMergeFields.CustomValues = new Dictionary<string, string>();
            vRecipientFields.CustomValues = new Dictionary<string, string>();

            foreach ( var panelWidget in phReportFields.ControlsOfTypeRecursive<PanelWidget>() )
            {
                // default support sorting to true, unless this is a DataSelectComponent and DataSelectComponent doesn't support sorting
                bool supportsSorting = true;

                // default support recipients to false, unless this is a DataSelectComponent and DataSelectComponent supports recipients
                bool supportsRecipients = false;

                try
                {
                    string ddlFieldsId = panelWidget.ID + "_ddlFields";
                    RockDropDownList ddlFields = phReportFields.ControlsOfTypeRecursive<RockDropDownList>().First( a => a.ID == ddlFieldsId );
                    ddlFields.EnhanceForLongLists = true;
                    var fieldTypeSelection = GetSelectedFieldTypeSelection( ddlFields );
                    if ( fieldTypeSelection != null )
                    {
                        if ( fieldTypeSelection.ReportFieldType == ReportFieldType.DataSelectComponent )
                        {
                            var entityTypeId = fieldTypeSelection.FieldSelection.AsIntegerOrNull();
                            if ( entityTypeId.HasValue )
                            {
                                var dataSelectComponent = this.GetDataSelectComponent( new RockContext(), entityTypeId.Value );
                                if ( dataSelectComponent != null )
                                {
                                    if ( dataSelectComponent.SortProperties( string.Empty ) == string.Empty )
                                    {
                                        supportsSorting = false;
                                    }

                                    var fieldType = dataSelectComponent.ColumnFieldType;
                                    if ( dataSelectComponent is IRecipientDataSelect )
                                    {
                                        fieldType = ( ( IRecipientDataSelect ) dataSelectComponent ).RecipientColumnFieldType;
                                    }

                                    supportsRecipients = fieldType.Equals( typeof( int ) ) || fieldType.Equals( typeof( IEnumerable<int> ) );
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // if an exception occurred, ignore and assume it supports sorting and doesn't support recipients
                    supportsSorting = true;
                    supportsRecipients = false;
                }

                Guid reportFieldGuid = panelWidget.ID.Replace( "reportFieldWidget_", string.Empty ).AsGuid();
                if ( supportsSorting )
                {
                    kvSortFields.CustomKeys.Add( reportFieldGuid.ToString(), panelWidget.Title );
                }

                vMergeFields.CustomValues.Add( reportFieldGuid.ToString(), panelWidget.Title );

                if ( supportsRecipients )
                {
                    vRecipientFields.CustomValues.Add( reportFieldGuid.ToString(), panelWidget.Title );
                }
            }

            kvSortFields.Enabled = kvSortFields.CustomKeys.Any();
            vMergeFields.Enabled = vMergeFields.CustomValues.Any();
            vRecipientFields.Enabled = vRecipientFields.CustomValues.Any();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the etpEntityType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void etpEntityType_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateControlsForEntityType( etpEntityType.SelectedEntityTypeId );
        }

        /// <summary>
        /// Handles the GridRebind event of the gReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gReport_GridRebind( object sender, GridRebindEventArgs e )
        {
            var reportReadOnly = new ReportService( new RockContextReadOnly() ).Get( hfReportId.ValueAsInt() );
            BindGrid( reportReadOnly, e.IsCommunication );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid( Report report, bool isCommunication )
        {
            if ( !this.ShowResults )
            {
                gReport.DataSource = null;

                return;
            }

            int? databaseTimeoutSeconds = GetAttributeValue( AttributeKey.DatabaseTimeout ).AsIntegerOrNull();
            ReportingHelper.BindGridOptions bindGridOptions = new ReportingHelper.BindGridOptions()
            {
                CurrentPerson = this.CurrentPerson,
                DatabaseTimeoutSeconds = databaseTimeoutSeconds,
                IsCommunication = isCommunication
            };

            nbErrorMessage.Visible = false;

            try
            {
                /*

                    11/30/2022 - CWR
                    If there is a dataview for the report, use its DbContext.  This null check is necessary because a report does not require a dataview.
                    
                 */
                if ( report.DataView != null )
                {
                    bindGridOptions.ReportDbContext = report.DataView.GetDbContext();
                }
                else
                {
                    if ( report.EntityTypeId.HasValue )
                    {
                        var entityType = EntityTypeCache.Get( report.EntityTypeId.Value );
                        if ( entityType != null )
                        {
                            var contextForEntityType = Reflection.GetDbContextForEntityType( entityType.GetEntityType() );

                            // If the DbContext for the entity type returns a standard RockContext, proceed with the ReadOnly context here.
                            // If it returns other than a RockContext, use that DbContext for the bindGridOptions.
                            if ( contextForEntityType.GetType() == typeof( Rock.Data.RockContext ) )
                            {
                                bindGridOptions.ReportDbContext = new RockContextReadOnly();
                            }
                            else
                            {
                                bindGridOptions.ReportDbContext = contextForEntityType;
                            }
                        }
                    }
                }

                ReportingHelper.BindGrid( report, gReport, bindGridOptions );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                var sqlTimeoutException = ReportingHelper.FindSqlTimeoutException( ex );

                if ( sqlTimeoutException != null )
                {
                    nbErrorMessage.NotificationBoxType = NotificationBoxType.Warning;
                    nbErrorMessage.Text = "This report did not complete in a timely manner. You can try again or adjust the timeout setting of this block.";
                }
                else
                {
                    if ( ex is RockDataViewFilterExpressionException )
                    {
                        RockDataViewFilterExpressionException rockDataViewFilterExpressionException = ex as RockDataViewFilterExpressionException;
                        nbErrorMessage.Text = rockDataViewFilterExpressionException.GetFriendlyMessage( ( IDataViewDefinition ) report.DataView );
                    }
                    else
                    {
                        nbErrorMessage.Text = "There was a problem with one of the filters for this report's dataview.";
                    }

                    nbErrorMessage.NotificationBoxType = NotificationBoxType.Danger;

                    nbErrorMessage.Details = ex.Message;
                    nbErrorMessage.Visible = true;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnAddField control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddField_Click( object sender, EventArgs e )
        {
            Guid reportFieldGuid = Guid.NewGuid();
            ReportFieldType reportFieldType = ReportFieldType.Property;
            string fieldSelection = string.Empty;
            ReportFieldsDictionary.Add( new ReportFieldInfo { Guid = reportFieldGuid, ReportFieldType = reportFieldType, FieldSelection = fieldSelection } );
            AddFieldPanelWidget( reportFieldGuid, reportFieldType, fieldSelection, true, new RockContext(), true, new ReportField { ShowInGrid = true } );
            kvSortFields.CustomKeys.Add( reportFieldGuid.ToString(), "(untitled)" );
            vMergeFields.CustomValues.Add( reportFieldGuid.ToString(), "(untitled)" );
            vRecipientFields.CustomValues.Add( reportFieldGuid.ToString(), "(untitled)" );
        }

        /// <summary>
        /// Handles the DeleteClick event of the FieldsPanelWidget control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void FieldsPanelWidget_DeleteClick( object sender, EventArgs e )
        {
            PanelWidget panelWidget = sender as PanelWidget;
            if ( panelWidget != null )
            {
                Guid reportFieldGuid = panelWidget.ID.Replace( "reportFieldWidget_", string.Empty ).AsGuid();
                phReportFields.Controls.Remove( panelWidget );
                var reportFieldInfo = ReportFieldsDictionary.First( a => a.Guid == reportFieldGuid );
                ReportFieldsDictionary.Remove( reportFieldInfo );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnToggleResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnToggleResults_Click( object sender, EventArgs e )
        {
            this.ShowResults = !this.ShowResults;
        }

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var item = new ReportService( new RockContext() ).Get( int.Parse( hfReportId.Value ) );
            ShowEditDetails( item );
        }

        /// <summary>
        /// Handles the Click event of the Copy button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCopy_Click( object sender, EventArgs e )
        {
            // Create a new Report using the current item as a template.
            var id = int.Parse( hfReportId.Value );

            var reportService = new ReportService( new RockContext() );

            var newItem = reportService.GetNewFromTemplate( id );

            if ( newItem == null )
            {
                return;
            }

            newItem.Name += " (Copy)";

            // Reset the stored identifier for the active Report.
            hfReportId.Value = "0";

            ShowEditDetails( newItem );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            int? categoryId = null;
            var rockContext = new RockContext();
            var reportService = new ReportService( rockContext );
            var report = reportService.Get( hfReportId.Value.AsInteger() );

            if ( report != null )
            {
                string errorMessage;
                if ( !reportService.CanDelete( report, out errorMessage ) )
                {
                    ShowReadonlyDetails( report );
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }
                else
                {
                    categoryId = report.CategoryId;

                    reportService.Delete( report );
                    rockContext.SaveChanges();

                    // reload page, selecting the deleted data view's parent
                    var qryParams = new Dictionary<string, string>();
                    if ( categoryId != null )
                    {
                        qryParams["CategoryId"] = categoryId.ToString();
                    }

                    NavigateToPage( RockPage.Guid, qryParams );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            cvSecurityError.IsValid = true;
            var addAdminToCreator = GetAttributeValue( AttributeKey.AddAdministrateSecurityToItemCreator ).AsBoolean();
            var rptCategory = CategoryCache.Get( cpCategory.SelectedValueAsInt( false ).Value );

            if ( !addAdminToCreator && !rptCategory.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                cvSecurityError.IsValid = false;
                cvSecurityError.ErrorMessage = string.Format( "You are not authorized to create or edit Reports for the Category '{0}'.", rptCategory.Name );
                return;
            }

            Report report = null;

            var rockContext = new RockContext();
            ReportService service = new ReportService( rockContext );
            ReportFieldService reportFieldService = new ReportFieldService( rockContext );

            int reportId = int.Parse( hfReportId.Value );

            if ( reportId == 0 )
            {
                report = new Report();
                report.IsSystem = false;
            }
            else
            {
                report = service.Get( reportId );
            }

            report.Name = tbName.Text;
            report.Description = tbDescription.Text;
            report.CategoryId = cpCategory.SelectedValueAsInt();
            report.EntityTypeId = etpEntityType.SelectedEntityTypeId;
            report.DataViewId = dvpDataView.SelectedValueAsInt();
            report.FetchTop = nbFetchTop.Text.AsIntegerOrNull();
            report.QueryHint = tbQueryHint.Text;

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !report.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            // delete all the reportFields so we can cleanly add them
            foreach ( var reportField in report.ReportFields.ToList() )
            {
                var field = reportFieldService.Get( reportField.Guid );
                reportFieldService.Delete( field );
            }

            report.ReportFields.Clear();

            var allPanelWidgets = phReportFields.ControlsOfTypeRecursive<PanelWidget>();
            int columnOrder = 0;

            var mergeFields = vMergeFields.Value.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList();
            var recipientFields = vRecipientFields.Value.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList();

            foreach ( var panelWidget in allPanelWidgets )
            {
                string ddlFieldsId = panelWidget.ID + "_ddlFields";
                RockDropDownList ddlFields = phReportFields.ControlsOfTypeRecursive<RockDropDownList>().First( a => a.ID == ddlFieldsId );
                ReportFieldType reportFieldType = ReportFieldType.Property;
                string fieldSelection = string.Empty;
                var fieldTypeSelection = GetSelectedFieldTypeSelection( ddlFields );

                if ( fieldTypeSelection != null )
                {
                    reportFieldType = fieldTypeSelection.ReportFieldType;
                    fieldSelection = fieldTypeSelection.FieldSelection;
                }
                else
                {
                    // skip over fields that have nothing selected in ddlFields
                    continue;
                }

                ReportField reportField = new ReportField();
                reportField.ReportFieldType = reportFieldType;

                string showInGridCheckBoxId = string.Format( "{0}_showInGridCheckBox", panelWidget.ID );
                RockCheckBox showInGridCheckBox = phReportFields.ControlsOfTypeRecursive<RockCheckBox>().First( a => a.ID == showInGridCheckBoxId );
                reportField.ShowInGrid = showInGridCheckBox.Checked;

                string columnHeaderTextTextBoxId = string.Format( "{0}_columnHeaderTextTextBox", panelWidget.ID );
                RockTextBox columnHeaderTextTextBox = phReportFields.ControlsOfTypeRecursive<RockTextBox>().First( a => a.ID == columnHeaderTextTextBoxId );
                reportField.ColumnHeaderText = columnHeaderTextTextBox.Text;

                reportField.ColumnOrder = columnOrder++;

                if ( reportFieldType == ReportFieldType.DataSelectComponent )
                {
                    reportField.DataSelectComponentEntityTypeId = fieldSelection.AsIntegerOrNull();

                    string placeHolderId = string.Format( "{0}_phDataSelectControls", panelWidget.ID );
                    var placeHolder = phReportFields.ControlsOfTypeRecursive<PlaceHolder>().FirstOrDefault( a => a.ID == placeHolderId );

                    var dataSelectComponent = this.GetDataSelectComponent( rockContext, reportField.DataSelectComponentEntityTypeId ?? 0 );
                    if ( dataSelectComponent != null )
                    {
                        reportField.Selection = dataSelectComponent.GetSelection( placeHolder.Controls.OfType<Control>().ToArray() );
                    }
                }
                else if ( reportFieldType == ReportFieldType.Attribute )
                {
                    string placeHolderId = string.Format( "{0}_phDataSelectControls", panelWidget.ID );
                    var placeHolder = phReportFields.ControlsOfTypeRecursive<PlaceHolder>().FirstOrDefault( a => a.ID == placeHolderId );

                    var info = GetAttributeFieldInfoFromControls( panelWidget, fieldSelection, rockContext );
                    reportField.Selection = info.SerializeConfiguration();
                }
                else
                {
                    reportField.Selection = fieldSelection;
                }

                reportField.Guid = panelWidget.ID.Replace( "reportFieldWidget_", string.Empty ).AsGuid();

                reportField.IsCommunicationMergeField = mergeFields.Contains( reportField.Guid );
                reportField.IsCommunicationRecipientField = recipientFields.Contains( reportField.Guid );

                report.ReportFields.Add( reportField );
            }

            int sortOrder = 0;
            foreach ( var itemPair in kvSortFields.Value.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).Select( a => a.Split( '^' ) ) )
            {
                var reportFieldGuid = itemPair[0].AsGuidOrNull();
                var sortDirection = itemPair[1].ConvertToEnum<SortDirection>( SortDirection.Ascending );
                var reportField = report.ReportFields.FirstOrDefault( a => a.Guid == reportFieldGuid );
                if ( reportField != null )
                {
                    reportField.SortOrder = sortOrder++;
                    reportField.SortDirection = sortDirection;
                }
            }

            var adding = report.Id.Equals( 0 );
            if ( adding )
            {
                service.Add( report );
            }

            rockContext.SaveChanges();

            if ( adding && GetAttributeValue( AttributeKey.AddAdministrateSecurityToItemCreator ).AsBoolean() )
            {
                Rock.Security.Authorization.AllowPerson( report, Authorization.EDIT, this.CurrentPerson, rockContext );
                Rock.Security.Authorization.AllowPerson( report, Authorization.ADMINISTRATE, this.CurrentPerson, rockContext );
            }

            var qryParams = new Dictionary<string, string>();
            qryParams["ReportId"] = report.Id.ToString();
            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Gets the selected field type select.
        /// </summary>
        /// <param name="ddlFields">The DDL fields.</param>
        /// <returns></returns>
        private FieldTypeSelection GetSelectedFieldTypeSelection( RockDropDownList ddlFields )
        {
            ReportFieldType reportFieldType = ReportFieldType.Property;
            string fieldSelection = string.Empty;

            string fieldSelectionValue = ddlFields.SelectedItem.Value;
            string[] fieldSelectionValueParts = fieldSelectionValue.Split( '|' );
            if ( fieldSelectionValueParts.Count() == 2 )
            {
                reportFieldType = fieldSelectionValueParts[0].ConvertToEnum<ReportFieldType>();
                fieldSelection = fieldSelectionValueParts[1];
                return new FieldTypeSelection { ReportFieldType = reportFieldType, FieldSelection = fieldSelection };
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///
        /// </summary>
        private class FieldTypeSelection
        {
            /// <summary>
            /// Gets or sets the type of the report field.
            /// </summary>
            /// <value>
            /// The type of the report field.
            /// </value>
            public ReportFieldType ReportFieldType { get; set; }

            /// <summary>
            /// Gets or sets the field selection.
            /// </summary>
            /// <value>
            /// The field selection.
            /// </value>
            public string FieldSelection { get; set; }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            // Check if we are editing an existing Report.
            int reportId = hfReportId.Value.AsInteger();

            if ( reportId == 0 )
            {
                // If not, check if we are editing a new copy of an existing Report.
                reportId = PageParameter( PageParameterKey.ReportId ).AsInteger();
            }

            if ( reportId == 0 )
            {
                int? parentCategoryId = PageParameter( PageParameterKey.ParentCategoryId ).AsIntegerOrNull();
                if ( parentCategoryId.HasValue )
                {
                    // Cancelling on Add, and we know the parentCategoryId, so we are probably in treeview mode, so navigate to the current page
                    var qryParams = new Dictionary<string, string>();
                    qryParams["CategoryId"] = parentCategoryId.ToString();
                    NavigateToPage( RockPage.Guid, qryParams );
                }
                else
                {
                    // Cancelling on Add.  Return to Grid
                    NavigateToParentPage();
                }
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                ReportService serviceReadOnly = new ReportService( new RockContextReadOnly() );
                Report itemReadOnly = serviceReadOnly.Get( reportId );
                ShowReadonlyDetails( itemReadOnly );
            }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Get the report through query list or block setting.
        /// </summary>
        public int? GetReportId()
        {
            int? reportId = PageParameter( PageParameterKey.ReportId ).AsIntegerOrNull();
            var reportGuid = GetAttributeValue( AttributeKey.Report ).AsGuidOrNull();

            if ( reportGuid.HasValue )
            {
                var reportReadOnly = new ReportService( new RockContextReadOnly() ).Get( reportGuid.Value );
                if ( reportReadOnly != null )
                {
                    reportId = reportReadOnly.Id;
                }
            }

            return reportId;
        }

        /// <summary>
        /// Sorts the panel widgets.
        /// </summary>
        /// <param name="eventParam">The event parameter.</param>
        /// <param name="values">The values.</param>
        private void SortPanelWidgets( string eventParam, string[] values )
        {
            var allPanelWidgets = phReportFields.ControlsOfTypeRecursive<PanelWidget>();
            string panelWidgetClientId = values[0];
            int newIndex = int.Parse( values[1] );

            PanelWidget panelWidget = allPanelWidgets.FirstOrDefault( a => a.ClientID == panelWidgetClientId );
            Guid reportFieldGuid = panelWidget.ID.Replace( "reportFieldWidget_", string.Empty ).AsGuid();
            if ( panelWidget != null )
            {
                phReportFields.Controls.Remove( panelWidget );
                var reportFieldInfo = ReportFieldsDictionary.Where( a => a.Guid == reportFieldGuid ).First();
                ReportFieldsDictionary.Remove( reportFieldInfo );
                if ( newIndex >= allPanelWidgets.Count() )
                {
                    phReportFields.Controls.Add( panelWidget );
                    ReportFieldsDictionary.Add( reportFieldInfo );
                }
                else
                {
                    phReportFields.Controls.AddAt( newIndex, panelWidget );
                    ReportFieldsDictionary.Insert( newIndex, reportFieldInfo );
                }
            }
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            var rockContext = new RockContext();
            etpEntityType.EntityTypes = new EntityTypeService( rockContext )
                .GetReportableEntities( this.CurrentPerson )
                .OrderBy( t => t.FriendlyName ).ToList();
        }

        /// <summary>
        /// Updates UI controls based on the selected EntityType
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        private void UpdateControlsForEntityType( int? entityTypeId )
        {
            // If the DataView picker already has a value, and it's not the same type as the entity type ID.
            if ( dvpDataView.SelectedValueAsId().HasValue && dvpDataView.EntityTypeId != entityTypeId )
            {
                dvpDataView.SetValue( null );
            }

            dvpDataView.EntityTypeId = entityTypeId;
            dvpDataView.Enabled = entityTypeId.HasValue;
            btnAddField.Enabled = entityTypeId.HasValue;
        }

        /// <summary>
        /// Loads the fields drop down.
        /// </summary>
        /// <param name="ddlFields">The DDL fields.</param>
        private void LoadFieldsDropDown( RockDropDownList ddlFields )
        {
            int? entityTypeId = etpEntityType.SelectedEntityTypeId;
            var rockContext = new RockContext();

            if ( entityTypeId.HasValue )
            {
                Type entityType = EntityTypeCache.Get( entityTypeId.Value, rockContext ).GetEntityType();
                var entityFields = Rock.Reporting.EntityHelper.GetEntityFields( entityType, true, false );
                ddlFields.Items.Clear();

                var listItems = new List<ListItem>();

                // Add Fields for the EntityType
                foreach ( var entityField in entityFields.OrderBy( a => !a.IsPreviewable ).ThenBy( a => a.FieldKind != FieldKind.Property ).ThenBy( a => a.Title ) )
                {
                    bool isAuthorizedForField = true;
                    var listItem = new ListItem();
                    listItem.Text = entityField.Title;

                    if ( entityField.FieldKind == FieldKind.Property )
                    {
                        listItem.Value = string.Format( "{0}|{1}", ReportFieldType.Property, entityField.Name );
                    }
                    else if ( entityField.FieldKind == FieldKind.Attribute )
                    {
                        if ( entityField.AttributeGuid.HasValue )
                        {
                            var attribute = AttributeCache.Get( entityField.AttributeGuid.Value );
                            if ( attribute != null )
                            {
                                // only show the Attribute field in the drop down if they have VIEW Auth to it
                                isAuthorizedForField = attribute.IsAuthorized( Rock.Security.Authorization.VIEW, this.CurrentPerson );
                            }
                        }

                        listItem.Value = GetAttributeFieldListKey( entityField.AttributeGuid );
                    }

                    if ( entityField.IsPreviewable )
                    {
                        listItem.Attributes["optiongroup"] = "Common";
                    }
                    else if ( entityField.FieldKind == FieldKind.Attribute )
                    {
                        listItem.Attributes["optiongroup"] = string.Format( "{0} Attributes", entityType.Name );
                    }
                    else
                    {
                        listItem.Attributes["optiongroup"] = string.Format( "{0} Fields", entityType.Name );
                    }

                    if ( entityField.FieldKind == FieldKind.Attribute && entityField.AttributeGuid.HasValue )
                    {
                        var attribute = AttributeCache.Get( entityField.AttributeGuid.Value );
                        if ( attribute != null )
                        {
                            listItem.Attributes.Add( "title", attribute.Description );
                        }
                    }

                    if ( isAuthorizedForField )
                    {
                        listItems.Add( listItem );
                    }
                }

                // Add DataSelect MEF Components that apply to this EntityType
                foreach ( var component in DataSelectContainer.GetComponentsBySelectedEntityTypeName( entityType.FullName ).OrderBy( c => c.Order ).ThenBy( c => c.GetTitle( entityType ) ) )
                {
                    if ( component.IsAuthorized( Authorization.VIEW, this.RockPage.CurrentPerson ) )
                    {
                        var selectEntityType = EntityTypeCache.Get( component.TypeName, true, rockContext );
                        var listItem = new ListItem();
                        listItem.Text = component.GetTitle( selectEntityType.GetEntityType() );
                        listItem.Value = string.Format( "{0}|{1}", ReportFieldType.DataSelectComponent, component.TypeId );
                        listItem.Attributes["optiongroup"] = component.Section;

                        string description = Reflection.GetDescription( component.GetType() );
                        if ( !string.IsNullOrWhiteSpace( description ) )
                        {
                            listItem.Attributes.Add( "title", description );
                        }

                        listItems.Add( listItem );
                    }
                }

                var commonFieldListItems = listItems.Where( a => a.Attributes["optiongroup"] == "Common" ).OrderBy( a => a.Text );
                foreach ( var item in commonFieldListItems )
                {
                    ddlFields.Items.Add( item );
                    listItems.Remove( item );
                }

                var normalFieldListItems = listItems.Where( a => a.Attributes["optiongroup"] == string.Format( "{0} Fields", entityType.Name ) ).OrderBy( a => a.Text );
                foreach ( var item in normalFieldListItems )
                {
                    ddlFields.Items.Add( item );
                    listItems.Remove( item );
                }

                var attributeFieldListItems = listItems.Where( a => a.Attributes["optiongroup"] == string.Format( "{0} Attributes", entityType.Name ) ).OrderBy( a => a.Text );
                foreach ( var item in attributeFieldListItems )
                {
                    ddlFields.Items.Add( item );
                    listItems.Remove( item );
                }

                foreach ( var item in listItems )
                {
                    ddlFields.Items.Add( item );
                }

                ddlFields.Items.Insert( 0, new ListItem( string.Empty, "0" ) );
            }
            else
            {
                ddlFields.Enabled = false;
                ddlFields.Items.Clear();
            }
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="reportId">The report identifier.</param>
        public void ShowDetail( int reportId )
        {
            ShowDetail( reportId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="reportId">The report identifier.</param>
        /// <param name="parentCategoryId">The parent category id.</param>
        public void ShowDetail( int reportId, int? parentCategoryId )
        {
            pnlDetails.Visible = false;

            var rockContext = new RockContext();
            var reportService = new ReportService( rockContext );
            Report report = null;

            if ( !reportId.Equals( 0 ) )
            {
                report = reportService.Get( reportId );
                pdAuditDetails.SetEntity( report, ResolveRockUrl( "~" ) );
            }

            if ( report == null )
            {
                report = new Report { Id = 0, IsSystem = false, CategoryId = parentCategoryId };

                // Hide the panel drawer that shows created and last modified dates.
                pdAuditDetails.Visible = false;
            }
            else
            {
                string quickReturnLava = "{{ Report.Name | AddQuickReturn:'Reports', 40 }}";
                var quickReturnMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new Rock.Lava.CommonMergeFieldsOptions() );
                quickReturnMergeFields.Add( "Report", report );
                quickReturnLava.ResolveMergeFields( quickReturnMergeFields );
            }

            pnlDetails.Visible = true;
            hfReportId.Value = report.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;

            string authorizationMessage;

            if ( !this.IsAuthorizedForAllReportComponents( Authorization.EDIT, report, rockContext, out authorizationMessage ) )
            {
                nbEditModeMessage.Text = authorizationMessage;
                readOnly = true;
            }

            if ( report.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( Report.FriendlyTypeName );
            }

            btnSecurity.Visible = report.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
            btnSecurity.Title = report.Name;
            btnSecurity.EntityId = report.Id;

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                ShowReadonlyDetails( report );
            }
            else
            {
                btnEdit.Visible = true;
                string errorMessage = string.Empty;
                btnDelete.Visible = reportService.CanDelete( report, out errorMessage );
                if ( report.Id > 0 )
                {
                    ShowReadonlyDetails( report );
                }
                else
                {
                    ShowEditDetails( report );
                }
            }
        }

        /// <summary>
        /// Determines whether [is authorized for all report components] [the specified report].
        /// </summary>
        /// <param name="reportAction">The report action.</param>
        /// <param name="report">The report.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="authorizationMessage">The authorization message.</param>
        /// <returns></returns>
        private bool IsAuthorizedForAllReportComponents( string reportAction, Report report, RockContext rockContext, out string authorizationMessage )
        {
            bool isAuthorized = true;
            authorizationMessage = string.Empty;

            // can't edit an existing report if not authorized for that report
            if ( report.Id != 0 && !report.IsAuthorized( reportAction, CurrentPerson ) )
            {
                isAuthorized = false;
                authorizationMessage = EditModeMessage.ReadOnlyEditActionNotAllowed( Report.FriendlyTypeName );
            }

            if ( report.EntityType != null && !report.EntityType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                isAuthorized = false;
                authorizationMessage = "INFO: This report uses an entity type that you do not have access to view.";
            }

            if ( report.ReportFields != null )
            {
                foreach ( var reportField in report.ReportFields )
                {
                    if ( reportField.ReportFieldType == ReportFieldType.DataSelectComponent )
                    {
                        var dataSelectComponent = GetDataSelectComponent( rockContext, reportField.DataSelectComponentEntityTypeId.GetValueOrDefault( 0 ) );
                        if ( dataSelectComponent != null )
                        {
                            if ( !dataSelectComponent.IsAuthorized( Authorization.VIEW, this.CurrentPerson ) )
                            {
                                isAuthorized = false;
                                authorizationMessage = "INFO: This report contains a data selection component that you do not have access to view.";
                                break;
                            }
                        }
                    }
                }
            }

            if ( report.DataView != null )
            {
                if ( !report.DataView.IsAuthorized( Authorization.VIEW, this.CurrentPerson ) )
                {
                    isAuthorized = false;
                    authorizationMessage = "INFO: This report uses a data view that you do not have access to view.";
                }
            }

            return isAuthorized;
        }

        /// <summary>
        /// Creates a DataSelectComponent instance from a unique identifier.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <param name="dataSelectComponentId"></param>
        /// <returns>A DataSelectComponent or null if the component cannot be created.</returns>
        private DataSelectComponent GetDataSelectComponent( RockContext rockContext, int dataSelectComponentId )
        {
            // Get the Type for the Data Select Component used in this column.
            // If the column refers to a Type that does not exist, ignore and continue.
            var componentType = EntityTypeCache.Get( dataSelectComponentId, rockContext ).GetEntityType();

            if ( componentType == null )
            {
                return null;
            }

            string dataSelectComponentTypeName = componentType.FullName;

            return DataSelectContainer.GetComponent( dataSelectComponentTypeName );
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="report">The data view.</param>
        public void ShowEditDetails( Report report )
        {
            if ( report.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( Report.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lReadOnlyTitle.Text = report.Name.FormatAsHtmlTitle();
                lActionTitle.Text = report.Name.FormatAsHtmlTitle();
            }

            LoadDropDowns();

            SetEditMode( true );

            tbName.Text = report.Name;
            tbDescription.Text = report.Description;
            cpCategory.SetValue( report.CategoryId );

            RockContext rockContext = new RockContext();

            var dataViewId = report.DataViewId;
            var entityTypeId = report.EntityTypeId;

            if ( report.Id == default( int ) )
            {
                dataViewId = PageParameter( PageParameterKey.DataViewId ).AsIntegerOrNull();
                if ( dataViewId.HasValue )
                {
                    var dataView = new DataViewService( rockContext ).Get( dataViewId.Value );
                    if ( dataView != null )
                    {
                        entityTypeId = dataView.EntityTypeId;
                        tbName.Text = dataView.Name;
                        tbDescription.Text = dataView.Description;
                    }
                }
            }

            etpEntityType.SelectedEntityTypeId = entityTypeId;
            UpdateControlsForEntityType( etpEntityType.SelectedEntityTypeId );
            dvpDataView.SetValue( dataViewId );
            if ( dataViewId.HasValue )
            {
                // If the Data View has a value, then prevent the report admin from changing the entity type since it was decided by the Data View already.
                // If the Data View is changed, then the page should reload the entire Edit control since options will change.
                etpEntityType.Enabled = false;
            }

            nbFetchTop.Text = report.FetchTop.ToString();
            tbQueryHint.Text = report.QueryHint;

            ReportFieldsDictionary = new List<ReportFieldInfo>();

            kvSortFields.CustomKeys = new Dictionary<string, string>();
            kvSortFields.CustomValues = new Dictionary<string, string>();
            kvSortFields.CustomValues.Add( SortDirection.Ascending.ConvertToInt().ToString(), "Ascending" );
            kvSortFields.CustomValues.Add( SortDirection.Descending.ConvertToInt().ToString(), "Descending" );

            vMergeFields.CustomValues = new Dictionary<string, string>();
            vRecipientFields.CustomValues = new Dictionary<string, string>();

            kvSortFields.Value = report.ReportFields.Where( a => a.SortOrder.HasValue ).OrderBy( a => a.SortOrder.Value ).Select( a => string.Format( "{0}^{1}", a.Guid, a.SortDirection.ConvertToInt() ) ).ToList().AsDelimited( "|" );
            vMergeFields.Value = report.ReportFields.Where( a => a.IsCommunicationMergeField.HasValue && a.IsCommunicationMergeField.Value ).OrderBy( a => a.ColumnOrder ).Select( a => string.Format( "{0}", a.Guid ) ).ToList().AsDelimited( "|" );
            vRecipientFields.Value = report.ReportFields.Where( a => a.IsCommunicationRecipientField.HasValue && a.IsCommunicationRecipientField.Value ).OrderBy( a => a.ColumnOrder ).Select( a => string.Format( "{0}", a.Guid ) ).ToList().AsDelimited( "|" );

            foreach ( var reportField in report.ReportFields.OrderBy( a => a.ColumnOrder ) )
            {
                kvSortFields.CustomKeys.Add( reportField.Guid.ToString(), reportField.ColumnHeaderText );
                vMergeFields.CustomValues.Add( reportField.Guid.ToString(), reportField.ColumnHeaderText );
                vRecipientFields.CustomValues.Add( reportField.Guid.ToString(), reportField.ColumnHeaderText );

                string fieldSelection;
                if ( reportField.ReportFieldType == ReportFieldType.DataSelectComponent )
                {
                    fieldSelection = reportField.DataSelectComponentEntityTypeId.ToString();
                }
                else
                {
                    fieldSelection = reportField.Selection;
                }

                ReportFieldsDictionary.Add( new ReportFieldInfo { Guid = reportField.Guid, ReportFieldType = reportField.ReportFieldType, FieldSelection = fieldSelection } );
                AddFieldPanelWidget( reportField.Guid, reportField.ReportFieldType, fieldSelection, false, rockContext, true, reportField );
            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="report">The data view.</param>
        private void ShowReadonlyDetails( Report report )
        {
            SetEditMode( false );
            hfReportId.SetValue( report.Id );
            lReadOnlyTitle.Text = report.Name.FormatAsHtmlTitle();
            lReportDescription.Text = report.Description.ConvertMarkdownToHtml();

            SetupTimeToRunLabel( report );
            SetupNumberOfRuns( report );
            SetupLastRun( report );

            if ( report.Category != null )
            {
                lCategory.Text = new DescriptionList()
                    .Add( "Category", report.Category.Name )
                    .Html;
            }

            if ( report.DataView != null )
            {
                lDataView.Visible = UserCanEdit;

                var queryParams = new Dictionary<string, string>();
                queryParams.Add( "DataViewId", report.DataViewId.ToString() );

                lDataView.Text = new DescriptionList()
                    .Add( "Data View", $"<a href='{LinkedPageUrl( AttributeKey.DataViewPage, queryParams )}'>{report.DataView.Name}</a>" )
                    .Html;
            }

            BindGrid( report, false );
        }

        /// <summary>
        /// Sets up the last run highlight label
        /// </summary>
        /// <param name="report">The report.</param>
        private void SetupLastRun( Report report )
        {
            if ( report.LastRunDateTime == null )
            {
                return;
            }

            hlLastRun.Text = string.Format( "Last Run: {0}", report.LastRunDateTime.ToShortDateString() );
            hlLastRun.LabelType = LabelType.Default;
        }

        /// <summary>
        /// Sets up the number of runs highlight label
        /// </summary>
        /// <param name="report">The report.</param>
        private void SetupNumberOfRuns( Report report )
        {
            hlRunSince.Text = "Not Run";
            hlRunSince.LabelType = LabelType.Info;

            var status = "Since Creation";

            if ( report.RunCount == null || report.RunCount.Value == 0 )
            {
                hlRunSince.LabelType = LabelType.Warning;
                hlRunSince.Text = string.Format( "Not Run {0}", status );
                return;
            }

            hlRunSince.Text = string.Format( "{0:0} Runs {1}", report.RunCount, status );
        }

        /// <summary>
        /// Sets up the time to run highlight label.
        /// </summary>
        /// <param name="report">The report.</param>
        private void SetupTimeToRunLabel( Report report )
        {
            hlTimeToRun.Text = string.Empty;
            hlTimeToRun.LabelType = LabelType.Default;

            if ( report == null || report.TimeToRunDurationMilliseconds == null )
            {
                return;
            }

            var labelValue = report.TimeToRunDurationMilliseconds.Value;
            var labelUnit = "ms";
            var labelType = LabelType.Success;
            if ( labelValue > 1000 )
            {
                labelValue = labelValue / 1000;
                labelUnit = "s";

                if ( labelValue > 10 )
                {
                    labelType = LabelType.Warning;
                }
            }

            if ( labelValue > 60 && labelUnit == "s" )
            {
                labelValue = labelValue / 60;
                labelUnit = "m";

                if ( labelValue > 1 )
                {
                    labelType = LabelType.Danger;
                }
            }

            hlTimeToRun.LabelType = labelType;
            var isValueAWholeNumber = Math.Abs( labelValue % 1 ) < 0.01;
            if ( isValueAWholeNumber )
            {
                hlTimeToRun.Text = string.Format( "Time To Run: {0:0}{1}", labelValue, labelUnit );
            }
            else
            {
                hlTimeToRun.Text = string.Format( "Time To Run: {0:0.0}{1}", labelValue, labelUnit );
            }
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Adds the field panel widget.
        /// </summary>
        /// <param name="reportFieldGuid">The report field unique identifier.</param>
        /// <param name="reportFieldType">Type of the report field.</param>
        /// <param name="fieldSelection">The field selection.</param>
        /// <param name="showExpanded">if set to <c>true</c> [show expanded].</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="setReportFieldValues">if set to <c>true</c> [set report field values].</param>
        /// <param name="reportField">The report field.</param>
        private void AddFieldPanelWidget( Guid reportFieldGuid, ReportFieldType reportFieldType, string fieldSelection, bool showExpanded, RockContext rockContext, bool setReportFieldValues = false, ReportField reportField = null )
        {
            PanelWidget panelWidget = new PanelWidget();
            panelWidget.ID = string.Format( "reportFieldWidget_{0}", reportFieldGuid.ToString( "N" ) );

            panelWidget.ShowDeleteButton = true;
            panelWidget.DeleteClick += FieldsPanelWidget_DeleteClick;
            panelWidget.ShowReorderIcon = true;
            panelWidget.Expanded = showExpanded;

            HiddenFieldWithClass hfReportFieldGuid = new HiddenFieldWithClass();
            hfReportFieldGuid.CssClass = "js-report-field-guid";
            hfReportFieldGuid.ID = panelWidget.ID + "_hfReportFieldGuid";
            hfReportFieldGuid.Value = reportFieldGuid.ToString();
            panelWidget.Controls.Add( hfReportFieldGuid );

            Label lbFields = new Label();
            lbFields.Text = "Field Type";

            RockDropDownList ddlFields = new RockDropDownList();
            panelWidget.Controls.Add( ddlFields );
            ddlFields.ID = panelWidget.ID + "_ddlFields";
            ddlFields.AutoPostBack = true;
            ddlFields.SelectedIndexChanged += ddlFields_SelectedIndexChanged;

            panelWidget.HeaderControls = new Control[2] { lbFields, ddlFields };
            this.LoadFieldsDropDown( ddlFields );

            RockCheckBox showInGridCheckBox = new RockCheckBox();
            showInGridCheckBox.ID = panelWidget.ID + "_showInGridCheckBox";
            showInGridCheckBox.Text = "Show in Grid";

            panelWidget.Controls.Add( showInGridCheckBox );

            RockTextBox columnHeaderTextTextBox = new RockTextBox();
            columnHeaderTextTextBox.ID = panelWidget.ID + "_columnHeaderTextTextBox";
            columnHeaderTextTextBox.Label = "Column Label";
            columnHeaderTextTextBox.CssClass = "js-column-header-textbox";
            panelWidget.Controls.Add( columnHeaderTextTextBox );

            phReportFields.Controls.Add( panelWidget );

            CreateFieldTypeSpecificControls( reportFieldType, fieldSelection, panelWidget, rockContext );

            if ( setReportFieldValues )
            {
                PopulateFieldPanelWidget( panelWidget, reportField, reportFieldType, fieldSelection, rockContext );
            }
        }

        /// <summary>
        /// Creates the data select controls.
        /// </summary>
        /// <param name="reportFieldType">Type of the report field.</param>
        /// <param name="fieldSelection">The field selection.</param>
        /// <param name="panelWidget">The panel widget.</param>
        /// <param name="rockContext">The rock context.</param>
        private void CreateFieldTypeSpecificControls( ReportFieldType reportFieldType, string fieldSelection, PanelWidget panelWidget, RockContext rockContext )
        {
            PlaceHolder phDataSelectControls = panelWidget.ControlsOfTypeRecursive<PlaceHolder>().FirstOrDefault( a => a.ID == panelWidget.ID + "_phDataSelectControls" );
            if ( phDataSelectControls == null )
            {
                phDataSelectControls = new PlaceHolder();
                phDataSelectControls.ID = panelWidget.ID + "_phDataSelectControls";
                panelWidget.Controls.Add( phDataSelectControls );
            }

            phDataSelectControls.Controls.Clear();

            if ( reportFieldType == ReportFieldType.DataSelectComponent )
            {
                var dataSelectComponent = GetDataSelectComponent( rockContext, fieldSelection.AsInteger() );
                if ( dataSelectComponent != null )
                {
                    Control[] dataSelectControls = dataSelectComponent.CreateChildControls( phDataSelectControls );
                    SetDataSelectControlsValidationGroup( dataSelectControls, this.BlockValidationGroup );
                }
            }
            else if ( reportFieldType == ReportFieldType.Attribute )
            {
                var fieldInfo = ReportingHelper.GetAttributeReportFieldInfo( fieldSelection, rockContext );

                var attributeGuid = fieldInfo.AttributeGuid;
                if ( attributeGuid.HasValue && fieldInfo.HasMaximumLength )
                {
                    var maxLengthControl = new NumberBox();
                    maxLengthControl.ID = phDataSelectControls.ID + "_maxLengthTextBox";
                    maxLengthControl.Label = "Maximum Length";
                    maxLengthControl.Help = "The number of characters after which the field content will be truncated. If not specified, the default condensed format will be displayed.";
                    maxLengthControl.NumberType = ValidationDataType.Integer;
                    maxLengthControl.MinimumValue = "0";
                    maxLengthControl.MaximumValue = "10000";

                    phDataSelectControls.Controls.Add( maxLengthControl );

                    SetValidationGroup( phDataSelectControls.Controls, this.BlockValidationGroup );
                }
            }
        }

        /// <summary>
        /// Sets the data select controls validation group.
        /// </summary>
        /// <param name="dataSelectControls">The data select controls.</param>
        /// <param name="validationGroup">The validation group.</param>
        private void SetDataSelectControlsValidationGroup( Control[] dataSelectControls, string validationGroup )
        {
            if ( dataSelectControls != null && validationGroup != null )
            {
                this.SetValidationGroup( dataSelectControls, validationGroup );
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlFields control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlFields_SelectedIndexChanged( object sender, EventArgs e )
        {
            RockDropDownList ddlFields = sender as RockDropDownList;
            PanelWidget panelWidget = ddlFields.Parent as PanelWidget;
            ReportFieldType reportFieldType = ReportFieldType.Property;
            string fieldSelection = string.Empty;

            string fieldSelectionValue = ddlFields.SelectedItem.Value;
            string[] fieldSelectionValueParts = fieldSelectionValue.Split( '|' );
            if ( fieldSelectionValueParts.Count() == 2 )
            {
                reportFieldType = fieldSelectionValueParts[0].ConvertToEnum<ReportFieldType>();
                fieldSelection = fieldSelectionValueParts[1];
            }

            var rockContext = new RockContext();

            Guid reportFieldGuid = new Guid( panelWidget.ID.Replace( "reportFieldWidget_", string.Empty ) );

            ReportField reportField = new ReportField { ShowInGrid = true, ReportFieldType = reportFieldType };
            if ( reportFieldType == ReportFieldType.DataSelectComponent )
            {
                reportField.Selection = string.Empty;
            }
            else if ( reportFieldType == ReportFieldType.Attribute )
            {
                var info = GetAttributeFieldInfoFromControls( panelWidget, fieldSelection, rockContext );
                fieldSelection = info.SerializeConfiguration();

                reportField.Selection = fieldSelection;
                reportField.ColumnHeaderText = info.ColumnHeader;
                reportField.ShowInGrid = info.ShowInGrid;
            }
            else
            {
                reportField.Selection = fieldSelection;
            }

            var reportFieldInfo = ReportFieldsDictionary.First( a => a.Guid == reportFieldGuid );

            if ( reportFieldInfo.ReportFieldType != reportFieldType || reportFieldInfo.FieldSelection != fieldSelection )
            {
                CreateFieldTypeSpecificControls( reportFieldType, fieldSelection, panelWidget, rockContext );

                reportFieldInfo.ReportFieldType = reportFieldType;
                reportFieldInfo.FieldSelection = fieldSelection;

                PopulateFieldPanelWidget( panelWidget, reportField, reportFieldType, fieldSelection, rockContext );
            }
        }

        private ReportingHelper.AttributeReportFieldInfo GetAttributeFieldInfoFromControls( Control container, string attributeGuid, RockContext rockContext )
        {
            var info = ReportingHelper.GetAttributeReportFieldInfo( attributeGuid, rockContext );

            // Column Header
            var columnHeaderTextTextBox = WebControlHelper.TryGetByName<RockTextBox>( container.Controls, "columnHeaderTextTextBox" );
            if ( columnHeaderTextTextBox != null )
            {
                info.ColumnHeader = columnHeaderTextTextBox.Text;
            }

            // Show in Grid
            var showInGridCheckBox = WebControlHelper.TryGetByName<RockCheckBox>( container.Controls, "showInGridCheckBox" );
            if ( showInGridCheckBox != null )
            {
                info.ShowInGrid = showInGridCheckBox.Checked;
            }

            // Maximum Length
            var maxLengthTextBox = GetMaxLengthControl( container );
            if ( maxLengthTextBox != null )
            {
                info.MaximumLength = maxLengthTextBox.Text.AsIntegerOrNull();
            }

            return info;
        }

        /// <summary>
        /// Populates the field panel widget.
        /// </summary>
        /// <param name="panelWidget">The panel widget.</param>
        /// <param name="reportField">The report field.</param>
        /// <param name="reportFieldType">Type of the report field.</param>
        /// <param name="fieldSelection">The field selection.</param>
        /// <param name="rockContext">The rock context.</param>
        private void PopulateFieldPanelWidget( PanelWidget panelWidget, ReportField reportField, ReportFieldType reportFieldType, string fieldSelection, RockContext rockContext )
        {
            int entityTypeId = etpEntityType.SelectedEntityTypeId ?? 0;
            if ( entityTypeId == 0 )
            {
                return;
            }

            string defaultColumnHeaderText = null;
            DataSelectComponent dataSelectComponent = null;
            bool fieldDefined = false;
            switch ( reportFieldType )
            {
                case ReportFieldType.Property:
                    var entityType = EntityTypeCache.Get( entityTypeId, rockContext ).GetEntityType();
                    var entityField = EntityHelper.GetEntityFields( entityType ).FirstOrDefault( a => a.Name == fieldSelection );
                    if ( entityField != null )
                    {
                        defaultColumnHeaderText = entityField.Title;
                        fieldDefined = true;
                    }

                    break;

                case ReportFieldType.Attribute:
                    var fieldInfo = ReportingHelper.GetAttributeReportFieldInfo( fieldSelection, rockContext );
                    var attributeGuid = fieldInfo.AttributeGuid;
                    if ( attributeGuid.HasValue )
                    {
                        var attribute = AttributeCache.Get( attributeGuid.Value, rockContext );
                        if ( attribute != null )
                        {
                            defaultColumnHeaderText = attribute.Name;
                            fieldDefined = true;
                        }
                    }

                    break;

                case ReportFieldType.DataSelectComponent:
                    dataSelectComponent = this.GetDataSelectComponent( rockContext, fieldSelection.AsInteger() );

                    if ( dataSelectComponent != null )
                    {
                        defaultColumnHeaderText = dataSelectComponent.ColumnHeaderText;
                        fieldDefined = true;
                    }

                    break;
            }

            // Show the common field properties.
            string fieldTitle = string.IsNullOrWhiteSpace( reportField.ColumnHeaderText ) ? defaultColumnHeaderText : reportField.ColumnHeaderText;
            panelWidget.Title = fieldTitle;

            RockCheckBox showInGridCheckBox = panelWidget.ControlsOfTypeRecursive<RockCheckBox>().FirstOrDefault( a => a.ID == panelWidget.ID + "_showInGridCheckBox" );
            showInGridCheckBox.Checked = reportField.ShowInGrid;

            RockTextBox columnHeaderTextTextBox = panelWidget.ControlsOfTypeRecursive<RockTextBox>().FirstOrDefault( a => a.ID == panelWidget.ID + "_columnHeaderTextTextBox" );
            columnHeaderTextTextBox.Text = string.IsNullOrWhiteSpace( reportField.ColumnHeaderText ) ? defaultColumnHeaderText : reportField.ColumnHeaderText;

            // Show settings that are specific to the field type.
            if ( !fieldDefined )
            {
                // return if we can't determine field
                return;
            }

            RockDropDownList ddlFields = panelWidget.ControlsOfTypeRecursive<RockDropDownList>().FirstOrDefault( a => a.ID == panelWidget.ID + "_ddlFields" );
            if ( reportField.ReportFieldType == ReportFieldType.Attribute )
            {
                var fieldInfo = ReportingHelper.GetAttributeReportFieldInfo( reportField.Selection, rockContext );
                var attributeGuid = fieldInfo.AttributeGuid;
                var selectedValue = GetAttributeFieldListKey( attributeGuid );
                if ( ddlFields.Items.OfType<ListItem>().Any( a => a.Value == selectedValue ) )
                {
                    ddlFields.SelectedValue = selectedValue;
                }
                else
                {
                    // if this EntityField is not available for the current person, but this reportField already has it configured, let them keep it
                    var attribute = AttributeCache.Get( attributeGuid.Value, rockContext );
                    ddlFields.Items.Add( new ListItem( attribute.Name, selectedValue ) );
                    ddlFields.SelectedValue = selectedValue;
                }

                var hasMaxLengthControl = GetMaxLengthControl( panelWidget );
                if ( hasMaxLengthControl != null
                        && fieldInfo.HasMaximumLength && fieldInfo.MaximumLength.HasValue )
                {
                    hasMaxLengthControl.Text = fieldInfo.MaximumLength.ToString();
                }
            }
            else if ( reportField.ReportFieldType == ReportFieldType.Property )
            {
                ddlFields.SelectedValue = string.Format( "{0}|{1}", reportField.ReportFieldType, reportField.Selection );
            }
            else if ( reportField.ReportFieldType == ReportFieldType.DataSelectComponent )
            {
                ddlFields.SelectedValue = string.Format( "{0}|{1}", reportField.ReportFieldType, dataSelectComponent.TypeId );
            }

            if ( dataSelectComponent != null )
            {
                PlaceHolder phDataSelectControls = panelWidget.ControlsOfTypeRecursive<PlaceHolder>().FirstOrDefault( a => a.ID == panelWidget.ID + "_phDataSelectControls" );
                if ( phDataSelectControls != null )
                {
                    var dataSelectControls = phDataSelectControls.Controls.OfType<Control>().ToArray();
                    dataSelectComponent.SetSelection( dataSelectControls, reportField.Selection ?? string.Empty );
                }
            }
        }

        private string GetAttributeFieldListKey( Guid? attributeGuid )
        {
            //var settings = ReportingHelper.DeserializeAttributeReportFieldConfig( attributeConfigurationString, rockContext );
            var key = "Attribute|" + attributeGuid.GetValueOrDefault().ToString("n");
            return key;
        }

        private NumberBox GetMaxLengthControl( Control container )
        {
            var maxLengthTextBox = WebControlHelper.TryGetByName<NumberBox>( container.Controls, "maxLengthTextBox" );
            return maxLengthTextBox;
        }

        #endregion

        #region ReportFieldInfo Class

        /// <summary>
        ///
        /// </summary>
        [Serializable]
        protected class ReportFieldInfo
        {
            /// <summary>
            /// Gets or sets the unique identifier.
            /// </summary>
            /// <value>
            /// The unique identifier.
            /// </value>
            [DataMember]
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the type of the report field.
            /// </summary>
            /// <value>
            /// The type of the report field.
            /// </value>
            [DataMember]
            public ReportFieldType ReportFieldType { get; set; }

            /// <summary>
            /// Gets or sets the field selection.
            /// </summary>
            /// <value>
            /// The selection.
            /// </value>
            [DataMember]
            public string FieldSelection { get; set; }
        }

        #endregion
    }
}