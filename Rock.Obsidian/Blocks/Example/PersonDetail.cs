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
using System.ComponentModel;
using System.Net;
using Rock.Attribute;
using Rock.Blocks;
using Rock.Data;
using Rock.Model;
using Rock.ViewModel;

namespace Rock.Obsidian.Blocks.Example
{
    /// <summary>
    /// Demonstrates a detail block.
    /// </summary>
    /// <seealso cref="Rock.Blocks.ObsidianBlockType" />

    [DisplayName( "Person Detail" )]
    [Category( "Obsidian > Example" )]
    [Description( "Demonstrates a detail block." )]
    [IconCssClass( "fa fa-flask" )]

    public class PersonDetail : ObsidianBlockType
    {
        /// <summary>
        /// Gets the person view model.
        /// </summary>
        /// <param name="personGuid">The person unique identifier.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetPersonViewModel( Guid personGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var person = personService.Get( personGuid );

                if ( person == null )
                {
                    return new BlockActionResult( HttpStatusCode.NotFound );
                }

                var currentPerson = GetCurrentPerson();
                var personViewModel = person.ToViewModel( currentPerson );
                return new BlockActionResult( HttpStatusCode.OK, personViewModel );
            }
        }

        /// <summary>
        /// Edits the person.
        /// </summary>
        /// <param name="personGuid">The person unique identifier.</param>
        /// <param name="personArgs">The person arguments.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult EditPerson( Guid personGuid, PersonViewModel personArgs )
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var person = personService.Get( personGuid );

                if ( person == null )
                {
                    return new BlockActionResult( HttpStatusCode.NotFound );
                }

                person.PrimaryFamily.CampusId = personArgs.PrimaryCampusId;
                person.Email = personArgs.Email;
                person.FirstName = personArgs.FirstName;
                person.NickName = personArgs.NickName;
                person.LastName = personArgs.LastName;

                rockContext.SaveChanges();
                return new BlockActionResult( HttpStatusCode.OK );
            }
        }
    }
}
