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

using com.bemaservices.HrManagement.Model;

namespace RockWeb.Plugins.com_bemaservices.HrManagement
{
    [DisplayName( "PTO Bracket List" )]
    [Category( "BEMA Services > HR Management" )]
    [Description( "Lists all the brackets for a given PTO Tier." )]
    [LinkedPage( "Detail Page" )]
    public partial class PtoBracketList : RockBlock, ISecondaryBlock, ICustomGridColumns
    {
        #region Fields

        private PtoTier _ptoTier = null;
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
        //public List<AttributeCache> AvailableAttributes { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        //protected override void LoadViewState( object savedState )
        //{
        //    base.LoadViewState( savedState );

        //    AvailableAttributes = ViewState["AvailableAttributes"] as List<AttributeCache>;

        //    AddDynamicControls();
        //}

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            int ptoTierId = 0;

            ptoTierId = PageParameter( "PtoTierId" ).AsInteger();

            if ( ptoTierId != 0 )
            {
                string key = string.Format( "PtoTier:{0}", ptoTierId );
                _ptoTier = RockPage.GetSharedItem( key ) as PtoTier;
                if ( _ptoTier == null )
                {
                    _ptoTier = new PtoTierService( new RockContext() ).Get( ptoTierId );
                    RockPage.SaveSharedItem( key, _ptoTier );
                }

                if ( _ptoTier != null )
                {
                    _canEdit = true; //UserCanEdit || _connectionType.IsAuthorized( Authorization.EDIT, CurrentPerson );
                    _canView = true; //_canEdit || _connectionType.IsAuthorized( Authorization.VIEW, CurrentPerson );

                    rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
                    gPtoBrackets.DataKeyNames = new string[] { "Id" };
                    gPtoBrackets.Actions.AddClick += gPtoBrackets_AddClick;
                    gPtoBrackets.GridRebind += gPtoBrackets_GridRebind;
                    gPtoBrackets.ExportFilename = _ptoTier.Name;
                    gPtoBrackets.RowDataBound += GPtoBrackets_RowDataBound;
                    gPtoBrackets.Actions.ShowAdd = _canEdit;
                    gPtoBrackets.IsDeleteEnabled = _canEdit;

                    var deleteField = new DeleteField();
                    gPtoBrackets.Columns.Add( deleteField );
                    deleteField.Click += gPtoBrackets_Delete;
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
                    BindPtoBracketsGrid();
                }
            }
        }

        protected void gPtoBrackets_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var ptoBracketService = new PtoBracketService( rockContext );

            PtoBracket ptoBracket = ptoBracketService.Get( e.RowKeyId );

            if ( ptoBracket != null )
            {

                var ptoBracketTypes = ptoBracket.PtoBracketTypes.ToList();
                var ptoBracketTypeService = new PtoBracketTypeService( rockContext );
                foreach ( var ptoBracketType in ptoBracketTypes )
                {
                    ptoBracketTypeService.Delete( ptoBracketType );
                }
                rockContext.SaveChanges();
                ptoBracketService.Delete( ptoBracket );
                rockContext.SaveChanges();
            }

            BindPtoBracketsGrid();
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        //protected override object SaveViewState()
        //{
        //    ViewState["AvailableAttributes"] = AvailableAttributes;

        //    return base.SaveViewState();
        //}

        #endregion

        #region ConnectionOpportunities Grid

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( MakeKeyUniqueToPtoTier( "Status" ), "Status", cbActive.Checked.ToTrueFalse() );

            //if ( AvailableAttributes != null )
            //{
            //    foreach ( var attribute in AvailableAttributes )
            //    {
            //        var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
            //        if ( filterControl != null )
            //        {
            //            try
            //            {
            //                var values = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
            //                rFilter.SaveUserPreference( MakeKeyUniqueToConnectionType( attribute.Key ), attribute.Name, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
            //            }
            //            catch { }
            //        }
            //    }
            //}

            BindPtoBracketsGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            //if ( AvailableAttributes != null )
            //{
            //    var attribute = AvailableAttributes.FirstOrDefault( a => MakeKeyUniqueToConnectionType( a.Key ) == e.Key );
            //    if ( attribute != null )
            //    {
            //        try
            //        {
            //            var values = JsonConvert.DeserializeObject<List<string>>( e.Value );
            //            e.Value = attribute.FieldType.Field.FormatFilterValues( attribute.QualifierValues, values );
            //            return;
            //        }
            //        catch { }
            //    }
            //}
            /* else  */
            if ( e.Key == MakeKeyUniqueToPtoTier( "Status" ) )
            {
                e.Value = e.Value == "True" ? "Only Show Active Items" : string.Empty;
            }
            else
            {
                e.Value = string.Empty;
            }
        }

        protected void gPtoBrackets_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var ptoBracket = e.Row.DataItem as PtoBracket;
                if ( ptoBracket != null )
                {
                    var lSummary = e.Row.FindControl( "lSummary" ) as Literal;

                    var bracketTypeItems = new List<string>();
                    foreach ( var bracketType in ptoBracket.PtoBracketTypes )
                    {
                        var ptoType = bracketType.PtoType.Name;
                        decimal defaultHours = bracketType.DefaultHours;
                        bracketTypeItems.Add( String.Format( "{0}: {1} hrs", ptoType, defaultHours ) );

                    }

                    lSummary.Text = bracketTypeItems.AsDelimited( "<br/>" );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the DeleteConnectionOpportunity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs" /> instance containing the event data.</param>
        protected void DeletePtoBracket_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                PtoBracketService ptoBracketService = new PtoBracketService( rockContext );
                PtoBracket ptoBracket = ptoBracketService.Get( e.RowKeyId );
                if ( ptoBracket != null )
                {
                    if ( _canEdit || ptoBracket.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                    {
                        string errorMessage;
                        //if ( !ptoBracketService.CanDelete( ptoBracket, out errorMessage ) )
                        //{
                        //    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        //    return;
                        //}

                        int ptoTierId = ptoBracket.PtoTier.Id;
                        ptoBracketService.Delete( ptoBracket );
                        rockContext.SaveChanges();

                    }
                    else
                    {
                        mdGridWarning.Show( "You are not authorized to delete this calendar item", ModalAlertType.Warning );
                    }
                }
            }
            BindPtoBracketsGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the GConnectionOpportunities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        private void GPtoBrackets_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var ptoBracket = e.Row.DataItem as PtoBracket;
                var lStatus = e.Row.FindControl( "lStatus" ) as Literal;
                if ( ptoBracket != null && lStatus != null )
                {
                    lStatus.Text = ptoBracket.IsActive ? "<span class='label label-success'>Active</span>" : "<span class='label label-default'>Inactive</span>";
                }
            }
        }

        /// <summary>
        /// Handles the AddClick event of the gConnectionOpportunities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gPtoBrackets_AddClick( object sender, EventArgs e )
        {
            if ( _canEdit )
            {
                NavigateToLinkedPage( "DetailPage", "PtoBracketId", 0, "PtoTierId", _ptoTier.Id );
            }
        }

        /// <summary>
        /// Handles the Edit event of the gConnectionOpportunities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPtoBracket_Edit( object sender, RowEventArgs e )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                PtoBracketService ptoBracketService = new PtoBracketService( rockContext );
                PtoBracket ptoBracket = ptoBracketService.Get( e.RowKeyId );
                if ( ptoBracket != null )
                {
                    NavigateToLinkedPage( "DetailPage", "PtoBracketId", ptoBracket.Id, "PtoTierId", _ptoTier.Id );
                }
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gConnectionOpportunities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gPtoBrackets_GridRebind( object sender, EventArgs e )
        {
            BindPtoBracketsGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void SetFilter()
        {
            string statusValue = rFilter.GetUserPreference( MakeKeyUniqueToPtoTier( "Status" ) );
            if ( !string.IsNullOrWhiteSpace( statusValue ) )
            {
                cbActive.Checked = statusValue.AsBoolean();
            }

            //BindAttributes();
            //AddDynamicControls();
        }

        /// <summary>
        /// Binds the attributes.
        /// </summary>
        //private void BindAttributes()
        //{
        //    // Parse the attribute filters 
        //    AvailableAttributes = new List<AttributeCache>();
        //    if ( _connectionType != null )
        //    {
        //        int entityTypeId = new ConnectionOpportunity().TypeId;
        //        foreach ( var attributeModel in new AttributeService( new RockContext() ).Queryable()
        //            .Where( a =>
        //                a.EntityTypeId == entityTypeId &&
        //                a.IsGridColumn &&
        //                a.EntityTypeQualifierColumn.Equals( "ConnectionTypeId", StringComparison.OrdinalIgnoreCase ) &&
        //                a.EntityTypeQualifierValue.Equals( _connectionType.Id.ToString() ) )
        //            .OrderBy( a => a.Order )
        //            .ThenBy( a => a.Name ) )
        //        {
        //            AvailableAttributes.Add( AttributeCache.Get( attributeModel ) );
        //        }
        //    }
        //}

        /// <summary>
        /// Adds the attribute columns.
        /// </summary>
        //private void AddDynamicControls()
        //{
        //    // Clear the filter controls
        //    phAttributeFilters.Controls.Clear();

        //    var deleteCol = gConnectionOpportunities.Columns.OfType<DeleteField>().FirstOrDefault();
        //    if ( deleteCol != null )
        //    {
        //        gConnectionOpportunities.Columns.Remove( deleteCol );
        //    }

        //    var securityCol = gConnectionOpportunities.Columns.OfType<SecurityField>().FirstOrDefault();
        //    if ( securityCol != null )
        //    {
        //        gConnectionOpportunities.Columns.Remove( securityCol );
        //    }

        //    // Remove attribute columns
        //    foreach ( var column in gConnectionOpportunities.Columns.OfType<AttributeField>().ToList() )
        //    {
        //        gConnectionOpportunities.Columns.Remove( column );
        //    }

        //    if ( AvailableAttributes != null )
        //    {
        //        foreach ( var attribute in AvailableAttributes )
        //        {
        //            var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filter_" + attribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );
        //            if ( control != null )
        //            {
        //                if ( control is IRockControl )
        //                {
        //                    var rockControl = (IRockControl)control;
        //                    rockControl.Label = attribute.Name;
        //                    rockControl.Help = attribute.Description;
        //                    phAttributeFilters.Controls.Add( control );
        //                }
        //                else
        //                {
        //                    var wrapper = new RockControlWrapper();
        //                    wrapper.ID = control.ID + "_wrapper";
        //                    wrapper.Label = attribute.Name;
        //                    wrapper.Controls.Add( control );
        //                    phAttributeFilters.Controls.Add( wrapper );
        //                }

        //                string savedValue = rFilter.GetUserPreference( MakeKeyUniqueToConnectionType( attribute.Key ) );
        //                if ( !string.IsNullOrWhiteSpace( savedValue ) )
        //                {
        //                    try
        //                    {
        //                        var values = JsonConvert.DeserializeObject<List<string>>( savedValue );
        //                        attribute.FieldType.Field.SetFilterValues( control, attribute.QualifierValues, values );
        //                    }
        //                    catch { }
        //                }
        //            }

        //            string dataFieldExpression = attribute.Key;
        //            bool columnExists = gConnectionOpportunities.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
        //            if ( !columnExists )
        //            {
        //                AttributeField boundField = new AttributeField();
        //                boundField.DataField = dataFieldExpression;
        //                boundField.AttributeId = attribute.Id;
        //                boundField.HeaderText = attribute.Name;

        //                var attributeCache = Rock.Web.Cache.AttributeCache.Get( attribute.Id );
        //                if ( attributeCache != null )
        //                {
        //                    boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
        //                }

        //                gConnectionOpportunities.Columns.Add( boundField );
        //            }
        //        }
        //    }

        //    securityCol = new SecurityField();
        //    securityCol.TitleField = "Name";
        //    securityCol.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.ConnectionOpportunity ) ).Id;
        //    gConnectionOpportunities.Columns.Add( securityCol );

        //    deleteCol = new DeleteField();
        //    gConnectionOpportunities.Columns.Add( deleteCol );
        //    deleteCol.Click += DeleteConnectionOpportunity_Click;
        //}

        /// <summary>
        /// Binds the event calendar items grid.
        /// </summary>
        protected void BindPtoBracketsGrid()
        {
            if ( _ptoTier != null )
            {
                pnlPtoBrackets.Visible = true;

                rFilter.Visible = true;
                gPtoBrackets.Visible = true;

                var rockContext = new RockContext();

                PtoBracketService ptoBracketSerivce = new PtoBracketService( rockContext );
                var qry = ptoBracketSerivce.Queryable()
                    .Where( o => o.PtoTier.Id == _ptoTier.Id );

                // Filter by Active Only
                if ( cbActive.Checked )
                {
                    qry = qry.Where( o => o.IsActive );
                }

                // Filter query by any configured attribute filters
                //if ( AvailableAttributes != null && AvailableAttributes.Any() )
                //{
                //    foreach ( var attribute in AvailableAttributes )
                //    {
                //        var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                //        qry = attribute.FieldType.Field.ApplyAttributeQueryFilter( qry, filterControl, attribute, connectionOpportunityService, Rock.Reporting.FilterMode.SimpleFilter );
                //    }
                //}
                SortProperty sortProperty = gPtoBrackets.SortProperty;

                List<PtoBracket> ptoBrackets = null;
                if ( sortProperty != null )
                {
                    ptoBrackets = qry.Sort( sortProperty ).ToList();
                }
                else
                {
                    ptoBrackets = qry.ToList().OrderBy( a => a.MinimumYear ).ToList();
                }

                //// Only include opportunities that current person is allowed to view
                //var authorizedOpportunities = new List<ConnectionOpportunity>();
                //foreach( var opportunity in connectionOpportunities )
                //{
                //    if ( opportunity.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                //    {
                //        authorizedOpportunities.Add( opportunity );
                //    }
                //}

                gPtoBrackets.DataSource = ptoBrackets;
                gPtoBrackets.DataBind();
            }
            else
            {
                pnlPtoBrackets.Visible = false;
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
        private string MakeKeyUniqueToPtoTier( string key )
        {
            if ( _ptoTier != null )
            {
                return string.Format( "{0}-{1}", _ptoTier.Id, key );
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