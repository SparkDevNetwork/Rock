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

using System.ComponentModel;
using System.Net;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModel;

namespace Rock.Blocks.Example
{
    /// <summary>
    /// Demonstrates a detail block.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianBlockType" />

    [DisplayName( "Person Detail" )]
    [Category( "Obsidian > Example" )]
    [Description( "Demonstrates a detail block." )]
    [IconCssClass( "fa fa-flask" )]

    public class PersonDetail : RockObsidianBlockType
    {
        /// <summary>
        /// Gets the person view model.
        /// </summary>
        /// <param name="personGuid">The person unique identifier.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetPersonViewModel()
        {
            var currentPerson = GetCurrentPerson();
            var personViewModel = currentPerson.ToViewModel( currentPerson );
            return new BlockActionResult( HttpStatusCode.OK, personViewModel );
        }

        /// <summary>
        /// Edits the person.
        /// </summary>
        /// <param name="personGuid">The person unique identifier.</param>
        /// <param name="personArgs">The person arguments.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult EditPerson( PersonViewModel personArgs )
        {
            var currentPerson = GetCurrentPerson();

            if ( currentPerson == null )
            {
                return new BlockActionResult( HttpStatusCode.Unauthorized );
            }

            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var person = personService.Get( currentPerson.Id );
                var family = person?.GetFamily( rockContext );

                if ( family == null )
                {
                    return new BlockActionResult( HttpStatusCode.NotFound );
                }

                family.CampusId = personArgs.PrimaryCampusId;
                person.Email = personArgs.Email;
                person.FirstName = personArgs.FirstName;
                person.NickName = personArgs.NickName;
                person.LastName = personArgs.LastName;
                person.BirthDay = personArgs.BirthDay;
                person.BirthMonth = personArgs.BirthMonth;
                person.BirthYear = personArgs.BirthYear;

                rockContext.SaveChanges();
                return new BlockActionResult( HttpStatusCode.OK );
            }
        }
    }
}
