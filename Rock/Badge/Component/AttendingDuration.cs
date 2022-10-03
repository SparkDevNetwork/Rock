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
using System.IO;

using Humanizer;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Badge.Component
{
    /// <summary>
    /// Attending Duration Badge
    /// </summary>
    [Description( "Badge that summarizes how long someone has been attending." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Attending Duration" )]

    [Rock.SystemGuid.EntityTypeGuid( "B50090B4-9424-4963-B34F-957394FFBB3E")]
    public class AttendingDuration : BadgeComponent
    {

        private const int _weeksPeriodInDays = 56;
        private const int _monthsPeriodInDays = 720;

        /// <summary>
        /// Determines of this badge component applies to the given type
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public override bool DoesApplyToEntityType( string type )
        {
            return type.IsNullOrWhiteSpace() || typeof( Person ).FullName == type;
        }

        /// <inheritdoc/>
        public override void Render( BadgeCache badge, IEntity entity, TextWriter writer )
        {
            if ( !( entity is Person person ) )
            {
                return;
            }

            DateTime? firstVisit = person.GetAttributeValue( "FirstVisit" ).AsDateTime();
            if (firstVisit.HasValue)
            {
                TimeSpan attendanceDuration = RockDateTime.Now - firstVisit.Value;

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
                    writer.Write(String.Format( "<div class='rockbadge rockbadge-standard rockbadge-attendingduration {1}' data-toggle='tooltip' data-original-title='{0} is new this week.'>", person.NickName, cssClass));
                }
                else
                {
                    writer.Write(String.Format( "<div class='rockbadge rockbadge-standard rockbadge-attendingduration {2}' data-toggle='tooltip' data-original-title='{0} first visited {1} ago.'>", person.NickName, spanUnit.ToQuantity(spanValue.AsInteger()), cssClass));
                }

                writer.Write(String.Format("<span class='metric-value'>{0}</span><span class='metric-unit'>{1}</span>", spanValue, spanUnit));

                writer.Write("</div>");
            }
        }
    }
}
