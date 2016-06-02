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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.Utility
{
    [DisplayName( "Import People" )]
    [Category( "com_centralaz > Utility" )]
    [Description( "Tool to import a CSV file of matching people into a group. The CSV must have the first name, last name and email. This block can also optionally add a person record (if no match is found) before adding it to the group." )]

    [BooleanField( "Auto Add People", "If true, will automatically add unmatched emails as new people records." )]
    [DefinedValueField( "2E6540EA-63F0-40FE-BE50-F2A84735E600", "Connection Status", "The connection status to use for new individuals (default: 'Web Prospect'.)", true, false, "368DD475-242C-49C4-A42C-7278BE690CC2", "", 2 )]
    [DefinedValueField( "8522BADD-2871-45A5-81DD-C76DA07E2E7E", "Record Status", "The record status to use for new individuals (default: 'Pending'.)", true, false, "283999EC-7346-42E3-B807-BCE9B2BABB49", "", 3 )]

    public partial class ImportPeople : RockBlock
    {
        #region Fields

        private static string SESSIONKEY_TO_PROCESS = "CccevImportPeople";
        private static string SESSIONKEY_PERSONIDs = "CccevImportPeoplePersonIDs";
        private static string SESSIONKEY_UNMATCHED_ITEMS = "CccevImportPeopleUnmatchedItems";

        #endregion

        #region Base Control Methods

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
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                Session[SESSIONKEY_TO_PROCESS] = null;
                Session[SESSIONKEY_PERSONIDs] = null;
                Session[SESSIONKEY_UNMATCHED_ITEMS] = null;
                BindFormData();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the NextButtonClick event of the Wizard1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="WizardNavigationEventArgs"/> instance containing the event data.</param>
        protected void Wizard1_NextButtonClick( object sender, WizardNavigationEventArgs e )
        {
            WizardStepType nextStep = Wizard1.WizardSteps[e.NextStepIndex].StepType;

            if ( e.CurrentStepIndex == 0 )
            {
                InitializeWizardStep2();

                if ( !ValidateFileUpload() )
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    int cleanMatches = ProcessCleanMatches();

                    ArrayList unmatchedItems = (ArrayList)Session[SESSIONKEY_UNMATCHED_ITEMS];
                    var remainingItems = (Queue<string[]>)Session[SESSIONKEY_TO_PROCESS];

                    lblRemainingRecords.Text = remainingItems.Count.ToString();
                    lblCleanMatches.Text = cleanMatches.ToString();
                    lblReadyToImportPeople.Text = lblCleanMatches.Text;
                    lblUnmatchedItems.Text = unmatchedItems.Count.ToString();
                }
            }

            // Resolve ambiguous person matches
            if ( e.CurrentStepIndex == 1 )
            {
                Wizard1.StepPreviousButtonText = "Restart";

                var remainingItems = (Queue<string[]>)Session[SESSIONKEY_TO_PROCESS];
                ArrayList personIDs = (ArrayList)Session[SESSIONKEY_PERSONIDs];

                lblRemainingRecords.Text = remainingItems.Count.ToString();

                if ( rblPeople.Items.Count > 1 && rblPeople.SelectedValue != "" )
                {
                    personIDs.Add( int.Parse( rblPeople.SelectedValue ) );
                    Session[SESSIONKEY_PERSONIDs] = personIDs;
                    rblPeople.ClearSelection();
                }

                lblReadyToImportPeople.Text = personIDs.Count.ToString();

                // are there ambiguous person matches?
                if ( ResolveAmbiguousPeople() )
                {
                    e.Cancel = true;
                }
            }
            else if ( e.CurrentStepIndex == 3 )
            {
                if ( String.IsNullOrWhiteSpace( ddlGroupType.SelectedValue ) || String.IsNullOrWhiteSpace( ddlGroupRole.SelectedValue ) || String.IsNullOrWhiteSpace( ddlGroupMemberStatus.SelectedValue ) || String.IsNullOrWhiteSpace( ddlGroup.SelectedValue ) )
                {
                    e.Cancel = true;
                    return;
                }
            }
            else if ( nextStep == WizardStepType.Finish )
            {
                // Prepare the summary and UI of the "Finish" step
                DisplaySummary();
            }
            else
            {
                Wizard1.StepPreviousButtonText = "Previous";
            }
        }

        /// <summary>
        /// Handles the FinishButtonClick event of the Wizard1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="WizardNavigationEventArgs"/> instance containing the event data.</param>
        protected void Wizard1_FinishButtonClick( object sender, WizardNavigationEventArgs e )
        {
            lblComplete.CssClass = "";
            int alreadyConnected = 0;
            int badEmail = 0;
            int miscProblems = 0;
            int totalSaved = 0;
            int recordNumber = 0;
            GroupMemberStatus groupMemberStatus = ddlGroupMemberStatus.SelectedValue.ConvertToEnum<GroupMemberStatus>();

            List<string> probItems = new List<string>();

            if ( GetAttributeValue( "AutoAddPeople" ).AsBoolean() && !cbDisableAutoAdd.Checked )
            {
                miscProblems = AddNewPeople();
            }

            try
            {
                int? groupId = ddlGroup.SelectedValueAsInt();
                if ( groupId != null )
                {
                    var rockContext = new RockContext();
                    var groupService = new GroupService( rockContext );
                    var groupMemberService = new GroupMemberService( rockContext );

                    var group = groupService.Get( groupId.Value );
                    if ( group != null )
                    {
                        ArrayList personIds = (ArrayList)Session[SESSIONKEY_PERSONIDs];

                        // now process through each personID and add them to the tag if not already there
                        foreach ( int personId in personIds )
                        {
                            recordNumber++;

                            GroupMember matchingMember = group.Members.Where( m => m.PersonId == personId ).FirstOrDefault();

                            // Add the person if he/she was not already a member
                            if ( matchingMember == null )
                            {
                                GroupMember member = new GroupMember();
                                member.GroupId = group.Id;
                                member.PersonId = personId;
                                member.GroupMemberStatus = groupMemberStatus;
                                member.GroupRoleId = ddlGroupRole.SelectedValueAsInt().Value;

                                try
                                {
                                    groupMemberService.Add( member );
                                    totalSaved++;
                                }
                                catch ( System.Exception saveException )
                                {
                                    miscProblems++;
                                    probItems.Add( string.Format( "{0} : {1}<br />", personId, saveException.Message ) );
                                }
                            }
                            else
                            {
                                alreadyConnected++;
                            }
                        }

                        rockContext.SaveChanges();

                        if ( totalSaved > 0 )
                        {
                            lblCompleteMsg.Text += totalSaved + " successfully saved.<br />";
                        }

                        if ( alreadyConnected > 0 )
                        {
                            lblCompleteMsg.Text += alreadyConnected + " of the email addresses were already tagged.<br />";
                        }

                        if ( badEmail > 0 )
                        {
                            lblCompleteMsg.Text += badEmail + " of the provided email addresses were blank or invalid.<br />";
                        }

                        if ( miscProblems > 0 )
                        {
                            lblCompleteMsg.Text += miscProblems + " of the provided items were unable to be saved:<br /><br /> " +
                                string.Join( "<br/> ", probItems.ToArray() ) + "<br /><br />";
                        }
                    }
                }
            }
            catch ( System.Exception ex )
            {
                lblComplete.Text += string.Format( "An error occurred while handling record {0} which prevented the people from being imported.<br/><br/>{1}<br/>", recordNumber, ex.Message, ex.StackTrace );
                lblComplete.CssClass = "errorText";
            }
            finally
            {
                Session[SESSIONKEY_TO_PROCESS] = null;
                Session[SESSIONKEY_PERSONIDs] = null;
                Session[SESSIONKEY_UNMATCHED_ITEMS] = null;
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroupType_SelectedIndexChanged( object sender, EventArgs e )
        {
            int? groupTypeId = ddlGroupType.SelectedValue.AsIntegerOrNull();
            LoadGroupRoles( groupTypeId );
            LoadGroups( groupTypeId );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the wizard step2.
        /// </summary>
        private void InitializeWizardStep2()
        {
            Session[SESSIONKEY_TO_PROCESS] = null;
            Session[SESSIONKEY_PERSONIDs] = null;
            Session[SESSIONKEY_UNMATCHED_ITEMS] = null;

            rblPeople.Items.Clear();
            lblRawLineData.Text = "";

            lblCleanMatches.Text = "TBD";
            lblReadyToImportPeople.Text = "TBD";
            lblUnmatchedItems.Text = "TBD";
            lblRemainingRecords.Text = "TBD";
        }

        /// <summary>
        /// Processes the clean matches.
        /// </summary>
        /// <returns></returns>
        private int ProcessCleanMatches()
        {
            var preProcess = (Queue<string[]>)Session[SESSIONKEY_TO_PROCESS];
            var reProcess = new Queue<string[]>();
            int numCleanMatches = 0;
            ArrayList personIds = (ArrayList)Session[SESSIONKEY_PERSONIDs];
            ArrayList unmatchedItems = (ArrayList)Session[SESSIONKEY_UNMATCHED_ITEMS];

            do
            {
                string[] item = preProcess.Dequeue();
                string firstName;
                string lastName;
                string email;
                ItemToPerson( item, out firstName, out lastName, out email );

                // skip invalid email addresses...
                if ( !string.IsNullOrEmpty( email ) )
                {
                    var people = new PersonService( new RockContext() ).GetByEmail( email ).ToList();

                    if ( people.Count == 0 )
                    {
                        unmatchedItems.Add( item );
                    }
                    else if ( people.Count == 1 )
                    {
                        personIds.Add( people[0].Id );
                        numCleanMatches++;
                    }
                    else if ( people.Count > 1 && FoundExactMatch( personIds, firstName.ToLower(), lastName.ToLower(), email ) )
                    {
                        numCleanMatches++;
                    }
                    else
                    {
                        reProcess.Enqueue( item );
                    }
                }

            } while ( preProcess.Count != 0 );

            Session[SESSIONKEY_PERSONIDs] = personIds;
            Session[SESSIONKEY_TO_PROCESS] = reProcess;
            Session[SESSIONKEY_UNMATCHED_ITEMS] = unmatchedItems;

            return numCleanMatches;
        }

        /// <summary>
        /// Resolves the ambiguous people.
        /// </summary>
        /// <returns></returns>
        private bool ResolveAmbiguousPeople()
        {
            var list = (Queue<string[]>)Session[SESSIONKEY_TO_PROCESS];
            ArrayList unmatchedItems = (ArrayList)Session[SESSIONKEY_UNMATCHED_ITEMS];
            ArrayList personIds = (ArrayList)Session[SESSIONKEY_PERSONIDs];

            if ( list.Count == 0 )
            {
                return false;
            }

            // now process through each record we're importing...
            bool moreUnresolved = false;
            do
            {
                string[] item = list.Dequeue();
                string firstName;
                string lastName;
                string email;
                ItemToPerson( item, out firstName, out lastName, out email );

                // skip invalid email addresses...
                if ( !string.IsNullOrEmpty( email ) )
                {
                    var people = new PersonService( new RockContext() ).GetByEmail( email ).ToList(); ;

                    if ( people.Count > 1 )
                    {
                        moreUnresolved = true;
                        // Add each person to the radio button list.
                        rblPeople.Items.Clear();
                        lblRawLineData.Text = string.Join( ", ", item );

                        foreach ( Person person in people )
                        {
                            rblPeople.Items.Add( new ListItem( string.Format( "{0} ({1}yrs) city: {2}", person.FullName, person.Age == null ? "? " : person.Age.ToString(), person.GetHomeLocation() == null ? " - " : person.GetHomeLocation().City ), person.Id.ToString() ) );
                        }

                        // now break out and let the user select the appropriate user.
                        break;
                    }
                }

            } while ( list.Count != 0 );

            Session[SESSIONKEY_PERSONIDs] = personIds;
            Session[SESSIONKEY_UNMATCHED_ITEMS] = unmatchedItems;
            lblUnmatchedItems.Text = unmatchedItems.Count.ToString();

            return moreUnresolved;
        }

        /// <summary>
        /// Founds the exact match.
        /// </summary>
        /// <param name="personIds">The person ids.</param>
        /// <param name="firstOrNickName">First name of the or nick.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        private bool FoundExactMatch( ArrayList personIds, string firstOrNickName, string lastName, string email )
        {
            var exactPeople = new PersonService( new RockContext() ).GetByMatch( firstOrNickName, lastName, email );

            if ( exactPeople.Count() == 1 )
            {
                personIds.Add( exactPeople.FirstOrDefault().Id );
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Adds the new person.
        /// </summary>
        /// <param name="people">The people.</param>
        /// <param name="firstName">The first name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        private bool AddNewPerson( List<Person> people, string firstName, string lastName, string email )
        {
            var rockContext = new RockContext();
            bool retval = true;
            try
            {
                Person person = new Person();
                person.FirstName = firstName;
                person.LastName = lastName;
                person.Email = email;
                person.IsEmailActive = true;
                person.EmailPreference = EmailPreference.EmailAllowed;
                person.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                person.RecordStatusValueId = DefinedValueCache.Read( GetAttributeValue( "RecordStatus" ).AsGuid() ).Id;
                person.ConnectionStatusValueId = DefinedValueCache.Read( GetAttributeValue( "ConnectionStatus" ).AsGuid() ).Id;
                person.Gender = Gender.Unknown;
                PersonService.SaveNewPerson( person, rockContext );
                rockContext.SaveChanges();

                people.Add( person );
            }
            catch
            {
                retval = false;
            }

            return retval;
        }

        /// <summary>
        /// Adds the new people.
        /// </summary>
        /// <returns></returns>
        private int AddNewPeople()
        {
            ArrayList unmatchedItems = (ArrayList)Session[SESSIONKEY_UNMATCHED_ITEMS];
            ArrayList personIds = (ArrayList)Session[SESSIONKEY_PERSONIDs];
            var badList = (ArrayList)Session[SESSIONKEY_UNMATCHED_ITEMS];

            int fail = 0;

            foreach ( string[] item in unmatchedItems )
            {
                string firstName;
                string lastName;
                string email;
                ItemToPerson( item, out firstName, out lastName, out email );
                List<Person> people = new List<Person>();
                if ( AddNewPerson( people, firstName, lastName, email ) )
                {
                    personIds.Add( people[0].Id );
                }
                else
                {
                    fail++;
                    badList.Add( item );
                }
            }

            Session[SESSIONKEY_PERSONIDs] = personIds;
            return fail;
        }

        /// <summary>
        /// Displays the summary.
        /// </summary>
        private void DisplaySummary()
        {
            int totalToImport = 0;

            var list = (ArrayList)Session[SESSIONKEY_PERSONIDs];
            var badList = (ArrayList)Session[SESSIONKEY_UNMATCHED_ITEMS];
            string importPeopleMessage = "Unable to";

            if ( GetAttributeValue( "AutoAddPeople" ).AsBoolean() )
            {
                totalToImport = list.Count + badList.Count;
                importPeopleMessage = "Add and then";
            }
            else
            {
                divAlert.Visible = false;
            }

            StringBuilder badItems = new StringBuilder();
            foreach ( string[] s in badList )
            {
                badItems.Append( "<br/>" + string.Join( ",", s ) );
            }

            lblSummary.Text = String.Format( @"
            <dl class='dl-horizontal'>
                <dt>People to import:</dt>
                <dd>{0}</dd>         
                <dt>Group:</dt>
                <dd>{1}</dd>
                <dt>{2} import:</dt>
                <dd>{3}</dd>
            </dl>", totalToImport, ddlGroup.SelectedItem.Text, importPeopleMessage, badItems.ToString() );
        }

        /// <summary>
        /// Validates the file upload.
        /// </summary>
        /// <returns></returns>
        private bool ValidateFileUpload()
        {
            bool errors = false;
            bool isValid = false;
            lblErrors.Text = string.Empty;
            StringBuilder sbErrors = new StringBuilder();

            if ( fuUpload.PostedFile != null )
            {
                int lineNum = 0;
                StreamReader sr = new StreamReader( fuUpload.PostedFile.InputStream );
                var list = new Queue<string[]>();
                string rec = null;
                while ( ( rec = sr.ReadLine() ) != null )
                {
                    lineNum++;
                    string[] split = rec.Split( new Char[] { ',' } );
                    if ( split.Length != 3 )
                    {
                        errors = true;
                        string adverb = split.Length < 3 ? "only" : "";
                        string plural = split.Length == 1 ? "" : "s";
                        sbErrors.AppendFormat( "Line {0} has {1} {2} field{3}. ({4})<br/>", lineNum, adverb, split.Length, plural, rec );
                    }
                    else if ( !split[2].Contains( "@" ) )
                    {
                        errors = true;
                        sbErrors.AppendFormat( "Line {0} is missing an email address in the third field ({1}).<br/>", lineNum, rec );
                    }
                    else if ( !split[2].IsValidEmail() )
                    {
                        errors = true;
                        sbErrors.AppendFormat( "Line {0} does not contain a valid email address ({1}).<br/>", lineNum, rec );
                    }
                    else
                    {
                        list.Enqueue( split );
                    }
                }
                Session[SESSIONKEY_TO_PROCESS] = list;
            }
            else
            {
                sbErrors.Append( "No file was found."  );
            }

            if ( errors )
            {
                lblErrors.Text = sbErrors.ToString();
                divErrors.Visible = true;
            }
            else
            {
                isValid = true;
                Session[SESSIONKEY_PERSONIDs] = new ArrayList();
                Session[SESSIONKEY_UNMATCHED_ITEMS] = new ArrayList();
            }

            return isValid;
        }

        /// <summary>
        /// Items to person.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="firstName">The first name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="email">The email.</param>
        private static void ItemToPerson( string[] item, out string firstName, out string lastName, out string email )
        {
            firstName = (string)item[0].Trim();
            lastName = (string)item[1].Trim();
            email = (string)item[2].ToLower().Trim();
        }

        /// <summary>
        /// Binds the form data.
        /// </summary>
        private void BindFormData()
        {
            // bind group types
            ddlGroupType.Items.Clear();

            using ( var rockContext = new RockContext() )
            {
                var groupTypeService = new Rock.Model.GroupTypeService( rockContext );

                // get all group types that have at least one role
                var groupTypes = groupTypeService.Queryable().Where( a => a.Roles.Any() ).OrderBy( a => a.Name ).ToList();

                foreach ( var g in groupTypes )
                {
                    ddlGroupType.Items.Add( new ListItem( g.Name, g.Id.ToString().ToUpper() ) );
                }
            }

            LoadGroupRoles( ddlGroupType.SelectedValue.AsInteger() );
            LoadGroups( ddlGroupType.SelectedValue.AsInteger() );

            // bind group member status
            ddlGroupMemberStatus.BindToEnum<GroupMemberStatus>();
        }

        /// <summary>
        /// Loads the groups.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        private void LoadGroups( int? groupTypeId )
        {
            int? currentGroupId = ddlGroup.SelectedValue.AsIntegerOrNull();
            ddlGroup.SelectedValue = null;
            ddlGroup.Items.Clear();

            if ( groupTypeId.HasValue )
            {
                var groupService = new Rock.Model.GroupService( new RockContext() );
                var groups = groupService.Queryable()
                    .Where( r =>
                        r.GroupTypeId == groupTypeId.Value )
                    .OrderBy( a => a.Name )
                    .ToList();

                foreach ( var r in groups )
                {
                    var groupItem = new ListItem( r.Name, r.Id.ToString().ToUpper() );
                    groupItem.Selected = r.Id == currentGroupId;
                    ddlGroup.Items.Add( groupItem );
                }
            }
        }

        /// <summary>
        /// Loads the group roles.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        private void LoadGroupRoles( int? groupTypeId )
        {
            int? currentGroupRoleId = ddlGroupRole.SelectedValue.AsIntegerOrNull();
            ddlGroupRole.SelectedValue = null;
            ddlGroupRole.Items.Clear();

            if ( groupTypeId.HasValue )
            {
                var groupRoleService = new Rock.Model.GroupTypeRoleService( new RockContext() );
                var groupRoles = groupRoleService.Queryable()
                    .Where( r =>
                        r.GroupTypeId == groupTypeId.Value )
                    .OrderBy( a => a.Name )
                    .ToList();

                foreach ( var r in groupRoles )
                {
                    var roleItem = new ListItem( r.Name, r.Id.ToString().ToUpper() );
                    roleItem.Selected = r.Id == currentGroupRoleId;
                    ddlGroupRole.Items.Add( roleItem );
                }
            }
        }

        #endregion
    }
}