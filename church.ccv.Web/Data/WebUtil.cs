using church.ccv.Web.Model;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace church.ccv.Web.Data
{
    class WebUtil
    {
        public static bool RegisterNewPerson( RegAccountData regAccountData, out Person newPerson, out UserLogin newLogin )
        {
            bool success = false;

            newPerson = null;
            newLogin = null;

            do
            {
                RockContext rockContext = new RockContext( );
                PersonService personService = new PersonService( rockContext );
                GroupService groupService = new GroupService( rockContext );

                // get all required values and make sure they exist
                DefinedValueCache cellPhoneType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
                DefinedValueCache connectionStatusWebProspect = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_WEB_PROSPECT);
                DefinedValueCache recordStatusPending = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING);
                DefinedValueCache recordTypePerson = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON);
                
                if( cellPhoneType == null || connectionStatusWebProspect == null || recordStatusPending == null || recordTypePerson == null ) break;
                                
                // create a new person, which will give us a new Id
                newPerson = new Person( );
                
                // for new people, copy the stuff sent by the Mobile App
                newPerson.FirstName = regAccountData.FirstName.Trim();
                newPerson.LastName = regAccountData.LastName.Trim();

                newPerson.Email = regAccountData.Email.Trim();
                newPerson.IsEmailActive = string.IsNullOrWhiteSpace( newPerson.Email ) == false ? true : false;
                newPerson.EmailPreference = EmailPreference.EmailAllowed;
                
                // now set values so it's a Person Record Type, and pending web prospect.
                newPerson.ConnectionStatusValueId = connectionStatusWebProspect.Id;
                newPerson.RecordStatusValueId = recordStatusPending.Id;
                newPerson.RecordTypeValueId = recordTypePerson.Id;

                // now, save the person so that all the extra stuff (known relationship groups) gets created.
                Group newFamily = PersonService.SaveNewPerson( newPerson, rockContext );
                
                // save all changes
                newPerson.SaveAttributeValues( rockContext );
                rockContext.SaveChanges( );
                
                
                // and now create the login for this person
                try
                {
                    newLogin = UserLoginService.Create(
                                    rockContext,
                                    newPerson,
                                    Rock.Model.AuthenticationServiceType.Internal,
                                    EntityTypeCache.Read( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                                    regAccountData.Username,
                                    regAccountData.Password,
                                    true,
                                    false );
                }
                catch
                {
                    // fail on exception
                    break;
                }
                
                success = true;
            }
            while( false );

            return success;
        }
    }
}
