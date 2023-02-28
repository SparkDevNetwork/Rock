using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks.Engagement.SignUp.SignUpDetail;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Engagement.SignUp
{
    [DisplayName( "Sign-Up Detail" )]
    [Category( "Engagement > Sign-Up" )]
    [Description( "Block used to show the details of a sign-up group/project." )]
    [IconCssClass( "fa fa-clipboard-check" )]

    #region Block Attributes

    [LinkedPage( "Registration Page",
        Key = AttributeKey.RegistrationPage,
        Description = "The page reference to pass to the Lava template for the registration page.",
        IsRequired = true,
        Order = 0 )]

    [BooleanField( "Set Page Title",
        Key = AttributeKey.SetPageTitle,
        Description = "When enabled, sets the page title to be the name of the sign-up project.",
        ControlType = BooleanFieldType.BooleanControlType.Checkbox,
        DefaultBooleanValue = true,
        Order = 1 )]

    [CodeEditorField( "Lava Template",
        Key = AttributeKey.LavaTemplate,
        Description = "The Lava template to use to show the details of the project. Merge fields include: Project. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 400,
        DefaultValue = AttributeDefault.LavaTemplate,
        IsRequired = true,
        Order = 2 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "3B92EA37-579A-4928-88C4-6A6808116D40" )]
    [Rock.SystemGuid.BlockTypeGuid( "432123B4-8FDD-4A2E-BAF7-927C2B049CAB" )]
    public class SignUpDetail : RockObsidianBlockType
    {
        #region Keys & Constants

        private static class AttributeKey
        {
            public const string RegistrationPage = "RegistrationPage";
            public const string SetPageTitle = "SetPageTitle";
            public const string LavaTemplate = "LavaTemplate";
        }

        private static class PageParameterKey
        {
            public const string ProjectId = "ProjectId";
            public const string LocationId = "LocationId";
            public const string ScheduleId = "ScheduleId";
        }

        private static class AttributeDefault
        {
            public const string LavaTemplate = @"{% if Project != null %}
    <div class=""panel panel-block"">
        <div class=""panel-heading"">
            <h1 class=""panel-title pull-left"">{{ Project.Name }}</h1>
        </div>
        <div class=""panel-body"">
            <div class=""row"">
                <div class=""col-md-6 mb-3"">
                    <div class=""d-flex justify-content-between"">
                        <h4>{{ Project.Name }}</h4>
                        {% if Project.CampusName and Project.CampusName != empty %}
                            <div class=""panel-labels"">
                                <span class=""label label-default"">hello</span>
                            </div>
                        {% endif %}
                    </div>
                    {% if Project.ScheduleName and Project.ScheduleName != empty %}
                        <p class=""text-muted mb-3"">{{ Project.ScheduleName }}</p>
                    {% endif %}
                    {% if Project.Description and Project.Description != empty %}
                        <p>{{ Project.Description }}</p>
                    {% endif %}
                    {% if Project.AvailableSpots != null %}
                        <span class=""badge badge-info"">Available Spots: {{ Project.AvailableSpots }}</span>
                    {% endif %}
                    {% if Project.ScheduleHasFutureStartDateTime %}
                        <div class=""mt-4"">
                            <a href=""{{ Project.RegisterPageUrl }}"" class=""btn btn-warning"">Register</a>
                        </div>
                    {% endif %}
                </div>
                {% if Project.MapCenter and Project.MapCenter != empty %}
                    <div class=""col-md-6 mb-3"">
                        {[ googlestaticmap center:'{{ Project.MapCenter }}' zoom:'15' ]}
                        {[ endgooglestaticmap ]}
                    </div>
                {% endif %}
            </div>
        </div>
    </div>
{% endif %}";
        }

        #endregion

        #region Properties

        public override string BlockFileUrl => $"{base.BlockFileUrl}.obs";

        #endregion

        #region Methods

        public override object GetObsidianBlockInitialization()
        {
            var box = new SignUpDetailInitializationBox();

            using ( var rockContext = new RockContext() )
            {
                SetBoxInitialState( box, rockContext );
            }

            return box;
        }

        /// <summary>
        /// Sets the initial state of the box.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialState( SignUpDetailInitializationBox box, RockContext rockContext )
        {
            var block = new BlockService( rockContext ).Get( this.BlockId );
            block.LoadAttributes();

            var occurrenceData = GetOccurrenceData( rockContext );
            if ( !occurrenceData.CanDisplayProject )
            {
                box.ErrorMessage = occurrenceData.ErrorMessage ?? "Unable to display project details.";
                return;
            }

            box.SignUpDetailHtml = GetSignUpDetailHtml( occurrenceData );
        }

        /// <summary>
        /// Gets the occurrence data, built using a combination of page parameter values and existing database records.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The occurrence data, built using a combination of page parameter values and existing database records.</returns>
        private OccurrenceData GetOccurrenceData( RockContext rockContext )
        {
            var occurrenceData = new OccurrenceData();

            var projectId = Rock.Utility.IdHasher.Instance.GetId( PageParameter( PageParameterKey.ProjectId ) );
            if ( !projectId.HasValue )
            {
                occurrenceData.ErrorMessage = "Project ID was not provided.";
                return occurrenceData;
            }

            var locationId = Rock.Utility.IdHasher.Instance.GetId( PageParameter( PageParameterKey.LocationId ) );
            if ( !locationId.HasValue )
            {
                occurrenceData.ErrorMessage = "Location ID was not provided";
                return occurrenceData;
            }

            var scheduleId = Rock.Utility.IdHasher.Instance.GetId( PageParameter( PageParameterKey.ScheduleId ) );
            if ( !scheduleId.HasValue )
            {
                occurrenceData.ErrorMessage = "Schedule ID was not provided.";
                return occurrenceData;
            }

            if ( !TryGetGroupLocationSchedule( rockContext, occurrenceData, projectId.Value, locationId.Value, scheduleId.Value ) )
            {
                // An error message will have been added.
                return occurrenceData;
            }

            GetParticipantCount( rockContext, occurrenceData );

            return occurrenceData;
        }

        /// <summary>
        /// Tries to get the <see cref="Group"/>, <see cref="Location"/> and <see cref="Schedule"/> instances for this occurrence,
        /// loading them onto the <see cref="OccurrenceData"/> instance.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="occurrenceData">The occurrence data.</param>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <returns>Whether <see cref="Group"/>, <see cref="Location"/> and <see cref="Schedule"/> instances were successfully loaded for this occurrence.</returns>
        private bool TryGetGroupLocationSchedule( RockContext rockContext, OccurrenceData occurrenceData, int projectId, int locationId, int scheduleId )
        {
            // We'll filter against the allowed GroupType(s) to ensure this block isn't being misused.
            var signUpGroupTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP.AsGuid() ).ToIntSafe();

            // Get the active (and valid sign-up GroupType) opportunities tied to this Group and Location.
            var groupLocationSchedules = new GroupLocationService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gl =>
                    gl.Group.IsActive
                    && gl.Group.Id == projectId
                    && gl.Location.Id == locationId
                    && ( gl.Group.GroupTypeId == signUpGroupTypeId || gl.Group.GroupType.InheritedGroupTypeId == signUpGroupTypeId )
                )
                .SelectMany( gl => gl.Schedules, ( gl, s ) => new
                {
                    gl.Group,
                    gl.Group.GroupType,
                    gl.Location,
                    Schedule = s,
                    Config = gl.GroupLocationScheduleConfigs.FirstOrDefault( glsc => glsc.ScheduleId == s.Id )
                } )
                .ToList();

            // Ensure the requested schedule exists.
            var occurrence = groupLocationSchedules.FirstOrDefault( gls => gls.Schedule?.Id == scheduleId );

            if ( occurrence == null )
            {
                occurrenceData.ErrorMessage = "Project occurrence not found.";
                return false;
            }

            occurrenceData.Schedule = occurrence.Schedule;

            if ( !occurrenceData.ScheduleHasFutureStartDateTime )
            {
                occurrenceData.ErrorMessage = "Project has no upcoming occurrences.";
                return false;
            }

            occurrenceData.Project = occurrence.Group;
            occurrenceData.Location = occurrence.Location;
            occurrenceData.Config = occurrence.Config;
            occurrenceData.ScheduleName = occurrenceData.Config?.ConfigurationName;
            occurrenceData.SlotsMin = occurrenceData.Config?.MinimumCapacity;
            occurrenceData.SlotsDesired = occurrenceData.Config?.DesiredCapacity;
            occurrenceData.SlotsMax = occurrenceData.Config?.MaximumCapacity;

            return true;
        }

        /// <summary>
        /// Gets the participant count for this <see cref="Group"/>, <see cref="Location"/> and <see cref="Schedule"/> occurrence.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="occurrenceData">The occurrence data.</param>
        private void GetParticipantCount( RockContext rockContext, OccurrenceData occurrenceData )
        {
            /*
             * Get the participant count for this opportunity.
             * This should be incorporated into the above query (for performance reasons) when we have more time to do so.
             */
            var participantCount = new GroupMemberAssignmentService( rockContext )
                .Queryable()
                .Where( gma =>
                    !gma.GroupMember.Person.IsDeceased
                    && gma.GroupMember.GroupId == occurrenceData.Project.Id
                    && gma.LocationId == occurrenceData.Location.Id
                    && gma.ScheduleId == occurrenceData.Schedule.Id
                )
                .Count();

            occurrenceData.ParticipantCount = participantCount;
        }

        /// <summary>
        /// Gets the sign-up detail HTML.
        /// </summary>
        /// <param name="occurrenceData">The occurrence data.</param>
        /// <returns>The sign-up detail HTML.</returns>
        private string GetSignUpDetailHtml( OccurrenceData occurrenceData )
        {
            var lavaTemplate = GetAttributeValue( AttributeKey.LavaTemplate );
            var mergeFields = this.RequestContext.GetCommonMergeFields();

            var projectIdKey = occurrenceData.Project.IdKey;
            var locationIdKey = occurrenceData.Location.IdKey;
            var scheduleIdKey = occurrenceData.Schedule.IdKey;
            var registrationPageUrl = GetLinkedPageUrl( AttributeKey.RegistrationPage, projectIdKey, locationIdKey, scheduleIdKey );

            var project = occurrenceData.ToProject( registrationPageUrl );
            mergeFields.Add( "Project", project );

            return lavaTemplate.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Gets the linked page URL.
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="projectIdKey">The project hashed identifier key.</param>
        /// <param name="locationIdKey">The location hashed identifier key.</param>
        /// <param name="scheduleIdKey">The schedule hashed identifier key.</param>
        /// <returns>The linked page URL or "#" if a given linked page attribute is not set.</returns>
        private string GetLinkedPageUrl( string attributeKey, string projectIdKey, string locationIdKey, string scheduleIdKey )
        {
            if ( string.IsNullOrWhiteSpace( GetAttributeValue( attributeKey ) ) )
            {
                return "#";
            }

            var queryParams = new Dictionary<string, string>
            {
                { PageParameterKey.ProjectId, projectIdKey },
                { PageParameterKey.LocationId, locationIdKey },
                { PageParameterKey.ScheduleId, scheduleIdKey }
            };

            return this.GetLinkedPageUrl( attributeKey, queryParams );
        }

        #endregion

        #region Supporting Classes

        /// <summary>
        /// A runtime object to represent a <see cref="Rock.Model.Group"/>, <see cref="Rock.Model.Location"/> and
        /// <see cref="Rock.Model.Schedule"/> combination, along with convenience properties for the final,
        /// Lava class, <see cref="SignUpDetail.Project"/> to easily pick from.
        /// </summary>
        private class OccurrenceData
        {
            public string ErrorMessage { get; set; }

            public Group Project { get; set; }

            public Location Location { get; set; }

            public Schedule Schedule { get; set; }

            public GroupLocationScheduleConfig Config { get; set; }

            public string ScheduleName { get; set; }

            public int? SlotsMin { get; set; }

            public int? SlotsDesired { get; set; }

            public int? SlotsMax { get; set; }

            public int ParticipantCount { get; set; }

            public string ProjectName
            {
                get
                {
                    return this.Project?.Name;
                }
            }

            public string Description
            {
                get
                {
                    return this.Project?.Description;
                }
            }

            public DateTime? NextStartDateTime
            {
                get
                {
                    return this.Schedule?.NextStartDateTime;
                }
            }

            public bool ScheduleHasFutureStartDateTime
            {
                get
                {
                    return this.NextStartDateTime.HasValue
                        && this.NextStartDateTime.Value >= RockDateTime.Now;
                }
            }

            public string FriendlySchedule
            {
                get
                {
                    if ( !this.ScheduleHasFutureStartDateTime )
                    {
                        return "No upcoming occurrences.";
                    }

                    var friendlySchedule = this.NextStartDateTime.Value.ToString( "dddd, MMM d h:mm tt" );

                    if ( this.NextStartDateTime.Value.Year != RockDateTime.Now.Year )
                    {
                        friendlySchedule = $"{friendlySchedule} ({this.NextStartDateTime.Value.Year})";
                    }

                    return friendlySchedule;
                }
            }

            public int SlotsAvailable
            {
                get
                {
                    if ( !this.ScheduleHasFutureStartDateTime )
                    {
                        return 0;
                    }

                    /*
                     * This more complex approach uses a dynamic/floating minuend (the first number in a subtraction problem):
                     * 1) If the max value is defined, use that;
                     * 2) Else, if the desired value is defined, use that;
                     * 3) Else, if the min value is defined, use that;
                     * 4) Else, use int.MaxValue (there is no limit to the slots available).
                     */
                    //var minuend = this.SlotsMax.GetValueOrDefault() > 0
                    //    ? this.SlotsMax.Value
                    //    : this.SlotsDesired.GetValueOrDefault() > 0
                    //        ? this.SlotsDesired.Value
                    //        : this.SlotsMin.GetValueOrDefault() > 0
                    //            ? this.SlotsMin.Value
                    //            : int.MaxValue;

                    /*
                     * This approach still uses a dynamic minuend, but it's much simpler:
                     * 1) If the max value is defined, use that;
                     * 2) Else, use int.MaxValue (there is no limit to the slots available).
                     */
                    var minuend = this.SlotsMax.GetValueOrDefault() > 0
                        ? this.SlotsMax.Value
                        : int.MaxValue;

                    var available = minuend - this.ParticipantCount;

                    return available < 0 ? 0 : available;
                }
            }

            public bool CanDisplayProject
            {
                get
                {
                    return string.IsNullOrEmpty( this.ErrorMessage )
                        && this.Project != null
                        && this.Location != null
                        && this.ScheduleHasFutureStartDateTime;
                }
            }

            /// <summary>
            /// Converts an <see cref="OccurrenceData"/> instance to a <see cref="SignUpDetail.Project"/> for display within the lava results template.
            /// </summary>
            /// <param name="registrationPageUrl">The registration page URL for this <see cref="SignUpDetail.Project"/>.</param>
            /// <returns>a <see cref="SignUpDetail.Project"/> instance for display within the lava results template.</returns>
            public Project ToProject( string registrationPageUrl )
            {
                int? availableSpots = null;
                if ( this.SlotsAvailable != int.MaxValue )
                {
                    availableSpots = this.SlotsAvailable;
                }

                string mapCenter = null;
                if ( this.Location.Latitude.HasValue && this.Location.Longitude.HasValue )
                {
                    mapCenter = $"{this.Location.Latitude.Value},{this.Location.Longitude.Value}";
                }
                else
                {
                    var streetAddress = this.Location.GetFullStreetAddress();
                    if ( !string.IsNullOrWhiteSpace( streetAddress ) )
                    {
                        mapCenter = streetAddress;
                    }
                }

                return new Project
                {
                    Name = this.ProjectName,
                    Description = this.Description,
                    ScheduleName = this.ScheduleName,
                    FriendlySchedule = this.FriendlySchedule,
                    ScheduleHasFutureStartDateTime = this.ScheduleHasFutureStartDateTime,
                    CampusName = this.Project.Campus?.Name,
                    AvailableSpots = availableSpots,
                    MapCenter = mapCenter,
                    RegisterPageUrl = registrationPageUrl,
                    GroupId = this.Project.Id,
                    LocationId = this.Location.Id,
                    ScheduleId = this.Schedule.Id
                };
            }
        }

        /// <summary>
        /// This POCO will be passed to the results Lava template to represent the project opportunity (GroupLocationSchedule).
        /// </summary>
        /// <seealso cref="Rock.Utility.RockDynamic" />
        private class Project : RockDynamic
        {
            public string Name { get; set; }

            public string Description { get; set; }

            public string ScheduleName { get; set; }

            public string FriendlySchedule { get; set; }

            public bool ScheduleHasFutureStartDateTime { get; set; }

            public string CampusName { get; set; }

            public int? AvailableSpots { get; set; }

            public double? DistanceInMiles { get; set; }

            public string MapCenter { get; set; }

            public string RegisterPageUrl { get; set; }

            public int GroupId { get; set; }

            public int LocationId { get; set; }

            public int ScheduleId { get; set; }
        }

        #endregion
    }
}
