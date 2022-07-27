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