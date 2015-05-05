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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Grid Bound Field for showing a Person as a link to the person profile page
    /// NOTE: Specify the full "Person" entity as the DataField
    /// </summary>
    [ToolboxData( "<{0}:PersonField runat=server></{0}:PersonField>" )]
    public class PersonField : RockBoundField
    {

        /// <summary>
        /// Gets or sets the URL format string.
        /// </summary>
        /// <value>
        /// The URL format string.
        /// </value>
        public string UrlFormatString
        {
            get { return ViewState["UrlFormatString"] as string ?? string.Empty; }
            set { ViewState["UrlFormatString"] = value; }
        }

        // Use private variable to store resolved format string.  This is so that the the string
        // does not need to be resolved on every row
        private string _resolvedUrlFormatString = null;
        private string ResolvedUrlFormatString
        {
            get
            {
                if ( _resolvedUrlFormatString == null )
                {
                    _resolvedUrlFormatString = UrlFormatString;
                    if ( string.IsNullOrWhiteSpace( _resolvedUrlFormatString ) )
                    {
                        _resolvedUrlFormatString = "~/Person/{0}";
                    }

                    _resolvedUrlFormatString = ( (RockPage)this.Control.Page ).ResolveRockUrl( _resolvedUrlFormatString );
                }

                return _resolvedUrlFormatString;
            }
        }

        /// <summary>
        /// Formats the specified field value for a cell in the <see cref="T:System.Web.UI.WebControls.BoundField" /> object.
        /// </summary>
        /// <param name="dataValue">The field value to format.</param>
        /// <param name="encode">true to encode the value; otherwise, false.</param>
        /// <returns>
        /// The field value converted to the format specified by <see cref="P:System.Web.UI.WebControls.BoundField.DataFormatString" />.
        /// </returns>
        protected override string FormatDataValue( object dataValue, bool encode )
        {
            var person = dataValue as Person;
            if ( person != null )
            {
                string url = string.Format( ResolvedUrlFormatString, person.Id );
                return string.Format( "<a href='{0}'>{1}</a>", url, person.FullName );
            }

            return base.FormatDataValue( dataValue, encode );
        }
    }
}
