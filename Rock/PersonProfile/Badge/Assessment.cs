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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.PersonProfile.Badge
{
    /// <summary>
    /// Badge that displays a person's Assessments results.
    /// </summary>
    [Description( "Badge that displays a person's Assessments results." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Assessment Badge" )]

    [AssessmentTypesField("Assessments To Show",
        Description = "Select the assessments that should be displayed in the badge. If none are selected then all assessments will be shown.",
        Key = AttributeKeys.AssessmentsToShow,
        IncludeInactive = false,
        IsRequired = false,
        Order = 0)]
    class Assessment : BadgeComponent
    {

        private class AttributeKeys
        {
            public const string AssessmentsToShow = "AssessmentsToShow";
        }

        private const string UNTAKEN_BADGE_COLOR = "#DBDBDB";

        public override void Render( BadgeCache badge, HtmlTextWriter writer )
        {
            if ( Person == null )
            {
                return;
            }

            string[] assessmentTypeGuids = new string[] { };

            // Create a list of assessments that should be included in the badge
            if ( !String.IsNullOrEmpty( GetAttributeValue( badge, AttributeKeys.AssessmentsToShow ) ) )
            {
                // Get from attribute if available
                var assessmentTypesGuidString = GetAttributeValue( badge, AttributeKeys.AssessmentsToShow );
                assessmentTypeGuids = assessmentTypesGuidString.IsNullOrWhiteSpace() ? null : assessmentTypesGuidString.Split( new char[] { ',' } );
            }
            else
            {
                // If none are selected then all are used.
                assessmentTypeGuids = new string[]
                {
                    Rock.SystemGuid.AssessmentType.CONFLICT,
                    Rock.SystemGuid.AssessmentType.DISC,
                    Rock.SystemGuid.AssessmentType.EQ,
                    Rock.SystemGuid.AssessmentType.GIFTS,
                    Rock.SystemGuid.AssessmentType.MOTIVATORS
                };
            }

            StringBuilder toolTipText = new StringBuilder();
            StringBuilder badgeIcons = new StringBuilder();

            foreach( var assessmentTypeGuid in assessmentTypeGuids )
            {
                var assessmentType = new AssessmentTypeService( new RockContext() ).GetNoTracking( assessmentTypeGuid.AsGuid() );
                string resultsPath = assessmentType.AssessmentResultsPath;
                string resultsPageUrl = System.Web.VirtualPathUtility.ToAbsolute( $"~{resultsPath}?Person={Person.UrlEncodedKey}" );
                string iconCssClass = assessmentType.IconCssClass;
                string badgeHtml = string.Empty;
                string assessmentTitle = string.Empty;
                var mergeFields = new Dictionary<string, object>();
                string mergedBadgeSummaryLava = "Not taken";

                switch ( assessmentTypeGuid.ToUpper() )
                {
                    case Rock.SystemGuid.AssessmentType.CONFLICT:

                        var conflictsThemes = new Dictionary<string, decimal>();
                        conflictsThemes.Add( "Winning", Person.GetAttributeValue( "core_ConflictThemeWinning" ).AsDecimalOrNull() ?? 0 );
                        conflictsThemes.Add( "Solving", Person.GetAttributeValue( "core_ConflictThemeSolving" ).AsDecimalOrNull() ?? 0 );
                        conflictsThemes.Add( "Accommodating", Person.GetAttributeValue( "core_ConflictThemeAccommodating" ).AsDecimalOrNull() ?? 0 );

                        string highestScoringTheme = conflictsThemes.Where( x => x.Value == conflictsThemes.Max( v => v.Value ) ).Select( x => x.Key ).FirstOrDefault() ?? string.Empty;
                        mergeFields.Add( "ConflictTheme", highestScoringTheme );
                        assessmentTitle = "Conflict Theme";
                        break;

                    case Rock.SystemGuid.AssessmentType.DISC:
                        assessmentTitle = "DISC";
                        break;

                    case Rock.SystemGuid.AssessmentType.EQ:
                        assessmentTitle = "EQ Self Aware";
                        break;

                    case Rock.SystemGuid.AssessmentType.GIFTS:
                        assessmentTitle = "Spiritual Gifts";
                        break;

                    case Rock.SystemGuid.AssessmentType.MOTIVATORS:
                        assessmentTitle = "Motivators";
                        break;
                }

                // Check if person has taken test
                var assessmentTest = new AssessmentService( new RockContext() )
                    .Queryable()
                    .Where( a => a.PersonAlias.PersonId == Person.Id )
                    .Where( a => a.AssessmentTypeId == assessmentType.Id )
                    .Where( a => a.Status == AssessmentRequestStatus.Complete )
                    .OrderByDescending( a => a.CreatedDateTime )
                    .FirstOrDefault();

                string badgeColor = assessmentTest != null ? assessmentType.BadgeColor : UNTAKEN_BADGE_COLOR;

                badgeIcons.AppendLine( $@"<div class='badge'>" );
                // If the latest request has been taken we want to link to it and provide a Lava merged summary
                if ( assessmentTest != null )
                {
                    badgeIcons.AppendLine( $@"<a href='{resultsPageUrl}' target='_blank'>" );

                    mergeFields.Add( "Person", Person );
                    mergedBadgeSummaryLava = assessmentType.BadgeSummaryLava.ResolveMergeFields( mergeFields );
                }

                badgeIcons.AppendLine( $@"
                        <span class='fa-stack'>
                            <i style='color:{badgeColor};' class='fa fa-circle fa-stack-2x'></i>
                            <i class='{iconCssClass} fa-stack-1x'></i>
                        </span>" );

                // Close the anchor for the linked assessment test
                if ( assessmentTest != null )
                {
                    badgeIcons.AppendLine( "</a>" );
                }

                badgeIcons.AppendLine( $@"</div>" );

                toolTipText.AppendLine( $@"
                    <p class='margin-b-sm'>
                        <span class='fa-stack'>
                            <i style='color:{assessmentType.BadgeColor};' class='fa fa-circle fa-stack-2x'></i>
                            <i style='font-size:15px; color:#ffffff;' class='{iconCssClass} fa-stack-1x'></i>
                        </span>
                        <strong>{assessmentTitle}:</strong> {mergedBadgeSummaryLava}
                    </p>" );
            }

            writer.Write( $@"<div class='badge badge-id-{badge.Id}'><div class='badge-grid' data-toggle='tooltip' data-html='true' data-sanitize='false' data-original-title=""{toolTipText.ToString()}"">" );
            writer.Write( badgeIcons.ToString() );
            writer.Write( "</div></div>" );
            writer.Write( $@"
                <script>
                    Sys.Application.add_load(function () {{
                        $('.badge-id-{badge.Id}').children('.badge-grid').tooltip({{ sanitize: false }});
                    }});
                </script>" );
        }
    }
}
