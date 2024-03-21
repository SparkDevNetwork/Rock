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

namespace Rock.Tests.Integration.Modules.Core
{
    /// <summary>
    /// Functions to assist with core module testing.
    /// </summary>
    public class CoreModuleTestHelper
    {
        private string _RecordTag = null;

        /// <summary>
        /// Constuctor
        /// </summary>
        /// <param name="recordTag">A tag that is added to the ForeignKey property of each record created by this helper instance.</param>
        public CoreModuleTestHelper( string recordTag )
        {
            _RecordTag = recordTag;
        }

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
    }
}
