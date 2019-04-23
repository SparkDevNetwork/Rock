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
using Rock.Extension;

namespace Rock.Search
{
    /// <summary>
    /// 
    /// </summary>
    [TextField( "Search Label", "The text to display in the search type dropdown", false, "Search" )]
    [TextField( "Result URL", "The url to redirect user to after they have entered search text.  (use '{0}' for the search text)" )]
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
        /// The url to redirect user to after they've entered search criteria
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
        /// Returns a list of value/label results matching the searchterm
        /// </summary>
        /// <param name="searchterm">The searchterm.</param>
        /// <returns></returns>
        public abstract IQueryable<string> Search( string searchterm );
    }
}