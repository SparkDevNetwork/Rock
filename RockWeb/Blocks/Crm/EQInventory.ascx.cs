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
    /// Calculates a person's EQ Inventory assessment score based on a series of question answers.
    /// </summary>
    [DisplayName( "EQ Assessment" )]
    [Category( "CRM" )]
    [Description( "Allows you to take a EQ Inventory test and saves your EQ Inventory score." )]

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
        DefaultValue = "EQ Inventory Assessment",
        Order = 2 )]

    [TextField( "Set Page Icon",
        Key = AttributeKeys.SetPageIcon,
        Description = "The css class name to use for the heading icon.",
        IsRequired = false,
        DefaultValue = "fa fa-theater-masks",
        Order = 3 )]

    [IntegerField( "Number of Questions",
        Key = AttributeKeys.NumberofQuestions,
        Description = "The number of questions to show per page while taking the test",
        IsRequired = true,
        DefaultIntegerValue = 7,
        Order = 4 )]

    [BooleanField( "Allow Retakes",
        Key = AttributeKeys.AllowRetakes,
        Description = "If enabled, the person can retake the test after the minimum days passes.",
        DefaultBooleanValue = true,
        Order = 5 )]
    #endregion Block Attributes
    public partial class EQInventory : Rock.Web.UI.RockBlock
    {
        #region Attribute Default values

        private const string InstructionsDefaultValue = @"
<h2>Welcome to the EQ Inventory Assessment</h2>
<p>
    {{ Person.NickName }}, this assessment was developed and researched by Dr. Gregory A. Wiens.
</p>
<p>
 Our TrueWiring Emotional Intelligence Inventory (EQ-W) assesses your developed skills in two domains:
   <ol>
      <li> understanding your own emotions </li>
      <li> understanding the emotions of others. This instrument identifies your ability to appropriately express your emotions while encouraging others to do the same. </li>
   </ol>
</p>
<p>
    Before you begin, please take a moment and pray that the Holy Spirit would guide your thoughts,
    calm your mind, and help you respond to each item as honestly as you can. Don't spend much time
    on each item. Your first instinct is probably your best response.
</p>";

        private const string ResultsMessageDefaultValue = @"

{%- assign chartColor = '#5c8ae7' -%}
{%- assign chartHeight = '100px' -%}
{%- assign includeFurtherReading = true -%}

{%- assign includeFurtherReading = includeFurtherReading | AsBoolean -%}


<h1 class='text-center'>EQ Inventory Assessment Results</h1>

<h3 id='eq-SelfAwareness'>Self Awareness</h3>
<p>
    Self Awareness is being aware of what emotions you are experiencing and why you
    are experiencing these emotions. This skill is demonstrated in real time. In other
    words, when you are in the midst of a discussion or even a disagreement with someone
    else, ask yourself these questions:
    <ul>
        <li>Are you aware of what emotions you are experiencing?</li>
        <li>Are you aware of why you are experiencing these emotions?</li>
    </ul>

    More than just knowing you are angry, our goal is to understand what has caused the
    anger, such as frustration, hurt, pain, confusion, etc.
</p>

<!-- Graph -->
{[ chart type:'horizontalBar' legendshow:'false' tooltipshow:'false' chartheight:'{{chartHeight}}' xaxistype:'linearhorizontal0to100' ]}
    [[ dataitem label:'Self Awareness' value:'{{SelfAwareness}}' fillcolor:'{{chartColor}}' ]] [[ enddataitem ]]
{[ endchart ]}

<blockquote>
    Your responses to the items on the Self Awareness scale indicate the score for the
    ability to be aware of your own emotions is equal to or better than {{ SelfAwareness }}%
    of those who completed this instrument.
</blockquote>

<h3 id='eq-selfregulating'>Self Regulating</h3>
<p>
    Self Regulating is appropriately expressing your emotions in the context of relationships
    around you. Don’t confuse this with learning to suppress your emotions; rather, think of Self
    Regulating as the ability to express your emotions appropriately. Healthy human beings
    experience a full range of emotions and these are important for family, friends, and
    co-workers to understand. Self Regulating is learning to tell others what you are
    feeling in the moment.
</p>

    {[ chart type:'horizontalBar' legendshow:'false' tooltipshow:'false' chartheight:'{{chartHeight}}' xaxistype:'linearhorizontal0to100' ]}
        [[ dataitem label:'Self Regulating' value:'{{SelfRegulating}}' fillcolor:'{{chartColor}}']] [[ enddataitem ]]
    {[ endchart ]}

<blockquote>
    Your responses to the items on the Self Regulation scale indicate the score for the
    ability to appropriately express your own emotions is equal to or better than {{ SelfRegulating }}%
    of those who completed this instrument.
</blockquote>


<h3 id='eq-othersawareness'>Others Awareness</h3>
<p>
    Others Awareness is being aware of what emotions others are experiencing around you and
    why they are experiencing these emotions. As with understanding your own emotions, this
    skill is knowing in real time what another is experiencing. This skill involves reading
    cues to their emotional state through their eyes, facial expressions, body posture, the
    tone of voice or many other ways. It is critical you learn to pay attention to these
    cues for you to enhance your awareness of others' emotions.
</p>

    {[ chart type:'horizontalBar' legendshow:'false' tooltipshow:'false' chartheight:'{{chartHeight}}' xaxistype:'linearhorizontal0to100' ]}
        [[ dataitem label:'Others Awareness' value:'{{OthersAwareness}}' fillcolor:'{{chartColor}}' ]] [[ enddataitem ]]
    {[ endchart ]}

<blockquote>
    Your responses to the items on the Others Awareness scale indicate the score for the
    ability to be aware of others emotions is equal to or better than {{ OthersAwareness }}%
    of those who completed this instrument.
</blockquote>


<h3 id='eq-othersregulating'>Others Regulating</h3>
<p>
    Others Regulating is helping those around you express their emotions appropriately
    in the context of your relationship with them. This skill centers on helping others
    know what emotions they are experiencing and then asking questions or giving permission
    to them to freely and appropriately express their emotions in the context of your relationship.
</p>

    {[ chart type:'horizontalBar' legendshow:'false' tooltipshow:'false' chartheight:'{{chartHeight}}' xaxistype:'linearhorizontal0to100' ]}
        [[ dataitem label:'Others Regulating' value:'{{OthersRegulating}}' fillcolor:'{{chartColor}}' ]] [[ enddataitem ]]
    {[ endchart ]}

<blockquote>
    Your responses to the items on the Others Regulation scale indicate the score for
    the ability to enable others to appropriately express their emotions in the context
    of your relationship is equal to or better than {{OthersRegulating}}% of those who
    completed this instrument.
</blockquote>


<h3 id='eq-additionalscales'>Additional Scales</h3>
<p>
    The EQ*i includes two additional scales which are particularly useful for those in
    leadership roles: 1) EQ in Problem Solving and 2) EQ under stress. Frequently we
    find it difficult to appreciate that conflicting emotions exacerbate most problems
    we experience in life. The solution, therefore, must account for these emotions, not
    just logic or doing the “right” thing.
