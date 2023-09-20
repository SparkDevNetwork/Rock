using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace org.lakepointe.MinistryPoint
{
    public class RegistrationAdditionalUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MobilePhone { get; set; }

        public RegistrationAdditionalUser()
        {

        }

        public RegistrationAdditionalUser( AttributeMatrixItem item )
        {
            LoadUserItem( item );
        }


        public Person GetRockPerson( RockContext context, RockPersonDefaultSettings settings, bool createLogin = true )
        {
            if ( FirstName.IsNullOrWhiteSpace() || LastName.IsNullOrWhiteSpace() )
            {
                throw new Exception( "First Name and Last Name is required." );
            }

            if ( Email.IsNullOrWhiteSpace() && MobilePhone.IsNullOrWhiteSpace())
            {
                throw new Exception( "Email or Mobile Phone is required");
            }

            Person person = null;
            var personService = new PersonService( context );

            var personQuery = new PersonService.PersonMatchQuery( FirstName, LastName, Email, MobilePhone );
            person = personService.FindPerson( personQuery, true );

            if ( person != null )
            {
                person = new Person();
                person.FirstName = FirstName.FixCase();
                person.LastName = LastName.FixCase();
                person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;

                if ( MobilePhone.IsNotNullOrWhiteSpace() )
                {
                    var phone = new PhoneNumber();
                    phone.NumberTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ).Id;
                    phone.Number = PhoneNumber.CleanNumber( MobilePhone );
                    phone.NumberFormatted = PhoneNumber.FormattedNumber( "1", MobilePhone, false );
                    person.PhoneNumbers.Add( phone );
                }

                if ( settings.RecordStatusGuid.HasValue )
                {
                    person.RecordStatusValueId = DefinedValueCache.Get( settings.RecordStatusGuid.Value ).Id;
                }

                if ( settings.ConnectionStatusGuid.HasValue )
                {
                    person.ConnectionStatusValueId = DefinedValueCache.Get( settings.ConnectionStatusGuid.Value ).Id;
                }

                if ( Email.IsNotNullOrWhiteSpace() )
                {
                    person.Email = Email;
                    person.EmailPreference = EmailPreference.EmailAllowed;
                    person.IsEmailActive = true;
                }

                int? defaultCampusId = null;
                if ( settings.CampusGuid.HasValue )
                {
                    defaultCampusId = CampusCache.Get( settings.CampusGuid.Value ).Id;
                }

                var familyGroup = PersonService.SaveNewPerson( person, context, defaultCampusId );
                if ( familyGroup != null && familyGroup.Members.Any() )
                {
                    person = familyGroup.Members.Select( m => m.Person ).First();
                }

            }

            return person;
        }

        private void LoadUserItem( AttributeMatrixItem item )
        {
            FirstName = item.GetAttributeValue( "FirstName" );
            LastName = item.GetAttributeValue( "LastName" );
            Email = item.GetAttributeValue( "EmailAddress" );
            MobilePhone = item.GetAttributeValue( "MobilePhone" );
        }



    }

    public class RockPersonDefaultSettings
    {
        public Guid? CampusGuid { get; set; }
        public Guid? ConnectionStatusGuid { get; set; }
        public Guid? RecordStatusGuid { get; set; }
        
    }
}
