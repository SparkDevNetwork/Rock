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
using System.Web.UI.WebControls;

namespace RockWeb.Plugins.org_lakepointe.PastoralCare
{
    [DisplayName("Visit History")]
    [Category("LPC > Pastoral Care")]
    [Description("A summary of all visits by the pastoral care team. Can be sorted by Visitor or by Facility.")]

    [BooleanField("Show Visitor Picker",
        Description = "Should the Visitor Picker be visible.",
        DefaultBooleanValue = true,
        IsRequired = false,
        Key = AttributeKey.ShowVisitorPicker)]

    [CustomCheckboxListField("Viewable Pastoral Care Types",
        "The Pastoral Care Types that can be Picked by this Picker.",
        PastoralCareWorkflow.HospitalizationGuid + "^Hospitalization," + PastoralCareWorkflow.NursingHomeGuid + "^Nursing Home," + PastoralCareWorkflow.HomeboundGuid + "^Homebound",
        IsRequired = true,
        Key = AttributeKey.ViewablePastoralCareTypes)]




    public partial class VisitHistory : RockBlock
    {

        protected static class PastoralCareWorkflow
        {
            public const string HospitalizationGuid = "314cc992-c90c-4d7d-aec6-09c0fb4c7a38";
            public const string NursingHomeGuid = "7818dfd9-e347-43b2-95e3-8fbf83ab962d";
            public const string HomeboundGuid = "3621645f-fbd0-4741-90ec-e032354aa375";

        }

        protected static class AttributeKey
        {
            public const string ShowVisitorPicker = "ShowVisitorPicker";
            public const string ViewablePastoralCareTypes = "ViewablePastoralCareTypes";
        }

        #region Fields
        bool? _showVisitorPicker = null;
        RockContext _rockContext = null;
        List<WorkflowTypeCache> _viewablePastoralCareTypes = null;
        const string HOSPITAL_DEFINED_TYPE_GUID = "0913f7a9-a2bf-479c-96ec-6cdb56310a83";
        const string NURSING_HOME_DEFINED_TYPE_GUID = "4573e600-4e00-4be9-ba92-d17093c735d6";
        const string WORKFLOW_ENTITY_TYPE_GUID = "3540e9a7-fe30-43a9-8b0a-a372b63dfc93";
        const string WORKFLOW_ACTIVITY_ENTITY_TYPE_GUID = "2cb52ed0-cb06-4d62-9e2c-73b60afa4c9f";



        #endregion

        #region Properties
        protected bool ShowVisitorPicker
        {
            get
            {
                if (!_showVisitorPicker.HasValue)
                {
                    _showVisitorPicker = GetAttributeValue(AttributeKey.ShowVisitorPicker).AsBoolean();
                }

                return _showVisitorPicker.Value;
            }
        }

        protected List<WorkflowTypeCache> ViewablePastoralCareTypes
        {
            get
            {
                if (_viewablePastoralCareTypes == null)
                {
                    _viewablePastoralCareTypes = LoadViewablePastoralCareTypes();
                }

                return _viewablePastoralCareTypes;
            }
        }


        #endregion

        #region Base Control Methods
        protected override void OnInit(EventArgs e)
        {
            _rockContext = new RockContext();
            base.OnInit(e);
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ClearInfoNotificationBox();

            nbWorkflowTypeWarning.Visible = false;
            nbDateRangeWarning.Visible = false;

            if (!IsPostBack)
            {
                ppVisitor.Visible = ShowVisitorPicker;
                LoadWorkflowTypeDropdown();
                LoadUserSettings();

                if (ValidateFilters())
                {
                    BindGrid();
                }
            }
        }
        #endregion

        #region Events
        protected void ddlWorkflowType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlWorkflowType.SelectedValue.IsNullOrWhiteSpace())
            {
                return;
            }

