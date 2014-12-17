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
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.Data;
using System.Data.Entity;
using System.Text;
using Rock.Security;
using Rock.Reporting.Dashboard;


namespace RockWeb.Blocks.Administraton
{
    /// <summary>
    /// Exception List Block
    /// </summary>
    [DisplayName( "Exception List" )]
    [Category( "Core" )]
    [Description( "Lists all exceptions." )]

    [IntegerField( "Summary Count Days", "Summary field for exceptions that have occurred within the last x days. Default value is 7.", false, 7, Order=1)]
    [LinkedPage("Detail Page")]
    [DefinedValueField( Rock.SystemGuid.DefinedType.CHART_STYLES, "Chart Style", Order = 2 )]
    [BooleanField( "Show Legend", "", true, Order = 3 )]
    [CustomDropdownListField( "Legend Position", "Select the position of the Legend (corner)", "ne,nw,se,sw", false, "ne", Order = 4 )]
    public partial class ExceptionList : RockBlock
    {
        #region Control Methods


        /// <summary>
        /// Initializes the control/Rock Block
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            //Only show clear exceptions button if user has edit rights
            btnClearExceptions.Visible = IsUserAuthorized( Authorization.EDIT );

            //Set Exception List filter properties
            fExceptionList.ApplyFilterClick += fExceptionList_ApplyFilterClick;
            fExceptionList.DisplayFilterValue += fExceptionList_DisplayFilterValue;

            //Set properties and events for exception list
            gExceptionList.DataKeyNames = new string[] { "Id" };
            gExceptionList.GridRebind += gExceptionList_GridRebind;
            gExceptionList.RowItemText = "Exception";

            //Set properties and events for Exception Occurrences
            gExceptionOccurrences.DataKeyNames = new string[] { "Id" };
            gExceptionOccurrences.GridRebind += gExceptionOccurrences_GridRebind;
            gExceptionOccurrences.RowSelected += gExceptionOccurrences_RowSelected;
            gExceptionOccurrences.RowItemText = "Exception";

            
            RockPage page = Page as RockPage;
            if ( page != null )
            {
                page.PageNavigate += page_PageNavigate;
            }


            RockPage.AddScriptLink( this.Page, "https://www.google.com/jsapi", false );
            RockPage.AddScriptLink( this.Page, "~/Scripts/jquery.smartresize.js" );
        }

