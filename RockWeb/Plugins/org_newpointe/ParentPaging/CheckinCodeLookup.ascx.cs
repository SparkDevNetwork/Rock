using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Attribute;
using Rock.Web.UI.Controls;
using System.Web.UI.WebControls;
using System.Data;
using Rock.Web.Cache;

namespace RockWeb.Plugins.org_newpointe.ParentPage
{

    /// <summary>
    /// Block to pick a person and get their URL encoded key.
    /// </summary>
    [DisplayName( "Check-in Code Lookup" )]
    [Category( "Check-in" )]
    [Description( "Check-in Code Lookup Block." )]

    [IntegerField( "Days Back to Search", "Select how many days back to search", true, 1, "", 0 )]
    [CustomCheckboxListField( "Included Relationships", "The relationships to include.", "SELECT Name AS Text, Guid AS Value FROM GroupTypeRole WHERE GroupTypeId = 11 OR GroupTypeId = 10 ORDER BY [Order]", true, "2639f9a5-2aae-4e48-a8c3-4ffe86681e42,ff9869f1-bc56-4410-8a12-cafc32c62257,567da89f-3c43-443d-a988-c05bc516ef28,03be336c-cd3d-445c-86ec-0856a51dc926,d14827ef-5d43-442d-8134-deb58aac93c5,6f3fadc4-6320-4b54-9cf6-02ef9586a660", "", 1 )]
    [WorkflowTypeField( "Workflow Type", "The workflow to launch.", false, false, "", "", 2 )]
    [LinkedPage( "Workflow Entry Page", "The page to redirect to.", true, "", "", 3 )]

    // TODO: Save search query douring check-in

    public partial class CheckinCodeLookup : Rock.Web.UI.RockBlock
    {

        RockContext rockContext = new RockContext();

        protected string CheckinCode
        {
            get { return ViewState["CheckinCode"] as string; }
            set { ViewState["CheckinCode"] = value; }
        }
        protected Guid SelectedAttendanceGuid
        {
            get { return ( ViewState["SelectedAttendanceGuid"] as string ).AsGuid(); }
            set { ViewState["SelectedAttendanceGuid"] = value.ToString(); }
        }

        protected override void OnLoad( EventArgs e )
        {
            if ( !IsPostBack )
            {
                CheckinCode = PageParameter( "CheckinCode" );
                if ( !String.IsNullOrWhiteSpace( CheckinCode ) )
                    bindGrid();
            }
        }

        static Guid GUID_OwnerRole = Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid();
        static Guid GUID_HomePhone = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid();
        static Guid GUID_MobilePhone = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid();

