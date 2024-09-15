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
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Core
{
    /// <summary>
    /// Functions to assist with core module testing.
    /// </summary>
    public class CoreDataManager
    {
        private static Lazy<CoreDataManager> _dataManager = new Lazy<CoreDataManager>();
        public static CoreDataManager Current => _dataManager.Value;

        /// <summary>
        /// Get a known Person who has been assigned a security role of Administrator.
        /// </summary>
        /// <param name="personService"></param>
        /// <returns></returns>
        public Person GetAdminPersonOrThrow( RockContext dataContext )
        {
            var personService = new PersonService( dataContext );

            return GetAdminPersonOrThrow( personService );
        }

        /// <summary>
        /// Get a known Person who has been assigned a security role of Administrator.
        /// </summary>
        /// <param name="personService"></param>
        /// <returns></returns>
        public Person GetAdminPersonOrThrow( PersonService personService )
        {
            var adminPerson = personService.Queryable().FirstOrDefault( x => x.FirstName == "Alisha" && x.LastName == "Marble" );

            if ( adminPerson == null )
            {
                throw new Exception( "Admin Person not found in test data set." );
            }

            return adminPerson;
        }

        #region Defined Types

        /// <summary>
        /// Gets the identifier of a defined value by value text, or returns null if the value is not found.
        /// </summary>
        /// <param name="definedTypeGuid"></param>
        /// <param name="definedValueText"></param>
        /// <returns></returns>
        public int? GetDefinedValueIdOrNull( string definedTypeGuid, string definedValueText )
        {
            var values = DefinedTypeCache.Get( definedTypeGuid.AsGuid() )
                ?.DefinedValues;
            var valueId = values?.FirstOrDefault( v => v.Value.Equals( definedValueText, StringComparison.OrdinalIgnoreCase ) )
                ?.Id;

            return valueId;
        }

        /// <summary>
        /// Add or update a Defined Value for the specified Defined Type.
        /// </summary>
        /// <param name="definedTypeId"></param>
        /// <param name="guid"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        public bool AddOrUpdateDefinedValue( int definedTypeId, string guid, string name, string description, RockContext rockContext )
        {
            // Add UTM Campaigns
            var definedValueService = new DefinedValueService( rockContext );

            var definedValue = definedValueService.Get( guid );
            if ( definedValue == null )
            {
                definedValue = new DefinedValue();
                definedValueService.Add( definedValue );
            }

            definedValue.Guid = guid.AsGuid();
            definedValue.DefinedTypeId = definedTypeId;
            definedValue.Value = name;
            definedValue.Description = description;

            return true;
        }

        #endregion
    }
}
