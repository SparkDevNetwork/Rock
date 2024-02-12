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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Crm
{
    /// <summary>
    /// Functions to assist with testing the Rock CRM module.
    /// </summary>
    public class CrmModuleTestHelper
    {
        /// <summary>
        /// Adds or replaces a Person, and returns the new Family to which the new Person has been added.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public static Group AddOrReplacePerson( Person person, RockContext dataContext = null )
        {
            dataContext = dataContext ?? new RockContext();
            var personService = new PersonService( dataContext );

            Assert.IsNotNull( person.Guid, "Person.Guid is a required value." );

            var existingPerson = personService.Get( person.Guid );
            if ( existingPerson != null )
            {
                var aliasService = new PersonAliasService( dataContext );
                aliasService.DeleteRange( existingPerson.Aliases );

                var searchService = new PersonSearchKeyService( dataContext );
                searchService.DeleteRange( existingPerson.GetPersonSearchKeys( dataContext ).ToList() );
                personService.Delete( existingPerson );

                dataContext.SaveChanges();
            }

            var newFamily = PersonService.SaveNewPerson( person, dataContext );
            return newFamily;
        }
    }

    /// <summary>
    /// Manages global configuration settings for a Rock instance.
    /// </summary>
    public class GlobalSettingsDataManager
    {
        private static Lazy<GlobalSettingsDataManager> _instance = new Lazy<GlobalSettingsDataManager>();

        public static GlobalSettingsDataManager Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        /// <summary>
        /// Add or update a Phone Number Country Code definition.
        /// These definitions are used to parse and format phone numbers for different countries.
        /// </summary>
        /// <param name="countryCode"></param>
        /// <param name="description"></param>
        /// <param name="matchRegEx"></param>
        /// <param name="formatRegEx"></param>
        /// <returns></returns>
        public int AddOrUpdatePhoneNumberCountryCode( string countryCode, string description, string matchRegEx, string formatRegEx )
        {
            var rockContext = new RockContext();
            var definedTypeService = new DefinedTypeService( rockContext );
            var definedType = definedTypeService.Get( Rock.SystemGuid.DefinedType.COMMUNICATION_PHONE_COUNTRY_CODE.AsGuid() );

            if ( definedType == null )
            {
                throw new Exception( "Defined Type \"Phone Country Code\" not found." );
            }

            var countryCodeValue = definedType.DefinedValues.FirstOrDefault( v => v.Value == countryCode );
            if ( countryCodeValue == null )
            {
                countryCodeValue = new DefinedValue()
                {
                    DefinedTypeId = definedType.Id,
                };
                definedType.DefinedValues.Add( countryCodeValue );
            }
            countryCodeValue.LoadAttributes();

            countryCodeValue.Value = countryCode;
            countryCodeValue.Description = description;
            countryCodeValue.SetAttributeValue( "MatchRegEx", matchRegEx );
            countryCodeValue.SetAttributeValue( "FormatRegEx", formatRegEx );

            rockContext.SaveChanges();
            countryCodeValue.SaveAttributeValues( rockContext );

            return countryCodeValue.Id;
        }

    }
}