        protected void bindGrid()
        {
            if ( String.IsNullOrWhiteSpace( CheckinCode ) )
            {
                ShowPanel( 0 );
            }
            else
            {
                rlCheckinCode.Text = CheckinCode + " <a href='?'><i class='fa fa-times'></i></a>";

                if ( !SelectedAttendanceGuid.IsEmpty() )
                {
                    ShowPanel( 2 );
                    var SelectedAttendance = new AttendanceService( rockContext ).Get( SelectedAttendanceGuid );
                    var attSearch = SelectedAttendance.SearchValue;
                    var hasSearch = !String.IsNullOrWhiteSpace( attSearch );
                    if ( hasSearch )
                        attSearch = attSearch.AsNumeric();

                    var SelectedPerson = SelectedAttendance.PersonAlias.Person;
                    rlSelectedPerson.Text = SelectedPerson.FullName + " <a href='?CheckinCode=" + CheckinCode + "'><i class='fa fa-times'></i></a>";

                    var groupRoleServ = new GroupTypeRoleService( rockContext );

                    var knownRelationship_GroupMemberships = new GroupMemberService( rockContext ).Queryable()
                        .Where( gm => gm.Group.GroupTypeId == 11 && gm.PersonId == SelectedPerson.Id );

                    var shownRelationships = GetAttributeValue( "IncludedRelationships" ).Split( ',' ).Select( g => g.AsGuid() );

                    gReleventPeople.DataSource = SelectedPerson.GetFamilyMembers( false, rockContext )
                        .Select( m => new
                        {
                            Person = m.Person,
                            Role = m.GroupRole,
                            Priority = 100
                        } )
                        .ToList()
                        .Union(
                            knownRelationship_GroupMemberships
                            .Where( gm => gm.GroupRole.Guid == GUID_OwnerRole )
                            .SelectMany( gm => gm.Group.Members.Where( gm2 => gm2.PersonId != SelectedPerson.Id ) )
                            .ToList()
                            .Select( gm => new
                            {
                                Person = gm.Person,
                                Role = GetInverseRole( gm.GroupRole, groupRoleServ ),
                                Priority = 50
                            } )
                        )
                        .Union(
                             knownRelationship_GroupMemberships
                            .Where( gm => gm.GroupRole.Guid != GUID_OwnerRole )
                            .Select( gm => new
                            {
                                Person = gm.Group.Members.Where( gm2 => gm2.GroupRole.Guid == GUID_OwnerRole ).FirstOrDefault().Person,
                                Role = gm.GroupRole,
                                Priority = 40
                            } ).ToList()
                        )
                        .Where( r => shownRelationships.Contains( r.Role.Guid ) )
                        .GroupBy( x => x.Person )
                        .Select( x => new PersonRelationship
                        {
                            PersonAliasGuid = x.Key.PrimaryAlias.Guid,
                            FullName = x.Key.FullName,
                            Roles = String.Join( " ,", x.Select( y => y.Role.Name ) ),
                            Priority = x.Select( y => y.Priority ).Max(),
                            HomePhone = GetFormatedNumber( x.Key, GUID_HomePhone ),
                            MobilePhone = GetFormatedNumber( x.Key, GUID_MobilePhone ),
                            Highlight = hasSearch && x.Key.PhoneNumbers.Any( y => y.Number.Contains( attSearch ) )
                        } )
                        .OrderByDescending( r => r.Priority + ( r.Highlight ? 100 : 0) );

                    gReleventPeople.DataKeyNames = new string[] { "PersonAliasGuid" };
                    gReleventPeople.DataBind();
                }
                else
                {
                    ShowPanel( 1 );

                    int daysBacktoSearch = GetAttributeValue( "DaysBacktoSearch" ).AsInteger();
                    var searchDate = DateTime.Now.Date.AddDays( -daysBacktoSearch );

                    gSearchResults.SetLinqDataSource( new AttendanceCodeService( rockContext )
                        .Queryable()
                        .Where( c => c.Code == CheckinCode && c.IssueDateTime > searchDate )
                        .SelectMany( c => c.Attendances )
                        .OrderByDescending( "StartDateTime" ) );

                    gSearchResults.DataKeyNames = new string[] { "Guid" };
                    gSearchResults.DataBind();
                }
            }
        }

        protected GroupTypeRole GetInverseRole( GroupTypeRole role, GroupTypeRoleService groupRoleServ )
        {
            role.LoadAttributes();
            return groupRoleServ.Get( role.GetAttributeValue( "InverseRelationship" ).AsGuid() );
        }

        protected string FormatCheckedIntoString( string grp, string loc )
        {
            return loc.Equals( grp, StringComparison.OrdinalIgnoreCase ) ? grp : grp + " at " + loc;
        }

        protected string GetFormatedNumber( Person person, Guid phoneNumberTypeValueGuid )
        {
            var phoneNumber = person.PhoneNumbers.Where( pn => pn.NumberTypeValue.Guid == phoneNumberTypeValueGuid ).FirstOrDefault();
            return phoneNumber != null ? ( phoneNumber.NumberFormatted + ( phoneNumber.IsMessagingEnabled ? " &nbsp;<i class=\"fa fa-comments\"></i>" : "" ) ) : "";
        }

        protected void ShowPanel( int panel )
        {
            pnlCheckinCode.Visible = panel < 1;
            pnlSearchedCheckinCode.Visible = panel > 0;
            pnlSelectedPerson.Visible = panel > 1;

            pnlAttendanceSearch.Visible = panel == 1;
            pnlRelationSearch.Visible = panel == 2;
        }

