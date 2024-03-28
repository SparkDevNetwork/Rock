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

// <copyright>

using System;
using System.Linq;

using Ical.Net;
using Ical.Net.DataTypes;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// The data access/service class for the <see cref="Rock.Model.Schedule"/> entity. This inherits from the Service class
    /// </summary>
    public partial class ScheduleService
    {
        /// <summary>
        /// Creates the Preview for all the occurrences of the schedule which can be viewed in HTML
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string CreatePreviewHTML( Schedule entity )
        {
            var sbPreviewHtml = new System.Text.StringBuilder();
            sbPreviewHtml.Append( $@"<strong>iCalendar Content</strong><div style='white-space: pre' Font-Names='Consolas' Font-Size='9'><br />{entity.iCalendarContent}</div>" );

            var calendarList = CalendarCollection.Load( new System.IO.StringReader( entity.iCalendarContent ) );
            Calendar calendar = null;
            if ( calendarList.Count > 0 )
            {
                calendar = calendarList[0] as Calendar;
            }

            var calendarEvent = calendar?.Events?[0];

            if ( calendarEvent?.DtStart != null )
            {
                var nextOccurrences = calendar.GetOccurrences( RockDateTime.Now, RockDateTime.Now.AddYears( 1 ) ).Take( 26 ).ToList();
                var sbOccurrenceItems = new System.Text.StringBuilder();
                if ( nextOccurrences.Any() )
                {
                    foreach ( var occurrence in nextOccurrences )
                    {
                        sbOccurrenceItems.Append( $"<li>{GetOccurrenceText( occurrence )}</li>" );
                    }
                }
                else
                {
                    sbOccurrenceItems.Append( "<li>No future occurrences</l1>" );
                }

                sbPreviewHtml.Append( $"<hr /><strong>Occurrences Preview</strong><ul>{sbOccurrenceItems}</ul>" );
            }

            return sbPreviewHtml.ToString();
        }

        /// <summary>
        /// Gets the occurrence text.
        /// Moved over from ScheduleDetails Blocks in Webforms
        /// </summary>
        /// <param name="occurrence">The occurrence.</param>
        /// <returns></returns>
        private static string GetOccurrenceText( Occurrence occurrence )
        {
            string occurrenceText;
            if ( occurrence.Period.Duration <= new TimeSpan( 0, 0, 1 ) )
            {
                // no or very short duration. Probably a schedule for starting something that doesn't care about duration, like Metrics
                occurrenceText = string.Format( "{0}", occurrence.Period.StartTime.Value.ToString( "g" ) );
            }
            else if ( occurrence.Period.StartTime.Value.Date.Equals( occurrence.Period.EndTime.Value.Date ) )
            {
                // same day for start and end time
                occurrenceText = string.Format( "{0} - {1} to {2} ( {3} hours) ", occurrence.Period.StartTime.Value.Date.ToShortDateString(), occurrence.Period.StartTime.Value.TimeOfDay.ToTimeString(), occurrence.Period.EndTime.Value.TimeOfDay.ToTimeString(), occurrence.Period.Duration.TotalHours.ToString( "#0.00" ) );
            }
            else
            {
                // spans over midnight
                occurrenceText = string.Format( "{0} to {1} ( {2} hours) ", occurrence.Period.StartTime.Value.ToString( "g" ), occurrence.Period.EndTime.Value.ToString( "g" ), occurrence.Period.Duration.TotalHours.ToString( "#0.00" ) );
            }
            return occurrenceText;
        }

        /// <summary>
        /// Clones a schedule given the id.
        /// </summary>
        /// <param name="id"> The idkey of the Schedule to be copied</param>
        /// <returns></returns>
        public Schedule Copy( string id )
        {
            var schedule = Get( id );
            var newSchedule = schedule.CloneWithoutIdentity();
            newSchedule.Name += " - Copy";
            this.Add( newSchedule );
            schedule.LoadAttributes();
            newSchedule.LoadAttributes();
            newSchedule.CopyAttributesFrom( schedule );

            var rockContext = this.Context as RockContext;

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();
                newSchedule.SaveAttributeValues( rockContext );
            } );
            return newSchedule;
        }
    }
}
