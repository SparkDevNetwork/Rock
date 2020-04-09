using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

/// <summary>
/// Barcode Roster with advanced filtering. 
/// North Point Ministries
/// Author: daniel.rychlik@bemaservices.com
/// </summary>
namespace RockWeb.Plugins.com_bemaservices.BarcodeAttendance
{
    #region Static Class

    /// <summary>
    /// Crazy little helper for handling opening pages in a new tab
    /// </summary>
    public static class ResponseHelper
    {
        public static void Redirect(this HttpResponse response, string url, string target, string windowFeatures)
        {

            if ((String.IsNullOrEmpty(target) || target.Equals("_self", StringComparison.OrdinalIgnoreCase)) && String.IsNullOrEmpty(windowFeatures))
            {
                response.Redirect(url);
            }
            else
            {
                System.Web.UI.Page page = (System.Web.UI.Page)HttpContext.Current.Handler;

                if (page == null)
                {
                    throw new InvalidOperationException("Cannot redirect to new window outside Page context.");
                }

                string script;
                if (!String.IsNullOrEmpty(windowFeatures))
                {
                    script = @"window.open(""{0}"", ""{1}"", ""{2}"");";
                }
                else
                {
                    script = @"window.open(""{0}"", ""{1}"");";
                }
                script = String.Format(script, url, target, windowFeatures);
                ScriptManager.RegisterStartupScript(page, typeof(System.Web.UI.Page), "Redirect", script, true);
            }
        }
    }
    #endregion

    [DisplayName("Barcode Group Location/Schedule Filter")]
    [Category("com_bemaservices > Barcode Attendance")]
    [Description("Displays groups, using pre-defined filters, that allows groups to be aggregated and printed together.  This only selects groups that have group types that actually takes attendance.")]
    [GroupTypesField("Group Types Include",
    "Select any specific group types to show in this block. If multiple items selected, dropdown list will appear in block", required: false, key: "GroupTypes", order: 1)]
    [BooleanField(name: "Display the Grid", key: "DisplayGrid", description: "Check this box to show/hide the groups display grid", defaultValue: true, order: 3)]
    [LinkedPage(name: "Page Redirect", description: "If set, the filter button will redirect to the selected page.", required: true, key: "PageId", order: 5)]
    [BooleanField( name: "Show Parent Group Filter", description: "If set, a Parent Group filter will show.", defaultValue: true, order: 6)]
    [BooleanField( name: "Show Schedule Filter", description: "If Set, a Group Location Schedule filter will show.", defaultValue: true, order: 7 )]
    [BooleanField( name: "Show Location Filter", description: "If Set, a Group Location filter will show.", defaultValue: true, order: 8)]
    [BooleanField( name: "Show Campus Filter", description: "If Set, the Campus filter selector will show.", defaultValue: true, order: 9)]
    public partial class CampusServiceTimeRoomFilter : RockBlock
    {

        #region Fields

        protected Campus SelectedCampus;

        #endregion

        #region Base Methods

        /// <summary>
        /// OnInit
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            grdGroups.DataKeyNames = new string[] { "GroupId" };

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger(upnlContent);
            //SetAllowedGroupTypes();

            //CheckForLavaToPdfPlugin();
            CheckRedirectPageId();

        }

        /// <summary>
        /// OnLoad
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                SetCampusByQueryStringOrCurrentPersonDefault();

                SetFilters();

                BindDropDownItems();

                //HandleSpecialCases();

