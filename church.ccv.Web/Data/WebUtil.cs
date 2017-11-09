using church.ccv.Web.Model;
using Rock;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Security;
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
        public static bool CreatePersonWithLogin( PersonWithLoginModel personWithLoginModel, out Person newPerson, out UserLogin newLogin )
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
                newPerson.FirstName = personWithLoginModel.FirstName.Trim();
                newPerson.LastName = personWithLoginModel.LastName.Trim();

                newPerson.Email = personWithLoginModel.Email.Trim();
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
                                    personWithLoginModel.Username,
                                    personWithLoginModel.Password,
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

        public static void SendForgotPasswordEmail( string confirmAccountUrl, string forgotPasswordEmailTemplateGuid, string appUrlWithRoot, string themeUrlWithRoot, string personEmail )
        {
            // setup merge fields
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null );
            mergeFields.Add( "ConfirmAccountUrl", confirmAccountUrl );
            var results = new List<IDictionary<string, object>>();

            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var userLoginService = new UserLoginService( rockContext );

            // get all the accounts associated with the person(s) using this email address
            bool hasAccountWithPasswordResetAbility = false;
            List<string> accountTypes = new List<string>();
                
            foreach ( Person person in personService.GetByEmail( personEmail )
                .Where( p => p.Users.Any()))
            {
                var users = new List<UserLogin>();
                foreach ( UserLogin user in userLoginService.GetByPersonId( person.Id ) )
                {
                    if ( user.EntityType != null )
                    {
                        var component = AuthenticationContainer.GetComponent( user.EntityType.Name );
                        if ( !component.RequiresRemoteAuthentication )
                        {
                            users.Add( user );
                            hasAccountWithPasswordResetAbility = true;
                        }

                        accountTypes.Add( user.EntityType.FriendlyName );
                    }
                }

                var resultsDictionary = new Dictionary<string, object>();
                resultsDictionary.Add( "Person", person);
                resultsDictionary.Add( "Users", users );
                results.Add( resultsDictionary );
            }

            // if we found user accounts that were valid, send the email
            if ( results.Count > 0 && hasAccountWithPasswordResetAbility )
            {
                mergeFields.Add( "Results", results.ToArray( ) );
                var recipients = new List<RecipientData>();
                recipients.Add( new RecipientData( personEmail, mergeFields ) );

                Email.Send( new Guid( forgotPasswordEmailTemplateGuid ), recipients, appUrlWithRoot, themeUrlWithRoot, false );
            }
        }
    }
}
