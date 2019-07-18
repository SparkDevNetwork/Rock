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

namespace Rock.PersonProfile.Badge
{
    /// <summary>
    /// Sequence Badge
    /// </summary>
    [Description( "Shows a chart of the engagement history with each bar representing one month or day depending on the sequence." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Sequence Engagement" )]

    [SequenceField(
        name: "Sequence Id",
        description: "The sequence to display enrollment data for the given person about",
        required: true,
        key: AttributeKeys.Sequence )]

    [IntegerField(
        name: "Units To Display",
        description: "The number of days or months to show on the chart (default 24.)",
        required: false,
        defaultValue: AttributeDefaults.BarCount,
        key: AttributeKeys.BarCount )]

    [IntegerField(
        name: "Minimum Bar Height",
        description: "The minimum height of a bar (in pixels). Useful for showing hint of bar when attendance was 0. (default 2.)",
        required: false,
        defaultValue: AttributeDefaults.MinBarHeight,
        key: AttributeKeys.MinBarHeight )]

    [BooleanField(
        name: "Animate Bars",
        description: "Determine whether bars should animate when displayed.",
        defaultValue: AttributeDefaults.AnimateBars,
        key: AttributeKeys.AnimateBars )]

    [LinkedPage(
        name: "Enrollment Detail Page",
        description: "If set, clicking this badge will navigate to the given page.",
        required: false,
        key: AttributeKeys.EnrollmentDetailPage )]

    public class SequenceEngagement : BadgeComponent
    {
        #region Keys

        /// <summary>
        /// Keys to use for the attributes
        /// </summary>
        private static class AttributeKeys
        {
            public const string Sequence = "Sequence";
            public const string BarCount = "BarCount";
            public const string MinBarHeight = "MinBarHeight";
            public const string AnimateBars = "AnimateBars";
            public const string EnrollmentDetailPage = "EnrollmentDetailPage";
        }

        /// <summary>
        /// Default values for the attributes
        /// </summary>
        private static class AttributeDefaults
        {
            public const int BarCount = 24;
            public const int MinBarHeight = 2;
            public const bool AnimateBars = true;
        }

        #endregion Keys

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
            
            var sequence = GetSequence( badge );

            if ( sequence == null )
            {
                return;
            }

            var isDaily = sequence.OccurrenceFrequency == SequenceOccurrenceFrequency.Daily;
            var timeUnit = isDaily ? "day" : "week";
            var timeUnits = isDaily ? "days" : "weeks";

            var minBarHeight = GetAttributeValue( badge, AttributeKeys.MinBarHeight ).AsIntegerOrNull() ?? AttributeDefaults.MinBarHeight;
            var unitsToDisplay = GetAttributeValue( badge, AttributeKeys.BarCount ).AsIntegerOrNull() ?? AttributeDefaults.BarCount;
            var doAnimateBars = GetAttributeValue( badge, AttributeKeys.AnimateBars ).AsBooleanOrNull() ?? AttributeDefaults.AnimateBars;

            var animateClass = doAnimateBars ? " animate" : string.Empty;

            var tooltip = $"{Person.NickName.ToPossessive().EncodeHtml()} attendance for the last {unitsToDisplay} {timeUnits}. Each bar is a {timeUnit}.";

            var chartHtml = $"<div class='badge badge-attendance{animateClass} badge-id-{badge.Id}' data-toggle='tooltip' data-original-title='{tooltip}'></div>";

            var script = $@"
<script>
    Sys.Application.add_load(function () {{
        $.ajax({{
                type: 'GET',
                url: Rock.settings.get('baseUrl') + 'api/Sequences/RecentEngagement/{sequence.Id}/{Person.Id}?unitCount={unitsToDisplay}' ,
                statusCode: {{
                    200: function (data, status, xhr) {{
                            var chartHtml = ['<ul class=\'attendance-chart list-unstyled\'>'];

                            if (data) {{
                                for(var i = data.length - 1; i >= 0; i--) {{
                                    var isBitSet = data[i];
                                    var barHeight = isBitSet ? 100 : {minBarHeight};                                
                                    chartHtml.push('<li><span style=\'height: ' + barHeight + '%\'></span></li>');
                                }}
                            }}

                            chartHtml.push('</ul>');
                            $('.badge-attendance.badge-id-{badge.Id}').html(chartHtml.join(''));

                        }}
                }},
        }});
    }});
</script>";

            var linkedPageGuid = GetAttributeValue( badge, AttributeKeys.EnrollmentDetailPage ).AsGuidOrNull();
            var linkedPageId = linkedPageGuid.HasValue ? PageCache.GetId( linkedPageGuid.Value ) : null;

            if ( !linkedPageId.HasValue )
            {
                writer.Write( $"{chartHtml}{script}" );
            }
            else
            {
                var link = $"/page/{linkedPageId.Value}?sequenceId={sequence.Id}&personId={Person.Id}";                
                writer.Write( $@"<a href=""{link}"">{chartHtml}</a>{script}" );
            }
        }

        /// <summary>
        /// Get the sequence described by the attribute value
        /// </summary>
        /// <param name="badge"></param>
        /// <returns></returns>
        private SequenceCache GetSequence( BadgeCache badge )
        {
            var sequenceGuid = GetAttributeValue( badge, AttributeKeys.Sequence ).AsGuidOrNull();

            if ( !sequenceGuid.HasValue )
            {
                return null;
            }

            return SequenceCache.Get( sequenceGuid.Value );
        }
    }
}