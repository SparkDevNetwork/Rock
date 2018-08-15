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

namespace RockWeb.Plugins.org_newpointe.WorkflowReport
{
    /// <summary>
    /// Template block for a TreeView.
    /// </summary>
    [DisplayName("Workflow Report")]
    [Category("NewPointe Reporting")]
    [Description("Workflow report/stats.")]

    public partial class WorkflowReport : Rock.Web.UI.RockBlock
    {

        RockContext _rockContext = null;
        WorkflowTypeService workTypeServ = null;
        WorkflowService workServ = null;
        WorkflowActivityService workActServ = null;
        CampusService campServ = null;
        CategoryService catServ = null;

        protected string workflowChartData1;
        protected string workflowChartData2;
        protected string workflowChartData3;
        protected string workflowChartData4;
        protected string workflowChartData5;

        protected String workflowGroupedReportTableItemName = "";

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            _rockContext = new RockContext();
            workServ = new WorkflowService(_rockContext);
            workTypeServ = new WorkflowTypeService(_rockContext);
            workActServ = new WorkflowActivityService(_rockContext);
            campServ = new CampusService(_rockContext);
            catServ = new CategoryService(_rockContext);

            dateRange.UpperValue = DateTime.Now;
            dateRange.LowerValue = DateTime.Now.AddYears( -1 );

            lReadOnlyTitle.Text = "Workflows".FormatAsHtmlTitle();
            workflowFilters.Show();

        }

        protected override void OnLoad(EventArgs e)
        {

            ScriptManager.RegisterStartupScript(Page, this.GetType(), "AKey", "initCharts();", true);



            if (!Page.IsPostBack)
            {
                BindFilters();
            }

        }

        public class adjWorkflowType : WorkflowType
        {
            public override ISecured ParentAuthority {
                get
                {
                    return Category != null ? Category : base.ParentAuthority;
                }
 	        }
        }

        protected bool checkPerms(WorkflowType type)
        {
            adjWorkflowType awt = new adjWorkflowType();
            awt.CopyPropertiesFrom(type);
            awt.Category = catServ.Get(awt.CategoryId ?? -1);
            return awt.IsAuthorized("View", CurrentPerson);
        }

        int workflowTypeFilter;
        int campusFilter;
        int workerFilter;
        string statusFilter;
        IEnumerable<WorkflowType> viewableWorkflowTypes;

        private void BindFilters()
        {
            BindFilters_Workflow();
            BindFilters_Campus();
        }

        private void BindFilters_Workflow()
        {
            // Workflow Type
            workflowTypeFilter = wtpWorkflowType.SelectedValueAsId() ?? -1;

            int? wtfpp = PageParameter("WorkflowTypeId").AsIntegerOrNull(); ;
            if (!IsPostBack && wtfpp.HasValue)
            {
                workflowTypeFilter = wtfpp.Value;
            }

            viewableWorkflowTypes = workTypeServ.Queryable().OrderBy( x => x.Name ).ToList().Where( awt => checkPerms( awt ) );

            WorkflowType wt = workTypeServ.Get( workflowTypeFilter );
            if ( wt != null )
            {
                wtpWorkflowType.SetValue( wt );
            }

            // Statuses
            IQueryable<Workflow> viewableWorkflowData;
            if (workflowTypeFilter < 0)
            {
                viewableWorkflowData = workServ.Queryable();
            }
            else
            {
                viewableWorkflowData = workServ.Queryable().Where(x => x.WorkflowTypeId == workflowTypeFilter);
            }

            statusFilter = workStatus.SelectedValue;
            bindObject(workStatus, viewableWorkflowData.GroupBy(wd => wd.Status).Select(x => x.FirstOrDefault().Status).OrderBy(x => x).ToList(), statusFilter);
            statusFilter = workStatus.SelectedValue;
        }

        private void BindFilters_Campus()
        {
            // Campus
            campusFilter = int.TryParse(campusPicker.SelectedValue, out campusFilter) ? campusFilter : -1;

            var oldVal = campusPicker.SelectedCampusId;
            campusPicker.Campuses = Rock.Web.Cache.CampusCache.All();
            campusPicker.SelectedCampusId = oldVal;
            campusFilter = campusPicker.SelectedCampusId ?? -1;

            // Workers
            workerFilter = ppAssignedPerson.PersonId ?? -1;
        }

