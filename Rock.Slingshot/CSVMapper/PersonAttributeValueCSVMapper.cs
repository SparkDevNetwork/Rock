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
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

public class PersonAttributeValueCsvMapper
{
    public static List<Slingshot.Core.Model.PersonAttributeValue> Map( IDictionary<string, object> csvEntryLookup, Dictionary<string, string> csvHeaderMapper )
    {
        var personAttributeValues = new List<Slingshot.Core.Model.PersonAttributeValue>();

        int personId = csvEntryLookup[csvHeaderMapper["Id"]].ToIntSafe();

        IEnumerable<string> rockPersonAttributeKeys = AttributeCache.GetPersonAttributes()
            .Select( a => a.Name );

        foreach ( var rockPersonAttributeKey in rockPersonAttributeKeys )
        {
            if ( !csvHeaderMapper.TryGetValue( rockPersonAttributeKey, out string csvColumnRockAttributeKey ) )
            {
                continue;
            }
            string attributeValue = csvEntryLookup
                .GetValueOrNull( csvColumnRockAttributeKey )
                .ToStringSafe();

            var personAttributeValue = new Slingshot.Core.Model.PersonAttributeValue
            {
                AttributeKey = rockPersonAttributeKey,
                AttributeValue = attributeValue,
                PersonId = personId
            };
            personAttributeValues.Add( personAttributeValue );
        }

        return personAttributeValues;
    }
}