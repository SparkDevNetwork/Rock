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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// <see cref="Grid"/> Column to display a checkmark icon if the ArrayKey exists in the FieldValue, which is an IEnumerable
    /// NOTE: can only be used from code-behind
    /// </summary>
    [ToolboxData( "<{0}:BoolFromArrayField runat=server></{0}:BoolFromArrayField>" )]
    public class BoolFromArrayField<T> : RockBoundField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoolField" /> class.
        /// </summary>
        public BoolFromArrayField()
            : base()
        {
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        }

        /// <summary>
        /// Gets or sets the array key.
        /// </summary>
        /// <value>
        /// The array key.
        /// </value>
        public T ArrayKey
        {
            get
            {

                return (T)ViewState["ArrayKey"];
            }
            set
            {

                ViewState["ArrayKey"] = value;
            }
        }

        /// <summary>
        /// Formats the specified field value for a cell in the <see cref="T:System.Web.UI.WebControls.BoundField"/> object.
        /// </summary>
        /// <param name="dataValue">The field value to format.</param>
        /// <param name="encode">true to encode the value; otherwise, false.</param>
        /// <returns>
        /// The field value converted to the format specified by <see cref="P:System.Web.UI.WebControls.BoundField.DataFormatString"/>.
        /// </returns>
        protected override string FormatDataValue( object dataValue, bool encode )
        {
            var sourceArray = dataValue as IEnumerable<T>;

            bool boolValue = sourceArray.Contains( ArrayKey );

            if ( boolValue )
            {
                return "<i class=\"fa fa-check\"></i>";
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Gets the value that should be exported to Excel
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public override object GetExportValue( GridViewRow row )
        {
            var dataValue = base.GetExportValue( row );
            var sourceArray = dataValue as IEnumerable<T>;

            return sourceArray.Contains( ArrayKey );
        }
    }
}