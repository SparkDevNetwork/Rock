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

// Alias Slingshot.Core namespace to avoid conflict with Rock.Slingshot.*
using SlingshotCore = global::Slingshot.Core;

namespace Rock.Slingshot
{
    public class PersonAddressCsvMapper
    {
        public static SlingshotCore.Model.PersonAddress Map( IDictionary<string, object> csvEntryLookup, Dictionary<string, string> csvHeaderMapper )
        {
            var personAddress = new SlingshotCore.Model.PersonAddress
            {
                PersonId = csvEntryLookup[csvHeaderMapper[CSVHeaders.Id]].ToIntSafe(),
                AddressType = SlingshotCore.Model.AddressType.Home
            };


            var csvColumnStreet1 = csvHeaderMapper.GetValueOrNull( CSVHeaders.HomeAddressStreet1 );
            if ( csvColumnStreet1 != null )
            {
                personAddress.Street1 = csvEntryLookup[csvColumnStreet1].ToStringSafe();
            }

            var csvColumnStreet2 = csvHeaderMapper.GetValueOrNull( CSVHeaders.HomeAddressStreet2 );
            if ( csvColumnStreet2 != null )
            {
                personAddress.Street2 = csvEntryLookup[csvColumnStreet2].ToStringSafe();
            }

            var csvColumnCity = csvHeaderMapper.GetValueOrNull( CSVHeaders.HomeAddressCity );
            if ( csvColumnCity != null )
            {
                personAddress.City = csvEntryLookup[csvColumnCity].ToStringSafe();
            }

            var csvColumnState = csvHeaderMapper.GetValueOrNull( CSVHeaders.HomeAddressState );
            if ( csvColumnState != null )
            {
                personAddress.State = csvEntryLookup[csvColumnState].ToStringSafe();
            }

            var csvColumnPostalCode = csvHeaderMapper.GetValueOrNull( CSVHeaders.HomeAddressPostalCode );
            if ( csvColumnPostalCode != null )
            {
                personAddress.PostalCode = csvEntryLookup[csvColumnPostalCode].ToStringSafe();
            }

            var csvColumnCountry = csvHeaderMapper.GetValueOrNull( CSVHeaders.HomeAddressCountry );
            if ( csvColumnCountry != null )
            {
                personAddress.Country = csvEntryLookup[csvColumnCountry].ToStringSafe();
            }

            return personAddress;
        }
    }
}