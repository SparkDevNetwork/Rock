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

using System;
using System.Collections.Generic;
using System.IO;

namespace Rock.Model
{
    /// <summary>
    /// DDay.ical LoadFromStream is not threadsafe, so use locking
    /// </summary>
    [RockObsolete( "1.12" )]
    [Obsolete( "Use InetCalendarHelper instead." )]
    public static class ScheduleICalHelper
    {
        private static object _initLock;
        private static Dictionary<string, DDay.iCal.Event> _iCalSchedules = new Dictionary<string, DDay.iCal.Event>();

        static ScheduleICalHelper()
        {
            ScheduleICalHelper._initLock = new object();
        }

        /// <summary>
        /// Gets the calendar event.
        /// </summary>
        /// <param name="iCalendarContent">Content of the i calendar.</param>
        /// <returns></returns>
        [RockObsolete( "1.9" )]
        [Obsolete( "Use GetCalendarEvent( iCalendarContent ) instead ", true )]
        public static DDay.iCal.Event GetCalenderEvent( string iCalendarContent )
        {
            return GetCalendarEvent( iCalendarContent );
        }

        /// <summary>
        /// Gets the calendar event.
        /// </summary>
        /// <param name="iCalendarContent">Content of the i calendar.</param>
        /// <returns></returns>
        public static DDay.iCal.Event GetCalendarEvent( string iCalendarContent )
        {
            string trimmedContent = iCalendarContent.Trim();

            if ( string.IsNullOrWhiteSpace( trimmedContent ) )
            {
                return null;
            }

            DDay.iCal.Event calendarEvent = null;

            lock ( ScheduleICalHelper._initLock )
            {
                if ( _iCalSchedules.ContainsKey( trimmedContent ) )
                {
                    return _iCalSchedules[trimmedContent];
                }

                StringReader stringReader = new StringReader( trimmedContent );
                var calendarList = DDay.iCal.iCalendar.LoadFromStream( stringReader );

                //// iCal is stored as a list of Calendar's each with a list of Events, etc.  
                //// We just need one Calendar and one Event
                if ( calendarList.Count > 0 )
                {
                    var calendar = calendarList[0] as DDay.iCal.iCalendar;
                    if ( calendar != null )
                    {
                        calendarEvent = calendar.Events[0] as DDay.iCal.Event;
                        _iCalSchedules.AddOrReplace( trimmedContent, calendarEvent );
                    }
                }
            }

            return calendarEvent;
        }

        /// <summary>
        /// Gets the occurrences.
        /// </summary>
        /// <param name="icalEvent">The ical event.</param>
        /// <param name="startTime">The start time.</param>
        /// <returns></returns>
        public static IList<DDay.iCal.Occurrence> GetOccurrences( DDay.iCal.Event icalEvent, DateTime startTime )
        {
            lock ( ScheduleICalHelper._initLock )
            {
                return icalEvent.GetOccurrences( startTime );
            }
        }

        /// <summary>
        /// Gets the occurrences.
        /// </summary>
        /// <param name="icalEvent">The ical event.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns></returns>
        public static IList<DDay.iCal.Occurrence> GetOccurrences( DDay.iCal.Event icalEvent, DateTime startTime, DateTime endTime )
        {
            lock ( ScheduleICalHelper._initLock )
            {
                return icalEvent.GetOccurrences( startTime, endTime );
            }
        }
    }
}
