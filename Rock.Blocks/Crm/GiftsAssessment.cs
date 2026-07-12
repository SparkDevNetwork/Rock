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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Model;
using Rock.ViewModels.Blocks.Crm.GiftsAssessment;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Calculates a person's spiritual gifts assessment score based on a series of question answers.
    /// </summary>
    [DisplayName( "Gifts Assessment" )]
    [Category( "CRM" )]
    [Description( "Allows you to take a spiritual gifts test and saves your spiritual gifts score." )]

    #region Block Attributes

    [CodeEditorField(
        "Instructions",
        Key = AttributeKey.Instructions,
        Description = "The text (HTML) to display at the top of the instructions section.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",
        EditorMode = CodeEditorMode.Html,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = InstructionsDefaultValue,
        Order = 0 )]

    [TextField(
        "Set Page Title",
        Key = AttributeKey.SetPageTitle,
        Description = "The text to display as the heading.",
        IsRequired = false,
        DefaultValue = "Spiritual Gifts Assessment",
        Order = 1 )]

    [TextField(
        "Set Page Icon",
        Key = AttributeKey.SetPageIcon,
        Description = "The css class name to use for the heading icon.",
        IsRequired = false,
        DefaultValue = "ti ti-gift",
        Order = 2 )]

    [IntegerField(
        "Number of Questions",
        Key = AttributeKey.NumberOfQuestions,
        Description = "The number of questions to show per page while taking the test",
        IsRequired = true,
        DefaultIntegerValue = 17,
        Order = 3 )]

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Primary )]
    [Rock.SystemGuid.EntityTypeGuid( "56ABBA9A-5770-46DB-9146-75DF99DBDC78" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "ED6F73FB-1818-47AD-B7DD-26359CF686AA" )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.GIFTS_ASSESSMENT )]
    public class GiftsAssessment : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string Instructions = "Instructions";
            public const string SetPageTitle = "SetPageTitle";
            public const string SetPageIcon = "SetPageIcon";
            public const string NumberOfQuestions = "NumberofQuestions";
        }

        private static class PageParameterKey
        {
            public const string AssessmentId = "AssessmentId";
            public const string Person = "Person";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// The assessment identifier from the page parameter, resolved from an Id, IdKey, or Guid.
        /// Returns <c>null</c> when the parameter is absent, or <c>0</c> for the individual-directed (re)take sentinel.
        /// </summary>
        private int? AssessmentId
        {
            get
            {
                var assessmentKey = PageParameter( PageParameterKey.AssessmentId );

                if ( assessmentKey.IsNullOrWhiteSpace() )
                {
                    return null;
                }

                // A literal "0" is a sentinel for an individual-directed (re)take rather than a reference to an existing assessment.
                if ( assessmentKey == "0" )
                {
                    return 0;
                }

                return new AssessmentService( RockContext ).Get( assessmentKey, !PageCache.Layout.Site.DisablePredictableIds )?.Id;
            }
        }

        /// <summary>
        /// The PersonKey from the page parameter.
        /// </summary>
        private string PersonKey => PageParameter( PageParameterKey.Person );

        /// <summary>
        /// The panel title block setting.
        /// </summary>
        private string PanelTitle => GetAttributeValue( AttributeKey.SetPageTitle ).ToStringSafe();

        /// <summary>
        /// The panel icon block setting.
        /// </summary>
        private string PanelIcon => GetAttributeValue( AttributeKey.SetPageIcon ).ToStringSafe();

        /// <summary>
        /// Gets the number of questions to show per page (defaults to 17 if unable to parse).
        /// </summary>
        private int PageSize => GetAttributeValue( AttributeKey.NumberOfQuestions ).ToIntSafe( 17 );

        /// <summary>
        /// Gets the target person from the "Person" query string key, or the current person if not provided.
        /// </summary>
        private Person TargetPerson
        {
            get
            {
                var personKey = PersonKey;

                // Set the target person according to the parameter or use the current user if not provided.
                if ( personKey.IsNotNullOrWhiteSpace() )
                {
                    var personService = new PersonService( RockContext );
                    return personService.GetByPersonActionIdentifier( personKey, "Assessment" ) ?? personService.GetByUrlEncodedKey( personKey );
                }

                return GetCurrentPerson();
            }
        }

        private const string InstructionsDefaultValue = @"
<h2>Welcome to Your Spiritual Gifts Assessment</h2>
<p>
    {{ Person.NickName }}, we are all called to a unique role in the church body, and are equipped with
    the gifts required for this calling. This assessment identifies the common spiritual gifts that
    you possess.
</p>
<p>
    Don’t spend too much time thinking about your answer. Usually, your first responses is your most
    natural. Since there are no right or wrong answers, just go with your instinct.
</p>";

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return GetGiftsBox();
        }

        /// <summary>
        /// Gets the Spiritual Gifts initialization box with populated values or with an error message.
        /// </summary>
        /// <returns>The populated box.</returns>
        private GiftsAssessmentInitializationBox GetGiftsBox()
        {
            /*
                2020-01-09 - ETD
                This block will either show the assessment results of the most recent assessment test or give the assessment test.
                The following use cases are considered:
                1. If the assessment ID "0" was provided then create a new test for the current user. This covers user directed retakes.
                2. If the assessment ID was provided and is not "0"
                    Note: The assessment results are stored on the person's attributes and are overwritten if the assessment is retaken. So past Assessments will not be loaded by this block.
                    The test data is saved in the assessment table but would need to be recomputed, which may be a future feature.
                    a. The assessment ID is ignored and the current person is used.
                    b. If the assessment exists for the current person and is completed then show the results.
                    c. If the assessment exists for the current person and is pending then show the questions.
                    d. If the assessment does not exist for the current person then nothing loads.
                3. If the assessment ID was not provided and the PersonKey was provided
                    a. If there is only one test of the type
                        1. If the assessment is completed show the results.
                        2. If the assessment is pending and the current person is the one assigned the test then show the questions.
                        3. If the assessment is pending and the current person is not the one assigned then show a message that the test has not been completed.
                    b. If more than one of type
                        1. If the latest requested assessment is completed show the results.
                        2. If the latest requested assessment is pending and the current person is the one assigned then show the questions.
                        3. If the latest requested assessment is pending and the current person is not the one assigned the show the results of the last completed test.
                        4. If the latest requested assessment is pending and the current person is not the one assigned and there are no previous completed assessments then show a message that the test has not been completed.
                4. If an assessment ID or PersonKey were not provided or are not valid then show an error message.
             */
            // Resolve the assessment key (Id, IdKey, or Guid) once so it isn't looked up multiple times.
            var assessmentId = AssessmentId;

            var box = new GiftsAssessmentInitializationBox
            {
                PanelTitle = PanelTitle,
                PanelIcon = PanelIcon,
                PageSize = PageSize,
                StartDateTime = RockDateTime.Now,
                AssessmentId = assessmentId
            };

            var targetPerson = TargetPerson;
            var currentPerson = GetCurrentPerson();
            var hasQueryStringForPersonKey = PersonKey.IsNotNullOrWhiteSpace();

            // Ensure we're able to get the target person; otherwise return an error message.
            if ( targetPerson == null )
            {
                box.ErrorMessage = hasQueryStringForPersonKey
                    ? "There is an issue locating the person associated with the request."
                    : "You must be signed in to take the assessment.";
                return box;
            }

            box.IsAssessmentForCurrentPerson = targetPerson.Id == currentPerson?.Id;
            box.Instructions = GetInstructions( targetPerson );
            box.Responses = GetAssessmentResponses();

            var assessmentType = new AssessmentTypeService( RockContext ).Get( Rock.SystemGuid.AssessmentType.GIFTS.AsGuid() );
            Assessment assessment = null;
            Assessment previouslyCompletedAssessment = null;

            // A "0" value indicates that the block should create a new assessment instead of looking for an existing one, so keep assessment null. e.g. a user directed re-take.
            if ( assessmentId != 0 )
            {
                var assessments = new AssessmentService( RockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( a =>
                        a.PersonAlias != null
                        && a.PersonAlias.PersonId == targetPerson.Id
                        && a.AssessmentTypeId == assessmentType.Id )
                    .OrderByDescending( a => a.CompletedDateTime ?? a.RequestedDateTime )
                    .ToList();

                if ( assessmentId == null && !assessments.Any() )
                {
                    // For this to happen the following is assumed to be true:
                    //   1. the individual has never taken the assessment,
                    //   2. the individual isn't using a link with the assessment ID, AND
                    //   3. they are arriving at the block directly rather than through the assessment list block.
                    // So treat this as an individual directed take/retake.
                    assessmentId = 0;
                }
                else
                {
                    if ( assessments.Count > 0 )
                    {
                        // If there are any results then pick the first one. If the assessment ID was specified then the query will only return one result.
                        assessment = assessments[0];
                    }

                    if ( assessments.Count > 1 && assessment.Status == AssessmentRequestStatus.Pending )
                    {
                        // If the most recent assessment is pending then check for a prior completed one.
                        previouslyCompletedAssessment = assessments.FirstOrDefault( a => a.Status == AssessmentRequestStatus.Complete );
                    }
                }
            }

            if ( assessment == null )
            {
                // If the assessment is null and the assessment id is 0 this is individual directed.
                // If the type doesn't require a request then return the box so the instructions are shown.
                if ( assessmentId == 0 && !assessmentType.RequiresRequest )
                {
                    box.AssessmentId = 0;
                    return box;
                }

                box.ErrorMessage = "Sorry, this test requires a request from someone before it can be taken.";
                return box;
            }

            box.AssessmentId = assessment.Id;

            // If the assessment is completed show the results.
            if ( assessment.Status == AssessmentRequestStatus.Complete )
            {
                var savedScores = SpiritualGiftsService.LoadSavedAssessmentResults( targetPerson, assessment );
                SetResult( savedScores, assessment, assessmentType, box );
                return box;
            }

            if ( assessment.Status == AssessmentRequestStatus.Pending )
            {
                if ( !box.IsAssessmentForCurrentPerson )
                {
                    // If the assessment is pending and the current person is not the one assigned then show the previously completed results.
                    if ( previouslyCompletedAssessment != null )
                    {
                        var savedScores = SpiritualGiftsService.LoadSavedAssessmentResults( targetPerson, previouslyCompletedAssessment );
                        box.InfoMessage = "A more recent assessment request has been made but has not been taken. Displaying the most recently completed test.";
                        SetResult( savedScores, previouslyCompletedAssessment, assessmentType, box );
                        return box;
                    }

                    // If there is no previously completed assessment then show a message that the test has not been completed.
                    box.ErrorMessage = $"{targetPerson.FullName} has not yet taken the {assessmentType.Title} Assessment.";
                    return box;
                }

                // The assessment is pending and the current person is the one assigned, so show the questions (results remain null).
                return box;
            }

            // This should never happen; if the block gets to this point then something is not right.
            box.ErrorMessage = "Unable to load assessment";
            return box;
        }

        /// <summary>
        /// Resolves the merge fields in the Instructions Lava template.
        /// </summary>
        /// <param name="targetPerson">The person the assessment is for (used in the Lava template).</param>
        /// <returns>A Lava resolved string.</returns>
        private string GetInstructions( Person targetPerson )
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, targetPerson );
            if ( targetPerson != null )
            {
                mergeFields.Add( "Person", targetPerson );
            }

            return GetAttributeValue( AttributeKey.Instructions ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Gets the assessment questions in a random order.
        /// </summary>
        /// <returns>A list of unanswered assessment responses.</returns>
        private List<AssessmentResponseBag> GetAssessmentResponses()
        {
            // GetQuestions() already returns the questions in a randomized order.
            return SpiritualGiftsService.GetQuestions()
                .Select( q => new AssessmentResponseBag
                {
                    Code = q.Key,
                    Question = q.Value
                } )
                .ToList();
        }

        /// <summary>
        /// Scores the responses, persists the assessment, and populates the result properties on the box.
        /// </summary>
        /// <param name="box">The box whose responses are saved and whose result properties are populated.</param>
        /// <param name="assessmentType">The Gifts assessment type the responses are saved against.</param>
        private void SaveAssessment( GiftsAssessmentInitializationBox box, AssessmentType assessmentType )
        {
            var targetPerson = TargetPerson;

            var responseData = box.Responses
                .Where( r => r.Response.HasValue )
                .ToDictionary( r => r.Code, r => r.Response.Value );

            // Score the responses and save the gift results to the person's attributes.
            var result = SpiritualGiftsService.GetResult( responseData );
            SpiritualGiftsService.SaveAssessmentResults( targetPerson, result );

            var assessmentService = new AssessmentService( RockContext );
            Assessment assessment = null;

            // A box assessment id greater than zero updates the existing assessment; otherwise a new one is created (e.g. a retake).
            // The lookup is scoped to the target person and the Gifts type so a forged or foreign id cannot overwrite another person's assessment.
            var effectiveAssessmentId = box.AssessmentId ?? 0;
            if ( effectiveAssessmentId > 0 )
            {
                assessment = assessmentService.Queryable()
                    .FirstOrDefault( a =>
                        a.Id == effectiveAssessmentId
                        && a.AssessmentTypeId == assessmentType.Id
                        && a.PersonAlias != null
                        && a.PersonAlias.PersonId == targetPerson.Id );
            }

            if ( assessment == null )
            {
                assessment = new Assessment
                {
                    AssessmentTypeId = assessmentType.Id,
                    PersonAliasId = targetPerson.PrimaryAliasId.Value
                };
                assessmentService.Add( assessment );
            }

            var startTime = box.StartDateTime ?? assessment.CreatedDateTime ?? RockDateTime.Now;
            assessment.Status = AssessmentRequestStatus.Complete;
            assessment.CompletedDateTime = RockDateTime.Now;
            assessment.AssessmentResultData = new SpiritualGiftsService.AssessmentResultData
            {
                Result = responseData,
                ResultScores = result.SpiritualGiftScores,
                TimeToTake = RockDateTime.Now.Subtract( startTime ).TotalSeconds
            }.ToJson();

            RockContext.SaveChanges();

            SetResult( result, assessment, assessmentType, box );
        }

        /// <summary>
        /// Populates the result properties on the box from the saved scores.
        /// </summary>
        /// <param name="result">The assessment results used to populate the box.</param>
        /// <param name="assessment">The assessment whose completion date determines retake eligibility.</param>
        /// <param name="assessmentType">The assessment type whose configuration determines retake eligibility.</param>
        /// <param name="box">The box whose properties should be populated.</param>
        private void SetResult( SpiritualGiftsService.AssessmentResults result, Assessment assessment, AssessmentType assessmentType, GiftsAssessmentInitializationBox box )
        {
            var hasQueryStringForPersonKey = PersonKey.IsNotNullOrWhiteSpace();

            // A request-required assessment cannot be self-retaken; a fresh request is needed (parity with WebForms).
            box.CanRetakeTest =
                !hasQueryStringForPersonKey
                && !assessmentType.RequiresRequest
                && assessment.CompletedDateTime.HasValue
                && assessment.CompletedDateTime.Value.AddDays( assessmentType.MinimumDaysToRetake ) <= RockDateTime.Now;

            var spiritualGifts = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.SPIRITUAL_GIFTS.AsGuid() );

            box.Results = new SpiritualGiftsResultBag
            {
                DominantGifts = MapGifts( spiritualGifts, result.DominantGifts ),
                SupportiveGifts = MapGifts( spiritualGifts, result.SupportiveGifts ),
                OtherGifts = MapGifts( spiritualGifts, result.OtherGifts ),
                GiftScores = result.SpiritualGiftScores?
                    .OrderByDescending( s => s.Percentage )
                    .ThenBy( s => s.SpiritualGiftName )
                    .Select( s => new SpiritualGiftScoreBag
                    {
                        SpiritualGiftName = s.SpiritualGiftName,
                        Percentage = s.Percentage
                    } )
                    .ToList()
            };
        }

        /// <summary>
        /// Maps a list of spiritual gift defined value Guids to their name and description, preserving defined type order.
        /// </summary>
        /// <param name="spiritualGifts">The Spiritual Gifts defined type cache.</param>
        /// <param name="giftGuids">The gift defined value Guids to map.</param>
        /// <returns>The mapped spiritual gifts.</returns>
        private static List<SpiritualGiftBag> MapGifts( DefinedTypeCache spiritualGifts, List<Guid> giftGuids )
        {
            if ( spiritualGifts == null || giftGuids == null )
            {
                return new List<SpiritualGiftBag>();
            }

            return spiritualGifts.DefinedValues
                .Where( dv => giftGuids.Contains( dv.Guid ) )
                .Select( dv => new SpiritualGiftBag
                {
                    Name = dv.Value,
                    Description = dv.Description
                } )
                .ToList();
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Returns the server time at the moment the individual begins the test so the persisted
        /// TimeToTake measures from when the test was started rather than from page load (parity with
        /// the WebForms btnStart click). Sourcing both endpoints from the server clock avoids skew.
        /// </summary>
        /// <returns>The current server date and time.</returns>
        [BlockAction]
        public BlockActionResult Start()
        {
            return ActionOk( RockDateTime.Now );
        }

        /// <summary>
        /// Returns a freshly randomized set of unanswered questions. Used on a retake so the question
        /// order is re-shuffled each time the test is taken (parity with the WebForms ShowQuestions call).
        /// </summary>
        /// <returns>A new list of unanswered assessment responses.</returns>
        [BlockAction]
        public BlockActionResult GetQuestions()
        {
            return ActionOk( GetAssessmentResponses() );
        }

        /// <summary>
        /// Saves the assessment responses and returns the box with the updated results.
        /// </summary>
        /// <param name="box">The box that contains the responses required to save.</param>
        /// <returns>The box with updated results; or an error message if unsuccessful.</returns>
        [BlockAction]
        public BlockActionResult Save( GiftsAssessmentInitializationBox box )
        {
            var hasQueryStringForPersonKey = PersonKey.IsNotNullOrWhiteSpace();
            var targetPerson = TargetPerson;

            if ( targetPerson == null )
            {
                // Ensure we're able to get the target person; otherwise return an error message.
                return ActionBadRequest( hasQueryStringForPersonKey
                    ? "There is an issue locating the person associated with the request."
                    : "You must be signed in to take the assessment." );
            }

            // Guard against a manipulated front-end saving an assessment that requires a request without one.
            var assessmentType = new AssessmentTypeService( RockContext ).Get( Rock.SystemGuid.AssessmentType.GIFTS.AsGuid() );
            var hasAssessment = new AssessmentService( RockContext )
                .Queryable()
                .Any( a =>
                    a.PersonAlias != null
                    && a.PersonAlias.PersonId == targetPerson.Id
                    && a.AssessmentTypeId == assessmentType.Id );

            var effectiveAssessmentId = box.AssessmentId ?? 0;
            if ( effectiveAssessmentId == 0 && assessmentType.RequiresRequest && !hasAssessment )
            {
                return ActionBadRequest( "Sorry, this test requires a request from someone before it can be taken." );
            }

            SaveAssessment( box, assessmentType );

            return ActionOk( box );
        }

        #endregion Block Actions
    }
}
