using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;

using com.ccvonline.SampleProject.Data;
using com.ccvonline.SampleProject.Model;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_ccvonline.SampleProject
{
    /// <summary>
    /// Lists all the Referral Agencies.
    /// </summary>
    [DisplayName( "Referral Agency List" )]
    [Category( "CCV > Sample Project" )]
    [Description( "Lists all the Referral Agencies." )]

    [LinkedPage( "Detail Page" )]
    public partial class ReferralAgencyList : Rock.Web.UI.RockBlock
    {

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            bool canEdit = IsUserAuthorized( Rock.Security.Authorization.EDIT );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;
            gfSettings.DisplayFilterValue += gfSettings_DisplayFilterValue;

            gAgencies.RowItemText = "Agency";
            gAgencies.DataKeyNames = new string[] { "id" };
            gAgencies.Actions.ShowAdd = canEdit;
            gAgencies.IsDeleteEnabled = canEdit;
            gAgencies.Actions.AddClick += gAgencies_Add;
            gAgencies.GridRebind += gAgencies_GridRebind;

            BindFilter();
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
                cpCampus.SetValue( gfSettings.GetUserPreference( "Campus" ) );
                ddlAgencyType.SelectedValue = gfSettings.GetUserPreference( "Agency Type" );

                BindGrid();
            }
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

        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            gfSettings.SaveUserPreference( "Campus", ( cpCampus.SelectedCampusId != null ? cpCampus.SelectedCampusId.Value.ToString() : string.Empty ) );
            gfSettings.SaveUserPreference( "Agency Type", ddlAgencyType.SelectedValue );

            BindGrid();
        }

        /// <summary>
        /// Gfs the settings_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfSettings_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Campus":
                    {
                        if ( !string.IsNullOrWhiteSpace( e.Value ) )
                        {
                            e.Value = CampusCache.Read( int.Parse( e.Value ) ).Name;
                        }
                        break;
                    }

                case "Agency Type":
                    {
                        int? valueId = gfSettings.GetUserPreference( "Agency Type" ).AsIntegerOrNull();
                        if ( valueId.HasValue )
                        {
                            var definedValue = DefinedValueCache.Read( valueId.Value );
                            if ( definedValue != null )
                            {
                                e.Value = definedValue.Value;
                            }
                        }
                        break;
                    }

                default:
                    {
                        e.Value = string.Empty;
                        break;
                    }
            }

        }

        /// <summary>
        /// Handles the Add event of the gAgencies control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gAgencies_Add( object sender, EventArgs e )
        {
            NavigateToDetailPage( 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gAgencies control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAgencies_Edit( object sender, RowEventArgs e )
        {
            NavigateToDetailPage( e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gAgencies control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAgencies_Delete( object sender, RowEventArgs e )
        {
            var dataContext = new SampleProjectContext();
            var service = new ReferralAgencyService( dataContext );
            var referralAgency = service.Get( (int)e.RowKeyValue );
            if ( referralAgency != null )
            {
                string errorMessage;
                if ( !service.CanDelete( referralAgency, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                service.Delete( referralAgency );
                dataContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gAgencies control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gAgencies_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            var campusi = CampusCache.All();
            cpCampus.Campuses = campusi;
            cpCampus.Visible = campusi.Any();

            var definedType = DefinedTypeCache.Read( com.ccvonline.SampleProject.SystemGuid.DefinedType.REFERRAL_AGENCY_TYPE.AsGuid() );
            if ( definedType != null )
            {
                ddlAgencyType.BindToDefinedType( definedType, true );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var service = new ReferralAgencyService( new SampleProjectContext() );
            SortProperty sortProperty = gAgencies.SortProperty;

            var qry = service.Queryable( "Campus,AgencyTypeValue" );

            int? campusId = gfSettings.GetUserPreference( "Campus" ).AsIntegerOrNull();
            if ( campusId.HasValue )
            {
                qry = qry.Where( a => a.CampusId == campusId.Value );
            }

            int? definedValueId = gfSettings.GetUserPreference( "Agency Type" ).AsIntegerOrNull();
            if ( definedValueId.HasValue )
            {
                qry = qry.Where( a => a.AgencyTypeValueId == definedValueId.Value );
            }

            // Sort results
            if ( sortProperty != null )
            {
                gAgencies.DataSource = qry.Sort( sortProperty ).ToList();
            }
            else
            {
                gAgencies.DataSource = qry.OrderBy( a => a.Name ).ToList();
            }

            gAgencies.DataBind();
        }

        /// <summary>
        /// Navigates to detail page.
        /// </summary>
        /// <param name="referralAgencyId">The referral agency identifier.</param>
        private void NavigateToDetailPage( int referralAgencyId )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "referralAgencyId", referralAgencyId.ToString() );
            qryParams.Add( "campusId", gfSettings.GetUserPreference( "Campus" ) );
            qryParams.Add( "agencyTypeId", gfSettings.GetUserPreference( "Agency Type" ) );
            NavigateToLinkedPage( "DetailPage", qryParams );
        }

        #endregion
    }
}