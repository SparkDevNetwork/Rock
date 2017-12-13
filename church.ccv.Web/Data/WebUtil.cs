using church.ccv.Web.Model;
using RestSharp;
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
using System.Web.Security;

namespace church.ccv.Web.Data
{
    class WebUtil
    {
        public static LoginResponse Login( LoginData loginData )
        {
            LoginResponse loginResponse = LoginResponse.Invalid;

            try
            {
                RockContext rockContext = new RockContext( );
                var userLoginService = new UserLoginService(rockContext);

                var userLogin = userLoginService.GetByUserName( loginData.Username );

                var component = AuthenticationContainer.GetComponent(userLogin.EntityType.Name);
                if ( component.IsActive && !component.RequiresRemoteAuthentication )
                {
                    // see if the credentials are valid
                    if ( component.Authenticate( userLogin, loginData.Password ) )
                    {
                        // if the account isn't locked or needing confirmation
                        if ( ( userLogin.IsConfirmed ?? true ) && !( userLogin.IsLockedOut ?? false ) )
                        {
                            // then proceed to the final step, validating them with PMG2's site
                            if ( PMG2Login( loginData.Username, loginData.Password ) )
                            {
                                // generate their cookie
                                UserLoginService.UpdateLastLogin( loginData.Username );
                                Rock.Security.Authorization.SetAuthCookie( loginData.Username, bool.Parse( loginData.Persist ), false );

                                // no issues!
                                loginResponse = LoginResponse.Success;
                            }
                        }
                        else
                        {
                            if ( userLogin.IsLockedOut ?? false )
                            {
                                loginResponse = LoginResponse.LockedOut;
                            }
                            else
                            {
                                loginResponse = LoginResponse.Confirm;
                            }
                        }
                    }
                }
            }
            catch
            {
                // fail on exception
            }

            return loginResponse;
        }

        public static bool Logout( Uri refererUri, int? pageId )
        {
            // assume we should not advise the browser to redirect the user,
            // but after logging out, if they are on a page that's not allowed, this will be set to true.
            bool shouldRedirectUser = false;

            UserLogin loggedIn = UserLoginService.GetCurrentUser( );

            if( loggedIn != null )
            {
                var transaction = new Rock.Transactions.UserLastActivityTransaction();
                transaction.UserId = loggedIn.Id;
                transaction.LastActivityDate = RockDateTime.Now;
                transaction.IsOnLine = false;
                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
            }
            
            FormsAuthentication.SignOut();

            string localPath = refererUri.LocalPath;

            if( pageId.HasValue )
            {
                // if the page they claim they're visiting is NOT valid, we need to send them to the default page.
                var currentPage = Rock.Web.Cache.PageCache.Read( pageId.Value );
                if ( currentPage == null || currentPage.IsAuthorized( Rock.Security.Authorization.VIEW, null ) == false )
                {
                    shouldRedirectUser = true;
                }
            }

            return shouldRedirectUser;
        }

        protected static bool PMG2Login( string username, string password )
        {
            // contact PMG2's site and attempt to login with the same credentials
            string pmg2RootSite = GlobalAttributesCache.Value( "PMG2Server" );
            
            var restClient = new RestClient(
                string.Format( pmg2RootSite + "auth?user[username]={0}&user[password]={1}", username, password ) );

            var restRequest = new RestRequest( Method.POST );
            var restResponse = restClient.Execute( restRequest );

            if ( restResponse.StatusCode == HttpStatusCode.Created )
            {
                return true;
            }

            return false;
        }

        public static bool CreatePersonWithLogin( PersonWithLoginModel personWithLoginModel )
        {
            bool success = false;

            try
            {            
                RockContext rockContext = new RockContext( );
                PersonService personService = new PersonService( rockContext );
                GroupService groupService = new GroupService( rockContext );

                // get all required values and make sure they exist
                DefinedValueCache connectionStatusWebProspect = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_WEB_PROSPECT);
                DefinedValueCache recordStatusPending = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING);
                DefinedValueCache recordTypePerson = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON);
                                
                // create a new person, which will give us a new Id
                Person newPerson = new Person( );
                
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
                UserLogin newLogin = UserLoginService.Create(
                                rockContext,
                                newPerson,
                                Rock.Model.AuthenticationServiceType.Internal,
                                EntityTypeCache.Read( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                                personWithLoginModel.Username,
                                personWithLoginModel.Password,
                                true,
                                false );
                    
                // email them confirmation
                var mergeObjects = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null );
                mergeObjects.Add( "ConfirmAccountUrl", personWithLoginModel.ConfirmAccountUrl );
                mergeObjects.Add( "Person", newPerson );
                mergeObjects.Add( "User", newLogin );

