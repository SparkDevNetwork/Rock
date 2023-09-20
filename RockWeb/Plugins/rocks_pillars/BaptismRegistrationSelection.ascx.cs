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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Lava;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Ical.Net.DataTypes;
using System.Data.Entity;
using System.Text.RegularExpressions;

namespace RockWeb.Plugins.rocks_pillars.Event
{
    /// <summary>
    /// Renders a Baptism selection page which will end up linking to the Registration Entry Page
    /// </summary>
    [DisplayName("Baptism Registration Selection")]
    [Category("Pillars > Event")]
    [Description("Renders a Baptism selection page which will end up linking to the Registration Entry Page.")]

    [GroupField("Base Baptism Group",
        Description = "",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.BaseBaptismGroup)]

    [RegistrationTemplateField("Baptism Registration Template",
        Description = "This is registration template to search for baptism registations",
        IsRequired = true,
        Order = 1,
        Key = AttributeKey.BaptismRegTemplate)]

    [BooleanField( "Enable Passing Query Parameters",
        Description = @"Should passing query parameters to this block to skip steps be allowed
Avalaible parameters:
    EventId (int) - The group Id of the baptism event group
    CampusGroupId (int) - The group Id of the campus group
    Date (string) - Format: M/d/yyyy",
        IsRequired = true,
        DefaultBooleanValue = true,
        Order = 2,
        Key = AttributeKey.EnableQueryParameters )]

    [BooleanField("Show Count On Baptism Type",
        Description = "Should we show the count on the Baptism Type",
        IsRequired = true,
        Order = 3,
        Key = AttributeKey.ShowEventTypeCount)]

    [BooleanField("Show Count On Campus",
        Description = "Should we show the count on the Campus Selection",
        IsRequired = true,
        Order = 4,
        Key = AttributeKey.ShowCampusCount)]

    [BooleanField("Show Count On Date",
        Description = "Should we show the count on the Date Selection",
        IsRequired = true,
        Order = 5,
        Key = AttributeKey.ShowDateCount)]

    [BooleanField("Show Count On Time",
        Description = "Should we show the count on the Schedule selection",
        IsRequired = true,
        Order = 6,
        Key = AttributeKey.ShowTimeCount)]

    [LavaField("Picker Lava Template",
        Description = "This is what each item the user selects along the way will see",
        IsRequired = true,
        DefaultValue = @"{% if Step != null and  Step == 'Baptism Type' %}
    <div class=""card"" style=""min-height:350px;"">
        <h2>{{ Item.ItemName }}</h2>
        <div style=""text-align:center;"">
            {% assign group = Item.ItemId | GroupById %}
            <p style=""padding-bottom:25px;""> {{ group.Description }}</p>
        </div>
        <a class=""btn btn-primary btn-cneckin-select margin-b-sm text-center"" style=""position:absolute;bottom:25px;left:25%;width:50%;"">Register</a>    
    </div>
{% else %}
    <a class='btn btn-primary btn-checkin-select margin-b-sm text-center btn-block'>
        {{ Item.ItemName }} {% if Item.ShowCount %}<span class=""badge badge-{% if Item.ItemSpotsLeft > 3 %}success{% elseif Item.ItemSpotsLeft > 0 %}warning{% else %}danger{% endif %}""> {{ Item.ItemSpotsLeft }}</span>{% endif %}
    </a>
{% endif %}",
        Order = 7,
        Key = AttributeKey.PickerLavaTemplate)]

    [CodeEditorField("Baptism Type Label",
        Description = "This is the label for the Baptism Type selection",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = "<h4>Choose Baptism Event</h4>",
        Order = 8,
        Key = AttributeKey.BaptismTypeLabel)]

    [CodeEditorField("Campus Selection Label",
        Description = "This is the label for the campus selection",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = "<h4>Choose Campus</h4>",
        Order = 9,
        Key = AttributeKey.CampusSelectionLabel)]

    [CodeEditorField("Date Selection Label",
        Description = "This is the label for the Date selection",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = "<h4>Choose Date</h4>",
        Order = 10,
        Key = AttributeKey.DateSelectionLabel)]

    [CodeEditorField("Time Selection Label",
        Description = "This is the label for the Time selection",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = "<h4>Choose Time</h4>",
        Order = 11,
        Key = AttributeKey.TimeSelectionLabel)]

