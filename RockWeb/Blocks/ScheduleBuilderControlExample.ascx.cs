using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DDay.iCal;
using Rock.Web.UI;

namespace RockWeb.Blocks
{
    public partial class ScheduleBuilderControlExample : RockBlock
    {

        /// <summary>
        /// Handles the SaveSchedule event of the sbExample control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void sbExample_SaveSchedule( object sender, EventArgs e )
        {
            lblScheduleCalendarContent.Text = sbExample.iCalendarContent;
            iCalendar calendar = iCalendar.LoadFromStream( new StringReader( sbExample.iCalendarContent ) ).First() as iCalendar;
            DDay.iCal.Event calendarEvent = calendar.Events[0] as Event;

            List<Occurrence> nextOccurrences = calendar.GetOccurrences( DateTime.Now.Date, DateTime.Now.Date.AddYears( 1 ) ).ToList();

            string listHtml = "<ul>";
            foreach ( var occurrence in nextOccurrences )
            {
                listHtml += string.Format( "<li>{0} to {1} ( {2} hours) </li>", occurrence.Period.StartTime.Value.ToString( "g" ), occurrence.Period.EndTime.Value.ToString( "g" ), occurrence.Period.Duration.TotalHours.ToString() );
            }

            listHtml += "</ul>";


            DateTime? effectiveEndDate = null;
            if ( calendarEvent.RecurrenceDates.Count() > 0 )
            {
                var dateList = calendarEvent.RecurrenceDates[0];
                effectiveEndDate = dateList.OrderBy( a => a.StartTime ).Last().StartTime.Value;
            }
            else if ( calendarEvent.RecurrenceRules.Count() > 0 )
            {
                var rrule = calendarEvent.RecurrenceRules[0];
                if ( rrule.Until > DateTime.MinValue )
                {
                    effectiveEndDate = rrule.Until;
                }
                else if ( rrule.Count > 0 )
                {
                    // not really a perfect way to figure out end date.  safer to assume null
                    effectiveEndDate = null;
                }
            }
            else
            {
                effectiveEndDate = calendarEvent.End.Value;
            }


            if ( effectiveEndDate != null )
            {
                listHtml = "Ends at " + effectiveEndDate.Value.ToString() + " </br>" + listHtml;
            }
            else
            {
                listHtml = "Never ends </br>" + listHtml;
            }

            lblOccurrances.Text = listHtml;

        }

        /// <summary>
        /// Handles the CancelSchedule event of the sbExample control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void sbExample_CancelSchedule( object sender, EventArgs e )
        {
            // make sure it didn't change
            lblScheduleCalendarContent.Text = sbExample.iCalendarContent;
        }

        /// <summary>
        /// Handles the Click event of the btnOK control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnOK_Click( object sender, EventArgs e )
        {
            sbExample.iCalendarContent = lblScheduleCalendarContent.Text;
        }
    }
}