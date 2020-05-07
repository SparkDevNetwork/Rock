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
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rockweb.Blocks.Crm
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
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = InstructionsDefaultValue,
        Order = 0 )]

    [CodeEditorField(
        "Results Message",
        Key = AttributeKey.ResultsMessage,
        Description = "The text (HTML) to display at the top of the results section.<span class='tip tip-lava'></span><span class='tip tip-html'></span>",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = ResultMessageDefaultValue,
        Order = 1 )]

    [TextField( "Set Page Title",
        Key = AttributeKey.SetPageTitle,
        Description = "The text to display as the heading.",
        IsRequired = false,
        DefaultValue = "Motivators Assessment",
        Order = 2 )]

    [TextField( "Set Page Icon",
        Key = AttributeKey.SetPageIcon,
        Description = "The css class name to use for the heading icon.",
        IsRequired = false,
        DefaultValue = "fa fa-key",
        Order = 3 )]

    [IntegerField(
        "Number of Questions",
        Key = AttributeKey.NumberOfQuestions,
        Description = "The number of questions to show per page while taking the test",
        IsRequired = true,
        DefaultIntegerValue = 20,
        Order = 4 )]
    #endregion Block Attributes

    public partial class Motivators : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys
        private static class AttributeKey
        {
            public const string Instructions = "Instructions";
            public const string SetPageTitle = "SetPageTitle";
            public const string SetPageIcon = "SetPageIcon";
            public const string ResultsMessage = "ResultsMessage";
            public const string NumberOfQuestions = "NumberofQuestions";
        }

        #endregion Attribute Keys

        #region PageParameterKeys
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
            /// The ULR encoded key for a person
            /// </summary>
            public const string Person = "Person";
        }

        #endregion PageParameterKeys

        #region Fields

        // View State Keys
        private const string ASSESSMENT_STATE = "AssessmentState";
        private const string START_DATETIME = "StartDateTime";

        // View State Variables
        private List<AssessmentResponse> _assessmentResponses;

        // used for private variables
        private Person _targetPerson = null;
        private int? _assessmentId = null;
        private bool _isQuerystringPersonKey = false;

        // protected variables
        private decimal _percentComplete = 0;

        #endregion

        #region Attribute Default values
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

        #endregion Attribute Default values

        #region Properties

        /// <summary>
        /// Gets or sets the percent complete.
        /// </summary>
        /// <value>
        /// The percent complete.
        /// </value>
        public decimal PercentComplete
        {
            get
            {
                return _percentComplete;
            }

            set
            {
                _percentComplete = value;
            }
        }

        /// <summary>
        /// Gets or sets the total number of questions
        /// </summary>
        public int QuestionCount
        {
            get { return ViewState[AttributeKey.NumberOfQuestions] as int? ?? 0; }
            set { ViewState[AttributeKey.NumberOfQuestions] = value; }
        }

        /// <summary>
        /// Gets or sets the time to take the result
        /// </summary>
        public DateTime StartDateTime
        {
            get { return ViewState[START_DATETIME] as DateTime? ?? RockDateTime.Now; }
            set { ViewState[START_DATETIME] = value; }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            _assessmentResponses = ViewState[ASSESSMENT_STATE] as List<AssessmentResponse> ?? new List<AssessmentResponse>();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            this.BlockUpdated += Block_BlockUpdated;

            SetPanelTitleAndIcon();

            _assessmentId = PageParameter( PageParameterKey.AssessmentId ).AsIntegerOrNull();
            string personKey = PageParameter( PageParameterKey.Person );

            // set the target person according to the parameter or use Current user if not provided.
            if ( personKey.IsNotNullOrWhiteSpace() )
            {
                try
                {
                    var personService = new PersonService( new RockContext() );
                    _targetPerson = personService.GetByPersonActionIdentifier( personKey, "Assessment" ) ?? personService.GetByUrlEncodedKey( personKey );
                    _isQuerystringPersonKey = true;
                }
                catch ( Exception )
                {
                    nbError.Visible = true;
                }
            }
            else if ( CurrentPerson != null )
            {
                _targetPerson = CurrentPerson;
            }

            if ( _targetPerson == null )
            {
                if ( _isQuerystringPersonKey )
                {
                    HidePanelsAndShowError( "There is an issue locating the person associated with the request." );
                }
                else
                {
                    HidePanelsAndShowError( "You must be signed in to take the assessment.");
                }
            }


        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowAssessment();
            }
            else
            {
                // Hide notification panels on every postback
                nbError.Visible = false;
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState[ASSESSMENT_STATE] = _assessmentResponses;

            return base.SaveViewState();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// We need to reload the page for the charts to appear.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            if ( pnlResult.Visible == true )
            {
                this.NavigateToCurrentPageReference();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnStart button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnStart_Click( object sender, EventArgs e )
        {
            StartDateTime = RockDateTime.Now;
            ShowQuestions();
        }

        /// <summary>
        /// Handles the Click event of the btnRetakeTest button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRetakeTest_Click( object sender, EventArgs e )
        {
            hfAssessmentId.SetValue( 0 );
            ShowInstructions();
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnNext_Click( object sender, EventArgs e )
        {
            int pageNumber = hfPageNo.ValueAsInt() + 1;
            GetResponse();

            LinkButton btn = ( LinkButton ) sender;
            string commandArgument = btn.CommandArgument;

            var totalQuestion = pageNumber * QuestionCount;
            if ( ( _assessmentResponses.Count > totalQuestion && !_assessmentResponses.All( a => a.Response.HasValue ) ) || "Next".Equals( commandArgument ) )
            {
                BindRepeater( pageNumber );
            }
            else
            {
                MotivatorService.AssessmentResults result = MotivatorService.GetResult( _assessmentResponses.ToDictionary( a => a.Code, b => b.Response.Value ) );
                MotivatorService.SaveAssessmentResults( _targetPerson, result );
                var rockContext = new RockContext();

                var assessmentService = new AssessmentService( rockContext );
                Assessment assessment = null;

                if ( hfAssessmentId.ValueAsInt() != 0 )
                {
                    assessment = assessmentService.Get( int.Parse( hfAssessmentId.Value ) );
                }

                if ( assessment == null )
                {
                    var assessmentType = new AssessmentTypeService( rockContext ).Get( Rock.SystemGuid.AssessmentType.MOTIVATORS.AsGuid() );
                    assessment = new Assessment()
                    {
                        AssessmentTypeId = assessmentType.Id,
                        PersonAliasId = _targetPerson.PrimaryAliasId.Value
                    };
                    assessmentService.Add( assessment );
                }

                assessment.Status = AssessmentRequestStatus.Complete;
                assessment.CompletedDateTime = RockDateTime.Now;
                assessment.AssessmentResultData = new { Result = result.AssessmentData, TimeToTake = RockDateTime.Now.Subtract( StartDateTime ).TotalSeconds }.ToJson();
                rockContext.SaveChanges();

                // Since we are rendering chart.js we have to register the script or reload the page.
                if ( _assessmentId == 0 )
                {
                    var removeParams = new List<string>
                    {
                        PageParameterKey.AssessmentId
                    };

                    NavigateToCurrentPageReferenceWithRemove( removeParams );
                }
                else
                {
                    this.NavigateToCurrentPageReference();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnPrevious control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnPrevious_Click( object sender, EventArgs e )
        {
            int pageNumber = hfPageNo.ValueAsInt() - 1;
            GetResponse();
            BindRepeater( pageNumber );
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rQuestions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rQuestions_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var assessmentResponseRow = e.Item.DataItem as AssessmentResponse;
            RockRadioButtonList rblQuestion = e.Item.FindControl( "rblQuestion" ) as RockRadioButtonList;

            if ( assessmentResponseRow.OptionType == MotivatorService.OptionType.Frequency )
            {
                rblQuestion.DataSource = MotivatorService.Frequency_Option;
            }
            else
            {
                rblQuestion.DataSource = MotivatorService.Agreement_Option;
            }

            rblQuestion.DataTextField = "Name";

            if ( assessmentResponseRow.Code.EndsWith( "N" ) )
            {
                rblQuestion.DataValueField = "Negative";
            }
            else
            {
                rblQuestion.DataValueField = "Positive";
            }

            rblQuestion.DataBind();

            rblQuestion.Label = assessmentResponseRow.Question;

            if ( assessmentResponseRow != null && assessmentResponseRow.Response.HasValue )
            {
                rblQuestion.SetValue( assessmentResponseRow.Response );
            }
            else
            {
                rblQuestion.SetValue( string.Empty );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the assessment.
        /// A null value for _targetPerson is already handled in OnInit() so this method assumes there is a value
        /// </summary>
        private void ShowAssessment()
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
                b. If the assessment exists for the current person and is completed then show the results
                c. If the assessment exists for the current person and is pending then show the questions.
                d. If the assessment does not exist for the current person then nothing loads.
            3. If the assessment ID was not provided and the PersonKey was provided
                a. If there is only one test of the type
                    1. If the assessment is completed show the results
                    2. If the assessment is pending and the current person is the one assigned the test then show the questions.
                    3. If the assessment is pending and the current person is not the one assigned then show a message that the test has not been completed.
                b. If more than one of type
                    1. If the latest requested assessment is completed show the results.
                    2. If the latest requested assessment is pending and the current person is the one assigned then show the questions.
                    3. If the latest requested assessment is pending and the current person is not the one assigned the show the results of the last completed test.
                    4. If the latest requested assessment is pending and the current person is not the one assigned and there are no previous completed assessments then show a message that the test has not been completed.
            4. If an assessment ID or PersonKey were not provided or are not valid then show an error message
            */

            var rockContext = new RockContext();
            var assessmentType = new AssessmentTypeService( rockContext ).Get( Rock.SystemGuid.AssessmentType.MOTIVATORS.AsGuid() );
            Assessment assessment = null;
            Assessment previouslyCompletedAssessment = null;

            // A "0" value indicates that the block should create a new assessment instead of looking for an existing one, so keep assessment null. e.g. a user directed re-take
            if ( _assessmentId != 0 )
            {
                var assessments = new AssessmentService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( a => a.PersonAlias != null
                             && a.PersonAlias.PersonId == _targetPerson.Id
                             && a.AssessmentTypeId == assessmentType.Id )
                .OrderByDescending( a => a.CompletedDateTime ?? a.RequestedDateTime )
                .ToList();

                if ( _assessmentId == null && assessments.Count == 0 )
                {
                    // For this to happen the user has to have never taken the assessment, the user isn't using a link with the assessment ID, AND they are arriving at the block directly rather than through the assessment list block.
                    // So treat this as a user directed take/retake.
                    _assessmentId = 0;
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
                        // If the most recent assessment is "Completed" then it is already set as the assessment and we can move on. Otherwise check if there are previoulsy completed assessments.
                        if ( assessment.Status == AssessmentRequestStatus.Pending )
                        {
                            // If the most recent assessment is pending then check for a prior completed one
                            previouslyCompletedAssessment = assessments.Where( a => a.Status == AssessmentRequestStatus.Complete ).FirstOrDefault();
                        }
                    }
                }
            }

            if ( assessment == null )
            {
                // If assessment is null and _assessmentId = 0 this is user directed. If the type does not require a request then show instructions
                if ( _assessmentId == 0 && !assessmentType.RequiresRequest )
                {
                    hfAssessmentId.SetValue( 0 );
                    ShowInstructions();
                }
                else
                {
                    // If assessment is null and _assessmentId != 0 or is 0 but the type does require a request then show requires request error
                    HidePanelsAndShowError( "Sorry, this test requires a request from someone before it can be taken." );
                }

                return;
            }

            hfAssessmentId.SetValue( assessment.Id );

            // If assessment is completed show the results
            if ( assessment.Status == AssessmentRequestStatus.Complete )
            {
                MotivatorService.AssessmentResults savedScores = MotivatorService.LoadSavedAssessmentResults( _targetPerson );
                ShowResult( savedScores, assessment );
                return;
            }

            if ( assessment.Status == AssessmentRequestStatus.Pending )
            {
                if ( _targetPerson.Id != CurrentPerson.Id )
                {
                    // If assessment is pending and the current person is not the one assigned the show previouslyCompletedAssessment results
                    if ( previouslyCompletedAssessment != null )
                    {
                        MotivatorService.AssessmentResults savedScores = MotivatorService.LoadSavedAssessmentResults( _targetPerson );
                        ShowResult( savedScores, previouslyCompletedAssessment, true );
                        return;
                    }

                    // If assessment is pending and the current person is not the one assigned and previouslyCompletedAssessment is null show a message that the test has not been completed.
                    HidePanelsAndShowError( string.Format("{0} does not have results for the {1} Assessment.", _targetPerson.FullName, assessmentType.Title ) );
                }
                else
                {
                    // If assessment is pending and the current person is the one assigned then show the questions
                    ShowInstructions();
                }

                return;
            }

            // This should never happen, if the block gets to this point then something is not right
            HidePanelsAndShowError( "Unable to load assessment" );
        }

        /// <summary>
        /// Hides the Instructions and Questions panels and shows the specified error.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        private void HidePanelsAndShowError( string errorMessage )
        {
            pnlInstructions.Visible = false;
            pnlQuestion.Visible = false;
            pnlResult.Visible = false;
            ShowNotification( errorMessage, NotificationBoxType.Danger );
        }

        /// <summary>
        /// Shows the notification.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="notificationBoxType">Type of the notification box.</param>
        private void ShowNotification( string errorMessage, NotificationBoxType notificationBoxType )
        {
            nbError.Visible = true;
            nbError.Text = errorMessage;
            nbError.NotificationBoxType = notificationBoxType;
        }

        /// <summary>
        /// Sets the page title and icon.
        /// </summary>
        private void SetPanelTitleAndIcon()
        {
            string panelTitle = this.GetAttributeValue( AttributeKey.SetPageTitle );
            if ( !string.IsNullOrEmpty( panelTitle ) )
            {
                lTitle.Text = panelTitle;
            }

            string panelIcon = this.GetAttributeValue( AttributeKey.SetPageIcon );
            if ( !string.IsNullOrEmpty( panelIcon ) )
            {
                iIcon.Attributes["class"] = panelIcon;
            }
        }

        /// <summary>
        /// Shows the instructions.
        /// </summary>
        private void ShowInstructions()
        {
            pnlInstructions.Visible = true;
            pnlQuestion.Visible = false;
            pnlResult.Visible = false;

            // Resolve the text field merge fields
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, _targetPerson );
            if ( _targetPerson != null )
            {
                mergeFields.Add( "Person", _targetPerson );
            }

            lInstructions.Text = GetAttributeValue( AttributeKey.Instructions ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Shows the result.
        /// </summary>
        private void ShowResult( MotivatorService.AssessmentResults result, Assessment assessment, bool isPrevious = false )
        {
            pnlInstructions.Visible = false;
            pnlQuestion.Visible = false;
            pnlResult.Visible = true;
            btnRetakeTest.Visible = false;

            if ( isPrevious )
            {
                ShowNotification( "A more recent assessment request has been made but has not been taken. Displaying the most recently completed test.", NotificationBoxType.Info );
            }

            bool requiresRequest = assessment.AssessmentType.RequiresRequest;
            var minDays = assessment.AssessmentType.MinimumDaysToRetake;

            if ( !_isQuerystringPersonKey && !requiresRequest && assessment.CompletedDateTime.HasValue && assessment.CompletedDateTime.Value.AddDays( minDays ) <= RockDateTime.Now )
            {
                btnRetakeTest.Visible = true;
            }

            // Resolve the text field merge fields
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, _targetPerson );
            if ( _targetPerson != null )
            {
                _targetPerson.LoadAttributes();
                mergeFields.Add( "Person", _targetPerson );

                // The five Mode scores
                mergeFields.Add( "MotivatorThemeScores", result.MotivatorThemeScores );
                mergeFields.Add( "MotivatorScores", result.MotivatorScores );
                mergeFields.Add( "GrowthScore", result.GrowthScore );
            }

            lResult.Text = GetAttributeValue( AttributeKey.ResultsMessage ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Shows the questions.
        /// </summary>
        private void ShowQuestions()
        {
            pnlInstructions.Visible = false;
            pnlQuestion.Visible = true;
            pnlResult.Visible = false;
            _assessmentResponses = MotivatorService.GetQuestions()
                                    .Select( a => new AssessmentResponse()
                                    {
                                        Code = a.Id,
                                        Question = a.Question,
                                        OptionType = a.OptionType
                                    } ).ToList();

            // If _maxQuestions has not been set yet...
            if ( QuestionCount == 0 && _assessmentResponses != null )
            {
                // Set the max number of questions to be no greater than the actual number of questions.
                int numQuestions = this.GetAttributeValue( AttributeKey.NumberOfQuestions ).AsInteger();
                QuestionCount = ( numQuestions > _assessmentResponses.Count ) ? _assessmentResponses.Count : numQuestions;
            }

            BindRepeater( 0 );
        }

        /// <summary>
        /// Binds the question data to the rQuestions repeater control.
        /// </summary>
        private void BindRepeater( int pageNumber )
        {
            hfPageNo.SetValue( pageNumber );

            var answeredQuestionCount = _assessmentResponses.Where( a => a.Response.HasValue ).Count();
            PercentComplete = Math.Round( ( Convert.ToDecimal( answeredQuestionCount ) / Convert.ToDecimal( _assessmentResponses.Count ) ) * 100.0m, 2 );

            var skipCount = pageNumber * QuestionCount;

            var questions = _assessmentResponses
                .Skip( skipCount )
                .Take( QuestionCount + 1 )
                .ToList();

            rQuestions.DataSource = questions.Take( QuestionCount );
            rQuestions.DataBind();

            // set next button
            if ( questions.Count() > QuestionCount )
            {
                btnNext.Text = "Next";
                btnNext.CommandArgument = "Next";
            }
            else
            {
                btnNext.Text = "Finish";
                btnNext.CommandArgument = "Finish";
            }

            // build prev button
            if ( pageNumber == 0 )
            {
                btnPrevious.Visible = btnPrevious.Enabled = false;
            }
            else
            {
                btnPrevious.Visible = btnPrevious.Enabled = true;
            }
        }

        /// <summary>
        /// Gets the response to the rQuestions repeater control.
        /// </summary>
        private void GetResponse()
        {
            foreach ( var item in rQuestions.Items.OfType<RepeaterItem>() )
            {
                HiddenField hfQuestionCode = item.FindControl( "hfQuestionCode" ) as HiddenField;
                RockRadioButtonList rblQuestion = item.FindControl( "rblQuestion" ) as RockRadioButtonList;
                var assessment = _assessmentResponses.SingleOrDefault( a => a.Code == hfQuestionCode.Value );
                if ( assessment != null )
                {
                    assessment.Response = rblQuestion.SelectedValueAsInt( false );
                }
            }
        }

        #endregion

        #region nested classes

        [Serializable]
        public class AssessmentResponse
        {
            /// <summary>
            /// Gets or sets the code.
            /// </summary>
            /// <value>
            /// The code.
            /// </value>
            public string Code { get; set; }

            /// <summary>
            /// Gets or sets the question.
            /// </summary>
            /// <value>
            /// The question.
            /// </value>
            public string Question { get; set; }

            /// <summary>
            /// Gets or sets the type of the option.
            /// </summary>
            /// <value>
            /// The type of the option.
            /// </value>
            public MotivatorService.OptionType OptionType { get; set; }

            /// <summary>
            /// Gets or sets the response.
            /// </summary>
            /// <value>
            /// The response.
            /// </value>
            public int? Response { get; set; }
        }

        #endregion
    }
}