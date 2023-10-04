using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web;
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


namespace RockWeb.Plugins.org_lakepointe.Groups
{
    [Category("LPC > Groups")]
    [DisplayName("Group Headcount Count Entry")]
    [Description("Allows a group leader to enter attendance head count. ")]
    [TextField("Group List Title", "The panel to display on the group list page.", false, "Groups", "", 0, "GroupListTitle")]
    [GroupField("Parent Group", "If a group is chosen only groups under this group will be displayed.", false, "", "", 1, "ParentGroup")]
    [GroupTypesField("Included Group Types", "The group types to include in the list. If none are selected, all non excluded group types are included.", false, "", "", 2, "IncludedGroupTypes")]
    [GroupTypesField("Excluded Group types", "The group types to excludes in the list. Only valid if including all group types.", false, "", "", 3, "ExcludedGroupTypes")]
    [CodeEditorField("Group List Instructions", "The lava template for the message to show on the Group List screen.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, true, "","", 4, "GroupListInstructions")]
    [LavaCommandsField("Enabled Lava Commands", "The Lava commands that should be enabled for this HTML block.", false, "","", 5, "EnabledLavaCommands")]
    [IntegerField("Weeks Back", "The number of previous weeks that a group leader can record attendance for.", false, 1, "", 6)]
    [UrlLinkField("Redirect URL","The page to redirect the user to after they have entered their headcounts. Use {GroupId} to include the GroupID in the url.", false, "", "Redirect", 0)]
    [IntegerField("Redirect Delay", "The time in seconds to delay redirecting the user.  Default is 5 seconds.", false, 5, "Redirect", 1)]

    public partial class GroupAttendanceCountEntry : RockBlock
    {
        RockContext _rockContext = null;

        int? _weeksBack = null;
        int? _groupViewerPageId = null;

        private int WeeksBack
        {
            get
            {
                if (!_weeksBack.HasValue)
                {
                    _weeksBack = GetAttributeValue("WeeksBack").AsInteger();

                    if (_weeksBack == 0)
                    {
                        _weeksBack = 1;
                    }
                }
                return _weeksBack.Value;
            }
        }

        private int GroupViewerPageId
        {
            get
            {
                if (!_groupViewerPageId.HasValue)
                {
                    _groupViewerPageId = PageParameter("gvp").AsInteger();
                }

                return _groupViewerPageId.Value;
            }
        }

        #region Base Control Methods
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            BlockUpdated += GroupAttendanceCountEntry_BlockUpdated;
            _rockContext = new RockContext();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UpdateNotification(null, null, NotificationBoxType.Default);
            if (!IsPostBack)
            {
                lbBackToList.CommandArgument = GroupViewerPageId.ToString();
                if (GroupViewerPageId > 0)
                {
                    lbBackToList.Text = "Back to Group";
                    lbBackToList.CommandName = "Redirect";
                    
                }
                else
                {
                    lbBackToList.Text = "Back to Group List";
                    lbBackToList.CommandName = "List";
                }
                LoadBlock();
            }


        }
        #endregion

        #region Events
        private void GroupAttendanceCountEntry_BlockUpdated(object sender, EventArgs e)
        {
            LoadBlock();
        }


        protected void gGroupList_RowSelected(object sender, RowEventArgs e)
        {
            var groupId = e.RowKeyId;

            if (groupId > 0)
            {
                pnlGroupList.Visible = false;
                LoadGroup(groupId);
            }

        }

        protected void gGroupList_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if(e.Row.RowType == DataControlRowType.DataRow )
            {
                var item = e.Row.DataItem as GroupAttendanceRowItem;
                var lThisWeek = e.Row.FindControl("lThisWeek") as Literal;
                var lPreviousWeek = e.Row.FindControl("lPreviousWeek") as Literal;

                var thisSunday = RockDateTime.Today.SundayDate();
                var occurrence = item.Occurrences.FirstOrDefault(o => o.SundayDate == thisSunday);

                if (occurrence == null)
                {
                    lThisWeek.Text = "<span class=\"label label-danger\">Not Submitted</span>";

                }
                else if (occurrence.DidNotOccur == true)
                {
                    lThisWeek.Text = "<span class=\"label label-warning\">Did Not Meet</span>";
                }
                else if (!occurrence.AnonymousAttendanceCount.HasValue)
                {
                    lThisWeek.Text = "<span class=\"label label-danger\">Not Submitted</span>";
                }
                else
                {
                    lThisWeek.Text = occurrence.AnonymousAttendanceCount.ToString();
                }

                var lastSunday = thisSunday.AddDays(-7);
                var lastSundayOccurrence = item.Occurrences.FirstOrDefault(o => o.SundayDate == lastSunday);
                if (lastSundayOccurrence == null)
                {
                    lPreviousWeek.Text = "<span class=\"label label-danger\">Not Submitted</span>";

                }
                else if (lastSundayOccurrence.DidNotOccur == true)
                {
                    lPreviousWeek.Text = "<span class=\"label label-warning\">Did Not Meet</span>";
                }
                else if (!lastSundayOccurrence.AnonymousAttendanceCount.HasValue)
                {
                    lPreviousWeek.Text = "<span class=\"label label-danger\">Not Submitted</span>";
                }
                else
                {
                    lPreviousWeek.Text = lastSundayOccurrence.AnonymousAttendanceCount.ToString();
                }

            }
            
        }

        protected void ddlClassDate_SelectedIndexChanged(object sender, EventArgs e)
        {
            var occurrenceValue = ddlClassDate.SelectedValue.Split("^".ToCharArray());
            var occurrenceId = occurrenceValue[1].AsIntegerOrNull();
            var year = occurrenceValue[0].Substring(0, 4).AsInteger();
            var month = occurrenceValue[0].Substring(4, 2).AsInteger();
            var day = occurrenceValue[0].Substring(6, 2).AsInteger();

            DateTime? occurrenceDate = null;
            if (year > 0 && month > 0 && day > 0)
            {
                occurrenceDate = new DateTime(year, month, day);
            }

            AttendanceOccurrence occurrence = null;
            if (occurrenceId.HasValue && occurrenceId > 0)
            {
                occurrence = new AttendanceOccurrenceService(_rockContext).Get(occurrenceId.Value);
            }
            else
            {
                var group = new GroupService(_rockContext).Get(hfGroupId.ValueAsInt());
                occurrence = new AttendanceOccurrence()
                {
                    OccurrenceDate = occurrenceDate.Value,
                    GroupId = group.Id,
                    Group = group,
                    ScheduleId = group.ScheduleId,
                    Schedule = group.Schedule
                };

            }

            UpdateOccurrenceFields(occurrence);

        }

        protected void lbBackToList_Click(object sender, EventArgs e)
        {
            var commandName = lbBackToList.CommandName;
            var commandArgument = lbBackToList.CommandArgument.AsInteger();

            if (commandName.Equals("list", StringComparison.InvariantCultureIgnoreCase))
            {
                hfGroupId.Value = String.Empty;
                LoadGroupList(false);
                return;
            }

            if (commandName.Equals("redirect", StringComparison.InvariantCultureIgnoreCase))
            {
                var globalAttributeCache = GlobalAttributesCache.Get();
                string appRoot = String.Empty;

                if (IsReferredFromInternalSite())
                {
                    appRoot = globalAttributeCache.GetValue("InternalApplicationRoot").EnsureTrailingForwardslash();
                }
                else
                {
                    appRoot = globalAttributeCache.GetValue("PublicApplicationRoot").EnsureTrailingForwardslash();
                }
                
                var url = string.Format("{0}page/{1}?GroupId={2}", appRoot, commandArgument, hfGroupId.Value);

                Response.Redirect(url, false);

                return;
            }
        }

        protected void lbSave_Click(object sender, EventArgs e)
        {
            SaveOccurrence();
        }

        protected void lReset_Click(object sender, EventArgs e)
        {
            LoadGroup(hfGroupId.ValueAsInt());
        }
        #endregion

        #region Methods

        private bool IsReferredFromInternalSite()
        {

            if (GroupViewerPageId <= 0)
            {
                return false;
            }

            var groupViewerPage = PageCache.Get(GroupViewerPageId);
            var internalSiteId = SiteCache.Get(Rock.SystemGuid.Site.SITE_ROCK_INTERNAL).Id;

            if(internalSiteId == groupViewerPage.SiteId)
            {
                return true;
            }

            return false;
        }

        private void LoadBlock()
        {
            pnlGroupList.Visible = false;
            pnlGroupAttendance.Visible = false;

            lGroupListHeader.Text = GetAttributeValue("GroupListTitle").Trim();
            var groupId = PageParameter("GroupId").AsIntegerOrNull();

            if (groupId.HasValue)
            {
                LoadGroup(groupId.Value);
            }
            else
            {
                LoadGroupList(true);
            }
        }

        private void LoadGroup(int groupId, int? selectedOccurrenceId = null)
        {
            lInstructions.Text = string.Empty;
            pnlGroupAttendance.Visible = false;
            pnlGroupList.Visible = false;

            var group = new GroupService(_rockContext).Get(groupId);

            if (group == null || group.Id <= 0)
            {
                UpdateNotification("Unable to View Group.", "<p>The selected group is not currently available.</p>", NotificationBoxType.Warning);
                return;
            }

            if (!group.IsAuthorized(Authorization.VIEW, CurrentPerson))
            {
                UpdateNotification("Unable to View Group Attendance", "<p>You do not have access to update attendance for this group. If you recieved this in error, please contact your ministry leader.</p>", NotificationBoxType.Danger);
                return;
            }

            lGroupEntryTitle.Text = string.Format("{0} Attendance Entry", group.Name);
            hfGroupId.Value = group.Id.ToString();

            // gets the number of days back to pull occurrences for
            // If 2 weeks back was choosen we would go back 2 weeks + 1 and then remove a day to get the Monday date
            // which is the beginning of the first week.
            var daysBack = ((WeeksBack + 1) * 7) - 1;  

            var thisSunday = RockDateTime.GetSundayDate(RockDateTime.Now.Date);
            var toDateTime = thisSunday.AddDays(1);      //Monday at 0:00:00
            var fromDate = thisSunday.AddDays(-daysBack);
            

            var occurrences = new AttendanceOccurrenceService(_rockContext)
                .GetGroupOccurrences(group, fromDate, toDateTime, new List<int>(), new List<int>())
                .Where(o => o.OccurrenceDate <= RockDateTime.Now && o.OccurrenceDate >= fromDate)
                .OrderByDescending(o => o.OccurrenceDate)
                .ToList();

            if (occurrences.Count == 0)
            {
                string url = ResolveRockUrl(string.Format("~/page/{0}", CurrentPageReference.PageId));
                string title = "No Scheduled Meetings Found";
                string alertBody = string.Format("No scheduled meetings were found for {0}. <br /> <a href=\"{1}\" class=\"btn btn-xs btn-link\">Return to Group List</a>.", group.Name, url);
                UpdateNotification(title, alertBody, NotificationBoxType.Warning);
                return;
            }
            
            ddlClassDate.Items.Clear();
            foreach (var o in occurrences)
            {
                var oValue = string.Format("{0:yyyyMMdd}^{1}", o.OccurrenceDate, o.Id);
                var oText = string.Format("{0:d}", o.OccurrenceDate);
                var listItem = new ListItem(oText, oValue);
                ddlClassDate.Items.Add(listItem);
            }

            AttendanceOccurrence selectedOccurrence = null;
            if (selectedOccurrenceId.HasValue)
            {
                selectedOccurrence = occurrences.SingleOrDefault(o => o.Id == selectedOccurrenceId.Value);
            }

            if (selectedOccurrence == null)
            {
                selectedOccurrence = occurrences.FirstOrDefault();
            }

            if (selectedOccurrence != null)
            {
                var selectedValue = string.Format("{0:yyyyMMdd}^{1}", selectedOccurrence.OccurrenceDate, selectedOccurrence.Id);
                ddlClassDate.SelectedValue = selectedValue;
                UpdateOccurrenceFields(selectedOccurrence);
            }

            pnlGroupAttendance.Visible = true;
        }

        private void LoadGroupList(bool bypassForSingleGroup)
        {
            pnlGroupAttendance.Visible = false;
            pnlGroupList.Visible = false;

            lGroupListHeader.Text = GetAttributeValue("GroupListTitle");

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false });
            lListInstructions.Text = GetAttributeValue("GroupListInstructions").ResolveMergeFields(mergeFields, GetAttributeValue("EnabledLavaCommands"));

