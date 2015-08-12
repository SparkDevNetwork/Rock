using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using com.centralaz.Accountability.Data;
using com.centralaz.Accountability.Model;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;

namespace RockWeb.Plugins.com_centralaz.Accountability
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Accountability Report List" )]
    [Category( "com_centralaz > Accountability" )]
    [Description( "A list of reports for a certain group member" )]
    public partial class AccountabilityReportList : Rock.Web.UI.RockBlock, Rock.Web.UI.ISecondaryBlock
    {

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gList.GridRebind += gList_GridRebind;
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
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                if ( int.Parse( PageParameter( "GroupMemberId" ) ) == 0 )
                {
                    pnlView.Visible = false;
                }
                else
                {
                    BindGrid();
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Binds the rows of the grid
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gList_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var responseSet = e.Row.DataItem as ResponseSet;
                if ( responseSet != null )
                {
                    Literal lSubmitForDate = e.Row.FindControl( "lSubmitForDate" ) as Literal;
                    if ( lSubmitForDate != null )
                    {
                        lSubmitForDate.Text = string.Format( "Report for week of {0}", responseSet.SubmitForDate.ToShortDateString() );
                    }

                    Literal lScore = e.Row.FindControl( "lScore" ) as Literal;
                    if ( lScore != null )
                    {
                        lScore.Text = responseSet.Score.ToString( "0.00" );
                    }

                    Literal lPercent = e.Row.FindControl( "lPercent" ) as Literal;
                    if ( lPercent != null )
                    {
                        lPercent.Text = responseSet.Score.ToString( "0.0%" );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        /// <summary>
        /// Handles the GridRebind event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRowSelected Event of the gList control
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_ExpandRow( object sender, RowEventArgs e )
        {
            ResponseSet responseSet = new ResponseSetService( new AccountabilityContext() ).Get( e.RowKeyId );
            string rowDetail = CreateModalMessage( responseSet );
            lReportContent.Text = rowDetail;
            mdReport.Show();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            AccountabilityContext accountabilityContext = new AccountabilityContext();
            ResponseSetService responseSetService = new ResponseSetService( accountabilityContext );
            GroupMember groupMember = new GroupMemberService( new RockContext() ).Get( PageParameter( "GroupMemberId" ).AsInteger() );
            // sample query to display a few people
            var qry = responseSetService.Queryable()
                        .Where( p => p.PersonId == groupMember.PersonId &&
                            p.GroupId == groupMember.GroupId )
                        .OrderByDescending( p => p.SubmitForDate )
                        .Take( 100 );

            gList.DataSource = qry.ToList();
            gList.DataBind();
        }

        /// <summary>
        /// Creates the report summary for the modal that appears upon the row selected event
        /// </summary>
        /// <param name="responseSet"> The responseSet the report summary is being written for</param>
        /// <returns>Returns a string containing HTML for the report summary</returns>
        private string CreateModalMessage( ResponseSet responseSet )
        {
            StringBuilder body = new StringBuilder();

            //Get the  group type's questions
            List<Question> questions = new QuestionService( new AccountabilityContext() ).GetQuestionsFromGroupTypeID( responseSet.Group.GroupTypeId );

            //Append the SubmitForDate
            body.Append( string.Format( "<div class='row'><div class='col-md-4'><b>Report for week:</b></div><div class='col-md-4'>{0}</div></div>", responseSet.SubmitForDate.ToShortDateString() ) );

            //Set up an empty response for use in the next section
            Response response = new Response();
            ResponseService responseService = new ResponseService( new AccountabilityContext() );

            //Append the questions and responses
            for ( int i = 0; i < questions.Count; i++ )
            {
                response = responseService.GetResponseForResponseSetAndQuestion( responseSet.Id, questions[i].Id );
                body.Append( "<div class='row'>" );
                body.Append( string.Format( "<div class='col-md-4'><b>{0}:</b></div>", questions[i].ShortForm ) );
                if ( response == null )
                {
                    body.Append( "<div class='col-md-4'><b>N/A</b>" );
                }
                else
                {
                    if ( response.IsResponseYes )
                    {
                        body.Append( "<div class='col-md-4'>yes" );

                    }
                    else
                    {
                        body.Append( "<div class='col-md-4'><b>no</b>" );
                    }
                }
                if ( response == null || response.Comment.Trim().Length == 0 )
                {
                    body.Append( "</div>" );
                }
                else
                {
                    body.Append( string.Format( " - {0}</div>", response.Comment ) );
                }
                body.Append( "</div>" );
            }

            //Append the ResponseSet Comment and return the string
            body.Append( "<div class='row'>" );
            body.Append( string.Format( "<div class='col-md-12'>{0}</div>", responseSet.Comment ) );
            body.Append( "</div>" );
            return body.ToString();
        }

        #endregion

        /// <summary>
        /// Determines the visibility of the block.
        /// </summary>
        /// <param name="visible">The boolean that determines whether the block is visible or not</param>
        public void SetVisible( bool visible )
        {
            pnlView.Visible = visible;
        }
    }
}