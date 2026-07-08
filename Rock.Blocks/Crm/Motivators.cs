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
using Rock.ViewModels.Blocks.Crm.Motivators;
using Rock.ViewModels.Utility;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Calculates a person's motivators assessment score based on a series of questions and answers.
    /// </summary>
    [DisplayName( "Motivators Assessment" )]
    [Category( "CRM" )]
    [Description( "Allows you to take a Motivators Assessment test and saves your results." )]

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

    [CodeEditorField(
        "Results Message",
        Key = AttributeKey.ResultsMessage,
        Description = "The text (HTML) to display at the top of the results section.<span class='tip tip-lava'></span><span class='tip tip-html'></span>",
        EditorMode = CodeEditorMode.Html,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = ResultMessageDefaultValue,
        Order = 1 )]

    [TextField(
        "Set Page Title",
        Key = AttributeKey.SetPageTitle,
        Description = "The text to display as the heading.",
        IsRequired = false,
        DefaultValue = "Motivators Assessment",
        Order = 2 )]

    [TextField(
        "Set Page Icon",
        Key = AttributeKey.SetPageIcon,
        Description = "The css class name to use for the heading icon.",
        IsRequired = false,
        DefaultValue = "ti ti-key",
        Order = 3 )]

    [IntegerField(
        "Number of Questions",
        Key = AttributeKey.NumberOfQuestions,
        Description = "The number of questions to show per page while taking the test",
        IsRequired = true,
        DefaultIntegerValue = 20,
        Order = 4 )]

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Primary )]
    [Rock.SystemGuid.EntityTypeGuid( "281DBFCF-14F1-4134-9A78-06A9D1D9E8A0" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "A291A539-6901-4EFD-A892-27180DBCAFFA" )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOTIVATORS )]
    public class Motivators : RockBlockType
    {
        #region Attribute Default Values

        private const string InstructionsDefaultValue = @"
<h2>Welcome to the Motivators Assessment</h2>
<p>
    {{ Person.NickName }}, our values dictate what we determine is important, how we lead and how
    we interact with others; in short they affect every area of our life. This assessment identifies
    the motivators that drive your perspective and goals.
</p>
<p>
   For best results with this assessment, picture a setting such as the workplace, at home or with
   friends, and keep that same setting in mind as you answer all the questions. Your responses may
   be different in different circumstances.
</p>
<p>
    Don’t spend too much time thinking about your answer. Usually, your first responses is your
    most natural. Since there are no right or wrong answers, just go with your instinct.
</p>";

        private const string ResultMessageDefaultValue = @"<p>
    {{ Person.NickName }}, here are your motivators results. We’ve listed your Top 5 Motivators, your
    growth propensity score, along with a complete listing of all 22 motivators and your results
    for each.
</p>
<h2>Growth Propensity</h2>
<p>
    Growth Propensity measures your perceived mindset on a continuum between a growth mindset and
    fixed mindset. These are two ends of a spectrum about how we view our own capacity and potential.
</p>
<div style='margin: 0;max-width:280px'>
    {[ chart type:'gauge' backgroundcolor:'#f13c1f,#f0e3ba,#0e9445,#3f56a1' gaugelimits:'0,2,17,85,100' chartheight:'150px']}
        [[ dataitem value:'{{ GrowthScore }}' fillcolor:'#484848' ]] [[ enddataitem ]]
    {[ endchart ]}
</div>
<h2>Individual Motivators</h2>
<p>
    There are 22 possible motivators in this assessment. While your Top 5 Motivators may be most helpful in understanding your results in a snapshot, you may also find it helpful to see your scores on each for a complete picture.
</p>
<!-- Theme Chart -->
<div class='panel panel-default'>
    <div class='panel-heading'>
        <h2 class='panel-title'><b>Composite Score</b></h2>
    </div>
    <div class='panel-body'>
        {[chart type:'horizontalBar' chartheight:'200px' xaxistype:'linearhorizontal0to100' ]}
            {% for motivatorThemeScore in MotivatorThemeScores %}
                [[dataitem label:'{{ motivatorThemeScore.DefinedValue.Value }}' value:'{{ motivatorThemeScore.Value }}' fillcolor:'{{ motivatorThemeScore.DefinedValue | Attribute:'Color' }}' ]]
                [[enddataitem]]
            {% endfor %}
        {[endchart]}
    </div>
</div>
<p>
    This graph is based on the average composite score for each Motivator Theme.
</p>
{% for motivatorThemeScore in MotivatorThemeScores %}
    <p>
        <b>{{ motivatorThemeScore.DefinedValue.Value }}</b>
        <br>
        {{ motivatorThemeScore.DefinedValue.Description }}
        <br>
        {{ motivatorThemeScore.DefinedValue | Attribute:'Summary' }}
    </p>
{% endfor %}
<p>
    The following graph shows your motivators ranked from top to bottom.
</p>
<div class='panel panel-default'>
    <div class='panel-heading'>
        <h2 class='panel-title'><b>Ranked Motivators</b></h2>
    </div>
    <div class='panel-body'>
        {[ chart type:'horizontalBar' xaxistype:'linearhorizontal0to100' ]}
            {% for motivatorScore in MotivatorScores %}
                {% assign theme = motivatorScore.DefinedValue | Attribute:'Theme' %}
                {% if theme and theme != empty %}
                    [[dataitem label:'{{ motivatorScore.DefinedValue.Value }}' value:'{{ motivatorScore.Value }}' fillcolor:'{{ motivatorScore.DefinedValue | Attribute:'Color' }}' ]]
                    [[enddataitem]]
                {% endif %}
            {% endfor %}
        {[endchart]}
    </div>
</div>";

        #endregion Attribute Default Values

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string Instructions = "Instructions";
            public const string ResultsMessage = "ResultsMessage";
            public const string SetPageTitle = "SetPageTitle";
            public const string SetPageIcon = "SetPageIcon";
            public const string NumberOfQuestions = "NumberofQuestions";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        /// <summary>
        /// A defined list of page parameter keys used by this block.
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The assessment identifier.
            /// </summary>
            public const string AssessmentId = "AssessmentId";

            /// <summary>
            /// The URL encoded key for a person.
            /// </summary>
            public const string Person = "Person";
        }

        #endregion Page Parameter Keys

        #region Fields

        /// <summary>
        /// The cached assessment state for the duration of the current request.
        /// </summary>
        private MotivatorAssessmentState _assessmentState;

        #endregion Fields

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
        /// The panel title configured by the block settings.
        /// </summary>
        private string PanelTitle => GetAttributeValue( AttributeKey.SetPageTitle ).ToStringSafe();

        /// <summary>
        /// The panel icon CSS class configured by the block settings.
        /// </summary>
        private string PanelIcon => GetAttributeValue( AttributeKey.SetPageIcon ).ToStringSafe();

        /// <summary>
        /// The number of questions to show per page (defaults to 20 if not configured).
        /// </summary>
        private int PageSize => GetAttributeValue( AttributeKey.NumberOfQuestions ).ToIntSafe( 20 );

        /// <summary>
        /// Gets the target person from the "Person" page parameter, or the current person if not provided.
        /// </summary>
        private Person TargetPerson
        {
            get
            {
                var personKey = PersonKey;

                if ( personKey.IsNotNullOrWhiteSpace() )
                {
                    // A malformed or tampered key can throw while decrypting; treat any failure as "not found"
                    // so the individual sees a friendly message rather than an error.
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

        #endregion Attributes and Parameters

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return GetInitializationBox();
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Scores and saves the completed assessment, returning the box updated to display the results.
        /// </summary>
        /// <param name="box">The box containing the individual's responses.</param>
        /// <returns>The box with the rendered results; or an error message if the assessment cannot be saved.</returns>
        [BlockAction]
        public BlockActionResult Save( MotivatorsInitializationBox box )
        {
            var hasQueryStringForPersonKey = PersonKey.IsNotNullOrWhiteSpace();
            var targetPerson = TargetPerson;

            if ( targetPerson == null )
            {
                return ActionBadRequest( hasQueryStringForPersonKey
                    ? "There is an issue locating the person associated with the request."
                    : "You must be signed in to take the assessment." );
            }

            var assessmentType = new AssessmentTypeService( RockContext ).Get( Rock.SystemGuid.AssessmentType.MOTIVATORS.AsGuid() );
            var assessmentService = new AssessmentService( RockContext );

            // Locate the assessment to update, ensuring it belongs to the target person and is the correct type.
            Assessment assessment = null;
            if ( box.AssessmentId > 0 )
            {
                assessment = assessmentService.Queryable()
                    .FirstOrDefault( a => a.Id == box.AssessmentId
                        && a.PersonAlias != null
                        && a.PersonAlias.PersonId == targetPerson.Id
                        && a.AssessmentTypeId == assessmentType.Id );
            }

            // A request-only assessment type can only be taken when a request already exists. This guards
            // against the front-end being manipulated to save an assessment that was never requested.
            if ( assessment == null && assessmentType.RequiresRequest )
            {
                var hasExistingAssessment = assessmentService.Queryable()
                    .Any( a => a.PersonAlias != null
                        && a.PersonAlias.PersonId == targetPerson.Id
                        && a.AssessmentTypeId == assessmentType.Id );

                if ( !hasExistingAssessment )
                {
                    return ActionBadRequest( "Sorry, this test requires a request from someone before it can be taken." );
                }
            }

            if ( box.Responses == null || !box.Responses.Any() )
            {
                return ActionBadRequest( "Please answer the assessment questions before submitting." );
            }

            // Each response value is the score recorded for the option the individual selected.
            // Group by code (last value wins) so a malformed payload with duplicate or empty
            // codes cannot throw while the score dictionary is built.
            var responseScores = box.Responses
                .Where( r => r.Code.IsNotNullOrWhiteSpace() && r.Response.IsNotNullOrWhiteSpace() )
                .GroupBy( r => r.Code )
                .ToDictionary( group => group.Key, group => group.Last().Response.AsInteger() );

            var result = MotivatorService.GetResult( responseScores );
            MotivatorService.SaveAssessmentResults( targetPerson, result );

            if ( assessment == null )
            {
                assessment = new Assessment
                {
                    AssessmentTypeId = assessmentType.Id,
                    PersonAliasId = targetPerson.PrimaryAliasId.Value
                };
                assessmentService.Add( assessment );
            }

            // The client measures the elapsed time from the moment the individual starts the test, which avoids
            // counting time spent reading the instructions and is immune to client/server clock differences.
            // Fall back to the server-side start time if the client did not provide a value.
            var timeToTakeSeconds = box.TimeToTake ?? RockDateTime.Now.Subtract( box.StartDateTime ?? RockDateTime.Now ).TotalSeconds;

            assessment.Status = AssessmentRequestStatus.Complete;
            assessment.CompletedDateTime = RockDateTime.Now;
            assessment.AssessmentResultData = new
            {
                Result = result.AssessmentData,
                TimeToTake = timeToTakeSeconds
            }.ToJson();

            RockContext.SaveChanges();

            // Update the box to display the freshly computed results.
            box.ShowResults = true;
            box.AssessmentId = assessment.Id;
            box.ResultsHtml = GetResultsHtml( targetPerson );
            box.CanRetakeTest =
                !hasQueryStringForPersonKey
                && !assessmentType.RequiresRequest
                && assessment.CompletedDateTime.Value.AddDays( assessmentType.MinimumDaysToRetake ) <= RockDateTime.Now;

            return ActionOk( box );
        }

        #endregion Block Actions

        #region Private Methods

        /// <summary>
        /// Builds the initialization box, populated with the test, the results, or an error message.
        /// </summary>
        /// <returns>The populated initialization box.</returns>
        private MotivatorsInitializationBox GetInitializationBox()
        {
            var state = GetAssessmentState();

            var box = new MotivatorsInitializationBox
            {
                PanelTitle = PanelTitle,
                PanelIcon = PanelIcon,
                PageSize = PageSize,
                StartDateTime = RockDateTime.Now
            };

            if ( state.ErrorMessage.IsNotNullOrWhiteSpace() )
            {
                box.ErrorMessage = state.ErrorMessage;
                return box;
            }

            box.Responses = BuildResponses();
            box.Instructions = GetInstructionsHtml( state.TargetPerson );
            box.AssessmentId = state.AssessmentRecordId;
            box.InfoMessage = state.InfoMessage;
            box.ShowResults = state.ShowResults;

            if ( state.ShowResults && state.ResultsAssessment != null )
            {
                box.ResultsHtml = GetResultsHtml( state.TargetPerson );
                box.CanRetakeTest =
                    !state.HasQueryStringForPersonKey
                    && !state.AssessmentType.RequiresRequest
                    && state.ResultsAssessment.CompletedDateTime.HasValue
                    && state.ResultsAssessment.CompletedDateTime.Value.AddDays( state.AssessmentType.MinimumDaysToRetake ) <= RockDateTime.Now;
            }

            return box;
        }

        /// <summary>
        /// Determines which assessment (if any) should be shown for the target person and whether the
        /// block should display the test or the completed results. The result is cached for the request.
        /// </summary>
        /// <returns>The resolved assessment state.</returns>
        private MotivatorAssessmentState GetAssessmentState()
        {
            return _assessmentState ?? ( _assessmentState = BuildAssessmentState() );
        }

        /// <summary>
        /// Resolves the assessment state from the page parameters and the target person's assessment history.
        /// </summary>
        /// <returns>The resolved assessment state.</returns>
        private MotivatorAssessmentState BuildAssessmentState()
        {
            /*
                This block will either show the results of the most recent completed assessment or present the
                test to take. The cases handled below mirror the long-standing assessment behavior:
                1. AssessmentId "0" creates a new test for the current individual (a user-directed re-take).
                2. AssessmentId provided and not "0": the most recent assessment for the person is used —
                   completed assessments show results; a pending assessment for the current person shows the test.
                3. No AssessmentId but a PersonKey: the latest assessment is used, falling back to the most recent
                   completed assessment (with an informational message) when a newer request is still pending and
                   the viewer is not the assigned individual.
                4. Otherwise an error message is shown.
            */

            var state = new MotivatorAssessmentState
            {
                HasQueryStringForPersonKey = PersonKey.IsNotNullOrWhiteSpace()
            };

            var targetPerson = TargetPerson;
            state.TargetPerson = targetPerson;

            if ( targetPerson == null )
            {
                state.ErrorMessage = state.HasQueryStringForPersonKey
                    ? "There is an issue locating the person associated with the request."
                    : "You must be signed in to take the assessment.";
                return state;
            }

            var assessmentType = new AssessmentTypeService( RockContext ).Get( Rock.SystemGuid.AssessmentType.MOTIVATORS.AsGuid() );
            state.AssessmentType = assessmentType;

            Assessment assessment = null;
            Assessment previouslyCompletedAssessment = null;
            var assessmentId = AssessmentId;

            // A "0" value indicates a user-directed new test, so leave the assessment unresolved.
            if ( assessmentId != 0 )
            {
                var assessments = new AssessmentService( RockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( a => a.PersonAlias != null
                        && a.PersonAlias.PersonId == targetPerson.Id
                        && a.AssessmentTypeId == assessmentType.Id )
                    .ToList()
                    .OrderByDescending( a => a.CompletedDateTime ?? a.RequestedDateTime )
                    .ToList();

                if ( assessmentId == null && !assessments.Any() )
                {
                    // The individual has never taken the assessment and arrived without an AssessmentId,
                    // so treat this as a user-directed take.
                    assessmentId = 0;
                }
                else
                {
                    if ( assessments.Count > 0 )
                    {
                        assessment = assessments[0];
                    }

                    if ( assessments.Count > 1 && assessment.Status == AssessmentRequestStatus.Pending )
                    {
                        previouslyCompletedAssessment = assessments.FirstOrDefault( a => a.Status == AssessmentRequestStatus.Complete );
                    }
                }
            }

            if ( assessment == null )
            {
                if ( assessmentId == 0 && !assessmentType.RequiresRequest )
                {
                    // User-directed take: present the test and create a new assessment when it is completed.
                    state.AssessmentRecordId = 0;
                }
                else
                {
                    state.ErrorMessage = "Sorry, this test requires a request from someone before it can be taken.";
                }

                return state;
            }

            state.AssessmentRecordId = assessment.Id;

            if ( assessment.Status == AssessmentRequestStatus.Complete )
            {
                state.ShowResults = true;
                state.ResultsAssessment = assessment;
                return state;
            }

            if ( assessment.Status == AssessmentRequestStatus.Pending )
            {
                var isForCurrentPerson = targetPerson.Id == GetCurrentPerson()?.Id;
                if ( !isForCurrentPerson )
                {
                    if ( previouslyCompletedAssessment != null )
                    {
                        // A newer request is pending but unassigned to the viewer, so show the most recent completed results.
                        state.ShowResults = true;
                        state.ResultsAssessment = previouslyCompletedAssessment;
                        state.AssessmentRecordId = previouslyCompletedAssessment.Id;
                        state.InfoMessage = "A more recent assessment request has been made but has not been taken. Displaying the most recently completed test.";
                        return state;
                    }

                    state.ErrorMessage = $"{targetPerson.FullName} does not have results for the {assessmentType.Title} Assessment.";
                    return state;
                }

                // The pending assessment is assigned to the current individual, so present the test.
                return state;
            }

            state.ErrorMessage = "Unable to load assessment";
            return state;
        }

        /// <summary>
        /// Builds the randomized list of questions with each question's answer options pre-scored.
        /// </summary>
        /// <returns>The list of response bags representing the questions to answer.</returns>
        private List<MotivatorsResponseBag> BuildResponses()
        {
            return MotivatorService.GetQuestions()
                .Select( question => new MotivatorsResponseBag
                {
                    Code = question.Id,
                    Question = question.Question,
                    Options = BuildOptions( question )
                } )
                .ToList();
        }

        /// <summary>
        /// Builds the answer options for a question. Each option's value is the score recorded when selected.
        /// Negatively worded questions (those whose code ends with "N") use the inverse score.
        /// </summary>
        /// <param name="question">The question to build options for.</param>
        /// <returns>The list of answer options.</returns>
        private static List<ListItemBag> BuildOptions( MotivatorService.MotivatorQuestion question )
        {
            var options = question.OptionType == MotivatorService.OptionType.Frequency
                ? MotivatorService.Frequency_Option
                : MotivatorService.Agreement_Option;

            var isNegativelyScored = question.Id.EndsWith( "N" );

            return options
                .Select( option => new ListItemBag
                {
                    Text = option.Name,
                    Value = ( isNegativelyScored ? option.Negative : option.Positive ).ToString()
                } )
                .ToList();
        }

        /// <summary>
        /// Resolves the merge fields in the Instructions Lava template.
        /// </summary>
        /// <param name="targetPerson">The person the assessment is for.</param>
        /// <returns>The resolved instructions HTML.</returns>
        private string GetInstructionsHtml( Person targetPerson )
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, targetPerson );
            if ( targetPerson != null )
            {
                mergeFields.Add( "Person", targetPerson );
            }

            return GetAttributeValue( AttributeKey.Instructions ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Resolves the Results Message Lava template using the person's saved assessment scores.
        /// </summary>
        /// <param name="targetPerson">The person whose saved results should be rendered.</param>
        /// <returns>The resolved results HTML, including any chart shortcodes.</returns>
        private string GetResultsHtml( Person targetPerson )
        {
            var results = MotivatorService.LoadSavedAssessmentResults( targetPerson );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, targetPerson );
            mergeFields.Add( "Person", targetPerson );
            mergeFields.Add( "MotivatorThemeScores", results.MotivatorThemeScores );
            mergeFields.Add( "MotivatorScores", results.MotivatorScores );
            mergeFields.Add( "GrowthScore", results.GrowthScore );

            return GetAttributeValue( AttributeKey.ResultsMessage ).ResolveMergeFields( mergeFields );
        }

        #endregion Private Methods

        #region Support Classes

        /// <summary>
        /// Holds the resolved state used to render the block for the current request.
        /// </summary>
        private class MotivatorAssessmentState
        {
            /// <summary>
            /// Gets or sets the person the assessment is for.
            /// </summary>
            public Person TargetPerson { get; set; }

            /// <summary>
            /// Gets or sets the assessment type for the Motivators assessment.
            /// </summary>
            public AssessmentType AssessmentType { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether a "Person" page parameter was supplied.
            /// </summary>
            public bool HasQueryStringForPersonKey { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the assessment record to write to. A value of zero
            /// indicates a new assessment should be created when the test is completed.
            /// </summary>
            public int AssessmentRecordId { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the completed results should be displayed.
            /// </summary>
            public bool ShowResults { get; set; }

            /// <summary>
            /// Gets or sets the completed assessment whose results should be displayed.
            /// </summary>
            public Assessment ResultsAssessment { get; set; }

            /// <summary>
            /// Gets or sets an error message that prevents the assessment from being shown.
            /// </summary>
            public string ErrorMessage { get; set; }

            /// <summary>
            /// Gets or sets an informational message to display alongside the results.
            /// </summary>
            public string InfoMessage { get; set; }
        }

        #endregion Support Classes
    }
}