            var qry = new GroupMemberService(_rockContext)
                .Queryable("Group,GroupRole,Group.Campus,Group.Schedule");

            var parentGroupGuid = GetAttributeValue("ParentGroup").AsGuidOrNull();
            if (parentGroupGuid != null)
            {
                var groupSvc = new GroupService(_rockContext);
                var parentGroup = groupSvc.Get(parentGroupGuid.Value);

                var availableGroups = groupSvc.GetAllDescendentGroupIds(parentGroup.Id, false);

                qry = qry.Where(gm => availableGroups.Contains(gm.GroupId));
            }

            qry = qry.Where(gm => gm.GroupMemberStatus == GroupMemberStatus.Active)
                .Where(gm => gm.PersonId == CurrentPersonId)
                .Where(gm => !gm.IsArchived)
                .Where(gm => gm.Group.IsActive)
                .Where(gm => !gm.Group.IsArchived)
                .Where(gm => gm.GroupRole.CanView);

            List<Guid> includeGroupTypeGuids = GetAttributeValue("IncludedGroupTypes").SplitDelimitedValues().Select(a => Guid.Parse(a)).ToList();
            if (includeGroupTypeGuids.Count > 0)
            {
                qry = qry.Where(t => includeGroupTypeGuids.Contains(t.Group.GroupType.Guid));
            }

