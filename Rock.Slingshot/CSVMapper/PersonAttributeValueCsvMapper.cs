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
using Rock.Web.Cache;

// Alias Slingshot.Core namespace to avoid conflict with Rock.Slingshot.*
using SlingshotCore = global::Slingshot.Core;

namespace Rock.Slingshot
{
    public class PersonAttributeValueCsvMapper
    {
        public static List<SlingshotCore.Model.PersonAttributeValue> Map( IDictionary<string, object> csvEntryLookup, Dictionary<string, string> csvHeaderMapper )
        {
            var personAttributeValues = new List<SlingshotCore.Model.PersonAttributeValue>();

            int personId = csvEntryLookup[csvHeaderMapper[CSVHeaders.Id]].ToIntSafe();

            IEnumerable<string> rockPersonAttributeKeys = AttributeCache.GetPersonAttributes()
                .Select( a => a.Key );

            foreach ( var rockPersonAttributeKey in rockPersonAttributeKeys )
            {
                if ( !csvHeaderMapper.TryGetValue( rockPersonAttributeKey, out string csvColumnRockAttributeKey ) )
                {
                    continue;
                }

                string attributeValue = csvEntryLookup
                    .GetValueOrNull( csvColumnRockAttributeKey )
                    .ToStringSafe();

                // Skip empty values (otherwise it will overwrite existing values with empty string).
                if ( attributeValue.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                var personAttributeValue = new SlingshotCore.Model.PersonAttributeValue
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
}