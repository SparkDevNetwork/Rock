// <copyright>
// Copyright by BEMA Information Services
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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Transactions;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_visitgracechurch.Connection
{
    /// <summary>
    /// Renders a particular calendar using Lava.
    /// </summary>
    [DisplayName("Connection Card")]
    [Category("BEMA Services > Connection")]
    [Description("Runs user through connection card entry")]

    [CampusField( "Online Campus", "The campus should be used for online option", true, "", "", 0 )]
    [GroupField("Check In Group", "Check In group configured to do attendance and find locations", true, "", "", 1)]
    [IntegerField("Max Results", "The max number of families shown on search", true, 10, "", 2)]
    [BooleanField("Prevent Inactive People", "Should we prevent inactive people from showing up?", true, "", 3)]
    [BooleanField("Show Address Option", "Should user be able to add Address for Family?", false, "", 4)]
    [DefinedValueField(Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Default Person Connection Status", "", true, false, "", "", 5)]
    [WorkflowTypeField("New Family Workflow Type", "Workflow to be activated for a new family", false, false, "", "", 6)]
    [WorkflowTypeField("New Person Workflow Type", "Workflow to be activated for a new person", false, false, "", "", 7)]
    [WorkflowTypeField("Check In Workflow", "Workflow to be launched on submit", false, false, "", "", 8)]
    [CodeEditorField( "Lava Template Families", "The lava template to use for the families", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, false, @"
<a class='btn btn-primary btn-block btn-checkin-select margin-b-sm text-left'>
    {% if Family.Group %}
        <i class='fa fa-home'></i> {{ Family.Caption }} Household{% if Family.SubCaption != '' %}: {{ Family.SubCaption }}{% endif %}
    {% else %}
        <i class='fa fa-plus'></i> {{ Family.Caption }}
    {% endif %}
</a>
", "", 9 )]
    [BooleanField("Display Final Steps","Show user be able to select optional checbox options for connection requests, prayer requests, etc.", true)]
    [TextField("Thanks Text","Override the default thanks text with this string.", false, "")]

    public partial class ConnectionCard : RockBlock
    {
        #region Properties

        private int? FamilyId
        {
            get { return ViewState["FamilyId"] as int? ?? 0; }
            set { ViewState["FamilyId"] = value; }
        }

        private string FamilyName
        {
            get { return ViewState["FamilyName"] as string ?? string.Empty; }
            set { ViewState["FamilyName"] = value; }
        }

        private int? CampusId
        {
            get { return ViewState["CampusId"] as int? ?? 0; }
            set { ViewState["CampusId"] = value; }
        }

        private string CampusName
        {
            get { return ViewState["CampusName"] as string ?? string.Empty; }
            set { ViewState["CampusName"] = value; }
        }

        private int? VenueId
        {
            get { return ViewState["VenueId"] as int? ?? 0; }
            set { ViewState["VenueId"] = value; }
        }

        private string VenueName
        {
            get { return ViewState["VenueName"] as string ?? string.Empty; }
            set { ViewState["VenueName"] = value; }
        }

        private int? ServiceId
        {
            get { return ViewState["ServiceId"] as int? ?? 0; }
            set { ViewState["ServiceId"] = value; }
        }

        private string ServiceName
        {
            get { return ViewState["ServiceName"] as string ?? string.Empty; }
            set { ViewState["ServiceName"] = value; }
        }

        private bool InPerson
        {
            get { return ViewState["InPerson"] as bool? ?? false; }
            set { ViewState["InPerson"] = value; }
        }

        private bool Comment
        {
            get { return ViewState["Comment"] as bool? ?? false; }
            set { ViewState["Comment"] = value; }
        }

        private bool Salvation
        {
            get { return ViewState["Salvation"] as bool? ?? false; }
            set { ViewState["Salvation"] = value; }
        }

        private bool Explore
        {
            get { return ViewState["Explore"] as bool? ?? false; }
            set { ViewState["Explore"] = value; }
        }

        private bool Discover
        {
            get { return ViewState["Discover"] as bool? ?? false; }
            set { ViewState["Discover"] = value; }
        }

        private bool Connect
        {
            get { return ViewState["Connect"] as bool? ?? false; }
            set { ViewState["Connect"] = value; }
        }

        //private bool WeeklyUpdate
        //{
        //    get { return ViewState["WeeklyUpdate"] as bool? ?? false; }
        //    set { ViewState["WeeklyUpdate"] = value; }
        //}

        private bool ContactMe
        {
            get { return ViewState["ContactMe"] as bool? ?? false; }
            set { ViewState["ContactMe"] = value; }
        }

        private bool Baptism
        {
            get { return ViewState["Baptism"] as bool? ?? false; }
            set { ViewState["Baptism"] = value; }
        }

        private bool NewToGrace
        {
            get { return ViewState["NewToGrace"] as bool? ?? false; }
            set { ViewState["NewToGrace"] = value; }
        }
		
        private bool ChangedAddress
        {
            get { return ViewState["ChangedAddress"] as bool? ?? false; }
            set { ViewState["ChangedAddress"] = value; }
        }

        /// <summary>
        /// A hash of EditFamilyState before any editing. Use this to determine if there were any changes
        /// </summary>
        /// <value>
        /// The initial state hash.
        /// </value>
        private int _initialEditFamilyStateHash
        {
            get
            {
                return ViewState["_initialEditFamilyStateHash"] as int? ?? 0;
            }
            set
            {
                ViewState["_initialEditFamilyStateHash"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the state of the edit family.
        /// </summary>
        /// <value>
        /// The state of the edit family.
        /// </value>
        public FamilyRegistrationState EditFamilyState { get; set; }

        /// <summary>
        /// The group type role adult identifier
        /// </summary>
        private static int _groupTypeRoleAdultId = GroupTypeCache.GetFamilyGroupType().Roles.FirstOrDefault(a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid()).Id;

        /// <summary>
        /// The index of the 'DeleteField' column in the grid for gFamilyMembers_RowDataBound
        /// </summary>
        private int _deleteFieldIndex;

        /// <summary>
        /// The person record status active identifier
        /// </summary>
        private static int _personRecordStatusActiveId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid()).Id;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gFamilyMembers.DataKeyNames = new string[] { "GroupMemberGuid" };
            gFamilyMembers.GridRebind += gFamilyMembers_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbSearchValidation.Visible = false;

            if ( Page.IsPostBack )
            {
                if (this.Request.Params["__EVENTTARGET"] == rSelection.UniqueID)
                {
                    HandleRepeaterPostback(this.Request.Params["__EVENTARGUMENT"]);
                }
            }
            else
            {
                if ( PageParameter( "Phone" ).AsDoubleOrNull().HasValue )
                {
                    pnbPhoneSearch.Number = PageParameter( "Phone" );
                }
                else if ( Request.Cookies["ConnectionCardPhoneNumber"] != null )
                {
                    pnbPhoneSearch.Number = Request.Cookies["ConnectionCardPhoneNumber"].Value;
                }

                if ( pnbPhoneSearch.Number.IsNotNullOrWhiteSpace() )
                {
                    FindFamilies();
                }
            }
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);

            EditFamilyState = (this.ViewState["EditFamilyState"] as string).FromJsonOrNull<FamilyRegistrationState>();
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            this.ViewState["EditFamilyState"] = EditFamilyState.ToJson();
            return base.SaveViewState();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            Search();
        }

        #endregion

        #region Methods

        #region Step 1 Find Families

        private void Search()
        {
            pnSearch.Visible = true;
            pnCheckingIn.Visible = false;

            pnbPhoneSearch.Number = "";
            rSelection.Visible = false;

            btnOnline.RemoveCssClass( "btn-select" );
            btnInPerson.RemoveCssClass( "btn-select" );

            FamilyId = 0;
            FamilyName = string.Empty;
            CampusId = -1;
            CampusName = string.Empty;
            ServiceId = -1;
            ServiceName = string.Empty;
            VenueId = -1;
            VenueName = string.Empty;
            if (EditFamilyState != null)
            {
                EditFamilyState.FamilyPersonListState.ForEach(p => p.IsSelected = false);
            }

            lFamily.Text = FamilyName;

            pnCheckingInMem.Visible = false;
            lCheckingIn.Text = "";

            pnCheckInType.Visible = false;
            lCheckInType.Text = "";

            pnFinalStep.Visible = false;
            pnSelectedData.Visible = false;
            pnService.Visible = false;
            pnVenue.Visible = false;
            pnNextCheckIn.Visible = false;
            pnCampus.Visible = false;
            pnFamilyMembers.Visible = false;
            pnlSelectFamily.Visible = false;
        }

        protected void btnGo_Click( object sender, EventArgs e )
        {
            FindFamilies();
        }

        private void FindFamilies() 
        {
            var families = new List<CheckInFamily>();

            var searchNum = pnbPhoneSearch.Number.AsNumeric();
            if (searchNum.Length > 6)
            {
                Response.Cookies["ConnectionCardPhoneNumber"].Value = pnbPhoneSearch.Number;
                Response.Cookies["ConnectionCardPhoneNumber"].Expires = RockDateTime.Now.AddYears( 1 );

                var rockContext = new RockContext();
                var personService = new PersonService(rockContext);
                var memberService = new GroupMemberService(rockContext);
                var groupService = new GroupService(rockContext);

                int personRecordTypeId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid()).Id;
                int familyGroupTypeId = GroupTypeCache.Get(Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid()).Id;
                var dvInactive = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid());

                var phoneQry = new PhoneNumberService(rockContext).Queryable().AsNoTracking()
                            .Where(o => o.Number.Contains(searchNum));

                var tmpQry = phoneQry.Join(personService.Queryable().AsNoTracking(),
                                o => new { PersonId = o.PersonId, IsDeceased = false, RecordTypeValueId = personRecordTypeId },
                                p => new { PersonId = p.Id, IsDeceased = p.IsDeceased, RecordTypeValueId = p.RecordTypeValueId.Value },
                                (pn, p) => new { Person = p, PhoneNumber = pn })
                                .Join(memberService.Queryable().AsNoTracking(),
                                pn => pn.Person.Id,
                                m => m.PersonId,
                                (o, m) => new { PersonNumber = o.PhoneNumber, GroupMember = m });

                var familyIdQry = groupService.Queryable().Where(g => tmpQry.Any(o => o.GroupMember.GroupId == g.Id) && g.GroupTypeId == familyGroupTypeId)
                    .Select(g => g.Id)
                    .Distinct();

                var max = GetAttributeValue("MaxResults").AsInteger();
                familyIdQry = familyIdQry.Take(max);

                // Load the family members
                var familyMembers = memberService
                    .Queryable().AsNoTracking()
                    .Where(m => m.Group.GroupTypeId == familyGroupTypeId && familyIdQry.Contains(m.GroupId)).Select(a =>
                   new
                   {
                       a.Group,
                       a.GroupId,
                       a.GroupRole.Order,
                       a.Person.BirthYear,
                       a.Person.BirthMonth,
                       a.Person.BirthDay,
                       a.Person.Gender,
                       a.Person.FirstName,
                       a.Person.RecordStatusValueId,
                       a.Person
                   })
                    .ToList();

                var preventInactive = GetAttributeValue("PreventInactivePeople").AsBoolean();

                // Add each family
                foreach (int familyId in familyIdQry)
                {
                    // Get each of the members for this family
                    var familyMemberQry = familyMembers
                        .Where(m =>
                           m.GroupId == familyId &&
                           m.FirstName != null);

                    if (preventInactive && dvInactive != null)
                    {
                        familyMemberQry = familyMemberQry
                            .Where(m =>
                               m.RecordStatusValueId != dvInactive.Id);
                    }

                    var thisFamilyMembers = familyMemberQry.ToList();

                    if (thisFamilyMembers.Any())
                    {
                        var group = thisFamilyMembers
                            .Select(m => m.Group)
                            .FirstOrDefault();

                        var firstNames = thisFamilyMembers
                            .OrderBy(m => m.Order)
                            .ThenBy(m => m.BirthYear)
                            .ThenBy(m => m.BirthMonth)
                            .ThenBy(m => m.BirthDay)
                            .ThenBy(m => m.Gender)
                            .Select(m => m.FirstName)
                            .ToList();

                        var family = new CheckInFamily();
                        family.Group = group.Clone(false);
                        family.Caption = group.ToString();
                        family.FirstNames = firstNames;
                        family.SubCaption = firstNames.AsDelimited(", ");
                        families.Add(family);
                    }
                }

                families = families.OrderBy(f => f.Caption)
                        .ThenBy(f => f.SubCaption)
                        .ToList();

                var newFamily = new CheckInFamily();
                newFamily.Group = null;
                newFamily.Caption = families.Any() ? "Not you? Enter new household" : "No match found! Enter new household";
                families.Add(newFamily);

                BindFamilyResults(families);

            }
            else
            {
                nbSearchValidation.Text = "Please enter at least 7 digits";
                nbSearchValidation.Visible = true;
            }
        }

        private void BindFamilyResults(List<CheckInFamily> families)
        {
            rSelection.DataSource = families;

            rSelection.DataBind();

            rSelection.Visible = true;
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rSelection control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rSelection_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item == null)
            {
                return;
            }

            var checkInFamily = e.Item.DataItem as CheckInFamily;
            if (checkInFamily == null)
            {
                return;
            }

            string argument = string.Empty;

            Panel pnlSelectFamilyPostback = e.Item.FindControl("pnlSelectFamilyPostback") as Panel;
            if (checkInFamily.Group != null)
            {
                argument = string.Format("{0},{1}", checkInFamily.Group.Id, checkInFamily.Caption);
            }
            else
            {
                argument = string.Format("-1,New Household");
            }
            pnlSelectFamilyPostback.Attributes["data-target"] = Page.ClientScript.GetPostBackEventReference(rSelection, argument);
            pnlSelectFamilyPostback.Attributes["data-loading-text"] = "Loading...";
            Literal lSelectFamilyButtonHtml = e.Item.FindControl("lSelectFamilyButtonHtml") as Literal;

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false });
            mergeFields.Add("Family", checkInFamily);

            var familySelectLavaTemplate = GetAttributeValue("LavaTemplateFamilies");

            pnlSelectFamily.Visible = true;
            lSelectFamilyButtonHtml.Text = familySelectLavaTemplate.ResolveMergeFields(mergeFields);
        }

        /// <summary>
        /// Handles the repeater postback.
        /// </summary>
        /// <param name="commandArgument">The command argument.</param>
        protected void HandleRepeaterPostback(string commandArgument)
        {
            var familyInfo = commandArgument.Split(',');

            if (familyInfo.Length == 2)
            {
                FamilyId = familyInfo[0].AsIntegerOrNull();
                FamilyName = familyInfo[1];

                CheckingInLocation();
            }
        }

        #endregion

        #region Step 2 Check In Location

        private void CheckingInLocation()
        {
            pnSearch.Visible = false;
            pnCheckingIn.Visible = true;

            btnOnline.RemoveCssClass( "btn-select" );
            btnInPerson.RemoveCssClass( "btn-select" );

            pnSelectedData.Visible = true;
            lFamily.Text = FamilyName;

            pnCheckInType.Visible = false;
            lCheckInType.Text = "";

            pnCheckingInMem.Visible = false;
            lCheckingIn.Text = "";

            CampusId = -1;
            CampusName = string.Empty;
            ServiceId = -1;
            ServiceName = string.Empty;
            VenueId = -1;
            VenueName = string.Empty;
            if (EditFamilyState != null)
            {
                EditFamilyState.FamilyPersonListState.ForEach(p => p.IsSelected = false);
            }

            pnFinalStep.Visible = false;
            pnService.Visible = false;
            pnVenue.Visible = false;
            pnNextCheckIn.Visible = false;
            pnCampus.Visible = false;
            pnFamilyMembers.Visible = false;
        }

        protected void btnOnline_Click(object sender, EventArgs e)
        {
            btnOnline.AddCssClass( "btn-select" );
            btnInPerson.RemoveCssClass( "btn-select" );

            CampusId = -1;
            CampusName = string.Empty;
            ServiceId = -1;
            ServiceName = string.Empty;
            VenueId = -1;
            VenueName = string.Empty;
            InPerson = false;

            pnCheckInType.Visible = true;
            lCheckInType.Text = GetCheckInLocationText();

            pnService.Visible = false;
            pnVenue.Visible = false;
            pnCampus.Visible = false;
            pnNextCheckIn.Visible = true;
        }

        protected void btnInPerson_Click(object sender, EventArgs e)
        {
            btnOnline.RemoveCssClass( "btn-select" );
            btnInPerson.AddCssClass( "btn-select" );

            CampusId = -1;
            CampusName = string.Empty;
            ServiceId = -1;
            ServiceName = string.Empty;
            VenueId = -1;
            VenueName = string.Empty;
            InPerson = true;

            pnCheckInType.Visible = true;
            lCheckInType.Text = GetCheckInLocationText();

            pnService.Visible = false;
            pnVenue.Visible = false;
            pnNextCheckIn.Visible = false;

            var rockContext = new RockContext();
            var campusService = new CampusService(rockContext);
            var group = GetCheckInGroup();

            if (group != null)
            {
                var tmp = group.GroupLocations.Select(g => g.Location.CampusId)
                                .Where(c => c.HasValue).Select(c => c.Value).ToList();

                rCampuses.DataSource = campusService.GetByIds( tmp ).ToList();
                rCampuses.DataBind();
            }

            pnCampus.Visible = true;
        }

        protected void rCampuses_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( e.CommandName == "SelectCampus" )
            {
                foreach ( RepeaterItem item in rCampuses.Items )
                {
                    var btn = item.FindControl( "lbCampus" ) as LinkButton;
                    if ( btn != null )
                    {
                        btn.RemoveCssClass( "btn-select" );
                    }
                }

                ServiceId = -1;
                ServiceName = string.Empty;
                VenueId = -1;
                VenueName = string.Empty;

                pnService.Visible = false;
                pnVenue.Visible = false;
                pnNextCheckIn.Visible = false;

                CampusId = e.CommandArgument.ToString().AsIntegerOrNull();
                if ( CampusId.HasValue )
                {
                    var lb = e.CommandSource as LinkButton;
                    if ( lb != null )
                    {
                        lb.AddCssClass( "btn-select" );
                        CampusName = lb.Text;
                    }

                    lCheckInType.Text = GetCheckInLocationText();

                    if ( GetVenues() == false )
                    {
                        if ( GetSchedules() == false )
                        {
                            pnNextCheckIn.Visible = true;
                        }
                    }
                }
            }
        }

        protected void rVenues_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( e.CommandName == "SelectVenue" )
            {
                foreach ( RepeaterItem item in rVenues.Items )
                {
                    var btn = item.FindControl( "lbVenue" ) as LinkButton;
                    if ( btn != null )
                    {
                        btn.RemoveCssClass( "btn-select" );
                    }
                }

                ServiceId = -1;
                ServiceName = string.Empty;

                VenueId = e.CommandArgument.ToString().AsIntegerOrNull();
                if ( VenueId.HasValue )
                {
                    var lb = e.CommandSource as LinkButton;
                    if ( lb != null )
                    {
                        lb.AddCssClass( "btn-select" );
                        VenueName = lb.Text;
                    }

                    lCheckInType.Text = GetCheckInLocationText();

                    if ( GetSchedules() == false )
                    {
                        pnNextCheckIn.Visible = true;
                    }
                }
            }
        }

        protected void rServices_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( e.CommandName == "SelectService" )
            {
                foreach ( RepeaterItem item in rServices.Items )
                {
                    var btn = item.FindControl( "lbService" ) as LinkButton;
                    if ( btn != null )
                    {
                        btn.RemoveCssClass( "btn-select" );
                    }
                }

                ServiceId = e.CommandArgument.ToString().AsIntegerOrNull();
                if ( ServiceId.HasValue )
                {
                    var lb = e.CommandSource as LinkButton;
                    if ( lb != null )
                    {
                        lb.AddCssClass( "btn-select" );
                        ServiceName = lb.Text;
                    }

                    lCheckInType.Text = GetCheckInLocationText();

                    pnNextCheckIn.Visible = true;
                }
            }
        }

        private bool GetSchedules()
        {
            var group = GetCheckInGroup();

            if (group != null)
            {
                var groupLocations = group.GroupLocations.Where(l => l.Location.CampusId == CampusId );

                var tmp = groupLocations.SelectMany(gl => gl.Schedules).Where( s => s.IsActive == true ).Distinct();

                if(tmp.Count() > 0)
                {
                    rServices.DataSource = tmp;
                    rServices.DataBind();

                    pnService.Visible = true;
                    return true;
                }
            }

            return false;
        }

        private bool GetVenues()
        {
			//Sheila requested that we turn off the Auditorium/Venue option for the South Campus for right now, so 
			//temporarily just returning false always here
			return false;
			
            var group = GetCheckInGroup();
            var campusLocationType = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.LOCATION_TYPE_CAMPUS.AsGuid());
            if (group != null && campusLocationType != null)
            {
                var venues = group.GroupLocations.Select( l => l.Location )
                    .Where( l => l.CampusId == CampusId && l.LocationTypeValueId != campusLocationType.Id )
                    .ToList();

                if (venues.Count() > 0)
                {
                    rVenues.DataSource = venues;
                    rVenues.DataBind();

                    pnVenue.Visible = true;

                    return true;
                }
            }

            return false;
        }

        protected void btnNextCheckIn_Click(object sender, EventArgs e)
        {
            if (pnCheckingInMem.Visible)
            {
                pnCheckingIn.Visible = false;
                pnFamilyMembers.Visible = true;
            }
            else
            {
                FamilyMembers();
            }
        }

        private string GetCheckInLocationText()
        {
            string res = string.Empty;

            res += InPerson ? "In Person" : "Online";
            res += CampusId > 0 ? ", " + CampusName : string.Empty;
            res += VenueId > 0 ? ", " + VenueName : string.Empty;
            res += ServiceId > 0 ? ", " + ServiceName : string.Empty;

            return res;
        }

        #endregion

        #region Step 3 Members Checking In

        private void FamilyMembers()
        {
            pnFinalStep.Visible = false;
            pnFamilyMembers.Visible = true;
            pnCheckingIn.Visible = false;
            if (EditFamilyState != null)
            {
                EditFamilyState.FamilyPersonListState.ForEach(p => p.IsSelected = false);
            }

            pnCheckingInMem.Visible = false;
            lCheckingIn.Text = "";

            rSelectionMembers.DataSource = LoadMembers();
            rSelectionMembers.DataBind();
        }

        private List<Person> LoadMembers()
        {
            var members = new List<Person>();

            if (FamilyId.HasValue && FamilyId.Value > 0)
            {
                var rockContext = new RockContext();
                var groupService = new GroupService(rockContext);

                var famGroup = groupService.GetByIds(new List<int> { FamilyId.Value }).First();

                this.EditFamilyState = FamilyRegistrationState.FromGroup(famGroup);

                int groupId = EditFamilyState.GroupId.Value;
                var groupMemberService = new GroupMemberService(rockContext);
                var groupMembersQuery = groupMemberService.Queryable(false)
                    .Include(a => a.Person)
                    .Where(a => a.GroupId == groupId)
                    .OrderBy(m => m.GroupRole.Order)
                    .ThenBy(m => m.Person.BirthYear)
                    .ThenBy(m => m.Person.BirthMonth)
                    .ThenBy(m => m.Person.BirthDay)
                    .ThenBy(m => m.Person.Gender);

                var groupMemberList = groupMembersQuery.ToList();

                foreach (var groupMember in groupMemberList)
                {
                    var familyPersonState = FamilyRegistrationState.FamilyPersonState.FromPerson(groupMember.Person, 0, true);
                    familyPersonState.GroupMemberGuid = groupMember.Guid;
                    familyPersonState.GroupId = groupMember.GroupId;
                    familyPersonState.IsAdult = groupMember.GroupRoleId == _groupTypeRoleAdultId;
                    this.EditFamilyState.FamilyPersonListState.Add(familyPersonState);
                    members.Add(groupMember.Person);
                }
            }

            return members;
        }

        protected void rSelectionMembers_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int id = e.CommandArgument.ToString().AsInteger();
            var btn = e.CommandSource as BootstrapButton;
            var person = EditFamilyState.GetPersonFromId(id);
            var primaryPersonId = EditFamilyState.PrimaryPersonId;

            if ( btn != null && person != null )
            {
                if ( person.IsSelected )
                {
                    person.IsSelected = false;
                    if ( primaryPersonId.HasValue && primaryPersonId.Value == person.PersonId )
                    {
                        var primaryPerson = EditFamilyState.FamilyPersonListState.FirstOrDefault( p => p.IsSelected );
                        EditFamilyState.PrimaryPersonId = primaryPerson != null ? primaryPerson.PersonId : (int?)null;
                    }
                }
                else
                {
                    if ( !EditFamilyState.FamilyPersonListState.Any( p => p.IsSelected ) )
                    {
                        EditFamilyState.PrimaryPersonId = person.PersonId;
                    }
                    person.IsSelected = true;
                }
            }

            // Set state
            foreach( RepeaterItem item in rSelectionMembers.Items )
            {
                var itemBtn = item.FindControl( "lbSelect" ) as BootstrapButton;
                var lSquare = item.FindControl( "lSquare" ) as Literal;
                var lSquareCheck = item.FindControl( "lSquareCheck" ) as Literal;
                var lblMe = item.FindControl( "lblMe" ) as Label;

                if ( itemBtn != null && lSquare != null && lSquareCheck != null && lblMe != null )
                {
                    int personId = itemBtn.CommandArgument.AsInteger();
                    var itemPerson = EditFamilyState.GetPersonFromId( personId );
                    lSquare.Visible = !itemPerson.IsSelected;
                    lSquareCheck.Visible = itemPerson.IsSelected;
                    if ( itemPerson.IsSelected )
                    {
                        itemBtn.AddCssClass( "btn-select" );
                    }
                    else
                    {
                        itemBtn.RemoveCssClass( "btn-select" );
                    }
                    lblMe.Visible = EditFamilyState.PrimaryPersonId.HasValue && EditFamilyState.PrimaryPersonId.Value == personId;
                }
            }

            var selectedPeople = EditFamilyState.FamilyPersonListState.Where(p => p.IsSelected);

            lCheckingIn.Text = selectedPeople.Count() > 0 ? GetCheckingInText(selectedPeople) : string.Empty;

            btnNextMembers.Visible = pnCheckingInMem.Visible = lCheckingIn.Text != string.Empty;
        }

        private string GetCheckingInText(IEnumerable<FamilyRegistrationState.FamilyPersonState> selectedPeople)
        {
            return string.Format("Checking In: {0}", string.Join(", ", selectedPeople.Select(p => p.FirstName)));
        }

        protected void lbEditFamily_Click(object sender, EventArgs e)
        {
            CheckInFamily family = null;

            if (FamilyId.Value > 0)
            {
                var rockContext = new RockContext();
                var groupService = new GroupService(rockContext);

                var famGroup = groupService.GetByIds(new List<int> { FamilyId.Value }).First();

                family = new CheckInFamily
                {
                    Group = famGroup
                };
            }

            ShowFamilyDetail(family);
        }

        protected void btnBackMembers_Click(object sender, EventArgs e)
        {
            pnFamilyMembers.Visible = false;
            pnCheckingIn.Visible = true;
        }

        #endregion

        #region Step 4 Final Step

        private void FinalStep()
        {
            pnFamilyMembers.Visible = false;

            if( GetAttributeValue("DisplayFinalSteps").AsBoolean() )
            {
                pnFinalStep.Visible = true;
            }
            else
            {
                //Skip this screen and submit
                btnSubmit_Click( null, null );
            }
        }

        protected void btnSubmit_Click( object sender, EventArgs e )
        {

            if ( Comment && tbComment.Text.IsNullOrWhiteSpace() )
            {
                pnSubmitWarning.Visible = true;
                return;
            }
			
			if ( ChangedAddress && (tbChangeAddress.Street1.IsNullOrWhiteSpace() || tbChangeAddress.City.IsNullOrWhiteSpace() || tbChangeAddress.State.IsNullOrWhiteSpace() || tbChangeAddress.PostalCode.IsNullOrWhiteSpace()) ) {
				pnAddressWarning.Visible = true;
				return;
			}

            if ( !CampusId.HasValue || CampusId.Value <= 0 )
            {
                var campusGuid = GetAttributeValue( "OnlineCampus" ).AsGuidOrNull();
                if ( campusGuid.HasValue )
                {
                    var onlineCampus = CampusCache.Get( campusGuid.Value );
                    if ( onlineCampus != null )
                    {
                        CampusId = onlineCampus.Id;
                    }
                }
            }

            foreach (var per in EditFamilyState.FamilyPersonListState.Where(p => p.IsSelected))
            {
                AddAttendance(per);
            }

            if ( EditFamilyState.PrimaryPersonId.HasValue )
            {
                WorkflowQueue( EditFamilyState.PrimaryPersonId.Value );
            }

            pnSelectedData.Visible = false;
            pnFinalStep.Visible = false;

            if( GetAttributeValue( "ThanksText" ).IsNotNullOrWhiteSpace() )
            {
                literalThanks.Text = GetAttributeValue( "ThanksText" );
            }
            pnThanks.Visible = true;
        }

        private void AddAttendance(FamilyRegistrationState.FamilyPersonState per)
        {
            int? locId = null;
            int? schId = null;
            int? cmpId = null;
            var group = GetCheckInGroup();

            if ( VenueId.HasValue && VenueId.Value > 0 )
            {
                var loc = group.GroupLocations.Select( l => l.Location ).Where( l => l.Id == VenueId ).FirstOrDefault();
                if ( loc != null )
                {
                    locId = loc.Id;
                }
            }
            else if ( CampusId.HasValue && CampusId.Value > 0 )
            {
                var campusLocationType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.LOCATION_TYPE_CAMPUS.AsGuid() );
                var loc = group.GroupLocations.Select( l => l.Location )
                    .Where( l => l.CampusId == CampusId && l.LocationTypeValueId == campusLocationType.Id ).FirstOrDefault();
                if ( loc != null )
                {
                    locId = loc.Id;
                }
            }
            else
            {

            }

            if (ServiceId.HasValue && ServiceId.Value > 0)
            {
                var sch = group.GroupLocations.SelectMany(s => s.Schedules).Where(s => s.Id == ServiceId).First();
                if(sch != null)
                {
                    schId = sch.Id;
                }
            }

            if(CampusId > 0)
            {
                cmpId = CampusId;
            }

            var rockContext = new RockContext();
            new AttendanceService(rockContext).AddOrUpdate(per.PersonAliasId.Value, RockDateTime.Now, group.Id, locId, schId, cmpId);
            rockContext.SaveChanges();
        }

        private void WorkflowQueue(int per)
        {
            LaunchWorkflowTransaction workflowTransaction = new LaunchWorkflowTransaction<Person>(GetAttributeValue("CheckInWorkflow").AsGuid(), per);

            if ( CampusId.HasValue )
            {
                var campus = CampusCache.Get( CampusId.Value );
                workflowTransaction.WorkflowAttributeValues.Add( "Campus", campus.Guid.ToString() );
            }

            //All attributes to add
            workflowTransaction.WorkflowAttributeValues.Add("FindOutAboutBaptism", Baptism.ToString());
            workflowTransaction.WorkflowAttributeValues.Add("SalvationToday", Salvation.ToString());
            workflowTransaction.WorkflowAttributeValues.Add("FindOutAboutExplore", Explore.ToString());
            workflowTransaction.WorkflowAttributeValues.Add("DiscoverWhereToServe", Discover.ToString());
            workflowTransaction.WorkflowAttributeValues.Add("ConnectWithOthers", Connect.ToString());
            //workflowTransaction.WorkflowAttributeValues.Add("ReceiveWeeklyUpdate", WeeklyUpdate.ToString());
            workflowTransaction.WorkflowAttributeValues.Add("GraceStaffContact", ContactMe.ToString());
            workflowTransaction.WorkflowAttributeValues.Add("NewToGrace", NewToGrace.ToString());
			workflowTransaction.WorkflowAttributeValues.Add("ChangedAddress", ChangedAddress.ToString());
            if(ContactMe)
            {
                workflowTransaction.WorkflowAttributeValues.Add("GraceStaffRegarding", tbContactRegarding.Text);
            }
            if(Comment)
            {
                var sel = rblComment.SelectedValue.AsInteger();
                var key = sel == 0 ? "Comment" :
                            sel == 1 ? "PrayerRequest" : "PraiseReport";
                workflowTransaction.WorkflowAttributeValues.Add(key, tbComment.Text);
                workflowTransaction.WorkflowAttributeValues.Add( "IsPublicRequest", cbKeepPrivate.Checked ? "False" : "True" );  //reverse boolean
            }
			if(ChangedAddress) {
				 workflowTransaction.WorkflowAttributeValues.Add("AddressStreet", tbChangeAddress.Street1);
				 workflowTransaction.WorkflowAttributeValues.Add("AddressCity", tbChangeAddress.City);
				 workflowTransaction.WorkflowAttributeValues.Add("AddressState", tbChangeAddress.State);
				 workflowTransaction.WorkflowAttributeValues.Add("AddressZip", tbChangeAddress.PostalCode);
			}

            workflowTransaction.Enqueue();
        }

        protected void btnBackFinal_Click(object sender, EventArgs e)
        {
            pnFamilyMembers.Visible = true;
            pnFinalStep.Visible = false;
        }

        protected void btnComment_Click(object sender, EventArgs e)
        {
            if(Comment)
            {
                Comment = false;
                pnComment.Visible = false;
                btnComment.RemoveCssClass("btn-select");
                btnComment.Text = btnComment.Text.Replace( "fa-check-square-o", "fa-square-o" );
            }
            else
            {
                Comment = true;
                pnComment.Visible = true;
                btnComment.AddCssClass("btn-select");
                btnComment.Text = btnComment.Text.Replace( "fa-square-o", "fa-check-square-o" );
            }
        }

        protected void btnSalvation_Click(object sender, EventArgs e)
        {
            if (Salvation)
            {
                Salvation = false;
                btnSalvation.RemoveCssClass("btn-select");
                btnSalvation.Text = btnSalvation.Text.Replace( "fa-check-square-o", "fa-square-o" );
            }
            else
            {
                Salvation = true;
                btnSalvation.AddCssClass("btn-select");
                btnSalvation.Text = btnSalvation.Text.Replace( "fa-square-o", "fa-check-square-o" );
            }
        }

        protected void btnExplore_Click(object sender, EventArgs e)
        {
            if (Explore)
            {
                Explore = false;
                btnExplore.RemoveCssClass("btn-select");
                btnExplore.Text = btnExplore.Text.Replace( "fa-check-square-o", "fa-square-o" );
            }
            else
            {
                Explore = true;
                btnExplore.AddCssClass("btn-select");
                btnExplore.Text = btnExplore.Text.Replace( "fa-square-o", "fa-check-square-o" );
            }
        }

        protected void btnServe_Click(object sender, EventArgs e)
        {
            if (Discover)
            {
                Discover = false;
                btnServe.RemoveCssClass("btn-select");
                btnServe.Text = btnServe.Text.Replace( "fa-check-square-o", "fa-square-o" );
            }
            else
            {
                Discover = true;
                btnServe.AddCssClass("btn-select");
                btnServe.Text = btnServe.Text.Replace( "fa-square-o", "fa-check-square-o" );
            }
        }

        protected void btnConnect_Click(object sender, EventArgs e)
        {
            if (Connect)
            {
                Connect = false;
                btnConnect.RemoveCssClass("btn-select");
                btnConnect.Text = btnConnect.Text.Replace( "fa-check-square-o", "fa-square-o" );
            }
            else
            {
                Connect = true;
                btnConnect.AddCssClass("btn-select");
                btnConnect.Text = btnConnect.Text.Replace( "fa-square-o", "fa-check-square-o" );
            }
        }

        //protected void btnUpdate_Click(object sender, EventArgs e)
        //{
        //    if (WeeklyUpdate)
        //    {
        //        WeeklyUpdate = false;
        //        btnUpdate.RemoveCssClass("btn-select");
        //        btnUpdate.Text = btnUpdate.Text.Replace( "fa-check-square-o", "fa-square-o" );
        //    }
        //    else
        //    {
        //        WeeklyUpdate = true;
        //        btnUpdate.AddCssClass("btn-select");
        //        btnUpdate.Text = btnUpdate.Text.Replace( "fa-square-o", "fa-check-square-o" );
        //    }
        //}

        protected void btnContact_Click(object sender, EventArgs e)
        {
            if (ContactMe)
            {
                ContactMe = false;
                pnContactRegarding.Visible = false;
                btnContact.RemoveCssClass("btn-select");
                btnContact.Text = btnContact.Text.Replace( "fa-check-square-o", "fa-square-o" );
            }
            else
            {
                ContactMe = true;
                pnContactRegarding.Visible = true;
                btnContact.AddCssClass("btn-select");
                btnContact.Text = btnContact.Text.Replace( "fa-square-o", "fa-check-square-o" );
            }
        }

        protected void btnBaptism_Click(object sender, EventArgs e)
        {
            if (Baptism)
            {
                Baptism = false;
                btnBaptism.RemoveCssClass("btn-select");
                btnBaptism.Text = btnBaptism.Text.Replace( "fa-check-square-o", "fa-square-o" );
            }
            else
            {
                Baptism = true;
                btnBaptism.AddCssClass("btn-select");
                btnBaptism.Text = btnBaptism.Text.Replace( "fa-square-o", "fa-check-square-o" );
            }
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            if (NewToGrace)
            {
                NewToGrace = false;
                btnNew.RemoveCssClass("btn-select");
                btnNew.Text = btnNew.Text.Replace( "fa-check-square-o", "fa-square-o" );
            }
            else
            {
                NewToGrace = true;
                btnNew.AddCssClass("btn-select");
                btnNew.Text = btnNew.Text.Replace( "fa-square-o", "fa-check-square-o" );
            }
        }
		
        protected void btnChangedAddress_Click(object sender, EventArgs e)
        {
            if (ChangedAddress)
            {
                ChangedAddress = false;
				pnChangeAddress.Visible = false;
                btnAddress.RemoveCssClass("btn-select");
                btnAddress.Text = btnAddress.Text.Replace( "fa-check-square-o", "fa-square-o" );
            }
            else
            {
                ChangedAddress = true;
				pnChangeAddress.Visible = true;
                btnAddress.AddCssClass("btn-select");
                btnAddress.Text = btnAddress.Text.Replace( "fa-square-o", "fa-check-square-o" );
				//lbEditFamily_Click(sender, e);
				//EditFamilyMember_Click(sender, e);
				//EditGroupMember(??);
				//private void EditGroupMember(Guid? groupMemberGuid
            }
        }

        #endregion

        private Group GetCheckInGroup()
        {
            var rockContext = new RockContext();
            return new GroupService(rockContext).GetByGuid(GetAttributeValue("CheckInGroup").AsGuid());
        }

        protected void btnRemoveFamily_Click(object sender, EventArgs e)
        {
            Search();
        }

        protected void btnRemoveCheckInType_Click(object sender, EventArgs e)
        {
            CheckingInLocation();
        }

        protected void btnRemoveMem_Click(object sender, EventArgs e)
        {
            FamilyMembers();
        }

        protected void btnNextMembers_Click(object sender, EventArgs e)
        {
            FinalStep();
        }

        #region Edit/Add Family

        /// <summary>
        /// Shows edit UI fo the family (or null adding a new family)
        /// </summary>
        /// <param name="checkInFamily">The check in family.</param>
        private void ShowFamilyDetail(CheckInFamily checkInFamily)
        {
            if (checkInFamily != null && checkInFamily.Group != null)
            {

                this.EditFamilyState = FamilyRegistrationState.FromGroup(checkInFamily.Group);
                hfGroupId.Value = checkInFamily.Group.Id.ToString();
                mdEditFamily.Title = checkInFamily.Group.Name;

                int groupId = hfGroupId.Value.AsInteger();
                var rockContext = new RockContext();
                var groupMemberService = new GroupMemberService(rockContext);
                var groupMembersQuery = groupMemberService.Queryable(false)
                    .Include(a => a.Person)
                    .Where(a => a.GroupId == groupId)
                    .OrderBy(m => m.GroupRole.Order)
                    .ThenBy(m => m.Person.BirthYear)
                    .ThenBy(m => m.Person.BirthMonth)
                    .ThenBy(m => m.Person.BirthDay)
                    .ThenBy(m => m.Person.Gender);

                var groupMemberList = groupMembersQuery.ToList();

                foreach (var groupMember in groupMemberList)
                {
                    var familyPersonState = FamilyRegistrationState.FamilyPersonState.FromPerson(groupMember.Person, 0, true);
                    familyPersonState.GroupMemberGuid = groupMember.Guid;
                    familyPersonState.GroupId = groupMember.GroupId;
                    familyPersonState.IsAdult = groupMember.GroupRoleId == _groupTypeRoleAdultId;
                    this.EditFamilyState.FamilyPersonListState.Add(familyPersonState);
                }

                BindFamilyMembersGrid();

                ShowFamilyView();
            }
            else
            {
                this.EditFamilyState = FamilyRegistrationState.FromGroup(new Group() { GroupTypeId = GroupTypeCache.GetFamilyGroupType().Id });
                hfGroupId.Value = "0";
                mdEditFamily.Title = "Add Family";
                EditGroupMember(null);
            }

            _initialEditFamilyStateHash = this.EditFamilyState.GetStateHash();

            // disable any idle redirect blocks that are on the page when the mdEditFamily modal is open
            DisableIdleRedirectBlocks(true);

            upContent.Update();
            mdEditFamily.Show();
        }

        /// <summary>
        /// Binds the family members grid.
        /// </summary>
        private void BindFamilyMembersGrid()
        {
            var deleteField = gFamilyMembers.ColumnsOfType<DeleteField>().FirstOrDefault();
            _deleteFieldIndex = gFamilyMembers.Columns.IndexOf(deleteField);

            gFamilyMembers.DataSource = this.EditFamilyState.FamilyPersonListState.Where(a => a.IsDeleted == false).ToList();
            gFamilyMembers.DataBind();
        }

        /// <summary>
        /// Edits the group member.
        /// </summary>
        /// <param name="groupMemberId">The group member identifier.</param>
        private void EditGroupMember(Guid? groupMemberGuid)
        {
            var rockContext = new RockContext();

            FamilyRegistrationState.FamilyPersonState familyPersonState = null;

            if (groupMemberGuid.HasValue)
            {
                familyPersonState = EditFamilyState.FamilyPersonListState.FirstOrDefault(a => a.GroupMemberGuid == groupMemberGuid);
            }

            if (familyPersonState == null)
            {
                // create a new temp record so we can set the defaults for the new person
                familyPersonState = FamilyRegistrationState.FamilyPersonState.FromTemporaryPerson();
                familyPersonState.GroupMemberGuid = Guid.NewGuid();

                // default Gender to Unknown so that it'll prompt to select gender if it hasn't been selected yet
                familyPersonState.Gender = Gender.Unknown;
                familyPersonState.IsAdult = false;
                familyPersonState.IsMarried = false;
                familyPersonState.RecordStatusValueId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid()).Id;
                familyPersonState.ConnectionStatusValueId = GetAttributeValue("DefaultPersonConnectionStatus").AsIntegerOrNull();

                var firstFamilyMember = EditFamilyState.FamilyPersonListState.FirstOrDefault();
                if (firstFamilyMember != null)
                {
                    // if this family already has a person, default the LastName to the first person
                    familyPersonState.LastName = firstFamilyMember.LastName;
                }
            }

            hfGroupMemberGuid.Value = familyPersonState.GroupMemberGuid.ToString();
            tglAdultChild.Checked = familyPersonState.IsAdult;

            // only allow Adult/Child and Relationship to be changed for newly added people
            tglAdultChild.Visible = !familyPersonState.PersonId.HasValue;

            ShowControlsForRole(tglAdultChild.Checked);
            if (familyPersonState.Gender == Gender.Unknown)
            {
                bgGender.SelectedValue = null;
            }
            else
            {
                bgGender.SetValue(familyPersonState.Gender.ConvertToInt());
            }
            tglAdultMaritalStatus.Checked = familyPersonState.IsMarried;

            // Only show the RecordStatus if they aren't currently active
            dvpRecordStatus.Visible = false;
            if (familyPersonState.PersonId.HasValue)
            {
                var personRecordStatusValueId = new PersonService(rockContext).GetSelect(familyPersonState.PersonId.Value, a => a.RecordStatusValueId);
                if (personRecordStatusValueId.HasValue)
                {
                    dvpRecordStatus.Visible = personRecordStatusValueId != _personRecordStatusActiveId;
                }
            }

            tbFirstName.Focus();
            tbFirstName.Text = familyPersonState.FirstName;
            tbLastName.Text = familyPersonState.LastName;
            acHomeAddress.Street1 = EditFamilyState.HomeLocation != null ? EditFamilyState.HomeLocation.Location.Street1 : string.Empty;
            acHomeAddress.Street2 = EditFamilyState.HomeLocation != null ? EditFamilyState.HomeLocation.Location.Street2 : string.Empty;
            acHomeAddress.City = EditFamilyState.HomeLocation != null ? EditFamilyState.HomeLocation.Location.City : string.Empty;
            acHomeAddress.State = EditFamilyState.HomeLocation != null ? EditFamilyState.HomeLocation.Location.State : string.Empty;
            acHomeAddress.PostalCode = EditFamilyState.HomeLocation != null ? EditFamilyState.HomeLocation.Location.PostalCode : string.Empty;

            dvpSuffix.DefinedTypeId = DefinedTypeCache.Get(Rock.SystemGuid.DefinedType.PERSON_SUFFIX.AsGuid()).Id;
            dvpSuffix.SetValue(familyPersonState.SuffixValueId);

            dvpRecordStatus.DefinedTypeId = DefinedTypeCache.Get(Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS.AsGuid()).Id;
            dvpRecordStatus.SetValue(familyPersonState.RecordStatusValueId);
            hfConnectionStatus.Value = familyPersonState.ConnectionStatusValueId.ToString();

            var mobilePhoneNumber = familyPersonState.MobilePhoneNumber;
            if (mobilePhoneNumber != null)
            {
                pnMobilePhone.CountryCode = familyPersonState.MobilePhoneCountryCode;
                pnMobilePhone.Number = mobilePhoneNumber;
            }
            else
            {
                pnMobilePhone.CountryCode = string.Empty;
                pnMobilePhone.Number = string.Empty;
            }

            tbEmail.Text = familyPersonState.Email;
            dpBirthDate.SelectedDate = familyPersonState.BirthDate;
            if (familyPersonState.GradeOffset.HasValue)
            {
                gpGradePicker.SetValue(familyPersonState.GradeOffset);
            }
            else
            {
                gpGradePicker.SelectedValue = null;
            }

            ShowPersonView(familyPersonState);
        }

        /// <summary>
        /// Shows the controls for role.
        /// </summary>
        /// <param name="isAdult">if set to <c>true</c> [is adult].</param>
        private void ShowControlsForRole(bool isAdult)
        {

            tglAdultMaritalStatus.Visible = isAdult;
            dpBirthDate.Visible = !isAdult;
            gpGradePicker.Visible = !isAdult;
            tbEmail.Visible = isAdult;
            acHomeAddress.Visible = isAdult && GetAttributeValue("ShowAddressOption").AsBoolean();
        }

        /// <summary>
        /// Shows the family view.
        /// </summary>
        private void ShowFamilyView()
        {
            pnlEditPerson.Visible = false;
            pnlEditFamily.Visible = true;
            mdEditFamily.Title = EditFamilyState.FamilyName;
            upContent.Update();
        }

        /// <summary>
        /// Shows the person view.
        /// </summary>
        /// <param name="familyPersonState">State of the family person.</param>
        private void ShowPersonView(FamilyRegistrationState.FamilyPersonState familyPersonState)
        {
            pnlEditFamily.Visible = false;
            pnlEditPerson.Visible = true;
            mdEditFamily.Title = familyPersonState.FullName;
            upContent.Update();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gFamilyMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gFamilyMembers_RowDataBound(object sender, System.Web.UI.WebControls.GridViewRowEventArgs e)
        {
            var familyPersonState = e.Row.DataItem as FamilyRegistrationState.FamilyPersonState;
            if (familyPersonState != null)
            {
                Literal lGroupRoleAndRelationship = e.Row.FindControl("lGroupRoleAndRelationship") as Literal;
                if (lGroupRoleAndRelationship != null)
                {
                    lGroupRoleAndRelationship.Text = familyPersonState.GroupRole;
                }

                var deleteCell = (e.Row.Cells[_deleteFieldIndex] as DataControlFieldCell).Controls[0];
                if (deleteCell != null)
                {
                    // only support deleting people that haven't been saved to the database yet
                    deleteCell.Visible = !familyPersonState.PersonId.HasValue;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnAddPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnAddPerson_Click(object sender, System.EventArgs e)
        {
            EditGroupMember(null);
        }

        /// <summary>
        /// Handles the Click event of the btnSaveFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveFamily_Click(object sender, EventArgs e)
        {

            if (!EditFamilyState.FamilyPersonListState.Any(x => !x.IsDeleted))
            {
                // Saving a new family, but nobody added to family, so just exit
                CancelFamilyEdit(false);
            }

            if (!this.Page.IsValid)
            {
                return;
            }

            var rockContext = new RockContext();
            var workflowTypeFamily = WorkflowTypeCache.Get(GetAttributeValue("NewFamilyWorkflowType").AsGuid());
            var workflowTypePerson = WorkflowTypeCache.Get(GetAttributeValue("NewPersonWorkflowType").AsGuid());

            FamilyRegistrationState.SaveResult saveResult = null;

            rockContext.WrapTransaction(() =>
            {
                saveResult = EditFamilyState.SaveFamilyAndPersonsToDatabase(CampusId, rockContext);
                FamilyId = EditFamilyState.GroupId;
                FamilyName = EditFamilyState.FamilyName;
            });

            // Queue up any Workflows that are configured to fire after a new person and/or family is added
            if (saveResult.NewFamilyList.Any() && workflowTypeFamily != null)
            {
                // only fire a NewFamily workflow if the Primary family is new (don't fire workflows for any 'Can Checkin' families that were created)
                var newPrimaryFamily = saveResult.NewFamilyList.FirstOrDefault(a => a.Id == EditFamilyState.GroupId.Value);
                if (newPrimaryFamily != null)
                {
                    LaunchWorkflowTransaction launchWorkflowTransaction = new LaunchWorkflowTransaction<Group>(workflowTypeFamily.Id, newPrimaryFamily.Id);
                    launchWorkflowTransaction.Enqueue();
                }
            }

            if (saveResult.NewPersonList.Any() && workflowTypePerson != null)
            {
                foreach (var newPerson in saveResult.NewPersonList)
                {
                    LaunchWorkflowTransaction launchWorkflowTransaction = new LaunchWorkflowTransaction<Person>(workflowTypePerson.Id, newPerson.Id);
                    launchWorkflowTransaction.Enqueue();
                }
            }

            upContent.Update();
            mdEditFamily.Hide();
            FamilyMembers();
        }

        /// <summary>
        /// Handles the Click event of the btnDonePerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDonePerson_Click(object sender, EventArgs e)
        {
            Guid groupMemberGuid = hfGroupMemberGuid.Value.AsGuid();
            var familyPersonState = EditFamilyState.FamilyPersonListState.FirstOrDefault(a => a.GroupMemberGuid == groupMemberGuid);
            if (familyPersonState == null)
            {
                // new person added
                familyPersonState = FamilyRegistrationState.FamilyPersonState.FromTemporaryPerson();
                familyPersonState.GroupMemberGuid = groupMemberGuid;
                familyPersonState.PersonId = null;
                EditFamilyState.FamilyPersonListState.Add(familyPersonState);
            }

            familyPersonState.RecordStatusValueId = dvpRecordStatus.SelectedValue.AsIntegerOrNull();
            familyPersonState.ConnectionStatusValueId = hfConnectionStatus.Value.AsIntegerOrNull();
            familyPersonState.IsAdult = tglAdultChild.Checked;

            familyPersonState.Gender = bgGender.SelectedValueAsEnumOrNull<Gender>() ?? Gender.Unknown;

            familyPersonState.IsMarried = tglAdultMaritalStatus.Checked;
            familyPersonState.FirstName = tbFirstName.Text.FixCase();
            familyPersonState.LastName = tbLastName.Text.FixCase();
            familyPersonState.SuffixValueId = dvpSuffix.SelectedValue.AsIntegerOrNull();

            familyPersonState.MobilePhoneNumber = pnMobilePhone.Number;
            familyPersonState.MobilePhoneCountryCode = pnMobilePhone.CountryCode;
            familyPersonState.BirthDate = dpBirthDate.SelectedDate;
            familyPersonState.Email = tbEmail.Text;

            if ( familyPersonState.IsAdult )
            {
                if ( AddressIsFilled( acHomeAddress ) )
                {
                    //pnAddress.Visible = false;
                    EditFamilyState.EditHomeAddress( acHomeAddress );
                }
                else
                {
                    //pnAddress.Visible = true;
                    //return;
                }
            }

            if (gpGradePicker.SelectedGradeValue != null)
            {
                familyPersonState.GradeOffset = gpGradePicker.SelectedGradeValue.Value.AsIntegerOrNull();
            }
            else
            {
                familyPersonState.GradeOffset = null;
            }

            ShowFamilyView();

            BindFamilyMembersGrid();
        }

        private bool AddressIsFilled(AddressControl address)
        {
            return !string.IsNullOrEmpty(address.Street1) || !string.IsNullOrEmpty(address.City)
                    || !string.IsNullOrEmpty(address.State) || !string.IsNullOrEmpty(address.PostalCode);
        }

        /// <summary>
        /// Handles the Click event of the btnCancelPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelPerson_Click(object sender, EventArgs e)
        {
            ShowFamilyView();

            if (!EditFamilyState.FamilyPersonListState.Any())
            {
                // cancelling on adding first person to family, so cancel adding the family too
                CancelFamilyEdit(false);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancelFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelFamily_Click(object sender, EventArgs e)
        {
            CancelFamilyEdit(false);
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglAdultChild control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglAdultChild_CheckedChanged(object sender, EventArgs e)
        {
            ShowControlsForRole(tglAdultChild.Checked);
        }

        /// <summary>
        /// Cancels the family edit.
        /// </summary>
        /// <param name="promptIfChangesMade">if set to <c>true</c> [prompt if changes made].</param>
        private void CancelFamilyEdit(bool promptIfChangesMade)
        {
            if (promptIfChangesMade)
            {
                int currentEditFamilyStateHash = EditFamilyState.GetStateHash();
                if (_initialEditFamilyStateHash != currentEditFamilyStateHash)
                {
                    hfShowCancelEditPrompt.Value = "1";
                    upContent.Update();
                    return;
                }
            }

            upContent.Update();
            mdEditFamily.Hide();
        }

        /// <summary>
        /// Handles the Click event of the EditFamilyMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void EditFamilyMember_Click(object sender, Rock.Web.UI.Controls.RowEventArgs e)
        {
            EditGroupMember((Guid)e.RowKeyValue);
        }

        /// <summary>
        /// Handles the Click event of the DeleteFamilyMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void DeleteFamilyMember_Click(object sender, Rock.Web.UI.Controls.RowEventArgs e)
        {
            var familyPersonState = EditFamilyState.FamilyPersonListState.FirstOrDefault(a => a.GroupMemberGuid == (Guid)e.RowKeyValue);
            familyPersonState.IsDeleted = true;
            BindFamilyMembersGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gFamilyMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        private void gFamilyMembers_GridRebind(object sender, Rock.Web.UI.Controls.GridRebindEventArgs e)
        {
            BindFamilyMembersGrid();
        }

        #endregion
        
        #endregion

        #region Helper Classes

        public class FamilyRegistrationState
        {
            /// <summary>
            /// Creates a FamilyState object from the group
            /// </summary>
            /// <param name="group">The group.</param>
            /// <returns></returns>
            public static FamilyRegistrationState FromGroup(Group group)
            {
                FamilyRegistrationState familyState = new FamilyRegistrationState();
                familyState.FamilyPersonListState = new List<FamilyRegistrationState.FamilyPersonState>();

                var rockContext = new RockContext();
                var groupLocation = new GroupLocationService(rockContext);

                if (group.Id > 0)
                {
                    var homeValue = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid()).Id;
                    var familyAddress = groupLocation.AsNoFilter().AsQueryable()
                        .Where(g => g.GroupId == group.Id && g.GroupLocationTypeValue.Id == homeValue)
                        .FirstOrDefault();


                    familyState.GroupId = group.Id;
                    familyState.FamilyName = group.Name;

                    familyState.HomeLocation = familyAddress;
                }
                else
                {
                    familyState.FamilyName = "Add Family";
                }

                return familyState;
            }

            /// <summary>
            /// The person search alternate value identifier (barcode search key)
            /// </summary>
            private static int _personSearchAlternateValueId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid()).Id;

            /// <summary>
            /// The marital status married identifier
            /// </summary>
            private static int _maritalStatusMarriedId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid()).Id;

            /// <summary>
            /// Gets the name of the family.
            /// </summary>
            /// <value>
            /// The name of the family.
            /// </value>
            public string FamilyName { get; set; }

            /// <summary>
            /// Gets or sets the group identifier.
            /// </summary>
            /// <value>
            /// The group identifier.
            /// </value>
            public int? GroupId { get; set; }

            /// <summary>
            /// Gets or sets the family Current Address
            /// </summary>
            /// <value>
            /// The group identifier.
            /// </value>
            public GroupLocation HomeLocation { get; set; }


            /// <summary>
            /// Gets or sets the list of people associated with this family ( a family member or a Person with a "Can Check-in, etc"  Relationship )
            /// </summary>
            /// <value>
            /// The state of the family person list.
            /// </value>
            public List<FamilyPersonState> FamilyPersonListState { get; set; }

            public FamilyPersonState GetPersonFromId(int id)
            {
                var res = FamilyPersonListState.Where(p => p.PersonId == id).FirstOrDefault();

                return res;
            }

            public int? PrimaryPersonId { get; set; }

            /// <summary>
            /// A Member of the Family or a Person with a "Can Check-in, etc"  Relationship
            /// </summary>
            [System.Diagnostics.DebuggerDisplay("{FullName}, {GroupRole}, InPrimaryFamily:{InPrimaryFamily}")]
            public class FamilyPersonState
            {
                /// <summary>
                /// Creates a temporary FamilyMemberState from a "new Person()"
                /// </summary>
                /// <returns></returns>
                public static FamilyPersonState FromTemporaryPerson()
                {
                    return FromPerson(new Person(), 0, true);
                }

                /// <summary>
                /// Creates a FamilyMemberState from the person object
                /// </summary>
                /// <param name="person">The person.</param>
                /// <param name="childRelationshipToAdult">The child relationship to adult.</param>
                /// <param name="inPrimaryFamily">if set to <c>true</c> [in primary family].</param>
                /// <returns></returns>
                public static FamilyPersonState FromPerson(Person person, int childRelationshipToAdult, bool inPrimaryFamily)
                {
                    var familyPersonState = new FamilyPersonState();
                    familyPersonState.IsAdult = person.AgeClassification == AgeClassification.Adult;
                    if (person.Id > 0)
                    {
                        familyPersonState.PersonId = person.Id;
                        familyPersonState.PersonAliasId = person.PrimaryAliasId;
                    }

                    familyPersonState.BirthDate = person.BirthDate;
                    familyPersonState.ChildRelationshipToAdult = childRelationshipToAdult;
                    familyPersonState.InPrimaryFamily = inPrimaryFamily;
                    familyPersonState.Email = person.Email;
                    familyPersonState.FirstName = person.FirstName;
                    familyPersonState.Gender = person.Gender;
                    familyPersonState.GradeOffset = person.GradeOffset;
                    familyPersonState.IsMarried = person.MaritalStatusValueId == _maritalStatusMarriedId;
                    familyPersonState.LastName = person.LastName;
                    var mobilePhone = person.GetPhoneNumber(Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid());
                    familyPersonState.MobilePhoneCountryCode = mobilePhone != null ? mobilePhone.CountryCode : string.Empty;
                    familyPersonState.MobilePhoneNumber = mobilePhone != null ? mobilePhone.Number : string.Empty;
                    familyPersonState.IsMessagingEnabled = mobilePhone != null ? mobilePhone.IsMessagingEnabled : false;

                    familyPersonState.SuffixValueId = person.SuffixValueId;

                    familyPersonState.RecordStatusValueId = person.RecordStatusValueId;
                    familyPersonState.ConnectionStatusValueId = person.ConnectionStatusValueId;

                    return familyPersonState;
                }

                /// <summary>
                /// Gets or sets a value indicating whether this family person was deleted from the grid
                /// </summary>
                /// <value>
                ///   <c>true</c> if this instance is deleted; otherwise, <c>false</c>.
                /// </value>
                public bool IsDeleted { get; set; }

                public bool IsSelected { get; set; }

                public int? PersonAliasId { get; set; }

                /// <summary>
                /// Gets or sets a value indicating whether this record was initially a 'New Person' in the UI, but was converted to a Matched person after a match was found
                /// </summary>
                /// <value>
                ///   <c>true</c> if [converted to matched person]; otherwise, <c>false</c>.
                /// </value>
                public bool ConvertedToMatchedPerson { get; set; }

                /// <summary>
                /// Gets or sets the group member unique identifier (or a new guid if this is a new record that hasn't been saved yet)
                /// </summary>
                /// <value>
                /// The group member unique identifier.
                /// </value>
                public Guid GroupMemberGuid { get; set; }

                /// <summary>
                /// Gets the person identifier or null if this is a new record that hasn't been saved yet
                /// </summary>
                /// <value>
                /// The person identifier.
                /// </value>
                public int? PersonId { get; set; }

                /// <summary>
                /// Gets or sets the group identifier for the family that this person is in (Person could be in a different family depending on ChildRelationshipToAdult)
                /// </summary>
                /// <value>
                /// The group identifier.
                /// </value>
                public int? GroupId { get; set; }

                /// <summary>
                /// Gets or sets a value indicating whether this instance is adult.
                /// </summary>
                /// <value>
                ///   <c>true</c> if this instance is adult; otherwise, <c>false</c>.
                /// </value>
                public bool IsAdult { get; set; }

                /// <summary>
                /// Gets or sets the gender.
                /// </summary>
                /// <value>
                /// The gender.
                /// </value>
                public Gender Gender { get; set; }

                /// <summary>
                /// Gets or sets GroupRoleId for the child relationship to adult KnownRelationshipType, or 0 if they are just a Child/Adult in this family
                /// </summary>
                /// <value>
                /// The child relationship to adult.
                /// </value>
                public int ChildRelationshipToAdult { get; set; }

                /// <summary>
                /// Gets or sets a value indicating whether [in primary family].
                /// </summary>
                /// <value>
                ///   <c>true</c> if [in primary family]; otherwise, <c>false</c>.
                /// </value>
                public bool InPrimaryFamily { get; set; }

                /// <summary>
                /// If InPrimaryFamily == False, indicates whether the ChildRelationshipToAdult should ensure there is a CanCheckIn relationship
                /// </summary>
                /// <value>
                ///   <c>true</c> if this instance can check in; otherwise, <c>false</c>.
                /// </value>
                public bool CanCheckIn { get; set; }

                /// <summary>
                /// Gets or sets a value indicating whether this instance is married.
                /// </summary>
                /// <value>
                ///   <c>true</c> if this instance is married; otherwise, <c>false</c>.
                /// </value>
                public bool IsMarried { get; set; }

                /// <summary>
                /// Gets or sets the first name.
                /// </summary>
                /// <value>
                /// The first name.
                /// </value>
                public string FirstName { get; set; }

                /// <summary>
                /// Gets or sets the last name.
                /// </summary>
                /// <value>
                /// The last name.
                /// </value>
                public string LastName { get; set; }

                /// <summary>
                /// Gets the group role.
                /// </summary>
                /// <value>
                /// The group role.
                /// </value>
                public string GroupRole { get { return IsAdult ? "Adult" : "Child"; } }

                /// <summary>
                /// Gets the full name.
                /// </summary>
                /// <value>
                /// The full name.
                /// </value>
                public string FullName
                {
                    get
                    {
                        if (this.FirstName == null)
                        {
                            return "Add Person";
                        }
                        else
                        {
                            return Person.FormatFullName(this.FirstName, this.LastName, this.SuffixValueId);
                        }
                    }
                }

                /// <summary>
                /// Returns the Search term to use when searching for this person's family
                /// </summary>
                /// <value>
                /// The full name for search.
                /// </value>
                public string FullNameForSearch { get { return Person.FormatFullName(this.FirstName, this.LastName, null); } }

                /// <summary>
                /// Gets the age.
                /// </summary>
                /// <value>
                /// The age.
                /// </value>
                public int? Age { get { return Person.GetAge(this.BirthDate); } }

                /// <summary>
                /// Gets the grade formatted.
                /// </summary>
                /// <value>
                /// The grade formatted.
                /// </value>
                public string GradeFormatted { get { return Person.GradeFormattedFromGradeOffset(this.GradeOffset); } }

                /// <summary>
                /// Gets or sets the suffix value identifier.
                /// </summary>
                /// <value>
                /// The suffix value identifier.
                /// </value>
                public int? SuffixValueId { get; set; }

                /// <summary>
                /// Gets or sets the record status value identifier.
                /// </summary>
                /// <value>
                /// The record status value identifier.
                /// </value>
                public int? RecordStatusValueId { get; set; }

                /// <summary>
                /// Gets or sets the connection status value identifier.
                /// </summary>
                /// <value>
                /// The connection status value identifier.
                /// </value>
                public int? ConnectionStatusValueId { get; set; }

                /// <summary>
                /// Gets or sets the mobile phone number.
                /// </summary>
                /// <value>
                /// The mobile phone number.
                /// </value>
                public string MobilePhoneNumber { get; set; }

                /// <summary>
                /// Gets or sets the mobile SMS enabled.
                /// </summary>
                /// <value>
                /// The SMS enabled
                /// </value>
                public bool IsMessagingEnabled { get; set; }

                /// <summary>
                /// Gets or sets the mobile phone country code.
                /// </summary>
                /// <value>
                /// The mobile phone country code.
                /// </value>
                public string MobilePhoneCountryCode { get; set; }

                /// <summary>
                /// Gets or sets the birth date.
                /// </summary>
                /// <value>
                /// The birth date.
                /// </value>
                public DateTime? BirthDate { get; set; }

                /// <summary>
                /// Gets or sets the email.
                /// </summary>
                /// <value>
                /// The email.
                /// </value>
                public string Email { get; set; }

                /// <summary>
                /// Gets or sets the grade offset.
                /// </summary>
                /// <value>
                /// The grade offset.
                /// </value>
                public int? GradeOffset { get; set; }

            }

            /// <summary>
            /// Any new Family (Group) record and/or Person records that were added as a result of the Save
            /// </summary>
            public class SaveResult
            {

                public SaveResult()
                {
                    NewFamilyList = new List<Group>();
                    NewPersonList = new List<Person>();
                }

                /// <summary>
                /// Gets the new family list.
                /// </summary>
                /// <value>
                /// The new family list.
                /// </value>
                public List<Group> NewFamilyList { get; private set; }

                /// <summary>
                /// Gets the new person list.
                /// </summary>
                /// <value>
                /// The new person list.
                /// </value>
                public List<Person> NewPersonList { get; private set; }
            }

            /// <summary>
            /// Saves the family and persons to the database
            /// </summary>
            /// <param name="kioskCampusId">The kiosk campus identifier.</param>
            /// <param name="rockContext">The rock context.</param>
            /// <returns></returns>
            public SaveResult SaveFamilyAndPersonsToDatabase(int? CampusId, RockContext rockContext)
            {
                SaveResult saveResult = new SaveResult();

                FamilyRegistrationState editFamilyState = this;
                var personService = new PersonService(rockContext);
                var groupService = new GroupService(rockContext);
                var recordTypePersonId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid()).Id;
                var recordStatusPending = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid() ).Id;
                var connectionStatusVisitor = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR.AsGuid() ).Id; // use default visitor if needed
                var maritalStatusMarried = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid());
                var maritalStatusSingle = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_SINGLE.AsGuid());
                var numberTypeValueMobile = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid());
                int groupTypeRoleAdultId = GroupTypeCache.GetFamilyGroupType().Roles.FirstOrDefault(a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid()).Id;
                int groupTypeRoleChildId = GroupTypeCache.GetFamilyGroupType().Roles.FirstOrDefault(a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid()).Id;

                //changed for c# 5 code
                int? id = null;
                var groupType = GroupTypeCache.Get(Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid());
                var roles = groupType != null ? groupType.Roles.FirstOrDefault(r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN.AsGuid()) : null;
                id = roles != null ? roles.Id : id;
                int? groupTypeRoleCanCheckInId = id;

                Group primaryFamily = null;

                if (editFamilyState.GroupId.HasValue)
                {
                    primaryFamily = groupService.Get(editFamilyState.GroupId.Value);
                }

                // see if we can find matches for new people that were added, and also set the primary family if this is a new family, but a matching family was found
                foreach (var familyPersonState in editFamilyState.FamilyPersonListState.Where(a => !a.PersonId.HasValue && !a.IsDeleted))
                {
                    var personQuery = new PersonService.PersonMatchQuery(familyPersonState.FirstName, familyPersonState.LastName, familyPersonState.Email, familyPersonState.MobilePhoneNumber, familyPersonState.Gender, familyPersonState.BirthDate, familyPersonState.SuffixValueId);
                    var matchingPerson = personService.FindPerson(personQuery, true);
                    if (matchingPerson != null)
                    {
                        // newly added person, but a match was found, so set the PersonId, GroupId, and ConnectionStatusValueID to the matching person instead of creating a new person
                        familyPersonState.PersonId = matchingPerson.Id;
                        var fam = matchingPerson.GetFamily(rockContext);
                        familyPersonState.GroupId = fam != null ? fam.Id : (int?)null;
                        familyPersonState.RecordStatusValueId = matchingPerson.RecordStatusValueId;
                        familyPersonState.ConnectionStatusValueId = matchingPerson.ConnectionStatusValueId;
                        familyPersonState.ConvertedToMatchedPerson = true;
                        if (primaryFamily == null && familyPersonState.IsAdult)
                        {
                            // if this is a new family, but we found a matching adult person, use that person's family as the family
                            primaryFamily = matchingPerson.GetFamily(rockContext);
                        }
                    }
                }

                // loop thru all people and add/update as needed
                foreach (var familyPersonState in editFamilyState.FamilyPersonListState.Where(a => !a.IsDeleted))
                {
                    Person person;
                    if (!familyPersonState.PersonId.HasValue)
                    {
                        person = new Person();
                        personService.Add(person);
                        saveResult.NewPersonList.Add(person);
                        person.RecordTypeValueId = recordTypePersonId;
                        familyPersonState.PersonAliasId = person.PrimaryAliasId;
                    }
                    else
                    {
                        person = personService.Get(familyPersonState.PersonId.Value);
                    }

                    // NOTE, Gender, MaritalStatusValueId, FirstName, LastName are required fields so, always updated them to match the UI (even if a matched person was found)
                    person.Gender = familyPersonState.Gender;
                    person.MaritalStatusValueId = familyPersonState.IsMarried ? maritalStatusMarried.Id : maritalStatusSingle.Id;
                    person.FirstName = familyPersonState.FirstName;
                    person.LastName = familyPersonState.LastName;

                    // if the familyPersonState was converted to a Matched Person, don't overwrite existing values with blank values
                    var saveEmptyValues = !familyPersonState.ConvertedToMatchedPerson;

                    if (familyPersonState.SuffixValueId.HasValue || saveEmptyValues)
                    {
                        person.SuffixValueId = familyPersonState.SuffixValueId;
                    }

                    if (familyPersonState.BirthDate.HasValue || saveEmptyValues)
                    {
                        person.SetBirthDate(familyPersonState.BirthDate);
                    }

                    if (familyPersonState.Email.IsNotNullOrWhiteSpace() || saveEmptyValues)
                    {
                        person.Email = familyPersonState.Email;
                    }

                    if (familyPersonState.GradeOffset.HasValue || saveEmptyValues)
                    {
                        person.GradeOffset = familyPersonState.GradeOffset;
                    }

                    // if a matching person was found, the familyPersonState's RecordStatusValueId and ConnectinoStatusValueId was already updated to match the matched person
                    person.RecordStatusValueId = familyPersonState.RecordStatusValueId ?? recordStatusPending;
                    person.ConnectionStatusValueId = familyPersonState.ConnectionStatusValueId ?? connectionStatusVisitor;

                    rockContext.SaveChanges();

                    bool isNewPerson = !familyPersonState.PersonId.HasValue;
                    if (!familyPersonState.PersonId.HasValue)
                    {
                        // if we added a new person, we know now the personId after SaveChanges, so set it
                        familyPersonState.PersonId = person.Id;
                    }

                    if (familyPersonState.MobilePhoneNumber.IsNotNullOrWhiteSpace() || saveEmptyValues)
                    {
                        person.UpdatePhoneNumber(numberTypeValueMobile.Id, familyPersonState.MobilePhoneCountryCode, familyPersonState.MobilePhoneNumber, familyPersonState.IsMessagingEnabled, false, rockContext);
                    }

                    rockContext.SaveChanges();
                }

                if ( primaryFamily == null )
                {
                    // new family and no family found by looking up matching adults, so create a new family
                    primaryFamily = new Group();
                    var familyLastName = editFamilyState.FamilyPersonListState.OrderBy( a => a.IsAdult ).Where( a => !a.IsDeleted ).Select( a => a.LastName ).FirstOrDefault();
                    primaryFamily.Name = familyLastName + " Family";
                    primaryFamily.GroupTypeId = GroupTypeCache.GetFamilyGroupType().Id;
                    primaryFamily.GroupLocations.Add( editFamilyState.HomeLocation );

                    // Set the Campus to the Campus of this Kiosk (Check for -1)
                    primaryFamily.CampusId = CampusId == -1 ? null : CampusId;

                    groupService.Add( primaryFamily );
                    saveResult.NewFamilyList.Add( primaryFamily );
                    rockContext.SaveChanges();
                }

                if ( !editFamilyState.GroupId.HasValue )
                {
                    editFamilyState.GroupId = primaryFamily.Id;
                }

                var groupMemberService = new GroupMemberService( rockContext );

                // loop thru all people that are part of the same family (in the UI) and ensure they are all in the same primary family (in the database)
                foreach ( var familyPersonState in editFamilyState.FamilyPersonListState.Where( a => !a.IsDeleted && a.InPrimaryFamily ) )
                {
                    var currentFamilyMember = primaryFamily.Members.FirstOrDefault( m => m.PersonId == familyPersonState.PersonId.Value );

                    if ( currentFamilyMember == null )
                    {
                        currentFamilyMember = new GroupMember
                        {
                            GroupId = primaryFamily.Id,
                            PersonId = familyPersonState.PersonId.Value,
                            GroupMemberStatus = GroupMemberStatus.Active
                        };

                        if ( familyPersonState.IsAdult )
                        {
                            currentFamilyMember.GroupRoleId = groupTypeRoleAdultId;
                        }
                        else
                        {
                            currentFamilyMember.GroupRoleId = groupTypeRoleChildId;
                        }

                        groupMemberService.Add( currentFamilyMember );

                        rockContext.SaveChanges();
                    }
                }

                return saveResult;
            }

            public void EditHomeAddress(AddressControl addressControl)
            {
                if (HomeLocation != null)
                {
                    HomeLocation.Location.Street1 = addressControl.Street1;
                    HomeLocation.Location.Street2 = addressControl.Street2;
                    HomeLocation.Location.City = addressControl.City;
                    HomeLocation.Location.State = addressControl.State;
                    HomeLocation.Location.PostalCode = addressControl.PostalCode;
                }
                else
                {
                    var newLocation = new Location();
                    newLocation.Street1 = addressControl.Street1;
                    newLocation.Street2 = addressControl.Street2;
                    newLocation.City = addressControl.City;
                    newLocation.State = addressControl.State;
                    newLocation.PostalCode = addressControl.PostalCode;

                    HomeLocation = new GroupLocation();
                    HomeLocation.GroupLocationTypeValueId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid()).Id;
                    HomeLocation.Location = newLocation;
                }
            }

            /// <summary>
            /// Gets a HashCode that can be used to determine if state has been changed
            /// </summary>
            /// <returns></returns>
            public int GetStateHash()
            {
                return this.ToJson().GetHashCode();
            }
        }

        #endregion


    }
}