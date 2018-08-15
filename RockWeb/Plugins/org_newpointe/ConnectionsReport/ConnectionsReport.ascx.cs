using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Security;
using Rock.Model;
using Rock.Web.UI.Controls;
using System.Data.Entity;
using Rock.Web.Cache;

namespace RockWeb.Plugins.org_newpointe.ConnectionsReport
{
    /// <summary>
    /// Template block for a TreeView.
    /// </summary>
    [DisplayName( "Connections Report" )]
    [Category( "Newpointe Reporting" )]
    [Description( "Connections report/stats." )]

    public partial class ConnectionsReport : Rock.Web.UI.RockBlock
    {

        RockContext _rockContext = null;
        ConnectionOpportunityService conOppServ = null;
        ConnectionRequestActivityService conReqActServ = null;
        ConnectionStatusService conStatServ = null;

        protected string workflowChartData1;
        protected string workflowChartData2;
        protected string workflowChartData3;
        protected string workflowChartData4;
        protected string workflowChartData5;


        protected String workflowGroupedReportTableItemName = "";

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            _rockContext = new RockContext();

            conOppServ = new ConnectionOpportunityService( _rockContext );
            conStatServ = new ConnectionStatusService( _rockContext );
            conReqActServ = new ConnectionRequestActivityService( _rockContext );

            dateRange.UpperValue = DateTime.Now;
            dateRange.LowerValue = DateTime.Now.AddYears( -1 );

            lReadOnlyTitle.Text = "Connection Requests".FormatAsHtmlTitle();
            workflowFilters.Show();

        }

        protected override void OnLoad( EventArgs e )
        {
            ScriptManager.RegisterStartupScript( Page, this.GetType(), "AKey", "initCharts();", true );

            if ( !Page.IsPostBack )
            {
                BindFilters();
            }

        }

        IEnumerable<ConnectionOpportunity> viewableConnectionOpportunities;

        private void BindFilters()
        {
            viewableConnectionOpportunities = conOppServ.Queryable().OrderBy( x => x.Name ).ToList().Where( awt => awt.IsAuthorized( "View", CurrentPerson ) );

            // Opportunities
            var conOppId = rddConnectionOpportunity.SelectedValueAsId();
            rddConnectionOpportunity.DataSource = viewableConnectionOpportunities.ToList();
            rddConnectionOpportunity.DataTextField = "Name";
            rddConnectionOpportunity.DataValueField = "Id";
            rddConnectionOpportunity.DataBind();
            rddConnectionOpportunity.Items.Insert( 0, new ListItem( "", "-1" ) );
            rddConnectionOpportunity.SetValue( conOppId );

            // Campuses
            var campId = campusPicker.SelectedValueAsId();
            campusPicker.Campuses = CampusCache.All();
            campusPicker.SetValue( campId );

            // Statuses
            List<string> statusIds = cblStatus.SelectedValues;
            var typeIds = viewableConnectionOpportunities.Select( o => o.ConnectionTypeId ).ToList();
            cblStatus.DataSource = conStatServ.Queryable().Where( s => typeIds.Contains( s.ConnectionTypeId ?? -1 ) ).Select( s => s.Name ).Distinct().Select( x => new { Name = x, Id = x } ).ToList();
            cblStatus.DataBind();
            cblStatus.SetValues( statusIds );
        }

