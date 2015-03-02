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
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// <see cref="Grid"/> Column to display a currency value using the Currency Symbol from Global Attributes
    /// </summary>
    [ToolboxData( "<{0}:CurrencyField runat=server></{0}:CurrencyField>" )]
    public class CurrencyField : RockBoundField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrencyField"/> class.
        /// </summary>
        public CurrencyField()
            : base()
        {
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
            this.DataFormatString = "{0:C}";
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
            string value = base.FormatDataValue( dataValue, encode );

            return String.Format( "{0}{1}", GlobalAttributesCache.Value( "CurrencySymbol" ), value );
        }
    }
}