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

namespace Rock.Obsidian.Blocks.Test
{
    /// <summary>
    /// Demonstrates a detail block.
    /// </summary>
    /// <seealso cref="Rock.Blocks.ObsidianBlockType" />

    [DisplayName( "Person Detail" )]
    [Category( "Obsidian > Test" )]
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

                if (person == null)
                {
                    return new BlockActionResult( HttpStatusCode.NotFound );
                }

                return new BlockActionResult( HttpStatusCode.OK, new PersonViewModel
                {
                    CampusId = person.PrimaryCampusId,
                    Email = person.Email,
                    FirstName = person.FirstName,
                    FullName = person.FullName,
                    Guid = person.Guid,
                    Id = person.Id,
                    LastName = person.LastName,
                    NickName = person.NickName
                } );
            }
        }

        /// <summary>
        /// Edits the person.
        /// </summary>
        /// <param name="personGuid">The person unique identifier.</param>
        /// <param name="personArgs">The person arguments.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult EditPerson( Guid personGuid, PersonArgs personArgs )
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var person = personService.Get( personGuid );

                if ( person == null )
                {
                    return new BlockActionResult( HttpStatusCode.NotFound );
                }

                person.FirstName = personArgs.FirstName;
                person.NickName = personArgs.NickName;
                person.LastName = personArgs.LastName;
                person.Email = personArgs.Email;
                person.PrimaryFamily.CampusId = personArgs.CampusId;

                rockContext.SaveChanges();
                return new BlockActionResult( HttpStatusCode.OK );
            }
        }

        #region Helper Classes

        /// <summary>
        /// Person Args
        /// </summary>
        public class PersonArgs
        {
            /// <summary>
            /// Gets or sets the first name.
            /// </summary>
            /// <value>
            /// The first name.
            /// </value>
            public string FirstName { get; set; }

            /// <summary>
            /// Gets or sets the name of the nick.
            /// </summary>
            /// <value>
            /// The name of the nick.
            /// </value>
            public string NickName { get; set; }

            /// <summary>
            /// Gets or sets the last name.
            /// </summary>
            /// <value>
            /// The last name.
            /// </value>
            public string LastName { get; set; }

            /// <summary>
            /// Gets or sets the email.
            /// </summary>
            /// <value>
            /// The email.
            /// </value>
            public string Email { get; set; }

            /// <summary>
            /// Gets or sets the campus identifier.
            /// </summary>
            /// <value>
            /// The campus identifier.
            /// </value>
            public int? CampusId { get; set; }
        }

        /// <summary>
        /// Person View Model
        /// </summary>
        public sealed class PersonViewModel : PersonArgs
        {
            /// <summary>
            /// Gets or sets the unique identifier.
            /// </summary>
            /// <value>
            /// The unique identifier.
            /// </value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the full name.
            /// </summary>
            /// <value>
            /// The full name.
            /// </value>
            public string FullName { get; set; }
        }

        #endregion Helper Classes
    }
}
