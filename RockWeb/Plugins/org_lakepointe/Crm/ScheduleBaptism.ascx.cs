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
using System.Linq;
using System.Web.UI.WebControls;

namespace RockWeb.Plugins.org_lakepointe.Crm
{
    [DisplayName("LPC - Baptism Scheduler")]
    [Category("LPC > CRM")]
    [Description("Allows a person to schedule their baptism.")]

    [TextField("Schedule Header Text",
        Description = "The header text for the Schedule Baptism Header",
        IsRequired = false,
        DefaultValue = "Schedule Baptism",
        Key = AttributeKey.ScheduleHeaderText,
        Order = 1)]

    [CodeEditorField("Intro Message template",
        Description = "The lava template for the message to show on the scheudle baptism screen.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = true,
        DefaultValue = "",
        Key = AttributeKey.IntroMessageTemplate,
        Order = 2)]

    [CodeEditorField("Confirmation Message template",
        Description = "The lava template for the message to show on the confirmation screen.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = true,
        DefaultValue = "",
        Key = AttributeKey.ConfirmationMessageTemplate,
        Order = 2)]

    [DefinedValueField(Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        Name = "Default Connection Status",
        Description = "The default connection statuis that is used when registering unknown people.",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = "76b06690-3109-44e1-b415-1dc82a84dc0a",
        Key = AttributeKey.DefaultConnectionStatus,
        Order = 3)]

    [LavaCommandsField("Enabled Lava Commands",
        Description = "The Lava commands that should be enabled for this HTML block.",
        IsRequired = false,
        DefaultValue = "",
        Key = AttributeKey.EnabledLavaCommands,
        Order = 4)]

    //[MatrixField("e0edfc68-4a5a-4938-8fcb-2f351ce7af95",
    //    "Control Configuration",
    //    Description = "Allows a user to configure control properties.",
    //    IsRequired = false,
    //    Key = AttributeKey.ControlConfiguration,
    //    Order = 5)]

    [CampusesField("Campuses",
        "The campuses that this block can schedule baptisms for.",
        true,
        "",
        "",
        6,
        AttributeKey.Campuses)]

    [WorkflowTypeField("Notification Workflow",
        Description = "The workflow used to notify the campus staff and send a confirmation to the person scheduling a baptism. The person will be sent to the workflow as the entity.",
        AllowMultiple = false,
        IsRequired = false,
        Key = AttributeKey.NotificationWorkflow)]

    [BooleanField("Show Address",
        Description = "Show Edit Address Panel",
        IsRequired = false,
        DefaultBooleanValue = false,
        Category = "",
        Order = 7,
        Key = "ShowAddress")]

    [IntegerField("Maximum Weeks Out",
        Description = "The number of weeks out to schedule for baptisms.",
        IsRequired = false,
        DefaultIntegerValue = 12,
        Key = AttributeKey.MaximumWeeksOut,
        Order = 8)]

    [DayOfWeekField("Close Current Week on",
        "Day of week to close scheduling for the upcoming week.",
        false,
        DayOfWeek.Friday,
        Key = AttributeKey.CloseWeekOn,
        Order = 9)]

    [ValueListField("Excluded Weeks",
        "The Sunday date of the weeks that should be excluded from the scheduling list.",
        false,
        "",
        "Dates",
        Key = AttributeKey.ExcludedWeeks,
        Order = 10)]

    public partial class ScheduleBaptism : RockBlock
    {
        #region Attribute Keys 
        protected static class AttributeKey
        {
            public const string Campuses = "Campuses";
            //public const string ControlConfiguration = "ControlConfiguration";
            public const string ConfirmationMessageTemplate = "ConfirmationMessageTemplate";
            public const string DefaultConnectionStatus = "DefaultConnectionStatus";
            public const string EnabledLavaCommands = "EnabledLavaCommands";
            public const string ExcludedWeeks = "ExcludedWeeks";
            public const string IntroMessageTemplate = "IntroMessageTemplete";
            public const string ScheduleHeaderText = "ScheduleHeaderText";
            public const string ShowAddress = "ShowAddress";
            public const string MaximumWeeksOut = "MaximumWeeksOut";
            public const string CloseWeekOn = "CloseCurrentWeekOn";
            public const string NotificationWorkflow = "NotificationWorkflow";

