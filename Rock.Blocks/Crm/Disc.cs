using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.Attribute;
using Rock.Model;
using Rock.SystemGuid;
using Rock.ViewModels.Blocks.Crm.Disc;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;


namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Calculates a targetPerson's DISC score based on a series of question answers.
    /// This is used with permission from Greg Wiens: http://www.gregwiens.com/scid/
    /// </summary>
    [DisplayName( "Disc" )]
    [Category( "CRM" )]
    [Description( "Allows you to take a DISC test and saves your DISC score." )]

    #region Block Attributes

    [CodeEditorField(
        "Instructions",
        Key = AttributeKey.Instructions,
        Description = "The text (HTML) to display at the top of the instructions section.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = InstructionsDefaultValue,
        Order = 0 )]

    [TextField(
        "Set Page Title",
        Key = AttributeKey.SetPageTitle,
        Description = "The text to display as the heading.",
        IsRequired = false,
        DefaultValue = "DISC Assessment",
        Order = 1 )]

    [TextField(
        "Set Page Icon",
        Key = AttributeKey.SetPageIcon,
        Description = "The css class name to use for the heading icon.",
        IsRequired = false,
        DefaultValue = "fa fa-chart-bar",
        Order = 2 )]

    [IntegerField(
        "Number of Questions",
        Key = AttributeKey.NumberOfQuestions,
        Description = "The number of questions to show per page while taking the test",
        IsRequired = true,
        DefaultIntegerValue = 5,
        Order = 3 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "5D8108B4-4877-4214-819F-78CA058A82E0" )]
    [Rock.SystemGuid.BlockTypeGuid( "F9261A63-92C8-4029-9CCA-2F9EDCCF6F7E" )]
    public class Disc : RockBlockType
    {

        #region Attribute Default Values
        private const string InstructionsDefaultValue = @"
<h2>Welcome!</h2>
<p>
    {{ Person.NickName }}, our behaviors are influenced by our natural personality wiring. This assessment
    evaluates your essential approach to the world around you and how that drives your behavior.
</p>
<p>
    For best results with this assessment, picture a setting such as the workplace, at home or with friends,
    and keep that same setting in mind as you answer all the questions. Your responses may be different in
    different circumstances.
</p>
<p>
    Don’t spend too much time thinking about your answer. Usually, your first responses is your most natural.
    Since there are no right or wrong answers, just go with your instinct.
</p>";

        #endregion Attribute Default Values

        #region Attribute Keys

        private static class AttributeKey
        {
            // Block Attributes
            public const string Instructions = "Instructions";
            public const string SetPageTitle = "SetPageTitle";
            public const string SetPageIcon = "SetPageIcon";
            public const string NumberOfQuestions = "NumberofQuestions";

            // Other Attributes
            public const string Strengths = "Strengths";
            public const string Challenges = "Challenges";
            public const string UnderPressure = "UnderPressure";
            public const string Motivation = "Motivation";
            public const string TeamContribution = "TeamContribution";
            public const string LeadershipStyle = "LeadershipStyle";
            public const string FollowerStyle = "FollowerStyle";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        /// <summary>
        /// A defined list of page parameter keys used by this block.
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The assessment identifier
            /// </summary>
            public const string AssessmentId = "AssessmentId";

            /// <summary>
            /// The URL encoded key for a targetPerson
            /// </summary>
            public const string Person = "Person";
        }

        #endregion Page Parameter Keys

        #region Attributes and Parameters

        /// <summary>
        /// The AssessmentId from the page parameter.
        /// </summary>
        private int? AssessmentId => PageParameter( PageParameterKey.AssessmentId ).AsIntegerOrNull();

        /// <summary>
        /// The PersonKey from the page parameter.
        /// </summary>
        private string PersonKey => PageParameter( PageParameterKey.Person );

        /// <summary>
        /// The PanelTitle block setting.
        /// </summary>
        private string PanelTitle => GetAttributeValue( AttributeKey.SetPageTitle )?.ToStringSafe();

        /// <summary>
        /// The PanelIcon block setting.
        /// </summary>
        private string PanelIcon => GetAttributeValue( AttributeKey.SetPageIcon ).ToStringSafe();

        /// <summary>
        /// Gets the page size configured (or defaults to 5 if unable to).
        /// </summary>
        private int PageSize => GetAttributeValue( AttributeKey.NumberOfQuestions ).ToIntSafe( 5 );

        /// <summary>
        /// Gets the TargetPerson from the Query String, "Person" or CurrentPerson if not provided.
        /// </summary>
        private Rock.Model.Person TargetPerson
        {
            get
            {
                string personKey = PersonKey;

                // set the target targetPerson according to the parameter or use Current user if not provided.
                if ( personKey.IsNotNullOrWhiteSpace() )
                {
                    var personService = new PersonService( RockContext );
                    return personService.GetByPersonActionIdentifier( personKey, "Assessment" ) ?? personService.GetByUrlEncodedKey( personKey );
                }
                else
                {
                    return GetCurrentPerson();
                }
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return GetDiscBox();
        }

        #endregion Methods

        #region Private Methods

        /// <summary>
        /// Gets the Disc initialization box with populated values or with an error message.
        /// </summary>
        /// <returns>The populated Disc box.</returns>
        private DiscInitializationBox GetDiscBox()
        {
            //var testing = new PersonService(RockContext).Get(2).GetPersonActionIdentifier( "Assessment" );
            /*
                2020-01-09 - ETD
                This block will either show the assessment results of the most recent assessment test or give the assessment test.
                The following use cases are considered:
                1. If the assessment ID "0" was provided then create a new test for the current user. This covers user directed retakes.
                2. If the assessment ID was provided and is not "0"
                    Note: The assessment results are stored on the targetPerson's attributes and are overwritten if the assessment is retaken. So past Assessments will not be loaded by this block.
                    The test data is saved in the assessment table but would need to be recomputed, which may be a future feature.
                    a. The assessment ID is ignored and the current targetPerson is used.
                    b. If the assessment exists for the current targetPerson and is completed then show the results
                    c. If the assessment exists for the current targetPerson and is pending then show the questions.
                    d. If the assessment does not exist for the current targetPerson then nothing loads.
                3. If the assessment ID was not provided and the PersonKey was provided
                    a. If there is only one test of the type
                        1. If the assessment is completed show the results
                        2. If the assessment is pending and the current targetPerson is the one assigned the test then show the questions.
                        3. If the assessment is pending and the current targetPerson is not the one assigned then show a message that the test has not been completed.
                    b. If more than one of type
                        1. If the latest requested assessment is completed show the results.
                        2. If the latest requested assessment is pending and the current targetPerson is the one assigned then show the questions.
                        3. If the latest requested assessment is pending and the current targetPerson is not the one assigned the show the results of the last completed test.
                        4. If the latest requested assessment is pending and the current targetPerson is not the one assigned and there are no previous completed assessments then show a message that the test has not been completed.
                4. If an assessment ID or PersonKey were not provided or are not valid then show an error message
             */
            var box = new DiscInitializationBox
            {
                PanelTitle = PanelTitle,
                PanelIcon = PanelIcon,
                PageSize = PageSize,
                StartDateTime = RockDateTime.Now
            };

            var targetPerson = TargetPerson;
            var currentPerson = GetCurrentPerson();
            var hasQueryStringForPersonKey = PersonKey.IsNotNullOrWhiteSpace();

            // Ensure we're able to get the target targetPerson; otherwise return an error message.
            if ( targetPerson == null )
            {
                box.ErrorMessage = hasQueryStringForPersonKey ? "There is an issue locating the targetPerson associated with the request." : "You must be signed in to take the assessment.";
                return box;
            }

            box.TargetPersonBag = targetPerson.ToListItemBag();
            box.IsAsessmentForCurrentPerson = targetPerson.Id == currentPerson?.Id;
            box.Responses = GetAssessmentResponses();

            var assessmentType = new AssessmentTypeService( RockContext ).Get( Rock.SystemGuid.AssessmentType.DISC.AsGuid() );
            Assessment assessment = null;
            Assessment previouslyCompletedAssessment = null;
            var assessmentId = AssessmentId;
            box.Instructions = GetDiscInstructions( targetPerson );

            // A "0" value indicates that the block should create a new assessment instead of looking for an existing one, so keep assessment null. e.g. a user directed re-take
            if ( assessmentId != 0 )
            {
                var assessments = new AssessmentService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( a =>
                    a.PersonAlias != null
                    && a.PersonAlias.PersonId == targetPerson.Id
                    && a.AssessmentTypeId == assessmentType.Id )
                .ToList()
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
                        // If there are any results then pick the first one. If the assesement ID was specified then the query will only return one result
                        assessment = assessments[0];
                    }

                    if ( assessments.Count > 1 )
                    {
                        // If there are more than one result then we need to pick the right one (see developer note)
                        // If the most recent assessment is "Complete" then it is already set as the assessment and we can move on. Otherwise check if there are previoulsy completed assessments.
                        if ( assessment.Status == AssessmentRequestStatus.Pending )
                        {
                            // If the most recent assessment is pending then check for a prior completed one
                            previouslyCompletedAssessment = assessments.FirstOrDefault( a => a.Status == AssessmentRequestStatus.Complete );
                        }
                    }
                }
            }

            if ( assessment == null )
            {
                // If assessment is null and assessmentId = 0 this is user directed. If the type doesn't require a request then return the box.
                // Otherwise return the box with an error message.
                if ( assessmentId == 0 && !assessmentType.RequiresRequest )
                {
                    return box;
                }
                else
                {
                    box.ErrorMessage = "Sorry, this test requires a request from someone before it can be taken.";
                    return box;
                }
            }

            // If assessment is completed show the results
            if ( assessment.Status == AssessmentRequestStatus.Complete )
            {
                DiscService.AssessmentResults savedScores = DiscService.LoadSavedAssessmentResults( targetPerson );
                SetResult( savedScores, assessment, box );

                return box;
            }

            if ( assessment.Status == AssessmentRequestStatus.Pending )
            {
                if ( !box.IsAsessmentForCurrentPerson )
                {
                    // If assessment is pending and the current targetPerson is not the one assigned the show previouslyCompletedAssessment results
                    if ( previouslyCompletedAssessment != null )
                    {
                        DiscService.AssessmentResults savedScores = DiscService.LoadSavedAssessmentResults( targetPerson );
                        box.InfoMessage = "A more recent assessment request has been made but has not been taken. Displaying the most recently completed test.";
                        SetResult( savedScores, previouslyCompletedAssessment, box );
                        return box;
                    }

                    // If assessment is pending and the current targetPerson is not the one assigned and previouslyCompletedAssessment is null show a message that the test has not been completed.
                    box.ErrorMessage = string.Format( "{0} has not yet taken the {1} Assessment.", targetPerson.FullName, assessmentType.Title );
                }
                else
                {
                    // If assessment is pending and the current targetPerson is the one assigned then show the questions
                    return box;
                }

                return box;
            }

            // This should never happen, if the block gets to this point then something is not right
            box.ErrorMessage = "Unable to load assessment";

            return box;
        }

        /// <summary>
        /// Resolves the merge fields in the Instructions Lava template.
        /// </summary>
        /// <param name="targetPerson">The target person for whom the assessment is for (used in the Lava template).</param>
        /// <returns>A Lava resolved string.</returns>
        private string GetDiscInstructions( Model.Person targetPerson )
        {
            // Resolve the text field merge fields
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, targetPerson );
            if ( targetPerson != null )
            {
                mergeFields.Add( "Person", targetPerson );
            }

            return GetAttributeValue( AttributeKey.Instructions ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Gets the assessment responses for the DISC assessment with questions in a random order.
        /// </summary>
        /// <returns>A List of AssessmentResponseBag from the ResponseItems grouped by QuestioNumber.</returns>
        private List<AssessmentResponseBag> GetAssessmentResponses()
        {
            Random r = new Random();
            return DiscService.GetResponses()
                .GroupBy( a => a.QuestionNumber )
                .Select( a => new AssessmentResponseBag()
                {
                    QuestionNumber = a.Key,
                    Questions = a.OrderBy( x => r.Next( 0, 4 ) ).ToDictionary( c => c.MostScore, b => b.ResponseText )
                } ).ToList();
        }

        /// <summary>
        /// Saves the assessment scores and populates the results poperties for the box.
        /// </summary>
        /// <param name="box">The box whose responses are used for saving the assessment and whose result properties will be set.</param>
        private void SaveAssessment( DiscInitializationBox box )
        {
            try
            {
                var targetPerson = TargetPerson;

                var moreD = box.Responses.Count( a => a.MostScore == "D" );
                var moreI = box.Responses.Count( a => a.MostScore == "I" );
                var moreS = box.Responses.Count( a => a.MostScore == "S" );
                var moreC = box.Responses.Count( a => a.MostScore == "C" );
                var lessD = box.Responses.Count( a => a.LeastScore == "D" );
                var lessI = box.Responses.Count( a => a.LeastScore == "I" );
                var lessS = box.Responses.Count( a => a.LeastScore == "S" );
                var lessC = box.Responses.Count( a => a.LeastScore == "C" );

                // Score the responses and return the results
                DiscService.AssessmentResults results = DiscService.Score( moreD, moreI, moreS, moreC, lessD, lessI, lessS, lessC );

                // Now save the results for this targetPerson
                DiscService.SaveAssessmentResults(
                    targetPerson,
                    results.AdaptiveBehaviorD.ToString(),
                    results.AdaptiveBehaviorI.ToString(),
                    results.AdaptiveBehaviorS.ToString(),
                    results.AdaptiveBehaviorC.ToString(),
                    results.NaturalBehaviorD.ToString(),
                    results.NaturalBehaviorI.ToString(),
                    results.NaturalBehaviorS.ToString(),
                    results.NaturalBehaviorC.ToString(),
                    results.PersonalityType );

                // Create a dictionary of QuestionNumbers whose value is an
                // object containing the most and least scores for that question.
                var assessmentData = box.Responses.ToDictionary( a =>
                    a.QuestionNumber,
                    b => new
                    {
                        Most = new string[2] {
                            b.MostScore,
                            b.Questions[b.MostScore]
                        },
                        Least = new string[2] {
                            b.LeastScore,
                            b.Questions[b.LeastScore]
                        }
                    } );

                var assessmentService = new AssessmentService( RockContext );
                Assessment assessment = null;

                if ( AssessmentId.ToIntSafe() > 0 )
                {
                    assessment = assessmentService.Get( AssessmentId.Value );
                }

                if ( assessment == null )
                {
                    var assessmentType = new AssessmentTypeService( RockContext ).Get( Rock.SystemGuid.AssessmentType.DISC.AsGuid() );
                    assessment = new Assessment()
                    {
                        AssessmentTypeId = assessmentType.Id,
                        PersonAliasId = targetPerson.PrimaryAliasId.Value
                    };
                    assessmentService.Add( assessment );
                }

                var startTime = box.StartDateTime.HasValue ? box.StartDateTime.Value : assessment.CreatedDateTime.Value;
                assessment.Status = AssessmentRequestStatus.Complete;
                assessment.CompletedDateTime = RockDateTime.Now;
                assessment.AssessmentResultData = new { Result = assessmentData, TimeToTake = RockDateTime.Now.Subtract( startTime ).TotalSeconds }.ToJson();
                RockContext.SaveChanges();

                SetResult( results, assessment, box );
            }
            catch ( Exception ex )
            {
                Logger.LogError( "", ex );
            }
        }

        /// <summary>
        /// Sets the various result properties for the Disc Box based on the saved scores.
        /// </summary>
        /// <param name="savedScores">The savedScores for which to populate results with.</param>
        /// <param name="assessment">The assessment whose configuration is used for determining if the individual can retake the test.</param>
        /// <param name="box">The box whose properties should be populated.</param>
        private void SetResult( DiscService.AssessmentResults savedScores, Assessment assessment, DiscInitializationBox box )
        {
            var minDaysToRetake = assessment.AssessmentType.MinimumDaysToRetake;
            var hasQueryStringForPersonKey = PersonKey.IsNotNullOrWhiteSpace();

            box.CanRetakeTest =
                !hasQueryStringForPersonKey
                && assessment.CompletedDateTime.HasValue
                && assessment.CompletedDateTime.Value.AddDays( minDaysToRetake ) <= RockDateTime.Now;

            box.PersonalityType = savedScores.PersonalityType;
            box.LastSavedDate = savedScores.LastSaveDate;

            box.Results = new AssessmentResultsBag
            {
                AdaptiveBehaviorS = savedScores.AdaptiveBehaviorS,
                AdaptiveBehaviorC = savedScores.AdaptiveBehaviorC,
                AdaptiveBehaviorI = savedScores.AdaptiveBehaviorI,
                AdaptiveBehaviorD = savedScores.AdaptiveBehaviorD,
                NaturalBehaviorS = savedScores.NaturalBehaviorS,
                NaturalBehaviorC = savedScores.NaturalBehaviorC,
                NaturalBehaviorI = savedScores.NaturalBehaviorI,
                NaturalBehaviorD = savedScores.NaturalBehaviorD,
                PersonalityType = savedScores.PersonalityType
            };

            SetPersonalityResults( box );
        }

        /// <summary>
        /// Sets the explanation for the given personality type as defined in one of the
        /// DefinedValues of the DISC Results DefinedType.
        /// </summary>
        /// <param name="personalityType">The one or two letter personality type.</param>
        /// <param name="box">The box to set the values for.</param>
        private void SetPersonalityResults( DiscInitializationBox box )
        {
            // Get the DefinedValue for the given personality type from the DISC Results DefinedType.
            var personalityValue = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.DISC_RESULTS_TYPE.AsGuid() )
                .DefinedValues.FirstOrDefault( v => v.Value == box.PersonalityType );

            // If we found a DefinedValue then set the box properties based on it's attributes.
            if ( personalityValue != null )
            {
                box.DiscPersonalityDescription = personalityValue.Description;
                box.DiscStrengths = personalityValue.GetAttributeValue( AttributeKey.Strengths );
                box.DiscChallenges = personalityValue.GetAttributeValue( AttributeKey.Challenges );

                box.DiscUnderPressure = personalityValue.GetAttributeValue( AttributeKey.UnderPressure );
                box.DiscMotivation = personalityValue.GetAttributeValue( AttributeKey.Motivation );
                box.DiscTeamContribution = personalityValue.GetAttributeValue( AttributeKey.TeamContribution );
                box.DiscLeadershipStyle = personalityValue.GetAttributeValue( AttributeKey.LeadershipStyle );
                box.DiscFollowerStyle = personalityValue.GetAttributeValue( AttributeKey.FollowerStyle );
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Saves the assessment and return the updated box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>The box with updated results; or an error message if unsuccessful.</returns>
        [BlockAction]
        public BlockActionResult Save( DiscInitializationBox box )
        {
            var hasQueryStringForPersonKey = PersonKey.IsNotNullOrWhiteSpace();
            var targetPerson = TargetPerson;

            if ( targetPerson == null )
            {
                // Ensure we're able to get the target targetPerson; otherwise return an error message.
                return ActionBadRequest(
                    hasQueryStringForPersonKey ?
                    "There is an issue locating the targetPerson associated with the request." :
                    "You must be signed in to take the assessment." );
            }

            // Check that the assessment type doesn't require a request.
            // Otherwise the front-end could be manipulated to save an assessment without a request.
            var assessmentType = new AssessmentTypeService( RockContext ).Get( Rock.SystemGuid.AssessmentType.DISC.AsGuid() );
            var hasAssessment = new AssessmentService( RockContext )
                .Queryable()
                .Any( a => a.PersonAlias != null
                             && a.PersonAlias.PersonId == targetPerson.Id
                             && a.AssessmentTypeId == assessmentType.Id );

            if ( AssessmentId == 0 && assessmentType.RequiresRequest && !hasAssessment )
            {
                return ActionBadRequest( "Sorry, this test requires a request from someone before it can be taken." );
            }

            // Save the assessment and return the box with the updated information.
            SaveAssessment( box );

            return ActionOk( box );
        }

        #endregion Block Actions
    }
}
