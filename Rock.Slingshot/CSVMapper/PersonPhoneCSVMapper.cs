using System.Collections.Generic;
using Rock;

public class PersonPhoneCSVMapper
{
    public static List<Slingshot.Core.Model.PersonPhone> Map( IDictionary<string, object> csvEntryLookup, Dictionary<string, string> csvHeaderMapper )
    {
        var personPhones = new List<Slingshot.Core.Model.PersonPhone>();

        string csvColumnId = csvHeaderMapper["Id"];
        int personId = csvEntryLookup[csvColumnId].ToIntSafe();


        {
            if ( csvHeaderMapper.TryGetValue( "Home Phone", out string csvColumnPhone ) )
            {
                var personHomePhone = new Slingshot.Core.Model.PersonPhone();
                personHomePhone.PersonId = personId;
                personHomePhone.PhoneNumber = csvEntryLookup[csvColumnPhone].ToStringSafe();
                personHomePhone.PhoneType = "Home";
                personPhones.Add( personHomePhone );
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Mobile Phone", out string csvColumnPhone ) )
            {
                var personMobilePhone = new Slingshot.Core.Model.PersonPhone();
                personMobilePhone.PersonId = personId;
                personMobilePhone.PhoneNumber = csvEntryLookup[csvColumnPhone].ToStringSafe();
                personMobilePhone.PhoneType = "Mobile";
                personPhones.Add( personMobilePhone );
            }
        }

        return personPhones;
    }

}