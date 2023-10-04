using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Transactions;

namespace org.lakepointe.MinistryPoint.Transactions
{
    public class CreateMinistryPointGroupTransaction : ITransaction
    {
        public int RegistrationId { get; set; }
        public int ParentGroupId { get; set; }
        public Guid? DefaultRecordStatusGuid { get; set; }
        public Guid? DefaultConnectionStatusGuid { get; set; }
        public Guid? DefaultCampusGuid { get; set; }
        public string AppRoot { get; set; }
        public string ThemeRoot { get; set; }



        public void Execute()
        {
            var registration = LoadRegistration( RegistrationId );


            if ( ParentGroupId <= 0 )
            {
                throw new Exception( "Parent Group Id is required." );
            }

            if ( registration == null )
            {
                return;
            }

            MinistryPointGroup group = new MinistryPointGroup();
            group.RegistrationId = registration.Id;
            group.OrganizationName = registration.GetAttributeValue( "OrganizationName" );
            group.AccountContactPersonAliasId = registration.PersonAliasId;

            group.CreateNewGroup( ParentGroupId );

            var attributeMatrixGuid = registration.GetAttributeValue( "AdditionalUsers" ).AsGuidOrNull();
            var additionalUsers = new List<RegistrationAdditionalUser>();

            var rockPersonDefaults = new RockPersonDefaultSettings()
            {
                CampusGuid = DefaultCampusGuid,
                ConnectionStatusGuid = DefaultConnectionStatusGuid,
                RecordStatusGuid = DefaultRecordStatusGuid
            };

            if(attributeMatrixGuid.HasValue)
            {
                using ( var rockContext = new RockContext()  )
                {
                    var attributeMatrix = new AttributeMatrixService( rockContext ).Get( attributeMatrixGuid.Value );

                    foreach ( var matrixItem in attributeMatrix.AttributeMatrixItems )
                    {
                        matrixItem.LoadAttributes();
                        var additionalUser = new RegistrationAdditionalUser( matrixItem );
                        var person = additionalUser.GetRockPerson( rockContext, rockPersonDefaults, true );
                        group.AddMember( person.Id );

                        var pinEntityType = Rock.Web.Cache.EntityTypeCache.Get( typeof( Rock.Security.Authentication.PINAuthentication ) );

                        var login = person.Users.Where( u => u.EntityTypeId != pinEntityType.Id ).FirstOrDefault();
                        string password = String.Empty;

                        if ( login == null )
                        {
                            login = CreateUser( person, rockContext, out password  );
                        }

                        group.SendWelcomeEmail( person, login.UserName, password, AppRoot, ThemeRoot );
                    }
                }

            }
        }

        private UserLogin CreateUser( Person p, RockContext rockContext, out string password )
        {
            password = string.Empty;
            if ( p == null )
            {
                throw new Exception( "Person is required." );
            }
            int databaseLoginEntityTypeId = Rock.Web.Cache.EntityTypeCache.Get( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id;
            password = System.Web.Security.Membership.GeneratePassword( 9, 1 );
            string username = Rock.Security.Authentication.Database.GenerateUsername( p.FirstName, p.LastName );

            UserLogin login = UserLoginService.Create(
                    rockContext,
                    p,
                    AuthenticationServiceType.Internal,
                    databaseLoginEntityTypeId,
                    username,
                    password,
                    true,
                    true );
            return login;
        }

        private Registration LoadRegistration(int registrationId)
        {
            using ( var context = new RockContext() )
            {
                var registration = new RegistrationService( context ).Get( registrationId );

                if ( registration != null )
                {
                    registration.LoadAttributes( context );
                }

                return registration;
            }
        }
    }
}
