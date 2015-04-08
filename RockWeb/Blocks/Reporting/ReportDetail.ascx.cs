// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Runtime.Serialization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    [DisplayName( "Report Detail" )]
    [Category( "Reporting" )]
    [Description( "Displays the details of the given report." )]

    [IntegerField( "Database Timeout", "The number of seconds to wait before reporting a database timeout.", false, 180 )]
    public partial class ReportDetail : RockBlock, IDetailBlock
    {
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
            gReport.RowDataBound += gReport_RowDataBound;
            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", Report.FriendlyTypeName );
            btnSecurity.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Report ) ).Id;

            //// set postback timeout to whatever the DatabaseTimeout is plus an extra 5 seconds so that page doesn't timeout before the database does
            //// note: this only makes a difference on Postback, not on the initial page visit
            int databaseTimeout = GetAttributeValue( "DatabaseTimeout" ).AsIntegerOrNull() ?? 180;
            var sm = ScriptManager.GetCurrent( this.Page );
            if ( sm.AsyncPostBackTimeout < databaseTimeout + 5 )
            {
                sm.AsyncPostBackTimeout = databaseTimeout + 5;
            }
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
                string itemId = PageParameter( "reportId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( PageParameter( "reportId" ).AsInteger(), PageParameter( "ParentCategoryId" ).AsIntegerOrNull() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }

            var rockContext = new RockContext();

            if ( pnlEditDetails.Visible )
            {
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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            // rebuild the CustomKeys list based on the current field titles
            kvSortFields.CustomKeys = new Dictionary<string, string>();
            foreach ( var panelWidget in phReportFields.ControlsOfTypeRecursive<PanelWidget>() )
            {
                Guid reportFieldGuid = panelWidget.ID.Replace( "reportFieldWidget_", string.Empty ).AsGuid();
                kvSortFields.CustomKeys.Add( reportFieldGuid.ToString(), panelWidget.Title );
            }
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
            LoadDropdownsForEntityType( etpEntityType.SelectedEntityTypeId );
        }

        /// <summary>
        /// Handles the GridRebind event of the gReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gReport_GridRebind( object sender, EventArgs e )
        {
            BindGrid( new ReportService( new RockContext() ).Get( hfReportId.ValueAsInt() ) );
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
        /// Handles the RowDataBound event of the gReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gReport_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                try
                {
                    // Format the attribute values based on their field type
                    for ( int i = 0; i < gReport.Columns.Count; i++ )
                    {
                        var boundField = gReport.Columns[i] as BoundField;

                        // AttributeFields are named in format "Attribute_{attributeId}_{columnIndex}". We need the attributeId portion
                        if ( boundField != null && boundField.DataField.StartsWith( "Attribute_" ) )
                        {
                            if ( boundField is BoolField )
                            {
                                // let BoolFields take care of themselves
                            }
                            else
                            {
                                string[] nameParts = boundField.DataField.Split( '_' );
                                if ( nameParts.Count() > 1 )
                                {
                                    string attributeIdPortion = nameParts[1];
                                    int attributeID = attributeIdPortion.AsInteger();
                                    if ( attributeID > 0 )
                                    {
                                        AttributeCache attr = AttributeCache.Read( attributeID );
                                        var cell = e.Row.Cells[i];
                                        string cellValue = HttpUtility.HtmlDecode( cell.Text ).Trim();
                                        cell.Text = attr.FieldType.Field.FormatValue( cell, cellValue, attr.QualifierValues, true );
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // intentionally ignore any errors and just let the original cell value be displayed
                }
            }
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
            report.DataViewId = ddlDataView.SelectedValueAsInt();
            report.FetchTop = nbFetchTop.Text.AsIntegerOrNull();

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
            foreach ( var panelWidget in allPanelWidgets )
            {
                string ddlFieldsId = panelWidget.ID + "_ddlFields";
                RockDropDownList ddlFields = phReportFields.ControlsOfTypeRecursive<RockDropDownList>().First( a => a.ID == ddlFieldsId );
                ReportFieldType reportFieldType = ReportFieldType.Property;
                string fieldSelection = string.Empty;

                string fieldSelectionValue = ddlFields.SelectedItem.Value;
                string[] fieldSelectionValueParts = fieldSelectionValue.Split( '|' );
                if ( fieldSelectionValueParts.Count() == 2 )
                {
                    reportFieldType = fieldSelectionValueParts[0].ConvertToEnum<ReportFieldType>();
                    fieldSelection = fieldSelectionValueParts[1];
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

                    string dataSelectComponentTypeName = EntityTypeCache.Read( reportField.DataSelectComponentEntityTypeId ?? 0, rockContext ).GetEntityType().FullName;
                    DataSelectComponent dataSelectComponent = Rock.Reporting.DataSelectContainer.GetComponent( dataSelectComponentTypeName );
                    string placeHolderId = string.Format( "{0}_phDataSelectControls", panelWidget.ID );
                    var placeHolder = phReportFields.ControlsOfTypeRecursive<PlaceHolder>().Where( a => a.ID == placeHolderId ).FirstOrDefault();

                    if ( dataSelectComponent != null )
                    {
                        reportField.Selection = dataSelectComponent.GetSelection( placeHolder.Controls.OfType<Control>().ToArray() );
                    }
                }
                else
                {
                    reportField.Selection = fieldSelection;
                }

                reportField.Guid = panelWidget.ID.Replace( "reportFieldWidget_", string.Empty ).AsGuid();

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

            if ( adding )
            {
                // add EDIT and ADMINISTRATE to the person who added the report 
                Rock.Security.Authorization.AllowPerson( report, Authorization.EDIT, this.CurrentPerson, rockContext );
                Rock.Security.Authorization.AllowPerson( report, Authorization.ADMINISTRATE, this.CurrentPerson, rockContext );
            }

            var qryParams = new Dictionary<string, string>();
            qryParams["ReportId"] = report.Id.ToString();
            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfReportId.Value.Equals( "0" ) )
            {
                int? parentCategoryId = PageParameter( "ParentCategoryId" ).AsIntegerOrNull();
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
                ReportService service = new ReportService( new RockContext() );
                Report item = service.Get( int.Parse( hfReportId.Value ) );
                ShowReadonlyDetails( item );
            }
        }

        #endregion

        #endregion

        #region Methods

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
        /// Loads the DataView and Fields dropdowns based on the selected EntityType
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        private void LoadDropdownsForEntityType( int? entityTypeId )
        {
            ddlDataView.EntityTypeId = entityTypeId;
            ddlDataView.Enabled = entityTypeId.HasValue;
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
                Type entityType = EntityTypeCache.Read( entityTypeId.Value, rockContext ).GetEntityType();
                var entityFields = Rock.Reporting.EntityHelper.GetEntityFields( entityType );
                ddlFields.Items.Clear();

                var listItems = new List<ListItem>();

                // Add Fields for the EntityType
                foreach ( var entityField in entityFields.OrderBy( a => !a.IsPreviewable ).ThenBy( a => a.Title ) )
                {
                    var listItem = new ListItem();
                    listItem.Text = entityField.Title;
                    if ( entityField.FieldKind == FieldKind.Property )
                    {
                        listItem.Value = string.Format( "{0}|{1}", ReportFieldType.Property, entityField.Name );
                    }
                    else if ( entityField.FieldKind == FieldKind.Attribute )
                    {
                        listItem.Value = string.Format( "{0}|{1}", ReportFieldType.Attribute, entityField.AttributeGuid.Value.ToString( "n" ) );
                    }

                    if ( entityField.IsPreviewable )
                    {
                        listItem.Attributes["optiongroup"] = "Common";
                    }
                    else
                    {
                        listItem.Attributes["optiongroup"] = "Other";
                    }

                    if ( entityField.FieldKind == FieldKind.Attribute && entityField.AttributeGuid.HasValue)
                    {
                        var attribute = AttributeCache.Read( entityField.AttributeGuid.Value );
                        if ( attribute != null )
                        {
                            listItem.Attributes.Add( "title", attribute.Description );
                        }
                    }

                    listItems.Add( listItem );
                }

                // Add DataSelect MEF Components that apply to this EntityType
                foreach ( var component in DataSelectContainer.GetComponentsBySelectedEntityTypeName( entityType.FullName ).OrderBy( c => c.Order ).ThenBy( c => c.GetTitle( entityType ) ) )
                {
                    if ( component.IsAuthorized( Authorization.VIEW, this.RockPage.CurrentPerson ) )
                    {
                        var selectEntityType = EntityTypeCache.Read( component.TypeName, true, rockContext );
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

                foreach ( var item in listItems.OrderByDescending( a => ( a.Attributes["optiongroup"] == "Common" ) ).ThenBy( a => a.Text ).ToArray() )
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
            }

            if ( report == null )
            {
                report = new Report { Id = 0, IsSystem = false, CategoryId = parentCategoryId };
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
                        string dataSelectComponentTypeName = EntityTypeCache.Read( reportField.DataSelectComponentEntityTypeId ?? 0, rockContext ).GetEntityType().FullName;
                        var dataSelectComponent = Rock.Reporting.DataSelectContainer.GetComponent( dataSelectComponentTypeName );
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
                    authorizationMessage = "INFO: This Reports uses a data view that you do not have access to view.";
                }
            }

            return isAuthorized;
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
            }

            LoadDropDowns();

            SetEditMode( true );

            tbName.Text = report.Name;
            tbDescription.Text = report.Description;
            cpCategory.SetValue( report.CategoryId );
            etpEntityType.SelectedEntityTypeId = report.EntityTypeId;
            LoadDropdownsForEntityType( etpEntityType.SelectedEntityTypeId );
            ddlDataView.SetValue( report.DataViewId );
            nbFetchTop.Text = report.FetchTop.ToString();

            ReportFieldsDictionary = new List<ReportFieldInfo>();
            RockContext rockContext = new RockContext();

            kvSortFields.CustomKeys = new Dictionary<string, string>();
            kvSortFields.CustomValues = new Dictionary<string, string>();
            kvSortFields.CustomValues.Add( SortDirection.Ascending.ConvertToInt().ToString(), "Ascending" );
            kvSortFields.CustomValues.Add( SortDirection.Descending.ConvertToInt().ToString(), "Descending" );

            kvSortFields.Value = report.ReportFields.Where( a => a.SortOrder.HasValue ).OrderBy( a => a.SortOrder.Value ).Select( a => string.Format( "{0}^{1}", a.Guid, a.SortDirection.ConvertToInt() ) ).ToList().AsDelimited( "|" );

            foreach ( var reportField in report.ReportFields.OrderBy( a => a.ColumnOrder ) )
            {
                kvSortFields.CustomKeys.Add( reportField.Guid.ToString(), reportField.ColumnHeaderText );
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
            lReportDescription.Text = report.Description;

            BindGrid( report );
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
        /// Shows the preview.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="filter">The filter.</param>
        private void BindGrid( Report report )
        {
            if ( report != null )
            {
                var errors = new List<string>();

                if ( !report.EntityTypeId.HasValue )
                {
                    gReport.Visible = false;
                    return;
                }

                var rockContext = new RockContext();

                if ( !report.IsAuthorized( Authorization.VIEW, this.CurrentPerson ) )
                {
                    gReport.Visible = false;
                    return;
                }

                Type entityType = EntityTypeCache.Read( report.EntityTypeId.Value, rockContext ).GetEntityType();

                bool isPersonDataSet = report.EntityTypeId == EntityTypeCache.Read( typeof( Rock.Model.Person ), true, rockContext ).Id;

                if ( isPersonDataSet )
                {
                    gReport.PersonIdField = "Id";
                    gReport.DataKeyNames = new string[] { "Id" };
                }
                else
                {
                    gReport.PersonIdField = null;
                }

                if ( report.EntityTypeId.HasValue )
                {
                    gReport.RowItemText = EntityTypeCache.Read( report.EntityTypeId.Value, rockContext ).FriendlyName;
                }

                List<EntityField> entityFields = Rock.Reporting.EntityHelper.GetEntityFields( entityType );

                var selectedEntityFields = new Dictionary<int, EntityField>();
                var selectedAttributes = new Dictionary<int, AttributeCache>();
                var selectedComponents = new Dictionary<int, ReportField>();

                // if there is a selectField, keep it to preserve which items are checked
                var selectField = gReport.Columns.OfType<SelectField>().FirstOrDefault();
                gReport.Columns.Clear();
                int columnIndex = 0;

                if ( !string.IsNullOrWhiteSpace( gReport.PersonIdField ) )
                {
                    // if we already had a selectField, use it (to preserve checkbox state)
                    gReport.Columns.Add( selectField ?? new SelectField() );
                    columnIndex++;
                }

                var reportFieldSortExpressions = new Dictionary<Guid, string>();

                foreach ( var reportField in report.ReportFields.OrderBy( a => a.ColumnOrder ) )
                {
                    columnIndex++;
                    if ( reportField.ReportFieldType == ReportFieldType.Property )
                    {
                        var entityField = entityFields.FirstOrDefault( a => a.Name == reportField.Selection );
                        if ( entityField != null )
                        {
                            selectedEntityFields.Add( columnIndex, entityField );

                            BoundField boundField = entityField.GetBoundFieldType();
                            boundField.DataField = string.Format( "Entity_{0}_{1}", entityField.Name, columnIndex );
                            boundField.HeaderText = string.IsNullOrWhiteSpace( reportField.ColumnHeaderText ) ? entityField.Title : reportField.ColumnHeaderText;
                            boundField.SortExpression = boundField.DataField;
                            reportFieldSortExpressions.AddOrReplace( reportField.Guid, boundField.SortExpression );
                            boundField.Visible = reportField.ShowInGrid;
                            gReport.Columns.Add( boundField );
                        }
                    }
                    else if ( reportField.ReportFieldType == ReportFieldType.Attribute )
                    {
                        Guid? attributeGuid = reportField.Selection.AsGuidOrNull();
                        if ( attributeGuid.HasValue )
                        {
                            var attribute = AttributeCache.Read( attributeGuid.Value, rockContext );
                            if ( attribute != null )
                            {
                                selectedAttributes.Add( columnIndex, attribute );

                                BoundField boundField;

                                if ( attribute.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.BOOLEAN.AsGuid() ) )
                                {
                                    boundField = new BoolField();
                                }
                                else
                                {
                                    boundField = new BoundField();
                                }

                                boundField.DataField = string.Format( "Attribute_{0}_{1}", attribute.Id, columnIndex );
                                boundField.HeaderText = string.IsNullOrWhiteSpace( reportField.ColumnHeaderText ) ? attribute.Name : reportField.ColumnHeaderText;
                                boundField.SortExpression = boundField.DataField;
                                reportFieldSortExpressions.AddOrReplace( reportField.Guid, boundField.SortExpression );

                                if ( attribute.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.INTEGER.AsGuid() ) ||
                                    attribute.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.DATE.AsGuid() ) ||
                                    attribute.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.FILTER_DATE.AsGuid() ) )
                                {
                                    boundField.HeaderStyle.HorizontalAlign = HorizontalAlign.Right;
                                    boundField.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
                                }

                                boundField.Visible = reportField.ShowInGrid;

                                // NOTE:  Additional formatting for attributes is done in the gReport_RowDataBound event
                                gReport.Columns.Add( boundField );
                            }
                        }
                    }
                    else if ( reportField.ReportFieldType == ReportFieldType.DataSelectComponent )
                    {
                        selectedComponents.Add( columnIndex, reportField );

                        DataSelectComponent selectComponent = DataSelectContainer.GetComponent( reportField.DataSelectComponentEntityType.Name );
                        if ( selectComponent != null )
                        {
                            DataControlField columnField = selectComponent.GetGridField( entityType, reportField.Selection );

                            if ( columnField is BoundField )
                            {
                                ( columnField as BoundField ).DataField = string.Format( "Data_{0}_{1}", selectComponent.ColumnPropertyName, columnIndex );
                                columnField.SortExpression = ( columnField as BoundField ).DataField;
                            }

                            columnField.HeaderText = string.IsNullOrWhiteSpace( reportField.ColumnHeaderText ) ? selectComponent.ColumnHeaderText : reportField.ColumnHeaderText;
                            if ( columnField.SortExpression != null )
                            {
                                reportFieldSortExpressions.AddOrReplace( reportField.Guid, columnField.SortExpression );
                            }

                            columnField.Visible = reportField.ShowInGrid;
                            gReport.Columns.Add( columnField );
                        }
                    }
                }

                // if no fields are specified, show the default fields (Previewable/All) for the EntityType
                var dataColumns = gReport.Columns.OfType<object>().Where( a => a.GetType() != typeof( SelectField ) );
                if ( dataColumns.Count() == 0 )
                {
                    // show either the Previewable Columns or all (if there are no previewable columns)
                    bool showAllColumns = !entityFields.Any( a => a.FieldKind == FieldKind.Property && a.IsPreviewable );
                    foreach ( var entityField in entityFields.Where( a => a.FieldKind == FieldKind.Property ) )
                    {
                        columnIndex++;
                        selectedEntityFields.Add( columnIndex, entityField );

                        BoundField boundField = entityField.GetBoundFieldType();

                        boundField.DataField = string.Format( "Entity_{0}_{1}", entityField.Name, columnIndex );
                        boundField.HeaderText = entityField.Name;
                        boundField.SortExpression = boundField.DataField;
                        boundField.Visible = showAllColumns || entityField.IsPreviewable;
                        gReport.Columns.Add( boundField );
                    }
                } 
                   
                try
                {
                    gReport.Visible = true;
                    gReport.ExportFilename = report.Name;
                    SortProperty sortProperty = gReport.SortProperty;
                    if ( sortProperty == null )
                    {
                        var reportSort = new SortProperty();
                        var sortColumns = new Dictionary<string, SortDirection>();
                        foreach ( var reportField in report.ReportFields.Where( a => a.SortOrder.HasValue ).OrderBy( a => a.SortOrder.Value ) )
                        {
                            if ( reportFieldSortExpressions.ContainsKey( reportField.Guid ) )
                            {
                                var sortField = reportFieldSortExpressions[reportField.Guid];
                                if ( !string.IsNullOrWhiteSpace( sortField ) )
                                {
                                    sortColumns.Add( sortField, reportField.SortDirection );
                                }
                            }
                        }

                        if ( sortColumns.Any() )
                        {
                            reportSort.Property = sortColumns.Select( a => a.Key + ( a.Value == SortDirection.Descending ? " desc" : string.Empty ) ).ToList().AsDelimited( "," );
                            sortProperty = reportSort;
                        }
                    }

                    gReport.DataSource = report.GetDataSource( entityType, selectedEntityFields, selectedAttributes, selectedComponents, sortProperty, GetAttributeValue( "DatabaseTimeout" ).AsIntegerOrNull() ?? 180, out errors );
                    gReport.DataBind();
                }
                catch ( Exception ex )
                {
                    Exception exception = ex;
                    this.LogException( ex );
                    while ( exception != null )
                    {
                        if ( exception is System.Data.SqlClient.SqlException )
                        {
                            // if there was a SQL Server Timeout, have the warning be a friendly message about that.
                            if ( ( exception as System.Data.SqlClient.SqlException ).Number == -2 )
                            {
                                nbEditModeMessage.NotificationBoxType = NotificationBoxType.Warning;
                                nbEditModeMessage.Text = "This report did not complete in a timely manner. You can try again or adjust the timeout setting of this block.";
                                return;
                            }
                            else
                            {
                                errors.Add( exception.Message );
                                exception = exception.InnerException;
                            }
                        }
                        else
                        {
                            errors.Add( exception.Message );
                            exception = exception.InnerException;
                        }
                    }
                }

                if ( errors.Any() )
                {
                    nbEditModeMessage.NotificationBoxType = NotificationBoxType.Warning;
                    nbEditModeMessage.Text = "INFO: There was a problem with one or more of the report's data components...<br/><br/> " + errors.AsDelimited( "<br/>" );
                }
            }
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
                string dataSelectComponentTypeName = EntityTypeCache.Read( fieldSelection.AsInteger() ).GetEntityType().FullName;
                DataSelectComponent dataSelectComponent = Rock.Reporting.DataSelectContainer.GetComponent( dataSelectComponentTypeName );

                if ( dataSelectComponent != null )
                {
                    var dataSelectControls = dataSelectComponent.CreateChildControls( phDataSelectControls );
                }
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

            Guid reportFieldGuid = new Guid( panelWidget.ID.Replace( "reportFieldWidget_", string.Empty ) );

            ReportField reportField = new ReportField { ShowInGrid = true, ReportFieldType = reportFieldType };
            if ( reportFieldType == ReportFieldType.DataSelectComponent )
            {
                reportField.Selection = string.Empty;
            }
            else
            {
                reportField.Selection = fieldSelection;
            }

            var reportFieldInfo = ReportFieldsDictionary.First( a => a.Guid == reportFieldGuid );
            var rockContext = new RockContext();
            if ( reportFieldInfo.ReportFieldType != reportFieldType || reportFieldInfo.FieldSelection != fieldSelection )
            {
                CreateFieldTypeSpecificControls( reportFieldType, fieldSelection, panelWidget, rockContext );

                reportFieldInfo.ReportFieldType = reportFieldType;
                reportFieldInfo.FieldSelection = fieldSelection;

                PopulateFieldPanelWidget( panelWidget, reportField, reportFieldType, fieldSelection, rockContext );
            }
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
                    var entityType = EntityTypeCache.Read( entityTypeId, rockContext ).GetEntityType();
                    var entityField = EntityHelper.GetEntityFields( entityType ).FirstOrDefault( a => a.Name == fieldSelection );
                    if ( entityField != null )
                    {
                        defaultColumnHeaderText = entityField.Title;
                        fieldDefined = true;
                    }

                    break;

                case ReportFieldType.Attribute:
                    var attribute = AttributeCache.Read( fieldSelection.AsGuid(), rockContext );
                    if ( attribute != null )
                    {
                        defaultColumnHeaderText = attribute.Name;
                        fieldDefined = true;
                    }

                    break;

                case ReportFieldType.DataSelectComponent:
                    string dataSelectComponentTypeName = EntityTypeCache.Read( fieldSelection.AsInteger(), rockContext ).GetEntityType().FullName;
                    dataSelectComponent = Rock.Reporting.DataSelectContainer.GetComponent( dataSelectComponentTypeName );
                    if ( dataSelectComponent != null )
                    {
                        defaultColumnHeaderText = dataSelectComponent.ColumnHeaderText;
                        fieldDefined = true;
                    }

                    break;
            }

            if ( !fieldDefined )
            {
                // return if we can't determine field
                return;
            }

            RockDropDownList ddlFields = panelWidget.ControlsOfTypeRecursive<RockDropDownList>().FirstOrDefault( a => a.ID == panelWidget.ID + "_ddlFields" );
            if ( reportField.ReportFieldType == ReportFieldType.Attribute )
            {
                ddlFields.SelectedValue = string.Format( "{0}|{1}", reportField.ReportFieldType, reportField.Selection );
            }
            else if ( reportField.ReportFieldType == ReportFieldType.Property )
            {
                ddlFields.SelectedValue = string.Format( "{0}|{1}", reportField.ReportFieldType, reportField.Selection );
            }
            else if ( reportField.ReportFieldType == ReportFieldType.DataSelectComponent )
            {
                ddlFields.SelectedValue = string.Format( "{0}|{1}", reportField.ReportFieldType, dataSelectComponent.TypeId );
            }

            string fieldTitle = string.IsNullOrWhiteSpace( reportField.ColumnHeaderText ) ? defaultColumnHeaderText : reportField.ColumnHeaderText;
            panelWidget.Title = fieldTitle;

            RockCheckBox showInGridCheckBox = panelWidget.ControlsOfTypeRecursive<RockCheckBox>().FirstOrDefault( a => a.ID == panelWidget.ID + "_showInGridCheckBox" );
            showInGridCheckBox.Checked = reportField.ShowInGrid;

            RockTextBox columnHeaderTextTextBox = panelWidget.ControlsOfTypeRecursive<RockTextBox>().FirstOrDefault( a => a.ID == panelWidget.ID + "_columnHeaderTextTextBox" );
            columnHeaderTextTextBox.Text = string.IsNullOrWhiteSpace( reportField.ColumnHeaderText ) ? defaultColumnHeaderText : reportField.ColumnHeaderText;

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