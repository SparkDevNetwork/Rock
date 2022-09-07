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
using System;
using System.Collections.Generic;

// Alias Slingshot.Core namespace to avoid conflict with Rock.Slingshot.*
using SlingshotCore = global::Slingshot.Core;

namespace Rock.Slingshot
{
    public class PersonPhoneCsvMapper
    {
        public static List<SlingshotCore.Model.PersonPhone> Map( IDictionary<string, object> csvEntryLookup, Dictionary<string, string> csvHeaderMapper, ref HashSet<string> parserErrors )
        {
            var personPhones = new List<SlingshotCore.Model.PersonPhone>();

            int personId = csvEntryLookup[csvHeaderMapper[CSVHeaders.Id]].ToIntSafe();

            var csvColumnHomePhone = csvHeaderMapper.GetValueOrNull( CSVHeaders.HomePhone );
            if ( csvColumnHomePhone != null )
            {
                var personHomePhone = new SlingshotCore.Model.PersonPhone
                {
                    PersonId = personId,
                    PhoneNumber = csvEntryLookup[csvColumnHomePhone].ToStringSafe(),
                    PhoneType = "Home"
                };
                personPhones.Add( personHomePhone );
            }

            var csvColumnMobilePhone = csvHeaderMapper.GetValueOrNull( CSVHeaders.MobilePhone );
            if ( csvColumnMobilePhone != null )
            {
                bool isSMSEnabled = false;
                var isSMSEnabledColumn = csvHeaderMapper.GetValueOrNull( CSVHeaders.IsSMSEnabled );
                if ( isSMSEnabledColumn != null)
                {
                    if( !Boolean.TryParse( csvEntryLookup[isSMSEnabledColumn].ToStringSafe(), out isSMSEnabled ) )
                    {
                        parserErrors.Add( $"Is SMS Enabled {csvEntryLookup[isSMSEnabledColumn]} is invalid defaulting to {isSMSEnabled}" );
                    }
                }

                var personMobilePhone = new SlingshotCore.Model.PersonPhone
                {
                    PersonId = personId,
                    PhoneNumber = csvEntryLookup[csvColumnMobilePhone].ToStringSafe(),
                    PhoneType = "Mobile",
                    IsMessagingEnabled = isSMSEnabled
                };

                personPhones.Add( personMobilePhone );
            }

            return personPhones;
        }
    }
}