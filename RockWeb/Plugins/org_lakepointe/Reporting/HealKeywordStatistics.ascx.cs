using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;


namespace RockWeb.Plugins.org_lakepointe.Reporting
{
    [DisplayName( "Heal Responses" )]
    [Category( "LPC > Reporting" )]
    [Description( "Reports on the number and type of responses that are received for the Heal keywords" )]


    [DateTimeField( "Minimum Date",
        Description = "The first date to check for responses",
        IsRequired = false,
        DefaultValue = "1/8/2021 18:00",
        Key = AttributeKey.MinimumDate,
        Order = 1 )]
    [TextField("Panel Title",
        Description = "The panel title",
        IsRequired = false,
        DefaultValue = "Heal Keyword Statistics",
        Order = 1,
        Key = AttributeKey.PanelTitle)]
    [WorkflowTypeField("Workflow Type",
        Description = "The workflow type that this keyword is assocated with.",
        AllowMultiple = false,
        IsRequired = false,
        DefaultValue = "C5359D5B-1339-4DE8-8635-8D61116AE4FA",
        Order = 2,
        Key = AttributeKey.WorkflowType )]
    [IntegerField("Activity Type Id",
        Description = "Activity Type Id of the Workflow Activity Type that contains the attribute to report from.",
        IsRequired = false,
        DefaultValue = "862",
        Order = 3,
        Key = AttributeKey.ActivityTypeId)]
    [TextField("Needs Attribute Key",
        Description = "Key to the Needs Activity Attribute",
        IsRequired = false,
        DefaultValue = "Ineedhelpwith",
        Order = 4,
        Key = AttributeKey.NeedsAttributeKey)]

    public partial class HealKeywordStatistics : RockBlock
    {
        public static class AttributeKey
        {
            public const string MinimumDate = "MinimumDate";
            public const string PanelTitle = "PanelTitle";
            public const string WorkflowType = "WorkflowType";
            public const string ActivityTypeId = "ActivityTypeId";
            public const string NeedsAttributeKey = "NeedsAttributeKey";

        }

        #region Fields

        #endregion

        #region Properties
        #endregion

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gfNeedTypes.ApplyFilterClick += gfNeedTypes_ApplyFilterClick;
            gfNeedTypes.ClearFilterClick += gfNeedTypes_ClearFilterClick;
            gfNeedTypes.DisplayFilterValue += gfNeedTypes_DisplayFilterValue;

            gNeedTypes.Actions.ShowAdd = false;
            gNeedTypes.Actions.ShowBulkUpdate = false;
            gNeedTypes.Actions.ShowCommunicate = false;
            gNeedTypes.Actions.ShowMergePerson = false;
            gNeedTypes.Actions.ShowMergeTemplate = false;

            gNeedTypes.IsDeleteEnabled = false;
            gNeedTypes.GridRebind += gNeedTypes_GridRebind;
            drpDates.LowerValue = GetAttributeValue( AttributeKey.MinimumDate ).AsDateTime();
            drpDates.UpperValue = RockDateTime.Today;
        }


        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            SetNotificationBox( null, NotificationBoxType.Info );

