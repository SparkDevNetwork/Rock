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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Attribute;
using Rock.Cms.StructuredContent;
using Rock.Data;
using Rock.Enums.Lms;
using Rock.Model;
using Rock.Net;

namespace Rock.Lms
{
    /// <summary>
    /// The Assessment activity is an activity that requires a partcipant to complete multiple items.
    /// </summary>
    [Description( "A Learning Activity that requires a partcipant to complete multiple items." )]
    [Export( typeof( LearningActivityComponent ) )]
    [ExportMetadata( "ComponentName", "Assessment" )]

    [RockInternal( "17.0" )]
    [Rock.SystemGuid.EntityTypeGuid( "a585c101-02e8-4953-bf77-c783c7cfdfdc" )]
    public class AssessmentComponent : LearningActivityComponent
    {
        #region Keys

        private static class SettingKey
        {
            public const string AssessmentTerm = "assessmentTerm";

            public const string Header = "header";

            public const string Items = "items";

            public const string MultipleChoiceWeight = "multipleChoiceWeight";

            public const string ShowMissedQuestionsOnResults = "shoeMissedQuestionsOnResults";

            public const string ShowResultsOnCompletion = "showResultsOnCompletion";
        }

        private static class CompletionKey
        {
            public const string CompletedItems = "completedItems";

            public const string MultipleChoiceWeight = "multipleChoiceWeight";
        }

        #endregion

        #region Properties

        /// <inheritdoc/>
        public override string HighlightColor => "#a9551d";

        /// <inheritdoc/>
        public override string IconCssClass => "fa fa-list";

        /// <inheritdoc/>
        public override string Name => "Assessment";

