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
using Rock.ViewModels.Blocks.Crm.EQInventory;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Calculates a person's EQ Inventory assessment scores based on a series of question answers.
    /// </summary>
    [DisplayName( "EQ Inventory Assessment" )]
    [Category( "CRM" )]
    [Description( "Allows you to take an EQ Inventory test and saves your EQ Inventory scores." )]

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
        DefaultValue = "EQ Inventory Assessment",
        Order = 1 )]

    [TextField(
        "Set Page Icon",
        Key = AttributeKey.SetPageIcon,
        Description = "The css class name to use for the heading icon.",
        IsRequired = false,
        DefaultValue = "ti ti-masks-theater",
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
    [Rock.SystemGuid.EntityTypeGuid( "60F1C9A9-BDE1-48DF-9AFE-760658B3E7E1" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "DC202F2E-8D6C-47FF-9067-DC29ED142005" )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.EQ_INVENTORY )]
    public class EQInventory : RockBlockType
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
                    var personService = new PersonService( RockContext );
                    return personService.GetByPersonActionIdentifier( personKey, "Assessment" ) ?? personService.GetByUrlEncodedKey( personKey );
                }

                return GetCurrentPerson();
            }
        }

        private const string InstructionsDefaultValue = @"
<h2>Welcome to the EQ Inventory Assessment</h2>
<p>
    {{ Person.NickName }}, we encounter emotions every day: our own and those of the people around us.
    This assessment measures your developed skills in two areas: understanding your emotions and
    understanding the emotions of others.
</p>
<p>
    For best results with this assessment, picture a setting such as the workplace, at home or with
    friends, and keep that same setting in mind as you answer all the questions. Your responses may
    be different in different circumstances.
</p>
<p>
    Don’t spend too much time thinking about your answer. Usually, your first responses is your most
    natural. Since there are no right or wrong answers, just go with your instinct.
</p>";

        /*
            6/18/26 - MSE

            These constants hold the static display copy for the six EQ Inventory dimensions shown on the
            results panel: the section heading, the explanatory HTML body, and the interpretation sentence
            (which carries a "{0}" placeholder for the individual's percentile). BuildResults pairs each
            dimension's copy with its scored percentile to produce the result bags the client renders.

            Reason: The results panel is rendered natively by the client, so this descriptive copy is owned by the block rather than an editable template.
        */

        private const string SelfAwarenessDescription = @"
Self-Awareness is being aware of what emotions you are experiencing and why you
are experiencing them. This skill is demonstrated in real time. In other words,
when you are in the midst of a discussion or even a disagreement with someone else,
ask yourself these questions:
<ul>
    <li>Are you aware of what emotions you are experiencing?</li>
    <li>Are you aware of why you are experiencing these emotions?</li>
</ul>";

        private const string SelfAwarenessInterpretationFormat = "Your responses to the items on the Self Awareness scale indicate the score for the ability to be aware of your own emotions is equal to or better than {0}% of those who completed this instrument.";

        private const string SelfRegulatingDescription = @"
Self-Regulating is appropriately expressing your emotions in the context of the relationships
around you. This doesn’t indicate suppressing emotions; rather the ability to express your
emotions appropriately. Healthy human beings experience a full range of emotions and these are
important for family, friends, and co-workers to understand. Self-Regulating is learning to
tell others what you are feeling in the moment.";

        private const string SelfRegulatingInterpretationFormat = "Your responses to the items on the Self Regulation scale indicate the score for the ability to appropriately express your own emotions is equal to or better than {0}% of those who completed this instrument.";

        private const string OthersAwarenessDescription = @"
Others-Awareness is being aware of what emotions others are experiencing around you and
why they are experiencing these emotions. As with understanding your own emotions, this
skill is knowing in real time what another person is experiencing. This skill involves
reading cues to their emotional state through their eyes, facial expressions, body
posture, the tone of voice and many other ways.";

        private const string OthersAwarenessInterpretationFormat = "Your responses to the items on the Others-Awareness scale indicate the score for the ability to be aware of others emotions is equal to or better than {0}% of those who completed this instrument.";

        private const string OthersRegulatingDescription = @"
