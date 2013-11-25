//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
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
    /// <summary>
    /// 
    /// </summary>
    public partial class ReportDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

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

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gReport.GridRebind += gReport_GridRebind;
            btnDelete.Attributes["onclick"] = string.Format( "javascript: return confirmDelete(event, '{0}');", Report.FriendlyTypeName );
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

        #endregion

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
                           reportService.Delete( report, CurrentPersonId );
                           reportService.Save( report, CurrentPersonId );
                       } );

                    // reload page, selecting the deleted data view's parent
                    var qryParams = new Dictionary<string, string>();
                    if ( categoryId != null )
                    {
                        qryParams["categoryId"] = categoryId.ToString();
                    }

                    NavigateToPage( this.CurrentPage.Guid, qryParams );
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
                        reportFieldService.Delete( field, this.CurrentPersonId );
                        reportFieldService.Save( field, this.CurrentPersonId );
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
                        service.Add( report, CurrentPersonId );
                    }

                    service.Save( report, CurrentPersonId );
                } );
            }

            var qryParams = new Dictionary<string, string>();
            qryParams["ReportId"] = report.Id.ToString();
            NavigateToPage( this.CurrentPage.Guid, qryParams );
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

                NavigateToPage( this.CurrentPage.Guid, qryParams );
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

        #region Internal Methods

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
                foreach ( var component in DataSelectContainer.GetComponentsBySelectedEntityTypeName( entityType.FullName ).OrderBy( c => c.Title ) )
                {
                    var selectEntityType = EntityTypeCache.Read( component.TypeName );
                    var listItem = new ListItem();
                    listItem.Text = component.Title;
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
                AddFieldPanelWidget( reportField.Guid, reportField.ReportFieldType, fieldSelection, false, true, reportField.Selection );
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

                List<EntityField> entityFields = Rock.Reporting.EntityHelper.GetEntityFields( entityType );

                var dataSelectComponentDictionary = new Dictionary<string, DataSelectComponent>();
                DataTable dataTable = new DataTable( "reportTable" );
                dataTable.Columns.Add( "Id", typeof( int ) );

                gReport.DataKeyNames = new string[] { "Id" };
                
                gReport.Columns.Clear();
                foreach ( var reportField in report.ReportFields.OrderBy( a => a.Order ) )
                {
                    if ( reportField.ReportFieldType == ReportFieldType.Property )
                    {
                        var entityField = entityFields.FirstOrDefault( a => a.Name == reportField.Selection );
                        if ( entityField != null )
                        {
                            BoundField boundField;
                            if ( entityField.DefinedTypeId.HasValue )
                            {
                                boundField = new DefinedValueField();
                                dataTable.Columns.Add( entityField.Name, typeof( int ) );
                            }
                            else
                            {
                                boundField = Grid.GetGridField( entityField.PropertyType );
                                dataTable.Columns.Add( entityField.Name );
                            }

                            boundField.DataField = entityField.Name;
                            boundField.HeaderText = entityField.Title;
                            boundField.SortExpression = entityField.Name;

                            gReport.Columns.Add( boundField );
                        }
                    }
                    else if ( reportField.ReportFieldType == ReportFieldType.Attribute )
                    {
                        // TODO get more info about the Attribute (FieldType, Name, etc);
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = reportField.Selection;
                        boundField.HeaderText = reportField.Selection.SplitCase();
                        boundField.SortExpression = null;
                        dataTable.Columns.Add( boundField.DataField );
                        gReport.Columns.Add( boundField );
                    }
                    else if ( reportField.ReportFieldType == ReportFieldType.DataSelectComponent )
                    {
                        DataSelectComponent dataSelectComponent = DataSelectContainer.GetComponent( reportField.DataSelectComponentEntityType.Name );

                        foreach ( var dataColumn in dataSelectComponent.DataColumns )
                        {
                            BoundField boundField = Grid.GetGridField( dataColumn.DataType );
                            if ( boundField != null )
                            {
                                boundField.HeaderText = dataColumn.ColumnName.SplitCase();
                                boundField.DataField = dataColumn.ColumnName;
                                boundField.SortExpression = null;
                                dataTable.Columns.Add( dataColumn );
                                gReport.Columns.Add( boundField );
                                dataSelectComponentDictionary.Add( dataColumn.ColumnName, dataSelectComponent );
                            }
                        }
                    }
                }

                using ( new UnitOfWorkScope() )
                {
                    IQueryable<IEntity> dataviewQry = report.DataView.GetQuery( out errors );
                    var list = dataviewQry.ToList();
                    var listItemType = EntityTypeCache.Read( report.EntityTypeId ?? 0 ).GetEntityType();
                    var itemProperties = listItemType.GetProperties();
                    foreach ( var item in list )
                    {
                        List<object> dataValues = new List<object>();
                        dataValues.Add( item.Id );
                        int reportFieldCount = 0;
                        foreach ( var c in gReport.Columns.OfType<BoundField>() )
                        {
                            reportFieldCount++;
                            var p = itemProperties.FirstOrDefault( a => a.Name == c.DataField );
                            if ( p != null )
                            {
                                var val = p.GetValue( item );
                                dataValues.Add( val );
                            }
                            else
                            {
                                //TODO: Get it from DataSelectComponent
                                var dataSelect = dataSelectComponentDictionary[c.DataField];
                                if ( dataSelect != null )
                                {
                                    if ( reportFieldCount >= dataValues.Count )
                                    {
                                        foreach ( var val in dataSelect.GetDataColumnValues( item ) )
                                        {
                                            dataValues.Add( val );
                                        }
                                    }
                                    
                                }
                            }
                        }

                        dataTable.Rows.Add( dataValues.ToArray() );
                    }


                    gReport.DataSource = dataTable;
                }

                if ( errors.Any() )
                {
                    nbEditModeMessage.Text = "INFO: There was a problem with one or more of the report's data view filters...<br/><br/> " + errors.AsDelimited( "<br/>" );
                }

                gReport.DataBind();
            }
        }

        #endregion

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
                AddFieldPanelWidget( reportFieldGuid, reportFieldType, fieldSelection, true );
            }
        }

        /// <summary>
        /// Adds the field panel widget.
        /// </summary>
        /// <param name="displayOrder">The display order.</param>
        /// <param name="fieldName">Name of the field.</param>
        private void AddFieldPanelWidget( Guid reportFieldGuid, ReportFieldType reportFieldType, string fieldSelection, bool showExpanded, bool setDataSelectComponentSelection = false, string dataSelectComponentSelection = null )
        {
            PanelWidget panelWidget = new PanelWidget();
            panelWidget.ID = string.Format( "reportFieldWidget_{0}", reportFieldGuid.ToString( "N" ) );

            string fieldTitle = null;
            DataSelectComponent dataSelectComponent = null;
            switch ( reportFieldType )
            {
                case ReportFieldType.Property:
                    var entityType = EntityTypeCache.Read( ddlEntityType.SelectedValueAsInt() ?? 0 ).GetEntityType();
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
                        fieldTitle = dataSelectComponent.Title;
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
            showInGridCheckBox.Checked = true;
            panelWidget.Controls.Add( showInGridCheckBox );

            if ( dataSelectComponent != null )
            {
                PlaceHolder phDataSelectControls = new PlaceHolder();
                phDataSelectControls.ID = panelWidget.ID + "_phDataSelectControls";
                panelWidget.Controls.Add( phDataSelectControls );
                var dataSelectControls = dataSelectComponent.CreateChildControls( phDataSelectControls );

                if ( setDataSelectComponentSelection )
                {
                    dataSelectComponent.SetSelection( dataSelectControls, dataSelectComponentSelection );
                }
            }

            phReportFields.Controls.Add( panelWidget );
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
    }
}