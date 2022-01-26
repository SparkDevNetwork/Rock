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

using System.Linq;
using System.Web.Http;

using Rock.Data;
using Rock.Rest.Filters;

namespace Rock.Rest.v2.Controls
{
    /// <summary>
    /// API endpoints for the Person Picker.
    /// </summary>
    [RoutePrefix( "api/v2/Controls/PersonPicker" )]
    public class PersonPickerController : ControlsControllerBase
    {
        /// <summary>
        /// Searches for people that match the given search options and returns
        /// those matches.
        /// </summary>
        /// <returns>A collection of <see cref="Rock.Rest.Controllers.PersonSearchResult"/> objects.</returns>
        [Authenticate]
        [Secured]
        [HttpPost]
        [System.Web.Http.Route( "Search" )]
        public IQueryable<Rock.Rest.Controllers.PersonSearchResult> PostSearch( [FromBody] SearchOptions searchOptions )
        {
            var rockContext = new RockContext();

            return Rock.Rest.Controllers.PeopleController.SearchForPeople( rockContext, searchOptions.Name, searchOptions.Address, searchOptions.Phone, searchOptions.Email, searchOptions.IncludeDetails, searchOptions.IncludeBusinesses, searchOptions.IncludeDeceased, false );
        }

        #region Options

        /// <summary>
        /// Class SearchOptions.
        /// </summary>
        public class SearchOptions
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>The name.</value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the address.
            /// </summary>
            /// <value>The address.</value>
            public string Address { get; set; }

            /// <summary>
            /// Gets or sets the phone.
            /// </summary>
            /// <value>The phone.</value>
            public string Phone { get; set; }

            /// <summary>
            /// Gets or sets the email.
            /// </summary>
            /// <value>The email.</value>
            public string Email { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [include details].
            /// </summary>
            /// <value><c>true</c> if [include details]; otherwise, <c>false</c>.</value>
            public bool IncludeDetails { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [include businesses].
            /// </summary>
            /// <value><c>true</c> if [include businesses]; otherwise, <c>false</c>.</value>
            public bool IncludeBusinesses { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [include deceased].
            /// </summary>
            /// <value><c>true</c> if [include deceased]; otherwise, <c>false</c>.</value>
            public bool IncludeDeceased { get; set; }
        }

        #endregion
    }
}