    [LinkedPage("Registration Page",
        Description = "This is the page that has a registration entry block on it",
        IsRequired = true,
        Order = 12,
        Key = AttributeKey.RegistrationPage)]

    [TextField("Hidden Baptism Event Group Ids",
        Description = "A comma seperated list of Baptism Event Group Ids to hide.",
        IsRequired = false,
        DefaultValue = "",
        Category = "Hidden Options",
        Order = 13,
        Key = AttributeKey.HiddenEventIds )]

    [TextField( "Hidden Campus Group Ids",
        Description = "A comma seperated list of Campus Group Ids to hide.",
        IsRequired = false,
        DefaultValue = "",
        Category = "Hidden Options",
        Order = 14,
        Key = AttributeKey.HiddenCampusIds )]

    [TextField( "Hidden Dates",
        Description = "A comma seperated list of Dates to hide. Dates should follow M/d/yyyy format (such as 1/25/2023 or 12/1/2023).",
        IsRequired = false,
        DefaultValue = "",
        Category = "Hidden Options",
        Order = 15,
        Key = AttributeKey.HiddenDates )]

    [TextField( "Hidden Times",
        Description = "A comma seperated list of Times to hide. Times should follow h:mmtt format (such as 11:00AM or 6:00PM).",
        IsRequired = false,
        DefaultValue = "",
        Category = "Hidden Options",
        Order = 16,
        Key = AttributeKey.HiddenTimes )]

    public partial class BaptismRegistrationSelection : Rock.Web.UI.RockBlock
    {
        #region AttributeKeys

        public class AttributeKey
        {
            public const string BaseBaptismGroup = "BaseBaptismGroup";
            public const string BaptismRegTemplate = "BaptismRegTemplate";
            public const string EnableQueryParameters = "EnableQueryParameters";
            public const string PickerLavaTemplate = "PickerLavaTemplate";
            public const string BaptismTypeLabel = "BaptismTypeLabel";
            public const string CampusSelectionLabel = "CampusSelectionLabel";
            public const string DateSelectionLabel = "DateSelectionLabel";
            public const string TimeSelectionLabel = "TimeSelectionLabel";
            public const string RegistrationPage = "RegistrationPage";
            public const string ShowEventTypeCount = "ShowEventTypeCount";
            public const string ShowCampusCount = "ShowCampusCount";
            public const string ShowDateCount = "ShowDateCount";
            public const string ShowTimeCount = "ShowTimeCount";
            public const string HiddenEventIds = "HiddenEventIds";
            public const string HiddenCampusIds = "HiddenCampusIds";
            public const string HiddenDates = "HiddenDates";
            public const string HiddenTimes = "HiddenTimes";
        }

        #endregion AttributeKeys

        #region Fields

        private int _baptismEventId
        {
            get { return ViewState["baptismEventId"] as int? ?? 0; }
            set { ViewState["baptismEventId"] = value; }
        }

        private int _campusGroupId
        {
            get { return ViewState["campusGroupId"] as int? ?? 0; }
            set { ViewState["campusGroupId"] = value; }
        }

        private string _date
        {
            get { return (ViewState["_date"] as string); }
            set { ViewState["_date"] = value; }
        }

        private List<int> _registrationIds
        {
            get { return (ViewState["_registrationIds"] as string).Split(',').AsIntegerList(); }
            set { ViewState["_registrationIds"] = string.Join(",", value); }
        }

        private List<int> _hiddenEventIds = new List<int>();
        private List<int> _hiddenCampusIds = new List<int>();
        private List<string> _hiddenDates = new List<string>();
        private List<string> _hiddenTimes = new List<string>();

        #endregion

        #region Properties

        #endregion

        #region Base ControlMethods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);


            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger(upnlContent);

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Add Hidden Event Ids to list
            foreach ( var eventIdString in GetAttributeValue( AttributeKey.HiddenEventIds ).Split( ',' ) )
            {
                if ( int.TryParse( eventIdString, out int eventId ) && eventId > 0 )
                {
                    _hiddenEventIds.Add( eventId );
                }
            }

            // Add Hidden Campus Ids to list
            foreach ( var campusIdString in GetAttributeValue( AttributeKey.HiddenCampusIds ).Split( ',' ) )
            {
                if ( int.TryParse( campusIdString, out int campusId ) && campusId > 0 )
                {
                    _hiddenCampusIds.Add( campusId );
                }
            }