        /// <summary>
        /// Loads the control.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                //Set Exception Panel visibility to show Exception List
                SetExceptionPanelVisibility( None.Id );
            }

            lcExceptions.Options.SetChartStyle( this.ChartStyle );
            lcExceptions.Options.legend = lcExceptions.Options.legend ?? new Legend();
            lcExceptions.Options.legend.show = this.GetAttributeValue( "ShowLegend" ).AsBooleanOrNull();
            lcExceptions.Options.legend.position = this.GetAttributeValue( "LegendPosition" );

            // get data for graphs
            ExceptionLogService exceptionLogService = new ExceptionLogService( new RockContext() );
            var exceptionList = exceptionLogService.Queryable()
            .Where(x => x.HasInnerException == false && x.CreatedDateTime != null)
            .GroupBy( x => DbFunctions.TruncateTime(x.CreatedDateTime.Value ))
            .Select( eg => new
            {
                DateValue = eg.Key,
                ExceptionCount = eg.Count(),
                UniqueExceptionCount = eg.Select( y => y.ExceptionType ).Distinct().Count()
            } ).OrderBy(eg => eg.DateValue).ToList();

            if ( exceptionList.Count == 1 )
            {
                // if there is only one datapoint for the Chart, the yaxis labeling gets messed up, plus the graph wouldn't be useful anyways
                lcExceptions.Visible = false;
            }

        }

        void page_PageNavigate( object sender, HistoryEventArgs e )
        {
            string stateData = e.State["Exception"];

            int exceptionId = 0;
            int.TryParse( stateData, out exceptionId );

            SetExceptionPanelVisibility( exceptionId );
        }

        /// <summary>
        /// Gets the chart style.
        /// </summary>
        /// <value>
        /// The chart style.
        /// </value>
        public ChartStyle ChartStyle
        {
            get
            {
                Guid? chartStyleDefinedValueGuid = this.GetAttributeValue( "ChartStyle" ).AsGuidOrNull();
                if ( chartStyleDefinedValueGuid.HasValue )
                {
                    var rockContext = new Rock.Data.RockContext();
                    var definedValue = new DefinedValueService( rockContext ).Get( chartStyleDefinedValueGuid.Value );
                    if ( definedValue != null )
                    {
                        try
                        {
                            definedValue.LoadAttributes( rockContext );
                            return ChartStyle.CreateFromJson( definedValue.Value, definedValue.GetAttributeValue( "ChartStyle" ) );
                        }
                        catch
                        {
                            // intentionally ignore and default to basic style
                        }
                    }
                }

                return new ChartStyle();
            }
        }

        #endregion

        #region Exception List Grid Events


        /// <summary>
        /// Handles the ApplyFilterClick event of the fExceptionList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fExceptionList_ApplyFilterClick( object sender, EventArgs e )
        {
            if ( ddlSite.SelectedValue != All.IdValue )
            {
                fExceptionList.SaveUserPreference( "Site", ddlSite.SelectedValue );
            }
            else
            {
                fExceptionList.SaveUserPreference( "Site", String.Empty );
            }

            
            if ( ppUser.PersonId.HasValue )
            {
                fExceptionList.SaveUserPreference( "User", ppUser.PersonId.ToString() );
            }
            else
            {
                fExceptionList.SaveUserPreference( "User", String.Empty );
            }

            if ( ppPage.SelectedValueAsInt( false ) != None.Id )
            {
                fExceptionList.SaveUserPreference( "Page", ppPage.SelectedValueAsInt( false ).ToString() );
            }
            else
            {
                fExceptionList.SaveUserPreference( "Page", String.Empty );
            }

            fExceptionList.SaveUserPreference( "Status Code", txtStatusCode.Text );

            DateTime startDate;
            if ( DateTime.TryParse( dpStartDate.Text, out startDate ) )
            {
                fExceptionList.SaveUserPreference( "Start Date", startDate.ToShortDateString() );
            }
            else
            {
                fExceptionList.SaveUserPreference( "Start Date", String.Empty );
            }

            DateTime endDate;
            if ( DateTime.TryParse( dpEndDate.Text, out endDate ) )
            {
                fExceptionList.SaveUserPreference( "End Date", endDate.ToShortDateString() );
            }
            else
            {
                fExceptionList.SaveUserPreference( "End Date", String.Empty );
            }
            BindExceptionListGrid();
        }

        /// <summary>
        /// Build filter values/summary with user friendly data from filters
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void fExceptionList_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Site":
                    int siteId;
                    if ( int.TryParse( e.Value, out siteId ) )
                    {
                        var site = SiteCache.Read( siteId );
                        if ( site != null )
                        {
                            e.Value = site.Name;
                        }
                    }
                    break;
                case "Page":
                    int pageId;
                    if ( int.TryParse( e.Value, out pageId ) )
                    {
                        var page = PageCache.Read( pageId );
                        if ( page != null )
                        {
                            e.Value = page.InternalName;
                        }
                    }
                    break;
                case "User":
                    int userPersonId;
                    if ( int.TryParse( e.Value, out userPersonId ) )
                    {
                        PersonService personService = new PersonService( new RockContext() );
                        var user = personService.Get( userPersonId );
                        if ( user != null )
                        {
                            e.Value = user.FullName;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gExceptionList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gExceptionList_GridRebind( object sender, EventArgs e )
        {
            BindExceptionListGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the gExceptionList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gExceptionList_RowSelected( object sender, RowEventArgs e )
        {
            SetExceptionPanelVisibility( e.RowKeyId );
        }

        #endregion

        #region Exception Occurrence Grid Events

        /// <summary>
        /// Handles the GridRebind event of the gExceptionOccurrences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gExceptionOccurrences_GridRebind( object sender, EventArgs e )
        {
            int exceptionID = 0;
            if ( int.TryParse( hfBaseExceptionID.Value, out exceptionID ) )
            {
                ExceptionLogService exceptionService = new ExceptionLogService( new RockContext() );
                ExceptionLog baseException = exceptionService.Get( exceptionID );

                BindExceptionOccurrenceGrid( baseException );
            }

        }

        protected void gExceptionOccurrences_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "ExceptionId", e.RowKeyId );
        }

        #endregion

        #region Page Events

        /// <summary>
        /// Handles the Click event of the btnClearExceptions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnClearExceptions_Click( object sender, EventArgs e )
        {
            ClearExceptionLog();
            BindExceptionListGrid();
        }


        /// <summary>
        /// Handles the Click event of the btnReturnToList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnReturnToList_Click( object sender, EventArgs e )
        {
            this.AddHistory( "Exception", None.Id.ToString(), "Exception List" );
            SetExceptionPanelVisibility( None.Id );
        }
        
        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the exception list filter.
        /// </summary>
        private void BindExceptionListFilter()
        {
            BindSitesFilter();

            int siteId;

            var rockContext = new RockContext();

            if ( int.TryParse( fExceptionList.GetUserPreference( "Site" ), out siteId )
                    && ddlSite.Items.FindByValue( siteId.ToString() ) != null )
            {
                ddlSite.SelectedValue = siteId.ToString();
            }

            int pageId;
            if ( int.TryParse( fExceptionList.GetUserPreference( "Page" ), out pageId ) )
            {
                PageService pageService = new PageService( rockContext );
                ppPage.SetValue( pageService.Get( pageId ) );
            }
            else
            {
                ppPage.SetValue( None.Id );
            }

            int userPersonId;
            if ( int.TryParse( fExceptionList.GetUserPreference( "User" ), out userPersonId ) )
            {
                PersonService personService = new PersonService( rockContext );
                ppUser.SetValue( personService.Get( userPersonId ) );
            }

            if ( !String.IsNullOrEmpty( fExceptionList.GetUserPreference( "Status Code" ) ) )
            {
                txtStatusCode.Text = fExceptionList.GetUserPreference( "Status Code" );
            }

            DateTime startDate;
            if ( DateTime.TryParse( fExceptionList.GetUserPreference( "Start Date" ), out startDate ) )
            {
                dpStartDate.Text = startDate.ToShortDateString();
            }

            DateTime endDate;
            if ( DateTime.TryParse( fExceptionList.GetUserPreference( "End Date" ), out endDate ) )
            {
                dpEndDate.Text = endDate.ToShortDateString();
            }

        }

        /// <summary>
        /// Binds the exception list grid.
        /// </summary>
        private void BindExceptionListGrid()
        {
            //get the summary count attribute
            int summaryCountDays = Convert.ToInt32( GetAttributeValue( "SummaryCountDays" ) );

            //set the header text for the subset/summary field
            gExceptionList.Columns[5].HeaderText = string.Format( "Last {0} days", summaryCountDays );

            //get the subset/summary date
            DateTime minSummaryCountDate = RockDateTime.Now.Date.AddDays( -( summaryCountDays ) );

            var exceptionQuery = BuildBaseExceptionListQuery()
                                    .GroupBy( e => new
                                        {
                                            SiteName = e.Site.Name,
                                            PageName = e.Page.InternalName,
                                            Description = e.Description
                                        } )
                                    .Select( eg => new
                                        {
                                            Id = eg.Max( e => e.Id ),
                                            SiteName = eg.Key.SiteName,
                                            PageName = eg.Key.PageName,
                                            Description = eg.Key.Description,
                                            LastExceptionDate = eg.Max( e => e.CreatedDateTime ),
                                            TotalCount = eg.Count(),
                                            SubsetCount = eg.Count( e => e.CreatedDateTime.HasValue && e.CreatedDateTime.Value >= minSummaryCountDate )
                                        } );

            if ( gExceptionList.SortProperty != null )
            {
                gExceptionList.DataSource = exceptionQuery.Sort( gExceptionList.SortProperty ).ToList();
            }
            else
            {
                gExceptionList.DataSource = exceptionQuery.OrderByDescending( e => e.LastExceptionDate ).ToList();
            }

            gExceptionList.DataBind();

        }

        /// <summary>
        /// Binds the exception occurrence grid.
        /// </summary>
        /// <param name="baseException">Exception to base the occurrence grid off of.</param>
        private void BindExceptionOccurrenceGrid( ExceptionLog baseException )
        {
            ExceptionLogService exceptionService = new ExceptionLogService( new RockContext() );

            var query = exceptionService.Queryable()
                .Where( e => e.HasInnerException == null || e.HasInnerException == false )
                .Where( e => e.SiteId == baseException.SiteId )
                .Where( e => e.PageId == baseException.PageId )
                .Where( e => e.Description == baseException.Description )
                .Select( e => new
                    {
                        Id = e.Id,
                        CreatedDateTime = e.CreatedDateTime,
                        FullName = ( e.CreatedByPersonAlias != null &&
                            e.CreatedByPersonAlias.Person != null ) ?
                            e.CreatedByPersonAlias.Person.LastName + ", " + e.CreatedByPersonAlias.Person.NickName : "",
                        Description = e.Description
                    } ).OrderBy(e => e.CreatedDateTime);

            if ( gExceptionOccurrences.SortProperty == null )
            {
                gExceptionOccurrences.DataSource = query.OrderByDescending( e => e.CreatedDateTime ).ToList();
            }
            else
            {
                gExceptionOccurrences.DataSource = query.Sort( gExceptionOccurrences.SortProperty ).ToList();
            }

            gExceptionOccurrences.DataBind();
        }

        /// <summary>
        /// Binds the sites filter.
        /// </summary>
        private void BindSitesFilter()
        {
            SiteService siteService = new SiteService( new RockContext() );
            ddlSite.DataTextField = "Name";
            ddlSite.DataValueField = "Id";
            ddlSite.DataSource = siteService.Queryable().OrderBy( s => s.Name ).ToList();
            ddlSite.DataBind();

            ddlSite.Items.Insert( 0, new ListItem( All.Text, All.IdValue ) );
        }


        /// <summary>
        /// Builds the base query for the Exception List grid data
        /// </summary>
        /// <returns>IQueryable containing filtered ExceptionLog records</returns>
        private IQueryable<ExceptionLog> BuildBaseExceptionListQuery()
        {
            ExceptionLogService exceptionLogService = new ExceptionLogService( new RockContext() );
            IQueryable<ExceptionLog> query = exceptionLogService.Queryable();

            int siteId;
            if ( int.TryParse( fExceptionList.GetUserPreference( "Site" ), out siteId ) && siteId > 0 )
            {
                query = query.Where( e => e.SiteId == siteId );
            }

            int pageId;
            if ( int.TryParse( fExceptionList.GetUserPreference( "Page" ), out pageId ) && pageId > 0 )
            {
                query = query.Where( e => e.PageId == pageId );
            }

            int userPersonID;
            if ( int.TryParse( fExceptionList.GetUserPreference( "User" ), out userPersonID ) && userPersonID > 0 )
            {
                query = query.Where( e => e.CreatedByPersonAlias != null && e.CreatedByPersonAlias.PersonId == userPersonID );
            }

            string statusCode = fExceptionList.GetUserPreference( "Status Code" );
            if ( !String.IsNullOrEmpty( statusCode ) )
            {
                query = query.Where( e => e.StatusCode == statusCode );
            }

            DateTime startDate;
            if ( DateTime.TryParse( fExceptionList.GetUserPreference( "Start Date" ), out startDate ) )
            {
                startDate = startDate.Date;
                query = query.Where( e => e.CreatedDateTime.HasValue && e.CreatedDateTime.Value >= startDate );
            }

            DateTime endDate;
            if ( DateTime.TryParse( fExceptionList.GetUserPreference( "End Date" ), out endDate ) )
            {
                endDate = endDate.Date.AddDays( 1 );
                query = query.Where( e => e.CreatedDateTime.HasValue && e.CreatedDateTime.Value < endDate );
            }

            //Only look for inner exceptions
            query = query.Where( e => e.HasInnerException == null || e.HasInnerException == false );

            return query;
        }


        /// <summary>
        /// Clears the exception log.
        /// </summary>
        private void ClearExceptionLog()
        {
            DbService.ExecuteCommand( "TRUNCATE TABLE ExceptionLog" );
        }

        /// <summary>
        /// Loads the exception list.
        /// </summary>
        private void LoadExceptionList()
        {
            //this.AddHistory( "Exception", None.Id.ToString(), "Exception List" );
            BindExceptionListFilter();
            BindExceptionListGrid();
        }

        /// <summary>
        /// Loads the exception occurrences panel
        /// </summary>
        /// <param name="exceptionId">The Id of the base exception for the grid</param>
        private void LoadExceptionOccurrences( int exceptionId )
        {
            //get the base exception
            ExceptionLogService exceptionService = new ExceptionLogService( new RockContext() );
            ExceptionLog exception = exceptionService.Get( exceptionId );

            //set the summary fields for the base exception
            if ( exception != null )
            {
                if ( Page.IsPostBack && Page.IsAsync )
                {
                    this.AddHistory( "Exception", exceptionId.ToString(), string.Format( "Exception Occurrences {0}", exception.Description ) );
                }
                hfBaseExceptionID.Value = exceptionId.ToString();

                var descriptionList = new Rock.Web.DescriptionList();

                // set detail title with formating
                lDetailTitle.Text = String.Format( "Occurrences of {0}", exception.ExceptionType ).FormatAsHtmlTitle();

                if ( !string.IsNullOrEmpty( exception.ExceptionType ) )
                {
                    descriptionList.Add( "Type", exception.ExceptionType );
                }

                if ( exception.Site != null )
                {
                    descriptionList.Add( "Site", exception.Site.Name );
                }

                if ( exception.Page != null )
                {
                    descriptionList.Add( "Page", exception.Page.InternalName );
                }
                
                lblMainDetails.Text = descriptionList.Html;

                //Load the occurrences for the selected exception
                BindExceptionOccurrenceGrid( exception );
            }
        }

        /// <summary>
        /// Sets the visibility of the exception panels
        /// </summary>
        /// <param name="baseExceptionId">The base exception id.</param>
        private void SetExceptionPanelVisibility( int baseExceptionId )
        {
            //initially hide both panels
            pnlExceptionGroups.Visible = false;
            pnlExceptionOccurrences.Visible = false;

            if ( baseExceptionId == None.Id )
            {
                //Load exception List if no base ExceptionId
                LoadExceptionList();
                pnlExceptionGroups.Visible = true;
            }
            else
            {
                //If an exception id is passed in load related exception occurrences.
                LoadExceptionOccurrences( baseExceptionId );
                pnlExceptionOccurrences.Visible = true;
            }
        }

        #endregion
}

}