            public const string PA_Baptism_FY = "BaptismScheduled-FY";
            public const string PA_Baptism_600 = "Arena-15-377";
            public const string PA_Baptism_930 = "Arena-15-378";
            public const string PA_Baptism_1100 = "Arena-15-379";
            public const string PA_Baptism_Other = "Arena-15-217";

        }
        #endregion

        #region Control Config
        private class ControlConfigurationItem
        {
            public string ControlId { get; set; }
            public string Property { get; set; }
            public string Value { get; set; }
        }

        #endregion

        #region Fields
        private Person _selectedPerson = null;
        private RockContext _rockContext = null;
        private bool? _showAddress = null;
        private int? _maximumWeeksOut = null;
        private DayOfWeek? _closeWeekOn = null;
        private List<DateTime> _excludedWeeks = null;
        #endregion

        #region Properties
        private DayOfWeek CloseWeekOn
        {
            get
            {
                if (!_closeWeekOn.HasValue)
                {
                    _closeWeekOn = GetAttributeValue(AttributeKey.CloseWeekOn).ConvertToEnum<DayOfWeek>();
                }

                return _closeWeekOn.Value;
            }
        }

        private List<DateTime> ExcludedWeeks
        {
            get
            {
                if (_excludedWeeks == null)
                {
                    _excludedWeeks = GetExcludedWeeks();
                }

                return _excludedWeeks;

            }
        }

        private int MaximumWeeksOut
        {
            get
            {
                if (!_maximumWeeksOut.HasValue)
                {
                    _maximumWeeksOut = GetAttributeValue(AttributeKey.MaximumWeeksOut).AsInteger();
                }
                return _maximumWeeksOut.Value;
            }
        }
        private Person SelectedPerson
        {
            get
            {
                if (_selectedPerson == null && hfPersonId.Value.IsNotNullOrWhiteSpace())
                {
                    LoadPerson(hfPersonId.Value.AsInteger());
                }
                return _selectedPerson;
            }
            set
            {
                _selectedPerson = value;

            }
        }

        private bool ShowAddress
        {
            get
            {
                if (!_showAddress.HasValue)
                {
                    _showAddress = GetAttributeValue(AttributeKey.ShowAddress).AsBoolean();
                }

                return _showAddress.Value;
            }
        }
        #endregion

        #region Control Methods

        protected override void OnInit(EventArgs e)
        {
            if (_rockContext == null)
            {
                _rockContext = new RockContext();
            }
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            UpdateNotificationBox();
            if (!IsPostBack)
            {
                SelectedPerson = CurrentPerson;
                if (SelectedPerson != null)
                {
                    hfPersonId.Value = CurrentPerson.Id.ToString();
                }
                else
                {
                    hfPersonId.Value = String.Empty;
                }
                

                BuildSchedulePanel();
            }

            base.OnLoad(e);

        }

        #endregion

        #region Events
        protected void ddlBaptismDate_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlBaptismTime.Enabled = false;
            if (ddlBaptismDate.SelectedValue.IsNullOrWhiteSpace())
            {
                return;
            }
            
            var valueArr = ddlBaptismDate.SelectedValue.Split("^".ToCharArray());
            int campusId = valueArr[0].AsInteger();
            var serviceDate = new DateTime(valueArr[1].Substring(0, 4).AsInteger(), valueArr[1].Substring(4, 2).AsInteger(), valueArr[1].Substring(6, 2).AsInteger());

            LoadScheduledTimes(campusId, serviceDate);
        }

        protected void ddlCampus_SelectedIndexChanged(object sender, EventArgs e)
        {
            var campusId = ddlCampus.SelectedValue.AsInteger();
            LoadAvailableDays(campusId);
        }

        protected void lbSave_Click(object sender, EventArgs e)
        {
            SaveRequest();
            CreateNotificationWorkflow();
            LoadConfirmation();

        }

        #endregion 

        #region Private Methods

        private void BuildSchedulePanel()
        {
            LoadCampusList();

            pnlAddress.Visible = ShowAddress;
            //UpdateControls();

            PopulateSchedulingControls();
            pnlSchedule.Visible = true;
        }

