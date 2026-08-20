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

using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Model;
using Rock.ViewModels.Blocks.Crm.ConflictProfile;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Calculates a person's Conflict Profile assessment scores based on a series of question answers.
    /// </summary>
    [DisplayName( "Conflict Profile" )]
    [Category( "CRM" )]
    [Description( "Allows you to take a conflict profile test and saves your conflict profile score." )]

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
        DefaultValue = "Conflict Profile",
        Order = 1 )]

    [TextField(
        "Set Page Icon",
        Key = AttributeKey.SetPageIcon,
        Description = "The css class name to use for the heading icon.",
        IsRequired = false,
        DefaultValue = "ti ti-heart-handshake",
        Order = 2 )]

    [IntegerField(
        "Number of Questions",
        Key = AttributeKey.NumberOfQuestions,
        Description = "The number of questions to show per page while taking the test",
        IsRequired = true,
        DefaultIntegerValue = 7,
        Order = 3 )]

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Primary )]
    [Rock.SystemGuid.EntityTypeGuid( "B01DBD7C-C4AA-4A89-8C63-4578EC8FF9F3" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "A8D42904-44A6-4A36-A4CF-840529E53772" )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.CONFLICT_PROFILE )]
    public class ConflictProfile : RockBlockType
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
        /// Gets the number of questions to show per page (defaults to 7 if unable to parse).
        /// </summary>
        private int PageSize => GetAttributeValue( AttributeKey.NumberOfQuestions ).ToIntSafe( 7 );

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
                    /*
                        6/18/26 - MSE

                        A malformed or corrupt person key can throw while being decoded (e.g. a decryption
                        failure in GetByUrlEncodedKey). Swallow it and return null so the caller surfaces the
                        friendly "issue locating the person" message rather than an unhandled block error.

                        Reason: Preserves the graceful handling of a bad person key.
                    */
                    try
                    {
                        var personService = new PersonService( RockContext );
                        return personService.GetByPersonActionIdentifier( personKey, "Assessment" ) ?? personService.GetByUrlEncodedKey( personKey );
                    }
                    catch
                    {
                        return null;
                    }
                }

                return GetCurrentPerson();
            }
        }

        private const string InstructionsDefaultValue = @"
<p>
    {{ Person.NickName }}, while we can’t avoid occasional conflict in life, we can approach its resolution in different ways.
    This assessment evaluates your natural approach and compares it to the five common conflict profile modes.
</p>
<p>
    For best results with this assessment, picture a setting in which you might encounter conflict and keep that same setting
    in mind as you answer all the questions. Your responses may be different in different environments.
</p>
<p>
    Don’t spend too much time thinking about your answer. Usually, your first responses is your most
    natural. Since there are no right or wrong answers, just go with your instinct.
