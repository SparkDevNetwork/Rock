using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Text;
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
using Rock.Communication;
using Rock.Security;

namespace RockWeb.Plugins.com_centralaz.Accountability
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Accountability Report Form" )]
    [Category( "com_centralaz > Accountability" )]
    [Description( "Block for accountability group members to fill out and submit reports" )]
    public partial class AccountabilityReportForm : Rock.Web.UI.RockBlock
    {
        #region Fields

        GroupMember _groupMember = null;

        #endregion

        #region Properties

        public string _emailReportIntroduction = "<p> The following is an Accountability Group Report from your team member. </p>";
        public string _emailReportSubject = "Accountability Group Report";

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
            if ( IsPersonMember( PageParameter( "GroupId" ).AsInteger() ) || IsUserAuthorized( Authorization.EDIT ) )
            {
                ShowQuestions();
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

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                AssignReportDateOptions();
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

        /// <summary>
        /// Handles the OnClick event of the btnSubmit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSubmit_OnClick( object sender, EventArgs e )
        {
            ClearErrorMessage();
            SaveReport();
        }

        /// <summary>
        /// Handles the OnClick event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_OnClick( object sender, EventArgs e )
        {
            Dictionary<string, string> qryString = new Dictionary<string, string>();
            qryString["GroupId"] = PageParameter( "GroupId" );
            NavigateToParentPage( qryString );
        }
        #endregion

        #region Methods

        /// <summary>
        /// Binds the options for report dates to the report date drop down list.
        /// </summary>
        protected void AssignReportDateOptions()
        {
            int groupId = int.Parse( PageParameter( "GroupId" ) );
            DateTime recentReportDate;
            Group group = GetGroup( groupId );
            group.LoadAttributes();
            DateTime reportStartDate = DateTime.Parse( group.GetAttributeValue( "ReportStartDate" ) );
            try
            {
                ResponseSet recentReport = new ResponseSetService( new AccountabilityContext() ).GetMostRecentReport( CurrentPersonId, groupId );
                recentReportDate = recentReport.SubmitForDate;
            }
            catch ( Exception e )
            {
                recentReportDate = reportStartDate;
            }
            DateTime nextDueDate = NextReportDate( reportStartDate );
            DateTime lastDueDate = nextDueDate.AddDays( -7 );
            ResponseSetService responseSetService = new ResponseSetService( new AccountabilityContext() );
            //Submit report for this week case
            if ( !responseSetService.DoesResponseSetExistWithSubmitDate( nextDueDate, CurrentPersonId, groupId ) )
            {
                ddlSubmitForDate.Items.Add( nextDueDate.ToShortDateString() );
            }
            //Report overdue case
            if ( !responseSetService.DoesResponseSetExistWithSubmitDate( lastDueDate, CurrentPersonId, groupId ) )
            {
                ddlSubmitForDate.Items.Add( lastDueDate.ToShortDateString() );
            }
        }

        /// <summary>
        /// Populates the phQuestions placeholder with labels, dropdowns, and textboxes for the report questions.
        /// </summary>
        protected void ShowQuestions()
        {
            int groupId = int.Parse( PageParameter( "GroupId" ) );
            int personId = (int)CurrentPersonId;
            GroupMemberService groupMemberService = new GroupMemberService( new RockContext() );
            _groupMember = groupMemberService.GetByGroupIdAndPersonId( groupId, personId ).FirstOrDefault();
            List<Question> questions = new QuestionService( new AccountabilityContext() ).GetQuestionsFromGroupTypeID( _groupMember.Group.GroupTypeId );
            if ( questions.Count > 0 )
            {
                HtmlGenericControl questionRow = new HtmlGenericControl();
                Literal questionLongFormLiteral = new Literal();
                RockDropDownList responseDropDown = new RockDropDownList();
                RockTextBox responseTextBox = new RockTextBox();
                HtmlGenericControl gridCell = new HtmlGenericControl( "div" );

                //For each question
                for ( int i = 0; i < questions.Count; i++ )
                {
                    questionRow = new HtmlGenericControl( "div" );

                    //The question id
                    HiddenField questionIdHiddenField = new HiddenField();
                    questionIdHiddenField.ID = "hfQuestionId" + i.ToString();
                    questionRow.Controls.Add( questionIdHiddenField );

                    //The question long form
                    questionLongFormLiteral = new Literal();
                    questionLongFormLiteral.ID = "lblquestionLongForm" + i.ToString();
                    questionLongFormLiteral.Text = "<div class='col-md-7'>" + questions[i].LongForm + "</div>";
                    questionRow.Controls.Add( questionLongFormLiteral );

                    //The response yes/no dropdown
                    responseDropDown = new RockDropDownList();
                    responseDropDown.ID = "ddlResponseAnswer" + i.ToString();
                    responseDropDown.Items.Add( "yes" );
                    responseDropDown.Items.Add( "no" );
                    gridCell = new HtmlGenericControl( "div" );
                    gridCell.AddCssClass( "col-md-1" );
                    gridCell.Controls.Add( responseDropDown );
                    questionRow.Controls.Add( gridCell );

                    //The response comment
                    responseTextBox = new RockTextBox();
                    responseTextBox.ID = "txtResponseComment" + i.ToString();
                    responseTextBox.MaxLength = 150;
                    gridCell = new HtmlGenericControl( "div" );
                    gridCell.AddCssClass( "col-md-4" );
                    gridCell.Controls.Add( responseTextBox );
                    questionRow.Controls.Add( gridCell );

                    //And the row to phQuestions
                    phQuestions.Controls.Add( questionRow );
                }
            }
        }

        /// <summary>
        /// Saves the report to the database.
        /// </summary>
        protected void SaveReport()
        {
            if ( !Page.IsValid )
            {
                return;

            }
            AccountabilityContext dataContext = new AccountabilityContext();
            ResponseSetService responseSetService = new ResponseSetService( dataContext );
            ResponseSet myResponseSet = new ResponseSet();
            myResponseSet.PersonId = (int)CurrentPersonId;
            myResponseSet.GroupId = int.Parse( PageParameter( "GroupId" ) );
            myResponseSet.Comment = tbComments.Text;
            myResponseSet.SubmitForDate = DateTime.Parse( ddlSubmitForDate.SelectedValue );
            double correct = 0;

            int groupId = int.Parse( PageParameter( "GroupId" ) );
            int personId = (int)CurrentPersonId;
            GroupMemberService groupMemberService = new GroupMemberService( new RockContext() );
            _groupMember = groupMemberService.GetByGroupIdAndPersonId( groupId, personId ).FirstOrDefault();

            dataContext = new AccountabilityContext();
            ResponseService responseService = new ResponseService( dataContext );
            List<Question> questions = new QuestionService( new AccountabilityContext() ).GetQuestionsFromGroupTypeID( _groupMember.Group.GroupTypeId );
            Response myResponse = null;
            for ( int i = 0; i < questions.Count; i++ )
            {
                myResponse = new Response();
                //myResponse.ResponseSetId = myResponseSet.Id;
                myResponse.QuestionId = questions[i].Id;
                String answerName = "ddlResponseAnswer" + i.ToString();
                Control control = this.FindControl( answerName );
                RockDropDownList questionAnswer = (RockDropDownList)control;
                if ( questionAnswer.SelectedValue == "yes" )
                {
                    myResponse.IsResponseYes = true;
                    correct++;
                }
                else
                {
                    myResponse.IsResponseYes = false;
                }
                String commentText = ( (RockTextBox)this.FindControl( "txtResponseComment" + i.ToString() ) ).Text;
                if ( commentText.Length > 300 )
                {
                    nbErrorMessage.Title = String.Format( "Your comment to '{0}' has exceeded the character limit of 300.", questions[i].ShortForm );
                }
                myResponse.Comment = commentText;
                myResponseSet.Responses.Add( myResponse );

            }
            dataContext = new AccountabilityContext();
            responseSetService = new ResponseSetService( dataContext );
            double score = correct / questions.Count;
            myResponseSet.Score = score;
            responseSetService.Add( myResponseSet );
            if ( !myResponseSet.IsValid )
            {
                nbErrorMessage.Title = "You have exceeded the 4000 character limit on your comment.";
                return;
            }
            else
            {
                dataContext.SaveChanges();
                SendEmail();
                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString["GroupId"] = PageParameter( "GroupId" );
                NavigateToParentPage( qryString );
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

        /// <summary>
        /// Sends an email of the report to all group members
        /// </summary>
        protected void SendEmail()
        {
            Group group = new GroupService( new RockContext() ).Get( int.Parse( PageParameter( "GroupId" ) ) );
            if ( group.Members.Count > 0 )
            {
                string fromAddress = CurrentPerson.Email;
                string subject = string.Format( "{0} for {1} - ", _emailReportSubject, group.Name, CurrentPerson.FullName );
                string body = CreateMessageBody( group );
                foreach ( GroupMember member in group.Members )
                {
                    if ( !string.IsNullOrWhiteSpace( member.Person.Email ) && member.Person.EmailPreference != EmailPreference.DoNotEmail )
                    {
                        Send( member.Person.Email, fromAddress, subject, body, new RockContext() );
                    }
                }
            }
        }

        /// <summary>
        /// Sends an email
        /// </summary>
        /// <param name="recipient">The recipient's email address</param>
        /// <param name="from">The sender's email address</param>
        /// <param name="subject">The subject</param>
        /// <param name="body">The body</param>
        /// <param name="rockContext">The Rock Context</param>
        private void Send( string recipient, string from, string subject, string body, RockContext rockContext )
        {
            var recipients = new List<string>();
            recipients.Add( recipient );

            var mediumData = new Dictionary<string, string>();
            mediumData.Add( "From", from );
            mediumData.Add( "Subject", subject );
            mediumData.Add( "Body", System.Text.RegularExpressions.Regex.Replace( body, @"\[\[\s*UnsubscribeOption\s*\]\]", string.Empty ) );

            var mediumEntity = EntityTypeCache.Read( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid(), rockContext );
            if ( mediumEntity != null )
            {
                var medium = MediumContainer.GetComponent( mediumEntity.Name );
                if ( medium != null && medium.IsActive )
                {
                    var transport = medium.Transport;
                    if ( transport != null && transport.IsActive )
                    {
                        var appRoot = GlobalAttributesCache.Read( rockContext ).GetValue( "InternalApplicationRoot" );
                        transport.Send( mediumData, recipients, appRoot, string.Empty );
                    }
                }
            }
        }

        /// <summary>
        /// Creates the email message body.
        /// </summary>
        /// <param name="group">The group</param>
        /// <returns>Returns a string containing html for the email body</returns>
        protected string CreateMessageBody( Group group )
        {
            StringBuilder body = new StringBuilder();
            body.Append( _emailReportIntroduction );

            // Start the HTML table...
            body.Append( "<table  width='75%' RULES='NONE'  cellpadding='1' cellspacing='3' style='font-family: Tahoma, Arial, Helvetica; font-size: 12px;'>" );

            //Get the  group type's questions
            List<Question> questions = new QuestionService( new AccountabilityContext() ).GetQuestionsFromGroupTypeID( _groupMember.Group.GroupTypeId );
            body.Append( string.Format( "<tr bgcolor='#eeeeee'><th valign='top' align='left' nowrap='nowrap' colspan='5' style='font-size: 14px;'>{0} - {1} {2}<br /></th></tr>", group.Name, CurrentPerson.FirstName, CurrentPerson.LastName ) );


            //Report week
            body.Append( string.Format( "<tr><td><b>Report for date:</b></td><td>{0}</td></tr>", ddlSubmitForDate.SelectedValue ) );

            //Question rows
            for ( int i = 0; i < questions.Count; i++ )
            {
                RockDropDownList dropDown = (RockDropDownList)this.FindControl( "ddlResponseAnswer" + i.ToString() );
                RockTextBox textBox = ( (RockTextBox)this.FindControl( "txtResponseComment" + i.ToString() ) );
                body.Append( string.Format( "<tr><td width='40%'><b>{0}</b></td>", questions[i].ShortForm ) );

                if ( dropDown.SelectedValue == "yes" )
                {
                    body.Append( "<td width='60%'>yes" );
                }
                else
                {
                    body.Append( "<td width='60%'><b>no</b>" );
                }
                if ( textBox.Text.Trim().Length == 0 )
                {
                    body.Append( "</td>" );
                }
                else
                {
                    body.Append( string.Format( " - {0}</td>", textBox.Text ) );
                }
                body.Append( "</tr>" );
            }

            //ResponseSet Comment
            body.Append( string.Format( "<tr>{0}</tr>", tbComments.Text ) );
            body.Append( "</table>" );
            return body.ToString();
        }

        /// <summary>
        /// Returns the next report date for the group.
        /// </summary>
        /// <param name="reportStartDate">The group's report start date</param>
        /// <returns>A DateTime that is the next report due date.</returns>
        protected DateTime NextReportDate( DateTime reportStartDate )
        {
            DateTime today = DateTime.Now;
            DateTime reportDue = today;

            int daysElapsed = ( today.Date - reportStartDate ).Days;
            if ( daysElapsed >= 0 )
            {
                int remainder = daysElapsed % 7;
                if ( remainder != 0 )
                {
                    int daysUntil = 7 - remainder;
                    reportDue = today.AddDays( daysUntil );
                }
            }
            else
            {
                reportDue = today.AddDays( -( daysElapsed ) );
            }
            return reportDue;
        }

        /// <summary>
        /// Clears the error message title and text.
        /// </summary>
        private void ClearErrorMessage()
        {
            nbErrorMessage.Title = string.Empty;
            nbErrorMessage.Text = string.Empty;
        }

        /// <summary>
        /// Gets a group of id groupId
        /// </summary>
        /// <param name="groupId">the id of the group to get</param>
        /// <returns>Returns the group</returns>
        private Group GetGroup( int groupId )
        {
            string key = string.Format( "Group:{0}", groupId );
            Group group = RockPage.GetSharedItem( key ) as Group;
            if ( group == null )
            {
                group = new GroupService( new RockContext() ).Queryable( "GroupType,GroupLocations.Schedules" )
                    .Where( g => g.Id == groupId )
                    .FirstOrDefault();
                RockPage.SaveSharedItem( key, group );
            }

            return group;
        }
        #endregion
    }
}