        private void CreateNotificationWorkflow()
        {
            var workflowGuid = GetAttributeValue(AttributeKey.NotificationWorkflow).AsGuid();
            var workflowType = WorkflowTypeCache.Get(workflowGuid);

            if (workflowType != null)
            {
                var campus = CampusCache.Get(ddlCampus.SelectedValue.AsInteger(), _rockContext);
                var baptismDate = GetScheduledBaptismDate();

                

                string baptismDateString = null;
                if (baptismDate.HasValue)
                {
                    var baptismTemp = DateTime.SpecifyKind(baptismDate.Value, DateTimeKind.Unspecified);
                    baptismDateString = string.Format("{0:o}", baptismTemp);
                }

                Dictionary<string, string> workflowAttributes = new Dictionary<string, string>();
                workflowAttributes.Add("Campus", campus.Guid.ToString());
                workflowAttributes.Add("BaptismDate", baptismDateString);

                if (dpAcceptChrist.SelectedDate.HasValue)
                {
                    workflowAttributes.Add("SalvationDate", dpAcceptChrist.SelectedDate.ToShortDateString());
                }

                if (tbComments.Text.IsNotNullOrWhiteSpace())
                {
                    workflowAttributes.Add("Comments", tbComments.Text.Trim());
                }

                var title = string.Format("Scheduled Baptism - {0}", SelectedPerson.FullName);

                var transaction = new Rock.Transactions.LaunchWorkflowTransaction<Person>(workflowType.Guid, title, SelectedPerson.Id);
                transaction.WorkflowAttributeValues = workflowAttributes;
                Rock.Transactions.RockQueue.GetStandardQueuedTransactions().Add(transaction);
            }

        }

        public List<DateTime> GetExcludedWeeks()
        {
            return  GetAttributeValue(AttributeKey.ExcludedWeeks).SplitDelimitedValues()
                .Select(d => d.AsDateTime())
                .Where(d => d.HasValue)
                .Where(d => d >= DateTime.Now)
                .Select(d => d.Value.SundayDate())
                .Distinct()
                .ToList();

            

        }

        public DateTime? GetScheduledBaptismDate()
        {
            try
            {
                var dateString = DateTime.ParseExact(ddlBaptismDate.SelectedValue.Split("^".ToCharArray())[1], "yyyyMMdd", null).ToShortDateString() +
                    " " + ddlBaptismTime.SelectedValue;

                return DateTime.Parse(dateString);
            }
            catch (Exception)
            {
                return null;
            }
            
        }

        private void LoadCampusList()
        {
            ddlCampus.Items.Clear();
            var campusGuids = GetAttributeValue(AttributeKey.Campuses).SplitDelimitedValues()
                .Select(g => g.AsGuidOrNull())
                .Where(g => g.HasValue)
                .ToList();

            var campusQry = CampusCache.All().AsEnumerable();

            if(campusGuids.Count > 0)
            {
                campusQry = campusQry.Where(c => campusGuids.Contains(c.Guid));
            }
            var campuses = campusQry
                .OrderBy(c => c.Order)
                .ToList();

            ddlCampus.DataSource = campuses;
            ddlCampus.DataValueField = "Id";
            ddlCampus.DataTextField = "Name";
            ddlCampus.DataBind();

            ddlCampus.Items.Insert(0, new ListItem("", ""));

        }

