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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Blocks.Types.Web.Events;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// A Block that displays the fees related to an event registration instance.
    /// </summary>
    [DisplayName( "Registration Instance - Fee List" )]
    [Category( "Event" )]
    [Description( "Displays the fees related to an event registration instance." )]

    public partial class RegistrationInstanceFeeList : RegistrationInstanceBlock, ISecondaryBlock
    {
        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The Registration Instance identifier
            /// </summary>
            public const string RegistrationInstanceId = "RegistrationInstanceId";
        }

        #endregion Page Parameter Keys

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        //protected override void LoadViewState( object savedState )
        //{
        //    base.LoadViewState( savedState );

        //    RegistrationTemplateId = ViewState["RegistrationTemplateId"] as int? ?? 0;

        //    // don't set the values if this is a postback from a grid 'ClearFilter'
        //    bool setValues = this.Request.Params["__EVENTTARGET"] == null || !this.Request.Params["__EVENTTARGET"].EndsWith( "_lbClearFilter" );
        //}

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            fFees.ApplyFilterClick += fFees_ApplyFilterClick;

            gFees.EmptyDataText = "There are no items to show in this view.";
            gFees.GridRebind += gFees_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            //this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            //InitializeActiveRegistrationInstance();

            if ( !Page.IsPostBack )
            {
                ShowDetail();
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        //protected override object SaveViewState()
        //{
        //    //ViewState["RegistrationTemplateId"] = RegistrationTemplateId;

        //    return base.SaveViewState();
        //}

        /// <summary>
        /// Gets the bread crumbs.
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        //public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        //{
        //    var breadCrumbs = new List<BreadCrumb>();

        //    int? registrationInstanceId = PageParameter( pageReference, PageParameterKey.RegistrationInstanceId ).AsIntegerOrNull();
        //    if ( registrationInstanceId.HasValue )
        //    {
        //        RegistrationInstance registrationInstance = GetRegistrationInstance( registrationInstanceId.Value );
        //        if ( registrationInstance != null )
        //        {
        //            breadCrumbs.Add( new BreadCrumb( registrationInstance.ToString(), pageReference ) );
        //            return breadCrumbs;
        //        }
        //    }

        //    breadCrumbs.Add( new BreadCrumb( "New Registration Instance", pageReference ) );
        //    return breadCrumbs;
        //}

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        //protected void Block_BlockUpdated( object sender, EventArgs e )
        //{
        //}

        #endregion

        #region Events

        /// <summary>
        /// Gets the display value for a filter field.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void fFees_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "FeeDateRange":
                    e.Value = SlidingDateRangePicker.FormatDelimitedValues( e.Value );
                    break;

                case "FeeName":
                    break;

                case "FeeOptions":
                    var values = new List<string>();
                    foreach ( string value in e.Value.Split( ';' ) )
                    {
                        var item = cblFeeOptions.Items.FindByValue( value );
                        if ( item != null )
                        {
                            values.Add( item.Text );
                        }
                    }

                    e.Value = values.AsDelimited( ", " );
                    break;

                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Handles the ClearFilterCick event of the fFees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fFees_ClearFilterCick( object sender, EventArgs e )
        {
            fFees.DeleteUserPreferences();

            BindFeesFilter();
        }

        protected void fFees_ApplyFilterClick( object sender, EventArgs e )
        {
            fFees.SaveUserPreference( "FeeDateRange", "Fee Date Range", sdrpFeeDateRange.DelimitedValues );
            fFees.SaveUserPreference( "FeeName", "Fee Name", ddlFeeName.SelectedItem.Text );
            fFees.SaveUserPreference( "FeeOptions", "Fee Options", cblFeeOptions.SelectedValues.AsDelimited( ";" ) );

            BindFeesGrid();
        }

        protected void ddlFeeName_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( ddlFeeName.SelectedIndex > 0 )
            {
                Populate_cblFeeOptions();
                cblFeeOptions.Visible = true;
            }
        }

        #endregion

        #region Methods

        //private RegistrationInstance _RegistrationInstance = null;

        /// <summary>
        /// Load the active Registration Instance for the current page context.
        /// </summary>
        //private void InitializeActiveRegistrationInstance()
        //{
        //    _RegistrationInstance = null;

        //    int? registrationInstanceId = this.PageParameter( PageParameterKey.RegistrationInstanceId ).AsInteger();

        //    if ( registrationInstanceId != 0 )
        //    {
        //        _RegistrationInstance = GetRegistrationInstance( registrationInstanceId.Value );
        //    }

        //    hfRegistrationInstanceId.Value = registrationInstanceId.ToString();
        //}

        /// <summary>
        /// Gets the registration instance.
        /// </summary>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        //private RegistrationInstance GetRegistrationInstance( int registrationInstanceId, RockContext rockContext = null )
        //{
        //    string key = string.Format( "RegistrationInstance:{0}", registrationInstanceId );
        //    RegistrationInstance registrationInstance = RockPage.GetSharedItem( key ) as RegistrationInstance;
        //    if ( registrationInstance == null )
        //    {
        //        rockContext = rockContext ?? new RockContext();
        //        registrationInstance = new RegistrationInstanceService( rockContext )
        //            .Queryable( "RegistrationTemplate,Account,RegistrationTemplate.Forms.Fields" )
        //            .AsNoTracking()
        //            .FirstOrDefault( i => i.Id == registrationInstanceId );
        //        RockPage.SaveSharedItem( key, registrationInstance );
        //    }

        //    return registrationInstance;
        //}

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            //int? registrationInstanceId = PageParameter( PageParameterKey.RegistrationInstanceId ).AsIntegerOrNull();
            //int? parentTemplateId = PageParameter( "RegistrationTemplateId" ).AsIntegerOrNull();

            var registrationInstance = this.RegistrationInstance;

            if ( registrationInstance == null )
            {
                //pnlDetails.Visible = false;
                return;
            }

            //using ( var rockContext = new RockContext() )
            //{
                
                //if ( registrationInstanceId.HasValue )
                //{
                //    registrationInstance = GetRegistrationInstance( registrationInstanceId.Value, rockContext );
                //}

                //if ( registrationInstance == null )
                //{
                //    registrationInstance = new RegistrationInstance();
                //    registrationInstance.Id = 0;
                //    registrationInstance.IsActive = true;
                //    registrationInstance.RegistrationTemplateId = parentTemplateId ?? 0;

                //    Guid? accountGuid = GetAttributeValue( "DefaultAccount" ).AsGuidOrNull();
                //    if ( accountGuid.HasValue )
                //    {
                //        var account = new FinancialAccountService( rockContext ).Get( accountGuid.Value );
                //        registrationInstance.AccountId = account != null ? account.Id : 0;
                //    }
                //}

                //if ( registrationInstance.RegistrationTemplate == null && registrationInstance.RegistrationTemplateId > 0 )
                //{
                //    registrationInstance.RegistrationTemplate = new RegistrationTemplateService( rockContext )
                //        .Get( registrationInstance.RegistrationTemplateId );
                //}

                pnlDetails.Visible = true;
                //hfRegistrationInstanceId.Value = registrationInstance.Id.ToString();
                //hfRegistrationTemplateId.Value = registrationInstance.RegistrationTemplateId.ToString();
                //RegistrationTemplateId = registrationInstance.RegistrationTemplateId;

                BindFeesFilter();
                BindFeesGrid();
            //}
        }

        /// <summary>
        /// Binds the fees filter.
        /// </summary>
        private void BindFeesFilter()
        {
            sdrpFeeDateRange.DelimitedValues = fFees.GetUserPreference( "FeeDateRange" );
            Populate_ddlFeeName();
            ddlFeeName.SelectedIndex = ddlFeeName.Items.IndexOf( ddlFeeName.Items.FindByText( fFees.GetUserPreference( "FeeName" ) ) );
            Populate_cblFeeOptions();
        }

        /// <summary>
        /// Binds the fees grid.
        /// </summary>
        private void BindFeesGrid()
        {
            var instanceId = this.RegistrationInstanceId;

            if ( instanceId.GetValueOrDefault(0) == 0 )
            {
                return;
            }

            var registrationTemplateFeeService = new RegistrationTemplateFeeService( new RockContext() );

            var data = registrationTemplateFeeService.GetRegistrationTemplateFeeReport( instanceId.Value );

            // Add Date Range
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( sdrpFeeDateRange.DelimitedValues );
            if ( dateRange.Start.HasValue )
            {
                data = data.Where( r => r.RegistrationDate >= dateRange.Start.Value );
            }

            if ( dateRange.End.HasValue )
            {
                data = data.Where( r => r.RegistrationDate < dateRange.End.Value );
            }

            // Fee Name
            if ( ddlFeeName.SelectedIndex > 0 )
            {
                data = data.Where( r => r.FeeName == ddlFeeName.SelectedItem.Text );
            }

            // Fee Options
            if ( cblFeeOptions.SelectedValues.Count > 0 )
            {
                data = data.Where( r => cblFeeOptions.SelectedValues.Any( v => v.Equals( r.FeeItem.Guid.ToString(), StringComparison.OrdinalIgnoreCase ) ) );
            }

            SortProperty sortProperty = gFees.SortProperty;
            if ( sortProperty != null )
            {
                data = data.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                data = data.OrderByDescending( f => f.RegistrationDate ).ToList();
            }

            gFees.DataSource = data;
            gFees.DataBind();
        }

        /// <summary>
        /// Handles the GridRebind event of the gFees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gFees_GridRebind( object sender, GridRebindEventArgs e )
        {
            var instance = this.RegistrationInstance;

            if ( instance == null )
            {
                return;
            }

            gFees.ExportTitleName = instance.Name + " - Registration Fees";
            gFees.ExportFilename = gFees.ExportFilename ?? instance.Name + "RegistrationFees";

            BindFeesGrid();
        }

        /// <summary>
        /// Populates ddlFeeName with the name of the DDL fee.
        /// </summary>
        private void Populate_ddlFeeName()
        {
            int? instanceId = this.RegistrationInstanceId;

            if ( instanceId.GetValueOrDefault(0) == 0 )
            {
                return;
            }

            var rockContext = new RockContext();
            var registrationInstanceService = new RegistrationInstanceService( rockContext );
            var templateId = registrationInstanceService.Get( ( int ) instanceId ).RegistrationTemplateId;

            var registrationTemplateFeeService = new RegistrationTemplateFeeService( new RockContext() );
            var templateFees = registrationTemplateFeeService.Queryable().Where( f => f.RegistrationTemplateId == templateId ).ToList();

            ddlFeeName.Items.Add( new ListItem() );

            foreach ( var templateFee in templateFees )
            {
                ddlFeeName.Items.Add( new ListItem( templateFee.Name, templateFee.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Populates cblFeeOptions with fee options.
        /// </summary>
        private void Populate_cblFeeOptions()
        {
            cblFeeOptions.Items.Clear();

            int? feeId = ddlFeeName.SelectedValue.AsIntegerOrNull();
            if ( feeId.HasValue )
            {
                var feeItems = new RegistrationTemplateFeeItemService( new RockContext() ).Queryable().Where( a => a.RegistrationTemplateFeeId == feeId );

                foreach ( var feeItem in feeItems )
                {
                    cblFeeOptions.Items.Add( new ListItem( feeItem.Name, feeItem.Guid.ToString() ) );
                }

                string feeOptionValues = fFees.GetUserPreference( "FeeOptions" );
                if ( !string.IsNullOrWhiteSpace( feeOptionValues ) )
                {
                    cblFeeOptions.SetValues( feeOptionValues.Split( ';' ).ToList() );
                }

                cblFeeOptions.Visible = true;
            }
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visibility of the block.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlDetails.Visible = visible;
        }

        #endregion   
    }
}