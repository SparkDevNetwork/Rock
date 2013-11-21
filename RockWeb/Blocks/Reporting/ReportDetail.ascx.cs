//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
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
        protected Dictionary<int, string> ReportFieldsDictionary
        {
            get
            {
                Dictionary<int, string> childGroupTypesDictionary = ViewState["ReportFieldsDictionary"] as Dictionary<int, string>;
                return childGroupTypesDictionary;
            }

            set
            {
                ViewState["ReportFieldsDictionary"] = value;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gReport.GridRebind += gReport_GridRebind;
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
                foreach ( var field in ReportFieldsDictionary.OrderBy( a => a.Key ) )
                {
                    string[] fieldSelectionValueParts = field.Value.Split( '|' );
                    if ( fieldSelectionValueParts.Count() == 2 )
                    {
                        ReportFieldType reportFieldType = fieldSelectionValueParts[0].ConvertToEnum<ReportFieldType>();
                        string fieldSelection = fieldSelectionValueParts[1];
                        AddFieldPanelWidget( field.Key, reportFieldType, fieldSelection, true );
                    }
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

                    foreach ( var field in ReportFieldsDictionary.OrderBy( a => a.Key ) )
                    {
                        string[] fieldSelectionValueParts = field.Value.Split( '|' );
                        if ( fieldSelectionValueParts.Count() == 2 )
                        {
                            ReportFieldType reportFieldType = fieldSelectionValueParts[0].ConvertToEnum<ReportFieldType>();
                            string fieldSelection = fieldSelectionValueParts[1];
                            ReportField reportField = new ReportField();
                            reportField.ReportFieldType = reportFieldType;

                            string showInGridCheckBoxId = string.Format( "reportFieldWidget_{0}_showInGridCheckBox", field.Key );
                            RockCheckBox showInGridCheckBox = phReportFields.ControlsOfTypeRecursive<RockCheckBox>().First( a => a.ID == showInGridCheckBoxId );
                            reportField.ShowInGrid = showInGridCheckBox.Checked;
                            reportField.Order = field.Key;

                            if ( reportFieldType == ReportFieldType.DataSelectComponent )
                            {
                                reportField.DataSelectComponentEntityTypeId = fieldSelection.AsInteger();

                                string dataSelectComponentTypeName = EntityTypeCache.Read( reportField.DataSelectComponentEntityTypeId ?? 0 ).GetEntityType().FullName;
                                DataSelectComponent dataSelectComponent = Rock.Reporting.DataSelectContainer.GetComponent( dataSelectComponentTypeName );

                                string placeHolderId = string.Format( "reportFieldWidget_{0}_phDataSelectControls", field.Key );
                                var placeHolder = phReportFields.ControlsOfTypeRecursive<PlaceHolder>().Where( a => a.ID == placeHolderId ).FirstOrDefault();
                                reportField.Selection = dataSelectComponent.GetSelection( placeHolder.Controls.OfType<Control>().ToArray() );
                            }
                            else
                            {
                                reportField.Selection = fieldSelection;
                            }

                            report.ReportFields.Add( reportField );
                        }
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

            ReportFieldsDictionary = new Dictionary<int, string>();

            int displayOrder = 0;
            foreach ( var reportField in report.ReportFields.OrderBy( a => a.Order ) )
            {
                string fieldSelection;
                if ( reportField.ReportFieldType == ReportFieldType.DataSelectComponent )
                {
                    fieldSelection = string.Format( "{0}|{1}", reportField.ReportFieldType, reportField.DataSelectComponentEntityTypeId );
                }
                else
                {
                    fieldSelection = string.Format( "{0}|{1}", reportField.ReportFieldType, reportField.Selection );
                }

                // use displayOrder instead of reportField.Order just in case there are gaps or duplicates in reportField.Orders
                ReportFieldsDictionary.Add( displayOrder++, fieldSelection );
                AddFieldPanelWidget( displayOrder, reportField.ReportFieldType, fieldSelection.Split( '|' )[1], false, true, reportField.Selection );
            }

            // TODO, set control values for DataSelectComponent
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
                gReport.DataSource = report.DataView.BindGrid( gReport, out errors, true );

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
            int displayOrder = ReportFieldsDictionary.Count();
            ReportFieldsDictionary.Add( displayOrder, fieldSelectionValue );
            string[] fieldSelectionValueParts = fieldSelectionValue.Split( '|' );
            if ( fieldSelectionValueParts.Count() == 2 )
            {
                ReportFieldType reportFieldType = fieldSelectionValueParts[0].ConvertToEnum<ReportFieldType>();
                string fieldSelection = fieldSelectionValueParts[1];
                AddFieldPanelWidget( displayOrder, reportFieldType, fieldSelection, true );
            }
        }

        /// <summary>
        /// Adds the field panel widget.
        /// </summary>
        /// <param name="displayOrder">The display order.</param>
        /// <param name="fieldName">Name of the field.</param>
        private void AddFieldPanelWidget( int displayOrder, ReportFieldType reportFieldType, string fieldSelection, bool showExpanded,
            bool setDataSelectComponentSelection = false, string dataSelectComponentSelection = null )
        {
            PanelWidget panelWidget = new PanelWidget();
            panelWidget.ID = string.Format( "reportFieldWidget_{0}", displayOrder );

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
            // TODO
        }
    }
}