</p>

<h3 id='eq-problemsolving'>EQ in Problem Solving</h3>
<p>
    The EQ in Problem Solving identifies how proficient you are in using emotions to
    solve problems. This skill requires first being aware of what emotions are involved
    in the problem and what is the source of those emotions. It also includes helping
    others (and yourself) express those emotions within the discussion.
</p>

    {[ chart type:'horizontalBar' legendshow:'false' tooltipshow:'false' chartheight:'{{chartHeight}}' xaxistype:'linearhorizontal0to100' ]}
        [[ dataitem label:'EQ in Problem Solving' value:'{{EQinProblemSolving}}' fillcolor:'{{chartColor}}' ]] [[ enddataitem ]]
    {[ endchart ]}

<blockquote>
    Your responses to the items on the EQ in Problem Solving scale indicate the score for
    the ability to use emotions in resolving problems is equal to or better than {{ EQinProblemSolving }}%
    of those who completed this instrument.
</blockquote>


<h3 id='eq-understress'>EQ Under Stress</h3>
<p>
    It is more difficult to maintain high EQ under high stress than at any other time,
    so EQ Under Stress identifies how capable you are to keep high EQ under high-stress
    moments. This skill requires highly developed Self and Others awareness to understand
    how the stress is impacting yourself and others. It also involves being able to
    articulate the appropriate emotions under pressure which may be different from
    articulating them when not under stress.
</p>

    {[ chart type:'horizontalBar' legendshow:'false' tooltipshow:'false' chartheight:'{{chartHeight}}' xaxistype:'linearhorizontal0to100' ]}
        [[ dataitem label:'EQ Under Stress' value:'{{EQUnderStress}}' fillcolor:'{{chartColor}}' ]] [[ enddataitem ]]
    {[ endchart ]}