                BindGrid();
            }
            
            GenerateUrl( GetGroupItemList() );
        }
        

        #endregion

        #region Private Methods

        /// <summary>
        /// SetAllowedGroupTypes
        /// </summary>
        //private void SetAllowedGroupTypes()
        //{
        //    // limit GroupType selection to what Block Attributes allow
        //    hfGroupTypesInclude.Value = string.Empty;
        //    List<Guid> groupTypeIncludeGuids = GetAttributeValue("GroupTypes").SplitDelimitedValues().AsGuidList();

        //    if (groupTypeIncludeGuids.Any())
        //    {
        //        var groupTypeIdIncludeList = new List<int>();
        //        foreach (Guid guid in groupTypeIncludeGuids)
        //        {
        //            var groupType = GroupTypeCache.Get(guid);
        //            if (groupType != null)
        //            {
        //                groupTypeIdIncludeList.Add(groupType.Id);
        //            }
        //        }

        //        hfGroupTypesInclude.Value = groupTypeIdIncludeList.AsDelimited(",");
        //    }

        //    hfGroupTypesExclude.Value = string.Empty;
        //    List<Guid> groupTypeExcludeGuids = GetAttributeValue("GroupTypesExclude").SplitDelimitedValues().AsGuidList();
        //    if (groupTypeExcludeGuids.Any())
        //    {
        //        var groupTypeIdExcludeList = new List<int>();
        //        foreach (Guid guid in groupTypeExcludeGuids)
        //        {
        //            var groupType = GroupTypeCache.Get(guid);
        //            if (groupType != null)
        //            {
        //                groupTypeIdExcludeList.Add(groupType.Id);
        //            }
        //        }

        //        hfGroupTypesExclude.Value = groupTypeIdExcludeList.AsDelimited(",");
        //    }
        //}

        

        /// <summary>
        /// Checks the redirect page for 0 value
        /// </summary>
        protected void CheckRedirectPageId()
        {
            string pageGuid = GetAttributeValue("PageId");

            wbZeroLavaToPdfPageId.Visible = string.IsNullOrEmpty(pageGuid);
            divZeroLavaToPdfPageId.Visible = string.IsNullOrEmpty(pageGuid);
        }

        /// <summary>
        /// SetCampusByQueryStringOrCurrentPersonDefault
        /// </summary>
        private void SetCampusByQueryStringOrCurrentPersonDefault()
        {
            CampusPicker campusPicker = FindControl("r_CampusPicker") as CampusPicker;

            if (!string.IsNullOrEmpty(Request.QueryString["campusId"]))
            {
                int campusId = 0;
                if (int.TryParse(Request.QueryString["campusId"], out campusId))
                {
                    using (RockContext rockContext = new RockContext())
                    {
                        CampusService campusService = new CampusService(rockContext);
                        Campus campus = campusService.Get(campusId);
                        campusPicker.SelectedCampusId = campusId;
                        SelectedCampus = campus;
                    }
                }
            }
            else
            {
                // Set the campus filter by the person's campus
                Campus campus = CurrentPerson.GetCampus();

                if (campus != null)
                {
                    campusPicker.SelectedCampusId = campus.Id;
                    SelectedCampus = campus;
                }
            }
        }

        /// <summary>
        /// SetSelectedCampusFromLocationItemPicker
        /// </summary>
        /// <param name="campusId"></param>
        private void SetSelectedCampusFromLocationItemPicker(int campusId)
        {
            using (RockContext rockContext = new RockContext())
            {
                CampusService campusService = new CampusService(rockContext);
                Campus campus = campusService.Get(campusId);
                SelectedCampus = campus;
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Block_BlockUpdated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Block_BlockUpdated(object sender, EventArgs e)
        {
            //SetAllowedGroupTypes();

            SetCampusByQueryStringOrCurrentPersonDefault();
            SetFilters();
            BindDropDownItems();

            BindGrid();
        }

        

        protected void SetFilters()
        {
            //Show/Hide GroupType selector
            hfGroupTypesInclude.Value = string.Empty;
            List<Guid> groupTypeIncludeGuids = GetAttributeValue( "GroupTypes" ).SplitDelimitedValues().AsGuidList();
            var groupTypes = GroupTypeCache.All().Where( gt => groupTypeIncludeGuids.Contains( gt.Guid ) ).ToList();
            if ( groupTypes.Any() )
            {
                ddl_GroupType.DataSource = groupTypes.Select( gt => new { Text = gt.Name, Value = gt.Id } );
                ddl_GroupType.DataBind();

                if ( groupTypes.Count() == 1 )
                {
                    ddl_GroupType.SelectedValue = groupTypes.First().Id.ToString();
                    ddl_GroupType.Visible = false;
                    hfGroupTypesInclude.Value = groupTypes.First().Id.ToString();
                }
            }
            

            //Show Filters based on block attributes
            r_CampusPicker.Visible = GetAttributeValue( "ShowCampusFilter" ).AsBoolean();
            rddl_ParentGroup.Visible = GetAttributeValue( "ShowParentGroupFilter" ).AsBoolean();
            rddl_ClassSelector.Visible = GetAttributeValue( "ShowLocationFilter" ).AsBoolean();
            rddl_ServiceTimeSelector.Visible = GetAttributeValue( "ShowScheduleFilter" ).AsBoolean();
            
            
        }

        

        /// <summary>
        /// Builds a list of Service times to choose from 
        /// </summary>
        protected void BindDropDownItems()
        {
            RockContext rockContext = new RockContext();

            //Get selected Group Type
            int? groupTypeId = hfGroupTypesInclude.Value.AsIntegerOrNull();
            if ( !groupTypeId.HasValue )
            {
                return;
            }

            //Try to get campus
            int? campusId = r_CampusPicker.SelectedCampusId;

            //Bind Parent Groups
            var parentGroups = new GroupService( rockContext ).GetByGroupTypeId( groupTypeId.Value )
                .Where( p => p.IsActive == true && p.IsArchived != true );
             
            if ( campusId.HasValue )
            {
                parentGroups = parentGroups.Where( p => p.CampusId == campusId );
            }

            rddl_ParentGroup.DataSource = parentGroups.Select( g => g.ParentGroup ).Select( p => new ListItem { Text = p.Name, Value = p.Id.ToString() } ).ToList().Distinct();
            rddl_ParentGroup.DataBind();

            //Bind Schedules

            var scheduleGroups = new GroupService( rockContext ).GetByGroupTypeId( groupTypeId.Value )
                .Where( g => g.IsActive == true && g.IsArchived != true );

            if ( campusId.HasValue )
            {
                scheduleGroups = scheduleGroups.Where( g => g.CampusId == campusId );
            }

            var schedules = scheduleGroups
                .SelectMany( g => g.GroupLocations.SelectMany( gl => gl.Schedules ) )
                .Where( s => s.IsActive == true ).ToList();

            // schedules directly bound to a group
            var groupSchedules = scheduleGroups.Select( g => g.Schedule ).Where( s => s.IsActive == true ).ToList();
            schedules.AddRange( groupSchedules );

            rddl_ServiceTimeSelector.DataSource = schedules.Select( s => new ListItem { Text = s.Name ?? s.FriendlyScheduleText, Value = s.Id.ToString() } ).ToList().Distinct();
            rddl_ServiceTimeSelector.DataBind();

            //Bind Locations

            var locationGroups = new GroupService( rockContext ).GetByGroupTypeId( groupTypeId.Value )
                .Where( g => g.IsActive == true && g.IsArchived != true );

            if ( campusId.HasValue )
            {
                locationGroups = locationGroups.Where( g => g.CampusId == campusId );
            }

            var locations = locationGroups
                .SelectMany( g => g.GroupLocations.Select( gl => gl.Location ) )
                .Where( l => l.IsActive == true ).ToList();

            rddl_ClassSelector.DataSource = locations.Select( s => new ListItem { Text = s.Name ?? s.FormattedAddress, Value = s.Id.ToString() } ).ToList().Distinct();
            rddl_ClassSelector.DataBind();
        }

        /// <summary>
        /// AddSelectedValues
        /// </summary>
        /// <param name="selectedValues">List<string></param>
        protected void AddSelectedServiceTimeValues(List<string> selectedValues)
        {
            if (selectedValues.Any())
            {
                // Read the HF, and add this value to that running list...  Re-write the HF
                hfSelectedServiceTimes.Value = string.Empty;

                string list = String.Join(",", selectedValues);

                hfSelectedServiceTimes.Value = list;
            }
            else
            {
                // The box could have been cleared out.
                hfSelectedServiceTimes.Value = string.Empty;
            }
        }

        /// <summary>
        /// AddSelectedLocationValues
        /// </summary>
        /// <param name="selectedValues">List<string></param>
        protected void AddSelectedLocationValues(List<string> selectedValues)
        {
            if (selectedValues.Any())
            {
                hfSelectedLocations.Value = string.Empty;

                string list = String.Join(",", selectedValues);

                hfSelectedLocations.Value = string.Empty;
            }
            else
            {
                hfSelectedLocations.Value = string.Empty;
            }
        }

        

        /// <summary>
        /// Bind Grid()
        /// </summary>
        protected void BindGrid()
        {
            List<GroupItem> groupItemList = GetGroupItemList();
            
            grdGroups.DataSource = groupItemList;
            grdGroups.DataBind();

            grdGroups.Visible = bool.Parse(GetAttributeValue("DisplayGrid"));
        }
        

        /// <summary>
        /// Generates the URL.
        /// </summary>
        /// <param name="groupItemList">The group item list.</param>
        protected void GenerateUrl( List<GroupItem> groupItems )
        {
            List<string> locationIds = rddl_ClassSelector.SelectedValues;
            List<int> groupIds = new List<int>();

            foreach ( GridViewRow gridViewRow in grdGroups.Rows )
            {
                // Be safe...  
                if ( gridViewRow.RowType == DataControlRowType.DataRow )
                {
                    CheckBox checkBox = gridViewRow.Cells[0].Controls[1] as CheckBox;
                    if ( checkBox != null && checkBox.Checked )
                    {
                        HiddenField groupHiddenField = gridViewRow.FindControl( "GroupId" ) as HiddenField;

                        int groupId = 0;

                        bool parsed = Int32.TryParse( groupHiddenField.Value, out groupId );

                        if ( parsed && groupId > 0 )
                        {
                            groupIds.Add( groupId );
                        }
                    }
                }
            }

            int? pageId = GetPage();

            if ( !pageId.HasValue )
            {
                throw new Exception( "Invalid Page selection.  Page not found" );
            }

            var selectedScheduleIds = hfSelectedServiceTimes.Value;

            //if no location Ids, add all of them
            if (!locationIds.Any())
            {
                foreach( ListItem item in rddl_ClassSelector.Items )
                {
                    if(item.Value != "0") //dont select 'Select All'
                    {
                        locationIds.Add( item.Value );
                    }
                }
            }
            string locationIdsAsCommaSeperated = string.Join( ",", locationIds );

            //if no groups selected, select all
            if( !groupIds.Any() )
            {
                groupIds = groupItems.Select( g => g.GroupId ).ToList();
            }
            

            RockRadioButtonList displayNameRadioList = FindControl( "rrb_DisplayName" ) as RockRadioButtonList;
            RockRadioButtonList combineClassesRadioList = FindControl( "rrb_CombineClasses" ) as RockRadioButtonList;

            var queryString = System.Web.HttpUtility.ParseQueryString( String.Empty );
            queryString.Set( "ViewDocument", "true" );
            queryString.Set( "RoomType", GetAttributeValue( "Ministry" ) );

            if ( r_CampusPicker.SelectedCampusId.HasValue )
            {
                queryString.Set( "CampusId", r_CampusPicker.SelectedCampusId.Value.ToStringSafe() );
            }

            // Lets help our report a bit for combined classes and just pass another string value
            if ( combineClassesRadioList.SelectedValue.Equals( "Yes" ) )
            {
                var rockContext = new RockContext();

                var s = GetSchedulesAsCommaSeperatedString( rockContext );
                var l = GetLocationsAsCommaSeperatedString( rockContext );

                queryString.Set( "SchedulesHeading", s );

                queryString.Set( "ClassesHeading", l );
            }

            queryString.Set( "GroupIds", groupIds.AsDelimited(",") );
            queryString.Set( "GroupTypeIds", hfGroupTypesInclude.Value );
            queryString.Set( "LocationIds", locationIdsAsCommaSeperated );
            queryString.Set( "ScheduleIds", selectedScheduleIds );
            queryString.Set( "DisplayNameFormat", displayNameRadioList.SelectedValue );
            queryString.Set( "CombineClasses", combineClassesRadioList.SelectedValue );

            String url = String.Format( "/page/{0}?{1}", pageId, queryString );

            url = ResolveRockUrlIncludeRoot( url );

            btnGenerate.Attributes.Add( "href", url );
            btnGenerate.Attributes.Add( "target", "_blank" );
            btnGenerate.Text = string.Format( "Generate {0} Groups", groupIds.Count );

        }

        /// <summary>
        /// Get the named list of the schedules as a comma seperated list.
        /// </summary>
        /// <returns>string</returns>
        private string GetSchedulesAsCommaSeperatedString(RockContext rockContext)
        {
            string s = string.Empty;

            RockListBox serviceTimes = FindControl("rddl_ServiceTimeSelector") as RockListBox;

            List<Schedule> schedules = new ScheduleService(rockContext).GetListByIds(serviceTimes.SelectedValuesAsInt);

            s = string.Join(", ", schedules.Select(p => p.Name).ToArray());


            return s;
        }


        /// <summary>
        /// Get the named locations list as comma seperated.
        /// </summary>
        /// <returns>string</returns>
        private string GetLocationsAsCommaSeperatedString(RockContext rockContext)
        {
            string s = string.Empty;

            RockListBox classRooms = FindControl("rddl_ClassSelector") as RockListBox;
            List<int> selectedInts = new List<int>();
            //if none already selected, select all
            if ( !classRooms.SelectedValues.Any() )
            {
                //Dont use item 0, its the Select All 
                for ( int i = 1; i < classRooms.Items.Count; i++ )
                {
                    selectedInts.Add( Int32.Parse(classRooms.Items[i].Value) );
                }
            }
            else
            {
                selectedInts = classRooms.SelectedValuesAsInt;
            }

            List<Location> locations = new LocationService(rockContext).GetListByIds(selectedInts);
            

            s = string.Join(", ", locations.Select(p => p.Name).ToArray());


            return s;
        }

        /// <summary>
        /// Get the group item list
        /// </summary>
        /// <returns>List<GroupItem></returns>
        private List<GroupItem> GetGroupItemList()
        {
            List<GroupItem> groupItemList = new List<GroupItem>();
            
            CampusPicker campusPicker = FindControl("r_CampusPicker") as CampusPicker;
            RockDropDownList groupTypePicker = FindControl( "ddl_GroupType" ) as RockDropDownList;
            RockListBox parentGroups = FindControl( "rddl_ParentGroup" ) as RockListBox;
            RockListBox serviceTimes = FindControl("rddl_ServiceTimeSelector") as RockListBox;
            RockListBox classRooms = FindControl("rddl_ClassSelector") as RockListBox;

            var groupTypeId = groupTypePicker.SelectedValueAsId();
            
            using (RockContext rockContext = new RockContext())
            {
                // Lets do this.
                var queryable = new GroupService(rockContext).Queryable("GroupType, GroupLocations.Schedules").AsNoTracking();

                // IQueryable<Group> queryable;

                // Need to only filter groups that actually takes attendance and that are active
                // and only selected group types
                queryable = queryable.AsNoTracking()
                    .Where(
                        p => p.GroupType.TakesAttendance == true
                        && p.IsActive == true
                        && p.IsArchived != true
                        && groupTypeId == p.GroupTypeId
                    );

                if (campusPicker.IsNotNull() && campusPicker.SelectedCampusId.HasValue)
                {
                    queryable = queryable.Where(p => p.CampusId == campusPicker.SelectedCampusId.Value);
                }
                

                // Handle the location item Picker picker
                if (classRooms.SelectedValuesAsInt.Any())
                {
                    var locationIds = classRooms.SelectedValuesAsInt;

                    queryable = queryable.AsQueryable().AsNoTracking()
                        .Where(p => p.GroupLocations
                             .Where(gl => locationIds.Contains(gl.Location.Id)
                         ).Any()
                    );
                }

                if (serviceTimes.SelectedValuesAsInt.Any())
                {
                    var selectedScheduleIds = serviceTimes.SelectedValuesAsInt;
                    // Handle Group location schedule filters
                    queryable = queryable.AsQueryable().AsNoTracking()
                        .Where(p => p.GroupLocations
                             .Where(gl => gl.Schedules.AsQueryable()
                                  .Where(s => selectedScheduleIds.Contains(s.Id) && s.IsActive == true).Any()
                         ).Any()
                          || selectedScheduleIds.Contains( p.ScheduleId ?? -1 ) );
                }

                if ( parentGroups.SelectedValuesAsInt.Any() )
                {
                    var selectedParentGroupIds = parentGroups.SelectedValuesAsInt;

                    queryable = queryable.AsQueryable().AsNoTracking()
                        .Where( p => selectedParentGroupIds.Contains( p.ParentGroupId ?? -1 ) );
                }

                List<Group> groupList = queryable.AsNoTracking().ToList();

                

                foreach (Group group in groupList)
                {

                    String parentGroupName = (group.ParentGroup != null) ? group.ParentGroup.Name : "";
                    groupItemList.Add(new GroupItem
                        (
                            group.Id,
                            group.Name,
                            (group.Campus != null) ? group.Campus.Name : string.Empty,
                            parentGroupName,
                            group.Members.Count,
                            group.GroupCapacity,
                            GetGroupLocationSchedules(group),
                            GetGroupLocation(group.GroupLocations)
                        )
                    );
                }
            }

            return groupItemList;
        }

        /// <summary>
        /// Gets the page id from the Guid selection
        /// </summary>
        /// <returns></returns>
        private int? GetPage()
        {
            int? pageId = 0;
            string pageGuidasString = GetAttributeValue("PageId");

            Guid pageGuid;

            if (Guid.TryParse(pageGuidasString, out pageGuid))
            {
                using (RockContext rockContext = new RockContext())
                {
                    PageService pageService = new PageService(rockContext);
                    List<Rock.Model.Page> pages = pageService.Queryable().Where(p => p.Guid == pageGuid).ToList();

                    Rock.Model.Page page = pages.FirstOrDefault();

                    if (page != null)
                    {
                        pageId = page.Id;
                    }
                }
            }

            return pageId;
        }

        /// <summary>
        /// Builds the locations and schedules
        /// </summary>
        /// <param name="group">Group</param>
        /// <returns>String</returns>
        private string GetGroupLocationSchedules(Group group)
        {
            string s = string.Empty;
            List<string> groupSchedules = new List<string>();

            if (group.Schedule != null)
            {
                groupSchedules.Add(group.Schedule.Name);
            }

            if (group.GroupLocations.Any())
            {
                foreach (GroupLocation groupLocation in group.GroupLocations)
                {
                    string name = (groupLocation.Location.IsNamedLocation) ? groupLocation.Location.Name : groupLocation.Location.FormattedAddress;

                    if (groupLocation.Schedules.Any())
                    {
                        string schedules = string.Join("; ", groupLocation.Schedules.Select(gls => gls.Name).ToArray());
                        groupSchedules.Add(string.Format("{0} @ {1}", name, schedules));

                    }
                    else
                    {
                        groupSchedules.Add(string.Format("{0} (no set schedule)", name));
                    }
                }
            }

            s = string.Join(", ", groupSchedules.ToArray());

            return s;

        }

        /// <summary>
        /// Get the group location
        /// </summary>
        /// <param name="groupLocations">ICollection<GroupLocation></param>
        /// <returns>string</returns>
        private string GetGroupLocation(ICollection<GroupLocation> groupLocations)
        {
            string s = "";

            if (groupLocations.Any())
            {
                List<string> namedLocationNames = groupLocations.Where(gl => gl.Location.IsNamedLocation == true).Select(p => p.Location.Name).ToList();
                List<string> addressLocations = groupLocations.Where(gl => gl.Location.IsNamedLocation == false).Select(p => p.Location.FormattedAddress).ToList();

                string[] list;

                if (namedLocationNames.Count > 0)
                {
                    list = namedLocationNames.Concat(addressLocations).ToArray();
                }
                else
                {
                    list = addressLocations.ToArray();
                }

                s = string.Join(", ", list);

            }

            return s;
        }

        #endregion

        #region Events

        /// <summary>
        /// r_CampusPicker_SelectedIndexChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void r_CampusPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            CampusPicker campusPicker = sender as CampusPicker;

            if (!campusPicker.SelectedCampusId.HasValue)
            {
                SelectedCampus = null; // Null it out.
                BindDropDownItems();
                return;
            }

            SetSelectedCampusFromLocationItemPicker(campusPicker.SelectedCampusId.Value);

            BindDropDownItems();
            
        }

        /// <summary>
        /// rddl_ServiceTimeSelector_SelectedIndexChanged
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">EventArgs</param>
        protected void rddl_ServiceTimeSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            RockListBox serviceTimeListBox = sender as RockListBox;
            

            List<string> selectedValues = serviceTimeListBox.SelectedValues;

            AddSelectedServiceTimeValues(selectedValues);
            
            
        }

        /// <summary>
        /// rddl_ClassSelector_SelectedIndexChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rddl_ClassSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            RockListBox rockListBox = sender as RockListBox;

            //if 'Select All', run again and select all elements.
            if(rockListBox.SelectedValuesAsInt.Contains(0))
            {
                for(int i=1; i < rockListBox.Items.Count; i++ )
                {
                    rockListBox.Items[i].Selected = true;
                }
                rockListBox.Items[0].Selected = false;
            }
            
        }


        protected void rddl_ParentGroup_SelectedIndexChanged( object sender, EventArgs e )
        {

        }


        protected void ddl_GroupType_SelectedIndexChanged( object sender, EventArgs e )
        {
            hfGroupTypesInclude.Value = ddl_GroupType.SelectedValue;
        }

        /// <summary>
        /// rbb_Filter_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rbb_Filter_Click(object sender, EventArgs e)
        {
            // Write the  values to the HF
            List<string> selectedLocationValues = rddl_ClassSelector.SelectedValues;

            //If none, select all locations
            if ( !selectedLocationValues.Any() )
            {
                // start at 1 to bypass the 'Select All' 0 option
                for ( int i = 1; i < rddl_ClassSelector.Items.Count; i++ )
                {
                    selectedLocationValues.Add( rddl_ClassSelector.Items[i].Value );
                }
            }

            AddSelectedLocationValues(selectedLocationValues);


            BindGrid();
        }

        /// <summary>
        /// generate_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void generate_Click(object sender, EventArgs e)
        {
            List<string> selectedLocationValues = rddl_ClassSelector.SelectedValues;

            //If none, select all locations
            if(!selectedLocationValues.Any())
            {
                // start at 1 to bypass the 'Select All' 0 option
                for ( int i = 1; i < rddl_ClassSelector.Items.Count; i++ )
                {
                    selectedLocationValues.Add( rddl_ClassSelector.Items[i].Value );
                }
            }

            // Do not use Navigate To Page becuase of the popup errors. Change button href instead when binding grid

            //NavigateToPage(selectedLocationValues);
        }

        protected void refreshUrl_Click( object sender, EventArgs e )
        {
            //recreate Url and post to the button
            this.GenerateUrl( GetGroupItemList() );
        }


        #endregion

        #region Classes

        /// <summary>
        /// GroupItem
        /// </summary>
        public class GroupItem
        {
            public int GroupId { get; set; }

            public bool MarkChecked
            {
                get
                {
                    return MembersCount > 0;
                }
            }

            public string GroupName { get; set; }

            public string Campus { get; set; }

            public string ParentGroupName { get; set; }

            public int MembersCount { get; set; }

            public int? GroupCapacity { get; set; }

            public string ScheduledList { get; set; }

            public string MeetingLocation { get; set; }

            /// <summary>
            /// GroupItem
            /// </summary>
            /// <param name="groupId">int</param>
            /// <param name="groupName">string</param>
            /// <param name="campus">string</param>
            /// <param name="parentGroupName">string</param>
            /// <param name="membersCount">int</param>
            /// <param name="groupCapacity">int</param>
            /// <param name="scheduledList">string</param>
            /// <param name="meetingLocation">string</param>
            public GroupItem(int groupId, string groupName, string campus, string parentGroupName, int membersCount,
                int? groupCapacity, string scheduledList, string meetingLocation)
            {
                this.GroupId = groupId;
                this.GroupName = groupName;
                this.Campus = campus;
                this.ParentGroupName = parentGroupName;
                this.MembersCount = membersCount;
                this.GroupCapacity = groupCapacity.GetValueOrDefault();
                this.ScheduledList = scheduledList;
                this.MeetingLocation = meetingLocation;
            }
        }


        #endregion



    }
}