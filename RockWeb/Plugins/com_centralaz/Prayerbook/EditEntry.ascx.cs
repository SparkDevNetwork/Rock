// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
//NOTES
//  - Look at GroupTypeDetail and PrayerRequestEntry blocks for use of validators, character counters, etc.
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Security;

using com.centralaz.Prayerbook.Utility;

namespace RockWeb.Plugins.com_centralaz.Prayerbook
{
    /// <summary>
    /// Edit and Add Prayer Book Entries
    /// </summary>
    [DisplayName( "UP Team Prayerbook Edit Entry" )]
    [Category( "centralaz > Prayerbook" )]
    [Description( "Edit and Add Prayerbook Entries" )]
    public partial class EditEntry : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        private GroupMember entry;
        private Rock.Model.Group book;
        private bool editEnabled = false;
        private bool administrateEnabled = false;

        private RockContext rockContext;
        private GroupService groupService;
        private DefinedValueService definedValueService;
        private GroupMemberService groupMemberService;

        #endregion

        #region Properties

        // used for public / protected properties

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
            base.OnLoad( e );
        }

        protected void Page_Load( object sender, EventArgs e )
        {
            editEnabled = IsUserAuthorized( Authorization.EDIT );
            administrateEnabled = IsUserAuthorized( Authorization.ADMINISTRATE );

            rockContext = new RockContext();
            groupService = new GroupService( rockContext );
            definedValueService = new DefinedValueService( rockContext );
            groupMemberService = new GroupMemberService( rockContext );

            if ( !IsPostBack )
            {
                //load ministry DefinedValues into DDL control
                var ministryDefinedValues = GetActiveDefinedValues( com.centralaz.Prayerbook.SystemGuid.DefinedType.MINISTRY_DEFINED_TYPE );
                ddlMinistry.DataSource = ministryDefinedValues;
                ddlMinistry.DataBind();

                //load subministry DefinedValues into DDL control
                var subministryDefinedValues = GetActiveDefinedValues( com.centralaz.Prayerbook.SystemGuid.DefinedType.SUBMINISTRY_DEFINED_TYPE );
                ddlSubministry.DataSource = subministryDefinedValues;
                ddlSubministry.DataBind();

                //Get the EntryId, if it is passed in through the URL
                int entryId = PageParameter( "Id" ).AsInteger();

                //The contributor
                Person contributor;

                //If an entryId was passed in from the URL
                if ( entryId > 0 )
                {
                    //Get the entry
                    entry = groupMemberService.Get( entryId );
                    entry.LoadAttributes( rockContext );

                    //Get the book the entry is in
                    book = entry.Group;

                    //Display the contributor name and, if exists, spouse name
                    lName.Text = entry.Person.FullName;
                    if ( entry.Person.GetSpouse() != null )
                    {
                        lSpouseName.Text = entry.Person.GetSpouse().FirstName;
                    }

                    //Hide the contributors dropdown
                    ddlContributors.Visible = false;
                    ddlContributors.Enabled = false;

                    //populate controls with entry attribute data
                    // select the ministry and subminitry from the entry in the ddls
                    string entryMinistryAttributeValueGuid = entry.GetAttributeValue( "Ministry" );
                    string entrySubministryAttributeValueGuid = entry.GetAttributeValue( "Subministry" );

                    if ( entryMinistryAttributeValueGuid != String.Empty )
                    {
                        var entryMinistryAttributeValue = definedValueService.GetByGuid( Guid.Parse( entryMinistryAttributeValueGuid ) );
                        ddlMinistry.SelectedValue = entryMinistryAttributeValue.Id.ToString();
                    }

                    if ( entrySubministryAttributeValueGuid != String.Empty )
                    {
                        var entrySubministryAttributeValue = definedValueService.GetByGuid( Guid.Parse( entrySubministryAttributeValueGuid ) );
                        ddlSubministry.SelectedValue = entrySubministryAttributeValue.Id.ToString();
                    }

                    // insert the text of the submissions
                    dtbPraise1.Text = entry.AttributeValues["Praise1"].Value;
                    dtbPersonalRequest1.Text = entry.AttributeValues["PersonalRequest1"].Value;
                    dtbPersonalRequest2.Text = entry.AttributeValues["PersonalRequest2"].Value;
                    dtbMinistryNeed1.Text = entry.AttributeValues["MinistryNeed1"].Value;
                    dtbMinistryNeed2.Text = entry.AttributeValues["MinistryNeed2"].Value;
                    dtbMinistryNeed3.Text = entry.AttributeValues["MinistryNeed3"].Value;

                    //set the contributor for this entry
                    contributor = entry.Person;

                    //save entry Id for future use
                    hidEntryId.Value = entry.Id.ToString();
                }
                //else this is a new entry
                else
                {
                    //get the current book
                    book = MostRecentBook.Get( rockContext );

                    //Load contributor control
                    BuildContributorsDropDownList( book );

                    //display the contributors dropdown
                    lName.Visible = false;
                    ddlContributors.Visible = true;
                    ddlContributors.Enabled = true;

                    btnDelete.Visible = false;

                    //set the contributor
                    if ( ddlContributors.SelectedIndex != -1 )
                    {
                        contributor = new PersonService( rockContext ).Get( int.Parse( ddlContributors.SelectedValue ) );

                        //update contributor spouse, ministry, subministry in associated controls
                        UpdateSelectedContributorInfo( contributor );
                    }
                }

                //Assign bookid to hidden fields for use in delete button event handler, etc.
                hidBookId.Value = book.Id.ToString();

                book.LoadAttributes( rockContext );

                //Lock all the controls if the book has already been published
                if ( Boolean.Parse( book.AttributeValues["isPublished"].Value ) )
                {
                    ddlMinistry.Enabled = false;
                    ddlSubministry.Enabled = false;
                    dtbPraise1.Enabled = false;
                    dtbPersonalRequest1.Enabled = false;
                    dtbPersonalRequest2.Enabled = false;
                    dtbMinistryNeed1.Enabled = false;
                    dtbMinistryNeed2.Enabled = false;
                    dtbMinistryNeed3.Enabled = false;
                    btnDelete.Enabled = false;
                    btnSave.Enabled = false;
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

        /// <summary>
        /// Contributors DropDown selection change event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ddlContributors_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateSelectedContributorInfo( new PersonService( rockContext ).Get( int.Parse( ddlContributors.SelectedValue ) ) );
        }

        /// <summary>
        /// Save button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            //kick user back to page to make corrections
            // NEED TO WRITE MY OWN VALIDATION
            if ( !Page.IsValid )
            {
                return;
            }

            //Initialize some variable
            Rock.Model.Person contributor;
            GroupMember contributorGroupMember;

            //get the EntryId on the page, if any
            int entryId;
            bool result = int.TryParse( hidEntryId.Value, out entryId );
            var currentBook = groupService.Get( int.Parse( hidBookId.Value ) );

            //Get associated Contributors GroupMember info
            if ( ddlContributors.Visible )
            {
                //Get the Contributor GroupMember info for the selected contributor
                contributor = new PersonService( rockContext ).Get( int.Parse( ddlContributors.SelectedValue ) );
                contributorGroupMember = groupService.Get( com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP.AsGuid() ).Members.Where( gm => gm.PersonId == contributor.Id ).Single();
                contributorGroupMember.LoadAttributes();
                entry = new GroupMember();
                entry.Person = contributor;
                entry.PersonId = contributor.Id;
                currentBook.Members.Add( entry );
                groupMemberService.Add( entry );
            }
            else
            {
                //Get the Contributor GroupMember info for the entrant
                entry = groupMemberService.Get( entryId );
                contributor = entry.Person;
                contributorGroupMember = groupService.Get( com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP.AsGuid() ).Members.Where( gm => gm.PersonId == entry.PersonId ).Single();
                contributorGroupMember.LoadAttributes();
            }

            entry.LoadAttributes( rockContext );

            //get updated entry data
            entry.SetAttributeValue( "Praise1", ScrubString( dtbPraise1.Text ) );
            entry.SetAttributeValue( "PersonalRequest1", ScrubString( dtbPersonalRequest1.Text ) );
            entry.SetAttributeValue( "PersonalRequest2", ScrubString( dtbPersonalRequest2.Text ) );
            entry.SetAttributeValue( "MinistryNeed1", ScrubString( dtbMinistryNeed1.Text ) );
            entry.SetAttributeValue( "MinistryNeed2", ScrubString( dtbMinistryNeed2.Text ) );
            entry.SetAttributeValue( "MinistryNeed3", ScrubString( dtbMinistryNeed3.Text ) );

            //Update the entry ministry and subministry definedvalues with those in the DropDownLists
            int selectedMinistryDefinedValueId = ddlMinistry.SelectedValue.AsInteger();
            int selectedSubministryDefinedValueId = ddlSubministry.SelectedValue.AsInteger();

            //if a ministry is selected in the ddl, update the book entry(GroupMember) AND the contributor(GroupMember)
            if ( selectedMinistryDefinedValueId > 0 )
            {
                string selectedMinistryDefinedValueGuid = definedValueService.Get( selectedMinistryDefinedValueId ).Guid.ToString();
                entry.SetAttributeValue( "Ministry", selectedMinistryDefinedValueGuid );
                contributorGroupMember.SetAttributeValue( "Ministry", selectedMinistryDefinedValueGuid );
            }
            //else clear it
            else
            {
                entry.SetAttributeValue( "Ministry", String.Empty );
                contributorGroupMember.SetAttributeValue( "Ministry", String.Empty );
            }

            //if a subministry is selected in the ddl
            if ( selectedSubministryDefinedValueId > 0 )
            {
                string selectedSubministryDefinedValueGuid = definedValueService.Get( selectedSubministryDefinedValueId ).Guid.ToString();
                entry.SetAttributeValue( "Subministry", selectedSubministryDefinedValueGuid );
                contributorGroupMember.SetAttributeValue( "Subministry", selectedSubministryDefinedValueGuid );
            }
            //else clear it
            else
            {
                entry.SetAttributeValue( "Subministry", String.Empty );
                contributorGroupMember.SetAttributeValue( "Subministry", String.Empty );
            }

            //Add if new entry
            if ( entry.Id == 0 )
            {
                //Set the GroupRole and GroupMemberStatus values for the new entry(GroupMember)
                entry.GroupMemberStatus = GroupMemberStatus.Active;
                GroupTypeRole gtr = new GroupTypeRoleService( rockContext ).Get( Guid.Parse( com.centralaz.Prayerbook.SystemGuid.GroupTypeRole.BOOKS_GROUPTYPE_MEMBER_GROUPTYPEROLE ) );
                entry.GroupRole = gtr;
                entry.GroupRoleId = gtr.Id;
            }

            //write changes to database. Do this BEFORE .SaveAttributeValues(), especially for new objects.
            rockContext.SaveChanges();

            //save both GroupMembers
            entry.SaveAttributeValues( rockContext );
            contributorGroupMember.SaveAttributeValues( rockContext );

            //navigate to Pryaer Book home page
            NavigateToParentPage();
        }

        /// <summary>
        /// Cancel button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            //navigate to parent page (the Prayerbook app home page)
            NavigateToParentPage();
        }

        /// <summary>
        /// Delete button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            //get the currently viewed entryid as an int
            int entryId = int.Parse( hidEntryId.Value );

            //if entry is null, get the entry currently being viewed on the page
            if ( entry == null )
            {
                entry = groupMemberService.Get( entryId );
            }

            //delete the current entry
            groupMemberService.Delete( entry );

            //write the changes to the database
            rockContext.SaveChanges();

            //navigate back to Prayer Book home page
            NavigateToParentPage();
        }

        #endregion

        #region Methods

        /// <summary>        
        ///	Scrubs input text, replacing/removing special characters and extended ASCII characters.
        ///	Thanks to: http://www.andornot.com/blog/post/Replace-MS-Word-special-characters-in-javascript-and-C.aspx
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns>Clean string</returns>
        private string ScrubString( string inputString )
        {
            // smart single quotes and apostrophe
            inputString = Regex.Replace( inputString, "[\u2018|\u2019|\u201A]", "'" );
            // smart double quotes
            inputString = Regex.Replace( inputString, "[\u201C|\u201D|\u201E]", "\"" );
            // ellipsis
            inputString = Regex.Replace( inputString, "\u2026", "..." );
            // dashes
            inputString = Regex.Replace( inputString, "[\u2013|\u2014]", "-" );
            // circumflex
            inputString = Regex.Replace( inputString, "\u02C6", "^" );
            // open angle bracket
            inputString = Regex.Replace( inputString, "\u2039", "<" );
            // close angle bracket
            inputString = Regex.Replace( inputString, "\u203A", ">" );
            // spaces
            inputString = Regex.Replace( inputString, "[\u02DC|\u00A0]", " " );
            // other non-ascii chars
            inputString = Regex.Replace( inputString, @"[^\u0020-\u007E]", string.Empty );

            return inputString;
        }

        /// <summary>
        /// Need to trim the contributors list with GroupMembers whose Person matches Persons in the Contributors Group
        /// </summary>
        /// <param name="book">The current book Group</param>
        protected void BuildContributorsDropDownList( Rock.Model.Group book )
        {
            List<Person> contributorsPersons = GetAllContributorPersons();

            List<Person> currentBookPersons = book.Members.Select( t => t.Person ).ToList();
            List<int> x = currentBookPersons.Select( cbp => cbp.Id ).ToList<int>();

            List<Person> filtered = contributorsPersons.Where( cp => !x.Contains( cp.Id ) ).ToList();

            if ( administrateEnabled )
            {
                ddlContributors.DataSource = filtered;
            }
            else
            {
                filtered = filtered.Where( f => f.Id == CurrentPerson.Id ).ToList();
                ddlContributors.DataSource = filtered;
            }

            ddlContributors.DataBind();
        }

        /// <summary>
        /// Get a Person List of all the GroupMembers in the Contributors group
        /// </summary>
        /// <returns>Person List</returns>
        protected List<Person> GetAllContributorPersons()
        {
            var contributorsPersons = groupService.Get( Guid.Parse( com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP ) )
                .Members
                .Select( t => t.Person )
                .OrderBy( t => t.FullName );

            return contributorsPersons.ToList();
        }

        protected void UpdateSelectedContributorInfo( Person contributor )
        {
            //Fill in the spouse name, if available
            String spouseFirstName;
            try
            {
                spouseFirstName = contributor.GetSpouse().FirstName;
            }
            catch
            {
                spouseFirstName = String.Empty;
            }
            lSpouseName.Text = spouseFirstName;

            //Get the Ministry and Subministry attribute values from the contributorGroupMember associated with the selected Person
            var selectedContributor = groupMemberService.GetByGroupIdAndPersonId( groupService.Get( Guid.Parse( com.centralaz.Prayerbook.SystemGuid.Group.CONTRIBUTORS_GROUP ) ).Id, contributor.Id ).Single();
            selectedContributor.LoadAttributes( rockContext );

            //populate the Ministry and Subministry ddls with selectedContributor values
            string selectedContributorMinistryAttributeValueGuid = selectedContributor.GetAttributeValue( "Ministry" );
            string selectedContributorSubministryAttributeValueGuid = selectedContributor.GetAttributeValue( "Subministry" );

            if ( selectedContributorMinistryAttributeValueGuid != String.Empty )
            {
                var selectedContributorMinistryAttributeValue = definedValueService.GetByGuid( Guid.Parse( selectedContributorMinistryAttributeValueGuid ) );
                ddlMinistry.SelectedValue = selectedContributorMinistryAttributeValue.Id.ToString();
            }
            else
            {
                ddlMinistry.SelectedIndex = 0;
            }

            if ( selectedContributorSubministryAttributeValueGuid != String.Empty )
            {
                var selectedContributorSubministryAttributeValue = definedValueService.GetByGuid( Guid.Parse( selectedContributorSubministryAttributeValueGuid ) );
                ddlSubministry.SelectedValue = selectedContributorSubministryAttributeValue.Id.ToString();
            }
            else
            {
                ddlSubministry.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Builds a list of DefinedValues. Used to build Ministry and Subministry DefinedValues lists.
        /// </summary>
        /// <param name="definedTypeGuid"></param>
        /// <returns>List of type DefinedValue</returns>
        private List<DefinedValue> GetActiveDefinedValues( string definedTypeGuid )
        {
            var ministryDefinedValues = definedValueService.GetByDefinedTypeGuid( Guid.Parse( definedTypeGuid ) )
                .WhereAttributeValue( rockContext, "isActive", "true" )
                .ToList();
            ministryDefinedValues.Insert( 0, new DefinedValue() );
            return ministryDefinedValues;
        }

        #endregion
    }
}