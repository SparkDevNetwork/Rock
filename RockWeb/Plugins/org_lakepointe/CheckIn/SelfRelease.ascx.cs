using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;


namespace RockWeb.Plugins.org_lakepointe.CheckIn
{
    [DisplayName("Self Release Authorization")]
    [Category("LPC > CheckIn")]
    [Description("Tool that will allow a parent to authorize their child for checkout self release.")]

    [CustomCheckboxListField(
        name: "Enabled Grades",
        description: "The grades where self release is allowed.",
        listSource:
            @"
                SELECT
	                dv.[Id] as [Value],
	                av.[Value] as [Text]
                FROM 
	                dbo.[DefinedType] dt
	                INNER JOIN dbo.[DefinedValue] dv on dt.[Id] = dv.[DefinedTypeId]
	                INNER JOIN dbo.[AttributeValue] av on dv.[Id] = av.[EntityId]
	                INNER JOIN dbo.[Attribute] a on av.[AttributeId] = a.[Id] and a.[Key] = 'Abbreviation' 
                WHERE 
	                dt.[Guid] = '24E5A79F-1E62-467A-AD5D-0D10A2328B4D'
	                AND dv.IsActive = 1
	                AND dt.IsActive = 1
                ORDER BY  
	                dv.[Order]
            ",
        required: true,
        key: ENABLED_GRADES_KEY
        )]
    [CampusesField(
        name: "Enabled Campuses",
        description: "The campuses which allow Self Release in Children's Ministry",
        required: false,
        defaultCampusGuids: "",
        category: "",
        order: 2,
        key: ENABLED_CAMPUS_KEY)]
    [CodeEditorField(
        name: "Introduction Message Template",
        description: "The lava template for the message to display in the block header.",
        mode: CodeEditorMode.Lava,
        theme: CodeEditorTheme.Rock,
        height: 200,
        required: true,
        defaultValue: "",
        key: INTRODUCTION_LAVA_TEMPATE_KEY,
        order: 3)]
    [CodeEditorField(
        name: "Confirmation Message Template",
        description: "The lava template for the message that is displayed on the confirmation panel.",
        mode: CodeEditorMode.Lava,
        theme: CodeEditorTheme.Rock,
        height: 200,
        required: true,
        defaultValue: "",
        key: CONFIRMATION_LAVA_TEMPLATE_KEY,
        order: 4)]
    [LavaCommandsField(
        name: "Enabled Lava Commands",
        description: "The Lava commans that are enabled for this block",
        required: false,
        defaultValue: "",
        key: ENABLED_LAVA_COMMAND_KEY,
        order: 5)]
    [AttributeField(
        Rock.SystemGuid.EntityType.PERSON,
        name: "Self Release Person Attribute",
        description: "The Person Attribute that indicates if a child is able to self release from Children's programming.",
        required: true,
        allowMultiple: false,
        defaultValue: "",
        order: 6,
        key: SELF_RELEASE_ATTRIBUTE_KEY)]

    public partial class SelfRelease : RockBlock
    {

        #region Block Property Keys
        private const string ENABLED_GRADES_KEY = "EnabledGrades";
        private const string ENABLED_CAMPUS_KEY = "EnabledCampuses";
        private const string INTRODUCTION_LAVA_TEMPATE_KEY = "IntroductionMessageTemplate";
        private const string CONFIRMATION_LAVA_TEMPLATE_KEY = "ConformationMessageTemplate";
        private const string ENABLED_LAVA_COMMAND_KEY = "EnabledLavaCommands";
        private const string SELF_RELEASE_ATTRIBUTE_KEY = "SelfReleasePersonAttribute";
        private const int SELF_RELEASE_REQUEST = 0;
        private const int SELF_RELEASE_CONFIRM = 1;
        #endregion

        #region Fields
        RockContext _context = null;
        Person _selectedPerson = null;
        int _step = 0;
        #endregion

        #region Properties
        private Guid? PersonGuid { get; set; }

        private Person SelectedPerson
        {
            get
            {
                if (_selectedPerson == null && PersonGuid.HasValue)
                {
                    _selectedPerson = new PersonService(_context).Get(PersonGuid.Value);
                }

                return _selectedPerson;
            }
            set
            {
                _selectedPerson = value;

                if (value != null)
                {
                    PersonGuid = value.Guid;
                }
                else
                {
                    PersonGuid = null;
                }
            }
        }

        private List<SelfReleaseChild> SelfReleaseChildren { get; set; }

        #endregion

        #region Base Control Methods
        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);
            PersonGuid = (Guid?)ViewState["PersonGuid"];
            SelfReleaseChildren = ViewState["SelfReleaseChildren"].ToString().FromJsonOrNull<List<SelfReleaseChild>>();

        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            _context = new RockContext();

