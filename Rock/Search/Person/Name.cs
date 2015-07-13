// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Search.Person
{
    /// <summary>
    /// Searches for people with matching names
    /// </summary>
    [Description("Person Name Search")]
    [Export(typeof(SearchComponent))]
    [ExportMetadata("ComponentName", "Person Name")]
    [BooleanField("Allow Search by Only First Name", "By default, when searching with only one name (without a space or comma), only people with a matching Last Names will be included.  Select this option to also include people with a matching First Name", false, "", 4, "FirstNameSearch")]
    public class Name : SearchComponent
    {

        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults
        {
            get
            {
                var defaults = new Dictionary<string, string>();
                defaults.Add( "SearchLabel", "Name" );
                return defaults;
            }
        }

        /// <summary>
        /// The url to redirect user to after they've entered search criteria
        /// </summary>
        public override string ResultUrl
        {
            get
            {
                bool allowFirstNameSearch = GetAttributeValue( "FirstNameSearch" ).AsBooleanOrNull() ?? false;
                if ( allowFirstNameSearch )
                {
                    string url = base.ResultUrl;
                    return url + ( url.Contains( "?" ) ? "&" : "?" ) + "allowFirstNameOnly=true";
                }

                return base.ResultUrl;
            }
        }

        /// <summary>
        /// Returns a list of matching people
        /// </summary>
        /// <param name="searchterm"></param>
        /// <returns></returns>
        public override IQueryable<string> Search( string searchterm )
        {
            bool allowFirstNameSearch = GetAttributeValue( "FirstNameSearch" ).AsBooleanOrNull() ?? false;

            bool reversed = false;
            var qry = new PersonService( new RockContext() ).GetByFullNameOrdered( searchterm, true, false, allowFirstNameSearch, out reversed );

            IQueryable<string> resultQry;

            if ( reversed )
            {
                resultQry = qry.Select( p => p.LastName + ", " + p.NickName).Distinct();
            }
            else
            {
                resultQry = qry.Select( p => p.NickName + " " + p.LastName ).Distinct();
            }

            return resultQry;
        }
    }
}