<blockquote>
    Your responses to the items on the EQ in Under Stress scale indicate the score
    for the ability to maintain EQ under significant stress is equal to or better than {{ EQUnderStress }}%
    of those who completed this instrument.
</blockquote>

<h3 id='eq-beforeyougo'>Before You Go</h3>

<p>
Your scores on each of these scales are simply a reflection on the skill sets you have
developed in each of these areas. You should not take pride or feel shame for any of the
scores. Instead, we should each commit to working
on our lowest scores or that which is most troublesome. Take a look at the list below for a number of resources which are
helpful as a place to begin</p>

<p>However, as we said in the introduction to this instrument, EQ is developed through
intentional effort, coaching and practice. The remarkable aspect of EQ is that research has
shown that individuals well into their 90s have the capacity to increase their EQ in any of
the six scales.</p>

<p>We have personally seen individuals raise their EQ significantly in a few short years.
You are not destined to retain your current EQ, but it is simply a launching point for where you can
go from this point forward.</p>

{% if includeFurtherReading %}
<h3>Further Reading</h3>

<p>Developing your EQ is like riding a bike. While it may be helpful to read a book on bike riding, it is even more useful to watch videos. But, the only way to learn to ride a bike is to DO IT! You must develop the kinesthetic sense to feel the balance by your senses, coordinated by your nerves and implemented by many muscles. The same is true with EQ--it is intentionally developed, seen in others, coached in real time, and practiced in relationships.</p>
<p>Here are a few books which we believe are great resources for developing your EQ:</p>

