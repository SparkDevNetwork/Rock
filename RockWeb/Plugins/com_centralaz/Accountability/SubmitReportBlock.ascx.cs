// <copyright>
// Copyright by Central Christian Church
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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;

using com.centralaz.Accountability.Data;
using com.centralaz.Accountability.Model;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_centralaz.Accountability
{
    [DisplayName( "Submit Report" )]
    [Category( "com_centralaz > Accountability" )]
    [Description( "The Submit Report Block" )]

    [LinkedPage( "Detail Page", "", true, "", "", 0 )]
    public partial class SubmitReportBlock : Rock.Web.UI.RockBlock
    {

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
            }
            GetDatesForMessage();
            base.OnLoad( e );
        }

        #endregion


        #region Internal Methods

        /// <summary>
        /// Handles the OnClick event of the lbSubmit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbSubmit_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "GroupId", PageParameter( "GroupId" ).AsInteger() );
        }

        /// <summary>
        /// Grabs the dates for the due date message and passes them to the WriteDueDateMessage method.
        /// </summary>
        protected void GetDatesForMessage()
        {
            bool canSubmit = IsGroupMember();
            if ( !canSubmit )
            {
                pnlContent.Visible = false;
            }
            else
            {
                var groupId = PageParameter( "GroupId" ).AsIntegerOrNull();
                if ( groupId.HasValue )
                {
                    Group group = GetGroup( groupId.Value );
                    group.LoadAttributes();
                    DateTime reportStartDate = DateTime.Parse( group.GetAttributeValue( "ReportStartDate" ) );
                    DateTime nextDueDate = NextReportDate( reportStartDate );
                    var responseSetService = new ResponseSetService( new AccountabilityContext() );

                    bool isThisWeeksReportSubmitted = responseSetService.Queryable().Where( rs =>
                        rs.SubmitForDate == nextDueDate.Date &&
                        rs.PersonId == CurrentPersonId &&
                        rs.GroupId == groupId.Value )
                        .Any();

                    bool isLastWeeksReportSubmitted = responseSetService.Queryable().Where( rs =>
                        rs.SubmitForDate == nextDueDate.AddDays(-7).Date &&
                        rs.PersonId == CurrentPersonId &&
                        rs.GroupId == groupId.Value )
                        .Any();

                    WriteDueDateMessage( nextDueDate, isThisWeeksReportSubmitted, isLastWeeksReportSubmitted );
                }

            }
        }

        /// <summary>
        /// Populates the lStatesMessage literal with the due date message.
        /// </summary>
        /// <param name="nextDueDate">The next due date</param>
        /// <param name="lastReportDate">The last report date</param>
        protected void WriteDueDateMessage( DateTime nextDueDate, bool isThisWeeksReportSubmitted, bool isLastWeeksReportSubmitted )
        {
            DateTime lastDueDate = nextDueDate.AddDays( -7 );
            int daysUntilDueDate = ( nextDueDate - DateTime.Today ).Days;
            ResponseSetService responseSetService = new ResponseSetService( new AccountabilityContext() );
            //All caught up case
            if ( isThisWeeksReportSubmitted )
            {
                lStatusMessage.Text = "Report Submitted";
                lbSubmitReport.Enabled = false;
            }
            //Submit report for this week case
            if ( daysUntilDueDate < 6 && !isThisWeeksReportSubmitted )
            {
                if ( daysUntilDueDate == 0 )
                {
                    lStatusMessage.Text = "Report due today";
                }
                else if ( daysUntilDueDate == 1 )
                {
                    lStatusMessage.Text = "Report due in 1 day";
                }
                else
                {
                    lStatusMessage.Text = "Report due in " + daysUntilDueDate + " days";
                }
                lbSubmitReport.Enabled = true;
            }

            //Report overdue case
            if ( !isLastWeeksReportSubmitted )
            {
                lStatusMessage.Text = "Report overdue for week of " + nextDueDate.AddDays(-7).ToShortDateString();
                lbSubmitReport.Enabled = true;
            }
        }

        /// <summary>
        /// Returns the next report due date.
        /// </summary>
        /// <param name="reportStartDate">The group's report start date</param>
        /// <returns>The next report due date</returns>
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
        /// Checks if the current person is a group member
        /// </summary>
        /// <returns>A bool of whether the current person is a member or not</returns>
        protected bool IsGroupMember()
        {
            bool isMember = false;

            var groupId = PageParameter( "GroupId" ).AsIntegerOrNull();
            if ( groupId.HasValue )
            {
                var groupMember = new GroupMemberService( new RockContext() ).Queryable()
                            .Where( gm => ( gm.PersonId == CurrentPersonId ) && ( gm.GroupId == groupId ) )
                            .FirstOrDefault();
                if ( groupMember != null )
                {
                    isMember = true;
                }
            }

            return isMember;
        }

        /// <summary>
        /// Returns the group with id groupId.
        /// </summary>
        /// <param name="groupId">the groupId.</param>
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