            this.BlockUpdated += selfRelease_BlockUpdated;
            rptSelfReleaseChildren.ItemDataBound += rptSelfReleaseChildren_ItemDataBound;
            lbNext.Click += lbNext_Click;
            lbBack.Click += lbBack_Click;
            lbSave.Click += lbSave_Click;
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SetAlertMessage();

            LoadIntroduction();
            if (!Page.IsPostBack)
            {
                _step = SELF_RELEASE_REQUEST;
                LoadSelfReleasePanel();
                SetVisibility();
            }

        }

        protected override object SaveViewState()
        {
            ViewState["PersonGuid"] = PersonGuid;
            ViewState["SelfReleaseChildren"] = SelfReleaseChildren.ToJson();
            return base.SaveViewState();

        }
        #endregion

        #region Events
        private void lbBack_Click(object sender, EventArgs e)
        {
            _step = SELF_RELEASE_REQUEST;
            LoadIntroduction();
            BindSelfReleaseList();

            SetVisibility();
        }

        private void lbNext_Click(object sender, EventArgs e)
        {
            _step = SELF_RELEASE_CONFIRM;
            LoadConfirmationMessage();
            GetSelfReleaseValues();

            SetVisibility();
            
        }

        private void lbSave_Click(object sender, EventArgs e)
        {
            SaveSelfReleaseSettings();
            _step = SELF_RELEASE_REQUEST;
            LoadSelfReleasePanel();

            SetVisibility();
        }


        private void rptSelfReleaseChildren_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            var srChild = (SelfReleaseChild)e.Item.DataItem;
            if (srChild == null)
            {
                return;
            }

            HiddenField hfPersonGuid = (HiddenField)e.Item.FindControl("hfPersonGuid");
            Literal lName = (Literal)e.Item.FindControl("lName");
            Literal lPhoto = (Literal)e.Item.FindControl("lFamilyMemberPhoto");
            Literal lAge = (Literal)e.Item.FindControl("lAge");
            Literal lGrade = (Literal)e.Item.FindControl("lGrade");

            Toggle tglSelfRelease = (Toggle)e.Item.FindControl("tglSelfRelease");
            Panel pnlSelfReleaseEdit = (Panel)e.Item.FindControl("pnlSelfReleaseEdit");
            Panel pnlSelfReleaseView = (Panel)e.Item.FindControl("pnlSelfReleaseView");
            Literal lSelfReleaseView = (Literal)e.Item.FindControl("lSelfReleaseView");

            string imgTag = Rock.Model.Person.GetPersonPhotoImageTag(srChild.Child, 75, 75, srChild.Child.FullName);

            if (srChild.Child.PhotoId.HasValue)
            {
                lPhoto.Text = string.Format("<a href='{0}'>{1}</a>", srChild.Child.PhotoUrl, imgTag);
            }
            else
            {
                lPhoto.Text = imgTag;
            }

            hfPersonGuid.Value = srChild.Child.Guid.ToString();
            lName.Text = srChild.Child.FullName;

            if (!srChild.Child.BirthDate.HasValue)
            {
                lAge.Text = string.Empty;
                lAge.Visible = false;
            }
            else if (srChild.Child.BirthYear.Value == DateTime.MinValue.Year)
            {
                lAge.Text = string.Format("<small>{0}/{1}</small>", srChild.Child.BirthMonth, srChild.Child.BirthDay);
            }
            else
            {
                lAge.Text = string.Format("{0} years old <small>({1})</small>", srChild.Child.Age, srChild.Child.BirthDate.Value.ToShortDateString());
            }

            if (srChild.Child.GradeFormatted.IsNotNullOrWhiteSpace())
            {
                lGrade.Text = srChild.Child.GradeFormatted;
                lGrade.Visible = true;
            }
            else
            {
                lGrade.Text = string.Empty;
                lGrade.Visible = false;
            }

            pnlSelfReleaseEdit.Visible = _step == SELF_RELEASE_REQUEST;
            pnlSelfReleaseView.Visible = _step == SELF_RELEASE_CONFIRM;

            if (_step == SELF_RELEASE_REQUEST)
            {
                tglSelfRelease.Checked = !srChild.NewSelfReleaseStatus.HasValue ? srChild.OriginalSelfReleaseStatus : srChild.NewSelfReleaseStatus.Value;
            }
            else if(_step == SELF_RELEASE_CONFIRM)
            {
                lSelfReleaseView.Text = string.Format("<span class='label {0}'>{1}</span>", srChild.NewSelfReleaseStatus == true ? "label-success" : "label-danger", srChild.NewSelfReleaseStatus.Value.ToYesNo());
            }

        }

        private void selfRelease_BlockUpdated(object sender, EventArgs e)
        {
            _step = SELF_RELEASE_REQUEST;
            LoadSelfReleasePanel();
            SetVisibility();
        }
        #endregion

        #region Methods

        private void BindSelfReleaseList()
        {
            rptSelfReleaseChildren.DataSource = SelfReleaseChildren.OrderBy(c => c.Child.BirthDate)
                .ToList();

            rptSelfReleaseChildren.DataBind();
        }

        private void GetSelfReleaseValues()
        {

            var selfReleaseAttribute = AttributeCache.Get(GetAttributeValue(SELF_RELEASE_ATTRIBUTE_KEY).AsGuid(), _context);

            foreach (RepeaterItem item in rptSelfReleaseChildren.Items)
            {
                HiddenField hfPersonGuid = (HiddenField)item.FindControl("hfPersonGuid");
                Toggle tglSelfRelease = (Toggle)item.FindControl("tglSelfRelease");

                var srChild = SelfReleaseChildren.SingleOrDefault(c => c.PersonGuid == hfPersonGuid.Value.AsGuid());

                if (srChild == null)
                {
                    continue;
                }

                srChild.NewSelfReleaseStatus = tglSelfRelease.Checked;  
            }

            BindSelfReleaseList();
        }

        private void LoadConfirmationMessage()
        {
            
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(this.RockPage, CurrentPerson,
                new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false });
            lConfirmation.Text = GetAttributeValue(CONFIRMATION_LAVA_TEMPLATE_KEY).ResolveMergeFields(mergeFields, GetAttributeValue(ENABLED_LAVA_COMMAND_KEY));
        }

        private void LoadIntroduction()
        {
           
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(this.RockPage, CurrentPerson,
                new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false });
            lIntroduction.Text = GetAttributeValue(INTRODUCTION_LAVA_TEMPATE_KEY).ResolveMergeFields(mergeFields, GetAttributeValue(ENABLED_LAVA_COMMAND_KEY));
        }
        private bool LoadSelfReleasePanel()
        {
            string message = String.Empty;
            string title = String.Empty;

            if (CurrentPerson == null)
            {
                message = "Please login above to use the Self Release tool.";
                title = "<i class=\"far fa-exclamation-triangle\"></i> Not Logged In";
                SetAlertMessage(title, message, NotificationBoxType.Info);
                return false;
            }
            SelectedPerson = CurrentPerson;
            bool isEligible = true;


            if (SelectedPerson.GetFamilyRole(_context).Guid != Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid())
            {
                isEligible = false;
                message = "<br />Sorry, you are currently not eligible to use the LP Kids Self Checkout Tool.";
                title = "<i class=\"far fa-exclamation-triangle\"></i> Not Eligible";
            }

            var campusGuids = GetAttributeValue(ENABLED_CAMPUS_KEY).SplitDelimitedValues()
                .Select(c => c.AsGuidOrNull())
                .Where(c => c.HasValue)
                .ToList();

            if (!campusGuids.Contains(SelectedPerson.GetCampus().Guid))
            {
                isEligible = false;
                message = string.Format("<br />Sorry, The LP Kids Self Checkout Tool is currently not available at the {0} campus. ", CurrentPerson.GetCampus().Name);
                title = "<i class=\"far fa - exclamation - triangle\"></i> Not Eligible";
            }

            var eligibleGradeIds = GetAttributeValue(ENABLED_GRADES_KEY).SplitDelimitedValues()
                .Select(g => g.AsIntegerOrNull())
                .Where(g => g.HasValue)
                .ToList();

            var graduationYears = new List<int>();
            foreach (var gradeId in eligibleGradeIds)
            {
                var gradeOffset = DefinedValueCache.Get(gradeId.Value).Value.AsInteger();
                graduationYears.Add( PersonService.GetCurrentGraduationYear() + gradeOffset);

            }

            var inactivePersonDefinedValue = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid(), _context);
            var childFamilyRole = new GroupTypeRoleService(_context).Get(Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid());

            SelfReleaseChildren = SelectedPerson.GetFamilyMembers(rockContext: _context)
                .Where(fm => isEligible &&  fm.GroupMemberStatus == GroupMemberStatus.Active)
                .Where(fm => fm.GroupRoleId == childFamilyRole.Id)
                .Where(fm => !fm.Person.IsDeceased)
                .Where(fm => fm.Person.RecordStatusValueId != inactivePersonDefinedValue.Id)
                .Where(fm => fm.Person.GraduationYear.HasValue && graduationYears.Contains(fm.Person.GraduationYear.Value))
                .Select(fm => new SelfReleaseChild
                {
                    Child = fm.Person,
                    PersonGuid = fm.Person.Guid
                })
                .OrderBy(c => c.Child.BirthDate)
                .ToList();

            if (SelfReleaseChildren.Count() == 0)
            {
                isEligible = false;
                message = "<br /> Sorry, you are not eligible to use the Self Checkout tool at this time.";
                title = "<i class=\"far fa-exclamation-triangle\"></i> Not Eligible";
            }

            AttributeCache selfReleaseAttribute = AttributeCache.Get(GetAttributeValue(SELF_RELEASE_ATTRIBUTE_KEY).AsGuid(), _context);

            if (selfReleaseAttribute != null)
            {
                foreach (var src in SelfReleaseChildren)
                {
                    src.Child.LoadAttributes(_context);
                    src.OriginalSelfReleaseStatus = src.Child.GetAttributeValue(selfReleaseAttribute.Key).AsBoolean();
                }
            }

            if (!isEligible)
            {
                SetAlertMessage(title, message, NotificationBoxType.Warning);
            }

            BindSelfReleaseList();

            return isEligible;
        }

        private void SaveSelfReleaseSettings()
        {
            bool wasSelfReleaseUpdated = false;
            var selfReleaseAttribute = AttributeCache.Get(GetAttributeValue(SELF_RELEASE_ATTRIBUTE_KEY).AsGuid(), _context);
            var noteType = NoteTypeCache.Get(Rock.SystemGuid.NoteType.PERSON_TIMELINE_NOTE.AsGuid(), _context);

            var updatePersonContext = new RockContext();
            var personService = new PersonService(updatePersonContext);
            var noteService = new NoteService(updatePersonContext);
            foreach (var c in SelfReleaseChildren)
            {
                if (!c.PersonGuid.HasValue)
                {
                    continue;
                }
                var person = personService.Get(c.PersonGuid.Value);
                person.LoadAttributes(updatePersonContext);
                var attributeValue = person.GetAttributeValue(selfReleaseAttribute.Key).AsBoolean();
                if (c.NewSelfReleaseStatus.HasValue && !attributeValue.Equals(c.NewSelfReleaseStatus.Value))
                {
                    wasSelfReleaseUpdated = true;
                    person.SetAttributeValue(selfReleaseAttribute.Key, c.NewSelfReleaseStatus.Value.ToTrueFalse());
                    person.SaveAttributeValue(selfReleaseAttribute.Key, updatePersonContext);

                    string noteText = string.Format("Self Release updated to {0} by {1}.",
                        c.NewSelfReleaseStatus.Value.ToYesNo(),
                        SelectedPerson.FullName);

                    var note = new Note
                    {
                        NoteTypeId = noteType.Id,
                        IsSystem = false,
                        IsAlert = false,
                        IsPrivateNote = false,
                        EntityId = c.Child.Id,
                        Caption = string.Empty,
                        Text = noteText,
                        CreatedByPersonAliasId = SelectedPerson.PrimaryAliasId
                    };
                    noteService.Add(note);
                }

                updatePersonContext.SaveChanges();
            }

            if (wasSelfReleaseUpdated)
            {
                SetAlertMessage("<i class=\"far fa-check-square\"></i> Success", "<br /> Self Checkout permissions have been updated.", NotificationBoxType.Success);
            }

        }

        private void SetAlertMessage()
        {
            SetAlertMessage(null, null, NotificationBoxType.Danger);
        }

        private void SetAlertMessage(string title, string message, NotificationBoxType type)
        {
            nbWarning.Title = title;
            nbWarning.Text = message;
            nbWarning.NotificationBoxType = type;
        }

        private void SetVisibility()
        {
            var isValid = SelfReleaseChildren != null && SelfReleaseChildren.Count > 0;
            lIntroduction.Visible = _step == SELF_RELEASE_REQUEST;
            lConfirmation.Visible = _step == SELF_RELEASE_CONFIRM;

            pnlSelfRelease.Visible = isValid;
            lbNext.Visible = _step == SELF_RELEASE_REQUEST && isValid;
            lbBack.Visible = _step == SELF_RELEASE_CONFIRM;
            lbSave.Visible = _step == SELF_RELEASE_CONFIRM;

        }
        #endregion
    }

    public class SelfReleaseChild
    {
        #region Fields
        private Person _child = null;
        private RockContext _context = null;
        #endregion

        #region Properties
        public Guid? PersonGuid { get; set; }
        
        [Newtonsoft.Json.JsonIgnore]
        [System.Xml.Serialization.XmlIgnore]
        public Person Child
        {
            get
            {
                if (_child == null && PersonGuid.HasValue)
                {
                    _child = LoadChild();
                }
                return _child;
            }
            set
            {
                _child = value;
                PersonGuid = value.Guid;
            }
        }

        public bool OriginalSelfReleaseStatus { get; set; }

        public bool? NewSelfReleaseStatus { get; set; }
        #endregion

        #region Private Methods
        private Person LoadChild()
        {
            if (_context == null)
            {
                _context = new RockContext();
            }

            return new PersonService(_context).Get(PersonGuid.Value);

        }
        #endregion

    }
}