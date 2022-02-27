//
// Copyright (C) Pillars Inc. - All Rights Reserved
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
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.rocks_pillars.ServiceReservation
{
    [DisplayName( "Reservation Entry" )]
    [Category( "Pillars > Service Reservation" )]
    [Description( "Block for users to register for a specific service (location/schedule/date)." )]

    [SlidingDateRangeField( "Date Range", "The date range to use for available schedule occurrences. Only the schedule occurrences that fall within this date range will be available for reservations.", true, "Next|7|Day||", enabledSlidingDateRangeTypes: "DateRange,Next,Upcoming,Current", order: 0 )]
    [LinkedPage( "Workflow Entry Page", "The page to redirect user to once they've selected a service.", false, "", "", 1 )]
    [WorkflowTypeField( "Reservation Workflow", "The workflow type that is used to track registrations. This workflow must have the following attribute keys: Campus, Location, Schedule, Date, HowMany", false, true, "", "", 2 )]
    [LocationField( "Location Filter", "Optionally limit any reservations to only this location.", false, "", "", 3, "Location" )]
    [SchedulesField( "Schedules Filter", "Optionally limit any reservations to only these schedules.", false, "", "", 4, "Schedules" )]
    [TextField( "Location Order Attribute Key", "By default, locations will display in order by name. If you have a location attribute that should control order, enter that attribute key here.", false, "", "", 5 )]
    [BooleanField( "Single Schedule Skip", "True", "False", "If there is only one schedule available, the schedule selection screen will be skipped.", false, "", 6 )]
    [BooleanField( "Enable Campus Context", "True", "False", "If this is enabled it will get the Campus from the Context of the users browser", false, "", 7 )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for the card formats.", false, "", "", 8 )]

    #region Formatting Settings

    [TextField("Modal Title", "The title to use for an optional modal dialog that will be displayed when user first navigates to this block.", false, "", "Formatting", 0)]
    [TextField("Team Seating", "This adds a parameter to the page for Seating='true'", false, "false", "Formatting", 0)]
    [CodeEditorField("Modal Content", "The Modal Content to display when user first navigates to this block.", CodeEditorMode.Html, CodeEditorTheme.Rock, 300, false, @"
<p>Be sure to ask the following questions on behalf of your entire group before saving seats for this weekend:</p>
<ul>
    <li class='small'>Am I showing any flu-like symptoms?</li>
    <li class='small'>Have I had a fever of 100.4 or higher in the past 14 days?</li>
    <li class='small'>Have I had prolonged exposure to someone who has tested positive for COVID-19?</li>
    <li class='small'>Is there any medical reason why I believe I shouldn’t be around others?</li>
    <li class='small'>If you answered yes to any of these questions, we strongly encourage you to continue 
        engaging with our online service this weekend and consider coming back in-person at a later date.</li>
</ul>
<p align='center'><b>All good? Let’s go ahead and save your seat.</b></p>
<a class='btn btn-primary text-center center-block modal-btn' data-dismiss='modal'>I'm Ready</a>
", "Formatting", 1)]

    [CodeEditorField("Campus Caption", "The Caption to display above the campus list.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 50, false, "<h2 class='text-center'>Please select your campus</h2>", "Formatting", 2)]
    [CodeEditorField("Campus Card", "The Lava Template to use when displaying a campus.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 300, false, @"
{% assign percentRemaining = 100 %}
{% if Threshhold and Threshhold > 0 %}
    {% assign percentRemaining = SpotsLeft | DividedBy:Threshhold | Times:100 %} 
{% endif %}
<div class='reservation-card'>
    {% if Campus.Location.ImageUrl and Campus.Location.ImageUrl != '' %}
        <div class='card-image'>
            {% comment %}
                The Campus command was added because some properties were missing such as the image Id.
                We are not using the Location.ImageUrl property because sometimes the URL contains the wrong domain..
            {% endcomment %}
            {% campus id:'{{ Campus.Id }}' %}
                <img src='https://connect.passion.team/GetImage.ashx?Id={{ campus.Location.Image.Id }}&mode=crop&height=540' class='img img-responsive'>
            {% endcampus %}
            <!--<img src='{{ Campus.Location.ImageUrl }}&mode=crop&height=540' class='img img-responsive'>-->
        </div>
    {% else %}
        <div class='card-icon'>
            <i class='fa fa-building-o'></i>
        </div>
    {% endif %}
    <div class='card-details'>
        <h4>{{ Campus.Name }}</h4>
        {% if Campus.Location.Street1 and Campus.Location.Street1 != '' %}
            <h6>{{ Campus.Location.Street1 }}<br/>{{ Campus.Location.City }}, {{ Campus.Location.State }} {{ Campus.Location.Zipcode }}</h6>
        {% endif %}
        {% if SpotsLeft %}
            {% if percentRemaining >= 35 %}
                <span class='label label-success'>Spots Left: {{ SpotsLeft }}</span>
            {% elseif percentRemaining > 20 and percentRemaining < 35 %}
                <span class='label label-warning'>Spots Left: {{ SpotsLeft }}</span>
            {% else %}
                <span class='label label-danger'>{% if SpotsLeft >= 2 %}Spots Left: {{ SpotsLeft }}{% else %}Unavailable{% endif %}</span>
            {% endif %}
        {% endif %}
    </div>
</div>
", "Formatting", 3)]

    [CodeEditorField( "Date Caption", "The Caption to display above the date list.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 50, false, "<h2 class='text-center'>Please select a date for {{ Campus.Name }}</h2>", "Formatting", 4 )]
    [CodeEditorField( "Date Card", "The Lava Template to use when displaying a date.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 300, false, @"
{% assign percentRemaining = 100 %}
{% if Threshhold and Threshhold > 0 %}
    {% assign percentRemaining = SpotsLeft | DividedBy:Threshhold | Times:100 %} 
{% endif %}
<div class='reservation-card'>
    <div class='card-icon'>
        <i class='fa fa-calendar'></i>
    </div>
    <div class='card-details'>
        <h4>{{ Date | Date:'dddd MMM' }} {{ Date | Date:'d' | NumberToOrdinal }}</h4>
        {% if SpotsLeft %}
            {% if percentRemaining >= 35 %}
                <span class='label label-success'>Spots Left: {{ SpotsLeft }}</span>
            {% elseif percentRemaining > 20 and percentRemaining < 35 %}
                <span class='label label-warning'>Spots Left: {{ SpotsLeft }}</span>
            {% else %}
                <span class='label label-danger'>{% if SpotsLeft >= 2 %}Spots Left: {{ SpotsLeft }}{% else %}Unavailable{% endif %}</span>
            {% endif %}
        {% endif %}
    </div>
</div>
", "Formatting", 5 )]

    [CodeEditorField( "Location Caption", "The Caption to display above the campus list.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 50, false, "<h2 class='text-center'>Please select a location for {{ Campus.Name }} on {{ Date | Date:'dddd MMM' }} {{ Date | Date:'d' | NumberToOrdinal }}</h2>", "Formatting", 6 )]
    [CodeEditorField( "Location Card", "The Lava Template to use when displaying a location.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 300, false, @"
{% assign percentRemaining = 100 %}
{% if Threshhold and Threshhold > 0 %}
    {% assign percentRemaining = SpotsLeft | DividedBy:Threshhold | Times:100 %} 
{% endif %}
<div class='reservation-card'>       
    {% assign imgId = Location.ImageId %}
    {% if imgId and imgId != 0 %}
        <div class='img-hover'>
            <img src='https://connect.passion.team/GetImage.ashx?id={{ imgId }}&mode=crop&height=540' class='card-image img-responsive'>
        </div>
    {% else %}
        <div class='card-icon'>
            <i class='fa fa-map-marker'></i>
        </div>
    {% endif %}
    <div class='card-details'>
        <h4>{{ Location.Name }}</h4>
        {% if SpotsLeft %}
            {% if percentRemaining >= 35 %}
                <span class='label label-success'>Spots Left: {{ SpotsLeft }}</span>
            {% elseif percentRemaining > 20 and percentRemaining < 35 %}
                <span class='label label-warning'>Spots Left: {{ SpotsLeft }}</span>
            {% else %}
                <span class='label label-danger'>{% if SpotsLeft >= 2 %}Spots Left: {{ SpotsLeft }}{% else %}Unavailable{% endif %}</span>
            {% endif %}
        {% endif %}
    </div>
</div>
", "Formatting", 7 )]

    [CodeEditorField( "Schedule Caption", "The Caption to display above the campus list.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 50, false, "<h2 class='text-center'>Please select a time for {{ Campus.Name }} on {{ Date | Date:'dddd MMM' }} {{ Date | Date:'d' | NumberToOrdinal }} in {{ Location.Name }}</h2>", "Formatting", 8 )]
    [CodeEditorField( "Schedule Card", "The Lava Template to use when displaying a schedule.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 300, false, @"
{% assign percentRemaining = 100 %}
{% if Threshhold and Threshhold > 0 %}
    {% assign percentRemaining = SpotsLeft | DividedBy:Threshhold | Times:100 %} 
{% endif %}
<div class='reservation-card'>       
    <div class='card-icon'>
        <i class='fa fa-clock'></i>
    </div>
    <div class='card-details'>
        <h4>{{ Schedule.Name }}</h4>
        {% if SpotsLeft %}
            {% if percentRemaining >= 35 %}
                <span class='label label-success'>Spots Left: {{ SpotsLeft }}</span>
            {% elseif percentRemaining > 20 and percentRemaining < 35 %}
                <span class='label label-warning'>Spots Left: {{ SpotsLeft }}</span>
            {% else %}
                <span class='label label-danger'>{% if SpotsLeft >= 2 %}Spots Left: {{ SpotsLeft }}{% else %}Unavailable{% endif %}</span>
            {% endif %}
        {% endif %}
    </div>
</div>
", "Formatting", 9)]

    [CodeEditorField( "Empty Content", "Message to display when there is not any reservation options", CodeEditorMode.Html, CodeEditorTheme.Rock, 300, false, "", "Formatting", 10 )]

    [TextField("Card Item Class", "Class to use on the div element that wraps each card item.", false, "col-md-4 col-sm-6 col-xs-12", "Formatting", 11 )]
    [CodeEditorField("CSS Styles", "CSS Styles used by block", CodeEditorMode.Html, CodeEditorTheme.Rock, 300, false, @"
    .reservation-card {
        background-color: #FFFFFF;
        margin: 15px;
        -webkit-box-shadow: 0px 0px 15px 1px rgba(0, 0, 0, 0.15);
        box-shadow: 0px 0px 15px 1px rgba(0, 0, 0, 0.15);
        -webkit-transition: all 0.3s ease;
        -o-transition: all 0.3s ease;
        transition: all 0.3s ease;
        text-align: center;
        touch-action: manipulation;
        cursor: pointer;
        border-radius: 4px;
        padding: 6px 12px;
        -webkit-filter: grayscale(100%);
        filter: grayscale(100%);
        transition: 200ms linear all;
    }

    .reservation-card:hover {
        -webkit-box-shadow: 0 8px 21px rgba(0, 0, 0, 0.45);
        box-shadow: 0 8px 21px rgba(0, 0, 0, 0.45);
        transform: translateY(-10px);
        -webkit-transition: all 0.3s ease;
        -o-transition: all 0.3s ease;
        transition: all 0.3s ease;
        filter: none;
    }

    .modal-scrollable {
        overflow-y: auto !important;
    }

    .modal-footer {
        display: none;
    }

    .modal-btn {
            width: 85%;
            border-radius: 25px;
    }

    .card-image {
        margin: 10px;
    }

    .card-image .img-responsive {
        margin: 0 auto;
        border-radius: 4px;
    }

    .card-details {
        min-height: 100px;
    }

    .card-icon > .fa {
        font-size: 40px;
        margin-top: 10px;
    }
", "Formatting", 12 )]

    #endregion

    public partial class ReservationEntry : Rock.Web.UI.RockBlock
    {

        #region Fields

        private List<OptionItem> _options = new List<OptionItem>();
        private Dictionary<string, object> _commonMergeFields = new Dictionary<string, object>();
        private string _schedulesAttrKey = string.Empty;
        private string _locationOrderAttrKey = string.Empty;
        private string _enabledLavaCommands = string.Empty;

        #endregion

        protected string CardItemClass { get; set; }

        #region Properties

        public bool CampusSelected
        {
            get { return ViewState["CampusSelected"] as bool? ?? false; }
            set { ViewState["CampusSelected"] = value; }
        }

        public bool DateSelected
        {
            get { return ViewState["DateSelected"] as bool? ?? false; }
            set { ViewState["DateSelected"] = value; }
        }

        public bool LocationSelected
        {
            get { return ViewState["LocationSelected"] as bool? ?? false; }
            set { ViewState["LocationSelected"] = value; }
        }

        public Dictionary<String, string> QryParameters
        {
            get { return ViewState["QryParameters"] as Dictionary<string, string> ?? new Dictionary<string, string>(); }
            set { ViewState["QryParameters"] = value; }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rptrCards.ItemCommand += RptrCards_ItemCommand;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            var cssStyle = GetAttributeValue( "CSSStyles" );
            if ( cssStyle.IsNotNullOrWhiteSpace() )
            {
                Page.Header.Controls.Add( new LiteralControl( string.Format( "{1}<style>{0}</style>{1}", cssStyle, Environment.NewLine ) ) );
            }

            CardItemClass = GetAttributeValue( "CardItemClass" );
            _locationOrderAttrKey = GetAttributeValue( "LocationOrderAttributeKey" );
            _enabledLavaCommands = GetAttributeValue( "EnabledLavaCommands" );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            mdWarning.Hide();

            LoadData();

            if ( !Page.IsPostBack )
            {
                if ( _options.Any() )
                {
                    var modalTitle = GetAttributeValue( "ModalTitle" ).ResolveMergeFields( _commonMergeFields, _enabledLavaCommands );
                    var modalContent = GetAttributeValue( "ModalContent" ).ResolveMergeFields( _commonMergeFields, _enabledLavaCommands );
                    if ( modalTitle.IsNotNullOrWhiteSpace() && modalContent.IsNotNullOrWhiteSpace() )
                    {
                        mdWarning.Title = modalTitle;
                        mdWarning.Content.Controls.Add( new LiteralControl( modalContent ) );
                        //mdWarning.Show();
                    }

                    BindCampuses();
                }
                else
                {
                    lCardCaption.Text = GetAttributeValue( "EmptyContent" ).ResolveMergeFields( _commonMergeFields, _enabledLavaCommands );
                }
            }
            else
            {
                // Save Page Parameters
                QryParameters = new Dictionary<string, string>();
                foreach ( string key in PageParameters().Select( p => p.Key ).ToList() )
                {
                    if ( !key.Equals( "PageId", StringComparison.OrdinalIgnoreCase ) )
                    {
                        QryParameters.Add( key, PageParameter( key ) );
                    }
                }
            }
        }

        #endregion

        #region Events

        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindCampuses();
        }

        private void RptrCards_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var datetime = e.CommandArgument.ToString().AsDateTime();
            if ( datetime.HasValue )
            {
                DateSelected = true;
                hfDate.Value = datetime.Value.ToISO8601DateString();
                BindLocations();
                return;
            }

            var guid = e.CommandArgument.ToString().AsGuidOrNull();

            if ( guid.HasValue )
            {
                var campus = CampusCache.Get( guid.Value );
                if ( campus != null )
                {
                    CampusSelected = true;
                    hfCampusGuid.Value = e.CommandArgument.ToString();
                    BindDates();
                    return;
                }

                using ( var rockContext = new RockContext() )
                {
                    var location = new LocationService( rockContext ).Get( guid.Value );
                    if ( location != null )
                    {
                        LocationSelected = true;
                        hfLocationGuid.Value = e.CommandArgument.ToString();
                        BindSchedules();
                        return;
                    }

                    var schedule = new ScheduleService( rockContext ).Get( guid.Value );
                    if ( schedule != null )
                    {
                        var workflowType = new WorkflowTypeService( rockContext ).Get( GetAttributeValue( "ReservationWorkflow" ).AsGuid() );
                        var campusGuid = hfCampusGuid.Value.AsGuid();
                        var locationGuid = hfLocationGuid.Value.AsGuid();
                        var date = hfDate.Value.AsDateTime();

                        var option = _options
                            .Where( o =>
                                o.CampusGuid == campusGuid &&
                                o.DateTime == date &&
                                o.LocationGuid == locationGuid &&
                                o.ScheduleGuid == schedule.Guid )
                            .FirstOrDefault();

                        if ( workflowType != null && option != null && ( !option.SpotsLeft.HasValue || option.SpotsLeft.Value >= 1 ) )
                        {
                            var pageParams = new Dictionary<string, string>( QryParameters );
                            var teamSeating = GetAttributeValue("TeamSeating");
                            pageParams.Add( "Campus", hfCampusGuid.Value );
                            pageParams.Add( "Date", hfDate.Value );
                            pageParams.Add( "Location", hfLocationGuid.Value );
                            pageParams.Add( "Schedule", e.CommandArgument.ToString() );
                            pageParams.Add("Seating", teamSeating);
                            NavigateToLinkedPage( "WorkflowEntryPage", pageParams );
                        }
                    }
                }
            }
        }


        protected void lBack_Click( object sender, EventArgs e )
        {
            if ( LocationSelected )
            {
                BindLocations();
                return;
            }

            if ( DateSelected )
            {
                BindDates();
                return;
            }

            BindCampuses();
        }

        #endregion

        #region Methods

        private void LoadData()
        {
            _commonMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );

            var schedulesAttribute = AttributeCache.Get( rocks.pillars.ServiceReservation.SystemGuid.Attribute.LOCATION_SCHEDULES );
            if ( schedulesAttribute == null )
            {
                throw new Exception( "The Location Schedules attribute was not defined." );
            }
            _schedulesAttrKey = schedulesAttribute.Key;

            var validLocation = GetAttributeValue( "Location" ).AsGuidOrNull();
            var validSchedules = GetAttributeValue( "Schedules" ).SplitDelimitedValues().AsGuidList();

            using ( var rockContext = new RockContext() )
            {
                // Get the unique schedules
                var scheduleDates = new Dictionary<Guid, List<DateTime>>();

                foreach ( var attributeValue in new AttributeValueService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( v => v.AttributeId == schedulesAttribute.Id )
                    .Select( v => v.Value ) )
                {
                    foreach ( var scheduleGuid in attributeValue.SplitDelimitedValues().AsGuidList() )
                    {
                        if ( !validSchedules.Any() || validSchedules.Contains( scheduleGuid ) )
                        {
                            scheduleDates.AddOrIgnore( scheduleGuid, new List<DateTime>() );
                        }
                    }
                }

                // Find all of the upcoming occurrences for these schedules
                var dateRangeDelimitedValues = this.GetAttributeValue( "DateRange" );
                var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( dateRangeDelimitedValues );
                var start = dateRange.Start ?? RockDateTime.Now;
                var end = dateRange.End ?? RockDateTime.Now.AddDays( 6 );

                var scheduleGuids = scheduleDates.Keys.ToList();
                foreach ( var schedule in new ScheduleService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( s => scheduleGuids.Contains( s.Guid ) ) )
                {
                    var dates = new List<DateTime>();
                    foreach ( var occurrence in schedule.GetICalOccurrences( start, end ) )
                    {
                        dates.Add( occurrence.Period.StartTime.Value.Date );
                    }
                    scheduleDates[schedule.Guid] = dates;
                }


                var locationService = new LocationService( rockContext );

                // Get all campuses
                var campuses = CampusCache.All();

                // If campus parameter was specified, limit to that campus
                int? campusId = PageParameter( "CampusId" ).AsIntegerOrNull();
                if ( !campusId.HasValue )
                {
                    if ( GetAttributeValue( "EnableCampusContext" ).AsBoolean() )
                    {
                        // Check for campus context 
                        var campusEntityType = EntityTypeCache.Get( "Rock.Model.Campus" );
                        var contextCampus = RockPage.GetCurrentContext( campusEntityType ) as Campus;
                        if ( contextCampus != null )
                        {
                            campusId = contextCampus.Id;
                        }
                    }
                }

                if ( campusId.HasValue )
                {
                    var validCampus = CampusCache.Get( campusId.Value );
                    if ( validCampus != null )
                    {
                        campuses = campuses.Where( c => c.Id == campusId.Value ).ToList();
                    }
                }

                foreach ( var campus in campuses )
                {
                    if ( ( campus.IsActive ?? true ) && campus.LocationId.HasValue )
                    {
                        var campusLocation = locationService.Get( campus.LocationId.Value );
                        if ( campusLocation != null )
                        {
                            var locations = locationService.GetAllDescendents( campus.LocationId.Value ).ToList();
                            locations.Add( campusLocation );

                            foreach ( var location in locations )
                            {
                                if ( !validLocation.HasValue || location.Guid == validLocation.Value )
                                {
                                    location.LoadAttributes( rockContext );
                                    foreach ( var scheduleGuid in location.GetAttributeValue( _schedulesAttrKey ).SplitDelimitedValues().AsGuidList() )
                                    {
                                        if ( scheduleDates.ContainsKey( scheduleGuid ) )
                                        {
                                            foreach ( var datetime in scheduleDates[scheduleGuid] )
                                            {
                                                var option = new OptionItem
                                                {
                                                    CampusGuid = campus.Guid,
                                                    LocationGuid = location.Guid,
                                                    ScheduleGuid = scheduleGuid,
                                                    DateTime = datetime,
                                                    Threshhold = location.FirmRoomThreshold,
                                                    HowMany = 0
                                                };
                                                _options.Add( option );
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                var validDates = _options.Select( o => o.DateTime ).Distinct().ToList();

                var workflowEntityTypeId = EntityTypeCache.GetId( "Rock.Model.Workflow" );
                var workflowType = new WorkflowTypeService( rockContext ).Get( GetAttributeValue( "ReservationWorkflow" ).AsGuid() );
                if ( workflowEntityTypeId.HasValue && workflowType != null )
                {
                    var qualifierValue = workflowType.Id.ToString();
                    var keys = new List<string> { "Campus", "Location", "Schedule", "Date", "HowMany" };
                    var attributes = new AttributeService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( a =>
                            a.EntityTypeId == workflowEntityTypeId.Value &&
                            a.EntityTypeQualifierColumn == "WorkflowTypeId" &&
                            a.EntityTypeQualifierValue == qualifierValue &&
                            keys.Contains( a.Key ) )
                        .ToDictionary( k => k.Key, v => v.Id );
                    if ( attributes.Count == 5 )
                    {
                        var attributeValueService = new AttributeValueService( rockContext );

                        var campusAttrId = attributes["Campus"];
                        var locationAttrId = attributes["Location"];
                        var scheduleAttrId = attributes["Schedule"];
                        var dateAttrId = attributes["Date"];
                        var howManyAttrId = attributes["HowMany"];

                        var campusQry = attributeValueService.Queryable().AsNoTracking().Where( v => v.AttributeId == campusAttrId );
                        var locationQry = attributeValueService.Queryable().AsNoTracking().Where( v => v.AttributeId == locationAttrId );
                        var scheduleQry = attributeValueService.Queryable().AsNoTracking().Where( v => v.AttributeId == scheduleAttrId );
                        var howManyQry = attributeValueService.Queryable().AsNoTracking().Where( v => v.AttributeId == howManyAttrId );
                        var dateQry = attributeValueService.Queryable().AsNoTracking().Where( v => v.AttributeId == dateAttrId && v.ValueAsDateTime.HasValue && validDates.Contains( v.ValueAsDateTime.Value ) );

                        var counts = new WorkflowService( rockContext )
                            .Queryable().AsNoTracking()
                            .Select( w => w.Id )
                            .Join( dateQry, w => w, d => d.EntityId, ( w, d ) => new {  Id = w, Date = d.ValueAsDateTime } )
                            .Join( campusQry, w => w.Id, c => c.EntityId, ( w, c ) => new { w.Id, w.Date, CampusValue = c.Value } )
                            .Join( locationQry, w => w.Id, l => l.EntityId, ( w, l ) => new { w.Id, w.Date, w.CampusValue, LocationValue = l.Value } )
                            .Join( scheduleQry, w => w.Id, s => s.EntityId, ( w, s ) => new { w.Id, w.Date, w.CampusValue, w.LocationValue, ScheduleValue = s.Value } )
                            .Join( howManyQry, w => w.Id, h => h.EntityId, ( w, h ) => new { w.Id, w.Date, w.CampusValue, w.LocationValue, w.ScheduleValue, HowMany = h.ValueAsNumeric } )
                            .ToList()
                            .Select( w => new
                            {
                                w.Id,
                                w.Date,
                                Campus = w.CampusValue.AsGuid(),
                                Location = w.LocationValue.AsGuid(),
                                Schedule = w.ScheduleValue.AsGuid(),
                                HowMany = Decimal.ToInt32(w.HowMany ?? 0)
                            } )
                            .ToList();

                        foreach ( var option in _options )
                        {
                            option.HowMany = counts
                                .Where( c =>
                                    c.Campus == option.CampusGuid &&
                                    c.Location == option.LocationGuid &&
                                    c.Schedule == option.ScheduleGuid &&
                                    c.Date == option.DateTime )
                                .Select( c => c.HowMany )
                                .Sum();
                            if ( option.Threshhold.HasValue )
                            {
                                option.SpotsLeft = option.Threshhold.Value >= option.HowMany ? option.Threshhold.Value - option.HowMany : 0;
                            }
                        }
                    }
                }
            }
        }

        private void BindCampuses()
        {
            CampusSelected = false;
            LocationSelected = false;

            hfCampusGuid.Value = string.Empty;
            hfLocationGuid.Value = string.Empty;

            lBack.Visible = false;

            var validKeys = _options
                .Select( o => o.CampusGuid )
                .Distinct()
                .ToList();

            var campuses = new List<CampusCache>();

            foreach( var campus in CampusCache.All() )
            {
                if ( validKeys.Contains( campus.Guid ) )
                {
                    campuses.Add( campus );
                }
            }

            if ( campuses.Any() )
            {
                if ( campuses.Count == 1 )
                {
                    hfCampusGuid.Value = campuses.First().Guid.ToString();
                    BindDates();
                }
                else
                {
                    lCardCaption.Text = GetAttributeValue( "CampusCaption" ).ResolveMergeFields( _commonMergeFields, _enabledLavaCommands );
                    rptrCards.DataSource = campuses.Select( c => new CardItem( c, _options ) ).OrderBy( i => i.Order );
                    rptrCards.DataBind();
                }
            }
            else
            {

            }

        }

        private void BindDates()
        {
            DateSelected = false;
            hfDate.Value = string.Empty;
            lBack.Visible = CampusSelected;

            using ( var rockContext = new RockContext() )
            {
                var campus = CampusCache.Get( hfCampusGuid.Value.AsGuid() );

                if ( campus != null )
                {
                    var validOptions = _options
                        .Where( o => o.CampusGuid == campus.Guid )
                        .ToList();

                    var validKeys = validOptions
                        .Select( o => o.DateTime )
                        .Distinct()
                        .ToList();

                    if ( validKeys.Any() )
                    {
                        if ( validKeys.Count == 1 )
                        {
                            hfDate.Value = validKeys.First().ToString().AsDateTime().ToISO8601DateString();
                            BindLocations();
                        }
                        else
                        {
                            var mergeFields = new Dictionary<string, object>( _commonMergeFields );
                            mergeFields.Add( "Campus", campus );
                            lCardCaption.Text = GetAttributeValue( "DateCaption" ).ResolveMergeFields( mergeFields, _enabledLavaCommands );

                            rptrCards.DataSource = validKeys.Select( d => new CardItem( d, validOptions ) ).OrderBy( i => i.Order );
                            rptrCards.DataBind();
                        }
                    }
                }
            }
        }

        private void BindLocations()
        {
            LocationSelected = false;
            hfLocationGuid.Value = string.Empty;
            lBack.Visible = CampusSelected || DateSelected;

            using ( var rockContext = new RockContext() )
            {
                var campus = CampusCache.Get( hfCampusGuid.Value.AsGuid() );
                var date = hfDate.Value.AsDateTime();

                if ( campus != null && date.HasValue )
                {
                    var validOptions = _options
                        .Where( o =>
                            o.CampusGuid == campus.Guid &&
                            o.DateTime == date.Value )
                        .ToList();

                    var validKeys = validOptions
                        .Select( o => o.LocationGuid )
                        .Distinct()
                        .ToList();

                    var locations = new LocationService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( l => validKeys.Contains( l.Guid ) )
                        .ToList();

                    if ( locations.Any() )
                    {
                        if ( locations.Count == 1 )
                        {
                            hfLocationGuid.Value = locations.First().Guid.ToString();
                            BindSchedules();
                        }
                        else
                        {
                            var mergeFields = new Dictionary<string, object>( _commonMergeFields );
                            mergeFields.Add( "Campus", campus );
                            mergeFields.Add( "Date", date );
                            lCardCaption.Text = GetAttributeValue( "LocationCaption" ).ResolveMergeFields( mergeFields, _enabledLavaCommands );

                            var cardItems = new List<CardItem>();
                            foreach( var location in locations )
                            {
                                var cardItem = new CardItem( location, validOptions );
                                if ( _locationOrderAttrKey.IsNotNullOrWhiteSpace() )
                                {
                                    location.LoadAttributes();
                                    cardItem.Order = location.GetAttributeValue( _locationOrderAttrKey ).AsInteger();
                                }
                                else
                                {
                                    cardItem.Order = location.Name;
                                }
                                cardItems.Add( cardItem );
                            }

                            rptrCards.DataSource = cardItems.OrderBy( i => i.Order );
                            rptrCards.DataBind();
                        }
                    }
                }
            }
        }


        private void BindSchedules()
        {
            lBack.Visible = CampusSelected || LocationSelected;

            using ( var rockContext = new RockContext() )
            {
                var campus = CampusCache.Get( hfCampusGuid.Value.AsGuid() );
                var date = hfDate.Value.AsDateTime();
                var location = new LocationService( rockContext ).Get( hfLocationGuid.Value.AsGuid() );

                if ( campus != null && location != null )
                {
                    var validOptions = _options
                        .Where( o =>
                            o.CampusGuid == campus.Guid &&
                            o.DateTime == date.Value &&
                            o.LocationGuid == location.Guid )
                        .ToList();

                    var validKeys = validOptions
                        .Select( o => o.ScheduleGuid )
                        .Distinct()
                        .ToList();

                    var schedules = new ScheduleService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( s => validKeys.Contains( s.Guid ) )
                        .ToList();

                    var mergeFields = new Dictionary<string, object>( _commonMergeFields );
                    mergeFields.Add( "Campus", campus );
                    mergeFields.Add( "Date", date );
                    mergeFields.Add( "Location", location );
                    lCardCaption.Text = GetAttributeValue( "ScheduleCaption" ).ResolveMergeFields( mergeFields, _enabledLavaCommands );

                    if ( schedules.Count == 1 && GetAttributeValue( "SingleScheduleSkip" ).AsBoolean() )
                    {
                        var workflowType = new WorkflowTypeService( rockContext ).Get( GetAttributeValue( "ReservationWorkflow" ).AsGuid() );

                        var campusGuid = hfCampusGuid.Value.AsGuid();
                        var locationGuid = hfLocationGuid.Value.AsGuid();
                        var scheduleGuid = schedules.FirstOrDefault().Guid;

                        var option = _options
                            .Where( o =>
                                o.CampusGuid == campusGuid &&
                                o.DateTime == date &&
                                o.LocationGuid == locationGuid &&
                                o.ScheduleGuid == scheduleGuid )
                            .FirstOrDefault();

                        if ( workflowType != null && option != null && ( !option.SpotsLeft.HasValue || option.SpotsLeft.Value >= 1 ) )
                        {
                            var pageParams = new Dictionary<string, string>();
                            var teamSeating = GetAttributeValue("TeamSeating");
                            pageParams.Add( "Campus", hfCampusGuid.Value );
                            pageParams.Add( "Date", hfDate.Value );
                            pageParams.Add( "Location", hfLocationGuid.Value );
                            pageParams.Add( "Schedule", scheduleGuid.ToString() );
                            pageParams.Add("Seating", teamSeating);
                            NavigateToLinkedPage( "WorkflowEntryPage", pageParams );
                        }
                        else
                        {
                            rptrCards.DataSource = schedules.Select( s => new CardItem( s, validOptions, date ) ).OrderBy( i => i.Order );
                            rptrCards.DataBind();
                        }
                    }
                    else
                    {
                        rptrCards.DataSource = schedules.Select( s => new CardItem( s, validOptions, date ) ).OrderBy( i => i.Order );
                        rptrCards.DataBind();
                    }
                }
            }
        }

        protected string FormatCard( object item )
        {
            var mergeFields = new Dictionary<string, object>( _commonMergeFields );

            var cardItem = item as CardItem;
            if ( cardItem != null )
            {
                mergeFields.Add( "Threshhold", cardItem.Threshhold );
                mergeFields.Add( "HowMany", cardItem.HowMany );
                mergeFields.Add( "SpotsLeft", cardItem.SpotsLeft );

                mergeFields.Add( "SelectedCampusGuid", hfCampusGuid.Value );
                mergeFields.Add( "SelectedDate", hfDate.Value );
                mergeFields.Add( "SelectedLocationGuid", hfLocationGuid.Value );

                var campus = cardItem.Item as CampusCache;
                if ( campus != null )
                {
                    mergeFields.Add( "Campus", campus );
                    return GetAttributeValue( "CampusCard" ).ResolveMergeFields( mergeFields, _enabledLavaCommands );
                }

                var dateItem = cardItem.Item as DateTime?;
                if ( dateItem.HasValue )
                {
                    mergeFields.Add( "Date", dateItem.Value );
                    return GetAttributeValue( "DateCard" ).ResolveMergeFields( mergeFields, _enabledLavaCommands );
                }

                var location = cardItem.Item as Location;
                if ( location != null )
                {
                    mergeFields.Add( "Location", location );
                    return GetAttributeValue( "LocationCard" ).ResolveMergeFields( mergeFields, _enabledLavaCommands );
                }

                var schedule = cardItem.Item as Schedule;
                if ( schedule != null )
                {
                    mergeFields.Add( "Schedule", schedule );
                    return GetAttributeValue( "ScheduleCard" ).ResolveMergeFields( mergeFields, _enabledLavaCommands );
                }
            }

            return string.Empty;
        }

        #endregion

        #region Helper Classes

        public class OptionItem
        {
            public Guid CampusGuid { get; set; }
            public Guid LocationGuid { get; set; }
            public Guid ScheduleGuid { get; set; }
            public DateTime DateTime { get; set; }
            public int? Threshhold { get; set; }
            public int HowMany { get; set; }
            public int? SpotsLeft { get; set; }
        }

        public class CardItem
        {
            public string Key { get; set; }
            public object Item { get; set; }
            public object Order { get; set; }
            public int? Threshhold { get; set; }
            public int HowMany { get; set; }
            public int? SpotsLeft { get; set; }

            private void SetCounts( IEnumerable<OptionItem> options )
            {
                if ( !options.Any( o => !o.Threshhold.HasValue ) )
                {
                    Threshhold = options.Select( o => o.Threshhold.Value ).Sum();
                }

                HowMany = options.Select( o => o.HowMany ).Sum();

                if ( !options.Any( o => !o.SpotsLeft.HasValue ) )
                {
                    SpotsLeft = options.Select( o => o.SpotsLeft.Value ).Sum();
                }
            }

            public CardItem( CampusCache campus, List<OptionItem> options ) 
            {
                Key = campus.Guid.ToString();
                Item = campus;
                Order = campus.Order;
                SetCounts( options.Where( o => o.CampusGuid == campus.Guid ) );
            }

            public CardItem( DateTime? dateTime, List<OptionItem> options )
            {
                Key = dateTime.HasValue ? dateTime.Value.ToShortDateString() : string.Empty;
                Item = dateTime;
                Order = dateTime.HasValue ? dateTime.Value.ToString( "o" ) : string.Empty;
                SetCounts( options.Where( o => o.DateTime == dateTime) );
            }

            public CardItem( Location location, List<OptionItem> options )
            {
                Key = location.Guid.ToString();
                Item = location;
                SetCounts( options.Where( o => o.LocationGuid == location.Guid ) );
            }

            public CardItem( Schedule schedule, List<OptionItem> options, DateTime? selectedDate )
            {
                Key = schedule.Guid.ToString();
                Item = schedule;

                var nextStart = schedule.GetNextStartDateTime( selectedDate ?? RockDateTime.Now );
                Order = nextStart.HasValue ? nextStart.Value.ToString( "o" ) : schedule.Name;

                SetCounts( options.Where( o => o.ScheduleGuid == schedule.Guid ) );
            }
        }

        #endregion
    }

}


