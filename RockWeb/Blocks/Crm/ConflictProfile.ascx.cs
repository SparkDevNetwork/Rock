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
    [DisplayName( "Conflict Profile" )]
    [Category( "CRM" )]
    [Description( "Allows you to take a conflict profile test and saves your conflict profile score." )]

    #region Block Attributes
    [CodeEditorField( "Instructions",
        Key = AttributeKeys.Instructions,
        Description = "The text (HTML) to display at the top of the instructions section.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = InstructionsDefaultValue,
        Order = 0 )]

    [CodeEditorField( "Results Message",
        Key = AttributeKeys.ResultsMessage,
        Description = "The text (HTML) to display at the top of the results section.<span class='tip tip-lava'></span><span class='tip tip-html'></span>",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = ResultsMessageDefaultValue,
        Order = 1 )]

    [TextField( "Set Page Title",
        Key = AttributeKeys.SetPageTitle,
        Description = "The text to display as the heading.",
        IsRequired = false,
        DefaultValue = "Conflict Profile",
        Order = 2 )]

    [TextField( "Set Page Icon",
        Key = AttributeKeys.SetPageIcon,
        Description = "The css class name to use for the heading icon.",
        IsRequired = false,
        DefaultValue = "fa fa-gift",
        Order = 3 )]

    [IntegerField(
        "Number of Questions",
        Key = AttributeKeys.NumberOfQuestions,
        Description = "The number of questions to show per page while taking the test",
        IsRequired = true,
        DefaultIntegerValue = 7,
        Order = 4 )]

    [BooleanField(
        "Allow Retakes",
        Key = AttributeKeys.AllowRetakes,
        Description = "If enabled, the person can retake the test after the minimum days passes.",
        DefaultBooleanValue = true,
        Order = 5 )]
    #endregion Block Attributes
    public partial class ConflictProfile : Rock.Web.UI.RockBlock
    {
        #region AttributeDefaultValues
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

        private const string ResultsMessageDefaultValue = @"<h2>Conflict Engagement Profile Results</h2>
<p>
   {{ Person.NickName }}, here are your conflict engagement results.
   You will rank high, medium or low in each of the following five modes.
</p>

{[ chart type:'bar' ]}
    [[ dataitem label:'Winning' value:'{{Winning}}' fillcolor:'#E15759' ]] [[ enddataitem ]]
    [[ dataitem label:'Resolving' value:'{{Resolving}}' fillcolor:'#5585B7' ]] [[ enddataitem ]]
    [[ dataitem label:'Compromising' value:'{{Compromising}}' fillcolor:'#6399D1' ]] [[ enddataitem ]]
    [[ dataitem label:'Avoiding' value:'{{Avoiding}}' fillcolor:'#94DB84' ]] [[ enddataitem ]]
    [[ dataitem label:'Yielding' value:'{{Yielding}}' fillcolor:'#A1ED90' ]] [[ enddataitem ]]
{[ endchart ]}

<h3>Conflict Engagement Modes</h3>

<h4>Winning</h4>
<p>
    Winning means you prefer competing over cooperating. You believe you have the right answer and you desire to
  prove you are right, whatever it takes. This may include standing up for your own rights, beliefs or position.
</p>

<h4>Resolving</h4>
<p>
    Resolving means you attempt to work with the other person in depth to find the best solution, regardless of
    who appears to get the most immediate benefit. This involves digging beneath the presenting issue to find a
    solution that offers benefit to both parties and can take more time than other approaches.
</p>

<h4>Compromising</h4>
<p>
    Compromising means you find a middle ground in the conflict. This often involves meeting in the middle or finding
    some mutually agreeable point between both positions. This is useful for quick solutions.
</p>

<h4>Avoiding</h4>
<p>
    Avoiding means not pursuing your own rights or those of the other person. You typically do not address the
    conflict at all, if possible. This may be diplomatically sidestepping an issue or staying away from a
    threatening situation.
</p>

<h4>Yielding</h4>
<p>
    Yielding means neglecting your own interests while giving in to those of the other person. This is
    self-sacrificing and maybe charitable; serving or choosing to obey another when you prefer not to.
</p>

<h3>Conflict Engagement Themes</h3>

<p>Often people find that they have a combined approach and gravitate toward one of the following themes.</p>

{[ chart type:'pie' ]}
    [[ dataitem label:'Solving' value:'{{EngagementProfileSolving}}' fillcolor:'#4E79A7' ]] [[ enddataitem ]]
    [[ dataitem label:'Accommodating' value:'{{EngagementProfileAccommodating}}' fillcolor:'#8CD17D' ]] [[ enddataitem ]]
    [[ dataitem label:'Winning' value:'{{EngagementProfileWinning}}' fillcolor:'#E15759' ]] [[ enddataitem ]]
{[ endchart ]}

<h4>Solving</h4>
<p>
    Solving describes those who seek to use both Resolving and Compromising modes for solving conflict. By combining
    these two modes, they seek to solve problems as a team. Their leadership styles are highly cooperative and
    empowering for the benefit of the entire group.
</p>

<h4>Accommodating</h4>
<p>
    Accommodating combines Avoiding and Yielding modes for solving conflict. They are most effective in roles
    where allowing others to have their way is better for the team, such as support roles or roles where an
    emphasis on the contribution of others is significant.
</p>

<h4>Winning</h4>
<p>
    Winning is not a combination of modes, but a theme that is based entirely on the Winning model alone for
    solving conflict. This theme is important for times when quick decisions need to be made and is helpful
    for roles such as sole-proprietor.
</p>";

        #endregion AttributeDefaultValues

        #region Attribute Keys
        protected static class AttributeKeys
        {
            public const string NumberOfQuestions = "NumberofQuestions";
            public const string Instructions = "Instructions";
            public const string SetPageTitle = "SetPageTitle";
            public const string SetPageIcon = "SetPageIcon";
            public const string ResultsMessage = "ResultsMessage";
            public const string AllowRetakes = "AllowRetakes";
        }
        #endregion Attribute keys

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

        #region PageParameterKeys
        /// <summary>
        /// A defined list of page parameter keys used by this block.
        /// </summary>
        protected static class PageParameterKey
        {
            /// <summary>
            /// The person identifier. Use this to get a person's Conflict Profile results.
            /// </summary>
            public const string PersonId = "PersonId";

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
            get
            {
                return ViewState[AttributeKeys.NumberOfQuestions] as int? ?? 0;
            }

            set
            {
                ViewState[AttributeKeys.NumberOfQuestions] = value;
            }
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

            // set the target person according to the parameter or use Current user if not provided.
            string personKey = PageParameter( PageParameterKey.Person );
            int? personId = PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();

            if ( personId.HasValue )
            {
                // Try the person ID first.
                _targetPerson = new PersonService( new RockContext() ).Get( personId.Value );
            }
            else if ( personKey.IsNotNullOrWhiteSpace() )
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

            if ( _targetPerson == null )
            {
                if ( _isQuerystringPersonKey || personId.HasValue )
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
                var rockContext = new RockContext();
                var assessmentType = new AssessmentTypeService( rockContext ).Get( Rock.SystemGuid.AssessmentType.CONFLICT.AsGuid() );
                Assessment assessment = null;

                if ( _targetPerson != null )
                {
                    var primaryAliasId = _targetPerson.PrimaryAliasId;

                    if ( _assessmentId == 0 )
                    {
                        // This indicates that the block should create a new assessment instead of looking for an existing one. e.g. a user directed re-take
                        assessment = null;
                    }
                    else
                    {
                        // Look for an existing pending or completed assessment.
                        assessment = new AssessmentService( rockContext )
                            .Queryable()
                            .Where( a => ( _assessmentId.HasValue && a.Id == _assessmentId ) || ( a.PersonAliasId == primaryAliasId && a.AssessmentTypeId == assessmentType.Id ) )
                            .OrderByDescending( a => a.CreatedDateTime )
                            .FirstOrDefault();
                    }

                    if ( assessment != null )
                    {
                        hfAssessmentId.SetValue( assessment.Id );
                    }
                    else
                    {
                        hfAssessmentId.SetValue( 0 );
                    }

                    if ( assessment != null && assessment.Status == AssessmentRequestStatus.Complete )
                    {
                        ConflictProfileService.AssessmentResults savedScores = ConflictProfileService.LoadSavedAssessmentResults( _targetPerson );
                        ShowResult( savedScores, assessment );
                    }
                    else if ( ( assessment == null && !assessmentType.RequiresRequest ) || ( assessment != null && assessment.Status == AssessmentRequestStatus.Pending ) )
                    {
                        if ( _targetPerson.Id != CurrentPerson.Id )
                        {
                            // If the current person is not the target person and there are no results to show then show a not taken message.
                            HidePanelsAndShowError( string.Format("{0} does not have results for the Conflict Profile Assessment.", _targetPerson.FullName ) );
                        }
                        else
                        {
                            ShowInstructions();
                        }
                    }
                    else
                    {
                        HidePanelsAndShowError( "Sorry, this test requires a request from someone before it can be taken." );
                    }
                }
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
                ConflictProfileService.AssessmentResults result = ConflictProfileService.GetResult( _assessmentResponses.ToDictionary( a => a.Code, b => b.Response.Value ) );
                ConflictProfileService.SaveAssessmentResults( _targetPerson, result );
                var rockContext = new RockContext();

                var assessmentService = new AssessmentService( rockContext );
                Assessment assessment = null;

                if ( hfAssessmentId.ValueAsInt() != 0 )
                {
                    assessment = assessmentService.Get( int.Parse( hfAssessmentId.Value ) );
                }

                if ( assessment == null )
                {
                    var assessmentType = new AssessmentTypeService( rockContext ).Get( Rock.SystemGuid.AssessmentType.CONFLICT.AsGuid() );
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

                // Since we are rendering chart.js we have to reload the page.
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
        /// Hides the Instructions and Questions panels and shows the specified error.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        private void HidePanelsAndShowError( string errorMessage )
        {
            pnlInstructions.Visible = false;
            pnlQuestion.Visible = false;
            pnlResult.Visible = false;
            nbError.Visible = true;
            nbError.Text = errorMessage;
        }

        /// <summary>
        /// Sets the page title and icon.
        /// </summary>
        private void SetPanelTitleAndIcon()
        {
            string panelTitle = this.GetAttributeValue( AttributeKeys.SetPageTitle );
            if ( !string.IsNullOrEmpty( panelTitle ) )
            {
                lTitle.Text = panelTitle;
            }

            string panelIcon = this.GetAttributeValue( AttributeKeys.SetPageIcon );
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

            lInstructions.Text = GetAttributeValue( AttributeKeys.Instructions ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Shows the result.
        /// </summary>
        private void ShowResult( ConflictProfileService.AssessmentResults result, Assessment assessment )
        {
            pnlInstructions.Visible = false;
            pnlQuestion.Visible = false;
            pnlResult.Visible = true;

            var allowRetakes = GetAttributeValue( AttributeKeys.AllowRetakes ).AsBoolean();
            var minDays = assessment.AssessmentType.MinimumDaysToRetake;

            if ( !_isQuerystringPersonKey && allowRetakes && assessment.CompletedDateTime.HasValue && assessment.CompletedDateTime.Value.AddDays( minDays ) <= RockDateTime.Now )
            {
                btnRetakeTest.Visible = true;
            }
            else
            {
                btnRetakeTest.Visible = false;
            }

            // Resolve the text field merge fields
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, _targetPerson );
            if ( _targetPerson != null )
            {
                _targetPerson.LoadAttributes();
                mergeFields.Add( "Person", _targetPerson );

                // The five Mode scores
                mergeFields.Add( "Winning", result.ModeWinningScore );
                mergeFields.Add( "Avoiding", result.ModeAvoidingScore );
                mergeFields.Add( "Compromising", result.ModeCompromisingScore );
                mergeFields.Add( "Yielding", result.ModeYieldingScore );
                mergeFields.Add( "Resolving", result.ModeResolvingScore );

                // The optional 'Conflict Engagement Profile' scores:
                mergeFields.Add( "EngagementProfileSolving", result.EngagementSolvingScore );
                mergeFields.Add( "EngagementProfileAccommodating", result.EngagementAccommodatingScore );
                mergeFields.Add( "EngagementProfileWinning", result.EngagementWinningScore );
            }

            lResult.Text = GetAttributeValue( AttributeKeys.ResultsMessage ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Shows the questions.
        /// </summary>
        private void ShowQuestions()
        {
            pnlInstructions.Visible = false;
            pnlQuestion.Visible = true;
            pnlResult.Visible = false;
            _assessmentResponses = ConflictProfileService.GetQuestions()
                                    .Select( a => new AssessmentResponse()
                                    {
                                        Code = a.Key,
                                        Question = a.Value
                                    } ).ToList();

            // If _maxQuestions has not been set yet...
            if ( QuestionCount == 0 && _assessmentResponses != null )
            {
                // Set the max number of questions to be no greater than the actual number of questions.
                int numQuestions = this.GetAttributeValue( AttributeKeys.NumberOfQuestions ).AsInteger();
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
        protected class AssessmentResponse
        {
            public string Code { get; set; }

            public string Question { get; set; }

            public int? Response { get; set; }
        }

        #endregion
    }
}