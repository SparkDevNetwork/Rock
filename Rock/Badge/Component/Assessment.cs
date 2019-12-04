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

namespace Rock.Badge.Component
{
    /// <summary>
    /// Badge that displays a person's Assessments results.
    /// </summary>
    [Description( "Badge that displays a person's Assessments results." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Assessment Badge" )]

    [AssessmentTypesField( "Assessments To Show",
        Description = "Select the assesements that should be displayed in the badge. If none are selected then all assessments will be shown.",
        Key = AttributeKeys.AssessmentsToShow,
        IncludeInactive = false,
        IsRequired = false,
        Order = 0 )]
    class Assessment : BadgeComponent
    {
        private class AttributeKeys
        {
            public const string AssessmentsToShow = "AssessmentsToShow";
        }

        private const string UNTAKEN_BADGE_COLOR = "#DBDBDB";

        /// <summary>
        /// Determines of this badge component applies to the given type
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public override bool DoesApplyToEntityType( string type )
        {
            return type.IsNullOrWhiteSpace() || typeof( Person ).FullName == type;
        }

        public override void Render( BadgeCache badge, HtmlTextWriter writer )
        {
            if ( Person == null )
            {
                return;
            }

            var assessmentTypes = new List<AssessmentTypeCache>();

            // Create a list of assessments that should be included in the badge
            if ( !String.IsNullOrEmpty( GetAttributeValue( badge, AttributeKeys.AssessmentsToShow ) ) )
            {
                // Get from attribute if available
                var assessmentTypesGuidString = GetAttributeValue( badge, AttributeKeys.AssessmentsToShow );
                var assessmentTypeGuids = assessmentTypesGuidString.IsNullOrWhiteSpace() ? null : assessmentTypesGuidString.Split( new char[] { ',' } );
                foreach( var assessmentGuid in assessmentTypeGuids )
                {
                    assessmentTypes.Add( AssessmentTypeCache.Get( assessmentGuid ) );
                }
            }
            else
            {
                // If none are selected then all are used.
                assessmentTypes = AssessmentTypeCache.All();
            }

            // Need a list of primitive types for assessmentTestsTaken linq
            var availableTypes = assessmentTypes.Select( t => t.Id ).ToList();

            // Get a list of all of the tests a the person has taken that the component is configured to show.
            var assessmentTestsTaken = new AssessmentService( new RockContext() )
                .Queryable()
                .Where( a => a.PersonAlias.PersonId == Person.Id )
                .Where( a => a.Status == AssessmentRequestStatus.Complete )
                .Where( a => availableTypes.Contains( a.AssessmentTypeId ) )
                .OrderByDescending( a => a.CreatedDateTime )
                .ToList();

            StringBuilder toolTipText = new StringBuilder();
            StringBuilder badgeRow1 = new StringBuilder( $@"<div class='badge-row'>" );
            StringBuilder badgeRow2 = new StringBuilder();

            if ( assessmentTypes.Count > 1 )
            {
                badgeRow2.AppendLine( $@"<div class='badge-row'>" );
            }

            for ( int i = 0; i < assessmentTypes.Count; i++ )
            {
                StringBuilder badgeIcons = new StringBuilder();

                badgeIcons = i % 2 == 0 ? badgeRow1 : badgeRow2;

                var assessmentType = assessmentTypes[i];
                var resultsPageUrl = System.Web.VirtualPathUtility.ToAbsolute( $"~{assessmentType.AssessmentResultsPath}?Person={this.Person.GetPersonActionIdentifier( "Assessment" ) }" );
                var assessmentTitle = assessmentType.Title;
                var mergeFields = new Dictionary<string, object>();
                var mergedBadgeSummaryLava = "Not taken";

                switch ( assessmentType.Guid.ToString().ToUpper() )
                {
                    case Rock.SystemGuid.AssessmentType.CONFLICT:

                        var conflictsThemes = new Dictionary<string, decimal>
                        {
                            { "Winning", Person.GetAttributeValue( "core_ConflictThemeWinning" ).AsDecimalOrNull() ?? 0 },
                            { "Solving", Person.GetAttributeValue( "core_ConflictThemeSolving" ).AsDecimalOrNull() ?? 0 },
                            { "Accommodating", Person.GetAttributeValue( "core_ConflictThemeAccommodating" ).AsDecimalOrNull() ?? 0 }
                        };

                        string highestScoringTheme = conflictsThemes.Where( x => x.Value == conflictsThemes.Max( v => v.Value ) ).Select( x => x.Key ).FirstOrDefault() ?? string.Empty;
                        mergeFields.Add( "ConflictTheme", highestScoringTheme );
                        assessmentTitle = "Conflict Theme";
                        break;

                    case Rock.SystemGuid.AssessmentType.EQ:
                        assessmentTitle = "EQ Self Aware";
                        break;
                }

                var assessmentTest = assessmentTestsTaken.Where( t => t.AssessmentTypeId == assessmentType.Id ).FirstOrDefault();
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
                            <i class='{assessmentType.IconCssClass} fa-stack-1x'></i>
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
                            <i style='font-size:15px; color:#ffffff;' class='{assessmentType.IconCssClass} fa-stack-1x'></i>
                        </span>
                        <strong>{assessmentTitle}:</strong> {mergedBadgeSummaryLava}
                    </p>" );
            }

            badgeRow1.AppendLine( $@"</div>" );

            if ( assessmentTypes.Count > 1 )
            {
                badgeRow2.AppendLine( $@"</div>" );
            }

            writer.Write( $@" <div class='badge badge-id-{badge.Id}'><div class='badge-grid' data-toggle='tooltip' data-html='true' data-sanitize='false' data-original-title=""{toolTipText.ToString()}"">" );
            writer.Write( badgeRow1.ToString() );
            writer.Write( badgeRow2.ToString() );
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
