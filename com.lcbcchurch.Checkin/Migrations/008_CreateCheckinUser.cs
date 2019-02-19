// <copyright>
// Copyright by LCBC Church
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Plugin;
using Rock.Web.Cache;

namespace com.lcbcchurch.Checkin.Migrations
{
    [MigrationNumber( 8, "1.0.14" )]
    class CreateCheckinUser : Migration
    {
        private const string FIRST_NAME = "Checkin";
        private const string LAST_NAME = "Checkin";
        private const string EMAIL = "checkin@lcbcchurch.com";
        private const string CHECKIN_USERNAME = "checkin";
        private const string CHECKIN_PASSWORD = "aide-rubicund-royalist-algiers-rummy";
        private static int _authenticationDatabaseEntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id;

        public override void Up()
        {
            RockContext rockContext = new RockContext();
            var userLoginService = new UserLoginService( rockContext );

            var userLogin = userLoginService.GetByUserName( CHECKIN_USERNAME );

            // only create the login if the username is not already taken
            if (userLogin == null)
            {
                PersonService personService = new PersonService( rockContext );
                Person person = personService.GetByFullName( "Checkin Checkin", false ).FirstOrDefault();
                if (person == null) // should be when it's a fresh install
                {
                    // Create new person with very simple profile. Just need name to be "Checkin Checkin"
                    var personRecordTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                    var personStatusActive = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
                    var personConnectionStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR.AsGuid() ).Id;

                    person = new Person();
                    person.IsSystem = false;
                    person.RecordTypeValueId = personRecordTypeId;
                    person.RecordStatusValueId = personStatusActive;
                    person.ConnectionStatusValueId = personConnectionStatus;
                    person.FirstName = FIRST_NAME;
                    person.LastName = LAST_NAME;
                    person.Email = EMAIL;
                    person.IsEmailActive = true;
                    person.EmailPreference = EmailPreference.DoNotEmail;

                    if (person != null)
                    {
                        PersonService.SaveNewPerson( person, rockContext, null, false );
                        person = personService.Get( person.Id );
                    }
                } 

                // Person exists, create a login for them
                UserLoginService.Create(
                                       rockContext,
                                       person,
                                       Rock.Model.AuthenticationServiceType.Internal,
                                       _authenticationDatabaseEntityTypeId,
                                       CHECKIN_USERNAME,
                                       CHECKIN_PASSWORD,
                                       isConfirmed: true );

                // Add Checkin user to the APP - Check-in Devices role
                Sql( string.Format( @"
                    DECLARE @AppCheckInDevicesGroupId INT
                    SET @AppCheckInDevicesGroupId = (SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '51E02A99-B7CB-4E64-B7C8-065076AABC05')

                    DECLARE @GroupRoleId INT
                    SET @GroupRoleId = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = '00f3ac1c-71b9-4ee5-a30e-4c48c8a0bf1f')

                    INSERT INTO [GroupMember]
                    ([IsSystem]
                        , [GroupId]
                        , [PersonId]
                        , [GroupRoleId]
                        , [GroupMemberStatus]
                        , [Guid]
                    )
                    VALUES (
                        1
                        , @AppCheckInDevicesGroupId
                        , '{0}'
                        , @GroupRoleId
                        , 1
                        , '7466bb69-4dc9-443a-a17b-80d265532183'
                    )", person.Id) );
            }
        }

        public override void Down()
        {
            // Delete checkin person from 'APP - Check-in Devices' group
            RockMigrationHelper.DeleteByGuid( "7466bb69-4dc9-443a-a17b-80d265532183", "GroupMember"); 

            // Delete checkin User Login
            Sql( @"
                DELETE FROM [UserLogin]
                WHERE [Username] = 'checkin'
            " );
        }
    }
}
