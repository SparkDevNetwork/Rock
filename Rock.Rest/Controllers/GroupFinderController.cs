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
using System.Data.Entity.Spatial;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.OData;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Group Finder API
    /// </summary>
    public class GroupFinderController : ApiControllerBase
    {
        /// <summary>
        /// Returns a list of all groups querying primarily based on the postal code and then  sort based on distance from
        /// a given zip code to the meeting location of a group.
        /// </summary>
        /// <param name="postalCode">The postal code.</param>
        /// <param name="groupTypeId">The group type id</param>
        /// <param name="schedules">The comma seperated representation of the group schedules.</param>
        /// <param name="campuses">The comma seperated representation of the group campuses.</param>
        /// <param name="tags">The list of tags</param>
        /// <param name="kidFriendly">The Childcare value.</param>
        /// <returns>
        /// Returns the list of all groups.
        /// </returns>
        [HttpGet]
        [EnableQuery]
        [Authenticate, Secured]
        [System.Web.Http.Route("api/GroupFinder")]
        public IQueryable<SmallGroup> GetGroups(int postalCode, int groupTypeId, string schedules = null, string campuses = null, string tags = null, bool? kidFriendly = null)
        {
            var groupList = new List<SmallGroup>();

            using (var rockContext = new RockContext())
            {
                var mapCoordinate = new LocationService(rockContext).GetMapCoordinateFromPostalCode(postalCode.ToString());
                if (mapCoordinate == null)
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }

                var groups = new GroupService(rockContext).Queryable().Where(a => a.GroupTypeId == groupTypeId && a.IsActive == true && a.IsPublic == true && a.IsArchived == false);

                if (schedules.IsNotNullOrWhiteSpace())
                {
                    var dayOfWeeks = schedules.SplitDelimitedValues(false).Select(a => a.ConvertToEnumOrNull<DayOfWeek>()).Where(a => a.HasValue).ToList();
                    groups = groups.Where(a => a.Schedule.WeeklyDayOfWeek.HasValue && dayOfWeeks.Contains(a.Schedule.WeeklyDayOfWeek));
                }

                if (campuses.IsNotNullOrWhiteSpace())
                {
                    var campusIds = campuses.SplitDelimitedValues().AsIntegerList();
                    groups = groups.Where(a => a.CampusId.HasValue && campusIds.Contains(a.CampusId.Value));
                }

                foreach (var group in groups)
                {
                    group.LoadAttributes(rockContext);
                    var childcare = group.GetAttributeValue("Childcare").AsBoolean();

                    if (tags.IsNotNullOrWhiteSpace())
                    {
                        var tagValues = tags.SplitDelimitedValues(false);
                        var groupTags = group.AttributeValues["Tags"].ValueFormatted.SplitDelimitedValues(false).Union(group.AttributeValues["Category"].ValueFormatted.SplitDelimitedValues(false));
                        if (!groupTags.Any(a => tagValues.Contains(a, StringComparer.OrdinalIgnoreCase)))
                        {
                            continue;
                        }
                    }

                    if (!kidFriendly.HasValue || (kidFriendly.HasValue && childcare == kidFriendly.Value))
                    {
                        var meetingLocationTypeId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_MEETING_LOCATION).Id;

                        var smallGroup = new SmallGroup()
                        {
                            IsActive = group.IsActive,
                            IsPublic = group.IsPublic,
                            IsArchived = group.IsArchived,
                            Description = group.Description,
                            Guid = group.Guid,
                            Id = group.Id,
                            KidFriendly = childcare,
                            Name = group.Name,
                            Campus = group.Campus?.Name,
                            CampusId = group.CampusId,
                            Schedule = group.Schedule?.FriendlyScheduleText
                        };

                        var location = group.GroupLocations.Where(a => a.GroupLocationTypeValueId == meetingLocationTypeId).Select(a => a.Location).FirstOrDefault();
                        if (location != null && location.GeoPoint != null)
                        {

                            smallGroup.GroupLocation = new MapCoordinate()
                            {
                                Latitude = location.Latitude,
                                Longitude = location.Longitude
                            };
                            string geoText = string.Format("POINT({0} {1})", mapCoordinate.Longitude, mapCoordinate.Latitude);
                            DbGeography geoPoint = DbGeography.FromText(geoText);
                            var distance = location.GeoPoint.Distance(geoPoint);

                            if (distance.HasValue)
                            {
                                smallGroup.Distance = Math.Round(distance.Value * 0.00062137, 2);
                            }
                        }

                        if (group.AttributeValues.ContainsKey("Topic"))
                        {
                            smallGroup.Topic = group.AttributeValues["Topic"].ValueFormatted;
                        }

                        var ageRange = group.GetAttributeValue("AgeRange").SplitDelimitedValues(false);
                        if (ageRange.Length == 2)
                        {
                            smallGroup.AgeRange = ageRange.Select(a => a.AsInteger()).OrderBy(a => a).ToArray();
                        }

                        if (group.AttributeValues.ContainsKey("Tags"))
                        {
                            smallGroup.Tags = group.AttributeValues["Tags"].ValueFormatted.SplitDelimitedValues(false);
                        }

                        if (group.AttributeValues.ContainsKey("GroupPhoto"))
                        {
                            smallGroup.Photo = group.AttributeValues["GroupPhoto"].ValueFormatted;
                        }

                        if (group.AttributeValues.ContainsKey("Type"))
                        {
                            smallGroup.Type = group.AttributeValues["Type"].ValueFormatted;
                        }

                        groupList.Add(smallGroup);
                    }
                }
            }
            return groupList.OrderBy(a => a.Distance.GetValueOrDefault(double.MaxValue)).AsQueryable();
        }

        /// <summary>
        /// 
        /// </summary>
        public class SmallGroup
        {

            /// <summary>
            /// Gets or sets the IsActive
            /// </summary>
            /// <value>
            /// The status of this group.
            /// </value>
            public bool IsActive { get; set; }

            /// <summary>
            /// Gets or sets IsPublic
            /// </summary>
            /// <value>
            /// The public/private status of this group.
            /// </value>
            public bool IsPublic { get; set; }

            /// <summary>
            /// Gets or sets IsArchived
            /// </summary>
            /// <value>
            /// The archived status of this group.
            /// </value>
            public bool IsArchived { get; set; }

            /// <summary>
            /// Gets or sets the group identifier.
            /// </summary>
            /// <value>
            /// The group identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the Name of the Group. This property is required.
            /// </summary>
            /// <value>
            /// A <see cref="System.String"/> representing the name of the Group. 
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the description of the group.
            /// </summary>
            /// <value>
            /// A <see cref="System.String"/> representing the description of the group.
            /// </value>
            public string Description { get; set; }

            /// <summary>
            /// Gets or sets a <see cref="System.Guid"/> value that is a guaranteed unique identifier for the group.
            /// </summary>
            /// <value>
            /// A <see cref="System.Guid"/> value that will uniquely identify the Group.
            /// </value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the distance of the group meeting location and postal code.
            /// </summary>
            /// <value>
            /// The distance of the group meeting location and postal code.
            /// </value>
            public double? Distance { get; set; }

            /// <summary>
            /// Gets or sets the kid friendly.
            /// </summary>
            /// <value>
            /// The kid friendly.
            /// </value>
            public bool KidFriendly { get; set; }

            /// <summary>
            /// Gets or sets the group location.
            /// </summary>
            /// <value>
            /// The group location.
            /// </value>
            public MapCoordinate GroupLocation { get; set; }

            /// <summary>
            /// Gets or sets the topic.
            /// </summary>
            /// <value>
            /// The topic.
            /// </value>
            public string Topic { get; set; }

            /// <summary>
            /// Gets or sets the campus name.
            /// </summary>
            /// <value>
            /// The campus name.
            /// </value>
            public string Campus { get; set; }

            /// <summary>
            /// Gets or sets the campus identifier.
            /// </summary>
            /// <value>
            /// The campus identifier.
            /// </value>
            public int? CampusId { get; set; }

            /// <summary>
            /// Gets or sets the age range.
            /// </summary>
            /// <value>
            /// The age range.
            /// </value>
            public int[] AgeRange { get; set; }

            /// <summary>
            /// Gets or sets the photo.
            /// </summary>
            /// <value>
            /// The photo.
            /// </value>
            public string Photo { get; set; }

            /// <summary>
            /// Gets or sets the schedule.
            /// </summary>
            /// <value>
            /// The schedule.
            /// </value>
            public string Schedule { get; set; }

            /// <summary>
            /// Gets or sets the tags.
            /// </summary>
            /// <value>
            /// The tags.
            /// </value>
            public string[] Tags { get; set; }

            /// <summary>
            /// Gets or sets the type.
            /// </summary>
            /// <value>
            /// The type.
            /// </value>
            public string Type { get; set; }
        }

    }
}