<ul>
<li><a href='https://www.goodreads.com/book/show/151881.A_Failure_of_Nerve'>A Failure of Nerve: Leadership in the Age of the Quick Fix</a>, by Edwin Friedman</li>
<li><a href='https://www.goodreads.com/book/show/336913.Coaching_for_Emotional_Intelligence'>Coaching for Emotional Intelligence: The Secret to Developing the Star Potential in Your Employees</a>, by Bob Wall</li>
<li><a href='https://www.goodreads.com/book/show/15014.Crucial_Conversations'>Crucial Conversations: Tools for Talking When Stakes Are High</a> by Kerry Patterson, Joseph Grenny, Ron McMillan, Al Switzler, Stephen R. Covey (Foreword)</li>
<li><a href='https://www.goodreads.com/book/show/6788858-emotional-intelligence-for-dummies'>Emotional Intelligence for Dummies</a> by Steven J. Stein</li>
<li><a href='https://www.goodreads.com/book/show/180463.Leadership_and_Self_Deception'>Leadership and Self-Deception: Getting Out of the Box</a> by The Arbinger Group</li>
<li><a href='https://www.goodreads.com/book/show/163106.Primal_Leadership'>Primal Leadership: Realizing the Power of Emotional Intelligence</a> by Daniel Goleman</li>
</ul>
{% endif %}";

        #endregion Attribute Default values

        #region Attribute Keys
        protected static class AttributeKeys
        {
            public const string NumberofQuestions = "NumberofQuestions";
            public const string Instructions = "Instructions";
            public const string SetPageTitle = "SetPageTitle";
            public const string SetPageIcon = "SetPageIcon";
            public const string ResultsMessage = "ResultsMessage";
            public const string AllowRetakes = "AllowRetakes";
        }

        #endregion Attribute Keys

        #region PageParameterKeys
        /// <summary>
        /// A defined list of page parameter keys used by this block.
        /// </summary>
        protected static class PageParameterKey
        {
            /// <summary>
            /// The person identifier. Use this to get a person's EQ results.
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

        #region Fields

        private Dictionary<int, string> _negativeOption = new Dictionary<int, string>
        {
            { 5, "Never" },
            { 4, "Rarely" },
            { 3, "Sometimes" },
            { 2, "Usually" },
            { 1, "Always" }
        };

        private Dictionary<int, string> _positiveOption = new Dictionary<int, string>
        {
            { 1, "Never" },
            { 2, "Rarely" },
            { 3, "Sometimes" },
            { 4, "Usually" },
            { 5, "Always" }
        };

        // View State Keys
        private const string ASSESSMENT_STATE = "AssessmentState";

        // View State Variables
        private List<AssessmentResponse> _assessmentResponses;

        // used for private variables
        private Person _targetPerson = null;
        private int? _assessmentId = null;
        private bool _isQuerystringPersonKey = false;

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
            get { return ViewState[AttributeKeys.NumberofQuestions] as int? ?? 0; }
            set { ViewState[AttributeKeys.NumberofQuestions] = value; }
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
            int? personId = PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();

            // set the target person according to the parameter or use Current user if not provided.
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
                var assessmentType = new AssessmentTypeService( rockContext ).Get( Rock.SystemGuid.AssessmentType.EQ.AsGuid() );
                Assessment assessment = null;

                if ( _targetPerson != null )
                {
                    var primaryAliasId = _targetPerson.PrimaryAliasId;
                    assessment = new AssessmentService( rockContext )
                        .Queryable()
                        .Where( a => ( _assessmentId.HasValue && a.Id == _assessmentId ) || ( a.PersonAliasId == primaryAliasId && a.AssessmentTypeId == assessmentType.Id ) )
                        .OrderByDescending( a => a.CreatedDateTime )
                        .FirstOrDefault();

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
                        EQInventoryService.AssessmentResults savedScores = EQInventoryService.LoadSavedAssessmentResults( _targetPerson );
                        ShowResult( savedScores, assessment );
                    }
                    else if ( ( assessment == null && !assessmentType.RequiresRequest ) || ( assessment != null && assessment.Status == AssessmentRequestStatus.Pending ) )
                    {
                        if ( _targetPerson.Id != CurrentPerson.Id )
                        {
                            // If the current person is not the target person and there are no results to show then show a not taken message.
                            HidePanelsAndShowError( string.Format("{0} does not have results for the EQ Inventory Assessment.", _targetPerson.FullName ) );
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
                EQInventoryService.AssessmentResults result = EQInventoryService.GetResult( _assessmentResponses.ToDictionary( a => a.Code, b => b.Response.Value ) );
                EQInventoryService.SaveAssessmentResults( _targetPerson, result );
                var rockContext = new RockContext();

                var assessmentService = new AssessmentService( rockContext );
                Assessment assessment = null;

                if ( hfAssessmentId.ValueAsInt() != 0 )
                {
                    assessment = assessmentService.Get( int.Parse( hfAssessmentId.Value ) );
                }

                if ( assessment == null )
                {
                    var assessmentType = new AssessmentTypeService( rockContext ).Get( Rock.SystemGuid.AssessmentType.EQ.AsGuid() );
                    assessment = new Assessment()
                    {
                        AssessmentTypeId = assessmentType.Id,
                        PersonAliasId = _targetPerson.PrimaryAliasId.Value
                    };
                    assessmentService.Add( assessment );
                }

                assessment.Status = AssessmentRequestStatus.Complete;
                assessment.CompletedDateTime = RockDateTime.Now;
                assessment.AssessmentResultData = result.AssessmentData.ToJson();
                rockContext.SaveChanges();

                // Since we are rendering chart.js we have to register the script or reload the page.
                this.NavigateToCurrentPageReference();
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

            if ( assessmentResponseRow.Code.EndsWith( "N" ) )
            {
                rblQuestion.DataSource = _negativeOption;
            }
            else
            {
                rblQuestion.DataSource = _positiveOption;
            }

            rblQuestion.DataTextField = "Value";
            rblQuestion.DataValueField = "Key";
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
        private void ShowResult( EQInventoryService.AssessmentResults result, Assessment assessment )
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
                mergeFields.Add( "SelfAwareness", result.SelfAwareConstruct );
                mergeFields.Add( "SelfRegulating", result.SelfRegulatingConstruct );
                mergeFields.Add( "OthersAwareness", result.OtherAwarenessContruct );
                mergeFields.Add( "OthersRegulating", result.OthersRegulatingConstruct );
                mergeFields.Add( "EQinProblemSolving", result.EQ_ProblemSolvingScale );
                mergeFields.Add( "EQUnderStress", result.EQ_UnderStressScale );
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
            _assessmentResponses = EQInventoryService.GetQuestions()
                                    .Select( a => new AssessmentResponse()
                                    {
                                        Code = a.Key,
                                        Question = a.Value
                                    } ).ToList();

            // If _maxQuestions has not been set yet...
            if ( QuestionCount == 0 && _assessmentResponses != null )
            {
                // Set the max number of questions to be no greater than the actual number of questions.
                int numQuestions = this.GetAttributeValue( AttributeKeys.NumberofQuestions ).AsInteger();
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