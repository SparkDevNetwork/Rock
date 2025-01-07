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
    /// Calculates a person's DISC score based on a series of question answers.
    /// This is used with permission from Greg Wiens: http://www.gregwiens.com/scid/
    /// </summary>
    [DisplayName( "DISC" )]
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

    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.DISC )]
    public partial class Disc : Rock.Web.UI.RockBlock
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
            /// The URL encoded key for a person
            /// </summary>
            public const string Person = "Person";
        }

        #endregion PageParameterKeys

        #region Fields

        // used for private variables
        private Person _targetPerson = null;
        private int? _assessmentId = null;
        private bool _isQuerystringPersonKey = false;

        private decimal _percentComplete = 0;

        private List<AssessmentResponse> _assessmentResponses;

        // View State Keys
        private const string ASSESSMENT_STATE = "AssessmentState";
        private const string START_DATETIME = "StartDateTime";

        #endregion

        #region Public and Protected Properties

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

            RockPage.AddCSSLink( "~/Styles/Blocks/Crm/Disc.css" );

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
                    HidePanelsAndShowError( "You must be signed in to take the assessment." );
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
            if ( ( _assessmentResponses.Count > totalQuestion && !_assessmentResponses.All( a => !string.IsNullOrEmpty( a.MostScore ) && !string.IsNullOrEmpty( a.LeastScore ) ) ) || "Next".Equals( commandArgument ) )
            {
                BindRepeater( pageNumber );
            }
            else
            {
                try
                {
                    var moreD = _assessmentResponses.Where( a => a.MostScore == "D" ).Count();
                    var moreI = _assessmentResponses.Where( a => a.MostScore == "I" ).Count();
                    var moreS = _assessmentResponses.Where( a => a.MostScore == "S" ).Count();
                    var moreC = _assessmentResponses.Where( a => a.MostScore == "C" ).Count();
                    var lessD = _assessmentResponses.Where( a => a.LeastScore == "D" ).Count();
                    var lessI = _assessmentResponses.Where( a => a.LeastScore == "I" ).Count();
                    var lessS = _assessmentResponses.Where( a => a.LeastScore == "S" ).Count();
                    var lessC = _assessmentResponses.Where( a => a.LeastScore == "C" ).Count();

                    // Score the responses and return the results
                    DiscService.AssessmentResults results = DiscService.Score( moreD, moreI, moreS, moreC, lessD, lessI, lessS, lessC );

                    // Now save the results for this person
                    DiscService.SaveAssessmentResults(
                        _targetPerson,
                        results.AdaptiveBehaviorD.ToString(),
                        results.AdaptiveBehaviorI.ToString(),
                        results.AdaptiveBehaviorS.ToString(),
                        results.AdaptiveBehaviorC.ToString(),
                        results.NaturalBehaviorD.ToString(),
                        results.NaturalBehaviorI.ToString(),
                        results.NaturalBehaviorS.ToString(),
                        results.NaturalBehaviorC.ToString(),
                        results.PersonalityType );

                    var assessmentData = _assessmentResponses.ToDictionary( a => a.QuestionNumber, b => new { Most = new string[2] { b.MostScore, b.Questions[b.MostScore] }, Least = new string[2] { b.LeastScore, b.Questions[b.LeastScore] } } );
                    var rockContext = new RockContext();

                    var assessmentService = new AssessmentService( rockContext );
                    Assessment assessment = null;

                    if ( hfAssessmentId.ValueAsInt() != 0 )
                    {
                        assessment = assessmentService.Get( int.Parse( hfAssessmentId.Value ) );
                    }

                    if ( assessment == null )
                    {
                        var assessmentType = new AssessmentTypeService( rockContext ).Get( Rock.SystemGuid.AssessmentType.DISC.AsGuid() );
                        assessment = new Assessment()
                        {
                            AssessmentTypeId = assessmentType.Id,
                            PersonAliasId = _targetPerson.PrimaryAliasId.Value
                        };
                        assessmentService.Add( assessment );
                    }

                    assessment.Status = AssessmentRequestStatus.Complete;
                    assessment.CompletedDateTime = RockDateTime.Now;
                    assessment.AssessmentResultData = new { Result = assessmentData, TimeToTake = RockDateTime.Now.Subtract( StartDateTime ).TotalSeconds }.ToJson();
                    rockContext.SaveChanges();

                    ShowResult( results, assessment );
                }
                catch ( Exception ex )
                {
                    nbError.Visible = true;
                    nbError.Title = "We're Sorry...";
                    nbError.Text = "Something went wrong while trying to save your test results.";
                    LogException( ex );
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
        /// Handles the ItemDataBound event of the rQuestions repeater control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rQuestions_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                Literal lQuestion1 = e.Item.FindControl( "lQuestion1" ) as Literal;
                Literal lQuestion2 = e.Item.FindControl( "lQuestion2" ) as Literal;
                Literal lQuestion3 = e.Item.FindControl( "lQuestion3" ) as Literal;
                Literal lQuestion4 = e.Item.FindControl( "lQuestion4" ) as Literal;

                RockRadioButtonList rblMore1 = e.Item.FindControl( "rblMore1" ) as RockRadioButtonList;
                RockRadioButtonList rblLess1 = e.Item.FindControl( "rblLess1" ) as RockRadioButtonList;

                RockRadioButtonList rblMore2 = e.Item.FindControl( "rblMore2" ) as RockRadioButtonList;
                RockRadioButtonList rblLess2 = e.Item.FindControl( "rblLess2" ) as RockRadioButtonList;

                RockRadioButtonList rblMore3 = e.Item.FindControl( "rblMore3" ) as RockRadioButtonList;
                RockRadioButtonList rblLess3 = e.Item.FindControl( "rblLess3" ) as RockRadioButtonList;

                RockRadioButtonList rblMore4 = e.Item.FindControl( "rblMore4" ) as RockRadioButtonList;
                RockRadioButtonList rblLess4 = e.Item.FindControl( "rblLess4" ) as RockRadioButtonList;

                var assessment = ( AssessmentResponse ) e.Item.DataItem;
                ListItem m1 = new ListItem( "<span class='sr-only'>Most</span>", assessment.Questions.Keys.ElementAt( 0 ) );
                ListItem m2 = new ListItem( "<span class='sr-only'>Most</span>", assessment.Questions.Keys.ElementAt( 1 ) );
                ListItem m3 = new ListItem( "<span class='sr-only'>Most</span>", assessment.Questions.Keys.ElementAt( 2 ) );
                ListItem m4 = new ListItem( "<span class='sr-only'>Most</span>", assessment.Questions.Keys.ElementAt( 3 ) );

                ListItem l1 = new ListItem( "<span class='sr-only'>Least</span>", assessment.Questions.Keys.ElementAt( 0 ) );
                ListItem l2 = new ListItem( "<span class='sr-only'>Least</span>", assessment.Questions.Keys.ElementAt( 1 ) );
                ListItem l3 = new ListItem( "<span class='sr-only'>Least</span>", assessment.Questions.Keys.ElementAt( 2 ) );
                ListItem l4 = new ListItem( "<span class='sr-only'>Least</span>", assessment.Questions.Keys.ElementAt( 3 ) );

                lQuestion1.Text = assessment.Questions.Values.ElementAt( 0 );
                rblMore1.Items.Add( m1 );
                rblLess1.Items.Add( l1 );

                lQuestion2.Text = assessment.Questions.Values.ElementAt( 1 );
                rblMore2.Items.Add( m2 );
                rblLess2.Items.Add( l2 );

                lQuestion3.Text = assessment.Questions.Values.ElementAt( 2 );
                rblMore3.Items.Add( m3 );
                rblLess3.Items.Add( l3 );

                lQuestion4.Text = assessment.Questions.Values.ElementAt( 3 );
                rblMore4.Items.Add( m4 );
                rblLess4.Items.Add( l4 );

                SetSelectedValue( assessment.MostScore, rblMore1, rblMore2, rblMore3, rblMore4 );
                SetSelectedValue( assessment.LeastScore, rblLess1, rblLess2, rblLess3, rblLess4 );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbRetake control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRetakeTest_Click( object sender, EventArgs e )
        {
            hfAssessmentId.SetValue( 0 );
            btnRetakeTest.Visible = false;
            ShowInstructions();
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
            var assessmentType = new AssessmentTypeService( rockContext ).Get( Rock.SystemGuid.AssessmentType.DISC.AsGuid() );
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
                DiscService.AssessmentResults savedScores = DiscService.LoadSavedAssessmentResults( _targetPerson );
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
                        DiscService.AssessmentResults savedScores = DiscService.LoadSavedAssessmentResults( _targetPerson );
                        ShowResult( savedScores, previouslyCompletedAssessment, true );
                        return;
                    }

                    // If assessment is pending and the current person is not the one assigned and previouslyCompletedAssessment is null show a message that the test has not been completed.
                    HidePanelsAndShowError( string.Format("{0} has not yet taken the {1} Assessment.", _targetPerson.FullName, assessmentType.Title ) );
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
        /// Gets the selected value from the given radiobuttonlists.
        /// </summary>
        /// <param name="rbl1">The first RadioButtonList.</param>
        /// <param name="rbl2">The second RadioButtonList.</param>
        /// <param name="rbl3">The third RadioButtonList.</param>
        /// <param name="rbl4">The fourth RadioButtonList.</param>
        /// <returns>the value from the first non-empty RadioButtonList</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">One of the RadioButtonList must be selected.</exception>
        private string GetSelectedValue( RadioButtonList rbl1, RadioButtonList rbl2, RadioButtonList rbl3, RadioButtonList rbl4 )
        {
            if ( !string.IsNullOrEmpty( rbl1.SelectedValue ) )
            {
                return rbl1.SelectedValue;
            }
            else if ( !string.IsNullOrEmpty( rbl2.SelectedValue ) )
            {
                return rbl2.SelectedValue;
            }
            else if ( !string.IsNullOrEmpty( rbl3.SelectedValue ) )
            {
                return rbl3.SelectedValue;
            }
            else if ( !string.IsNullOrEmpty( rbl4.SelectedValue ) )
            {
                return rbl4.SelectedValue;
            }

            return string.Empty;
        }

        private void SetSelectedValue( string value, RadioButtonList rbl1, RadioButtonList rbl2, RadioButtonList rbl3, RadioButtonList rbl4 )
        {
            if ( rbl1.Items.FindByValue( value ) != null )
            {
                rbl1.SelectedValue = value;
            }
            else if ( rbl2.Items.FindByValue( value ) != null )
            {
                rbl2.SelectedValue = value;
            }
            else if ( rbl3.Items.FindByValue( value ) != null )
            {
                rbl3.SelectedValue = value;
            }
            else if ( rbl4.Items.FindByValue( value ) != null )
            {
                rbl4.SelectedValue = value;
            }
        }

        /// <summary>
        /// Plots the graphs using the Disc score results.
        /// </summary>
        /// <param name="results">The results.</param>
        private void PlotGraph( DiscService.AssessmentResults results )
        {
            // Plot the Natural graph
            DiscService.PlotOneGraph(
                discNaturalScore_D,
                discNaturalScore_I,
                discNaturalScore_S,
                discNaturalScore_C,
                results.NaturalBehaviorD,
                results.NaturalBehaviorI,
                results.NaturalBehaviorS,
                results.NaturalBehaviorC,
                100 );
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
        /// Shows the results of the assessment test.
        /// </summary>
        /// <param name="savedScores">The saved scores.</param>
        private void ShowResult( DiscService.AssessmentResults savedScores, Assessment assessment, bool isPrevious = false )
        {
            pnlInstructions.Visible = false;
            pnlQuestion.Visible = false;
            pnlResult.Visible = true;

            if ( isPrevious )
            {
                ShowNotification( "A more recent assessment request has been made but has not been taken. Displaying the most recently completed test.", NotificationBoxType.Info );
            }

            if ( CurrentPersonId == _targetPerson.Id )
            {
                lPrintTip.Visible = true;
            }

            lHeading.Text = string.Format( "<div class='disc-heading'><h1>{0}</h1><h4>Personality Type: {1}</h4></div>", _targetPerson.FullName, savedScores.PersonalityType );

            double days = assessment.AssessmentType.MinimumDaysToRetake;

            if ( !_isQuerystringPersonKey && assessment.CompletedDateTime.HasValue && assessment.CompletedDateTime.Value.AddDays( days ) <= RockDateTime.Now )
            {
                btnRetakeTest.Visible = true;
            }

            PlotGraph( savedScores );

            ShowExplaination( savedScores.PersonalityType );
        }

        /// <summary>
        /// Shows the explaination for the given personality type as defined in one of the
        /// DefinedValues of the DISC Results DefinedType.
        /// </summary>
        /// <param name="personalityType">The one or two letter personality type.</param>
        private void ShowExplaination( string personalityType )
        {
            var personalityValue = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.DISC_RESULTS_TYPE.AsGuid() ).DefinedValues.Where( v => v.Value == personalityType ).FirstOrDefault();
            if ( personalityValue != null )
            {
                lDescription.Text = personalityValue.Description;
                lStrengths.Text = personalityValue.GetAttributeValue( AttributeKey.Strengths );
                lChallenges.Text = personalityValue.GetAttributeValue( AttributeKey.Challenges );

                lUnderPressure.Text = personalityValue.GetAttributeValue( AttributeKey.UnderPressure );
                lMotivation.Text = personalityValue.GetAttributeValue( AttributeKey.Motivation );
                lTeamContribution.Text = personalityValue.GetAttributeValue( AttributeKey.TeamContribution );
                lLeadershipStyle.Text = personalityValue.GetAttributeValue( AttributeKey.LeadershipStyle );
                lFollowerStyle.Text = personalityValue.GetAttributeValue( AttributeKey.FollowerStyle );
            }
        }

        /// <summary>
        /// Shows the questions.
        /// </summary>
        private void ShowQuestions()
        {
            pnlInstructions.Visible = false;
            pnlQuestion.Visible = true;

            Random r = new Random();

            _assessmentResponses = DiscService.GetResponses()
                                    .GroupBy( a => a.QuestionNumber )
                                    .Select( a => new AssessmentResponse()
                                    {
                                        QuestionNumber = a.Key,
                                        Questions = a.OrderBy( x => r.Next( 0, 4 ) ).ToDictionary( c => c.MostScore, b => b.ResponseText )
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

            var answeredQuestionCount = _assessmentResponses.Where( a => !string.IsNullOrEmpty( a.MostScore ) && !string.IsNullOrEmpty( a.LeastScore ) ).Count();
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
                RockRadioButtonList rblMore1 = item.FindControl( "rblMore1" ) as RockRadioButtonList;
                RockRadioButtonList rblMore2 = item.FindControl( "rblMore2" ) as RockRadioButtonList;
                RockRadioButtonList rblMore3 = item.FindControl( "rblMore3" ) as RockRadioButtonList;
                RockRadioButtonList rblMore4 = item.FindControl( "rblMore4" ) as RockRadioButtonList;

                RockRadioButtonList rblLess1 = item.FindControl( "rblLess1" ) as RockRadioButtonList;
                RockRadioButtonList rblLess2 = item.FindControl( "rblLess2" ) as RockRadioButtonList;
                RockRadioButtonList rblLess3 = item.FindControl( "rblLess3" ) as RockRadioButtonList;
                RockRadioButtonList rblLess4 = item.FindControl( "rblLess4" ) as RockRadioButtonList;

                var assessment = _assessmentResponses.SingleOrDefault( a => a.QuestionNumber == hfQuestionCode.Value );

                if ( assessment != null )
                {
                    assessment.MostScore = GetSelectedValue( rblMore1, rblMore2, rblMore3, rblMore4 );
                    assessment.LeastScore = GetSelectedValue( rblLess1, rblLess2, rblLess3, rblLess4 );
                }
            }
        }

        #endregion

        #region nested classes

        [Serializable]
        public class AssessmentResponse
        {
            /// <summary>
            /// Gets or sets the question number.
            /// </summary>
            /// <value>
            /// The question number.
            /// </value>
            public string QuestionNumber { get; set; }

            /// <summary>
            /// Gets or sets the questions.
            /// </summary>
            /// <value>
            /// The questions.
            /// </value>
            public Dictionary<string, string> Questions { get; set; }

            /// <summary>
            /// Gets or sets the most score.
            /// </summary>
            /// <value>
            /// The most score.
            /// </value>
            public string MostScore { get; set; }

            /// <summary>
            /// Gets or sets the least score.
            /// </summary>
            /// <value>
            /// The least score.
            /// </value>
            public string LeastScore { get; set; }
        }

        public class AssessmentData
        {
            /// <summary>
            /// Gets or sets the most.
            /// </summary>
            /// <value>
            /// The most.
            /// </value>
            public string Most { get; set; }

            /// <summary>
            /// Gets or sets the least.
            /// </summary>
            /// <value>
            /// The least.
            /// </value>
            public string Least { get; set; }
        }

        #endregion
    }
}