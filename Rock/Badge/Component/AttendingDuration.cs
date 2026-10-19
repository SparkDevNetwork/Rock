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
                    writer.Write(String.Format( "<div class='rockbadge rockbadge-standard rockbadge-attendingduration {1}' data-toggle='tooltip' data-original-title='{0}'>", GetTooltipAttributeValue( person.NickName, "is new this week." ), cssClass));
                }
                else
                {
                    writer.Write(String.Format( "<div class='rockbadge rockbadge-standard rockbadge-attendingduration {1}' data-toggle='tooltip' data-original-title='{0}'>", GetTooltipAttributeValue( person.NickName, $"first visited {spanUnit.ToQuantity( spanValue.AsInteger() )} ago." ), cssClass));
                }

                writer.Write(String.Format("<span class='metric-value'>{0}</span><span class='metric-unit'>{1}</span>", spanValue, spanUnit));

                writer.Write("</div>");
            }
        }

        /// <summary>
        /// Gets the tooltip text for the badge, encoded for use inside a single quoted
        /// HTML attribute whose value is then rendered as an HTML tooltip.
        /// </summary>
        /// <param name="nickName">The person's nick name.</param>
        /// <param name="message">The plain text message that follows the nick name.</param>
        /// <returns>The encoded tooltip text.</returns>
        internal static string GetTooltipAttributeValue( string nickName, string message )
        {
            /*
                10/18/2026 - MSE

                The tooltip is placed in a single quoted HTML attribute, which the browser
                decodes once, and badge tooltips are then rendered as HTML (BadgeControl and
                the Obsidian badges use html: true). A person can set their own nick name, so
                the HTML markup characters in the name are encoded first (to still be text when
                the tooltip is rendered as HTML), and then the whole tooltip is HTML encoded for
                the attribute. Quotes are only encoded by the outer layer, so a name such as
                O'Brien still shows correctly where the tooltip is rendered as plain text.

                Reason: Prevent stored XSS through a person's nick name in the badge tooltip.
            */
            var tooltipName = ( nickName ?? string.Empty )
                .Replace( "&", "&amp;" )
                .Replace( "<", "&lt;" )
                .Replace( ">", "&gt;" );

            return $"{tooltipName} {message}".EncodeHtml();
        }
    }
}