        private void BindGrids()
        {
            SortProperty workflowSort = workflowReportTable.SortProperty;

            BindFilters();

            var ids = viewableWorkflowTypes.Select(vwt => vwt.Id).ToList();
            IQueryable<Workflow> viewableWorkflows;
            if (ids.Contains(workflowTypeFilter))
            {
                viewableWorkflows = workServ.Queryable().Where(awd => awd.WorkflowTypeId == workflowTypeFilter);
            }
            else
            {
                viewableWorkflows = workServ.Queryable().Where(awd => ids.Contains(awd.WorkflowTypeId));
            }

            var lastAssignedWorkflowActivities = from ws in viewableWorkflows
                                                 join was in workActServ.Queryable() on ws.Id equals was.WorkflowId into wj
                                                 select wj.Where(o => o.AssignedGroupId != null || o.AssignedPersonAliasId != null).OrderByDescending(o => o.ActivatedDateTime).FirstOrDefault();

            IQueryable<WorkflowActivity> filteredWorkflowActivities = lastAssignedWorkflowActivities.Where(x => x != null);

            if (workerFilter == -1)
            {
                if (campusFilter != -1)
                {
                    filteredWorkflowActivities = (from vwd in filteredWorkflowActivities.ToList()
                                                  where (vwd.AssignedPersonAlias != null && vwd.AssignedPersonAlias.Person.GetFamilies(_rockContext).FirstOrDefault().CampusId == campusFilter) ||
                                                        (vwd.AssignedGroup != null && vwd.AssignedGroup.CampusId == campusFilter)
                                                  select vwd).AsQueryable();
                }
            }
            else
            {
                filteredWorkflowActivities = from vwd in filteredWorkflowActivities
                                             where vwd.AssignedPersonAliasId == workerFilter ||
                                            (vwd.AssignedGroup != null && vwd.AssignedGroup.Members.Select(x => x.Person.Aliases.Select(y => y.Id).Contains(workerFilter)).Contains(true))
                                             select vwd;
            }

            if (statusFilter != "-1")
            {
                filteredWorkflowActivities = filteredWorkflowActivities.Where(x => x.Workflow.Status == statusFilter);
            }

            DateTime? dMin = dateRange.LowerValue;
            DateTime dMax = (dateRange.UpperValue ?? DateTime.Now).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            
            filteredWorkflowActivities = filteredWorkflowActivities.Where(ws => ws.CreatedDateTime > dMin && ws.CreatedDateTime < dMax);


            IQueryable<WorkflowData> filteredWorkflowData = filteredWorkflowActivities.Select(x => new WorkflowData { Activity = x, Workflow = x.Workflow });

            List<WorkflowData> wrtData;
            if (rddlGroupBy.SelectedValue == "")
            {
                if (workflowSort != null)
                {
                    wrtData = filteredWorkflowData.Sort(workflowSort).ToList();
                }
                else
                {
                    wrtData = filteredWorkflowData.ToList().AsQueryable().OrderBy("Completed").ThenBy("WorkflowTypeName").ThenBy("AssignedEntityName").ToList();
                }
            }
            else
            {
                wrtData = filteredWorkflowData.ToList();
            }
            
            IEnumerable<GroupedWorkflowData> gwrtData;

            DateTime oneMonthAgo = DateTime.Now.AddMonths(-1);
            DateTime twoMonthsAgo = DateTime.Now.AddMonths(-2);
            DateTime threeMonthsAgo = DateTime.Now.AddMonths(-3);

            switch (rddlGroupBy.SelectedValue)
            {
                case "1":
                    workflowReportTable.Visible = false;
                    workflowGroupedReportTable.Visible = true;
                    workflowGroupedReportTableItemName = "Workflow";

                    gwrtData = wrtData.GroupBy(x => x.WorkflowTypeId).Select(grp => new GroupedWorkflowData
                    {
                        GroupedItem = grp.FirstOrDefault().WorkflowTypeName,
                        Count = grp.Count(),
                        OneMonthOldStats = "Open: " + grp.Where(x => x.Completed == 0 && (x.CreatedDateTime ?? DateTime.Now) > oneMonthAgo).Count() + "<br/>Closed: " + grp.Where(x => x.Completed == 1 && x.AgeInt < 30).Count(),
                        TwoMonthsOldStats = "Open: " + grp.Where(x => x.Completed == 0 && (x.CreatedDateTime ?? DateTime.Now) < oneMonthAgo && (x.CreatedDateTime ?? DateTime.Now) > twoMonthsAgo).Count() + "<br/>Closed: " + grp.Where(x => x.Completed == 1 && x.AgeInt >= 30 && x.AgeInt < 60).Count(),
                        ThreeMonthsOldStats = "Open: " + grp.Where(x => x.Completed == 0 && (x.CreatedDateTime ?? DateTime.Now) < twoMonthsAgo && (x.CreatedDateTime ?? DateTime.Now) > threeMonthsAgo).Count() + "<br/>Closed: " + grp.Where(x => x.Completed == 1 && x.AgeInt >= 60 && x.AgeInt < 90).Count(),
                        OlderThanThreeMonthsStats = "Open: " + grp.Where(x => x.Completed == 0 && (x.CreatedDateTime ?? DateTime.Now) < threeMonthsAgo).Count() + "<br/>Closed: " + grp.Where(x => x.Completed == 1 && x.AgeInt >= 90).Count(),
                        TotalStats = "Open: " + grp.Where(x => x.Completed == 0).Count() + "<br/>Closed: " + grp.Where(x => x.Completed == 1).Count()
                    }).OrderBy(x => x.GroupedItem);

                    workflowGroupedReportTable.DataSource = gwrtData.ToList();
                    workflowGroupedReportTable.DataBind();
                    break;
                case "2":
                    workflowReportTable.Visible = false;
                    workflowGroupedReportTable.Visible = true;
                    workflowGroupedReportTableItemName = "Campus";

                    gwrtData = wrtData.GroupBy(x => x.CampusId).Select(grp => new GroupedWorkflowData
                    {
                        GroupedItem = grp.FirstOrDefault().CampusName,
                        Count = grp.Count(),
                        OneMonthOldStats = "Open: " + grp.Where(x => x.Completed == 0 && (x.CreatedDateTime ?? DateTime.Now) > oneMonthAgo).Count() + "<br/>Closed: " + grp.Where(x => x.Completed == 1 && x.AgeInt < 30).Count(),
                        TwoMonthsOldStats = "Open: " + grp.Where(x => x.Completed == 0 && (x.CreatedDateTime ?? DateTime.Now) < oneMonthAgo && (x.CreatedDateTime ?? DateTime.Now) > twoMonthsAgo).Count() + "<br/>Closed: " + grp.Where(x => x.Completed == 1 && x.AgeInt >= 30 && x.AgeInt < 60).Count(),
                        ThreeMonthsOldStats = "Open: " + grp.Where(x => x.Completed == 0 && (x.CreatedDateTime ?? DateTime.Now) < twoMonthsAgo && (x.CreatedDateTime ?? DateTime.Now) > threeMonthsAgo).Count() + "<br/>Closed: " + grp.Where(x => x.Completed == 1 && x.AgeInt >= 60 && x.AgeInt < 90).Count(),
                        OlderThanThreeMonthsStats = "Open: " + grp.Where(x => x.Completed == 0 && (x.CreatedDateTime ?? DateTime.Now) < threeMonthsAgo).Count() + "<br/>Closed: " + grp.Where(x => x.Completed == 1 && x.AgeInt >= 90).Count(),
                        TotalStats = "Open: " + grp.Where(x => x.Completed == 0).Count() + "<br/>Closed: " + grp.Where(x => x.Completed == 1).Count()
                    }).OrderBy(x => x.GroupedItem);

                    workflowGroupedReportTable.DataSource = gwrtData.ToList();
                    workflowGroupedReportTable.DataBind();
                    break;
                case "3":
                    workflowReportTable.Visible = false;
                    workflowGroupedReportTable.Visible = true;
                    workflowGroupedReportTableItemName = "Assigned Worker";

                    gwrtData = wrtData.GroupBy(x => x.AssignedEntityName).Select(grp => new GroupedWorkflowData
                    {
                        GroupedItem = grp.FirstOrDefault().AssignedEntityName,
                        Count = grp.Count(),
                        OneMonthOldStats = "Open: " + grp.Where(x => x.Completed == 0 && (x.CreatedDateTime ?? DateTime.Now) > oneMonthAgo).Count() + "<br/>Closed: " + grp.Where(x => x.Completed == 1 && x.AgeInt < 30).Count(),
                        TwoMonthsOldStats = "Open: " + grp.Where(x => x.Completed == 0 && (x.CreatedDateTime ?? DateTime.Now) < oneMonthAgo && (x.CreatedDateTime ?? DateTime.Now) > twoMonthsAgo).Count() + "<br/>Closed: " + grp.Where(x => x.Completed == 1 && x.AgeInt >= 30 && x.AgeInt < 60).Count(),
                        ThreeMonthsOldStats = "Open: " + grp.Where(x => x.Completed == 0 && (x.CreatedDateTime ?? DateTime.Now) < twoMonthsAgo && (x.CreatedDateTime ?? DateTime.Now) > threeMonthsAgo).Count() + "<br/>Closed: " + grp.Where(x => x.Completed == 1 && x.AgeInt >= 60 && x.AgeInt < 90).Count(),
                        OlderThanThreeMonthsStats = "Open: " + grp.Where(x => x.Completed == 0 && (x.CreatedDateTime ?? DateTime.Now) < threeMonthsAgo).Count() + "<br/>Closed: " + grp.Where(x => x.Completed == 1 && x.AgeInt >= 90).Count(),
                        TotalStats = "Open: " + grp.Where(x => x.Completed == 0).Count() + "<br/>Closed: " + grp.Where(x => x.Completed == 1).Count()
                    }).OrderBy(x => x.GroupedItem);

                    workflowGroupedReportTable.DataSource = gwrtData.ToList();
                    workflowGroupedReportTable.DataBind();
                    break;
                case "4":
                    workflowReportTable.Visible = false;
                    workflowGroupedReportTable.Visible = true;
                    workflowGroupedReportTableItemName = "Status";

                    gwrtData = wrtData.GroupBy(x => x.Status).Select(grp => new GroupedWorkflowData
                    {
                        GroupedItem = grp.FirstOrDefault().Status,
                        Count = grp.Count(),
                        OneMonthOldStats = "Open: " + grp.Where(x => x.Completed == 0 && (x.CreatedDateTime ?? DateTime.Now) > oneMonthAgo).Count() + "<br/>Closed: " + grp.Where(x => x.Completed == 1 && x.AgeInt < 30).Count(),
                        TwoMonthsOldStats = "Open: " + grp.Where(x => x.Completed == 0 && (x.CreatedDateTime ?? DateTime.Now) < oneMonthAgo && (x.CreatedDateTime ?? DateTime.Now) > twoMonthsAgo).Count() + "<br/>Closed: " + grp.Where(x => x.Completed == 1 && x.AgeInt >= 30 && x.AgeInt < 60).Count(),
                        ThreeMonthsOldStats = "Open: " + grp.Where(x => x.Completed == 0 && (x.CreatedDateTime ?? DateTime.Now) < twoMonthsAgo && (x.CreatedDateTime ?? DateTime.Now) > threeMonthsAgo).Count() + "<br/>Closed: " + grp.Where(x => x.Completed == 1 && x.AgeInt >= 60 && x.AgeInt < 90).Count(),
                        OlderThanThreeMonthsStats = "Open: " + grp.Where(x => x.Completed == 0 && (x.CreatedDateTime ?? DateTime.Now) < threeMonthsAgo).Count() + "<br/>Closed: " + grp.Where(x => x.Completed == 1 && x.AgeInt >= 90).Count(),
                        TotalStats = "Open: " + grp.Where(x => x.Completed == 0).Count() + "<br/>Closed: " + grp.Where(x => x.Completed == 1).Count()
                    }).OrderBy(x => x.GroupedItem);

                    workflowGroupedReportTable.DataSource = gwrtData.ToList();
                    workflowGroupedReportTable.DataBind();
                    break;
                default:
                    workflowReportTable.Visible = true;
                    workflowGroupedReportTable.Visible = false;

                    workflowReportTable.DataSource = wrtData;
                    workflowReportTable.DataKeyNames = new string[] { "Id" };
                    workflowReportTable.DataBind();
                    break;
            }

            doStats(wrtData);
        }