            ConfigureReport(ddlWorkflowType.SelectedValueAsGuid() ?? Guid.Empty);
            ValidateFilters();
        }

        protected void btnApply_Click(object sender, EventArgs e)
        {
            if (!ValidateFilters())
            {
                return;
            }

            BindGrid();
            SaveSettings();
        }

        protected void gVisits_GridRebind(object sender, GridRebindEventArgs e)
        {
            BindGrid();
        }

        #endregion

        #region Methods

        private void BindGrid()
        {
            using (RockContext rockContext = new RockContext())
            {
                var qry = GetQuery(rockContext);

                gVisits.DataSource = qry.OrderBy(q => q.VisitDate).ToList();
                gVisits.DataBind();
                pnlResults.Visible = true;
            }
        }

        private void ConfigureReport(Guid wfTypeGuid)
        {
            Guid definedTypeGuid = Guid.Empty;
            var facilityTitle = string.Empty;
            switch (wfTypeGuid.ToString())
            {
                case PastoralCareWorkflow.HospitalizationGuid:
                    definedTypeGuid = HOSPITAL_DEFINED_TYPE_GUID.AsGuid();
                    facilityTitle = "Hospital";
                    break;
                case PastoralCareWorkflow.NursingHomeGuid:
                    definedTypeGuid = NURSING_HOME_DEFINED_TYPE_GUID.AsGuid();
                    facilityTitle = "Nursing Home";
                    break;
                case PastoralCareWorkflow.HomeboundGuid:
                    dvpFacilityType.Visible = false;
                    gVisits.Columns[1].Visible = false;
                    return;
                default:
                    break;
            }

            dvpFacilityType.Visible = true;
            dvpFacilityType.Label = facilityTitle;
            dvpFacilityType.DefinedTypeId = DefinedTypeCache.Get(definedTypeGuid, _rockContext).Id;

            gVisits.Columns[1].Visible = true;
            gVisits.Columns[1].HeaderText = facilityTitle;

            gVisits.ExportTitleName = string.Format("{0} Visit History", facilityTitle);
        }

        private void ClearInfoNotificationBox()
        {
            nbInfo.Title = null;
            nbInfo.Text = null;
            nbInfo.NotificationBoxType = NotificationBoxType.Info;
            nbInfo.Visible = false;

        }

        private IQueryable<VisitHistoryItem> GetHospitalVisitQry()
        {
            var workflowType = WorkflowTypeCache.Get(PastoralCareWorkflow.HospitalizationGuid.AsGuid(), _rockContext);
            var workflowTypeIdAsString = workflowType.Id.ToString();

            var workflowEntityTypeId = EntityTypeCache.Get("3540e9a7-fe30-43a9-8b0a-a372b63dfc93".AsGuid(), _rockContext).Id;
            var workflowActivityEntityTypeId = EntityTypeCache.Get("2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F".AsGuid(), _rockContext).Id;

            var visitActivityType = workflowType.ActivityTypes.Where(a => a.Name == "Visitation Info").FirstOrDefault();
            var visitActivityTypeIdAsString = visitActivityType.Id.ToString();

            var hospitalDefinedTypeId = DefinedTypeCache.Get(HOSPITAL_DEFINED_TYPE_GUID.AsGuid(), _rockContext).Id;
            var attributeValueService = new AttributeValueService(_rockContext);
            var definedValueEntityTypeId = EntityTypeCache.Get("53D4BF38-C49E-4A52-8B0E-5E016FB9574E".AsGuid(), _rockContext).Id;



            var hospitalQry = new DefinedValueService(_rockContext).Queryable().AsNoTracking()
                .Where(v => v.DefinedTypeId == hospitalDefinedTypeId)
                .Join(attributeValueService.Queryable().AsNoTracking(),
                    v => v.Id,
                    av => av.EntityId,
                    (v, av) => new { DefinedValue = v, AttributeValue = av })
                .Where(av => av.AttributeValue.Attribute.EntityTypeId == definedValueEntityTypeId)
                .GroupBy(v => v.DefinedValue)
                .Select(v => new VisitHistoryFacility
                {
                    Id = v.Key.Id,
                    Name = v.Key.Value,
                    Guid = v.Key.Guid,
                    StreetAddress = v.FirstOrDefault(av => av.AttributeValue.Attribute.Key == "Qualifier1").AttributeValue.Value,
                    City = v.FirstOrDefault(av => av.AttributeValue.Attribute.Key == "Qualifier2").AttributeValue.Value,
                    State = v.FirstOrDefault(av => av.AttributeValue.Attribute.Key == "Qualifier3").AttributeValue.Value,
                    PostalCode = v.FirstOrDefault(av => av.AttributeValue.Attribute.Key == "Qualifier4").AttributeValue.Value
                });


            var personAliasQry = new PersonAliasService(_rockContext).Queryable().AsNoTracking();


            var workflowQry = new WorkflowService(_rockContext).Queryable().AsNoTracking()
                .Join(attributeValueService.Queryable().AsNoTracking(), w => w.Id, av => av.EntityId, (w, av) => new { Workflow = w, AttributeValue = av })
                .Where(w => w.Workflow.WorkflowTypeId == workflowType.Id)
                .Where(w => w.AttributeValue.Attribute.EntityTypeId == workflowEntityTypeId)
                .GroupBy(w => w.Workflow)
                .Select(w => new
                {
                    WorkflowId = w.Key.Id,
                    WorkflowName = w.Key.Name,
                    WorkflowStatus = w.Key.Status,
                    WorkflowComplete = w.Key.CompletedDateTime == null ? true : false,
                    FacilityGuid = w.Where(wa => wa.AttributeValue.Attribute.Key == "Hospital").Select(wa => wa.AttributeValue.Value).FirstOrDefault(),
                    PersonToVisitGuid = w.Where(wa => wa.AttributeValue.Attribute.Key == "PersonToVisit").Select(wa => wa.AttributeValue.Value).FirstOrDefault()
                })
                .Join(hospitalQry, w => w.FacilityGuid, h => h.Guid.ToString(),
                    (w, h) => new { Workflow = w, Hospital = h })
                .Join(personAliasQry,
                    w => w.Workflow.PersonToVisitGuid,
                    pa => pa.Guid.ToString(),
                    (w, pa) => new
                    {
                        w.Workflow.WorkflowId,
                        w.Workflow.WorkflowName,
                        w.Workflow.WorkflowStatus,
                        w.Workflow.WorkflowComplete,
                        w.Workflow.PersonToVisitGuid,
                        w.Hospital,
                        PersonId = pa.Person.Id,
                        pa.Person.LastName,
                        pa.Person.NickName,
                        PrimaryCampus = pa.Person.PrimaryCampus.Name
                    });

            var visitQry = new WorkflowActivityService(_rockContext).Queryable().AsNoTracking()
                .Join(attributeValueService.Queryable().AsNoTracking(), v => v.Id, av => av.EntityId, (v, av) => new { Visit = v, AttributeValue = av })
                .Where(v => v.Visit.ActivityTypeId == visitActivityType.Id)
                .Where(v => v.AttributeValue.Attribute.EntityTypeId == workflowActivityEntityTypeId)
                .GroupBy(v => v.Visit)
                .Select(v => new
                {
                    VisitId = v.Key,
                    WorkflowId = v.Key.WorkflowId,
                    VisitDate = v.Where(va => va.AttributeValue.Attribute.Key == "VisitDate").Select(va => va.AttributeValue.ValueAsDateTime).FirstOrDefault(),
                    VisitNote = v.Where(va => va.AttributeValue.Attribute.Key == "VisitNote").Select(va => va.AttributeValue.Value).FirstOrDefault(),
                    VisitorGuid = v.Where(va => va.AttributeValue.Attribute.Key == "Visitor").Select(va => va.AttributeValue.Value).FirstOrDefault()
                })
                .Join(personAliasQry,
                    v => v.VisitorGuid,
                    pa => pa.Guid.ToString(),
                    (v, pa) => new
                    {
                        v.VisitId,
                        v.WorkflowId,
                        v.VisitDate,
                        v.VisitNote,
                        v.VisitorGuid,
                        VisitorPersonId = pa.Person.Id,
                        VisitorLastName = pa.Person.LastName,
                        VisitorFirstName = pa.Person.NickName,
                    })
                .Join(workflowQry,
                    v => v.WorkflowId,
                    w => w.WorkflowId,
                    (v, w) => new VisitHistoryItem
                    {
                        WorkflowActivityId = v.WorkflowId,
                        WorkflowId = w.WorkflowId,
                        Facility = w.Hospital,
                        VisitDate = v.VisitDate,
                        VisitorPersonId = v.VisitorPersonId,
                        VisitorLastName = v.VisitorLastName,
                        VisitorFirstName = v.VisitorFirstName,
                        PersonVisitedPersonId = w.PersonId,
                        PersonVisitedFirstName = w.NickName,
                        PersonVisitedLastName = w.LastName,
                        VisitNotes = v.VisitNote
                    });

            return visitQry;

        }

        private IQueryable<VisitHistoryItem> GetNursingHomeVisitQry()
        {
            var workflowType = WorkflowTypeCache.Get(PastoralCareWorkflow.NursingHomeGuid.AsGuid(), _rockContext);
            var workflowTypeIdAsString = workflowType.Id.ToString();

            var workflowEntityTypeId = EntityTypeCache.Get("3540e9a7-fe30-43a9-8b0a-a372b63dfc93".AsGuid(), _rockContext).Id;
            var workflowActivityEntityTypeId = EntityTypeCache.Get("2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F".AsGuid(), _rockContext).Id;

            var visitActivityType = workflowType.ActivityTypes.Where(a => a.Name == "Visitation Info").FirstOrDefault();
            var visitActivityTypeIdAsString = visitActivityType.Id.ToString();

            var personAliasQry = new PersonAliasService(_rockContext).Queryable().AsNoTracking();

            var nursingHomeDefinedTypeId = DefinedTypeCache.Get(NURSING_HOME_DEFINED_TYPE_GUID.AsGuid(), _rockContext).Id;
            var attributeValueService = new AttributeValueService(_rockContext);
            var definedValueEntityTypeId = EntityTypeCache.Get("53D4BF38-C49E-4A52-8B0E-5E016FB9574E".AsGuid(), _rockContext).Id;

            var nursingHomeQry = new DefinedValueService(_rockContext).Queryable().AsNoTracking()
                .Where(v => v.DefinedTypeId == nursingHomeDefinedTypeId)
                .Join(attributeValueService.Queryable().AsNoTracking(),
                    v => v.Id,
                    av => av.EntityId,
                    (v, av) => new { DefinedValue = v, AttributeValue = av })
                .Where(av => av.AttributeValue.Attribute.EntityTypeId == definedValueEntityTypeId)
                .GroupBy(v => v.DefinedValue)
                .Select(v => new VisitHistoryFacility
                {
                    Id = v.Key.Id,
                    Name = v.Key.Value,
                    Guid = v.Key.Guid,
                    StreetAddress = v.FirstOrDefault(av => av.AttributeValue.Attribute.Key == "Qualifier1").AttributeValue.Value,
                    City = v.FirstOrDefault(av => av.AttributeValue.Attribute.Key == "Qualifier2").AttributeValue.Value,
                    State = v.FirstOrDefault(av => av.AttributeValue.Attribute.Key == "Qualifier3").AttributeValue.Value,
                    PostalCode = v.FirstOrDefault(av => av.AttributeValue.Attribute.Key == "Qualifier4").AttributeValue.Value
                });


            var workflowQry = new WorkflowService(_rockContext).Queryable().AsNoTracking()
                .Where(w => w.WorkflowTypeId == workflowType.Id)
                //.Join(workflowAttributeValues,
                //    w => w.Id,
                //    av => av.EntityId.Value,
                //    (w, av) => new { Workflow = w, AttributeValue = av })
                .Join(attributeValueService.Queryable().AsNoTracking(), w => w.Id, av => av.EntityId,
                    (w, av) => new { Workflow = w, AttributeValue = av })
                .Where(w => w.Workflow.WorkflowTypeId == workflowType.Id)
                .Where(w => w.AttributeValue.Attribute.EntityTypeId == workflowEntityTypeId)
                .GroupBy(w => w.Workflow)
                .Select(w => new
                {
                    WorkflowId = w.Key.Id,
                    WorkflowName = w.Key.Name,
                    WorkflowStatus = w.Key.Status,
                    WorkflowComplete = w.Key.CompletedDateTime == null ? true : false,
                    FacilityGuid = w.Where(wa => wa.AttributeValue.Attribute.Key == "NursingHome").Select(wa => wa.AttributeValue.Value).FirstOrDefault(),
                    PersonToVisitGuid = w.Where(wa => wa.AttributeValue.Attribute.Key == "PersonToVisit").Select(wa => wa.AttributeValue.Value).FirstOrDefault()
                })
                .Join(nursingHomeQry, w => w.FacilityGuid, h => h.Guid.ToString(),
                    (w, h) => new { Workflow = w, NursingHome = h })
                .Join(personAliasQry,
                    w => w.Workflow.PersonToVisitGuid,
                    pa => pa.Guid.ToString(),
                    (w, pa) => new
                    {
                        w.Workflow.WorkflowId,
                        w.Workflow.WorkflowName,
                        w.Workflow.WorkflowStatus,
                        w.Workflow.WorkflowComplete,
                        w.NursingHome,
                        w.Workflow.PersonToVisitGuid,
                        PersonId = pa.Person.Id,
                        pa.Person.LastName,
                        pa.Person.NickName,
                        PrimaryCampus = pa.Person.PrimaryCampus.Name
                    });


            var visitQry = new WorkflowActivityService(_rockContext).Queryable().AsNoTracking()
                .Join(attributeValueService.Queryable().AsNoTracking(), v => v.Id, av => av.EntityId,
                    (v, av) => new { Visit = v, AttributeValue = av })
                .Where(v => v.Visit.ActivityTypeId == visitActivityType.Id)
                .Where(av => av.AttributeValue.Attribute.EntityTypeId == workflowActivityEntityTypeId)
                .GroupBy(v => v.Visit)
                .Select(v => new
                {
                    VisitId = v.Key,
                    WorkflowId = v.Key.WorkflowId,
                    VisitDate = v.Where(va => va.AttributeValue.Attribute.Key == "VisitDate").Select(va => va.AttributeValue.ValueAsDateTime).FirstOrDefault(),
                    VisitNote = v.Where(va => va.AttributeValue.Attribute.Key == "VisitNote").Select(va => va.AttributeValue.Value).FirstOrDefault(),
                    VisitorGuid = v.Where(va => va.AttributeValue.Attribute.Key == "Visitor").Select(va => va.AttributeValue.Value).FirstOrDefault()
                })
                .Join(personAliasQry,
                    v => v.VisitorGuid,
                    pa => pa.Guid.ToString(),
                    (v, pa) => new
                    {
                        v.VisitId,
                        v.WorkflowId,
                        v.VisitDate,
                        v.VisitNote,
                        v.VisitorGuid,
                        VisitorPersonId = pa.Person.Id,
                        VisitorLastName = pa.Person.LastName,
                        VisitorFirstName = pa.Person.NickName,
                    })
                .Join(workflowQry,
                    v => v.WorkflowId,
                    w => w.WorkflowId,
                    (v, w) => new VisitHistoryItem
                    {
                        WorkflowActivityId = v.WorkflowId,
                        WorkflowId = w.WorkflowId,
                        Facility = w.NursingHome,
                        VisitDate = v.VisitDate,
                        VisitorPersonId = v.VisitorPersonId,
                        VisitorLastName = v.VisitorLastName,
                        VisitorFirstName = v.VisitorFirstName,
                        PersonVisitedPersonId = w.PersonId,
                        PersonVisitedFirstName = w.NickName,
                        PersonVisitedLastName = w.LastName,
                        VisitNotes = v.VisitNote
                    });




            return visitQry;

        }

        private IQueryable<VisitHistoryItem> GetQuery(RockContext rockContext)
        {
            IQueryable<VisitHistoryItem> qry = null;
            if (ddlWorkflowType.SelectedValue == PastoralCareWorkflow.HospitalizationGuid)
            {
                qry = GetHospitalVisitQry();
            }
            else if (ddlWorkflowType.SelectedValue == PastoralCareWorkflow.NursingHomeGuid)
            {
                qry = GetNursingHomeVisitQry();
            }

            if (ppVisitor.SelectedValue.HasValue)
            {
                qry = qry.Where(v => v.VisitorPersonId == ppVisitor.SelectedValue.Value);
            }

            if (dvpFacilityType.SelectedDefinedValuesId.Length > 0)
            {
                var facilityGuids = new List<Guid?>();
                foreach (var id in dvpFacilityType.SelectedValuesAsInt)
                {
                    facilityGuids.Add(DefinedValueCache.Get(id, _rockContext).Guid);
                }

                qry = qry.Where(v => facilityGuids.Contains(v.Facility.Guid));
            }

            var daterange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues(drpSlidingDateRange.DelimitedValues);
            var startDate = daterange.Start;
            var endDate = daterange.End;

            if (startDate.HasValue)
            {
                qry = qry.Where(v => v.VisitDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                qry = qry.Where(v => v.VisitDate <= endDate.Value);
            }

            return qry;

        }

        private void LoadUserSettings()
        {
            string keyPrefix = string.Format("pastoralVisit-reporting-{0}-", this.BlockId);

            var visitType = GetUserPreference(keyPrefix + "VisitType");
            if (visitType.IsNotNullOrWhiteSpace())
            {
                ddlWorkflowType.SelectedValue = visitType;
                ConfigureReport(visitType.AsGuid());
            }
            var dateRange = GetUserPreference(keyPrefix + "SlidingDateRange");
            if (dateRange.IsNullOrWhiteSpace())
            {
                drpSlidingDateRange.SlidingDateRangeMode = SlidingDateRangePicker.SlidingDateRangeType.Current;
                drpSlidingDateRange.TimeUnit = SlidingDateRangePicker.TimeUnitType.Month;
            }
            else
            {
                drpSlidingDateRange.DelimitedValues = dateRange;
            }

            var facility = GetUserPreference(keyPrefix + "Facilities");
            if (facility.IsNotNullOrWhiteSpace())
            {
                dvpFacilityType.SetValues(facility.SplitDelimitedValues().Select(v => v.AsInteger()).ToArray());
            }

            var visitor = GetUserPreference(keyPrefix + "Visitor").AsIntegerOrNull();
            if (visitor.HasValue)
            {
                ppVisitor.SelectedValue = visitor.Value;
            }
        }
        private List<WorkflowTypeCache> LoadViewablePastoralCareTypes()
        {
            var wtGuids = GetAttributeValue(AttributeKey.ViewablePastoralCareTypes).SplitDelimitedValues()
                .Select(v => v.AsGuid())
                .Where(v => v != Guid.Empty)
                .ToList();

            var wtList = new List<WorkflowTypeCache>();

            foreach (var wtGuid in wtGuids)
            {
                wtList.Add(WorkflowTypeCache.Get(wtGuid, _rockContext));
            }

            return wtList;
        }

        private void LoadWorkflowTypeDropdown()
        {
            if (ViewablePastoralCareTypes.Count == 0)
            {
                if (this.UserCanAdministrate)
                {
                    nbInfo.Title = "Configure Workflow Types";
                    nbInfo.Text = "At least one Pastoral Care Type must be viewable.  Pleasae check the \"Viewable Pastoral Care Types\" block setting.";
                    nbInfo.NotificationBoxType = NotificationBoxType.Warning;
                    nbInfo.Visible = true;
                }
                else
                {
                    nbInfo.Title = "Please contact administrator";
                    nbInfo.Text = "An error occurred, please contact the administrator.";
                    nbInfo.NotificationBoxType = NotificationBoxType.Danger;
                    nbInfo.Visible = false;
                }
                return;
            }

            ddlWorkflowType.Items.Clear();

            ddlWorkflowType.DataSource = ViewablePastoralCareTypes.OrderBy(v => v.Name).ToList();
            ddlWorkflowType.DataValueField = "Guid";
            ddlWorkflowType.DataTextField = "Name";
            ddlWorkflowType.DataBind();

            if (ViewablePastoralCareTypes.Count > 1)
            {
                ddlWorkflowType.Items.Insert(0, new ListItem("", ""));
            }
            else
            {
                ConfigureReport(ddlWorkflowType.SelectedValueAsGuid() ?? Guid.Empty);
            }

        }

        private void SaveSettings()
        {
            string keyPrefix = string.Format("pastoralVisit-reporting-{0}-", this.BlockId);

            this.SetUserPreference(keyPrefix + "VisitType", ddlWorkflowType.SelectedValue, false);
            this.SetUserPreference(keyPrefix + "SlidingDateRange", drpSlidingDateRange.DelimitedValues, false);
            this.SetUserPreference(keyPrefix + "Facilities", dvpFacilityType.SelectedValues.ToList().AsDelimited(","), false);
            this.SetUserPreference(keyPrefix + "Visitor", ppVisitor.SelectedValue.ToString(), false);

            this.SaveUserPreferences(keyPrefix);
        }

        private bool ValidateFilters()
        {
            bool areFiltersValid = true;

            if (ddlWorkflowType.SelectedValue.IsNullOrWhiteSpace())
            {
                nbWorkflowTypeWarning.Visible = true;
                areFiltersValid = false;
            }

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues(drpSlidingDateRange.DelimitedValues);
            if (!dateRange.Start.HasValue || !dateRange.End.HasValue)
            {
                nbDateRangeWarning.Visible = true;
                areFiltersValid = false;
            }

            return areFiltersValid;

        }


        #endregion

        protected class VisitHistoryFacility
        {
            public int Id { get; set; }
            public Guid Guid { get; set; }
            public string Name { get; set; }

            public string StreetAddress { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string PostalCode { get; set; }

        }


        protected class VisitHistoryItem
        {

            string _facilityAddress = null;
            public int WorkflowActivityId { get; set; }
            public int WorkflowId { get; set; }
            public VisitHistoryFacility Facility { get; set; }

            public string FacilityName
            {
                get
                {
                    if (Facility != null)
                    {
                        return Facility.Name;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            public string FacilityAddress
            {
                get
                {
                    if (_facilityAddress == null && Facility != null)
                    {
                        _facilityAddress = string.Concat(Facility.StreetAddress, ", ",
                            Facility.City, ", ",
                            Facility.State, " ",
                            Facility.PostalCode).Trim();

                    }

                    return _facilityAddress;
                }
            }

            public DateTime? VisitDate { get; set; }
            public string PersonVisitedFullName
            {
                get
                {
                    return string.Concat(PersonVisitedFirstName, " ", PersonVisitedLastName);
                }
            }

            public string PersonVisitedFullNameReversed
            {
                get
                {
                    return string.Concat(PersonVisitedLastName, ",", PersonVisitedFirstName);
                }
            }

            public string PersonVisitedLastName { get; set; }
            public string PersonVisitedFirstName { get; set; }

            public int PersonVisitedPersonId { get; set; }

            public int VisitorPersonId { get; set; }
            public string VisitorFullName
            {
                get
                {
                    return string.Concat(VisitorFirstName, " ", VisitorLastName);
                }

            }

            public string VisitorFullNameReversed
            {
                get
                {
                    return string.Concat(VisitorLastName, ",", VisitorFirstName);
                }
            }
            public string VisitorLastName { get; set; }
            public string VisitorFirstName { get; set; }
            public string VisitorLastFirst { get; set; }
            public string VisitNotes { get; set; }
        }




    }
}