        protected void rbbSearch_Click( object sender, EventArgs e )
        {
            CheckinCode = rtbCheckinCode.Text.ToUpper();
            bindGrid();
        }

        protected void gSearchResults_RowSelected( object sender, RowEventArgs e )
        {
            SelectedAttendanceGuid = ( Guid ) e.RowKeyValue;
            bindGrid();
        }

        protected void gReleventPeople_RowSelected( object sender, RowEventArgs e )
        {
            var attendance = new AttendanceService( rockContext ).Get( SelectedAttendanceGuid );
            var selectedPlegePersonAlias = new PersonAliasService( rockContext ).Get( ( Guid ) e.RowKeyValue );
            var workflowTypeCache = WorkflowTypeCache.Get( GetAttributeValue( "WorkflowType" ).AsGuid() );

            if ( attendance != null && selectedPlegePersonAlias != null )
                LaunchWorkflow( rockContext, attendance, selectedPlegePersonAlias, workflowTypeCache);

        }


        /// <summary>
        /// Launches the workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="connectionWorkflow">The connection workflow.</param>
        /// <param name="name">The name.</param>
        private void LaunchWorkflow( RockContext rockContext, Attendance attendance, PersonAlias pagePersonAlias, WorkflowTypeCache pageWorkflowTypeCache )
        {
            if ( pageWorkflowTypeCache != null )
            {
                var workflow = Rock.Model.Workflow.Activate( pageWorkflowTypeCache, pageWorkflowTypeCache.WorkTerm, rockContext );
                if ( workflow != null )
                {
                    workflow.SetAttributeValue( "Person", attendance.PersonAlias.Guid );
                    workflow.SetAttributeValue( "PagePerson", pagePersonAlias.Guid );

                    var workflowService = new Rock.Model.WorkflowService( rockContext );

                    List<string> workflowErrors;
                    if ( workflowService.Process( workflow, attendance, out workflowErrors ) )
                    {
                        if ( workflow.Id != 0 )
                        {

                            if ( workflow.HasActiveEntryForm( CurrentPerson ) )
                            {
                                var qryParam = new Dictionary<string, string>();
                                qryParam.Add( "WorkflowTypeId", pageWorkflowTypeCache.Id.ToString() );
                                qryParam.Add( "WorkflowId", workflow.Id.ToString() );
                                NavigateToLinkedPage( "WorkflowEntryPage", qryParam );
                            }
                            else
                            {
                                mdWorkflowLaunched.Show( string.Format( "A '{0}' workflow has been started.",
                                    pageWorkflowTypeCache.Name ), ModalAlertType.Information );
                            }
                        }
                        else
                        {
                            mdWorkflowLaunched.Show( string.Format( "A '{0}' workflow was processed (but not persisted).",
                                pageWorkflowTypeCache.Name ), ModalAlertType.Information );
                        }
                    }
                    else
                    {
                        mdWorkflowLaunched.Show( "Workflow Processing Error(s):<ul><li>" + workflowErrors.AsDelimited( "</li><li>" ) + "</li></ul>", ModalAlertType.Information );
                    }
                    return;
                }
            }

            Dictionary<string, string> qParams = new Dictionary<string, string>();
            qParams.Add( "AttendanceId", attendance.Id.ToString() );
            qParams.Add( "PagePersonId", pagePersonAlias.Person.Id.ToString() );
            NavigateToLinkedPage( "WorkflowEntryPage", qParams );

        }

        protected void gReleventPeople_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                if ( (e.Row.DataItem as PersonRelationship).Highlight )
                {
                    foreach ( TableCell cell in e.Row.Cells )
                    {
                        cell.BackColor = System.Drawing.Color.FromArgb( 150, 200, 150 );
                    }
                }
            }
        }

        private class PersonRelationship
        {
            public Guid PersonAliasGuid { get; set; }
            public string FullName { get; set; }
            public string Roles { get; set; }
            public int Priority { get; set; }
            public string HomePhone { get; set; }
            public string MobilePhone { get; set; }
            public bool Highlight { get; set; }
        }
    }
}