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

using Rock.Attribute;
using Rock.Enums.Core;
using Rock.Extension;
using Rock.Model;

namespace Rock.Search
{
    /// <summary>
    /// The base class for search components.
    /// </summary>
    [TextField( "Search Label", "The text to display in the search type dropdown", false, "Search" )]
    [TextField( "Result URL", "The URL to redirect user to after they have entered search text.  (use '{0}' for the search text)" )]
    public abstract class SearchComponent : Component
    {
        /// <summary>
        /// The label to display for the type of search
        /// </summary>
        public virtual string SearchLabel
        {
            get
            {

                if ( !String.IsNullOrWhiteSpace( GetAttributeValue( "SearchLabel" ) ) )
                {
                    return GetAttributeValue( "SearchLabel" );
                }
                else
                {
                    return "Search";
                }
            }
        }

        /// <summary>
        /// The URL to redirect user to after they've entered search criteria
        /// </summary>
        public virtual string ResultUrl
        {
            get
            {
                if ( !String.IsNullOrWhiteSpace( GetAttributeValue( "ResultURL" ) ) )
                {
                    return GetAttributeValue( "ResultURL" );
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Sets the preferred keyboard mode for this search component.
        /// </summary>
        /// <remarks>
        /// <para>This is a suggestion to the interface, and it may not be honored in some cases.</para>
        /// <para>Currently, this is only utilized in Rock Mobile and may be expanded later.</para>
        /// </remarks>
        public virtual KeyboardInputMode PreferredKeyboardMode
        {
            get
            {
                return KeyboardInputMode.Default;
            }
        }

        /// <summary>
        /// Returns a queryable of objects that match the search term.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <returns>A queryable of objects that match the search.</returns>
        /// <remarks>Results should be limited to implementations of
        /// <see cref="Rock.Data.IEntity"/> or
        /// <see cref="Rock.UniversalSearch.IndexModels.IndexModelBase"/>.
        /// Any other object type included in the results is currently considered
        /// an error.
        /// </remarks>
        public virtual IOrderedQueryable<object> SearchQuery( string searchTerm )
        {
            return Array.Empty<object>().AsQueryable().OrderBy( a => a );
        }

        /// <summary>
        /// Returns a list of value/label results matching the searchterm
        /// </summary>
        /// <param name="searchterm">The searchterm.</param>
        /// <returns></returns>
        public abstract IQueryable<string> Search( string searchterm );
    }
}