            List<Guid> excludeGroupTypeGuids = GetAttributeValue("ExcludedGroupTypes").SplitDelimitedValues().Select(a => Guid.Parse(a)).ToList();
            if (excludeGroupTypeGuids.Count > 0)
            {
                qry = qry.Where(t => !excludeGroupTypeGuids.Contains(t.Group.GroupType.Guid));
            }

            List<GroupAttendanceRowItem> groupAttendance = new List<GroupAttendanceRowItem>();
            var aoService = new AttendanceOccurrenceService(_rockContext);
            var sundayDate = RockDateTime.Today.SundayDate();
            var lastSundayDate = sundayDate.AddDays(-7);
            foreach (var gm in qry.OrderBy(m => m.Group.Name).ToList())
            {
                if (gm.IsAuthorized(Authorization.VIEW, CurrentPerson))
                {
                    var groupRow = new GroupAttendanceRowItem()
                    {
                        Group = gm.Group,
                        GroupId = gm.GroupId,
                        GroupRole = gm.GroupRole,
                        Id = gm.Id,
                    };

                    if (gm.Group.Schedule != null)
                    {
                        if (gm.Group.Schedule.WeeklyDayOfWeek.HasValue && gm.Group.Schedule.WeeklyTimeOfDay.HasValue)
                        {
                            var dayOfWeek = String.Empty;
                            switch (gm.Group.Schedule.WeeklyDayOfWeek.Value)
                            {
                                case DayOfWeek.Sunday:
                                    dayOfWeek = "Sun";
                                    break;
                                case DayOfWeek.Monday:
                                    dayOfWeek = "Mon";
                                    break;
                                case DayOfWeek.Tuesday:
                                    dayOfWeek = "Tues";
                                    break;
                                case DayOfWeek.Wednesday:
                                    dayOfWeek = "Wed";
                                    break;
                                case DayOfWeek.Thursday:
                                    dayOfWeek = "Thur";
                                    break;
                                case DayOfWeek.Friday:
                                    dayOfWeek = "Fri";
                                    break;
                                case DayOfWeek.Saturday:
                                    dayOfWeek = "Sat";
                                    break;
                                default:
                                    break;
                            }

                            groupRow.MeetingTime = string.Concat(dayOfWeek, " ",
                                RockDateTime.Today.Add(gm.Group.Schedule.WeeklyTimeOfDay.Value).ToShortTimeString());
                        }
                    }

                    groupRow.Occurrences = aoService.Queryable()
                        .AsNoTracking()
                        .Where(o => o.GroupId == gm.GroupId)
                        .Where(o => o.SundayDate >= lastSundayDate && o.SundayDate <= sundayDate)
                        .ToList();

                    groupAttendance.Add(groupRow);
                }
            }

