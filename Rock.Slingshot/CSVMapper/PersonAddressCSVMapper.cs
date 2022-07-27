// <copyright>
// Copyright by the Spark Development Network
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
