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
        public static HttpStatusCode RegisterNewPerson( RegAccountData regAccountData )
        {
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;

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
                Person person = new Person( );
                
                // for new people, copy the stuff sent by the Mobile App
                person.FirstName = regAccountData.FirstName.Trim();
                person.LastName = regAccountData.LastName.Trim();

                person.Email = regAccountData.Email.Trim();
                person.IsEmailActive = string.IsNullOrWhiteSpace( person.Email ) == false ? true : false;
                person.EmailPreference = EmailPreference.EmailAllowed;
                
                // now set values so it's a Person Record Type, and pending web prospect.
                person.ConnectionStatusValueId = connectionStatusWebProspect.Id;
                person.RecordStatusValueId = recordStatusPending.Id;
                person.RecordTypeValueId = recordTypePerson.Id;

                // now, save the person so that all the extra stuff (known relationship groups) gets created.
                Group newFamily = PersonService.SaveNewPerson( person, rockContext );
                
                // set the phone number (we only support Cell Phone for the Mobile App)
                if( string.IsNullOrWhiteSpace( regAccountData.CellPhoneNumber ) == false )
                {
                    person.UpdatePhoneNumber( cellPhoneType.Id, PhoneNumber.DefaultCountryCode(), regAccountData.CellPhoneNumber, null, null, rockContext );
                }

                // save all changes
                person.SaveAttributeValues( rockContext );
                rockContext.SaveChanges( );
                
                
                // and now create the login for this person
                try
                {
                    UserLogin login = UserLoginService.Create(
                                    rockContext,
                                    person,
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
                
                // report that we created the person / login
                statusCode = HttpStatusCode.Created;
            }
            while( false );
            
            return statusCode;
        }
    }
}