        protected void doStuff()
        {
            BindGrids();

        }
        protected void bindObject<T>(ListControl control, List<T> entityList, string selectedValue)
        {
            control.DataSource = entityList;
            //control.DataTextField = "Name";
            //control.DataValueField = "Id";
            control.DataBind();
            control.Items.Insert(0, new ListItem("", "-1"));

            if (control.Items.FindByValue(selectedValue) != null)
            {
                control.SelectedValue = selectedValue;
            }
            else
            {
                control.SelectedValue = "-1";
            }

        }

        protected void bindNameAndId<T>(ListControl control, List<T> entityList, string selectedValue)
        {
            control.DataSource = entityList;
            control.DataTextField = "Name";
            control.DataValueField = "Id";
            control.DataBind();
            control.Items.Insert(0, new ListItem("", "-1"));

            if (control.Items.FindByValue(selectedValue) != null)
            {
                control.SelectedValue = selectedValue;
            }
            else
            {
                control.SelectedValue = "-1";
            }

        }

        protected void workflowReportTable_RowSelected(object sender, RowEventArgs e)
        {
            Response.Redirect("~/Workflow/" + ((GridView)sender).DataKeys[e.RowIndex]["Id"].ToString(), false);
        }


        protected int countWorkflows(IQueryable<WorkflowData> workflowData, int completed, int dayDiffStart, int dayDiffEnd)
        {
            return workflowData.Where(
                    x => x.Completed == completed &&
                    (dayDiffStart == 0 || x.CreatedDateTime > DateTime.Now.AddDays(dayDiffStart)) &&
                    (dayDiffEnd == 0 || x.CreatedDateTime < DateTime.Now.AddDays(dayDiffEnd))
                ).Count();
        }
        protected void workflowReportTable_GridRebind(object sender, EventArgs e)
        {
            doStuff();
        }