Others-Regulating is helping those around you express their emotions appropriately
in the context of your relationship with them. This skill centers on helping others
know what emotions they are experiencing and then asking questions or giving them
permission to freely and appropriately express their emotions in the context of
your relationship.";

        private const string OthersRegulatingInterpretationFormat = "Your responses to the items on the Others-Regulation scale indicate the score for the ability to enable others to appropriately express their emotions in the context of your relationship is equal to or better than {0}% of those who completed this instrument.";

        private const string ProblemSolvingDescription = @"
EQ in Problem Solving identifies how proficient you are at using emotions to solve
problems. This skill requires first being aware of what emotions are involved in
the problem and what is the source of those emotions. It also includes helping
others (and yourself) express those emotions appropriate in the context of
the situation.";

        private const string ProblemSolvingInterpretationFormat = "Your responses to the items on the EQ in Problem Solving scale indicate the score for the ability to use emotions in resolving problems is equal to or better than {0}% of those who completed this instrument.";

        private const string UnderStressDescription = @"
EQ Under Stress identifies how capable you are of keeping high EQ under high-stress
moments; which is particularly challenging. This skill requires highly developed
Self- and Others-Awareness to understand the impact of the current stress. It also
involves being able to articulate the appropriate emotions under pressure which
may be different from articulating them when not under stress.";

        private const string UnderStressInterpretationFormat = "Your responses to the items on the EQ in Under Stress scale indicate the score for the ability to maintain EQ under significant stress is equal to or better than {0}% of those who completed this instrument.";

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return GetEqBox();
        }

        /// <summary>
        /// Gets the EQ Inventory initialization box with populated values or with an error message.
        /// </summary>
        /// <returns>The populated box.</returns>
        private EQInventoryInitializationBox GetEqBox()
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

            var box = new EQInventoryInitializationBox
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

            var assessmentType = new AssessmentTypeService( RockContext ).Get( Rock.SystemGuid.AssessmentType.EQ.AsGuid() );
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
                var savedScores = EQInventoryService.LoadSavedAssessmentResults( targetPerson );
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
                        var savedScores = EQInventoryService.LoadSavedAssessmentResults( targetPerson );
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
        /// Gets the assessment questions in a random order, each with its answer options.
        /// </summary>
        /// <returns>A list of unanswered assessment responses.</returns>
        private List<AssessmentResponseBag> GetAssessmentResponses()
        {
            // GetQuestions() already returns the questions in a randomized order.
            return EQInventoryService.GetQuestions()
                .Select( q => new AssessmentResponseBag
                {
                    Code = q.Key,
                    Question = q.Value,
                    Options = GetOptions( q.Key )
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the answer options for a question, reversing the recorded score for negatively-keyed questions.
        /// </summary>
        /// <param name="code">The question code. A trailing "N" indicates a negatively-keyed question.</param>
        /// <returns>The five answer options in a consistent display order (Never through Always).</returns>
        private static List<ListItemBag> GetOptions( string code )
        {
            // Negatively-keyed questions (code ends with "N") reverse the score so the displayed labels stay in the same order.
            var isNegative = code.EndsWith( "N" );

            return new List<ListItemBag>
            {
                new ListItemBag { Text = "Never", Value = ( isNegative ? 5 : 1 ).ToString() },
                new ListItemBag { Text = "Rarely", Value = ( isNegative ? 4 : 2 ).ToString() },
                new ListItemBag { Text = "Sometimes", Value = "3" },
                new ListItemBag { Text = "Usually", Value = ( isNegative ? 2 : 4 ).ToString() },
                new ListItemBag { Text = "Always", Value = ( isNegative ? 1 : 5 ).ToString() }
            };
        }

        /// <summary>
        /// Scores the responses, persists the assessment, and populates the result properties on the box.
        /// </summary>
        /// <param name="box">The box whose responses are saved and whose result properties are populated.</param>
        /// <param name="assessmentType">The EQ assessment type the responses are saved against.</param>
        private void SaveAssessment( EQInventoryInitializationBox box, AssessmentType assessmentType )
        {
            var targetPerson = TargetPerson;

            var responseData = box.Responses
                .Where( r => r.Code.IsNotNullOrWhiteSpace() && r.Response.IsNotNullOrWhiteSpace() )
                .GroupBy( r => r.Code )
                .ToDictionary( g => g.Key, g => g.Last().Response.AsInteger() );

            // Score the responses and save the dimension percentiles to the person's attributes.
            var result = EQInventoryService.GetResult( responseData );
            EQInventoryService.SaveAssessmentResults( targetPerson, result );

            var assessmentService = new AssessmentService( RockContext );
            Assessment assessment = null;

            // A box assessment id greater than zero updates the existing assessment; otherwise a new one is created (e.g. a retake).
            // The lookup is scoped to the target person and the EQ type so a forged or foreign id cannot overwrite another person's assessment.
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
        private void SetResult( EQInventoryService.AssessmentResults result, Assessment assessment, AssessmentType assessmentType, Person targetPerson, EQInventoryInitializationBox box )
        {
            var hasQueryStringForPersonKey = PersonKey.IsNotNullOrWhiteSpace();

            // A request-required assessment cannot be self-retaken; a fresh request is needed.
            box.CanRetakeTest =
                !hasQueryStringForPersonKey
                && !assessmentType.RequiresRequest
                && assessment.CompletedDateTime.HasValue
                && assessment.CompletedDateTime.Value.AddDays( assessmentType.MinimumDaysToRetake ) <= RockDateTime.Now;

            box.ResultsGreeting = $"{targetPerson.NickName}, here are your emotional intelligence results. This is a snapshot in time and may change through intentional effort and practice. You will rank high, medium or low in each of the following six areas.";
            box.Results = BuildResults( result );
        }

        /// <summary>
        /// Builds the ordered list of dimension results from the scored percentiles.
        /// </summary>
        /// <param name="result">The scored assessment results.</param>
        /// <returns>The six dimension results in display order.</returns>
        private static List<EQInventoryDimensionScoreBag> BuildResults( EQInventoryService.AssessmentResults result )
        {
            return new List<EQInventoryDimensionScoreBag>
            {
                BuildDimension( "Self Awareness", SelfAwarenessDescription, SelfAwarenessInterpretationFormat, result.SelfAwareConstruct ),
                BuildDimension( "Self-Regulating", SelfRegulatingDescription, SelfRegulatingInterpretationFormat, result.SelfRegulatingConstruct ),
                BuildDimension( "Others-Awareness", OthersAwarenessDescription, OthersAwarenessInterpretationFormat, result.OtherAwarenessContruct ),
                BuildDimension( "Others-Regulating", OthersRegulatingDescription, OthersRegulatingInterpretationFormat, result.OthersRegulatingConstruct ),
                BuildDimension( "EQ in Problem Solving", ProblemSolvingDescription, ProblemSolvingInterpretationFormat, result.EQ_ProblemSolvingScale ),
                BuildDimension( "EQ Under Stress", UnderStressDescription, UnderStressInterpretationFormat, result.EQ_UnderStressScale )
            };
        }

        /// <summary>
        /// Builds a single dimension result, formatting the interpretation sentence with the percentile score.
        /// </summary>
        /// <param name="name">The dimension name.</param>
        /// <param name="description">The descriptive HTML for the dimension.</param>
        /// <param name="interpretationFormat">The interpretation sentence with a "{0}" placeholder for the score.</param>
        /// <param name="score">The percentile score (0-100).</param>
        /// <returns>The populated dimension result.</returns>
        private static EQInventoryDimensionScoreBag BuildDimension( string name, string description, string interpretationFormat, decimal score )
        {
            return new EQInventoryDimensionScoreBag
            {
                Name = name,
                Description = description,
                Interpretation = string.Format( interpretationFormat, score ),
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
        public BlockActionResult Save( EQInventoryInitializationBox box )
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
            var assessmentType = new AssessmentTypeService( RockContext ).Get( Rock.SystemGuid.AssessmentType.EQ.AsGuid() );
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
