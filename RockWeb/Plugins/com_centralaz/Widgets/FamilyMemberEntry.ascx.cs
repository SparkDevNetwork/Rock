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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace RockWeb.Plugins.com_centralaz.Widgets
{
    /// <summary>
    /// Block for user to add or edit a family member.
    /// </summary>
    [DisplayName( "Family Member Entry" )]
    [Category( "com_centralaz > Widgets" )]
    [Description( "Block allows users to add or edit a family member." )]

    [DefinedValueField( "2E6540EA-63F0-40FE-BE50-F2A84735E600", "Connection Status", "The connection status to use for new individuals (default: 'Web Prospect'.)", true, false, "368DD475-242C-49C4-A42C-7278BE690CC2" )]
    [DefinedValueField( "8522BADD-2871-45A5-81DD-C76DA07E2E7E", "Record Status", "The record status to use for new individuals (default: 'Pending'.)", true, false, "283999EC-7346-42E3-B807-BCE9B2BABB49" )]
    public partial class FamilyMemberEntry : Rock.Web.UI.RockBlock
    {
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
        protected override void OnLoad( System.EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetails();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            var personId = PageParameter( "PersonId" ).AsIntegerOrNull();
            if ( personId != null && personId > 0 )
            {
                var person = new PersonService( rockContext ).Get( personId.Value );
                if ( person != null )
                {
                    var changes = new List<string>();

                    History.EvaluateChange( changes, "First Name", person.FirstName, tbFirstName.Text );
                    person.FirstName = tbFirstName.Text;

                    History.EvaluateChange( changes, "Last Name", person.LastName, tbLastName.Text );
                    person.LastName = tbLastName.Text;

                    var newGender = rblGender.SelectedValue.ConvertToEnum<Gender>();
                    History.EvaluateChange( changes, "Gender", person.Gender, newGender );
                    person.Gender = newGender;

                    var birthMonth = person.BirthMonth;
                    var birthDay = person.BirthDay;
                    var birthYear = person.BirthYear;

                    var birthday = bdaypBirthDay.SelectedDate;
                    if ( birthday.HasValue )
                    {
                        person.BirthMonth = birthday.Value.Month;
                        person.BirthDay = birthday.Value.Day;
                        if ( birthday.Value.Year != DateTime.MinValue.Year )
                        {
                            person.BirthYear = birthday.Value.Year;
                        }
                    }

                    History.EvaluateChange( changes, "Birth Month", birthMonth, person.BirthMonth );
                    History.EvaluateChange( changes, "Birth Day", birthDay, person.BirthDay );
                    History.EvaluateChange( changes, "Birth Year", birthYear, person.BirthYear );

                    if ( rockContext.SaveChanges() > 0 )
                    {
                        if ( changes.Any() )
                        {
                            HistoryService.SaveChanges(
                                rockContext,
                                typeof( Person ),
                                Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                                person.Id,
                                changes );
                        }
                    }
                }
            }
            else
            {
                CreatePerson();
            }
            NavigateToParentPage();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            var rockContext = new RockContext();
            var groupId = PageParameter( "FamilyId" ).AsIntegerOrNull();
            if ( groupId != null )
            {
                var group = new GroupService( rockContext ).Get( groupId.Value );
                if ( group != null )
                {
                    bool familyAdult = false;
                    if ( group.Members.Any( m => m.PersonId == CurrentPersonId && m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) ) )
                    {
                        familyAdult = true;
                    }

                    if ( group.IsAuthorized( Authorization.EDIT, CurrentPerson ) || familyAdult )
                    {
                        var personId = PageParameter( "PersonId" ).AsIntegerOrNull();
                        if ( personId != null && personId > 0 )
                        {
                            var person = new PersonService( rockContext ).Get( personId.Value );
                            if ( person != null )
                            {
                                tbFirstName.Text = person.FirstName;
                                tbLastName.Text = person.LastName;
                                bdaypBirthDay.SelectedDate = person.BirthDate;
                                rblGender.SelectedValue = person.Gender.ConvertToString( false );
                            }
                        }
                    }
                    else
                    {
                        pnlDetails.Visible = false;
                        nbMessage.Text = "You are not authorized to view this page.";
                        nbMessage.Visible = true;
                    }
                }
                else
                {
                    pnlDetails.Visible = false;
                    nbMessage.Text = "No group has been selected.";
                    nbMessage.Visible = true;
                }
            }
            else
            {
                pnlDetails.Visible = false;
                nbMessage.Text = "No group has been selected.";
                nbMessage.Visible = true;
            }


        }

        /// <summary>
        /// Creates the person.
        /// </summary>
        /// <returns></returns>
        private Person CreatePerson()
        {
            var rockContext = new RockContext();

            DefinedValueCache dvcConnectionStatus = DefinedValueCache.Read( GetAttributeValue( "ConnectionStatus" ).AsGuid() );
            DefinedValueCache dvcRecordStatus = DefinedValueCache.Read( GetAttributeValue( "RecordStatus" ).AsGuid() );

            Person person = new Person();
            person.FirstName = tbFirstName.Text;
            person.LastName = tbLastName.Text;
            person.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
            if ( dvcConnectionStatus != null )
            {
                person.ConnectionStatusValueId = dvcConnectionStatus.Id;
            }

            if ( dvcRecordStatus != null )
            {
                person.RecordStatusValueId = dvcRecordStatus.Id;
            }

            person.Gender = rblGender.SelectedValue.ConvertToEnum<Gender>();

            var birthday = bdaypBirthDay.SelectedDate;
            if ( birthday.HasValue )
            {
                person.BirthMonth = birthday.Value.Month;
                person.BirthDay = birthday.Value.Day;
                if ( birthday.Value.Year != DateTime.MinValue.Year )
                {
                    person.BirthYear = birthday.Value.Year;
                }
            }
            var familyGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
            if ( familyGroupType != null )
            {
                var adultRole = familyGroupType.Roles
                        .FirstOrDefault( r =>
                            r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) );

                var childRole = familyGroupType.Roles
                    .FirstOrDefault( r =>
                        r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ) );

                var age = person.Age;

                var familyRole = age.HasValue && age < 18 ? childRole : adultRole;

                PersonService.AddPersonToFamily( person, true, PageParameter( "FamilyId" ).AsInteger(), familyRole.Id, rockContext );
            }
            return person;
        }

        #endregion

    }
}