        protected void doStats(List<WorkflowData> workflowData)
        {
            //IQueryable<int> tmp = workflowData.AsQueryable().Where(x => x.Completed == 1).Select(x => ((x.CompletedDateTime - x.CreatedDateTime) ?? TimeSpan.Zero).Days);
            //int _0to90_AverageTime = tmp.Count() == 0 ? -1 : (int)tmp.Average();
            //rlWorkflowStats.Text = "Average time to completion is " + _0to90_AverageTime + " days.";

            System.Web.Script.Serialization.JavaScriptSerializer jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();

            List<object[]> campusCount = workflowData.GroupBy(wd => wd.CampusId).Select(wd => new object[] { wd.FirstOrDefault().CampusName, wd.Count() }).ToList();
            campusCount.Insert(0, new string[] { "Campus", "Count" });
            workflowChartData2 = jsSerializer.Serialize(campusCount).EncodeHtml();

            List<object[]> wfTypeCount = workflowData.GroupBy(wd => wd.WorkflowTypeId).Select(wd => new object[] { wd.FirstOrDefault().WorkflowTypeName, wd.Count() }).ToList();
            wfTypeCount.Insert(0, new string[] { "Workflow Type", "Count" });
            workflowChartData3 = jsSerializer.Serialize(wfTypeCount).EncodeHtml();

            List<object[]> statusCount = workflowData.GroupBy(wd => wd.Status).Select(wd => new object[] { wd.FirstOrDefault().Status, wd.Count() }).ToList();
            statusCount.Insert(0, new string[] { "Status", "Count" });
            workflowChartData4 = jsSerializer.Serialize(statusCount).EncodeHtml();

            List<object[]> workerCount = workflowData.GroupBy(wd => wd.AssignedEntityName).Select(wd => new object[] { wd.FirstOrDefault().AssignedEntityName, wd.Count() }).ToList();
            workerCount.Insert(0, new string[] { "Worker", "Count" });
            workflowChartData5 = jsSerializer.Serialize(workerCount).EncodeHtml();

            List<String> statuses = workflowData.GroupBy(wd => wd.Status).Select(x => x.FirstOrDefault().Status).ToList();

            List<object[]> ageList = workflowData.Select(x => makeWFHistRow(statuses, x)).ToList();
            ageList.Insert(0, statuses.ToArray());
            workflowChartData1 = jsSerializer.Serialize(ageList).EncodeHtml();
        }

