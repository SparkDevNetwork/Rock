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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Connection
{
    [DisplayName( "Connection Opportunity List" )]
    [Category( "Connection" )]
    [Description( "Lists all the opportunities for a given connection type." )]

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "481AE184-4654-48FB-A2B4-90F6604B59B8" )]
    public partial class ConnectionOpportunityList : RockBlock, ISecondaryBlock, ICustomGridColumns
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

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string ConnectionTypeId = "ConnectionTypeId";
            public const string ConnectionOpportunityId = "ConnectionOpportunityId";
        }

        #endregion

        #region Fields

        private ConnectionType _connectionType = null;
        private bool _canView = false;
        private bool _canEdit = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the available attributes.
        /// </summary>
        /// <value>
        /// The available attributes.
        /// </value>
        public List<AttributeCache> AvailableAttributes { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            AvailableAttributes = ViewState["AvailableAttributes"] as List<AttributeCache>;

            AddDynamicControls();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // if this block has a specific ConnectionTypeId set, use that, otherwise, determine it from the PageParameters
            Guid connectionTypeGuid = GetAttributeValue( "ConnectionType" ).AsGuid();
            int connectionTypeId = 0;

            if ( connectionTypeGuid == Guid.Empty )
            {
                connectionTypeId = PageParameter( PageParameterKey.ConnectionTypeId ).AsInteger();
            }

            if ( !( connectionTypeId == 0 && connectionTypeGuid == Guid.Empty ) )
            {
                string key = string.Format( "ConnectionType:{0}", connectionTypeId );
                _connectionType = RockPage.GetSharedItem( key ) as ConnectionType;
                if ( _connectionType == null )
                {
                    _connectionType = new ConnectionTypeService( new RockContext() ).Queryable()
                        .Where( g => g.Id == connectionTypeId || g.Guid == connectionTypeGuid )
                        .FirstOrDefault();
                    RockPage.SaveSharedItem( key, _connectionType );
                }

                if ( _connectionType != null )
                {
                    _canEdit = UserCanEdit || _connectionType.IsAuthorized( Authorization.EDIT, CurrentPerson );
                    _canView = _canEdit || _connectionType.IsAuthorized( Authorization.VIEW, CurrentPerson );

                    rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
                    gConnectionOpportunities.DataKeyNames = new string[] { "Id" };
                    gConnectionOpportunities.Actions.AddClick += gConnectionOpportunities_AddClick;
                    gConnectionOpportunities.GridReorder += gConnectionOpportunities_GridReorder;
                    gConnectionOpportunities.GridRebind += gConnectionOpportunities_GridRebind;
                    gConnectionOpportunities.ExportFilename = _connectionType.Name;
                    gConnectionOpportunities.RowDataBound += GConnectionOpportunities_RowDataBound;
                    gConnectionOpportunities.Actions.ShowAdd = _canEdit;
                    gConnectionOpportunities.IsDeleteEnabled = _canEdit;

                    var reorderField = gConnectionOpportunities.ColumnsOfType<ReorderField>().FirstOrDefault();

                    if ( reorderField != null )
                    {
                        reorderField.Visible = _canEdit;
                    }
                }
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
                pnlContent.Visible = _canView;
                if ( _canView )
                {
                    SetFilter();
                    BindConnectionOpportunitiesGrid();
                }
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
            ViewState["AvailableAttributes"] = AvailableAttributes;

            return base.SaveViewState();
        }

        #endregion

        #region ConnectionOpportunities Grid

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SetFilterPreference( MakeKeyUniqueToConnectionType( "Status" ), "Status", cbActive.Checked.ToTrueFalse() );

            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                    if ( filterControl != null )
                    {
                        try
                        {
                            var values = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                            rFilter.SetFilterPreference( MakeKeyUniqueToConnectionType( attribute.Key ), attribute.Name, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
                        }
                        catch
                        {
                        }
                    }
                }
            }

            BindConnectionOpportunitiesGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( e.Key == MakeKeyUniqueToConnectionType( "Status" ) )
            {
                e.Value = e.Value == "True" ? "Only Show Active Items" : string.Empty;
            }
            else if ( AvailableAttributes != null )
            {
                var attribute = AvailableAttributes.FirstOrDefault( a => MakeKeyUniqueToConnectionType( a.Key ) == e.Key );
                if ( attribute != null )
                {
                    try
                    {
                        var values = JsonConvert.DeserializeObject<List<string>>( e.Value );
                        e.Value = attribute.FieldType.Field.FormatFilterValues( attribute.QualifierValues, values );
                        return;
                    }
                    catch
                    {
                    }
                }

                e.Value = string.Empty;
            }
            else
            {
                e.Value = string.Empty;
            }
        }

        /// <summary>
        /// Handles the Click event of the DeleteConnectionOpportunity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs" /> instance containing the event data.</param>
        protected void DeleteConnectionOpportunity_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                ConnectionOpportunityService connectionOpportunityService = new ConnectionOpportunityService( rockContext );
                ConnectionOpportunity connectionOpportunity = connectionOpportunityService.Get( e.RowKeyId );
                if ( connectionOpportunity != null )
                {
                    if ( _canEdit || connectionOpportunity.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                    {
                        string errorMessage;
                        if ( !connectionOpportunityService.CanDelete( connectionOpportunity, out errorMessage ) )
                        {
                            mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                            return;
                        }

                        int connectionTypeId = connectionOpportunity.ConnectionTypeId;
                        connectionOpportunityService.Delete( connectionOpportunity );
                        rockContext.SaveChanges();

                        ConnectionWorkflowService.RemoveCachedTriggers();
                    }
                    else
                    {
                        mdGridWarning.Show( "You are not authorized to delete this calendar item", ModalAlertType.Warning );
                    }
                }
            }

            BindConnectionOpportunitiesGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the GConnectionOpportunities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        private void GConnectionOpportunities_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var connectionOpportunity = e.Row.DataItem as ConnectionOpportunity;
                var lStatus = e.Row.FindControl( "lStatus" ) as Literal;
                if ( connectionOpportunity != null && lStatus != null )
                {
                    lStatus.Text = connectionOpportunity.IsActive ? "<span class='label label-success'>Active</span>" : "<span class='label label-default'>Inactive</span>";
                }
            }
        }

        /// <summary>
        /// Handles the AddClick event of the gConnectionOpportunities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gConnectionOpportunities_AddClick( object sender, EventArgs e )
        {
            if ( _canEdit )
            {
                NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.ConnectionOpportunityId, 0, PageParameterKey.ConnectionTypeId, _connectionType.Id );
            }
        }

        /// <summary>
        /// Handles the Edit event of the gConnectionOpportunities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gConnectionOpportunities_Edit( object sender, RowEventArgs e )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                ConnectionOpportunityService connectionOpportunityService = new ConnectionOpportunityService( rockContext );
                ConnectionOpportunity connectionOpportunity = connectionOpportunityService.Get( e.RowKeyId );
                if ( connectionOpportunity != null )
                {
                    NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.ConnectionOpportunityId, connectionOpportunity.Id, PageParameterKey.ConnectionTypeId, _connectionType.Id );
                }
            }
        }

        /// <summary>
        /// Handles the GridReorder event of the gConnectionOpportunities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs" /> instance containing the event data.</param>
        protected void gConnectionOpportunities_GridReorder( object sender, GridReorderEventArgs e )
        {
            if ( _connectionType == null )
            {
                return;
            }

            var rockContext = new RockContext();

            var connectionOpportunityService = new ConnectionOpportunityService( rockContext );
            var connectionOpportunities = connectionOpportunityService.Queryable().Where( o => o.ConnectionTypeId == _connectionType.Id ).OrderBy( o => o.Order ).ThenBy( o => o.Name );
            connectionOpportunityService.Reorder( connectionOpportunities.ToList(), e.OldIndex, e.NewIndex );
            rockContext.SaveChanges();

            BindConnectionOpportunitiesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gConnectionOpportunities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gConnectionOpportunities_GridRebind( object sender, EventArgs e )
        {
            BindConnectionOpportunitiesGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void SetFilter()
        {
            string statusValue = rFilter.GetFilterPreference( MakeKeyUniqueToConnectionType( "Status" ) );
            if ( !string.IsNullOrWhiteSpace( statusValue ) )
            {
                cbActive.Checked = statusValue.AsBoolean();
            }

            BindAttributes();
            AddDynamicControls();
        }

        /// <summary>
        /// Binds the attributes.
        /// </summary>
        private void BindAttributes()
        {
            // Parse the attribute filters 
            AvailableAttributes = new List<AttributeCache>();
            if ( _connectionType != null )
            {
                int entityTypeId = new ConnectionOpportunity().TypeId;
                foreach ( var attributeModel in new AttributeService( new RockContext() ).Queryable()
                    .Where( a =>
                        a.EntityTypeId == entityTypeId &&
                        a.IsGridColumn &&
                        a.EntityTypeQualifierColumn.Equals( "ConnectionTypeId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( _connectionType.Id.ToString() ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name ) )
                {
                    AvailableAttributes.Add( AttributeCache.Get( attributeModel ) );
                }
            }
        }

        /// <summary>
        /// Adds the attribute columns.
        /// </summary>
        private void AddDynamicControls()
        {
            // Clear the filter controls
            phAttributeFilters.Controls.Clear();

            var deleteCol = gConnectionOpportunities.Columns.OfType<DeleteField>().FirstOrDefault();
            if ( deleteCol != null )
            {
                gConnectionOpportunities.Columns.Remove( deleteCol );
            }

            var securityCol = gConnectionOpportunities.Columns.OfType<SecurityField>().FirstOrDefault();
            if ( securityCol != null )
            {
                gConnectionOpportunities.Columns.Remove( securityCol );
            }

            // Remove attribute columns
            foreach ( var column in gConnectionOpportunities.Columns.OfType<AttributeField>().ToList() )
            {
                gConnectionOpportunities.Columns.Remove( column );
            }

            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filter_" + attribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );
                    if ( control != null )
                    {
                        if ( control is IRockControl )
                        {
                            var rockControl = ( IRockControl ) control;
                            rockControl.Label = attribute.Name;
                            rockControl.Help = attribute.Description;
                            phAttributeFilters.Controls.Add( control );
                        }
                        else
                        {
                            var wrapper = new RockControlWrapper();
                            wrapper.ID = control.ID + "_wrapper";
                            wrapper.Label = attribute.Name;
                            wrapper.Controls.Add( control );
                            phAttributeFilters.Controls.Add( wrapper );
                        }

                        string savedValue = rFilter.GetFilterPreference( MakeKeyUniqueToConnectionType( attribute.Key ) );
                        if ( !string.IsNullOrWhiteSpace( savedValue ) )
                        {
                            try
                            {
                                var values = JsonConvert.DeserializeObject<List<string>>( savedValue );
                                attribute.FieldType.Field.SetFilterValues( control, attribute.QualifierValues, values );
                            }
                            catch
                            {
                            }
                        }
                    }

                    string dataFieldExpression = attribute.Key;
                    bool columnExists = gConnectionOpportunities.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = dataFieldExpression;
                        boundField.AttributeId = attribute.Id;
                        boundField.HeaderText = attribute.Name;

                        var attributeCache = Rock.Web.Cache.AttributeCache.Get( attribute.Id );
                        if ( attributeCache != null )
                        {
                            boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                        }

                        gConnectionOpportunities.Columns.Add( boundField );
                    }
                }
            }

            securityCol = new SecurityField();
            securityCol.TitleField = "Name";
            securityCol.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.ConnectionOpportunity ) ).Id;
            gConnectionOpportunities.Columns.Add( securityCol );

            deleteCol = new DeleteField();
            gConnectionOpportunities.Columns.Add( deleteCol );
            deleteCol.Click += DeleteConnectionOpportunity_Click;
        }

        /// <summary>
        /// Binds the event calendar items grid.
        /// </summary>
        protected void BindConnectionOpportunitiesGrid()
        {
            if ( _connectionType != null )
            {
                pnlConnectionOpportunities.Visible = true;

                rFilter.Visible = true;
                gConnectionOpportunities.Visible = true;

                var rockContext = new RockContext();

                ConnectionOpportunityService connectionOpportunityService = new ConnectionOpportunityService( rockContext );
                var qry = connectionOpportunityService.Queryable()
                    .Where( o => o.ConnectionTypeId == _connectionType.Id );

                // Filter by Active Only
                if ( cbActive.Checked )
                {
                    qry = qry.Where( o => o.IsActive );
                }

                // Filter query by any configured attribute filters
                if ( AvailableAttributes != null && AvailableAttributes.Any() )
                {
                    foreach ( var attribute in AvailableAttributes )
                    {
                        var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                        qry = attribute.FieldType.Field.ApplyAttributeQueryFilter( qry, filterControl, attribute, connectionOpportunityService, Rock.Reporting.FilterMode.SimpleFilter );
                    }
                }

                // Sort GridView by Order and then Name.
                qry = qry.OrderBy( q => q.Order ).ThenBy( q => q.Name );

                // Only include opportunities that current person is allowed to view
                var authorizedOpportunities = new List<ConnectionOpportunity>();
                foreach ( var opportunity in qry.ToList() )
                {
                    if ( opportunity.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        authorizedOpportunities.Add( opportunity );
                    }
                }

                gConnectionOpportunities.DataSource = authorizedOpportunities;
                gConnectionOpportunities.DataBind();
            }
            else
            {
                pnlConnectionOpportunities.Visible = false;
            }
        }

        /// <summary>
        /// Resolves the values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="listControl">The list control.</param>
        /// <returns></returns>
        private string ResolveValues( string values, System.Web.UI.WebControls.CheckBoxList checkBoxList )
        {
            var resolvedValues = new List<string>();

            foreach ( string value in values.Split( ';' ) )
            {
                var item = checkBoxList.Items.FindByValue( value );
                if ( item != null )
                {
                    resolvedValues.Add( item.Text );
                }
            }

            return resolvedValues.AsDelimited( ", " );
        }

        /// <summary>
        /// Makes the key unique to event calendar.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string MakeKeyUniqueToConnectionType( string key )
        {
            if ( _connectionType != null )
            {
                return string.Format( "{0}-{1}", _connectionType.Id, key );
            }

            return key;
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        #endregion
    }
}