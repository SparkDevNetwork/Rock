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
using System.Data.Entity;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using System.Web.UI.WebControls;
using Rock.Web.Cache;
using Rock.Reporting;
using Newtonsoft.Json;
using Rock.Web.UI;
using Rock.Utility;
using Rock.Field.Types;

namespace RockWeb.Plugins.com_lcbcchurch.NewVisitor
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Campus Metrics" )]
    [Category( "LCBC > New Visitor" )]
    [Description( "Block to display campus metrics." )]

    #region Block Attributes
    [TextField( "Set Title",
        Description = "The title to display in the panel header.",
        IsRequired = true,
        Key = AttributeKeys.Title,
        Order = 0 )]
    [TextField( "Set Icon Css Class",
        Description = "The icon to display in the panel header.",
        IsRequired = false,
        Key = AttributeKeys.IconCssClass,
        Order = 1 )]

    // Custom Settings
    [LinkedPage( "Campus Staff Personal Contacts Page",
        Key = AttributeKeys.CampusStaffPersonalContactsPage,
        Description = "The page that handles the selected row clicks.",
        IsRequired = false,
        Category = "CustomSetting" )]
    [SecurityRoleField( "Executive Team Role",
        Key = AttributeKeys.ExecutiveTeamRole,
        Description = "Member of this group can see all campuses.",
        IsRequired = true,
        Category = "CustomSetting" )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON,
        name: "Campus Attribute",
        description: "The person attribute used to determine which campus a staff person is assigned to.",
        required: true,
        allowMultiple: false,
        Key = AttributeKeys.CampusAttribute,
        Category = "CustomSetting" )]
    [IntegerField( "Success Minimum",
        Description = "The minimum percentage to display the success label.",
        IsRequired = false,
        Key = AttributeKeys.SuccessMinimum,
        Category = "CustomSetting" )]
    [IntegerField( "Warning Minimum",
        Description = "The minimum percentage to display the warning label.",
        IsRequired = false,
        Key = AttributeKeys.WarningMinimum,
        Category = "CustomSetting" )]
    [TextField( "Metrics",
        Description = "Configures the metric you need like to display.",
        IsRequired = false,
        DefaultValue = "",
        Key = AttributeKeys.Metrics,
        Category = "CustomSetting" )]
    # endregion Block Attributes

    public partial class CampusMetrics : RockBlockCustomSettings
    {
        #region Fields

        private int _campusEntityId = EntityTypeCache.Get( typeof( Rock.Model.Campus ) ).Id;
        private int? _campusId = null;

        #endregion

        #region Properties

        private List<MetricRow> MetricRowsState { get; set; }

        #endregion Properties

        #region Attribute Keys

        protected static class AttributeKeys
        {
            public const string Title = "Title";
            public const string IconCssClass = "IconCssClass";
            public const string SuccessMinimum = "SuccessMinimum";
            public const string WarningMinimum = "WarningMinimum";
            public const string CampusAttribute = "CampusAttribute";
            public const string ExecutiveTeamRole = "ExecutiveTeamRole";
            public const string CampusStaffPersonalContactsPage = "CampusStaffPersonalContactsPage";
            public const string Metrics = "Metrics";
        }

        #endregion Attribute Keys

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );


            string json = ViewState["MetricRowsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                MetricRowsState = new List<MetricRow>();
            }
            else
            {
                MetricRowsState = RockJsonTextReader.DeserializeObjectInSimpleMode<List<MetricRow>>( json );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gCampusMetrics.DataKeyNames = new string[] { "CampusId" };
            gCampusMetrics.GridRebind += gCampusMetrics_GridRebind;
            gCampusMetrics.RowDataBound += gCampusMetrics_RowDataBound;

            gMetrics.DataKeyNames = new string[] { "Guid" };
            gMetrics.RowDataBound += gMetrics_RowDataBound;
            gMetrics.RowEditing += gMetrics_RowEditing;
            gMetrics.GridReorder += gMetrics_GridReorder;
            gMetrics.RowUpdating += gMetrics_RowUpdating;
            gMetrics.RowCancelingEdit += gMetrics_RowCancelingEdit;
            gMetrics.Actions.ShowAdd = true;
            gMetrics.Actions.AddClick += gMetrics_Add;
            gMetrics.IsDeleteEnabled = true;

            SetPanelTitleAndIcon();
            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                ShowView();
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.BlockStateContractResolver(),
            };

            ViewState["MetricRowsState"] = RockJsonTextWriter.SerializeObjectInSimpleMode( MetricRowsState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowView();
        }

        /// <summary>
        /// Handles the GridRebind event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        private void gCampusMetrics_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Selected event of the gCampusMetrics control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gCampusMetrics_Selected( object sender, RowEventArgs e )
        {
            int campusId = e.RowKeyId;
            NavigateToLinkedPage( AttributeKeys.CampusStaffPersonalContactsPage, "campusId", campusId );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gCampusMetrics control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gCampusMetrics_RowDataBound( object sender, GridViewRowEventArgs e )
        {

            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                MetricData metricData = e.Row.DataItem as MetricData;
                if ( metricData != null )
                {
                    foreach ( var item in metricData.Denominators.Keys )
                    {
                        var lItem = e.Row.FindControl( item ) as Literal;
                        decimal percentage = 100;
                        if ( metricData.Denominators[item] > 0 )
                        {
                            percentage = Math.Round( metricData.Numerators[item] / metricData.Denominators[item] * 100, 1 );
                        }
                        lItem.Text = GetMetricColumnHtml( percentage, metricData.Denominators[item] );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the metric percentage HTML include bootstrap label
        /// </summary>
        /// <param name="percentage">The percentage.</param>
        /// <param name="denominator">The denominator.</param>
        /// <returns></returns>
        public string GetMetricColumnHtml( decimal percentage, decimal denominator )
        {
            string css = "label-danger";

            var warningMinimum = GetAttributeValue( AttributeKeys.WarningMinimum ).AsInteger();
            var successMinimum = GetAttributeValue( AttributeKeys.SuccessMinimum ).AsInteger();
            if ( percentage >= successMinimum )
            {
                css = "label-success";
            }
            else if ( percentage >= warningMinimum )
            {
                css = "label-warning";
            }

            return string.Format( "<span class='label {0} grid-label'> {1}% <span style='display: none;'> / {2}</span></span>", css, percentage.ToString("0.#"), Convert.ToInt32( denominator ) );
        }


        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            var ppFieldType = new PageReferenceFieldType();
            var campusStaffPersonalContactsPageValue = ppFieldType.GetEditValue( ppCampusStaffPersonalContactsPage, null );
            SetAttributeValue( AttributeKeys.CampusStaffPersonalContactsPage, campusStaffPersonalContactsPageValue );
            SetAttributeValue( AttributeKeys.ExecutiveTeamRole, ddlSecurityRole.SelectedValue );
            SetAttributeValue( AttributeKeys.CampusAttribute, ddlCampusAttribute.SelectedValue );
            SetAttributeValue( AttributeKeys.SuccessMinimum, numSuccessMinimum.Text );
            SetAttributeValue( AttributeKeys.WarningMinimum, numWarningMinimum.Text );

            MetricRowsState.RemoveAll( a => a.Guid == default( Guid ) );
            SetAttributeValue( AttributeKeys.Metrics, MetricRowsState.ToJson() );
            SaveAttributeValues();

            mdEdit.Hide();
            pnlEditModal.Visible = false;
            upnlContent.Update();
            ShowView();
        }

        #region Metrics Grid Event

        /// <summary>
        /// Handles the GridReorder event of the gMetricPartitions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gMetrics_GridReorder( object sender, GridReorderEventArgs e )
        {
            ReorderMetricRows( MetricRowsState, e.OldIndex, e.NewIndex );
            BindMetricGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gMetrics control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gMetrics_RowDataBound( object sender, GridViewRowEventArgs e )
        {

            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                MetricRow metricRow = e.Row.DataItem as MetricRow;
                if ( metricRow != null )
                {
                    if ( ( e.Row.RowState & DataControlRowState.Edit ) == DataControlRowState.Edit )
                    {

                        var tbLabel = e.Row.FindControl( "tbLabel" ) as RockTextBox;
                        if ( tbLabel != null )
                        {
                            tbLabel.Text = metricRow.Label;
                        }

                        var metricCategoryService = new MetricCategoryService( new RockContext() );
                        var mcpNumeratorMetric = e.Row.FindControl( "mcpNumeratorMetric" ) as MetricCategoryPicker;
                        if ( mcpNumeratorMetric != null )
                        {
                            MetricCategory metricCategory = null;
                            if ( metricRow.NumeratorMetricId != default( int ) )
                            {
                                metricCategory = metricCategoryService.Queryable().Where( a => a.MetricId == metricRow.NumeratorMetricId ).FirstOrDefault();
                                mcpNumeratorMetric.SetValue( metricCategory );
                            }
                        }

                        var mcpDenominatorMetric = e.Row.FindControl( "mcpDenominatorMetric" ) as MetricCategoryPicker;
                        if ( mcpDenominatorMetric != null )
                        {
                            MetricCategory metricCategory = null;
                            if ( metricRow.DenominatorMetricId != default( int ) )
                            {
                                metricCategory = metricCategoryService.Queryable().Where( a => a.MetricId == metricRow.DenominatorMetricId ).FirstOrDefault();
                                mcpDenominatorMetric.SetValue( metricCategory );
                            }
                        }

                        var ddlValueOptionType = e.Row.FindControl( "ddlValueOptionType" ) as RockDropDownList;
                        if ( ddlValueOptionType != null )
                        {
                            ddlValueOptionType.BindToEnum<ValueOptionType>();
                            ddlValueOptionType.SetValue( ( int ) metricRow.ValueOptionType );
                        }
                    }
                    else
                    {
                        var metricService = new MetricService( new RockContext() );
                        var lNumeratorMetric = e.Row.FindControl( "lNumeratorMetric" ) as Literal;
                        var metric = metricService.Get( metricRow.NumeratorMetricId );
                        if ( metric != null )
                        {
                            lNumeratorMetric.Text = metric.Title;
                        }

                        var lDenominatorMetric = e.Row.FindControl( "lDenominatorMetric" ) as Literal;
                        metric = metricService.Get( metricRow.DenominatorMetricId );
                        if ( metric != null )
                        {
                            lDenominatorMetric.Text = metric.Title;
                        }

                        var lUseValueOption = e.Row.FindControl( "lUseValueOption" ) as Literal;
                        if ( lUseValueOption != null )
                        {
                            lUseValueOption.Text = metricRow.ValueOptionType.ConvertToString();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the RowEditing event of the gMetrics control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewEditEventArgs"/> instance containing the event data.</param>
        protected void gMetrics_RowEditing( object sender, GridViewEditEventArgs e )
        {
            gMetrics.EditIndex = e.NewEditIndex;
            BindMetricGrid( true );
        }

        /// <summary>
        /// Handles the RowUpdating event of the gMetrics control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewUpdateEventArgs" /> instance containing the event data.</param>
        protected void gMetrics_RowUpdating( object sender, GridViewUpdateEventArgs e )
        {
            nbError.Visible = false;
            var guidKey = e.Keys["Guid"].ToString().AsGuid();
            var metricRow = MetricRowsState.FirstOrDefault( a => a.Guid == guidKey );
            if ( metricRow != null )
            {
                var row = gMetrics.Rows[e.RowIndex];
                RockTextBox tbLabel = row.FindControl( "tbLabel" ) as RockTextBox;
                MetricCategoryPicker mcpNumeratorMetric = row.FindControl( "mcpNumeratorMetric" ) as MetricCategoryPicker;
                MetricCategoryPicker mcpDenominatorMetric = row.FindControl( "mcpDenominatorMetric" ) as MetricCategoryPicker;
                RockDropDownList ddlValueOptionType = row.FindControl( "ddlValueOptionType" ) as RockDropDownList;

                var metricCategoryService = new MetricCategoryService( new RockContext() );
                var numeratorMetricCategoryId = mcpNumeratorMetric.SelectedValue.AsInteger();
                var denominatorMetricCategoryId = mcpDenominatorMetric.SelectedValue.AsInteger();
                var numeratorMetricCategory = metricCategoryService.Queryable().Where( a => a.Id == numeratorMetricCategoryId ).FirstOrDefault();
                var denominatorMetricCategory = metricCategoryService.Queryable().Where( a => a.Id == denominatorMetricCategoryId ).FirstOrDefault();

                if ( tbLabel.Text.IsNullOrWhiteSpace() || numeratorMetricCategory == null || denominatorMetricCategory == null )
                {
                    nbError.Visible = true;
                    nbError.Text = "Configuration error. Please make sure that data in all the fields are entered and are valid.";
                    nbError.NotificationBoxType = NotificationBoxType.Danger;
                    return;
                }

                if ( metricRow.Guid == default( Guid ) )
                {
                    metricRow.Guid = Guid.NewGuid();
                }

                metricRow.Label = tbLabel.Text;
                metricRow.ValueOptionType = ddlValueOptionType.SelectedValueAsEnum<ValueOptionType>();

                if ( numeratorMetricCategory != null )
                {
                    metricRow.NumeratorMetricId = numeratorMetricCategory.MetricId;
                }

                if ( denominatorMetricCategory != null )
                {
                    metricRow.DenominatorMetricId = denominatorMetricCategory.MetricId;
                }
            }

            gMetrics.EditIndex = -1;
            BindMetricGrid();
        }

        /// <summary>
        /// Handles the RowDelete event of the gMetrics control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gMetrics_RowDelete( object sender, RowEventArgs e )
        {
            MetricRowsState.RemoveAt( e.RowIndex );

            gMetrics.EditIndex = -1;

            BindMetricGrid();
        }

        /// <summary>
        /// Handles the Add event of the gMetrics control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gMetrics_Add( object sender, EventArgs e )
        {
            if ( !MetricRowsState.Any( a => a.Guid == default( Guid ) ) )
            {
                var order = MetricRowsState.Any() ? MetricRowsState.Max( a => a.Order ) + 1 : 0;
                MetricRowsState.Add( new MetricRow() { Order = order } );
                gMetrics.EditIndex = MetricRowsState.Count - 1;
            }
            BindMetricGrid( true );
        }

        /// <summary>
        /// Handles the RowCancelingEdit event of the gMetrics control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewCancelEditEventArgs"/> instance containing the event data.</param>
        protected void gMetrics_RowCancelingEdit( object sender, GridViewCancelEditEventArgs e )
        {

            var metricRow = MetricRowsState[e.RowIndex];

            if ( metricRow != null && metricRow.Guid == default( Guid ) )
            {
                // was added
                MetricRowsState.RemoveAt( e.RowIndex );
            }

            gMetrics.EditIndex = -1;

            BindMetricGrid();
        }

        #endregion Metrics Grid

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {

            pnlEditModal.Visible = true;
            upnlContent.Update();
            mdEdit.Show();

            numWarningMinimum.Text = GetAttributeValue( AttributeKeys.WarningMinimum );
            numSuccessMinimum.Text = GetAttributeValue( AttributeKeys.SuccessMinimum );

            var ppFieldType = new PageReferenceFieldType();
            ppFieldType.SetEditValue( ppCampusStaffPersonalContactsPage, null, GetAttributeValue( AttributeKeys.CampusStaffPersonalContactsPage ) );

            var rockContext = new RockContext();
            var roles = new GroupService( rockContext ).Queryable().Where( g => g.IsSecurityRole ).OrderBy( t => t.Name ).Select( t => new { value = t.Guid, text = t.Name } )
                .ToList();
            ddlSecurityRole.DataSource = roles;
            ddlSecurityRole.DataTextField = "text";
            ddlSecurityRole.DataValueField = "value";
            ddlSecurityRole.DataBind();
            ddlSecurityRole.Items.Insert( 0, new ListItem() );
            ddlSecurityRole.SelectedValue = GetAttributeValue( AttributeKeys.ExecutiveTeamRole );

            var campusFieldTypeId = FieldTypeCache.Get( new Guid( Rock.SystemGuid.FieldType.CAMPUS ) ).Id;
            var personAttributesQry = new AttributeService( rockContext ).GetByEntityTypeId( new Person().TypeId, false );
            var datePersonAttributes = personAttributesQry
                .Where( a => a.FieldTypeId == campusFieldTypeId )
                .OrderBy( t => t.Order )
                .ThenBy( t => t.Name )
                .Select( t => new { value = t.Guid, text = t.Name } )
                .ToList();
            ddlCampusAttribute.DataSource = datePersonAttributes;
            ddlCampusAttribute.DataTextField = "text";
            ddlCampusAttribute.DataValueField = "value";
            ddlCampusAttribute.DataBind();
            ddlCampusAttribute.Items.Insert( 0, new ListItem() );
            ddlCampusAttribute.SelectedValue = GetAttributeValue( AttributeKeys.CampusAttribute );

            MetricRowsState = GetAttributeValue( AttributeKeys.Metrics ).FromJsonOrNull<List<MetricRow>>() ?? new List<MetricRow>();
            BindMetricGrid();
            upnlContent.Update();
        }

        /// <summary>
        /// Binds the Metric grid.
        /// </summary>
        private void BindMetricGrid( bool isEditMode = false )
        {
            var reorderField = gMetrics.ColumnsOfType<ReorderField>().FirstOrDefault();
            if ( reorderField != null )
            {
                reorderField.Visible = !isEditMode;
            }
            gMetrics.DataSource = MetricRowsState.OrderBy( a => a.Order );
            gMetrics.DataBind();
        }

        /// <summary>
        /// Reorders the metric rows.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        private void ReorderMetricRows( List<MetricRow> itemList, int oldIndex, int newIndex )
        {
            var movedItem = itemList.Where( a => a.Order == oldIndex ).FirstOrDefault();
            if ( movedItem != null )
            {
                if ( newIndex < oldIndex )
                {
                    // Moved up
                    foreach ( var otherItem in itemList.Where( a => a.Order < oldIndex && a.Order >= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order + 1;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherItem in itemList.Where( a => a.Order > oldIndex && a.Order <= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order - 1;
                    }
                }

                movedItem.Order = newIndex;
            }
        }

        /// <summary>
        /// Sets the panel title and icon.
        /// </summary>
        private void SetPanelTitleAndIcon()
        {
            string title = this.GetAttributeValue( AttributeKeys.Title );
            if ( !string.IsNullOrEmpty( title ) )
            {
                lTitle.Text = title;
            }
            else
            {
                lTitle.Text = "Campus Metrics";
            }

            string panelIcon = this.GetAttributeValue( AttributeKeys.IconCssClass );
            if ( !string.IsNullOrEmpty( panelIcon ) )
            {
                iIcon.Attributes["class"] = panelIcon;
            }
            else
            {
                iIcon.Attributes["class"] = "fas fa-building";
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            List<CampusCache> campuses = new List<CampusCache>();
            if ( _campusId.HasValue )
            {
                var campus = CampusCache.Get( _campusId.Value );
                if ( campus != null )
                {
                    campuses.Add( campus );
                }
            }
            else
            {
                campuses = CampusCache.All();
            }

            var rockContext = new RockContext();
            List<MetricData> metricDatas = new List<MetricData>();
            foreach ( var campus in campuses )
            {
                MetricData metricData = new MetricData()
                {
                    CampusId = campus.Id,
                    CampusName = campus.Name
                };

                metricData.Numerators = new Dictionary<string, decimal>();
                metricData.Denominators = new Dictionary<string, decimal>();

                foreach ( var column in MetricRowsState )
                {
                    Dictionary<int, decimal> values = new Dictionary<int, decimal>();
                    if ( column.ValueOptionType == ValueOptionType.Current )
                    {
                        values = GetCurrentMetricValues( column, campus.Id, rockContext );
                    }
                    else
                    {
                        values = GetSum4MetricValues( column, campus.Id, rockContext );
                    }

                    if ( values.ContainsKey( column.NumeratorMetricId ) )
                    {
                        metricData.Numerators.AddOrReplace( GetColumnKey( column ), values[column.NumeratorMetricId] );
                    }
                    else
                    {
                        metricData.Numerators.AddOrReplace( GetColumnKey( column ), 0 );
                    }

                    if ( values.ContainsKey( column.DenominatorMetricId ) )
                    {
                        metricData.Denominators.AddOrReplace( GetColumnKey( column ), values[column.DenominatorMetricId] );
                    }
                    else
                    {
                        metricData.Denominators.AddOrReplace( GetColumnKey( column ), 0 );
                    }
                }
                metricDatas.Add( metricData );
            }

            SortProperty sortProperty = gCampusMetrics.SortProperty;
            if ( sortProperty != null )
            {
                gCampusMetrics.DataSource = metricDatas.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                gCampusMetrics.DataSource = metricDatas
                    .OrderBy( r => r.CampusName )
                    .ToList();
            }
            gCampusMetrics.DataBind();
        }

        private static string GetColumnKey( MetricRow column )
        {
            return string.Format( "l{0}{1}", column.Label, column.Order );
        }

        private Dictionary<int, decimal> GetSum4MetricValues( MetricRow metricRow, int campusId, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            MetricValueService metricValueService = new MetricValueService( rockContext );
            var qry = metricValueService.Queryable().Include( a => a.MetricValuePartitions );
            qry = qry.Where( a => ( metricRow.NumeratorMetricId == a.MetricId || metricRow.DenominatorMetricId == a.MetricId ) && a.MetricValuePartitions.Any( x => x.MetricPartition.EntityTypeId == _campusEntityId && x.EntityId == campusId ) );

            return qry.GroupBy( a => a.MetricId ).Select( a => new
            {
                Id = a.Key,
                Value = a.OrderByDescending( b => b.MetricValueDateTime ).Skip( 1 ).Take( 4 ).Where( b => b.YValue.HasValue ).Select( b => b.YValue.Value ).DefaultIfEmpty( 0 ).Sum()
            } ).ToDictionary( a => a.Id, b => b.Value );
        }

        private Dictionary<int, decimal> GetCurrentMetricValues( MetricRow metricRow, int campusId, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            MetricValueService metricValueService = new MetricValueService( rockContext );
            var qry = metricValueService.Queryable().Include( a => a.MetricValuePartitions );
            qry = qry.Where( a => ( metricRow.NumeratorMetricId == a.MetricId || metricRow.DenominatorMetricId == a.MetricId ) && a.MetricValuePartitions.Any( x => x.MetricPartition.EntityTypeId == _campusEntityId && x.EntityId == campusId ) );

            return qry.GroupBy( a => a.MetricId ).Select( a => new
            {
                Id = a.Key,
                Value = a.OrderByDescending( b => b.MetricValueDateTime ).Take( 1 ).Where( b => b.YValue.HasValue ).Select( b => b.YValue.Value ).DefaultIfEmpty( 0 ).FirstOrDefault()
            } ).ToDictionary( a => a.Id, b => b.Value );
        }

        /// <summary>
        /// Display the Campus Metric Grid
        /// </summary>
        private void ShowView()
        {
            upnlContent.Update();


            var rockLiteralFields = gCampusMetrics.Columns.OfType<RockLiteralField>().ToList();
            foreach ( var field in rockLiteralFields )
            {
                gCampusMetrics.Columns.Remove( field );
            }

            MetricRowsState = GetAttributeValue( AttributeKeys.Metrics ).FromJsonOrNull<List<MetricRow>>() ?? new List<MetricRow>();
            foreach ( var column in MetricRowsState.OrderBy( a => a.Order ) )
            {
                var rockLiteralField = new RockLiteralField();
                gCampusMetrics.Columns.Add( rockLiteralField );
                rockLiteralField.HeaderText = column.Label;
                rockLiteralField.HeaderStyle.CssClass = "col-md-3";
                rockLiteralField.ID = GetColumnKey( column );
            }

            BindGrid();
        }

        #endregion

        #region Helper Classes

        public class MetricRow
        {
            public int Order { get; set; }
            public Guid Guid { get; set; }
            public string Label { get; set; }
            public int NumeratorMetricId { get; set; }
            public int DenominatorMetricId { get; set; }
            public ValueOptionType ValueOptionType { get; set; }
        }

        public enum ValueOptionType
        {
            Current,
            SumOfPrevious4
        }

        /// <summary>
        /// A class to store the metric data
        /// </summary>
        public class MetricData
        {
            /// <summary>
            /// Gets or sets the campus identifier.
            /// </summary>
            /// <value>
            /// The campus identifier.
            /// </value>
            public int CampusId { get; set; }

            /// <summary>
            /// Gets or sets the campus name.
            /// </summary>
            /// <value>
            /// The campus name.
            /// </value>
            public string CampusName { get; set; }

            /// <summary>
            /// Gets or sets the numerators.
            /// </summary>
            /// <value>
            /// The numerators.
            /// </value>
            public Dictionary<string, decimal> Numerators { get; set; }

            /// <summary>
            /// Gets or sets the denominators.
            /// </summary>
            /// <value>
            /// The denominators.
            /// </value>
            public Dictionary<string, decimal> Denominators { get; set; }

        }

        #endregion

    }
}