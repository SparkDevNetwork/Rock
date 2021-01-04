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
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Badge.Component
{
    /// <summary>
    /// Badge that displays a person's Assessments results.
    /// </summary>
    [Description( "Badge that displays a person's Assessments results." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Assessment Badge" )]

    [AssessmentTypesField( "Assessments To Show",
        Description = "Select the assessments that should be displayed in the badge. If none are selected then all assessments will be shown.",
        Key = AttributeKeys.AssessmentsToShow,
        IncludeInactive = false,
        IsRequired = false,
        Order = 0 )]
    public class Assessment : BadgeComponent
    {
        private class AttributeKeys
        {
            public const string AssessmentsToShow = "AssessmentsToShow";
        }

        private class AssessmentBadgeCssClasses
        {
            /// <summary>
            /// The CSS class to be assigned for assessments that have been taken
            /// </summary>
            public const string Taken = "taken";

            /// <summary>
            /// The CSS class to be assigned for assessments that have been requested
            /// </summary>
            public const string Requested = "requested";

            /// <summary>
            /// The CSS class be assigned for assessments that have not been requested and are not completed.
            /// </summary>
            public const string NotRequested = "not-requested";

            /// <summary>
            /// The CSS class used to indicate which <i></i> holds the assessment icon
            /// </summary>
            public const string AssessmentIcon = "assessment-icon";

            /// <summary>
            /// Each assessment type in the badge should have a class indicating it's type.
            /// The class will be named using this prefix and then the assessment title in all lower case
            /// with no spaces. e.g. "Spiritual Gifts" will get the "assessment-spiritualgifts" CSS class
            /// </summary>
            public const string AssessmentTypePrefix = "badge-assessment assessment-";
        }

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
        public override void Render( BadgeCache badge, HtmlTextWriter writer )
        {
            if ( Person == null )
            {
                return;
            }

            var assessmentTypes = new List<AssessmentTypeCache>();

            // Create a list of assessments that should be included in the badge
            if ( !string.IsNullOrEmpty( GetAttributeValue( badge, AttributeKeys.AssessmentsToShow ) ) )
            {
                // Get from attribute if available
                var assessmentTypesGuidString = GetAttributeValue( badge, AttributeKeys.AssessmentsToShow );
                var assessmentTypeGuids = assessmentTypesGuidString.IsNullOrWhiteSpace() ? null : assessmentTypesGuidString.Split( new char[] { ',' } );
                foreach ( var assessmentGuid in assessmentTypeGuids )
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

            var assessmentTestsTaken = new AssessmentService( new RockContext() )
                .Queryable()
                .AsNoTracking()
                .Where( a => a.PersonAlias != null
                             && a.PersonAlias.PersonId == Person.Id
                             && availableTypes.Contains( a.AssessmentTypeId ) )
                .OrderByDescending( a => a.CompletedDateTime ?? a.RequestedDateTime )
                .Select( a => new PersonBadgeAssessment { AssessmentTypeId = a.AssessmentTypeId, RequestedDateTime = a.RequestedDateTime, Status = a.Status } )
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
                var mergedBadgeSummaryLava = "Not requested";

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

                string assessmentTypeClass = AssessmentBadgeCssClasses.AssessmentTypePrefix + assessmentTitle.RemoveSpaces().ToLower();
                string assessmentStatusClass = AssessmentBadgeCssClasses.NotRequested;
                PersonBadgeAssessment previouslyCompletedAssessmentTest = null;

                // Get the status of the assessment
                var assessmentTests = assessmentTestsTaken.Where( t => t.AssessmentTypeId == assessmentType.Id ).ToList();
                PersonBadgeAssessment assessmentTest = null;
                if ( assessmentTests.Count > 0)
                {
                    assessmentTest = assessmentTests.First();
                    assessmentStatusClass = assessmentTest.Status == AssessmentRequestStatus.Pending ? AssessmentBadgeCssClasses.Requested : AssessmentBadgeCssClasses.Taken;
                }

                if ( assessmentTests.Count > 1 )
                {
                    // If the most recent one is completed then it is already set as the test and we can move on, the initial query ordered them by RequestedDateTime. Otherwise check if there are previoulsy completed assessments.
                    if ( assessmentTests[0].Status != AssessmentRequestStatus.Complete )
                    {
                        // If the most recent one is pending then check for a completed one prior, if found then we need to display the competed text and note that an the assessment has been requested. The link should go to the completed assessment.
                        previouslyCompletedAssessmentTest = assessmentTests.Where( a => a.Status == AssessmentRequestStatus.Complete ).FirstOrDefault();
                        if ( previouslyCompletedAssessmentTest != null )
                        {
                            // There is a new pending assessment and a previously completed assessment, display both classes
                            assessmentStatusClass = $"{AssessmentBadgeCssClasses.Requested} {AssessmentBadgeCssClasses.Taken}";
                        }
                    }
                }

                // Only set the color if the test has been taken.
                string badgeColorHtml = string.Empty;

                // If there is a completed request we want to link to it and provide a Lava merged summary
                if ( assessmentTest != null )
                {
                    if ( assessmentTest.Status == AssessmentRequestStatus.Complete || previouslyCompletedAssessmentTest != null )
                    {
                        badgeColorHtml = assessmentType.BadgeColor.IsNotNullOrWhiteSpace() ? $"style='color:{assessmentType.BadgeColor};' " : string.Empty;

                        badgeIcons.AppendLine( $@"<div {badgeColorHtml} class='badge {assessmentTypeClass} {assessmentStatusClass}'>" );
                        badgeIcons.AppendLine( $@"<a href='{resultsPageUrl}' target='_blank'>" );

                        mergeFields.Add( "Person", Person );
                        mergedBadgeSummaryLava = assessmentType.BadgeSummaryLava.ResolveMergeFields( mergeFields );
                    }

                    if ( assessmentTest.Status == AssessmentRequestStatus.Pending && previouslyCompletedAssessmentTest == null )
                    {
                        badgeIcons.AppendLine( $@"<div class='badge {assessmentTypeClass} {assessmentStatusClass}'>" );

                        // set the request string and requested datetime to the merged lava
                        mergedBadgeSummaryLava = $"Requested: {assessmentTest.RequestedDateTime.ToShortDateString()}";
                    }
                }
                else
                {
                    badgeIcons.AppendLine( $@"<div class='badge {assessmentTypeClass} {assessmentStatusClass}'>" );
                }

                badgeIcons.AppendLine( $@"
                        <span class='fa-stack'>
                            <i class='fa fa-circle fa-stack-2x'></i>
                            <i class='{assessmentType.IconCssClass} fa-stack-1x {AssessmentBadgeCssClasses.AssessmentIcon}'></i>
                        </span>" );

                // Close the anchor for the linked assessment test
                if ( assessmentTest != null )
                {
                    badgeIcons.AppendLine( "</a>" );
                }

                badgeIcons.AppendLine( $@"</div>" );

                string badgeToolTipColorHtml = assessmentType.BadgeColor.IsNotNullOrWhiteSpace() ? $"style='color:{assessmentType.BadgeColor};'" : string.Empty;
                toolTipText.AppendLine( $@"
                    <p class='margin-b-sm'>
                        <span {badgeToolTipColorHtml} class='{assessmentTypeClass}'>
                            <span class='fa-stack'>
                                <i class='fa fa-circle fa-stack-2x'></i>
                                <i class='{assessmentType.IconCssClass} fa-stack-1x {AssessmentBadgeCssClasses.AssessmentIcon}'></i>
                            </span>
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
        }

        /// <summary>
        /// Gets the java script.
        /// </summary>
        /// <param name="badge"></param>
        /// <returns></returns>
        protected override string GetJavaScript( BadgeCache badge )
        {
            return $"$('.badge-id-{badge.Id}').children('.badge-grid').tooltip({{ sanitize: false }});";
        }

        /// <summary>
        /// A private class just for the badge
        /// </summary>
        private class PersonBadgeAssessment
        {
            public int AssessmentTypeId { get; set; }

            public DateTime? RequestedDateTime { get; set; }

            public AssessmentRequestStatus Status { get; set; }
        }
    }
}