        private void LoadConfirmation()
        {
            pnlSchedule.Visible = false;
            pnlConfirm.Visible = true;

            

            var confirmMessage = GetAttributeValue(AttributeKey.ConfirmationMessageTemplate);

            if (confirmMessage.IsNotNullOrWhiteSpace())
            {
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(this.RockPage, CurrentPerson, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false });
                mergeFields.Add("Person", SelectedPerson);
                mergeFields.Add("BaptismDate", GetScheduledBaptismDate());


                var campus = CampusCache.Get(ddlCampus.SelectedValue.AsInteger(), _rockContext);
                mergeFields.Add("Campus", campus);
                
                lConfirmMessage.Text = confirmMessage.ResolveMergeFields(mergeFields, GetAttributeValue(AttributeKey.EnabledLavaCommands));
            }
        }

        private void LoadAvailableDays(int campusId)
        {
            ddlBaptismDate.Items.Clear();
            var campus = CampusCache.Get(campusId, _rockContext);

            if (campus == null)
            {
                return;
            }

            var serviceDays = campus.ServiceTimes.Select(s => s.Day);

            var includeSaturday = serviceDays.Count(s => s.Equals("Saturday", StringComparison.InvariantCultureIgnoreCase)) > 0;
            var includeSunday = serviceDays.Count(s => s.Equals("Sunday", StringComparison.InvariantCultureIgnoreCase)) > 0;

            var firstSunday = RockDateTime.Now.SundayDate();
            if (RockDateTime.Now.DayOfWeek > CloseWeekOn)
            {
                firstSunday = firstSunday.AddDays(7);
            }


            for (int i = 0; i < MaximumWeeksOut; i++)
            {
                var daysAdd = 7 * i;
                var sunday = firstSunday.AddDays(daysAdd);
                var valueFormat = "{0}^{1:yyyyMMdd}";
                var textFormat = "{0:dddd M/d/yyyy}";


                if (ExcludedWeeks.Contains(sunday))
                {
                    continue;
                }
                if (includeSaturday)
                {
                    var saturday = sunday.AddDays(-1);
                    var li = new ListItem();
                    li.Value = string.Format(valueFormat, campus.Id, saturday);
                    li.Text = string.Format(textFormat, saturday);

                    ddlBaptismDate.Items.Add(li);
                }

                if (includeSunday)
                {
                    var li = new ListItem();
                    li.Value = string.Format(valueFormat, campusId, sunday);
                    li.Text = string.Format(textFormat, sunday);

                    ddlBaptismDate.Items.Add(li);
                }

            }
            ddlBaptismDate.Items.Insert(0, new ListItem("", ""));
        }

        private void LoadPerson(int personId)
        {
            hfPersonId.Value = String.Empty;
            _selectedPerson = null;
            if (personId <= 0)
            {
                return;
            }

            var person = new PersonService(_rockContext).Get(personId);

            if (person != null && person.Id > 0)
            {
                hfPersonId.Value = person.Id.ToString();
                _selectedPerson = person;
            }
        }

        private void LoadScheduledTimes(int campusId, DateTime date)
        {
            ddlBaptismTime.Items.Clear();

            var campus = CampusCache.Get(campusId, _rockContext);
            var serviceTimes = campus.ServiceTimes
                .Where(s => s.Day.Equals(date.DayOfWeek.ToString()))
                .ToList();

            foreach (var item in serviceTimes)
            {
                ddlBaptismTime.Items.Add(new ListItem(item.Time, item.Time));
            }

            ddlBaptismTime.Enabled = true;

        }

        private void PopulateSchedulingControls()
        {

            var introMessage = GetAttributeValue(AttributeKey.IntroMessageTemplate);

            if (introMessage.IsNotNullOrWhiteSpace())
            {
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(this.RockPage, CurrentPerson, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false });
                lIntroText.Text = introMessage.ResolveMergeFields(mergeFields, GetAttributeValue(AttributeKey.EnabledLavaCommands));
            }

            if (SelectedPerson == null)
            {
                return;
            }

            if (SelectedPerson.PrimaryCampusId.HasValue)
            {
                ddlCampus.SelectedValue = SelectedPerson.PrimaryCampusId.Value.ToString();
                LoadAvailableDays(SelectedPerson.PrimaryCampusId.Value);
            }

            tbNickName.Text = SelectedPerson.NickName;
            tbLastName.Text = SelectedPerson.LastName;
            dpBirthdate.SelectedDate = SelectedPerson.BirthDate;
            rblGender.SelectedValue = ((int)SelectedPerson.Gender).ToString();
            tbEmail.Text = SelectedPerson.Email;

            var mobilePhone = SelectedPerson.GetPhoneNumber(Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid());
            if (mobilePhone != null)
            {
                tbMobilePhone.Text = mobilePhone.NumberFormatted;
                tglMayWeTextYou.Checked = mobilePhone.IsMessagingEnabled;
            }
            else
            {
                tglMayWeTextYou.Checked = true;
            }

            if (ShowAddress)
            {
                var homeLocation = SelectedPerson.GetHomeLocation(_rockContext);

                if (homeLocation != null)
                {
                    acHomeAddress.SetValues(homeLocation);
                }
            }
            
        }

        private void SaveRequest()
        {
            var personContext = new RockContext();
            var personService = new PersonService(personContext);
            Person person = null;

            var gender = rblGender.SelectedValueAsEnum<Gender>();

            if (SelectedPerson != null)
            {
                //Attempt to use the selected person and verify that the name matches
                if (tbLastName.Text.Trim().Equals(SelectedPerson.LastName))
                {
                    if (tbNickName.Text.Trim().Equals(SelectedPerson.NickName) || tbNickName.Text.Trim().Equals(SelectedPerson.FirstName))
                    {
                        person = personService.Get(SelectedPerson.Guid);
                    }
                }
            }

            if (person == null)
            {
                person = personService.FindPerson(new PersonService.PersonMatchQuery(tbNickName.Text.Trim(), tbLastName.Text.Trim(), tbEmail.Text.Trim(), tbMobilePhone.Text.Trim(), gender, dpBirthdate.SelectedDate), false, false, false);
            }

            if (person != null)
            {
                if (!person.NickName.Equals(tbNickName.Text.Trim()))
                {
                    person.NickName = tbNickName.Text.Trim();
                }

                if (person.Gender == Gender.Unknown && person.Gender != gender)
                {
                    person.Gender = gender;
                }

                if ((!person.BirthDate.HasValue || person.BirthDate < new DateTime(1900,1,1) )&& dpBirthdate.SelectedDate.HasValue)
                {
                    person.BirthYear = dpBirthdate.SelectedDate.Value.Year;
                    person.BirthMonth = dpBirthdate.SelectedDate.Value.Month;
                    person.BirthDay = dpBirthdate.SelectedDate.Value.Day;
                }

                if (person.Email.IsNullOrWhiteSpace())
                {
                    person.Email = tbEmail.Text.Trim();
                    person.IsEmailActive = true;
                    person.EmailPreference = EmailPreference.EmailAllowed;
                }
                else if (!person.Email.Equals(tbEmail.Text.Trim(), StringComparison.InvariantCultureIgnoreCase))
                {
                    person.Email = tbEmail.Text.Trim();
                    person.IsEmailActive = true;
                }

                var selectedCampusId = ddlCampus.SelectedValue.AsInteger();
                if (person.PrimaryCampusId != selectedCampusId && selectedCampusId > 0)
                {
                    person.PrimaryCampusId = selectedCampusId;
                    person.PrimaryFamily.CampusId = selectedCampusId;
                }

                personContext.SaveChanges();
                
            }
            else if (person == null && (tbEmail.Text.IsNotNullOrWhiteSpace() || PhoneNumber.CleanNumber(tbMobilePhone.Text.Trim()).IsNotNullOrWhiteSpace()))
            {
                var personRecordTypeId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid()).Id;
                var personStatusPendingId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid()).Id;
                var defaultConnectionStatusId = DefinedValueCache.Get(GetAttributeValue(AttributeKey.DefaultConnectionStatus).AsGuid()).Id;

                person = new Person();
                person.IsSystem = false;
                person.RecordTypeValueId = personRecordTypeId;
                person.RecordStatusValueId = personStatusPendingId;
                person.ConnectionStatusValueId = defaultConnectionStatusId;

                person.FirstName = tbNickName.Text.Trim();
                person.NickName = tbNickName.Text.Trim();
                person.LastName = tbLastName.Text.Trim();
                person.Gender = gender;

                if (dpBirthdate.SelectedDate.HasValue)
                {
                    person.BirthYear = dpBirthdate.SelectedDate.Value.Year;
                    person.BirthMonth = dpBirthdate.SelectedDate.Value.Month;
                    person.BirthDay = dpBirthdate.SelectedDate.Value.Day;
                }

                if (tbEmail.Text.IsNotNullOrWhiteSpace())
                {
                    person.Email = tbEmail.Text.Trim();
                    person.IsEmailActive = true;
                    person.EmailPreference = EmailPreference.EmailAllowed;
                }

                PersonService.SaveNewPerson(person, personContext, ddlCampus.SelectedValue.AsInteger());
            }


            var currentMobile = person.GetPhoneNumber(Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid());
            var cleanNumber = PhoneNumber.CleanNumber(tbMobilePhone.Text.Trim());

            if (currentMobile == null)
            {
                currentMobile = new PhoneNumber();
                currentMobile.NumberTypeValueId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid()).Id;
                person.PhoneNumbers.Add(currentMobile);
            }

            currentMobile.Number = PhoneNumber.CleanNumber(tbMobilePhone.Text.Trim());
            currentMobile.IsMessagingEnabled = tglMayWeTextYou.Checked;

            if (ShowAddress)
            {
                var locationService = new LocationService(personContext);
                var homeLocationTypeId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid()).Id;

                var address = locationService.Get(acHomeAddress.Street1, acHomeAddress.Street2, acHomeAddress.City, acHomeAddress.State, acHomeAddress.PostalCode, acHomeAddress.Country, true);

                var currentHome = person.PrimaryFamily.GroupLocations.FirstOrDefault(l => l.GroupLocationTypeValueId == homeLocationTypeId);

                if (currentHome.LocationId != address.Id)
                {
                    currentHome.IsMailingLocation = false;
                    currentHome.GroupLocationTypeValueId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid()).Id;

                    var groupLocation = new GroupLocation();
                    groupLocation.LocationId = address.Id;
                    groupLocation.IsMappedLocation = true;
                    groupLocation.IsMailingLocation = true;

                    person.PrimaryFamily.GroupLocations.Add(groupLocation);

                }
            }

            personContext.SaveChanges();

            hfPersonId.Value = person.Id.ToString();
            SelectedPerson = person;
            person.LoadAttributes(personContext);

            var baptismDate = GetScheduledBaptismDate();

            if (!baptismDate.HasValue)
            {
                UpdateNotificationBox("An error occurred scheduling baptism date. Please verify your selection and resubmit or contact us at info@lakepointe.org", NotificationBoxType.Danger);
                return;
            }

            var selectedCampus = CampusCache.Get(ddlCampus.SelectedValue.AsInteger(), _rockContext);

            if (selectedCampus.ShortCode == "FY")
            {
                person.SetAttributeValue(AttributeKey.PA_Baptism_FY, baptismDate.Value);
                person.SaveAttributeValue(AttributeKey.PA_Baptism_FY, personContext);
            }
            else if (baptismDate.Value.Hour == 9 && baptismDate.Value.Minute == 30)
            {
                person.SetAttributeValue(AttributeKey.PA_Baptism_930, baptismDate.Value.Date);
                person.SaveAttributeValue(AttributeKey.PA_Baptism_930, personContext);
            }
            else if (baptismDate.Value.Hour == 18 && baptismDate.Value.Minute == 0)
            {
                person.SetAttributeValue(AttributeKey.PA_Baptism_600, baptismDate.Value.Date);
                person.SaveAttributeValue(AttributeKey.PA_Baptism_600, personContext);
            }
            else if (baptismDate.Value.Hour == 11 && baptismDate.Value.Minute == 0)
            {
                person.SetAttributeValue(AttributeKey.PA_Baptism_1100, baptismDate.Value.Date);
                person.SaveAttributeValue(AttributeKey.PA_Baptism_1100, personContext);
            }
            else
            {
                person.SetAttributeValue(AttributeKey.PA_Baptism_Other, baptismDate.Value);
                person.SaveAttributeValue(AttributeKey.PA_Baptism_Other, personContext);
            }
            
        }

        private void UpdateControls()
        {
            throw new NotImplementedException();
        }

        private void UpdateNotificationBox()
        {
            UpdateNotificationBox(string.Empty);
        }

        private void UpdateNotificationBox(string message, NotificationBoxType boxType = NotificationBoxType.Info)
        {
            nbMessage.NotificationBoxType = boxType;
            nbMessage.Text = message.Trim();
            nbMessage.Visible = message.IsNotNullOrWhiteSpace();
         }
        #endregion




    }

}