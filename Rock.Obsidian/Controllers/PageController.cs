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
using System.Web.Http;
using Rock.Rest.Filters;

namespace Rock.Obsidian.Controllers.Page
{
    /// <summary>
    /// Obsidian Page Controller
    /// </summary>
    public class PageController : ObsidianController
    {
        /// <summary>
        /// Gets the defined values.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type unique identifier.</param>
        /// <param name="IsActive">if set to <c>true</c> [is active].</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [HttpGet]
        [Authenticate]
        [System.Web.Http.Route( "api/obsidian/v1/page/initialization/{pageGuid}" )]
        public PageInitializationViewModel GetPageInitializationViewModel( Guid pageGuid )
        {
            var currentPerson = GetPerson();
            var viewModel = new PageInitializationViewModel();

            if ( currentPerson == null )
            {
                return viewModel;
            }

            viewModel.CurrentPerson = new CurrentPersonViewModel
            {
                FirstName = currentPerson.FirstName,
                FullName = currentPerson.FullName,
                Guid = currentPerson.Guid,
                Id = currentPerson.Id,
                LastName = currentPerson.LastName,
                MiddleName = currentPerson.MiddleName,
                NickName = currentPerson.NickName,
                PrimaryAliasId = currentPerson.PrimaryAliasId,
                PhotoUrl = currentPerson.PhotoUrl
            };

            return viewModel;
        }
    }

    /// <summary>
    /// Page Initialization View Model
    /// </summary>
    public sealed class PageInitializationViewModel
    {
        /// <summary>
        /// Gets or sets the current person.
        /// </summary>
        /// <value>
        /// The current person.
        /// </value>
        public CurrentPersonViewModel CurrentPerson { get; set; }
    }

    /// <summary>
    /// Current Person View Model
    /// </summary>
    public sealed class CurrentPersonViewModel
    {

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the primary alias identifier.
        /// </summary>
        /// <value>
        /// The primary alias identifier.
        /// </value>
        public int? PrimaryAliasId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the name of the nick.
        /// </summary>
        /// <value>
        /// The name of the nick.
        /// </value>
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the name of the middle.
        /// </summary>
        /// <value>
        /// The name of the middle.
        /// </value>
        public string MiddleName { get; set; }

        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        /// <value>
        /// The full name.
        /// </value>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the photo URL.
        /// </summary>
        /// <value>
        /// The photo URL.
        /// </value>
        public string PhotoUrl { get; set; }
    }
}
