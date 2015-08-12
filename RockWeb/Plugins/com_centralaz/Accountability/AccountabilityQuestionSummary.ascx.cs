using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using com.centralaz.Accountability.Data;
using com.centralaz.Accountability.Model;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Security;

namespace RockWeb.Plugins.com_centralaz.Accountability
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Accountability Question Summary" )]
    [Category( "com_centralaz > Accountability" )]
    [Description( "Block for accountability group members to view question response stats" )]
    public partial class AccountabilityQuestionSummary : Rock.Web.UI.RockBlock
    {
        #region Fields

        GroupMember _groupMember = null;

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( IsPersonMember( PageParameter( "GroupId" ).AsInteger() ) || IsUserAuthorized( Authorization.EDIT ) )
            {
                base.OnLoad( e );

                if ( !Page.IsPostBack )
                {
                    ShowQuestions();
                }
            }
            else
            {
                if ( CurrentPerson == null )
                {
                    RockPage.Layout.Site.RedirectToLoginPage( true );
                }
                else
                {
                    RockPage.Layout.Site.RedirectToPageNotFoundPage();
                }
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }
        #endregion

        #region Methods

        /// <summary>
        /// Populates the lBlockTitle label and phQuestionSummary placeholder with the question statistics
        /// </summary>
        protected void ShowQuestions()
        {
            lBlockTitle.Text = "Current Statistics through " + DateTime.Today.ToShortDateString();
            int groupId = int.Parse( PageParameter( "GroupId" ) );
            int personId = (int)CurrentPersonId;
            GroupMemberService groupMemberService = new GroupMemberService( new RockContext() );
            _groupMember = groupMemberService.GetByGroupIdAndPersonId( groupId, personId ).FirstOrDefault();
            List<Question> questions = new QuestionService( new AccountabilityContext() ).GetQuestionsFromGroupTypeID( _groupMember.Group.GroupTypeId );
            if ( questions.Count > 0 )
            {
                HtmlGenericControl questionRow = new HtmlGenericControl();
                Literal questionScore = new Literal();
                HtmlGenericControl gridCell = new HtmlGenericControl( "div" );
                double[] responsePercent = new double[2];

                //For each row of two questions
                for ( int i = 0; i < questions.Count; i++ )
                {
                    questionRow = new HtmlGenericControl( "div" );

                    //The question short form
                    questionScore = new Literal();
                    questionScore.ID = "lblquestionShortForm" + i.ToString();
                    questionScore.Text = "<div class='col-md-3'>" + questions[i].ShortForm + "</div>";
                    questionRow.Controls.Add( questionScore );

                    //The question percentage
                    responsePercent = new ResponseService( new AccountabilityContext() ).ResponsePercentage( personId, groupId, questions[i].Id );
                    questionScore = new Literal();
                    questionScore.ID = "lblquestionPercent" + i.ToString();
                    if ( responsePercent[1] == 0 )
                    {
                        questionScore.Text = "<div class='col-md-3'>N/A</div>";
                    }
                    else
                    {
                        questionScore.Text = String.Format( "<div class='col-md-3'>{0:P0} ({1} of {2})</div>", responsePercent[0] / responsePercent[1], responsePercent[0], responsePercent[1] );
                    }
                    questionRow.Controls.Add( questionScore );

                    //check if there is still another question
                    i++;
                    if ( i < questions.Count )
                    {
                        //The second question short form
                        questionScore = new Literal();
                        questionScore.ID = "lblquestionShortForm" + i.ToString();
                        questionScore.Text = "<div class='col-md-3'>" + questions[i].ShortForm + "</div>";
                        questionRow.Controls.Add( questionScore );

                        //The seccond question percentage
                        responsePercent = new ResponseService( new AccountabilityContext() ).ResponsePercentage( personId, groupId, questions[i].Id );
                        questionScore = new Literal();
                        questionScore.ID = "lblquestionPercent" + i.ToString();
                        if ( responsePercent[1] == 0 )
                        {
                            questionScore.Text = "<div class='col-md-3'>N/A</div>";
                        }
                        else
                        {
                            questionScore.Text = String.Format( "<div class='col-md-3'>{0:P0} ({1} of {2})</div>", responsePercent[0] / responsePercent[1], responsePercent[0], responsePercent[1] );
                        }
                        questionRow.Controls.Add( questionScore );
                    }

                    phQuestionSummary.Controls.Add( questionRow );
                }
            }
        }

        /// <summary>
        /// Returns true if the current person is a group member.
        /// </summary>
        /// <param name="groupId">The group Id</param>
        /// <returns>A boolean: true if the person is a member, false if not.</returns>
        protected bool IsPersonMember( int groupId )
        {
            int count = new GroupMemberService( new RockContext() ).Queryable( "GroupTypeRole" )
                .Where( m =>
                    m.PersonId == CurrentPersonId &&
                    m.GroupId == groupId
                    )
                 .Count();
            if ( count == 1 )
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}