            // Add Hidden Dates to list
            foreach ( var dateString in GetAttributeValue( AttributeKey.HiddenDates ).Split( ',' ) )
            {
                // Regex matches string that:
                //     (1[0-2]|[1-9]) - Starts with a number between 1 and 12
                //     / - Which is followed by a slash
                //     (3[01]|[12][0-9]|[1-9]) - Which is followed by a number between 1 and 31
                //     / - Which is followed by a slash
                //     (20[0-9]{2}) - Which is followed by a number between 2000 and 2099
                string pattern = @"(1[0-2]|[1-9])/(3[01]|[12][0-9]|[1-9])/(20[0-9]{2})";
                Match match = Regex.Match( dateString, pattern );
                if ( match.Success )
                {
                    _hiddenDates.Add( match.Value );
                }
            }

            // Add Hidden Times to list
            foreach ( var timeString in GetAttributeValue( AttributeKey.HiddenTimes ).Split( ',' ) )
            {
                // Regex matches string that:
                //     (1[0-2]|[1-9]) - Starts with a number between 1 and 12
                //     : - Which is followed by a colon
                //     ([0-5][0-9]) - Which is followed by a number between 00 and 59
                //     (AM|PM) - Which is followed by either AM or PM
                string pattern = @"(1[0-2]|[1-9]):([0-5][0-9])(AM|PM)";
                Match match = Regex.Match( timeString.ToUpper(), pattern );
                if ( match.Success )
                {
                    _hiddenTimes.Add( match.Value );
                }
            }

