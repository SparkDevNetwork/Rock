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
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
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
    [IntegerField( "Min Days To Retake", "The number of days that must pass before the test can be taken again.", false, 30 )]
    [CodeEditorField( "Instructions", "The text (HTML) to display at the top of the instructions section.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", CodeEditorMode.Html, CodeEditorTheme.Rock, 400, true, @"
            <h2>Welcome!</h2>
            <p>
                {{ Person.NickName }}, in this assessment you are given a series of questions, each containing four phrases.
                Select one phrase that MOST describes you and one phrase that LEAST describes you.
            </p>
            <p>
                This assessment is environmentally sensitive, which means that you may score differently
                in different situations. In other words, you may act differently at home than you
                do on the job. So, as you complete the assessment you should focus on one environment
                for which you are seeking to understand yourself. For instance, if you are trying
                to understand yourself in marriage, you should only think of your responses to situations
                in the context of your marriage. On the other hand, if you want to know your behavioral
                needs on the job, then only think of how you would respond in the job context.
            </p>
            <p>
                One final thought as you give your responses. On these kinds of assessments, it
                is often best and easiest if you respond quickly and do not deliberate too long
                on each question. Your response on one question will not unduly influence your scores,
                so simply answer as quickly as possible and enjoy the process. Don't get too hung
                up, if none of the phrases describe you or if there are some phrases that seem too
                similar, just go with your instinct.
            </p>
            <p>
                When you are ready, click the 'Start' button to proceed.
            </p>
" )]
    [BooleanField( "Always Allow Retakes", "Determines if the retake button should be shown.", false, order: 5 )]
    [IntegerField( "Number of Questions", "The number of questions to show per page while taking the test", true, 5, order: 6 )]
    public partial class Disc : Rock.Web.UI.RockBlock
    {
        #region Fields

        private const string NUMBER_OF_QUESTIONS = "NumberofQuestions";
        // used for private variables
        Person _targetPerson = null;

        private decimal _percentComplete = 0;

        private List<AssessmentResponse> AssessmentResponses;

        // View State Keys
        private const string ASSESSMENT_STATE = "AssessmentState";

        #endregion

        #region Properties

        // used for public / protected properties
        /// <summary>
        /// Gets or sets the percent complete.
        /// </summary>
        /// <value>
        /// The percent complete.
        /// </value>
        public decimal PercentComplete
        {
            get {
                return _percentComplete;
            }

            set {
                _percentComplete = value;
            }
        }

        /// <summary>
        /// Gets or sets the total number of questions
        /// </summary>
        public int QuestionCount
        {
            get { return ViewState[NUMBER_OF_QUESTIONS] as int? ?? 0; }
            set { ViewState[NUMBER_OF_QUESTIONS] = value; }
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

            AssessmentResponses = ViewState[ASSESSMENT_STATE] as List<AssessmentResponse> ?? new List<AssessmentResponse>();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // otherwise use the currently logged in person
            if ( CurrentPerson != null )
            {
                _targetPerson = CurrentPerson;
            }
            else
            {
                nbError.Visible = true;
                pnlInstructions.Visible = false;
                pnlQuestions.Visible = false;
                pnlResults.Visible = false;
            }

            if ( _targetPerson != null )
            {
                DiscService.AssessmentResults savedScores = DiscService.LoadSavedAssessmentResults( _targetPerson );

                if ( savedScores.LastSaveDate <= DateTime.MinValue || !string.IsNullOrWhiteSpace( PageParameter( "RetakeDisc" ) ) )
                {
                    ShowInstructions();
                }
                else
                {
                    ShowResults( savedScores );
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState[ASSESSMENT_STATE] = AssessmentResponses;

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
            if ( ( AssessmentResponses.Count > totalQuestion && !AssessmentResponses.All( a => !string.IsNullOrEmpty( a.MostScore ) && !string.IsNullOrEmpty( a.LeastScore ) ) ) || "Next".Equals( commandArgument ) )
            {
                BindRepeater( pageNumber );
            }
            else
            {
                try
                {
                    var moreD = AssessmentResponses.Where( a => a.MostScore == "D" ).Count();
                    var moreI = AssessmentResponses.Where( a => a.MostScore == "I" ).Count();
                    var moreS = AssessmentResponses.Where( a => a.MostScore == "S" ).Count();
                    var moreC = AssessmentResponses.Where( a => a.MostScore == "C" ).Count();
                    var lessD = AssessmentResponses.Where( a => a.LeastScore == "D" ).Count();
                    var lessI = AssessmentResponses.Where( a => a.LeastScore == "I" ).Count();
                    var lessS = AssessmentResponses.Where( a => a.LeastScore == "S" ).Count();
                    var lessC = AssessmentResponses.Where( a => a.LeastScore == "C" ).Count();
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
                        results.PersonalityType
                    );
                    ShowResults( results );
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

                var assessment = ( ( AssessmentResponse ) ( e.Item.DataItem ) );
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
            btnRetakeTest.Visible = false;
            ShowInstructions();
        }

        #endregion

        #region Methods

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
            {
                return string.Empty;
            }
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
            DiscService.PlotOneGraph( discNaturalScore_D, discNaturalScore_I, discNaturalScore_S, discNaturalScore_C,
                results.NaturalBehaviorD, results.NaturalBehaviorI, results.NaturalBehaviorS, results.NaturalBehaviorC, 100 );
        }

        /// <summary>
        /// Shows the instructions.
        /// </summary>
        private void ShowInstructions()
        {
            pnlInstructions.Visible = true;
            pnlQuestions.Visible = false;
            pnlResults.Visible = false;

            // Resolve the text field merge fields
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, _targetPerson );
            if ( _targetPerson != null )
            {
                mergeFields.Add( "Person", _targetPerson );
            }
            lInstructions.Text = GetAttributeValue( "Instructions" ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Shows the results of the assessment test.
        /// </summary>
        /// <param name="savedScores">The saved scores.</param>
        private void ShowResults( DiscService.AssessmentResults savedScores )
        {
            pnlInstructions.Visible = false;
            pnlQuestions.Visible = false;
            pnlResults.Visible = true;

            if ( CurrentPersonId == _targetPerson.Id )
            {
                lPrintTip.Visible = true;
            }
            lHeading.Text = string.Format( "<div class='disc-heading'><h1>{0}</h1><h4>Personality Type: {1}</h4></div>", _targetPerson.FullName, savedScores.PersonalityType );

            // Show re-take test button if MinDaysToRetake has passed...
            double days = GetAttributeValue( "MinDaysToRetake" ).AsDouble();
            if ( ( savedScores.LastSaveDate.AddDays( days ) <= RockDateTime.Now ) || GetAttributeValue( "AlwaysAllowRetakes" ).AsBoolean() )
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
                lStrengths.Text = personalityValue.GetAttributeValue( "Strengths" );
                lChallenges.Text = personalityValue.GetAttributeValue( "Challenges" );
            }
        }

        /// <summary>
        /// Shows the questions.
        /// </summary>
        private void ShowQuestions()
        {
            pnlInstructions.Visible = false;
            pnlQuestions.Visible = true;

            Random r = new Random();

            AssessmentResponses = DiscService.GetResponses()
                                    .GroupBy( a => a.QuestionNumber )
                                    .Select( a => new AssessmentResponse()
                                    {
                                        QuestionNumber = a.Key,
                                        Questions = a.OrderBy( x => r.Next( 0, 4 ) ).ToDictionary( c => c.MostScore, b => b.ResponseText )
                                    } ).ToList();

            // If _maxQuestions has not been set yet...
            if ( QuestionCount == 0 && AssessmentResponses != null )
            {
                // Set the max number of questions to be no greater than the actual number of questions.
                int numQuestions = this.GetAttributeValue( NUMBER_OF_QUESTIONS ).AsInteger();
                QuestionCount = ( numQuestions > AssessmentResponses.Count ) ? AssessmentResponses.Count : numQuestions;
            }

            BindRepeater( 0 );
        }


        /// <summary>
        /// Binds the question data to the rQuestions repeater control.
        /// </summary>
        private void BindRepeater( int pageNumber )
        {
            hfPageNo.SetValue( pageNumber );

            var answeredQuestionCount = AssessmentResponses.Where( a => !string.IsNullOrEmpty( a.MostScore ) && !string.IsNullOrEmpty( a.LeastScore ) ).Count();
            PercentComplete = Math.Round( ( Convert.ToDecimal( answeredQuestionCount ) / Convert.ToDecimal( AssessmentResponses.Count ) ) * 100.0m, 2 );

            var skipCount = pageNumber * QuestionCount;

            var questions = AssessmentResponses
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

                var assessment = AssessmentResponses.SingleOrDefault( a => a.QuestionNumber == hfQuestionCode.Value );

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
            public string QuestionNumber { get; set; }
            public Dictionary<string, string> Questions { get; set; }
            public string MostScore { get; set; }
            public string LeastScore { get; set; }
        }

        #endregion
    }
}