using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using com.centralaz.Accountability.Model;
using com.centralaz.Accountability.Data;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.Accountability
{
    [DisplayName( "Accountability Group Member List" )]
    [Category( "com_centralaz > Accountability" )]
    [Description( "Lists all members in the accountability group" )]

    [LinkedPage( "Detail Page", "", true, "", "", 0 )]
    public partial class AccountabilityGroupMemberList : Rock.Web.UI.RockBlock
    {
        #region Private Variables

        private DefinedValueCache _inactiveStatus = null;
        private Group _group = null;
        private List<ResponseSet> _responseSets = null;
        private DateTime _reportStartDate;
        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // if this block has a specific GroupId set, use that, otherwise, determine it from the PageParameters
            int groupId = GetAttributeValue( "Group" ).AsInteger();
            if ( groupId == 0 )
            {
                groupId = PageParameter( "GroupId" ).AsInteger();
            }

            if ( groupId != 0 )
            {
                string key = string.Format( "Group:{0}", groupId );
                _group = RockPage.GetSharedItem( key ) as Group;
                if ( _group == null )
                {
                    _group = new GroupService( new RockContext() ).Queryable( "GroupType" )
                        .Where( g => g.Id == groupId )
                        .FirstOrDefault();
                    RockPage.SaveSharedItem( key, _group );
                }

                if ( _group != null )
                {
                    gGroupMembers.DataKeyNames = new string[] { "Id" };
                    gGroupMembers.CommunicateMergeFields = new List<string> { "GroupRole.Name" };
                    gGroupMembers.PersonIdField = "PersonId";
                    gGroupMembers.RowDataBound += gGroupMembers_RowDataBound;
                    gGroupMembers.Actions.AddClick += gGroupMembers_AddClick;
                    gGroupMembers.Actions.ShowAdd = true;
                    gGroupMembers.IsDeleteEnabled = true;
                    gGroupMembers.GridRebind += gGroupMembers_GridRebind;
                    gGroupMembers.RowItemText = _group.GroupType.GroupTerm + " " + _group.GroupType.GroupMemberTerm;
                    gGroupMembers.ExportFilename = _group.Name;

                    // make sure they have Auth to the block AND Edit to the Group
                    bool canEditBlock = ( IsUserAuthorized( Authorization.EDIT ) && _group.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) ) || IsPersonLeader( groupId );
                    gGroupMembers.Actions.ShowAdd = canEditBlock;
                    gGroupMembers.IsDeleteEnabled = canEditBlock;

                    // Add attribute columns
                    AddAttributeColumns();

                    // Add delete column
                    var deleteField = new DeleteField();
                    gGroupMembers.Columns.Add( deleteField );
                    deleteField.Click += DeleteGroupMember_Click;
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
                int groupId = int.Parse( PageParameter( "GroupId" ) );
                if ( groupId == 0 )
                {
                    SetVisible( false );
                }
                else
                {
                    _responseSets = new ResponseSetService( new AccountabilityContext() ).GetResponseSetsForGroup( groupId );
                    Group group = new GroupService( new RockContext() ).Get( groupId );
                    group.LoadAttributes();
                    _reportStartDate = DateTime.Parse( group.GetAttributeValue( "ReportStartDate" ) );
                    BindGroupMembersGrid();
                }

            }
        }

        #endregion

        #region GroupMembers Grid

        /// <summary>
        /// Handles the RowDataBound event of the gGroupMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        public void gGroupMembers_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var groupMember = e.Row.DataItem as GroupMember;
                if ( groupMember != null && groupMember.Person != null )
                {
                    if ( _inactiveStatus != null &&
                        groupMember.Person.RecordStatusValueId.HasValue &&
                        groupMember.Person.RecordStatusValueId == _inactiveStatus.Id )
                    {
                        e.Row.AddCssClass( "inactive" );
                    }

                    //if ( groupMember.Person.IsDeceased )
                    //{
                    //    e.Row.AddCssClass( "deceased" );
                    //}

                    String[] personInfo = GetPersonInfo( _responseSets, groupMember );

                    Literal lFirstReport = e.Row.FindControl( "lFirstReport" ) as Literal;
                    if ( lFirstReport != null )
                    {
                        lFirstReport.Text = personInfo[0];
                    }

                    Literal lLastReport = e.Row.FindControl( "lLastReport" ) as Literal;
                    if ( lLastReport != null )
                    {
                        lLastReport.Text = personInfo[1];
                    }

                    Literal lWeeksSinceLast = e.Row.FindControl( "lWeeksSinceLast" ) as Literal;
                    if ( lWeeksSinceLast != null )
                    {
                        lWeeksSinceLast.Text = personInfo[2];
                    }

                    Literal lReportsOpportunities = e.Row.FindControl( "lReportsOpportunities" ) as Literal;
                    if ( lReportsOpportunities != null )
                    {
                        lReportsOpportunities.Text = personInfo[3];
                    }

                    Literal lPercentSubmitted = e.Row.FindControl( "lPercentSubmitted" ) as Literal;
                    if ( lPercentSubmitted != null )
                    {
                        lPercentSubmitted.Text = personInfo[4];
                    }
                    Literal lScore = e.Row.FindControl( "lScore" ) as Literal;
                    if ( lScore != null )
                    {
                        lScore.Text = personInfo[5];
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the DeleteGroupMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs" /> instance containing the event data.</param>
        protected void DeleteGroupMember_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            RockContext rockContext = new RockContext();
            GroupMemberService groupMemberService = new GroupMemberService( rockContext );
            GroupMember groupMember = groupMemberService.Get( e.RowKeyId );
            if ( groupMember != null )
            {
                string errorMessage;
                if ( !groupMemberService.CanDelete( groupMember, out errorMessage ) )
                {
                    maGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                int groupId = groupMember.GroupId;

                groupMemberService.Delete( groupMember );
                rockContext.SaveChanges();

                Group group = new GroupService( rockContext ).Get( groupId );
                if ( group.IsSecurityRole || group.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() ) )
                {
                    // person removed from SecurityRole, Flush
                    Rock.Security.Role.Flush( group.Id );
                    Rock.Security.Authorization.Flush();
                }
            }
            _responseSets = new ResponseSetService( new AccountabilityContext() ).GetResponseSetsForGroup( PageParameter( "GroupId" ).AsInteger() );
            BindGroupMembersGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gGroupMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gGroupMembers_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "GroupMemberId", 0, "GroupId", _group.Id );
        }

        /// <summary>
        /// Handles the View event of the gGroupMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroupMembers_View( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "GroupMemberId", e.RowKeyId, "GroupId", _group.Id );
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gGroupMembers_GridRebind( object sender, EventArgs e )
        {
            BindGroupMembersGrid();
        }

        #endregion

        #region Internal Methods


        /// <summary>
        /// Adds the attribute columns.
        /// </summary>
        private void AddAttributeColumns()
        {
            // Remove attribute columns
            foreach ( var column in gGroupMembers.Columns.OfType<AttributeField>().ToList() )
            {
                gGroupMembers.Columns.Remove( column );
            }

            if ( _group != null )
            {
                // Add attribute columns
                int entityTypeId = new GroupMember().TypeId;
                string groupQualifier = _group.Id.ToString();
                string groupTypeQualifier = _group.GroupTypeId.ToString();
                foreach ( var attribute in new AttributeService( new RockContext() ).Queryable()
                    .Where( a =>
                        a.EntityTypeId == entityTypeId &&
                        a.IsGridColumn &&
                        ( ( a.EntityTypeQualifierColumn.Equals( "GroupId", StringComparison.OrdinalIgnoreCase ) && a.EntityTypeQualifierValue.Equals( groupQualifier ) ) ||
                         ( a.EntityTypeQualifierColumn.Equals( "GroupTypeId", StringComparison.OrdinalIgnoreCase ) && a.EntityTypeQualifierValue.Equals( groupTypeQualifier ) ) ) )
                    .OrderByDescending( a => a.EntityTypeQualifierColumn )
                    .ThenBy( a => a.Order )
                    .ThenBy( a => a.Name ) )
                {
                    string dataFieldExpression = attribute.Key;
                    bool columnExists = gGroupMembers.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = dataFieldExpression;
                        boundField.HeaderText = attribute.Name;
                        boundField.SortExpression = string.Empty;

                        var attributeCache = Rock.Web.Cache.AttributeCache.Read( attribute.Id );
                        if ( attributeCache != null )
                        {
                            boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                        }

                        gGroupMembers.Columns.Add( boundField );
                    }
                }
            }
        }

        /// <summary>
        /// Binds the group members grid.
        /// </summary>
        protected void BindGroupMembersGrid()
        {
            if ( _group != null )
            {
                pnlGroupMembers.Visible = true;

                lHeading.Text = string.Format( "{0} {1}", _group.GroupType.GroupTerm, _group.GroupType.GroupMemberTerm.Pluralize() );

                if ( _group.GroupType.Roles.Any() )
                {
                    nbRoleWarning.Visible = false;
                    gGroupMembers.Visible = true;

                    GroupMemberService groupMemberService = new GroupMemberService( new RockContext() );
                    var qry = groupMemberService.Queryable( "Person,GroupRole", true )
                        .Where( m => m.GroupId == _group.Id );

                    SortProperty sortProperty = gGroupMembers.SortProperty;

                    if ( sortProperty != null )
                    {
                        gGroupMembers.DataSource = qry.Sort( sortProperty ).ToList();
                    }
                    else
                    {
                        gGroupMembers.DataSource = qry.OrderBy( a => a.GroupRole.Order ).ThenBy( a => a.Person.LastName ).ThenBy( a => a.Person.FirstName ).ToList();
                    }

                    gGroupMembers.DataBind();
                }
                else
                {
                    nbRoleWarning.Text = string.Format(
                        "{0} cannot be added to this {1} because the '{2}' group type does not have any roles defined.",
                        _group.GroupType.GroupMemberTerm.Pluralize(),
                        _group.GroupType.GroupTerm,
                        _group.GroupType.Name );

                    nbRoleWarning.Visible = true;
                    gGroupMembers.Visible = false;
                }
            }
            else
            {
                pnlGroupMembers.Visible = false;
            }

        }

        /// <summary>
        /// For a group member, calculates the values for the columns
        /// </summary>
        /// <param name="responseSets">The group's reports</param>
        /// <param name="personId">The member's PersonId</param>
        /// <returns>Returns a string array. 
        /// personInfo[0] holds the first report. 
        /// personInfo[1] holds the last report.
        /// personInfo[2] holds the weeks since last report
        /// personInfo[3] holds the reports / opportunities
        /// personInfo[4] holds the reports/opportunites percentage
        /// personInfo[5] holds the person's score</returns>
        private String[] GetPersonInfo( List<ResponseSet> responseSets, GroupMember groupMember )
        {
            DateTime firstReport = new DateTime();
            DateTime lastReport = new DateTime();
            DateTime defaultDate = new DateTime();
            double? weeksSinceLast = null;
            double reports = 0;
            double opportunities = 0;
            double? percentSubmitted = null;
            double score = 0;
            groupMember.LoadAttributes();
            DateTime memberStartDate = DateTime.Parse( groupMember.GetAttributeValue( "MemberStartDate" ) );

            //Iterate through the responseSets
            for ( int i = 0; i < responseSets.Count; i++ )
            {
                if ( responseSets[i].PersonId == groupMember.PersonId )
                {
                    reports++;
                    score += responseSets[i].Score;
                    if ( firstReport == defaultDate || firstReport > responseSets[i].SubmitForDate )
                    {
                        firstReport = responseSets[i].SubmitForDate;
                    }
                    if ( lastReport == defaultDate || lastReport < responseSets[i].SubmitForDate )
                    {
                        lastReport = responseSets[i].SubmitForDate;
                    }
                }
            }
            if ( lastReport != defaultDate )
            {
                weeksSinceLast = ( ( DateTime.Today - lastReport ).Days ) / 7;
            }
            if ( _reportStartDate != null )
            {
                opportunities = ( ( NextReportDate( _reportStartDate ) - memberStartDate ).Days / 7 );
                if ( lastReport != defaultDate )
                {
                    DateTime x1 = lastReport.Date;
                    DateTime x2 = NextReportDate( _reportStartDate ).Date;
                    if ( lastReport.Date == NextReportDate( _reportStartDate ).Date )
                    {
                        opportunities++;
                    }
                }

                score = score / opportunities;
                percentSubmitted = reports / opportunities;
            }
            else
            {
                score = 0;
            }

            //Put info into string array
            String[] personInfo = new String[6];
            if ( firstReport != defaultDate )
            {
                personInfo[0] = firstReport.ToShortDateString();
            }
            else
            {
                personInfo[0] = "-";
            }
            if ( lastReport != defaultDate )
            {
                personInfo[1] = lastReport.ToShortDateString();
            }
            else
            {
                personInfo[1] = "-";
            }
            if ( weeksSinceLast != null )
            {
                personInfo[2] = weeksSinceLast.ToString();
            }
            else
            {
                personInfo[2] = "no reports";
            }
            if ( _reportStartDate != null )
            {
                personInfo[3] = string.Format( "{0} / {1}", reports, opportunities );
                personInfo[4] = string.Format( "{0:P0}", reports / opportunities );
            }
            else
            {
                personInfo[3] = "-";
                personInfo[4] = "-";
            }
            if ( opportunities == 0 )
            {
                personInfo[4] = "-";
                personInfo[5] = "-";
            }
            else
            {
                personInfo[5] = score.ToString( "0.00" );
            }
            return personInfo;
        }

        /// <summary>
        /// Gets the next report date based on the report start date
        /// </summary>
        /// <param name="reportStartDate">The group's report start date</param>
        /// <returns>Returns the next date the report is due</returns>
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
        /// Returns true if the current person is a group leader.
        /// </summary>
        /// <param name="groupId">The group Id</param>
        /// <returns>A boolean: true if the person is a leader, false if not.</returns>
        protected bool IsPersonLeader( int groupId )
        {
            int count = new GroupMemberService( new RockContext() ).Queryable( "GroupTypeRole" )
                .Where( m =>
                    m.PersonId == CurrentPersonId &&
                    m.GroupId == groupId &&
                    m.GroupRole.Name == "Leader"
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

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        #endregion
    }
}