        private void BindGrids()
        {

            BindFilters();
            var viewableOppIds = viewableConnectionOpportunities.Select( x => x.Id ).ToList();

            var requests = new ConnectionRequestService( _rockContext ).Queryable()
                .Where( r => r.ConnectionRequestActivities.Any( a => a.CreatedDateTime > dateRange.LowerValue && a.CreatedDateTime < dateRange.UpperValue ) )
                .Where( r => viewableOppIds.Contains( r.ConnectionOpportunityId ) );

            var conOppId = rddConnectionOpportunity.SelectedValueAsId();
            if ( conOppId > 0 )
            {
                requests = requests.Where( r => r.ConnectionOpportunityId == conOppId );
            }

            var campId = campusPicker.SelectedValueAsId();
            if ( campId > 0 )
            {
                requests = requests.Where( r => r.CampusId == campId );
            }

            var connectorId = ppAssignedPerson.PersonId;
            if ( connectorId > 0 )
            {
                requests = requests.Where( r => r.ConnectorPersonAlias != null && r.ConnectorPersonAlias.PersonId == connectorId );
            }

            // Filter by State
            var midnightToday = RockDateTime.Today.AddDays( 1 );
            var states = new List<ConnectionState>();
            bool futureFollowup = false;
            foreach ( string stateValue in cblState.SelectedValues )
            {
                futureFollowup = futureFollowup || stateValue.AsInteger() == -2;
                var state = stateValue.ConvertToEnumOrNull<ConnectionState>();
                if ( state.HasValue )
                {
                    states.Add( state.Value );
                }
            }
            if ( futureFollowup || states.Any() )
            {
                requests = requests
                    .Where( r =>
                        ( futureFollowup && r.ConnectionState == ConnectionState.FutureFollowUp &&
                            r.FollowupDate.HasValue && r.FollowupDate.Value < midnightToday ) ||
                        states.Contains( r.ConnectionState ) );
            }


            // Filter by Status
            List<string> statusIds = cblStatus.SelectedValues;
            if ( statusIds.Any() )
            {
                requests = requests
                    .Where( r => statusIds.Contains( r.ConnectionStatus.Name ) );
            }


            SortProperty sortProperty = workflowReportTable.SortProperty;
            if ( sortProperty != null )
            {
                requests = requests.Sort( sortProperty );
            }
            else
            {
                requests = requests
                    .OrderBy( r => (r.ConnectionState != ConnectionState.Inactive && r.ConnectionState != ConnectionState.Connected ) ? 0 : 1 )
                    .ThenBy( r => r.ConnectorPersonAlias.Person.NickName )
                    .ThenBy( r => r.ConnectorPersonAlias.Person.LastName )
                    .ThenBy( r => r.ConnectionOpportunity.Name );
            }


            var requestsData = requests.ToList()
            .Select( r => new
            {
                r.Id,
                r.Guid,
                PersonId = r.PersonAlias.PersonId,
                Name = r.PersonAlias.Person.FullName,
                Campus = r.Campus,
                Group = r.AssignedGroup != null ? r.AssignedGroup.Name : "",
                Connector = r.ConnectorPersonAlias != null ? r.ConnectorPersonAlias.Person.FullName : "",
                LastActivity = FormatActivity( r.ConnectionRequestActivities.OrderByDescending( a => a.CreatedDateTime ).FirstOrDefault() ),
                CreatedDateTime = r.ConnectionRequestActivities.OrderByDescending( a => a.CreatedDateTime ).FirstOrDefault().CreatedDateTime,
                Opened = ( r.CreatedDateTime.HasValue ? ( r.CreatedDateTime.Value.ToShortDateString() + " " + r.CreatedDateTime.Value.ToShortTimeString() + " " ) : "" ) + "(<span class='small'>" + r.CreatedDateTime.ToRelativeDateString() + "</small>)",
                Status = r.ConnectionStatus.Name,
                StatusLabel = r.ConnectionStatus.IsCritical ? "warning" : "info",
                ConnectionState = r.ConnectionState,
                StateLabel = FormatStateLabel( r.ConnectionState, r.FollowupDate ),
                Opportunity = r.ConnectionOpportunity,
                Completed = r.ConnectionState != ConnectionState.Inactive && r.ConnectionState != ConnectionState.Connected ? 0 : 1,
                AgeInt = (DateTime.Now - r.CreatedDateTime).Value.Days
            } )
           .ToList();
            /*

            DateTime oneMonthAgo = DateTime.Now.AddMonths( -1 );
            DateTime twoMonthsAgo = DateTime.Now.AddMonths( -2 );
            DateTime threeMonthsAgo = DateTime.Now.AddMonths( -3 );
            IEnumerable<GroupedData> groupedRequestsData = null;

            switch ( rddlGroupBy.SelectedValue )
            {
                case "1":
                    workflowReportTable.Visible = false;
                    workflowGroupedReportTable.Visible = true;
                    workflowGroupedReportTableItemName = "Workflow";

                    groupedRequestsData = requestsData.GroupBy( x => x.Opportunity.Id ).Select( grp => new GroupedData
                    {
                        GroupedItem = grp.FirstOrDefault().Opportunity.Name,
                        Count = grp.Count(),
                        OneMonthOldStats = "Open: " + grp.Where( x => x.Status != "Inactive" && ( x.CreatedDateTime ?? DateTime.Now ) > oneMonthAgo ).Count() + "<br/>Closed: " + grp.Where( x => x.Completed == 1 && x.AgeInt < 30 ).Count(),
                        TwoMonthsOldStats = "Open: " + grp.Where( x => x.Completed == 0 && ( x.CreatedDateTime ?? DateTime.Now ) < oneMonthAgo && ( x.CreatedDateTime ?? DateTime.Now ) > twoMonthsAgo ).Count() + "<br/>Closed: " + grp.Where( x => x.Completed == 1 && x.AgeInt >= 30 && x.AgeInt < 60 ).Count(),
                        ThreeMonthsOldStats = "Open: " + grp.Where( x => x.Completed == 0 && ( x.CreatedDateTime ?? DateTime.Now ) < twoMonthsAgo && ( x.CreatedDateTime ?? DateTime.Now ) > threeMonthsAgo ).Count() + "<br/>Closed: " + grp.Where( x => x.Completed == 1 && x.AgeInt >= 60 && x.AgeInt < 90 ).Count(),
                        OlderThanThreeMonthsStats = "Open: " + grp.Where( x => x.Completed == 0 && ( x.CreatedDateTime ?? DateTime.Now ) < threeMonthsAgo ).Count() + "<br/>Closed: " + grp.Where( x => x.Completed == 1 && x.AgeInt >= 90 ).Count(),
                        TotalStats = "Open: " + grp.Where( x => x.Completed == 0 ).Count() + "<br/>Closed: " + grp.Where( x => x.Completed == 1 ).Count()
                    } ).OrderBy( x => x.GroupedItem );

                    workflowGroupedReportTable.DataSource = groupedRequestsData.ToList();
                    workflowGroupedReportTable.DataBind();
                    break;
                case "2":
                    workflowReportTable.Visible = false;
                    workflowGroupedReportTable.Visible = true;
                    workflowGroupedReportTableItemName = "Campus";

                    groupedRequestsData = requestsData.GroupBy( x => x.Campus.Id ).Select( grp => new GroupedData
                    {
                        GroupedItem = grp.FirstOrDefault().Campus.Name,
                        Count = grp.Count(),
                        OneMonthOldStats = "Open: " + grp.Where( x => x.Completed == 0 && ( x.CreatedDateTime ?? DateTime.Now ) > oneMonthAgo ).Count() + "<br/>Closed: " + grp.Where( x => x.Completed == 1 && x.AgeInt < 30 ).Count(),
                        TwoMonthsOldStats = "Open: " + grp.Where( x => x.Completed == 0 && ( x.CreatedDateTime ?? DateTime.Now ) < oneMonthAgo && ( x.CreatedDateTime ?? DateTime.Now ) > twoMonthsAgo ).Count() + "<br/>Closed: " + grp.Where( x => x.Completed == 1 && x.AgeInt >= 30 && x.AgeInt < 60 ).Count(),
                        ThreeMonthsOldStats = "Open: " + grp.Where( x => x.Completed == 0 && ( x.CreatedDateTime ?? DateTime.Now ) < twoMonthsAgo && ( x.CreatedDateTime ?? DateTime.Now ) > threeMonthsAgo ).Count() + "<br/>Closed: " + grp.Where( x => x.Completed == 1 && x.AgeInt >= 60 && x.AgeInt < 90 ).Count(),
                        OlderThanThreeMonthsStats = "Open: " + grp.Where( x => x.Completed == 0 && ( x.CreatedDateTime ?? DateTime.Now ) < threeMonthsAgo ).Count() + "<br/>Closed: " + grp.Where( x => x.Completed == 1 && x.AgeInt >= 90 ).Count(),
                        TotalStats = "Open: " + grp.Where( x => x.Completed == 0 ).Count() + "<br/>Closed: " + grp.Where( x => x.Completed == 1 ).Count()
                    } ).OrderBy( x => x.GroupedItem );

                    workflowGroupedReportTable.DataSource = groupedRequestsData.ToList();
                    workflowGroupedReportTable.DataBind();
                    break;
                case "3":
                    workflowReportTable.Visible = false;
                    workflowGroupedReportTable.Visible = true;
                    workflowGroupedReportTableItemName = "Assigned Worker";

                    groupedRequestsData = requestsData.GroupBy( x => x.AssignedEntityName ).Select( grp => new GroupedData
                    {
                        GroupedItem = grp.FirstOrDefault().AssignedEntityName,
                        Count = grp.Count(),
                        OneMonthOldStats = "Open: " + grp.Where( x => x.Completed == 0 && ( x.CreatedDateTime ?? DateTime.Now ) > oneMonthAgo ).Count() + "<br/>Closed: " + grp.Where( x => x.Completed == 1 && x.AgeInt < 30 ).Count(),
                        TwoMonthsOldStats = "Open: " + grp.Where( x => x.Completed == 0 && ( x.CreatedDateTime ?? DateTime.Now ) < oneMonthAgo && ( x.CreatedDateTime ?? DateTime.Now ) > twoMonthsAgo ).Count() + "<br/>Closed: " + grp.Where( x => x.Completed == 1 && x.AgeInt >= 30 && x.AgeInt < 60 ).Count(),
                        ThreeMonthsOldStats = "Open: " + grp.Where( x => x.Completed == 0 && ( x.CreatedDateTime ?? DateTime.Now ) < twoMonthsAgo && ( x.CreatedDateTime ?? DateTime.Now ) > threeMonthsAgo ).Count() + "<br/>Closed: " + grp.Where( x => x.Completed == 1 && x.AgeInt >= 60 && x.AgeInt < 90 ).Count(),
                        OlderThanThreeMonthsStats = "Open: " + grp.Where( x => x.Completed == 0 && ( x.CreatedDateTime ?? DateTime.Now ) < threeMonthsAgo ).Count() + "<br/>Closed: " + grp.Where( x => x.Completed == 1 && x.AgeInt >= 90 ).Count(),
                        TotalStats = "Open: " + grp.Where( x => x.Completed == 0 ).Count() + "<br/>Closed: " + grp.Where( x => x.Completed == 1 ).Count()
                    } ).OrderBy( x => x.GroupedItem );

                    workflowGroupedReportTable.DataSource = groupedRequestsData.ToList();
                    workflowGroupedReportTable.DataBind();
                    break;
                case "4":
                    workflowReportTable.Visible = false;
                    workflowGroupedReportTable.Visible = true;
                    workflowGroupedReportTableItemName = "Status";

                    groupedRequestsData = requestsData.GroupBy( x => x.Status ).Select( grp => new GroupedData
                    {
                        GroupedItem = grp.FirstOrDefault().Status,
                        Count = grp.Count(),
                        OneMonthOldStats = "Open: " + grp.Where( x => x.Completed == 0 && ( x.CreatedDateTime ?? DateTime.Now ) > oneMonthAgo ).Count() + "<br/>Closed: " + grp.Where( x => x.Completed == 1 && x.AgeInt < 30 ).Count(),
                        TwoMonthsOldStats = "Open: " + grp.Where( x => x.Completed == 0 && ( x.CreatedDateTime ?? DateTime.Now ) < oneMonthAgo && ( x.CreatedDateTime ?? DateTime.Now ) > twoMonthsAgo ).Count() + "<br/>Closed: " + grp.Where( x => x.Completed == 1 && x.AgeInt >= 30 && x.AgeInt < 60 ).Count(),
                        ThreeMonthsOldStats = "Open: " + grp.Where( x => x.Completed == 0 && ( x.CreatedDateTime ?? DateTime.Now ) < twoMonthsAgo && ( x.CreatedDateTime ?? DateTime.Now ) > threeMonthsAgo ).Count() + "<br/>Closed: " + grp.Where( x => x.Completed == 1 && x.AgeInt >= 60 && x.AgeInt < 90 ).Count(),
                        OlderThanThreeMonthsStats = "Open: " + grp.Where( x => x.Completed == 0 && ( x.CreatedDateTime ?? DateTime.Now ) < threeMonthsAgo ).Count() + "<br/>Closed: " + grp.Where( x => x.Completed == 1 && x.AgeInt >= 90 ).Count(),
                        TotalStats = "Open: " + grp.Where( x => x.Completed == 0 ).Count() + "<br/>Closed: " + grp.Where( x => x.Completed == 1 ).Count()
                    } ).OrderBy( x => x.GroupedItem );

                    workflowGroupedReportTable.DataSource = groupedRequestsData.ToList();
                    workflowGroupedReportTable.DataBind();
                    break;
                default:*/
            workflowReportTable.Visible = true;
                    workflowGroupedReportTable.Visible = false;

                    workflowReportTable.DataSource = requestsData;
                    workflowReportTable.DataKeyNames = new string[] { "Id" };
                    workflowReportTable.DataBind();
            /*
            break;
    }
    */


            //IQueryable<int> tmp = workflowData.AsQueryable().Where(x => x.Completed == 1).Select(x => ((x.CompletedDateTime - x.CreatedDateTime) ?? TimeSpan.Zero).Days);
            //int _0to90_AverageTime = tmp.Count() == 0 ? -1 : (int)tmp.Average();
            //rlWorkflowStats.Text = "Average time to completion is " + _0to90_AverageTime + " days.";

            System.Web.Script.Serialization.JavaScriptSerializer jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();

            List<object[]> campusCount = requestsData.GroupBy( wd => wd.Campus.Id ).Select( wd => new object[] { wd.FirstOrDefault().Campus.Name, wd.Count() } ).ToList();
            campusCount.Insert( 0, new string[] { "Campus", "Count" } );
            workflowChartData2 = jsSerializer.Serialize( campusCount ).EncodeHtml();

            List<object[]> wfTypeCount = requestsData.GroupBy( wd => wd.Opportunity.Id ).Select( wd => new object[] { wd.FirstOrDefault().Opportunity.Name, wd.Count() } ).ToList();
            wfTypeCount.Insert( 0, new string[] { "Workflow Type", "Count" } );
            workflowChartData3 = jsSerializer.Serialize( wfTypeCount ).EncodeHtml();

            List<object[]> statusCount = requestsData.GroupBy( wd => wd.Status ).Select( wd => new object[] { wd.FirstOrDefault().Status, wd.Count() } ).ToList();
            statusCount.Insert( 0, new string[] { "Status", "Count" } );
            workflowChartData4 = jsSerializer.Serialize( statusCount ).EncodeHtml();

            List<object[]> workerCount = requestsData.GroupBy( wd => wd.Connector ).Select( wd => new object[] { wd.FirstOrDefault().Connector, wd.Count() } ).ToList();
            workerCount.Insert( 0, new string[] { "Worker", "Count" } );
            workflowChartData5 = jsSerializer.Serialize( workerCount ).EncodeHtml();

            List<String> statuses = requestsData.GroupBy( wd => wd.Status ).Select( x => x.FirstOrDefault().Status ).ToList();

            List<object[]> ageList = requestsData.Select( wd => statuses.Select( x => x == wd.Status ? ( object ) wd.AgeInt : null ).ToArray() ).ToList();
            ageList.Insert( 0, statuses.ToArray() );
            workflowChartData1 = jsSerializer.Serialize( ageList ).EncodeHtml();

        }

