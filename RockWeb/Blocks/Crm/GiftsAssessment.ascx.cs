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
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rockweb.Blocks.Crm
{
    /// <summary>
    /// Calculates a person's spiritual gift assessment score based on a series of question answers.
    /// </summary>
    [DisplayName( "Gifts Assessment" )]
    [Category( "CRM" )]
    [Description( "Allows you to take a spiritual gifts test and saves your spiritual gifts score." )]

    [TextField( "Set Page Title", "The text to display as the heading.", false, "Spiritual Gifts Assessment", order: 0 )]
    [TextField( "Set Page Icon", "The css class name to use for the heading icon.", false, "fa fa-gift", order: 1 )]
    [IntegerField( "Number of Questions", "The number of questions to show per page while taking the test", true, 17, order: 2 )]
    [BooleanField( "Allow Retakes", "If enabled, the person can retake the test after the minimum days passes.", true, order: 3 )]
    [IntegerField( "Min Days To Retake", "The number of days that must pass before the test can be taken again.", false, 360, order: 4 )]
    [CodeEditorField( "Instructions", "The text (HTML) to display at the top of the instructions section.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", CodeEditorMode.Html, CodeEditorTheme.Rock, 400, true, @"
<h2>Welcome to Your Spiritual Gifts Assessment</h2>
<p>
    {{ Person.NickName }}, the purpose of this assessment is to help you identify spiritual gifts that are most naturally
    used in the life of the local church. This survey does not include all spiritual gifts, just those that are often
    seen in action for most churches and most people.
</p>
<p>
    In churches it’s not uncommon to see 90% of the work being done by a weary 10%. Why does this happen?
    Partially due to ignorance and partially due to avoidance of spiritual gifts. Here’s the process:
</p>
<ol>
    <li>Discover the primary gifts given to us at our Spiritual birth.</li>
    <li>Learn what these gifts are and what they are not.</li>
    <li>See where these gifts fit into the functioning of the body. </li>
</ol>
<p>
    When you are working within your Spirit-given gifts, you will be most effective for the body
    of Christ in your local setting.
</p>
<p>
    Before you begin, please take a moment and pray that the Holy Spirit would guide your thoughts,
    calm your mind, and help you respond to each item as honestly as you can. Don't spend much time
    on each item. Your first instinct is probably your best response.
</p>" )]
    [CodeEditorField( "Results Message", "The text (HTML) to display at the top of the results section.<span class='tip tip-lava'></span><span class='tip tip-html'></span>", CodeEditorMode.Html, CodeEditorTheme.Rock, 400, true, @"
<div class='row'>
    <div class='col-md-12'>
    <h2 class='h2'> Dominant Gifts</h2>
    </div>
    <div class='col-md-9'>
    <table class='table table-bordered table-responsive'>
    <thead>
        <tr>
            <th>
                Spiritual Gift
            </th>
            <th>
                You are uniquely wired to:
            </th>
        </tr>
    </thead>
    <tbody>
        {% for dominantGift in DominantGifts %}
        <tr>
        <td>
        {{ dominantGift.Value }}
        </td>
        <td>
        {{ dominantGift.Description }}    
        </td>
        </tr>
        {% endfor %}
    </tbody>
    </table>
    </div>
    </div>
    
<div class='row'>
    <div class='col-md-12'>
        <h2 class='h2'> Supportive Gifts</h2>
    </div>
    <div class='col-md-9'>
        <table class='table table-bordered table-responsive'>
            <thead>
                <tr>
                   <th>
                    Spiritual Gift
                    </th>
                    <th>
                    You are uniquely wired to:
                    </th>
                </tr>
            </thead>
            <tbody>
            {% for supportiveGift in SupportiveGifts %}
            <tr>
                <td>
                {{ supportiveGift.Value }}
                </td>
                <td>
                {{ supportiveGift.Description }}
                </td>
            </tr>
                {% endfor %}
            </tbody>
        </table>
    </div>
</div?
<div class='row'>
    <div class='col-md-12'>
        <h2 class='h2'> Other Gifts</h2>
    </div>
    <div class='col-md-9'>
        <table class='table table-bordered table-responsive'>
            <thead>
                <tr>
                   <th>
                    Spiritual Gift
                    </th>
                    <th>
                    You are uniquely wired to:
                    </th>
                </tr>
            </thead>
            <tbody>
                {% for otherGift in OtherGifts %}
            <tr>
                <td>
                {{ otherGift.Value }}
                </td>
                <td>
                    {{ otherGift.Description }}
                </td>
            </tr>
                {% endfor %}
           </tbody>
        </table>
    </div>
</div>
" )]
    public partial class GiftsAssessment : Rock.Web.UI.RockBlock
    {
        #region Fields

        //block attribute keys
        private const string NUMBER_OF_QUESTIONS = "NumberofQuestions";
        private const string INSTRUCTIONS = "Instructions";
        private const string SET_PAGE_TITLE = "SetPageTitle";
        private const string SET_PAGE_ICON = "SetPageIcon";
        private const string RESULTS_MESSAGE = "ResultsMessage";
        private const string ALLOW_RETAKES = "AllowRetakes";
        private const string MIN_DAYS_TO_RETAKE = "MinDaysToRetake";

        // View State Keys
        private const string ASSESSMENT_STATE = "AssessmentState";

        // View State Variables
        private List<AssessmentResponse> AssessmentResponses;

        // used for private variables
        Person _targetPerson = null;
        bool _isQuerystringPersonKey = false;

        // protected variables
        private decimal _percentComplete = 0;

        #endregion

        #region Properties

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

            SetPanelTitleAndIcon();

            // otherwise use the currently logged in person
            string personKey = PageParameter( "Person" );
            if ( !string.IsNullOrEmpty( personKey ) )
            {
                try
                {
                    _targetPerson = new PersonService( new RockContext() ).GetByUrlEncodedKey( personKey );
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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            // Hide notification panels on every postback
            nbError.Visible = false;

            if ( !Page.IsPostBack )
            {
                if ( _targetPerson != null )
                {
                    SpiritualGiftsService.AssessmentResults savedScores = SpiritualGiftsService.LoadSavedAssessmentResults( _targetPerson );

                    if ( !savedScores.LastSaveDate.HasValue && !_isQuerystringPersonKey )
                    {
                        ShowInstructions();
                    }
                    else
                    {
                        ShowResult( savedScores );
                    }
                }
                else
                {
                    pnlInstructions.Visible = false;
                    pnlQuestion.Visible = false;
                    pnlResult.Visible = false;
                    nbError.Visible = true;

                    if ( _isQuerystringPersonKey )
                    {
                        nbError.Text = "There is an issue locating the person associated with the request.";
                    }
                }
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
        /// Handles the Click event of the btnRetakeTest button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRetakeTest_Click( object sender, EventArgs e )
        {
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
            if ( AssessmentResponses.Count > totalQuestion && !AssessmentResponses.All( a => a.Response.HasValue ) || "Next".Equals( commandArgument ) )
            {
                BindRepeater( pageNumber );
            }
            else
            {
                SpiritualGiftsService.AssessmentResults result = SpiritualGiftsService.GetResult( AssessmentResponses.ToDictionary( a => a.Code, b => b.Response.Value ) );
                SpiritualGiftsService.SaveAssessmentResults( _targetPerson, result );
                ShowResult( result );
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
        /// Sets the page title and icon.
        /// </summary>
        private void SetPanelTitleAndIcon()
        {
            string panelTitle = this.GetAttributeValue( SET_PAGE_TITLE );
            if ( !string.IsNullOrEmpty( panelTitle ) )
            {
                lTitle.Text = panelTitle;
            }

            string panelIcon = this.GetAttributeValue( SET_PAGE_ICON );
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
            lInstructions.Text = GetAttributeValue( INSTRUCTIONS ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Shows the result.
        /// </summary>
        private void ShowResult( SpiritualGiftsService.AssessmentResults result )
        {
            pnlInstructions.Visible = false;
            pnlQuestion.Visible = false;
            pnlResult.Visible = true;

            var allowRetakes = GetAttributeValue( ALLOW_RETAKES ).AsBoolean();
            var minDays = GetAttributeValue( MIN_DAYS_TO_RETAKE ).AsInteger();
            if ( !_isQuerystringPersonKey && allowRetakes && result.LastSaveDate.HasValue && result.LastSaveDate.Value.AddDays( minDays ) <= RockDateTime.Now )
            {
                btnRetakeTest.Visible = true;
            }
            else
            {
                btnRetakeTest.Visible = false;
            }
            var spiritualGifts = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.SPIRITUAL_GIFTS );
            // Resolve the text field merge fields
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, _targetPerson );
            if ( _targetPerson != null )
            {
                _targetPerson.LoadAttributes();
                mergeFields.Add( "Person", _targetPerson );
                mergeFields.Add( "DominantGifts", spiritualGifts.DefinedValues.Where( a => result.DominantGifts.Contains( a.Guid ) ).ToList() );
                mergeFields.Add( "SupportiveGifts", spiritualGifts.DefinedValues.Where( a => result.SupportiveGifts.Contains( a.Guid ) ).ToList() );
                mergeFields.Add( "OtherGifts", spiritualGifts.DefinedValues.Where( a => result.OtherGifts.Contains( a.Guid ) ).ToList() );
            }
            lResult.Text = GetAttributeValue( RESULTS_MESSAGE ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Shows the questions.
        /// </summary>
        private void ShowQuestions()
        {
            pnlInstructions.Visible = false;
            pnlQuestion.Visible = true;
            pnlResult.Visible = false;
            AssessmentResponses = SpiritualGiftsService.GetQuestions()
                                    .Select( a => new AssessmentResponse()
                                    {
                                        Code = a.Key,
                                        Question = a.Value
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

            var answeredQuestionCount = AssessmentResponses.Where( a => a.Response.HasValue ).Count();
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
                RockRadioButtonList rblQuestion = item.FindControl( "rblQuestion" ) as RockRadioButtonList;
                var assessment = AssessmentResponses.SingleOrDefault( a => a.Code == hfQuestionCode.Value );
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
            public string Code { get; set; }
            public string Question { get; set; }
            public int? Response { get; set; }
        }

        #endregion
    }
}