                var recipients = new List<RecipientData>();
                recipients.Add( new RecipientData( newPerson.Email, mergeObjects ) );

                Email.Send( new Guid( personWithLoginModel.AccountCreatedEmailTemplateGuid ), recipients, personWithLoginModel.AppUrl, personWithLoginModel.ThemeUrl, false );

                success = true;
            }
            catch
            {
                // fail on exception
            }

            return success;
        }

        public static CreateLoginModel.Response CreateLogin( CreateLoginModel createLoginModel )
        {
            CreateLoginModel.Response response = CreateLoginModel.Response.Failed;

            try
            {
                RockContext rockContext = new RockContext();

                // start by getting the person being worked on
                PersonService personService = new PersonService( rockContext );
                Person person = personService.Get( createLoginModel.PersonId );
                
                // now, see if the person already has ANY logins attached
                var userLoginService = new Rock.Model.UserLoginService( rockContext );
                var userLogins = userLoginService.GetByPersonId( createLoginModel.PersonId ).ToList();
                if ( userLogins.Count == 0 )
                {
                    // and create a new, UNCONFIRMED login for them.
                    var newLogin = UserLoginService.Create(
                                    rockContext,
                                    person,
                                    Rock.Model.AuthenticationServiceType.Internal,
                                    EntityTypeCache.Read( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                                    createLoginModel.Username,
                                    createLoginModel.Password,
                                    false,
                                    false );

                    WebUtil.SendConfirmAccountEmail( newLogin, createLoginModel.ConfirmAccountUrl, createLoginModel.ConfirmAccountEmailTemplateGuid, createLoginModel.AppUrl, createLoginModel.ThemeUrl );

                    response = CreateLoginModel.Response.Created;
                }
                else
                {
                    // they DO have a login, so simply email the person being worked on a list of them.
                    response = CreateLoginModel.Response.Emailed;

                    WebUtil.SendForgotPasswordEmail( person.Email, createLoginModel.ConfirmAccountUrl, createLoginModel.ForgotPasswordEmailTemplateGuid, createLoginModel.AppUrl, createLoginModel.ThemeUrl );
                }
            }
            catch
            {
                // fail on exception
            }

            return response;
        }

        public static List<DuplicatePersonInfo> GetDuplicates( string lastName, string email )
        {
            List<DuplicatePersonInfo> duplicateList = new List<DuplicatePersonInfo>( );

            // first, see if there's already a person with this matching last name and email
            PersonService personService = new PersonService( new RockContext() );
            var matches = personService.Queryable().Where( p =>
                    p.Email.ToLower() == email.ToLower() && p.LastName.ToLower() == lastName.ToLower() ).ToList();

            // add all duplicates to our list
            foreach ( Person match in matches )
            {
                DuplicatePersonInfo duplicateInfo = new DuplicatePersonInfo( )
                {
                    Id = match.Id,
                    FullName = match.FullName,
                    Birthday = match.BirthDate.HasValue ? match.BirthDate.Value.ToString("MMMM") + " " + match.BirthDay : ""
                };

                duplicateList.Add( duplicateInfo );
            }

            return duplicateList;
        }
        
        public static void SendConfirmAccountEmail( UserLogin userLogin, string confirmAccountUrl, string confirmAccountEmailTemplateGuid, string appUrl, string themeUrl )
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null );
            mergeFields.Add( "ConfirmAccountUrl", confirmAccountUrl );

            var personDictionary = userLogin.Person.ToLiquid() as Dictionary<string, object>;
            mergeFields.Add( "Person", personDictionary );
            mergeFields.Add( "User", userLogin );

            var recipients = new List<RecipientData>();
            recipients.Add( new RecipientData( userLogin.Person.Email, mergeFields ) );

            Email.Send( new Guid( confirmAccountEmailTemplateGuid ), recipients, appUrl, themeUrl, false );
        }

        public static void SendForgotPasswordEmail( string personEmail, string confirmAccountUrl, string forgotPasswordEmailTemplateGuid, string appUrl, string themeUrl )
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

                Email.Send( new Guid( forgotPasswordEmailTemplateGuid ), recipients, appUrl, themeUrl, false );
            }
        }
    }
}
