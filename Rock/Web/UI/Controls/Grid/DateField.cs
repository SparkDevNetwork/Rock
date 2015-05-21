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

using Rock;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [ToolboxData( "<{0}:DateField runat=server></{0}:DateField>" )]
    public class DateField : RockBoundField
    {
        /// <summary>
        /// Gets or sets a value indicating whether value should be displayed as an elapsed time (i.e. "3 days ago").
        /// </summary>
        /// <value>
        /// <c>true</c> if [format as elapsed time]; otherwise, <c>false</c>.
        /// </value>
        public bool FormatAsElapsedTime
        {
            get { return ViewState["FormatAsElapsedTime"] as bool? ?? false; }
            set { ViewState["FormatAsElapsedTime"] = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateField" /> class.
        /// </summary>
        public DateField()
            : base()
        {
            // Let the Header be left aligned (that's how Bootstrap wants it), but have the item be right-aligned
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
            this.DataFormatString = "{0:d}";
        }

        /// <summary>
        /// Initializes the <see cref="T:System.Web.UI.WebControls.BoundField" /> object.
        /// </summary>
        /// <param name="enableSorting">true if sorting is supported; otherwise, false.</param>
        /// <param name="control">The data control that owns the <see cref="T:System.Web.UI.WebControls.BoundField" />.</param>
        /// <returns>
        /// false in all cases.
        /// </returns>
        public override bool Initialize( bool enableSorting, Control control )
        {
            string script = @"
    $('.grid-table tr td span.date-field').tooltip({html: true, container: 'body', delay: { show: 100, hide: 100 }});
    $('.grid-table tr td span.date-field').click( function(){ $(this).tooltip('hide'); });;
";
            ScriptManager.RegisterStartupScript( control, control.GetType(), "date-field-popover", script, true );

            return base.Initialize( enableSorting, control );
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
            // if the dataValue is a string, try to convert it to a DateTime
            if ( dataValue is string )
            {
                dataValue = ( dataValue as string ).AsDateTime();
            }
            
            if ( FormatAsElapsedTime )
            {
                DateTime dateValue = DateTime.MinValue;
                if ( dataValue is DateTime )
                {
                    dateValue = ( (DateTime)dataValue );
                }

                if ( dataValue is DateTime? )
                {
                    dateValue = ( (DateTime?)dataValue ) ?? DateTime.MinValue; ;
                }

                if ( dateValue != DateTime.MinValue )
                {
                    return string.Format( "<span class='date-field' title='{0}'>{1}</span>", dateValue.ToString(), dateValue.ToElapsedString() );
                }
            }

            return base.FormatDataValue( dataValue, encode );
        }
    }
}