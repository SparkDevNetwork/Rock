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
using System.ComponentModel;
using System.ComponentModel.Composition;

using Humanizer;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.PersonProfile.Badge
{
    /// <summary>
    /// Attending Duration Badge
    /// </summary>
    [Description( "Badge that summarizes how long someone has been attending." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Attending Duration" )]
    
    public class AttendingDuration : BadgeComponent
    {

        private int _weeksPeriodInDays = 56;
        private int _monthsPeriodInDays = 720;

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            DateTime? firstVisit = Person.GetAttributeValue( "FirstVisit" ).AsDateTime();
            if (firstVisit.HasValue)
            {
                TimeSpan attendanceDuration = DateTime.Now - firstVisit.Value;

                string spanValue = string.Empty;
                string spanUnit = string.Empty;
                string cssClass = string.Empty;

                if (attendanceDuration.Days < _weeksPeriodInDays) // display value in weeks
                {
                    
                    if (attendanceDuration.Days < 7)
                    {
                        spanValue = "New";
                        cssClass = "duration-new";
                    }
                    else
                    {
                        spanValue = (attendanceDuration.Days / 7).ToString();
                        spanUnit = "wk";
                        cssClass = "duration-weeks";
                    }

                } else if(attendanceDuration.Days < _monthsPeriodInDays) { // display value in months
                    spanValue = (attendanceDuration.Days / 30).ToString();
                    spanUnit = "mo";
                    cssClass = "duration-months";
                }
                else // display value in months
                {
                    spanValue = (attendanceDuration.Days / 365).ToString();
                    spanUnit = "yr";
                    cssClass = "duration-years";
                }

                if (spanValue == "New")
                {
                    writer.Write(String.Format( "<div class='badge badge-attendingduration' data-toggle='tooltip' data-original-title='{0} is new this week.'>", Person.NickName));
                }
                else
                {
                    writer.Write(String.Format( "<div class='badge badge-attendingduration' data-toggle='tooltip' data-original-title='{0} first visited {1} ago.'>", Person.NickName, spanUnit.ToQuantity(spanValue.AsInteger())));
                }

                writer.Write(String.Format("<div class='duration-metric {0}'>", cssClass));
                writer.Write(String.Format("<span class='metric-value'>{0}<span class='metric-unit'>{1}</span></span>", spanValue, spanUnit));
                writer.Write("</div>");

                writer.Write("</div>");
            }
            

        }


    }
}