        protected object[] makeWFHistRow(List<string> statuses, WorkflowData wd)
        {
            return statuses.Select(x => x == wd.Status ? (object)wd.AgeInt : null).ToArray();
        }

        protected void workflowFilters_ApplyFilterClick(object sender, EventArgs e)
        {
            doStuff();
            workflowFilters.Show();
        }


        public class WorkflowData
        {
            public int Id { get { return Workflow.Id; } }
            public int WorkflowTypeId { get { return Workflow.WorkflowTypeId; } }
            public String WorkflowTypeName { get { return Workflow.WorkflowType.Name; } }
            public String Name { get { return Workflow.Name; } }
            public String Status { get { return Workflow.CompletedDateTime != null ? "Completed" : Workflow.Status; } }
            public DateTime? CreatedDateTime { get { return Workflow.CreatedDateTime; } }
            public DateTime? CompletedDateTime { get { return Workflow.CompletedDateTime; } }
            public int Completed { get { return (Workflow.Status == "Completed" || Workflow.CompletedDateTime != null).Bit(); } }
            public int AgeInt { get { return ((Workflow.CompletedDateTime ?? DateTime.Now) - (Workflow.CreatedDateTime ?? DateTime.Now)).Days; } }
            public int CampusId { get { return (Activity.AssignedPersonAlias != null ? Activity.AssignedPersonAlias.Person.GetFamilies().FirstOrDefault().CampusId : (Activity.AssignedGroup != null ? Activity.AssignedGroup.CampusId : -1)) ?? -1; } }
            public String CampusName { get { return (Activity.AssignedPersonAlias != null ? Activity.AssignedPersonAlias.Person.GetFamilies().FirstOrDefault().Campus.Name : (Activity.AssignedGroup != null ? (Activity.AssignedGroup.Campus != null ? Activity.AssignedGroup.Campus.Name : "") : "")) ?? ""; } }