            gGroupList.DataSource = groupAttendance;
            gGroupList.DataBind();

            if (groupAttendance.Count == 0)
            {
                UpdateNotification("No Groups Found", "<p>No Groups available to list.</p>", NotificationBoxType.Warning);
                return;
            }
            else if(groupAttendance.Count == 1 && bypassForSingleGroup)
            {
                LoadGroup(groupAttendance.FirstOrDefault().GroupId);
                return;
            }

            pnlGroupList.Visible = true;

        }

        private void SaveOccurrence()
        {
            var occurrenceValue = ddlClassDate.SelectedValue.Split("^".ToCharArray());
            var occurrenceId = occurrenceValue[1].AsInteger();
            var year = occurrenceValue[0].Substring(0, 4).AsInteger();
            var month = occurrenceValue[0].Substring(4, 2).AsInteger();
            var day = occurrenceValue[0].Substring(6, 2).AsInteger();

            if (tbAttendees.Text.IsNullOrWhiteSpace() && !cbDidNotOccur.Checked)
            {
                UpdateNotification(
                    "Please Correct the Following",
                    "<p style=\"font-weight:400;\"> <strong>Attendee Count</strong> is required or <strong>We Did Not Meet</strong> must be checked.</p>",
                    NotificationBoxType.Validation);
                return;
            }

            if (tbAttendees.Text.IsNotNullOrWhiteSpace() && cbDidNotOccur.Checked)
            {
                UpdateNotification(
                    "Please Correct the Following",
                    "<p style=\"font-weight:400;\"> If <strong>Attendee Count</strong> is entered, <strong>We Did Not Meet</strong> must be un-checked.</p>",
                    NotificationBoxType.Validation);
                return;
            }

            var occurrenceDate = new DateTime(year, month, day);

            using (var occurrenceContext = new RockContext())
            {
                var aoService = new AttendanceOccurrenceService(occurrenceContext);
                var occurrence = aoService.Get(occurrenceId);

                var group = new GroupService(occurrenceContext).Get(hfGroupId.ValueAsInt());

                if (occurrence == null || occurrence.Id <= 0)
                {
                    occurrence = aoService.Queryable()
                        .Where(o => o.GroupId == group.Id)
                        .Where(o => o.OccurrenceDate == occurrenceDate)
                        .Where(o => o.ScheduleId == group.ScheduleId)
                        .FirstOrDefault();
                }
                if (occurrence == null || occurrence.Id <= 0)
                {

                    
                    occurrence = new AttendanceOccurrence();
                    occurrence.GroupId = group.Id;

                    //if (group.GroupLocations.Count > 0)
                    //{
                    //    occurrence.LocationId = group.GroupLocations.First().LocationId;
                    //}

                    occurrence.ScheduleId = group.ScheduleId;
                    occurrence.OccurrenceDate = new DateTime(year, month, day);
                    aoService.Add(occurrence);
                }

                var attendeeCount = tbAttendees.Text.AsInteger();

                if (attendeeCount > 0)
                {
                    occurrence.AnonymousAttendanceCount = attendeeCount;
                }
                else
                {
                    occurrence.AnonymousAttendanceCount = null;
                }

                occurrence.DidNotOccur = cbDidNotOccur.Checked;
                occurrence.Notes = tbNotes.Text.Trim();
                               
                occurrenceContext.SaveChanges();
                LoadGroup(hfGroupId.ValueAsInt(), occurrence.Id);
            }

    
            string redirectUrl = GetAttributeValue("RedirectURL");
            int redirectDelay = GetAttributeValue("RedirectDelay").AsInteger();

            var notificationMessageFormat = "<p>Thank you for updating your group's attendance.{0}</p>";
            string notificationMessage = String.Empty;
            if (redirectUrl.IsNotNullOrWhiteSpace() && !IsReferredFromInternalSite())
            {
                string redirectMessage = string.Format("<br />You will be redirected to your group within {0} {1}.",
                    redirectDelay, "second".PluralizeIf(redirectDelay != 1));
                notificationMessage = string.Format(notificationMessageFormat, redirectMessage);

                RedirectOnSave(redirectUrl, redirectDelay);
            }
            else
            {
                notificationMessage = string.Format(notificationMessageFormat, string.Empty);
            }

            UpdateNotification("Attendance Saved", notificationMessage, NotificationBoxType.Success);
        }

        private void RedirectOnSave(string url, int delaySeconds)
        {
           
            if (url.IsNullOrWhiteSpace())
            {
                return;
            }
            url = url.ToLower().Replace("{groupid}", hfGroupId.Value);
            var delay = delaySeconds * 1000;
            var script = string.Format("submitRedirect(\"{0}\",{1});", url, delay);
            ScriptManager.RegisterStartupScript(upAttendanceEntry, upAttendanceEntry.GetType(), "redirect" + DateTime.Now.Ticks, script, true);


        }

        private void UpdateNotification(string title, string message, NotificationBoxType boxType)
        {
            nbAlert.Title = title;
            nbAlert.Text = message;
            nbAlert.NotificationBoxType = boxType;

            nbAlert.Visible = message.IsNotNullOrWhiteSpace();
        }

        private void UpdateOccurrenceFields(AttendanceOccurrence o)
        {
            tbAttendees.Text = String.Empty;
            cbDidNotOccur.Checked = false;
            tbNotes.Text = string.Empty;
            lUpdatedBy.Text = string.Empty;
            lUpdatedBy.Visible = false;

            if (o.DidNotOccur == true)
            {
                tbAttendees.Attributes.Add("disabled", "");
                cbDidNotOccur.Checked = true;
            }
            else
            {
                tbAttendees.Attributes.Remove("disabled");
                tbAttendees.Text = o.AnonymousAttendanceCount.HasValue ? o.AnonymousAttendanceCount.Value.ToString() : String.Empty;
            }

            if (o.Notes.IsNotNullOrWhiteSpace())
            {
                tbNotes.Text = o.Notes.Trim();
            }

            if (o.ModifiedByPersonAliasId.HasValue)
            {
                lUpdatedBy.Text = string.Format("{0} on {1:g}", o.ModifiedByPersonName, o.ModifiedDateTime);
                lUpdatedBy.Visible = true;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendFormat("$('#{0}').click(function() {{ \n", cbDidNotOccur.ClientID);
            sb.Append("     if(this.checked){\n");
            sb.AppendFormat("          $('#{0}').prop('disabled', true);\n", tbAttendees.ClientID);
            sb.AppendFormat("          $('#{0}').val('');\n", tbAttendees.ClientID);
            sb.Append("     }\n");
            sb.Append("     else {\n");
            sb.AppendFormat("          $('#{0}').prop('disabled', false);\n", tbAttendees.ClientID);
            sb.Append("     }\n");
            sb.Append("});");
     

            ScriptManager.RegisterStartupScript(pnlGroupAttendance, pnlGroupAttendance.GetType(), "disableAttendeeBox" + DateTime.Now.Ticks, sb.ToString(), true);

        }





        #endregion
    }

    public class GroupAttendanceRowItem
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }
        public GroupTypeRole GroupRole { get; set; }
        public string MeetingTime { get; set; }
        public List<AttendanceOccurrence> Occurrences { get; set; }
    }
}