            if( Page.IsPostBack )
            {
                if (this.Request.Params["__EVENTTARGET"] == rBapTypeSelection.UniqueID)
                {
                    HandleRepeaterBapTypePostback(this.Request.Params["__EVENTARGUMENT"]);
                }
                else if(this.Request.Params["__EVENTTARGET"] == rCampusSelection.UniqueID)
                {
                    HandleRepeaterCampSelPostback(this.Request.Params["__EVENTARGUMENT"]);
                }
                else if (this.Request.Params["__EVENTTARGET"] == rDateSelection.UniqueID)
                {
                    HandleRepeaterDtSelPostback(this.Request.Params["__EVENTARGUMENT"]);
                }
                else if (this.Request.Params["__EVENTTARGET"] == rTimeSelection.UniqueID)
                {
                    HandleRepeaterTimeSelPostback(this.Request.Params["__EVENTARGUMENT"]);
                }
                else
                {
                    _baptismEventId = 0;
                    _campusGroupId = 0;
                    _date = null;
                    LoadData();
                }
            }
            else
            {
                _baptismEventId = 0;
                _campusGroupId = 0;
                _date = null;
                LoadData();

                // If Query Parameters are enabled
                if ( GetAttributeValue( AttributeKey.EnableQueryParameters ).AsBoolean() )
                {
                    // Set _baptismEventId to the value of the EventId query string if it is a valid ID
                    string eventIdParam = this.Request.QueryString["EventId"];
                    if ( int.TryParse( eventIdParam, out int eventId ) && eventId > 0 )
                    {
                        HandleRepeaterBapTypePostback( eventId.ToString() );
                    }

                    // Set _campusGroupId to the value of the CampusGroupId query string if it is a valid ID
                    string campusGroupIdParam = this.Request.QueryString["CampusGroupId"];
                    if ( int.TryParse( campusGroupIdParam, out int campusGroupId ) && campusGroupId > 0 )
                    {
                        HandleRepeaterCampSelPostback( campusGroupId.ToString() );
                    }

                    // Set _date to the value of the Date query string if it is in a valid layout
                    string dateParam = this.Request.QueryString["Date"];
                    if ( dateParam != null && dateParam != string.Empty )
                    {
                        // Regex matches string that:
                        //     (1[0-2]|[1-9]) - Starts with a number between 1 and 12
                        //     / - Which is followed by a slash
                        //     (3[01]|[12][0-9]|[1-9]) - Which is followed by a number between 1 and 31
                        //     / - Which is followed by a slash
                        //     (20[0-9]{2}) - Which is followed by a number between 2000 and 2099
                        string pattern = @"(1[0-2]|[1-9])/(3[01]|[12][0-9]|[1-9])/(20[0-9]{2})";
                        Match match = Regex.Match( dateParam, pattern );
                        if ( match.Success )
                        {
                            HandleRepeaterDtSelPostback( match.Value );
                        }
                    }

                    bbtnStart.Visible = false;
                }
            }
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false });
            mergeFields.Add("BaptismEventGroupId", _baptismEventId);
            mergeFields.Add("CampusGroupId", _campusGroupId);
            mergeFields.Add("Date", _date);

            lSelectBapType.Text = GetAttributeValue(AttributeKey.BaptismTypeLabel).ResolveMergeFields(mergeFields);
            lCampusSelection.Text = GetAttributeValue(AttributeKey.CampusSelectionLabel).ResolveMergeFields(mergeFields);
            lDateSelection.Text = GetAttributeValue(AttributeKey.DateSelectionLabel).ResolveMergeFields(mergeFields);
            lTimeSelection.Text = GetAttributeValue(AttributeKey.TimeSelectionLabel).ResolveMergeFields(mergeFields);
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            return base.SaveViewState();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated(object sender, EventArgs e)
        {
            LoadData();
        }

        #endregion

        #region Events

        protected void rBapTypeSelection_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if(e.Item == null)
            {
                return;
            }

            var item = e.Item.DataItem as Item;
            if(item == null)
            {
                return;
            }

            Panel pnlBapTyp = e.Item.FindControl("pnlSelectBapTypePostback") as Panel;
            string argument = item.ItemId;

            pnlBapTyp.Attributes["data-target"] = Page.ClientScript.GetPostBackEventReference(rBapTypeSelection, argument);
            pnlBapTyp.Attributes["data-loading-text"] = "Loading...";

            Literal lSelectBapTypeHtml = e.Item.FindControl("lSelectBapTypeHtml") as Literal;

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false });
            mergeFields.Add("Item", item);
            mergeFields.Add("Step", "Baptism Type");

            var tmplate = GetAttributeValue(AttributeKey.PickerLavaTemplate);

            lSelectBapTypeHtml.Text = tmplate.ResolveMergeFields(mergeFields);
        }

        protected void rCampusSelection_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item == null)
            {
                return;
            }

            var item = e.Item.DataItem as Item;
            if (item == null)
            {
                return;
            }

            Panel pnlCampus = e.Item.FindControl("pnlCampusSelectionPostback") as Panel;
            string argument = item.ItemId;

            pnlCampus.Attributes["data-target"] = Page.ClientScript.GetPostBackEventReference(rCampusSelection, argument);
            pnlCampus.Attributes["data-loading-text"] = "Loading...";

            Literal lCampusSelectionHtml = e.Item.FindControl("lCampusSelectionHtml") as Literal;

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false });
            mergeFields.Add("Item", item);
            mergeFields.Add("Step", "Campus Selection");

            var tmplate = GetAttributeValue(AttributeKey.PickerLavaTemplate);

            lCampusSelectionHtml.Text = tmplate.ResolveMergeFields(mergeFields);
        }

        protected void rDateSelection_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item == null)
            {
                return;
            }

            var item = e.Item.DataItem as Item;
            if (item == null)
            {
                return;
            }

            Panel pnlDate = e.Item.FindControl("pnlDateSelectionPostback") as Panel;
            string argument = item.ItemId;

            pnlDate.Attributes["data-target"] = Page.ClientScript.GetPostBackEventReference(rDateSelection, argument);
            pnlDate.Attributes["data-loading-text"] = "Loading...";

            Literal lDateSelectionHtml = e.Item.FindControl("lDateSelectionHtml") as Literal;

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false });
            mergeFields.Add("Item", item);
            mergeFields.Add("Step", "Date Selection");

            var tmplate = GetAttributeValue(AttributeKey.PickerLavaTemplate);

            lDateSelectionHtml.Text = tmplate.ResolveMergeFields(mergeFields);
        }

        protected void rTimeSelection_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item == null)
            {
                return;
            }

            var item = e.Item.DataItem as Item;
            if (item == null)
            {
                return;
            }

            Panel pnlTime = e.Item.FindControl("pnlTimeSelectionPostback") as Panel;
            string argument = string.Format("{0},{1}", item.ItemId, item.RegistrationId);

            pnlTime.Attributes["data-target"] = Page.ClientScript.GetPostBackEventReference(rTimeSelection, argument);
            pnlTime.Attributes["data-loading-text"] = "Loading...";

            Literal lTimeSelectionHtml = e.Item.FindControl("lTimeSelectionHtml") as Literal;

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false });
            mergeFields.Add("Item", item);
            mergeFields.Add("Step", "Time Selection");

            var tmplate = GetAttributeValue(AttributeKey.PickerLavaTemplate);

            lTimeSelectionHtml.Text = tmplate.ResolveMergeFields(mergeFields);
        }

        /// <summary>
        /// Handles the repeater postback.
        /// </summary>
        /// <param name="commandArgument">The command argument.</param>
        protected void HandleRepeaterBapTypePostback(string commandArgument)
        {
            _baptismEventId = commandArgument.AsInteger();
            pnlSelectBapType.Visible = false;
            pnlCampusSelection.Visible = true;
            ProcessNextSelection();
        }

        /// <summary>
        /// Handles the repeater postback.
        /// </summary>
        /// <param name="commandArgument">The command argument.</param>
        protected void HandleRepeaterCampSelPostback(string commandArgument)
        {
            _campusGroupId = commandArgument.AsInteger();
            pnlCampusSelection.Visible = false;
            pnlDateSelection.Visible = true;
            ProcessNextSelection();
        }

        /// <summary>
        /// Handles the repeater postback.
        /// </summary>
        /// <param name="commandArgument">The command argument.</param>
        protected void HandleRepeaterDtSelPostback(string commandArgument)
        {
            _date = commandArgument;
            pnlDateSelection.Visible = false;
            pnlTimeSelection.Visible = true;
            ProcessNextSelection();
        }

        /// <summary>
        /// Handles the repeater postback.
        /// </summary>
        /// <param name="commandArgument">The command argument.</param>
        protected void HandleRepeaterTimeSelPostback(string commandArgument)
        {
            var splitCommand = commandArgument.Split(',');
            var eventId = splitCommand[0];
            var regId = splitCommand[1];
            string slug = null;

            int id = regId.ToIntSafe();
            if (id != 0)
            {
                var groupMap = new EventItemOccurrenceGroupMapService(new RockContext()).Queryable().AsNoTracking().Where(m => m.RegistrationInstanceId == id).FirstOrDefault();
                slug = groupMap.UrlSlug;
            }

            var urlParams = new Dictionary<string, string>();
            urlParams.Add("EventOccurrenceId", eventId);
            urlParams.Add("RegistrationInstanceId", regId);
            urlParams.Add("Slug", slug);

            // If Query Parameters are enabled
            if ( GetAttributeValue( AttributeKey.EnableQueryParameters ).AsBoolean() )
            {
                // Add baptism query parameters so that the registration can pass them back to this block if the user wants to register another person for the same event
                string eventIdParam = this.Request.QueryString["EventId"];
                string campusGroupIdParam = this.Request.QueryString["CampusGroupId"];
                string dateParam = this.Request.QueryString["Date"];

                if ( eventIdParam.IsNotNullOrWhiteSpace() )
                {
                    urlParams.Add( "EventId", eventIdParam );
                }
                if ( campusGroupIdParam.IsNotNullOrWhiteSpace() )
                {
                    urlParams.Add( "CampusGroupId", campusGroupIdParam );
                }
                if ( dateParam.IsNotNullOrWhiteSpace() )
                {
                    urlParams.Add( "Date", dateParam );
                }
            }

            NavigateToLinkedPage(AttributeKey.RegistrationPage, urlParams);
        }

        #endregion

        #region Methods

        private void LoadData()
        {
            pnlSelectBapType.Visible = false;
            pnlDateSelection.Visible = false;
            pnlCampusSelection.Visible = false;
            pnlTimeSelection.Visible = false;
            bbtnStart.Visible = false;
            lWarning.Visible = false;

            var rockContext = new RockContext();
            var regInsService = new RegistrationInstanceService(rockContext);
            var groupService = new GroupService(rockContext);

            var regTmpGuid = GetAttributeValue(AttributeKey.BaptismRegTemplate).AsGuidOrNull();
            var baseGroupGuid = GetAttributeValue(AttributeKey.BaseBaptismGroup).AsGuidOrNull();

            if (regTmpGuid.HasValue && baseGroupGuid.HasValue)
            {
                var registrations = regInsService.Queryable()
                    .AsNoTracking()
                    .Where(i => i.IsActive && i.StartDateTime <= RockDateTime.Now && i.EndDateTime > RockDateTime.Now
                        && i.RegistrationTemplate.Guid == regTmpGuid.Value)
                    .ToList();

                _registrationIds = registrations.Select(i => i.Id).ToList();

                var group = groupService.Get(baseGroupGuid.Value);
                List<Item> items = new List<Item>();

                if(group.Groups.Count > 1)
                {
                    foreach(var g in group.Groups)
                    {
                        var childGroups = g.Groups.Select(c => c).ToList();

                        var regInstances = registrations
                            .Where(i => i.Linkages.FirstOrDefault() != null
                                && i.Linkages.FirstOrDefault().Group != null
                                && i.Linkages.FirstOrDefault().Group.ParentGroupId.HasValue
                                && childGroups.Select(c => c.Id).Contains(i.Linkages.FirstOrDefault().Group.ParentGroupId.Value))
                            .Select(i => new
                            {
                                i.MaxAttendees,
                                Registrants = i.Linkages.FirstOrDefault().Group.Members.Count()
                            })
                            .ToList();

                        var maxSpots = regInstances.Sum(i => i.MaxAttendees);
                        var reg = regInstances.Sum(i => i.Registrants);

                        if (maxSpots != 0 && _hiddenEventIds.Contains( g.Id ) == false)
                        {
                            items.Add(new Item
                            {
                                ItemId = g.Id.ToString(),
                                ItemName = g.Name,
                                ItemSpotsLeft = maxSpots.Value - reg,
                                IsAvailable = (maxSpots.Value - reg) > 0,
                                ShowCount = GetAttributeValue(AttributeKey.ShowEventTypeCount).AsBoolean()
                            });
                        }
                    }

                    rBapTypeSelection.DataSource = items;
                    rBapTypeSelection.DataBind();

                    if (items.Count == 0)
                    {
                        lWarning.Text = "There are no registrations active at this time";
                        lWarning.Visible = true;
                    }
                    else
                    {
                        pnlSelectBapType.Visible = true;
                    }
                }
                else if(group.Groups.Count == 1)
                {
                    _baptismEventId = group.Groups.First().Id;
                    ProcessNextSelection();
                }
            }
            else
            {
                lWarning.Text = "There is no registration template or base group selected in the block settings";
                lWarning.Visible = true;
            }
        }

        private void ProcessNextSelection()
        {
            if(_baptismEventId != 0)
            {
                bbtnStart.Visible = true;
                if (_campusGroupId != 0)
                {
                    if(_date != null && _date != string.Empty)
                    {
                        SetTimes();
                    }
                    else
                    {
                        SetDates();
                    }
                }
                else
                {
                    var setCampuses = SetCampuses();

                    if(setCampuses == false)
                    {
                        SetDates();
                    }
                }
            }
        }

        private void SetTimes()
        {
            var rockContext = new RockContext();
            var regInsService = new RegistrationInstanceService(rockContext);
            var groupService = new GroupService(rockContext);

            var group = groupService.Get(_campusGroupId);
            var instances = regInsService.GetByIds(_registrationIds).ToList();

            if (group.Groups.Count > 0)
            {
                List<Item> items = new List<Item>();

                var groupsToLoop = group.Groups.Where(g => g.Name.Contains(_date));

                foreach (var g in groupsToLoop)
                {
                    var regInstances = instances
                        .Where(i => i.Linkages.FirstOrDefault() != null
                            && i.Linkages.FirstOrDefault().Group != null
                            && i.Linkages.FirstOrDefault().GroupId == g.Id)
                        .Select(i => new
                        {
                            i.MaxAttendees,
                            Registrants = i.Linkages.FirstOrDefault().Group.Members.Count()
                        }).ToList();

                    var tiedInstance = instances.Where(i => i.Linkages.FirstOrDefault() != null
                            && i.Linkages.FirstOrDefault().Group != null
                            && i.Linkages.FirstOrDefault().GroupId == g.Id).FirstOrDefault();

                    var maxSpots = regInstances.Sum(i => i.MaxAttendees);
                    var reg = regInstances.Sum(i => i.Registrants);
                    var regId = tiedInstance != null ? tiedInstance.Id : 0;
                    var eventOccId = tiedInstance != null && tiedInstance.Linkages.FirstOrDefault() != null ?
                        tiedInstance.Linkages.FirstOrDefault().EventItemOccurrenceId : 0;

                    var time = g.Name.Split('-')[1].Trim();

                    var sameDate = items.Where(i => i.ItemName == time).FirstOrDefault();

                    if (sameDate != null)
                    {
                        sameDate.ItemSpotsLeft = (maxSpots.Value - reg) >= 0 ?
                            sameDate.ItemSpotsLeft + (maxSpots.Value - reg) :
                            sameDate.ItemSpotsLeft;

                        sameDate.IsAvailable = (maxSpots.Value - reg) >= 0 ? true : sameDate.IsAvailable;
                    }
                    else
                    {
                        if (maxSpots != 0 && _hiddenTimes.Contains( time ) == false)
                        {
                            items.Add(new Item
                            {
                                ItemId = eventOccId.ToString(),
                                ItemName = time,
                                ItemSpotsLeft = maxSpots.Value - reg,
                                IsAvailable = (maxSpots.Value - reg) > 0,
                                RegistrationId = regId,
                                ShowCount = GetAttributeValue(AttributeKey.ShowTimeCount).AsBoolean()
                            });
                        }
                    }
                }

                if (items.Count == 0)
                {
                    lWarning.Text = "There are no active registrations for this date";
                    lWarning.Visible = true;
                    pnlTimeSelection.Visible = false;
                }

                rTimeSelection.DataSource = items.OrderBy( i => i.ItemName.AsDateTime() );
                rTimeSelection.DataBind();
            }
        }

        private void SetDates()
        {
            pnlCampusSelection.Visible = false;
            pnlDateSelection.Visible = true;

            var rockContext = new RockContext();
            var regInsService = new RegistrationInstanceService(rockContext);
            var groupService = new GroupService(rockContext);

            var group = groupService.Get(_campusGroupId);
            var instances = regInsService.GetByIds(_registrationIds).ToList();

            if(group.Groups.Count > 0)
            {
                List<Item> items = new List<Item>();

                foreach(var g in group.Groups)
                {
                    var regInstances = instances
                        .Where(i => i.Linkages.FirstOrDefault() != null
                            && i.Linkages.FirstOrDefault().Group != null
                            && i.Linkages.FirstOrDefault().GroupId == g.Id)
                        .Select(i => new
                        {
                            i.MaxAttendees,
                            Registrants = i.Linkages.FirstOrDefault().Group.Members.Count()
                        }).ToList();

                    var maxSpots = regInstances.Sum(i => i.MaxAttendees);
                    var reg = regInstances.Sum(i => i.Registrants);

                    var groupDate = g.Name.Split('-').FirstOrDefault().Trim();

                    var sameDate = items.Where(i => i.ItemName == groupDate).FirstOrDefault();

                    if(sameDate != null)
                    {
                        sameDate.ItemSpotsLeft = (maxSpots.Value - reg) >= 0 ?
                            sameDate.ItemSpotsLeft + (maxSpots.Value - reg) :
                            sameDate.ItemSpotsLeft;

                        sameDate.IsAvailable = (maxSpots.Value - reg) >= 0 ? true : sameDate.IsAvailable;
                    }
                    else
                    {
                        if (maxSpots != 0 && _hiddenDates.Contains( groupDate ) == false)
                        {
                            items.Add(new Item
                            {
                                ItemId = groupDate,
                                ItemName = groupDate,
                                ItemSpotsLeft = maxSpots.Value - reg,
                                IsAvailable = (maxSpots.Value - reg) > 0,
                                ShowCount = GetAttributeValue(AttributeKey.ShowDateCount).AsBoolean()
                            });
                        }
                    }
                }

                if (items.Count == 0)
                {
                    lWarning.Text = "There are no active registrations for this campus";
                    lWarning.Visible = true;
                    pnlDateSelection.Visible = false;
                }

                rDateSelection.DataSource = items.OrderBy(i => i.ItemName.AsDateTime());
                rDateSelection.DataBind();

            }
        }

        private bool SetCampuses()
        {
            var set = false;

            var rockContext = new RockContext();
            var regInsService = new RegistrationInstanceService(rockContext);
            var groupService = new GroupService(rockContext);

            var group = groupService.Get(_baptismEventId);
            var instances = regInsService.GetByIds(_registrationIds).ToList();

            if(group.Groups.Count > 1)
            {
                set = true;
                List<Item> items = new List<Item>();
                
                foreach(var g in group.Groups)
                {
                    var regInstances = instances
                        .Where(i => i.Linkages.FirstOrDefault() != null
                            && i.Linkages.FirstOrDefault().Group != null
                            && i.Linkages.FirstOrDefault().Group.ParentGroupId.HasValue
                            && g.Id == i.Linkages.FirstOrDefault().Group.ParentGroupId.Value)
                        .Select(i => new
                        {
                            i.MaxAttendees,
                            Registrants = i.Linkages.FirstOrDefault().Group.Members.Count()
                        })
                        .ToList();

                    var maxSpots = regInstances.Sum(i => i.MaxAttendees);
                    var reg = regInstances.Sum(i => i.Registrants);

                    if (maxSpots != 0 && _hiddenCampusIds.Contains( g.Id ) == false)
                    {
                        items.Add(new Item
                        {
                            ItemId = g.Id.ToString(),
                            ItemName = g.Name,
                            ItemSpotsLeft = maxSpots.Value - reg,
                            IsAvailable = (maxSpots.Value - reg) > 0,
                            ShowCount = GetAttributeValue(AttributeKey.ShowCampusCount).AsBoolean()
                        });
                    }
                }

                if (items.Count == 0)
                {
                    lWarning.Text = "There are no active registrations for this baptism event";
                    lWarning.Visible = true;
                    pnlDateSelection.Visible = false;
                }

                rCampusSelection.DataSource = items.OrderBy(i => i.ItemName).ToList();
                rCampusSelection.DataBind();
            }
            else if(group.Groups.Count == 1)
            {
                _campusGroupId = group.Groups.First().Id;
            }
            else
            {
                set = true;
            }

            return set;
        }

        #endregion

        #region Helper Classes

        public class Item : LavaDataObject
        {
            public string ItemId { get; set; }
            public string ItemName { get; set; }
            public bool IsAvailable { get; set; }
            public int ItemSpotsLeft { get; set; }
            public int RegistrationId { get; set; }
            public bool ShowCount { get; set; }
        }

        #endregion

        protected void bbtnStart_Click(object sender, EventArgs e)
        {
            LoadData();

            // If Query Parameters are enabled
            if ( GetAttributeValue( AttributeKey.EnableQueryParameters ).AsBoolean() )
            {
                // Set _baptismEventId to the value of the EventId query string if it is a valid ID
                string eventIdParam = this.Request.QueryString["EventId"];
                if ( int.TryParse( eventIdParam, out int eventId ) && eventId > 0 )
                {
                    HandleRepeaterBapTypePostback( eventId.ToString() );
                }

                // Set _campusGroupId to the value of the CampusGroupId query string if it is a valid ID
                string campusGroupIdParam = this.Request.QueryString["CampusGroupId"];
                if ( int.TryParse( campusGroupIdParam, out int campusGroupId ) && campusGroupId > 0 )
                {
                    HandleRepeaterCampSelPostback( campusGroupId.ToString() );
                }

                // Set _date to the value of the Date query string if it is in a valid layout
                string dateParam = this.Request.QueryString["Date"];
                if ( dateParam != null && dateParam != string.Empty )
                {
                    // Regex matches string that:
                    //     (1[0-2]|[1-9]) - Starts with a number between 1 and 12
                    //     / - Which is followed by a slash
                    //     (3[01]|[12][0-9]|[1-9]) - Which is followed by a number between 1 and 31
                    //     / - Which is followed by a slash
                    //     (20[0-9]{2}) - Which is followed by a number between 2000 and 2099
                    string pattern = @"(1[0-2]|[1-9])/(3[01]|[12][0-9]|[1-9])/(20[0-9]{2})";
                    Match match = Regex.Match( dateParam, pattern );
                    if ( match.Success )
                    {
                        HandleRepeaterDtSelPostback( match.Value );
                    }
                }

                bbtnStart.Visible = false;
            }
        }
    }
}
