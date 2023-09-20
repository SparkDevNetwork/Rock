using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RockWeb.Plugins.org_lakepointe.Forms
{
    [DisplayName( "Employee Coaching HR Portal" )]
    [Category( "LPC > Forms" )]
    [Description( "Grid with filters and sorts to manage Employee Coaching forms." )]

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Description = "Page used to view details of an requests.",
        IsRequired = true,
        Order = 1,
        Key = AttributeKey.DetailPage )]

    #endregion

    public partial class EmployeeCoachingHRPortal : RockBlock
    {
        #region Attribute Keys
        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }
        #endregion Attribute Keys
        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string CoachingReportId = "report";
        }

        #endregion

        #region Fields
        #endregion

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rFilter.ApplyFilterClick += RFilter_ApplyFilterClick;

            Dictionary<string, BoundField> boundFields = gReports.Columns.OfType<BoundField>().ToDictionary( a => a.DataField );
            boundFields["EmployeeName"].Visible = true;

            gReports.DataKeyNames = new string[] { "ReportId" };
            gReports.EntityTypeId = EntityTypeCache.Get<WorkflowActionFormAttributeCache>().Id;
            gReports.GridRebind += GReports_GridRebind;

            BlockUpdated += EmployeeCoachingHRPortal_BlockUpdated;
            AddConfigurationUpdateTrigger( upnlContent );
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
                InitializeForm();
            }
        }

        protected override object SaveViewState()
        {
            return base.SaveViewState();
        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
        }

        #endregion

        #region Events
        #endregion

        #region Methods

        private void InitializeForm()
        {
            SetFilter();
            BindGrid();
        }

        private void BindGrid()
        {
            using ( var context = new RockContext() )
            {
                // Get queryable of all workflows of this type that the user is authorized to view
                var reports = new WorkflowService( context ).Queryable().AsNoTracking()
                    .Where( r => r.WorkflowTypeId == 382 ).AsEnumerable()
                    .Where( r => r.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) )
                    .ToList();
                reports.ForEach( r => r.LoadAttributes() ); // would be nice if we could filter before loading attributes, but we're filtering on attributes so ...

                var personAliasService = new PersonAliasService( context );

                if ( ppSupervisor.SelectedValue.HasValue )
                {
                    reports = reports.Where( r => personAliasService.Get( r.AttributeValues["Supervisor"].Value.AsGuid() ).Person.Id == ppSupervisor.SelectedValue.Value ).ToList();
                }

                if ( ppEmployee.SelectedValue.HasValue )
                {
                    reports = reports.Where( r => personAliasService.Get( r.AttributeValues["Employee"].Value.AsGuid() ).Person.Id == ppEmployee.SelectedValue.Value ).ToList();
                }

                if ( nreMonth.LowerValue.HasValue || nreMonth.UpperValue.HasValue )
                {
                    var lowerValue = nreMonth.LowerValue ?? 1;
                    var upperValue = nreMonth.UpperValue ?? 12;
                    reports = reports.Where( r => { var m = r.AttributeValues["Month"].Value.AsInteger(); return m >= lowerValue && m <= upperValue; } ).ToList();
                }

                if ( drpSupervisor.LowerValue.HasValue || drpSupervisor.UpperValue.HasValue )
                {
                    var dateRange = drpSupervisor.DateRange;
                    reports = reports.Where( r => { var d = r.AttributeValues["SupervisorSubmitDate"].Value.AsDateTime(); return d.HasValue && d.Value >= dateRange.Start && d.Value < dateRange.End; } ).ToList();
                }

                if ( drpEmployee.LowerValue.HasValue || drpEmployee.UpperValue.HasValue )
                {
                    var dateRange = drpEmployee.DateRange;
                    reports = reports.Where( r => { var d = r.AttributeValues["ReportDate"].Value.AsDateTime(); return d.HasValue && d.Value >= dateRange.Start && d.Value < dateRange.End; } ).ToList();
                }

                if ( !ddlStatusFilter.SelectedValue.Equals( "all" ) )
                {
                    reports = reports.Where( r => r.AttributeValues["CurrentOwner"].Value.Equals( ddlStatusFilter.SelectedValue ) ).ToList();
                }

                SortProperty sortProperty = gReports.SortProperty;
                if ( sortProperty == null )
                {
                    sortProperty = new SortProperty( new GridViewSortEventArgs( "SupervisorSubmitDate", SortDirection.Descending ) );
                }

                if ( reports.Any() )
                {
                    var reportDetails = reports.Select( r => new ReportListRowInfo
                    {
                        Month = r.AttributeValues["Month"].Value.AsInteger(),
                        SupervisorSubmitDate = r.AttributeValues["SupervisorSubmitDate"].Value.AsDateTime(),
                        EmployeeSubmitDate = r.AttributeValues["ReportDate"].Value.AsDateTime(),
                        EmployeeName = personAliasService.Get( r.AttributeValues["Employee"].Value.AsGuid() ).Person.FullName,
                        SupervisorName = personAliasService.Get( r.AttributeValues["Supervisor"].Value.AsGuid() ).Person.FullName,
                        PositionTitle = r.AttributeValues["PositionTitle"].Value,
                        Status = r.AttributeValues["CurrentOwner"].Value,
                        Jesus = GetIndexValue( r, "LovesfollowsJesus" ),
                        JesusSupervisor = GetIndexValue( r, "LovesAndFollowsJesusSupervisor" ),
                        Honor = GetIndexValue( r, "HonorsUpDownAllAround" ),
                        HonorSupervisor = GetIndexValue( r, "HonorsUpDownAndAllAroundSupervisor" ),
                        Fun = GetIndexValue( r, "MakesItFun" ),
                        FunSupervisor = GetIndexValue( r, "MakesItFunSupervisor" ),
                        Great = GetIndexValue( r, "RejectsGoodforGreat" ),
                        GreatSupervisor = GetIndexValue( r, "RejectsGoodForGreatSupervisor" ),
                        Whatever = GetIndexValue( r, "WhateverItTakes" ),
                        WhateverSupervisor = GetIndexValue( r, "WhateverItTakesSupervisor" ),
                        Lakepointe = GetIndexValue( r, "LovesLakepointe" ),
                        LakepointeSupervisor = GetIndexValue( r, "LovesLakepointeSupervisor" ),
                        Joy = r.AttributeValues["Joy"].Value.AsInteger(),
                        Wins = r.AttributeValues["Wins"].Value,
                        Help = r.AttributeValues["Help"].Value,
                        Learning = r.AttributeValues["Learning"].Value,
                        Bugs = r.AttributeValues["Bugs"].Value,
                        Six = r.AttributeValues["6x6"].Value,
                        Comments = r.AttributeValues["Comments"].Value,
                        SuperComments = r.AttributeValues["SupervisorConfidentialCommentstoHR"].Value,
                        HRComments = r.AttributeValues["HRConfidentialComments"].Value,
                        EmployeeId = personAliasService.Get( r.AttributeValues["Employee"].Value.AsGuid() ).Person.Id,
                        SupervisorId = personAliasService.Get( r.AttributeValues["Supervisor"].Value.AsGuid() ).Person.Id,
                        ReportId = r.Id
                    } ).AsQueryable().Sort( sortProperty ).ToList();

                    if ( reportDetails.Any() )
                    {
                        nbNoOpportunities.Visible = false;
                        gReports.Visible = true;
                        gReports.DataSource = reportDetails;
                        gReports.DataBind();
                    }
                    else
                    {
                        gReports.Visible = false;
                        nbNoOpportunities.Visible = true;
                    }
                }
                else
                {
                    gReports.Visible = false;
                    nbNoOpportunities.Visible = true;
                }
            }
        }

        private static int GetIndexValue( Workflow workflow, string key, int d = 5 )
        {
            return workflow.AttributeValues.ContainsKey( key ) ? workflow.AttributeValues[key].Value.AsInteger() : d;
        }

        private void SetFilter()
        {
            using ( var context = new RockContext() )
            {
                var personService = new PersonService( context );
                var supervisorId = rFilter.GetUserPreference( "Supervisor" ).AsIntegerOrNull();
                if ( supervisorId.HasValue )
                {
                    ppSupervisor.SetValue( personService.Get( supervisorId.Value ) );
                }

                var employeeId = rFilter.GetUserPreference( "Employee" ).AsIntegerOrNull();
                if ( employeeId.HasValue )
                {
                    ppEmployee.SetValue( personService.Get( employeeId.Value ) );
                }

                nreMonth.DelimitedValues = rFilter.GetUserPreference( "Month" );
                drpSupervisor.DelimitedValues = rFilter.GetUserPreference( "SupervisorDateRange" );
                drpEmployee.DelimitedValues = rFilter.GetUserPreference( "EmployeeDateRange" );
                ddlStatusFilter.SelectedValue = rFilter.GetUserPreference( "Status" );
            }
        }

        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Supervisor":
                case "Employee":
                    var personName = string.Empty;
                    var personId = e.Value.AsIntegerOrNull();
                    if ( personId.HasValue )
                    {
                        using ( var context = new RockContext() )
                        {
                            var person = new PersonService( context ).Get( personId.Value );
                            if ( person != null )
                            {
                                personName = person.FullName;
                            }
                        }
                    }
                    e.Value = personName;
                    break;

                case "Month":
                    e.Value = NumberRangeEditor.FormatDelimitedValues( e.Value );
                    break;

                case "SupervisorDateRange":
                case "EmployeeDateRange":
                    e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                    break;

                case "Status":
                    break; // e.Value = e.Value;

                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        protected void gReport_RowDataBound( object sender, GridViewRowEventArgs e )
        {
        }

        protected void gReport_View( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.CoachingReportId, e.RowKeyId );
        }

        private void EmployeeCoachingHRPortal_BlockUpdated( object sender, EventArgs e )
        {
        }

        private void GReports_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        private void RFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            var supervisorId = ppSupervisor.PersonId;
            rFilter.SaveUserPreference( "Supervisor", "Supervisor", supervisorId.HasValue ? supervisorId.Value.ToString() : string.Empty );

            var employeeId = ppEmployee.PersonId;
            rFilter.SaveUserPreference( "Employee", "Employee", employeeId.HasValue ? employeeId.Value.ToString() : string.Empty );

            rFilter.SaveUserPreference( "Month", "Month", nreMonth.DelimitedValues );
            rFilter.SaveUserPreference( "SupervisorDateRange", "Supervisor Date Range", drpSupervisor.DelimitedValues );
            rFilter.SaveUserPreference( "EmployeeDateRange", "Employee Date Range", drpEmployee.DelimitedValues );
            rFilter.SaveUserPreference( "Status", "Status", ddlStatusFilter.SelectedValue );

            BindGrid();
        }

        protected void DeleteReportClicked( object sender, RowEventArgs e )
        {
            using ( var context = new RockContext() )
            {
                var workflowService = new WorkflowService( context );
                var workflow = workflowService.Get( e.RowKeyId );
                workflowService.Delete( workflow );
                context.SaveChanges();
                BindGrid();
            }
        }

        #endregion

        private class ReportListRowInfo : RockDynamic
        {
            public int Month { get; set; }
            public DateTime? SupervisorSubmitDate { get; set; }
            public DateTime? EmployeeSubmitDate { get; set; }
            public string EmployeeName { get; set; }
            public string SupervisorName { get; set; }
            public string PositionTitle { get; set; }
            public string Status { get; set; }
            public int Jesus { get; set; }
            public int JesusSupervisor { get; set; }
            public int Honor { get; set; }
            public int HonorSupervisor { get; set; }
            public int Fun { get; set; }
            public int FunSupervisor { get; set; }
            public int Great { get; set; }
            public int GreatSupervisor { get; set; }
            public int Whatever { get; set; }
            public int WhateverSupervisor { get; set; }
            public int Lakepointe { get; set; }
            public int LakepointeSupervisor { get; set; }
            public int Joy { get; set; }
            public string Wins { get; set; }
            public string Help { get; set; }
            public string Learning { get; set; }
            public string Bugs { get; set; }
            public string Six { get; set; }
            public string Comments { get; set; }
            public string SuperComments { get; set; }
            public string HRComments { get; set; }
            public int EmployeeId { get; set; }
            public int SupervisorId { get; set; }
            public int ReportId { get; set; }
        }
    }
}