        private string FormatActivity( object item )
        {
            var connectionRequestActivity = item as ConnectionRequestActivity;
            if ( connectionRequestActivity != null )
            {
                return string.Format( "{0} (<span class='small'>{1}</small>)",
                    connectionRequestActivity.ConnectionActivityType.Name, connectionRequestActivity.CreatedDateTime.ToRelativeDateString() );
            }
            return string.Empty;
        }

        private string FormatStateLabel( ConnectionState connectionState, DateTime? followupDate )
        {
            string css = string.Empty;
            switch ( connectionState )
            {
                case ConnectionState.Active:
                    css = "success";
                    break;
                case ConnectionState.Inactive:
                    css = "danger";
                    break;
                case ConnectionState.FutureFollowUp:
                    css = ( followupDate.HasValue && followupDate.Value >= RockDateTime.Today.AddDays( 1 ) ) ? "info" : "info";
                    break;
                case ConnectionState.Connected:
                    css = "success";
                    break;
            }

            string text = connectionState.ConvertToString();
            if ( connectionState == ConnectionState.FutureFollowUp && followupDate.HasValue )
            {
                text += string.Format( " ({0})", followupDate.Value.ToShortDateString() );
            }

            return string.Format( "<span class='label label-{0}'>{1}</span>", css, text );
        }

        protected void workflowReportTable_RowSelected( object sender, RowEventArgs e )
        {
            Response.Redirect( "~/page/999?ConnectionRequestId=" + ( ( GridView ) sender ).DataKeys[e.RowIndex]["Id"].ToString(), false );
        }
        
        protected void workflowReportTable_GridRebind( object sender, EventArgs e )
        {
            BindGrids();
        }

        protected void workflowFilters_ApplyFilterClick( object sender, EventArgs e )
        {
            BindGrids();
            workflowFilters.Show();
        }
        

        public class GroupedData
        {
            public String GroupedItem { get; set; }
            public int Count { get; set; }
            public String OneMonthOldStats { get; set; }
            public String TwoMonthsOldStats { get; set; }
            public String ThreeMonthsOldStats { get; set; }
            public String OlderThanThreeMonthsStats { get; set; }
            public String TotalStats { get; set; }
        }
    }
}