</p>";

        /*
            6/18/26 - MSE

            These constants hold the static display copy for the five conflict modes and three conflict themes shown
            on the results panel. BuildResults pairs each mode/theme's copy and chart color with its scored value to
            produce the result bags the client renders (a bar chart for the modes and a pie chart for the themes).

            Reason: The results panel is rendered natively by the client, so this descriptive copy is owned by the block rather than an editable template.
        */

        private const string WinningModeDescription = "Winning means you prefer competing over cooperating. You believe you have the right answer and you desire to prove you are right, whatever it takes. This may include standing up for your own rights, beliefs or position.";

        private const string ResolvingModeDescription = "Resolving means you attempt to work with the other person in depth to find the best solution, regardless of who appears to get the most immediate benefit. This involves digging beneath the presenting issue to find a solution that offers benefit to both parties and can take more time than other approaches.";

        private const string CompromisingModeDescription = "Compromising means you find a middle ground in the conflict. This often involves meeting in the middle or finding some mutually agreeable point between both positions. This is useful for quick solutions.";

        private const string AvoidingModeDescription = "Avoiding means not pursuing your own rights or those of the other person. You typically do not address the conflict at all, if possible. This may be diplomatically sidestepping an issue or staying away from a threatening situation.";

        private const string YieldingModeDescription = "Yielding means neglecting your own interests while giving in to those of the other person. This is self-sacrificing and maybe charitable; serving or choosing to obey another when you prefer not to.";

        private const string SolvingThemeDescription = "Solving describes those who seek to use both Resolving and Compromising modes for solving conflict. By combining these two modes, they seek to solve problems as a team. Their leadership styles are highly cooperative and empowering for the benefit of the entire group.";

        private const string AccommodatingThemeDescription = "Accommodating combines Avoiding and Yielding modes for solving conflict. They are most effective in roles where allowing others to have their way is better for the team, such as support roles or roles where an emphasis on the contribution of others is significant.";

        private const string WinningThemeDescription = "Winning is not a combination of modes, but a theme that is based entirely on the Winning model alone for solving conflict. This theme is important for times when quick decisions need to be made and is helpful for roles such as sole-proprietor.";

        // The themes pie chart's slices share one slot and are distinguishable only by color, so each theme
        // gets its own categorical color, assigned in sequence starting from --color-categorical-1. Each
        // mode bar is colored to match the theme it belongs to (Winning modes/themes coincide; Resolving
        // and Compromising combine into Solving; Avoiding and Yielding combine into Accommodating), so the
        // bar chart and pie chart read as one related result instead of the mode colors looking arbitrary.
        private const string SolvingThemeColor = "--color-categorical-1";
        private const string AccommodatingThemeColor = "--color-categorical-2";
        private const string WinningThemeColor = "--color-categorical-3";

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return GetBox();
        }

        /// <summary>
        /// Gets the Conflict Profile initialization box with populated values or with an error message.
        /// </summary>
        /// <returns>The populated box.</returns>
        private ConflictProfileInitializationBox GetBox()
        {
            /*
                This block will either show the assessment results of the most recent assessment test or give the assessment test.
                The following use cases are considered:
                1. If the assessment ID "0" was provided then create a new test for the current user. This covers user directed retakes.
                2. If the assessment ID was provided and is not "0" then load that assessment for the target person; show its results if completed
                   or its questions if pending (and the current person is the one assigned).
                3. If the assessment ID was not provided but the PersonKey was, then load the most recent assessment for that person, falling back to
                   the most recently completed one when a newer request is still pending and the viewer is not the assigned person.
                4. If neither an assessment ID nor a PersonKey were provided or are not valid then show an error message.
            */
            // Resolve the assessment key (Id, IdKey, or Guid) once so it isn't looked up multiple times.
            var assessmentId = AssessmentId;

            var box = new ConflictProfileInitializationBox
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

            var assessmentType = new AssessmentTypeService( RockContext ).Get( Rock.SystemGuid.AssessmentType.CONFLICT.AsGuid() );
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
                var savedScores = ConflictProfileService.LoadSavedAssessmentResults( targetPerson );
                SetResult( savedScores, assessment, assessmentType, targetPerson, box );
                return box;
            }

            if ( assessment.Status == AssessmentRequestStatus.Pending )
            {
                if ( !box.IsAssessmentForCurrentPerson )
                {
                    // If the assessment is pending and the current person is not the one assigned then show the previously completed results.
                    if ( previouslyCompletedAssessment != null )
                    {
                        var savedScores = ConflictProfileService.LoadSavedAssessmentResults( targetPerson );
                        box.InfoMessage = "A more recent assessment request has been made but has not been taken. Displaying the most recently completed test.";
                        SetResult( savedScores, previouslyCompletedAssessment, assessmentType, targetPerson, box );
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
            return ConflictProfileService.GetQuestions()
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
        /// <param name="assessmentType">The Conflict Profile assessment type the responses are saved against.</param>
        private void SaveAssessment( ConflictProfileInitializationBox box, AssessmentType assessmentType )
        {
            var targetPerson = TargetPerson;

            var responseData = box.Responses
                .Where( r => r.Code.IsNotNullOrWhiteSpace() && r.Response.HasValue )
                .GroupBy( r => r.Code )
                .ToDictionary( g => g.Key, g => g.Last().Response.Value );

            // Score the responses and save the mode/theme scores to the person's attributes.
            var result = ConflictProfileService.GetResult( responseData );
            ConflictProfileService.SaveAssessmentResults( targetPerson, result );

            var assessmentService = new AssessmentService( RockContext );
            Assessment assessment = null;

            // A box assessment id greater than zero updates the existing assessment; otherwise a new one is created (e.g. a retake).
            // The lookup is scoped to the target person and the Conflict type so a forged or foreign id cannot overwrite another person's assessment.
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
            assessment.AssessmentResultData = new
            {
                Result = result.AssessmentData,
                TimeToTake = RockDateTime.Now.Subtract( startTime ).TotalSeconds
            }.ToJson();

            RockContext.SaveChanges();

            SetResult( result, assessment, assessmentType, targetPerson, box );
        }

        /// <summary>
        /// Populates the result properties on the box from the scored results.
        /// </summary>
        /// <param name="result">The assessment results used to populate the box.</param>
        /// <param name="assessment">The assessment whose completion date determines retake eligibility.</param>
        /// <param name="assessmentType">The assessment type whose configuration determines retake eligibility.</param>
        /// <param name="targetPerson">The person the results are for (used for the personalized greeting).</param>
        /// <param name="box">The box whose properties should be populated.</param>
        private void SetResult( ConflictProfileService.AssessmentResults result, Assessment assessment, AssessmentType assessmentType, Person targetPerson, ConflictProfileInitializationBox box )
        {
            var hasQueryStringForPersonKey = PersonKey.IsNotNullOrWhiteSpace();

            // A request-required assessment cannot be self-retaken; a fresh request is needed.
            box.CanRetakeTest =
                !hasQueryStringForPersonKey
                && !assessmentType.RequiresRequest
                && assessment.CompletedDateTime.HasValue
                && assessment.CompletedDateTime.Value.AddDays( assessmentType.MinimumDaysToRetake ) <= RockDateTime.Now;

            box.Results = BuildResults( result, targetPerson );
        }

        /// <summary>
        /// Builds the scored result bag (greeting, five modes, three themes) from the scored results.
        /// </summary>
        /// <param name="result">The scored assessment results.</param>
        /// <param name="targetPerson">The person the results are for (used for the personalized greeting).</param>
        /// <returns>The populated result bag.</returns>
        private static ConflictProfileResultBag BuildResults( ConflictProfileService.AssessmentResults result, Person targetPerson )
        {
            return new ConflictProfileResultBag
            {
                Greeting = $"{targetPerson.NickName}, here are your conflict engagement results. You will rank high, medium or low in each of the following five modes.",
                Modes = new List<ConflictProfileScoreBag>
                {
                    BuildScore( "Winning", WinningModeDescription, WinningThemeColor, result.ModeWinningScore ),
                    BuildScore( "Resolving", ResolvingModeDescription, SolvingThemeColor, result.ModeResolvingScore ),
                    BuildScore( "Compromising", CompromisingModeDescription, SolvingThemeColor, result.ModeCompromisingScore ),
                    BuildScore( "Avoiding", AvoidingModeDescription, AccommodatingThemeColor, result.ModeAvoidingScore ),
                    BuildScore( "Yielding", YieldingModeDescription, AccommodatingThemeColor, result.ModeYieldingScore )
                },
                Themes = new List<ConflictProfileScoreBag>
                {
                    BuildScore( "Solving", SolvingThemeDescription, SolvingThemeColor, result.EngagementSolvingScore ),
                    BuildScore( "Accommodating", AccommodatingThemeDescription, AccommodatingThemeColor, result.EngagementAccommodatingScore ),
                    BuildScore( "Winning", WinningThemeDescription, WinningThemeColor, result.EngagementWinningScore )
                }
            };
        }

        /// <summary>
        /// Builds a single mode or theme result.
        /// </summary>
        /// <param name="name">The mode or theme name.</param>
        /// <param name="description">The descriptive copy for the mode or theme.</param>
        /// <param name="chartColor">The CSS custom property (e.g. "--color-metric-primary") used for this item in the results chart.</param>
        /// <param name="score">The score for the mode or theme.</param>
        /// <returns>The populated score bag.</returns>
        private static ConflictProfileScoreBag BuildScore( string name, string description, string chartColor, decimal score )
        {
            return new ConflictProfileScoreBag
            {
                Name = name,
                Description = description,
                ChartColor = chartColor,
                Percentage = ( double ) score
            };
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Returns the server time at the moment the individual begins the test so the persisted
        /// TimeToTake measures from when the test was started rather than from page load. Sourcing both
        /// endpoints from the server clock avoids skew.
        /// </summary>
        /// <returns>The current server date and time.</returns>
        [BlockAction]
        public BlockActionResult Start()
        {
            return ActionOk( RockDateTime.Now );
        }

        /// <summary>
        /// Returns a freshly randomized set of unanswered questions. Used on a retake so the question
        /// order is re-shuffled each time the test is taken.
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
        public BlockActionResult Save( ConflictProfileInitializationBox box )
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
            var assessmentType = new AssessmentTypeService( RockContext ).Get( Rock.SystemGuid.AssessmentType.CONFLICT.AsGuid() );
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
