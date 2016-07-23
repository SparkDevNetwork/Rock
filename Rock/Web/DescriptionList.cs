﻿// <copyright>
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
using System.Collections.Generic;

namespace Rock.Web
{
    /// <summary>
    /// Helps generate Bootstrap Description List HTML (<dl>...</dl>)
    /// </summary>
    public class DescriptionList
    {
        /// <summary>
        /// The term description list
        /// </summary>
        private Dictionary<string, string> _termDescriptionList = new Dictionary<string, string>();

        /// <summary>
        /// Adds the specified term.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <param name="description">The description.</param>
        /// <param name="showIfBlank">if set to <c>true</c> [show if blank].</param>
        /// <returns></returns>
        public DescriptionList Add( string term, object description, bool showIfBlank = false )
        {
            string value = description != null ? description.ToString() : string.Empty;
            if ( !string.IsNullOrWhiteSpace( value ) || showIfBlank )
            {
                _termDescriptionList.Add( term, value );
            }
            return this;
        }

        /// <summary>
        /// Adds the specified term.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="format">The format.</param>
        /// <param name="showIfBlank">if set to <c>true</c> [show if blank].</param>
        /// <returns></returns>
        public DescriptionList Add( string term, DateTime? dateTime, string format = "g", bool showIfBlank = false )
        {
            if ( dateTime != null )
            {
                return Add(term, dateTime.Value.ToString(format), showIfBlank);
            }
            else
            {
                return Add( term, null, showIfBlank );
            }
        }

        /// <summary>
        /// Gets the HTML.
        /// </summary>
        /// <value>
        /// The HTML.
        /// </value>
        public string Html
        {
            get
            {
                if ( _termDescriptionList.Any() )
                {
                    string descriptionFormat = "<dt>{0}</dt><dd>{1}</dd>";

                    string result = @"<dl>";

                    foreach ( var pair in _termDescriptionList )
                    {
                        string displayValue = pair.Value;
                        if ( string.IsNullOrWhiteSpace( displayValue ) )
                        {
                            displayValue = Rock.Constants.None.TextHtml;
                        }

                        result += string.Format( descriptionFormat, pair.Key, displayValue );
                    }

                    result += @"</dl>";

                    return result;
                }

                return string.Empty;

            }
        }
    }
}