            public Workflow Workflow { get; set; }
            public WorkflowActivity Activity { get; set; }
            public String AssignedEntityName
            {
                get
                {
                    return Activity.AssignedPersonAlias != null ? Activity.AssignedPersonAlias.Person.NickName + " " + Activity.AssignedPersonAlias.Person.LastName : (
                        Activity.AssignedGroup != null ? Activity.AssignedGroup.Name : "Nobody"
                    );
                }
            }
        }

        public class GroupedWorkflowData
        {
            public String GroupedItem { get; set; }
            public int Count { get; set; }
            public String OneMonthOldStats { get; set; }
            public String TwoMonthsOldStats { get; set; }
            public String ThreeMonthsOldStats { get; set; }
            public String OlderThanThreeMonthsStats { get; set; }
            public String TotalStats { get; set; }
        }

        public class ShallowPersonData
        {
            public int? Id { get; set; }
            public String Name { get { return FirstName + " " + LastName + (Note != null ? " [" + Note + "]" : ""); } }
            public String FirstName { get; set; }
            public String LastName { get; set; }
            public String Note { get; set; }
        }

        public class ShallowPersonDataComparer : IEqualityComparer<ShallowPersonData>
        {
            public bool Equals(ShallowPersonData a, ShallowPersonData b)
            {
                return a.Id == b.Id;
            }

            public int GetHashCode(ShallowPersonData a)
            {
                return a.Id ?? -1;
            }
        }

        protected void wftListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindFilters_Workflow();
        }

        protected void campusPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindFilters_Campus();
        }

        protected void wtpWorkflowType_SelectItem( object sender, EventArgs e )
        {
            BindFilters_Workflow();
        }
    }
}