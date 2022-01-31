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

using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Badge.Component
{
    /// <summary>
    /// Streak Engagement Badge
    /// </summary>
    [Description( "Shows a chart of the engagement history with each bar representing one month or day depending on the streak type." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Streak Engagement" )]

    [StreakTypeField(
        name: "Streak Type",
        description: "The streak type to display streak data for the given person about",
        required: true,
        key: AttributeKey.StreakType )]

    [IntegerField(
        name: "Units To Display",
        description: "The number of days or months to show on the chart (default 24.)",
        required: false,
        defaultValue: AttributeDefault.BarCount,
        key: AttributeKey.BarCount )]

    [IntegerField(
        name: "Minimum Bar Height",
        description: "The minimum height of a bar (in pixels). Useful for showing hint of bar when attendance was 0. (default 2.)",
        required: false,
        defaultValue: AttributeDefault.MinBarHeight,
        key: AttributeKey.MinBarHeight )]

    [BooleanField(
        name: "Animate Bars",
        description: "Determine whether bars should animate when displayed.",
        defaultValue: AttributeDefault.AnimateBars,
        key: AttributeKey.AnimateBars )]

    [LinkedPage(
        name: "Streak Detail Page",
        description: "If set, clicking this badge will navigate to the given page.",
        required: false,
        key: AttributeKey.StreakDetailPage )]

    public class StreakEngagement : BadgeComponent
    {
        #region Keys

        /// <summary>
        /// Keys to use for the attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The streak type attribute key
            /// </summary>
            public const string StreakType = "StreakType";

            /// <summary>
            /// The bar count attribute key
            /// </summary>
            public const string BarCount = "BarCount";

            /// <summary>
            /// The min bar height attribute key
            /// </summary>
            public const string MinBarHeight = "MinBarHeight";

            /// <summary>
            /// The animate bars attribute key
            /// </summary>
            public const string AnimateBars = "AnimateBars";

            /// <summary>
            /// The enrollment detail page attribute key
            /// </summary>
            public const string StreakDetailPage = "EnrollmentDetailPage";
        }

        /// <summary>
        /// Default values for the attributes
        /// </summary>
        private static class AttributeDefault
        {
            /// <summary>
            /// The bar count attribute default
            /// </summary>
            public const int BarCount = 24;

            /// <summary>
            /// The min bar height attribute default
            /// </summary>
            public const int MinBarHeight = 2;

            /// <summary>
            /// The animate bars attribute default
            /// </summary>
            public const bool AnimateBars = true;
        }

        #endregion Keys

        /// <summary>
        /// Determines of this badge component applies to the given type
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public override bool DoesApplyToEntityType( string type )
        {
            return type.IsNullOrWhiteSpace() || typeof( Person ).FullName == type;
        }

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( BadgeCache badge, System.Web.UI.HtmlTextWriter writer )
        {
            if ( Person == null )
            {
                return;
            }

            var streakTypeCache = GetStreakTypeCache( badge );

            if ( streakTypeCache == null )
            {
                return;
            }

            var timeUnit = string.Empty;
            var timeUnits = string.Empty;

            switch ( streakTypeCache.OccurrenceFrequency )
            {
                case StreakOccurrenceFrequency.Daily:
                    timeUnit = "day";
                    timeUnits = "days";
                    break;
                case StreakOccurrenceFrequency.Weekly:
                    timeUnit = "week";
                    timeUnits = "weeks";
                    break;
                case StreakOccurrenceFrequency.Monthly:
                    timeUnit = "month";
                    timeUnits = "months";
                    break;
                case StreakOccurrenceFrequency.Yearly:
                    timeUnit = "year";
                    timeUnits = "years";
                    break;
                default:
                    throw new NotImplementedException( string.Format( "StreakOccurrenceFrequency '{0}' is not implemented", streakTypeCache.OccurrenceFrequency ) );
            }

            var unitsToDisplay = GetAttributeValue( badge, AttributeKey.BarCount ).AsIntegerOrNull() ?? AttributeDefault.BarCount;
            var doAnimateBars = GetAttributeValue( badge, AttributeKey.AnimateBars ).AsBooleanOrNull() ?? AttributeDefault.AnimateBars;

            var animateClass = doAnimateBars ? " animate" : string.Empty;

            var tooltip = $"{Person.NickName.ToPossessive()} engagement in the streak '{streakTypeCache.Name}' for the last {unitsToDisplay} {timeUnits}. Each bar is a {timeUnit}.";

            var chartHtml = $"<div class='badge badge-attendance{animateClass} badge-id-{badge.Id}' data-toggle='tooltip' data-original-title='{tooltip.EncodeHtml()}'></div>";
            var linkedPageGuid = GetAttributeValue( badge, AttributeKey.StreakDetailPage ).AsGuidOrNull();
            var linkedPageId = linkedPageGuid.HasValue ? PageCache.GetId( linkedPageGuid.Value ) : null;

            if ( !linkedPageId.HasValue )
            {
                writer.Write( $"{chartHtml}" );
            }
            else
            {
                var link = $"/page/{linkedPageId.Value}?StreakTypeId={streakTypeCache.Id}&PersonId={Person.Id}";
                writer.Write( $@"<a href=""{link}"">{chartHtml}</a>" );
            }
        }

        /// <summary>
        /// Gets the java script.
        /// </summary>
        /// <param name="badge"></param>
        /// <returns></returns>
        protected override string GetJavaScript( BadgeCache badge )
        {
            if ( Person == null )
            {
                return null;
            }

            var streakTypeCache = GetStreakTypeCache( badge );

            if ( streakTypeCache == null )
            {
                return null;
            }

            var unitsToDisplay = GetAttributeValue( badge, AttributeKey.BarCount ).AsIntegerOrNull() ?? AttributeDefault.BarCount;
            var minBarHeight = GetAttributeValue( badge, AttributeKey.MinBarHeight ).AsIntegerOrNull() ?? AttributeDefault.MinBarHeight;

            return $@"
$.ajax({{
    type: 'GET',
    url: Rock.settings.get('baseUrl') + 'api/StreakTypes/RecentEngagement/{streakTypeCache.Id}/{Person.Id}?unitCount={unitsToDisplay}' ,
    statusCode: {{
        200: function (data, status, xhr) {{
            var chartHtml = ['<ul class=\'trend-chart list-unstyled\'>'];

            if (data) {{
                for(var i = data.length - 1; i >= 0; i--) {{
                    var occurrenceEngagement = data[i];
                    var isBitSet = occurrenceEngagement && occurrenceEngagement.HasEngagement;
                    var title = occurrenceEngagement ? new Date(occurrenceEngagement.DateTime).toLocaleDateString() : '';
                    var barHeight = isBitSet ? 100 : {minBarHeight};
                    chartHtml.push('<li title=""' + title + '""><span style=\'height: ' + barHeight + '%\'></span></li>');
                }}
            }}

            chartHtml.push('</ul>');
            $('.badge-attendance.badge-id-{badge.Id}').html(chartHtml.join(''));
        }}
    }},
}});";
        }

        /// <summary>
        /// Get the streak type described by the attribute value
        /// </summary>
        /// <param name="badge"></param>
        /// <returns></returns>
        private StreakTypeCache GetStreakTypeCache( BadgeCache badge )
        {
            var streakTypeGuid = GetAttributeValue( badge, AttributeKey.StreakType ).AsGuidOrNull();

            if ( !streakTypeGuid.HasValue )
            {
                return null;
            }

            return StreakTypeCache.Get( streakTypeGuid.Value );
        }
    }
}