            if ( !IsPostBack )
            {
                lTitle.Text = GetAttributeValue( AttributeKey.PanelTitle );
                LoadUserPreferences();
                BindGrid();
            }
        }
        #endregion

        #region Events
        private void gfNeedTypes_ApplyFilterClick( object sender, EventArgs e )
        {
            SetUserPreferences();
            BindGrid();
        }

        private void gfNeedTypes_ClearFilterClick( object sender, EventArgs e )
        {
            gfNeedTypes.DeleteUserPreferences();

            LoadUserPreferences();
        }

        private void gfNeedTypes_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "DateRange":
                    e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                    break;
            }
        }

        private void gNeedTypes_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGrid();
        }
        #endregion

        #region Methods
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var activityTypeId = WorkflowActivityTypeCache.Get( GetAttributeValue(AttributeKey.ActivityTypeId).AsInteger(), rockContext ).Id;


            DateTime? startDate = GetAttributeValue( AttributeKey.MinimumDate ).AsDateTime();
            DateTime? endDate = null;

            if ( drpDates.LowerValue > startDate )
            {
                startDate = drpDates.LowerValue;
            }

            if ( drpDates.UpperValue > startDate )
            {
                endDate = drpDates.UpperValue;
            }

            var WorkflowActivityEntityType = EntityTypeCache.Get( typeof( WorkflowActivity ) );
            var attributeKey = GetAttributeValue( AttributeKey.NeedsAttributeKey );
            var activityTypeIdAsString = activityTypeId.ToString();

            var needsAttributeId = new AttributeService( rockContext ).Queryable()
                .Where( a => a.EntityTypeId == WorkflowActivityEntityType.Id )
                .Where( a => a.Key == attributeKey )
                .Where( a => a.EntityTypeQualifierColumn == "ActivityTypeId" )
                .Where( a => a.EntityTypeQualifierValue == activityTypeIdAsString )
                .Select(a => a.Id)
                .FirstOrDefault();




            var activitiesQry = new WorkflowActivityService( rockContext ).Queryable()
                .Where( a => a.ActivityTypeId == activityTypeId )
                .Where( a => a.Workflow.Status == "Completed" )
                .Where( a => a.Workflow.CompletedDateTime.HasValue );

            if ( startDate.HasValue )
            {
                activitiesQry = activitiesQry.Where( a => a.Workflow.ActivatedDateTime >= startDate.Value );
            }

            if ( endDate.HasValue )
            {
                var endDateEOD = endDate.Value.Date.AddDays( 1 );
                activitiesQry = activitiesQry.Where( a => a.Workflow.ActivatedDateTime < endDateEOD );
            }

            var activities = activitiesQry.ToList();

            var needSelections = new AttributeValueService( rockContext ).Queryable()
                .Where( v => v.AttributeId == needsAttributeId )
                .Where( v => v.Value != "" && v.Value != null )
                .Select( v => new { v.EntityId, v.Value } )
                .ToList()
                .Select( v => new { v.EntityId, Values = v.Value } )
                .ToList();

            var activityNeeds = activities
                .GroupJoin( needSelections, a => a.Id, n => n.EntityId,
                    ( a, n ) => new { ActivityId = a.Id, Needs = n.Select( n1 => n1.Values ) } )
                .Select( a => new { ActivityId = a.ActivityId, Needs = a.Needs.FirstOrDefault() } )
                .ToList();

            var results = new NeedResults();

            foreach ( var activityNeed in activityNeeds )
            {
                var needs = activityNeed.Needs.SplitDelimitedValues();
                results.SubmittedRequests += 1;

                if ( activityNeed.Needs.IsNullOrWhiteSpace() || needs.Length == 0 )
                {
                    results.AddNeed( Guid.Empty );
                    results.BlankRequests += 1;
                }
                if ( needs.Count() > 1 )
                {
                    results.MulipleSelections += 1;
                }

                foreach ( var need in needs )
                {
                    results.AddNeed( need.AsGuid() );
                }
            }

            lSubmissions.Text = results.SubmittedRequests.ToString();
            lMultipleItems.Text = results.MulipleSelections.ToString();
            lNoBoxesChecked.Text = results.BlankRequests.ToString();

            gNeedTypes.DataSource = results.SelectedNeeds;
            gNeedTypes.DataBind();

        }

        private void SetNotificationBox( string message, NotificationBoxType boxType )
        {
            nbWarning.Text = message;
            nbWarning.NotificationBoxType = boxType;
            nbWarning.Visible = message.IsNotNullOrWhiteSpace();
        }

        private void LoadUserPreferences()
        {
            drpDates.DelimitedValues = gfNeedTypes.GetUserPreference( "DateRange" );
        }

        private void SetUserPreferences()
        {
            gfNeedTypes.SaveUserPreference( "DateRange", drpDates.DelimitedValues );
        }

        #endregion
    }

    public class NeedResults
    {
        public NeedResults()
        {
            SelectedNeeds = new List<NeedItem>();
        }

        public int SubmittedRequests { get; set; }
        public int BlankRequests { get; set; }
        public int MulipleSelections { get; set; }

        public List<NeedItem> SelectedNeeds { get; set; }


        public void AddNeed( Guid g )
        {
            var selectedNeed = SelectedNeeds.Where( n => n.Guid == g ).SingleOrDefault();

            if ( selectedNeed == null )
            {
                if ( g == Guid.Empty )
                {
                    selectedNeed = new NeedItem( g, "No Selection" );
                }
                else
                {
                    var dv = DefinedValueCache.Get( g );
                    selectedNeed = new NeedItem( g, dv.Description );
                }

                SelectedNeeds.Add( selectedNeed );
            }

            selectedNeed.Count += 1;
        }
    }

    public class NeedItem
    {
        public NeedItem()
        {
            Count = 0;
        }

        public NeedItem( Guid guid, string description )
        {
            Guid = guid;
            Description = description;
            Count = 0;
        }

        public Guid Guid { get; set; }
        public string Description { get; set; }
        public int Count { get; set; }

    }
}