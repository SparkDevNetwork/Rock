// <copyright>
// Copyright by Triumph Tech
//
// NOTICE: All information contained herein is, and remains
// the property of Triumph Tech LLC. The intellectual and technical concepts contained
// herein are proprietary to Triumph Tech LLC  and may be covered by U.S. and Foreign Patents,
// patents in process, and are protected by trade secret or copyright law.
//
// Dissemination of this information or reproduction of this material
// is strictly forbidden unless prior written permission is obtained
// from Triumph Tech LLC.
// </copyright>
//
using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

using Newtonsoft.Json.Linq;

using tech.triumph.WistiaIntegration;
using tech.triumph.WistiaIntegration.Model;

namespace RockWeb.Plugins.tech_triumph.WistiaIntegration
{
    /// <summary>
    /// Lists all Media and allows for managing them.
    /// </summary>
    [DisplayName( "Media Interaction List" )]
    [Category( "Triumpn Tech > Wistia Integration" )]
    [Description( "Lists all the Wistia Media and allows for managing them." )]
    public partial class MediaInteractionList : RockBlock
    {
        #region Private Variables

        private WistiaMedia _media = null;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            int mediaId = PageParameter( "MediaId" ).AsInteger();

            if ( mediaId != 0 )
            {
                string key = string.Format( "WistiaMedia:{0}", mediaId );
                _media = RockPage.GetSharedItem( key ) as WistiaMedia;
                if ( _media == null )
                {
                    _media = new WistiaMediaService( new RockContext() ).Get( mediaId );
                    RockPage.SaveSharedItem( key, _media );
                }

                rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
                rFilter.DisplayFilterValue += RFilter_DisplayFilterValue;

                gInteractions.DataKeyNames = new string[] { "Id" };
                gInteractions.GridRebind += gInteraction_GridRebind;
            }
        }

        /// <summary>
        /// rs the filter display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void RFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Date Range":
                    e.Value = SlidingDateRangePicker.FormatDelimitedValues( e.Value );
                    break;

                case "Person":
                    if ( !( this.ContextEntity() is Person ) )
                    {
                        var person = new PersonService( new RockContext() ).Get( e.Value.AsInteger() );
                        if ( person != null )
                        {
                            e.Value = person.FullName;
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }
                    }
                    else
                    {
                        e.Value = string.Empty;
                    }

                    break;
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
                if ( _media != null )
                {
                    pnlContent.Visible = true;
                    BindFilter();
                    BindGrid();
                }
                else
                {
                    pnlContent.Visible = false;
                }
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events
        /// <summary>
        /// Handles the GridRebind event of the gMedia control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gInteraction_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Person", ppFilterPerson.SelectedValue.ToString() );
            rFilter.SaveUserPreference( "Date Range", sdrpFilterDateRange.DelimitedValues );
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gMedia control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gMedia_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var interactionResult = e.Row.DataItem as Interaction;

                dynamic data = JObject.Parse( interactionResult.InteractionData );

                Literal lThumbnail = e.Row.FindControl( "lThumbnail" ) as Literal;
                if ( lThumbnail != null )
                {
                    lThumbnail.Text = string.Format( "<iframe src='{0}' style='height: 45px; width: 100%;'></iframe>", data.IframeHeatmapUrl );
                }

                Literal lClient = e.Row.FindControl( "lClient" ) as Literal;
                if ( lClient != null )
                {
                    lClient.Text = string.Format( "{0} v{1}", data.UserAgentDetails.Browser, data.UserAgentDetails.browser_version );
                }

                Literal lLocation = e.Row.FindControl( "lLocation" ) as Literal;
                if ( lLocation != null )
                {
                    if ( data.City != null && data.City.ToString().Trim() == string.Empty )
                    {
                        lLocation.Text = string.Format( "{0}", data.Country );
                    }
                    else
                    {
                        lLocation.Text = string.Format( "{0}, {1} {2}", data.City, data.Region, data.Country );
                    }
                }

                Literal lOrganization = e.Row.FindControl( "lOrganization" ) as Literal;
                if ( lOrganization != null )
                {
                    lOrganization.Text = string.Format( "{0}", data.IpOrganization );
                }
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            var personId = rFilter.GetUserPreference( "Person" ).AsIntegerOrNull();
            if ( personId.HasValue )
            {
                var person = new PersonService( new RockContext() ).Get( personId.Value );
                ppFilterPerson.SetValue( person );
            }
            else
            {
                ppFilterPerson.SetValue( null );
            }

            sdrpFilterDateRange.DelimitedValues = rFilter.GetUserPreference( "Date Range" );
        }

        /// <summary>
        /// Binds the grid for Wistia Media.
        /// </summary>
        private void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {

                var wistiaChannelTypeMediumValueId = DefinedValueCache.Get( tech.triumph.WistiaIntegration.SystemGuid.DefinedValue.WISTIA_INTERACTION_MEDIUM.AsGuid() ).Id;
                var interactions = new InteractionService( rockContext ).Queryable()
                    .Where( i => i.InteractionComponent.EntityId == _media.Id 
                                    && i.InteractionComponent.InteractionChannel.ChannelEntityId == _media.WistiaProject.WistiaAccountId
                                    && i.InteractionComponent.InteractionChannel.ChannelTypeMediumValueId == wistiaChannelTypeMediumValueId 
                                    && i.Operation == "Watched" );

                // person filter
                var filterPersonId = rFilter.GetUserPreference( "Person" ).AsIntegerOrNull();
                if ( filterPersonId.HasValue )
                {
                    // get the transactions for the person or all the members in the person's giving group (Family)
                    var filterPerson = new PersonService( rockContext ).Get( filterPersonId.Value );
                    if ( filterPerson != null )
                    {
                        // fetch all the possible PersonAliasIds that have this GivingID to help optimize the SQL
                        var personAliasIds = new PersonAliasService( rockContext ).Queryable().Where( a => a.Person.GivingId == filterPerson.GivingId ).Select( a => a.Id ).ToList();

                        // get the transactions for the person or all the members in the person's giving group (Family)
                        interactions = interactions.Where( i => i.PersonAliasId.HasValue && personAliasIds.Contains( i.PersonAliasId.Value ) );
                    }
                }

                // date range filter
                var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( sdrpFilterDateRange.DelimitedValues );
                if ( dateRange.Start.HasValue )
                {
                    interactions = interactions.Where( i => i.InteractionDateTime >= dateRange.Start.Value );
                }
                if ( dateRange.End.HasValue )
                {
                    interactions = interactions.Where( i => i.InteractionDateTime < dateRange.End.Value );
                }

                var items = interactions.AsQueryable().AsNoTracking();

                gInteractions.EntityTypeId = EntityTypeCache.Get<Interaction>().Id;
                var sortProperty = gInteractions.SortProperty;

                if ( sortProperty != null )
                {
                    gInteractions.DataSource = items.Sort( sortProperty ).ToList();
                }
                else
                {
                    gInteractions.DataSource = items.OrderByDescending( p => p.InteractionDateTime ).ToList();
                }

                gInteractions.DataBind();
            }
        }
        #endregion
    }
}