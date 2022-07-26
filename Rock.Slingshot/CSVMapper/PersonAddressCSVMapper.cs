using System.Collections.Generic;
using Rock;

class PersonAddressCSVMapper
{
    public static Slingshot.Core.Model.PersonAddress Map( IDictionary<string, object> csvEntryLookup, Dictionary<string, string> csvHeaderMapper )
    {
        var personAddress = new Slingshot.Core.Model.PersonAddress();

        string csvColumnId = csvHeaderMapper["Id"];
        personAddress.PersonId = csvEntryLookup[csvColumnId].ToIntSafe();

        personAddress.AddressType = Slingshot.Core.Model.AddressType.Home;

        {
            if ( csvHeaderMapper.TryGetValue( "Home Address Street 1", out string csvColumnStreet1 ) )
            {
                personAddress.Street1 = csvEntryLookup[csvColumnStreet1].ToStringSafe();
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Home Address Street 2", out string csvColumnStreet2 ) )
            {
                personAddress.Street2 = csvEntryLookup[csvColumnStreet2].ToStringSafe();
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Home Address City", out string csvColumnCity ) )
            {
                personAddress.City = csvEntryLookup[csvColumnCity].ToStringSafe();
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Home Address State", out string csvColumnState ) )
            {
                personAddress.State = csvEntryLookup[csvColumnState].ToStringSafe();
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Home Address Postal Code", out string csvColumnPostalCode ) )
            {
                personAddress.PostalCode = csvEntryLookup[csvColumnPostalCode].ToStringSafe();
            }
        }

        {
            if ( csvHeaderMapper.TryGetValue( "Home Address Country", out string csvColumnCountry ) )
            {
                personAddress.Country = csvEntryLookup[csvColumnCountry].ToStringSafe();
            }
        }

        return personAddress;
    }
}