        /// <inheritdoc/>
        public override string ComponentUrl => @"/Obsidian/Controls/Internal/LearningActivity/assessmentLearningActivity.obs";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override Dictionary<string, string> GetActivityConfiguration( LearningClassActivity activity, Dictionary<string, string> componentData, PresentedFor presentation, RockContext rockContext, RockRequestContext requestContext )
        {
            if ( presentation == PresentedFor.Configuration )
            {
                return new Dictionary<string, string>();
            }
            else
            {
                var content = componentData.GetValueOrNull( SettingKey.Header );

                var headerHtml = content.IsNotNullOrWhiteSpace()
                    ? new StructuredContentHelper( content ).Render()
                    : string.Empty;

                if ( headerHtml.IsNotNullOrWhiteSpace() )
                {
                    var mergeFields = requestContext.GetCommonMergeFields();

                    headerHtml = headerHtml.ResolveMergeFields( mergeFields );
                }

                var items = componentData.GetValueOrNull( SettingKey.Items ).FromJsonOrNull<List<AssessmentItem>>()
                    ?? new List<AssessmentItem>();

                if ( presentation == PresentedFor.Student )
                {
                    // Hide the correct answer from the student.
                    foreach ( var item in items )
                    {
                        item.CorrectAnswer = null;
                    }
                }

                return new Dictionary<string, string>
                {
                    [SettingKey.AssessmentTerm] = componentData.GetValueOrNull( SettingKey.AssessmentTerm ),
                    [SettingKey.Header] = headerHtml,
                    [SettingKey.Items] = items.ToCamelCaseJson( false, false ),
                    [SettingKey.MultipleChoiceWeight] = componentData.GetValueOrNull( SettingKey.MultipleChoiceWeight ),
                    [SettingKey.ShowMissedQuestionsOnResults] = componentData.GetValueOrNull( SettingKey.ShowMissedQuestionsOnResults ),
                    [SettingKey.ShowResultsOnCompletion] = componentData.GetValueOrNull( SettingKey.ShowResultsOnCompletion )
                };
            }
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetCompletionValues( LearningClassActivityCompletion completion, Dictionary<string, string> completionData, Dictionary<string, string> componentData, PresentedFor presentation, RockContext rockContext, RockRequestContext requestContext )
        {
            // Note: We don't strip the correct from students here because
            // they have already answered the questions. The answers are now
            // included so we can display the result data to them.
            return completionData;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetCompletionData( LearningClassActivityCompletion completion, Dictionary<string, string> completionValues, Dictionary<string, string> componentData, PresentedFor presentation, RockContext rockContext, RockRequestContext requestContext )
        {
            var completionData = new Dictionary<string, string>( completionValues );

            if ( presentation == PresentedFor.Student )
            {
                var items = componentData.GetValueOrNull( SettingKey.Items ).FromJsonOrNull<List<AssessmentItem>>()
                    ?? new List<AssessmentItem>();
                var completedItems = completionData.GetValueOrNull( CompletionKey.CompletedItems ).FromJsonOrNull<List<AssessmentItem>>()
                    ?? new List<AssessmentItem>();

                // Restore the correct value when this is coming from the student
                // so we can properly display it on the summary screen later. But
                // only restore the answer if it hasn't already been set, otherwise
                // the facilitator might change the answer after it was responded to
                // and skew the results.
                foreach ( var item in completedItems )
                {
                    if ( item.CorrectAnswer == null )
                    {
                        item.CorrectAnswer = items.FirstOrDefault( i => i.UniqueId == item.UniqueId )?.CorrectAnswer;
                    }
                }

                completionData[CompletionKey.CompletedItems] = completedItems.ToCamelCaseJson( false, false );
            }

            return completionData;
        }

        /// <inheritdoc/>
        public override int? CalculatePointsEarned( LearningClassActivityCompletion completion, Dictionary<string, string> completionData, Dictionary<string, string> componentData, int pointsPossible, RockContext rockContext, RockRequestContext requestContext )
        {
            var multipleChoiceSectionPoints = GetMultipleChoiceSectionPoints( componentData, completionData, pointsPossible );
            var shortAnswerSectionPoints = GetShortAnswerSectionPoints( componentData, completionData );

            if ( shortAnswerSectionPoints.HasValue )
            {
                return null;
            }

            return multipleChoiceSectionPoints.HasValue
                ? ( int? ) Math.Round( multipleChoiceSectionPoints.Value )
                : null;
        }

        /// <inheritdoc/>
        public override bool RequiresGrading( LearningClassActivityCompletion completion, Dictionary<string, string> completionData, Dictionary<string, string> componentData, RockContext rockContext, RockRequestContext requestContext )
        {
            if ( completion.GradedByPersonAliasId.HasValue )
            {
                return false;
            }

            var completionHasShortAnswer = HasShortAnswerItems( completionData, "completedItems" );

            if ( completionHasShortAnswer.HasValue )
            {
                return completionHasShortAnswer.Value;
            }

            return HasShortAnswerItems( componentData, "items" ) ?? false;
        }

        /// <summary>
        /// Calculates the points earned for the multiple choice section of the
        /// assessment.
        /// </summary>
        /// <param name="componentData">The component configuraiton data.</param>
        /// <param name="completionData">The completion values from being submitted by student.</param>
        /// <param name="pointsPossible">The maximum number of points possible.</param>
        /// <returns>The number of points from correct answers or <c>null</c> if there were no multiple choice questions.</returns>
        private decimal? GetMultipleChoiceSectionPoints( Dictionary<string, string> componentData, Dictionary<string, string> completionData, int pointsPossible )
        {
            try
            {
                var correctMultipleChoiceItems = 0;
                var configuredItems = componentData["items"].FromJsonOrNull<List<AssessmentItem>>()
                    ?.Where( item => item.Type == AssessmentItemType.MultipleChoice )
                    .ToList()
                    ?? new List<AssessmentItem>();
                var completedItems = completionData["completedItems"].FromJsonOrNull<List<AssessmentItem>>()
                    ?? new List<AssessmentItem>();
                var multipleChoiceWeight = componentData["multipleChoiceWeight"].AsDecimal();

                if ( !configuredItems.Any() )
                {
                    return null;
                }

                foreach ( var question in configuredItems )
                {
                    if ( question.UniqueId == Guid.Empty )
                    {
                        continue;
                    }

                    var correctAnswer = question.CorrectAnswer.ToStringSafe();
                    var response = completedItems.FirstOrDefault( item => item.UniqueId == question.UniqueId )?.Response ?? string.Empty;

                    if ( correctAnswer.Equals( response, StringComparison.OrdinalIgnoreCase ) )
                    {
                        correctMultipleChoiceItems++;
                    }
                }

                decimal multipleChoiceItemCount = configuredItems.Count();
                var sectionWeight = multipleChoiceWeight / 100;
                var availablePoints = pointsPossible * sectionWeight;
                var percentCorrect = correctMultipleChoiceItems / multipleChoiceItemCount;
                var pointsEarned = availablePoints * percentCorrect;

                return pointsEarned;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
            }

            return null;
        }

        /// <summary>
        /// Calculates the points given by the facilitator for the non-multiple
        /// choice questions in the assessment.
        /// </summary>
        /// <param name="componentData">The component configuraiton data.</param>
        /// <param name="completionData">The completion values from being submitted by student.</param>
        /// <returns>The number of points from correct answers or <c>null</c> if there were no non-multiple choice questions.</returns>
        private decimal? GetShortAnswerSectionPoints( Dictionary<string, string> componentData, Dictionary<string, string> completionData )
        {
            decimal? pointsEarned = null;

            try
            {
                var configuredItems = componentData["items"].FromJsonOrNull<List<AssessmentItem>>()
                    ?.Where( item => item.Type == AssessmentItemType.MultipleChoice )
                    .ToList()
                    ?? new List<AssessmentItem>();
                var completedItems = completionData["completedItems"].FromJsonOrNull<List<AssessmentItem>>()
                    ?? new List<AssessmentItem>();

                foreach ( var question in configuredItems )
                {
                    if ( question.UniqueId == Guid.Empty )
                    {
                        continue;
                    }

                    var facilitatorScore = completedItems.FirstOrDefault( item => item.UniqueId == question.UniqueId )?.PointsEarned;

                    if ( facilitatorScore.HasValue )
                    {
                        pointsEarned += facilitatorScore.Value;
                    }
                }

            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
            }

            return pointsEarned;
        }

        /// <summary>
        /// Checks the configuration JSON for any items of the "Short Answer" type.
        /// </summary>
        /// <param name="data">The component data or completion data.</param>
        /// <param name="itemsPropertyNames">The name of the property containing the items.</param>
        /// <returns><c>true</c>If able to positively determine the configuration contains "Short Answer" items; otherwise <c>false</c>.</returns>
        private bool? HasShortAnswerItems( Dictionary<string, string> data, string itemsPropertyNames )
        {
            try
            {
                var items = data[itemsPropertyNames].FromJsonOrNull<List<AssessmentItem>>() ?? new List<AssessmentItem>();

                return items.Any( item => item.Type == AssessmentItemType.ShortAnswer );
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Support Classes

        private enum AssessmentItemType
        {
            MultipleChoice = 0,
            Section = 1,
            ShortAnswer = 2
        }

        private class AssessmentItem
        {
            public AssessmentItemType Type { get; set; }

            public Guid UniqueId { get; set; }

            public bool? HasBeenGraded { get; set; }

            public int Order { get; set; }

            public decimal? PointsEarned { get; set; }

            public string Response { get; set; }

            public List<string> Answers { get; set; }

            public string CorrectAnswer { get; set; }

            public string HelpText { get; set; }

            public string Question { get; set; }

            public string Title { get; set; }

            public string Summary { get; set; }

            public int? AnswerBoxRows { get; set; }

            public int? MaxCharacterCount { get; set; }

            public decimal? PointsPossible { get; set; }

            public decimal? QuestionWeight { get; set; }
        }

        #endregion
    }
}
