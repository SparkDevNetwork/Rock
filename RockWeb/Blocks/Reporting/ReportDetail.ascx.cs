﻿// <copyright>
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
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    [DisplayName( "Report Detail" )]
    [Category( "Reporting" )]
    [Description( "Displays the details of the given report." )]

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
                List<ReportFieldInfo> childGroupTypesDictionary = ViewState["ReportFieldsDictionary"] as List<ReportFieldInfo>;
                return childGroupTypesDictionary;
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
                string parentCategoryId = PageParameter( "parentCategoryId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    if ( string.IsNullOrWhiteSpace( parentCategoryId ) )
                    {
                        ShowDetail( "reportId", int.Parse( itemId ) );
                    }
                    else
                    {
                        ShowDetail( "reportId", int.Parse( itemId ), int.Parse( parentCategoryId ) );
                    }
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }

            if ( pnlEditDetails.Visible )
            {
                foreach ( var field in ReportFieldsDictionary )
                {
                    AddFieldPanelWidget( field.Guid, field.ReportFieldType, field.Selection, true );
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

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlEntityType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ddlEntityType_SelectedIndexChanged( object sender, EventArgs e )
        {
            LoadDropdownsForEntityType( ddlEntityType.SelectedValueAsInt() );
        }

        /// <summary>
        /// Handles the GridRebind event of the gReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gReport_GridRebind( object sender, EventArgs e )
        {
            BindGrid( new ReportService().Get( hfReportId.ValueAsInt() ) );
        }

        /// <summary>
        /// Handles the Click event of the btnAddField control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddField_Click( object sender, EventArgs e )
        {
            string fieldSelectionValue = ddlFields.SelectedItem.Value;
            Guid reportFieldGuid = Guid.NewGuid();
            string[] fieldSelectionValueParts = fieldSelectionValue.Split( '|' );
            if ( fieldSelectionValueParts.Count() == 2 )
            {
                ReportFieldType reportFieldType = fieldSelectionValueParts[0].ConvertToEnum<ReportFieldType>();
                string fieldSelection = fieldSelectionValueParts[1];
                ReportFieldsDictionary.Add( new ReportFieldInfo { Guid = reportFieldGuid, ReportFieldType = reportFieldType, Selection = fieldSelection } );
                AddFieldPanelWidget( reportFieldGuid, reportFieldType, fieldSelection, true, true, new ReportField { ShowInGrid = true } );
            }
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
                                    int attributeID = attributeIdPortion.AsInteger() ?? 0;
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
            var item = new ReportService().Get( int.Parse( hfReportId.Value ) );
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

            var reportService = new ReportService();
            var report = reportService.Get( int.Parse( hfReportId.Value ) );

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

                    RockTransactionScope.WrapTransaction( () =>
                    {
                        reportService.Delete( report, CurrentPersonAlias );
                        reportService.Save( report, CurrentPersonAlias );
                    } );

                    // reload page, selecting the deleted data view's parent
                    var qryParams = new Dictionary<string, string>();
                    if ( categoryId != null )
                    {
                        qryParams["categoryId"] = categoryId.ToString();
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

            using ( new UnitOfWorkScope() )
            {
                ReportService service = new ReportService();
                ReportFieldService reportFieldService = new ReportFieldService();

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
                report.EntityTypeId = ddlEntityType.SelectedValueAsInt();
                report.DataViewId = ddlDataView.SelectedValueAsInt();

                if ( !Page.IsValid )
                {
                    return;
                }

                if ( !report.IsValid )
                {
                    // Controls will render the error messages                    
                    return;
                }

                RockTransactionScope.WrapTransaction( () =>
                {
                    // delete all the reportFields so we can cleanly add them
                    foreach ( var reportField in report.ReportFields.ToList() )
                    {
                        var field = reportFieldService.Get( reportField.Guid );
                        reportFieldService.Delete( field, this.CurrentPersonAlias );
                        reportFieldService.Save( field, this.CurrentPersonAlias );
                    }

                    report.ReportFields.Clear();

                    var allPanelWidgets = phReportFields.ControlsOfTypeRecursive<PanelWidget>();
                    int displayOrder = 0;
                    foreach ( var panelWidget in allPanelWidgets )
                    {
                        string hfReportFieldTypeID = panelWidget.ID + "_hfReportFieldType";
                        HiddenField hfReportFieldType = phReportFields.ControlsOfTypeRecursive<HiddenField>().First( a => a.ID == hfReportFieldTypeID );

                        string hfFieldSelectionID = panelWidget.ID + "_hfFieldSelection";
                        HiddenField hfFieldSelection = phReportFields.ControlsOfTypeRecursive<HiddenField>().First( a => a.ID == hfFieldSelectionID );

                        ReportFieldType reportFieldType = hfReportFieldType.Value.ConvertToEnum<ReportFieldType>();
                        string fieldSelection = hfFieldSelection.Value;
                        ReportField reportField = new ReportField();
                        reportField.ReportFieldType = reportFieldType;

                        string showInGridCheckBoxId = string.Format( "{0}_showInGridCheckBox", panelWidget.ID );
                        RockCheckBox showInGridCheckBox = phReportFields.ControlsOfTypeRecursive<RockCheckBox>().First( a => a.ID == showInGridCheckBoxId );
                        reportField.ShowInGrid = showInGridCheckBox.Checked;

                        string columnHeaderTextTextBoxId = string.Format( "{0}_columnHeaderTextTextBox", panelWidget.ID );
                        RockTextBox columnHeaderTextTextBox = phReportFields.ControlsOfTypeRecursive<RockTextBox>().First( a => a.ID == columnHeaderTextTextBoxId );
                        reportField.ColumnHeaderText = columnHeaderTextTextBox.Text;

                        reportField.Order = displayOrder++;

                        if ( reportFieldType == ReportFieldType.DataSelectComponent )
                        {
                            reportField.DataSelectComponentEntityTypeId = fieldSelection.AsInteger();

                            string dataSelectComponentTypeName = EntityTypeCache.Read( reportField.DataSelectComponentEntityTypeId ?? 0 ).GetEntityType().FullName;
                            DataSelectComponent dataSelectComponent = Rock.Reporting.DataSelectContainer.GetComponent( dataSelectComponentTypeName );

                            string placeHolderId = string.Format( "{0}_phDataSelectControls", panelWidget.ID );
                            var placeHolder = phReportFields.ControlsOfTypeRecursive<PlaceHolder>().Where( a => a.ID == placeHolderId ).FirstOrDefault();
                            reportField.Selection = dataSelectComponent.GetSelection( placeHolder.Controls.OfType<Control>().ToArray() );
                        }
                        else
                        {
                            reportField.Selection = fieldSelection;
                        }

                        report.ReportFields.Add( reportField );
                    }

                    if ( report.Id.Equals( 0 ) )
                    {
                        service.Add( report, CurrentPersonAlias );
                    }

                    service.Save( report, CurrentPersonAlias );
                } );
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
                // Cancelling on Add.  Return to tree view with parent category selected
                var qryParams = new Dictionary<string, string>();

                string parentCategoryId = PageParameter( "parentCategoryId" );
                if ( !string.IsNullOrWhiteSpace( parentCategoryId ) )
                {
                    qryParams["CategoryId"] = parentCategoryId;
                }

                NavigateToPage( RockPage.Guid, qryParams );
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                ReportService service = new ReportService();
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
            ddlEntityType.DataSource = new DataViewService().GetAvailableEntityTypes().ToList();
            ddlEntityType.DataBind();
            ddlEntityType.Items.Insert( 0, new ListItem( string.Empty, "0" ) );
        }

        /// <summary>
        /// Loads the DataView and Fields dropdowns based on the selected EntityType
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        private void LoadDropdownsForEntityType( int? entityTypeId )
        {
            if ( entityTypeId.HasValue )
            {
                ddlDataView.Enabled = true;
                ddlDataView.DataSource = new DataViewService().GetByEntityTypeId( entityTypeId.Value ).ToList();
                ddlDataView.DataBind();
                ddlDataView.Items.Insert( 0, new ListItem( string.Empty, "0" ) );

                ddlFields.Enabled = true;

                Type entityType = EntityTypeCache.Read( entityTypeId.Value ).GetEntityType();
                var entityFields = Rock.Reporting.EntityHelper.GetEntityFields( entityType );
                ddlFields.Items.Clear();

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
                        listItem.Value = string.Format( "{0}|{1}", ReportFieldType.Attribute, entityField.AttributeId );
                    }

                    if ( entityField.IsPreviewable )
                    {
                        listItem.Attributes["optiongroup"] = "Common";
                    }
                    else
                    {
                        listItem.Attributes["optiongroup"] = "Other";
                    }

                    ddlFields.Items.Add( listItem );
                }

                // Add DataSelect MEF Components that apply to this EntityType
                foreach ( var component in DataSelectContainer.GetComponentsBySelectedEntityTypeName( entityType.FullName ).OrderBy( c => c.Order ) )
                {
                    var selectEntityType = EntityTypeCache.Read( component.TypeName );
                    var listItem = new ListItem();
                    listItem.Text = component.GetTitle( selectEntityType.GetEntityType() );
                    listItem.Value = string.Format( "{0}|{1}", ReportFieldType.DataSelectComponent, component.TypeId );
                    listItem.Attributes["optiongroup"] = component.Section;
                    ddlFields.Items.Add( listItem );
                }

                ddlFields.Items.Insert( 0, new ListItem( string.Empty, "0" ) );
            }
            else
            {
                ddlDataView.Enabled = false;
                ddlDataView.Items.Clear();

                ddlFields.Enabled = false;
                ddlFields.Items.Clear();
            }
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            ShowDetail( itemKey, itemKeyValue, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        /// <param name="parentCategoryId">The parent category id.</param>
        public void ShowDetail( string itemKey, int itemKeyValue, int? parentCategoryId )
        {
            pnlDetails.Visible = false;
            if ( !itemKey.Equals( "reportId" ) )
            {
                return;
            }

            var reportService = new ReportService();
            Report report = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                report = reportService.Get( itemKeyValue );
            }
            else
            {
                report = new Report { Id = 0, IsSystem = false, CategoryId = parentCategoryId };
            }

            if ( report == null )
            {
                return;
            }

            pnlDetails.Visible = true;
            hfReportId.Value = report.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !report.IsAuthorized( "Edit", CurrentPerson ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Report.FriendlyTypeName );
            }

            if ( report.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( Report.FriendlyTypeName );
            }

            btnSecurity.Visible = report.IsAuthorized( "Administrate", CurrentPerson );
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
            LoadDropdownsForEntityType( report.EntityTypeId );

            SetEditMode( true );

            tbName.Text = report.Name;
            tbDescription.Text = report.Description;
            cpCategory.SetValue( report.CategoryId );
            ddlEntityType.SetValue( report.EntityTypeId );
            ddlDataView.SetValue( report.DataViewId );

            ReportFieldsDictionary = new List<ReportFieldInfo>();

            foreach ( var reportField in report.ReportFields.OrderBy( a => a.Order ) )
            {
                string fieldSelection;
                if ( reportField.ReportFieldType == ReportFieldType.DataSelectComponent )
                {
                    fieldSelection = reportField.DataSelectComponentEntityTypeId.ToString();
                }
                else
                {
                    fieldSelection = reportField.Selection;
                }

                ReportFieldsDictionary.Add( new ReportFieldInfo { Guid = reportField.Guid, ReportFieldType = reportField.ReportFieldType, Selection = fieldSelection } );
                AddFieldPanelWidget( reportField.Guid, reportField.ReportFieldType, fieldSelection, false, true, reportField );
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
            lblMainDetails.Text = report.Description;

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
            if ( report != null && report.DataView != null )
            {
                var errors = new List<string>();

                if ( !report.EntityTypeId.HasValue )
                {
                    return;
                }

                Type entityType = EntityTypeCache.Read( report.EntityTypeId.Value ).GetEntityType();

                bool isPersonDataSet = report.EntityTypeId == EntityTypeCache.Read( typeof( Rock.Model.Person ) ).Id;

                if ( isPersonDataSet )
                {
                    gReport.PersonIdField = "Id";
                }
                else
                {
                    gReport.PersonIdField = null;
                }

                if ( report.EntityTypeId.HasValue )
                {
                    gReport.RowItemText = EntityTypeCache.Read( report.EntityTypeId.Value ).FriendlyName;
                }

                List<EntityField> entityFields = Rock.Reporting.EntityHelper.GetEntityFields( entityType );

                var selectedEntityFields = new Dictionary<int,EntityField>();
                var selectedAttributes = new Dictionary<int, AttributeCache>();
                var selectedComponents = new Dictionary<int, ReportField>();

                gReport.Columns.Clear();
                int columnIndex = 0;
                foreach ( var reportField in report.ReportFields.OrderBy( a => a.Order ) )
                {
                    columnIndex++;
                    if ( reportField.ReportFieldType == ReportFieldType.Property )
                    {
                        var entityField = entityFields.FirstOrDefault( a => a.Name == reportField.Selection );
                        if ( entityField != null )
                        {
                            selectedEntityFields.Add( columnIndex, entityField );

                            if ( reportField.ShowInGrid )
                            {
                                BoundField boundField;
                                if ( entityField.DefinedTypeId.HasValue )
                                {
                                    boundField = new DefinedValueField();
                                }
                                else
                                {
                                    boundField = Grid.GetGridField( entityField.PropertyType );
                                }

                                boundField.DataField = string.Format( "Entity_{0}_{1}", entityField.Name, columnIndex );
                                boundField.HeaderText = string.IsNullOrWhiteSpace( reportField.ColumnHeaderText ) ? entityField.Title : reportField.ColumnHeaderText;
                                boundField.SortExpression = entityField.Name;
                                gReport.Columns.Add( boundField );
                            }
                        }
                    }
                    else if ( reportField.ReportFieldType == ReportFieldType.Attribute )
                    {
                        int? attributeId = reportField.Selection.AsInteger( false );
                        if ( attributeId.HasValue )
                        {
                            var attribute = AttributeCache.Read( attributeId.Value );
                            selectedAttributes.Add( columnIndex, attribute );

                            if ( reportField.ShowInGrid )
                            {
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
                                boundField.SortExpression = null;

                                if ( attribute.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.INTEGER.AsGuid() ) ||
                                    attribute.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.DATE.AsGuid() ) )
                                {
                                    boundField.HeaderStyle.HorizontalAlign = HorizontalAlign.Right;
                                    boundField.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
                                }

                                // NOTE:  Additional formatting for attributes is done in the gReport_RowDataBound event
                                gReport.Columns.Add( boundField );
                            }
                        }
                    }
                    else if ( reportField.ReportFieldType == ReportFieldType.DataSelectComponent )
                    {
                        selectedComponents.Add( columnIndex, reportField );
                        if ( reportField.ShowInGrid )
                        {
                            DataSelectComponent selectComponent = DataSelectContainer.GetComponent( reportField.DataSelectComponentEntityType.Name );
                            if ( selectComponent != null )
                            {
                                var boundField = Grid.GetGridField( selectComponent.ColumnFieldType );
                                boundField.DataField = string.Format( "Data_{0}_{1}", selectComponent.ColumnPropertyName, columnIndex );
                                boundField.HeaderText = string.IsNullOrWhiteSpace( reportField.ColumnHeaderText ) ? selectComponent.ColumnHeaderText : reportField.ColumnHeaderText;
                                boundField.SortExpression = null;
                                gReport.Columns.Add( boundField );
                            }
                        }
                    }
                }

                gReport.DataSource = report.GetDataSource( new RockContext(), entityType, selectedEntityFields, selectedAttributes, selectedComponents, gReport.SortProperty, out errors );

                if ( errors.Any() )
                {
                    nbEditModeMessage.Text = "INFO: There was a problem with one or more of the report's data components...<br/><br/> " + errors.AsDelimited( "<br/>" );
                }

                gReport.DataBind();
            }
        }

        /// <summary>
        /// Adds the field panel widget.
        /// </summary>
        /// <param name="reportFieldGuid">The report field unique identifier.</param>
        /// <param name="reportFieldType">Type of the report field.</param>
        /// <param name="fieldSelection">The field selection.</param>
        /// <param name="showExpanded">if set to <c>true</c> [show expanded].</param>
        /// <param name="setReportFieldValues">if set to <c>true</c> [set report field values].</param>
        /// <param name="reportField">The report field.</param>
        private void AddFieldPanelWidget( Guid reportFieldGuid, ReportFieldType reportFieldType, string fieldSelection, bool showExpanded, bool setReportFieldValues = false, ReportField reportField = null )
        {
            int entityTypeId = ddlEntityType.SelectedValueAsInt() ?? 0;
            if (entityTypeId == 0)
            {
                return;
            }
            
            PanelWidget panelWidget = new PanelWidget();
            panelWidget.ID = string.Format( "reportFieldWidget_{0}", reportFieldGuid.ToString( "N" ) );

            string fieldTitle = null;
            DataSelectComponent dataSelectComponent = null;
            switch ( reportFieldType )
            {
                case ReportFieldType.Property:
                    var entityType = EntityTypeCache.Read( entityTypeId ).GetEntityType();
                    var entityField = EntityHelper.GetEntityFields( entityType ).FirstOrDefault( a => a.Name == fieldSelection );
                    if ( entityField != null )
                    {
                        fieldTitle = entityField.Title;
                    }

                    break;

                case ReportFieldType.Attribute:
                    var attribute = AttributeCache.Read( fieldSelection.AsInteger() ?? 0 );
                    if ( attribute != null )
                    {
                        fieldTitle = attribute.Name;
                    }

                    break;

                case ReportFieldType.DataSelectComponent:
                    string dataSelectComponentTypeName = EntityTypeCache.Read( fieldSelection.AsInteger() ?? 0 ).GetEntityType().FullName;
                    dataSelectComponent = Rock.Reporting.DataSelectContainer.GetComponent( dataSelectComponentTypeName );
                    if ( dataSelectComponent != null )
                    {
                        fieldTitle = dataSelectComponent.GetTitle( null );
                    }

                    break;
            }

            if ( fieldTitle == null )
            {
                // return if we can't determine field
                return;
            }

            panelWidget.Title = fieldTitle;
            panelWidget.ShowDeleteButton = true;
            panelWidget.DeleteClick += FieldsPanelWidget_DeleteClick;
            panelWidget.ShowReorderIcon = true;
            panelWidget.Expanded = showExpanded;

            HiddenField hfReportFieldType = new HiddenField();
            hfReportFieldType.ID = panelWidget.ID + "_hfReportFieldType";
            hfReportFieldType.Value = reportFieldType.ConvertToString();
            panelWidget.Controls.Add( hfReportFieldType );

            HiddenField hfFieldSelection = new HiddenField();
            hfFieldSelection.ID = panelWidget.ID + "_hfFieldSelection";
            hfFieldSelection.Value = fieldSelection;
            panelWidget.Controls.Add( hfFieldSelection );

            RockCheckBox showInGridCheckBox = new RockCheckBox();
            showInGridCheckBox.ID = panelWidget.ID + "_showInGridCheckBox";
            showInGridCheckBox.Text = "Show in Grid";

            if ( setReportFieldValues )
            {
                showInGridCheckBox.Checked = reportField.ShowInGrid;
            }

            panelWidget.Controls.Add( showInGridCheckBox );

            RockTextBox columnHeaderTextTextBox = new RockTextBox();
            columnHeaderTextTextBox.ID = panelWidget.ID + "_columnHeaderTextTextBox";
            columnHeaderTextTextBox.Label = "Column Label";
            if ( setReportFieldValues )
            {
                columnHeaderTextTextBox.Text = string.IsNullOrWhiteSpace( reportField.ColumnHeaderText ) ? fieldTitle : reportField.ColumnHeaderText;
            }

            panelWidget.Controls.Add( columnHeaderTextTextBox );

            if ( dataSelectComponent != null )
            {
                PlaceHolder phDataSelectControls = new PlaceHolder();
                phDataSelectControls.ID = panelWidget.ID + "_phDataSelectControls";
                panelWidget.Controls.Add( phDataSelectControls );
                var dataSelectControls = dataSelectComponent.CreateChildControls( phDataSelectControls );

                if ( setReportFieldValues )
                {
                    dataSelectComponent.SetSelection( dataSelectControls, reportField.Selection ?? string.Empty );
                }
            }

            phReportFields.Controls.Add( panelWidget );
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
            /// Gets or sets the selection.
            /// </summary>
            /// <value>
            /// The selection.
            /// </value>
            [DataMember]
            public string Selection { get; set; }
        }

        #endregion
    }
}