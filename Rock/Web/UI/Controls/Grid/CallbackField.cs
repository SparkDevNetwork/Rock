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
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Grid field control with a OnFormatDataValue event that can be used to determine the formatted output
    /// </summary>
    [ToolboxData( "<{0}:CallbackField runat=server></{0}:CallbackField>" )]
    public class CallbackField : RockBoundField
    {
        /// <summary>
        /// Formats the specified field value for a cell in the <see cref="T:System.Web.UI.WebControls.BoundField" /> object.
        /// </summary>
        /// <param name="dataValue">The field value to format.</param>
        /// <param name="encode">true to encode the value; otherwise, false.</param>
        /// <returns>
        /// The field value converted to the format specified by <see cref="P:System.Web.UI.WebControls.BoundField.DataFormatString" />.
        /// </returns>
        protected override string FormatDataValue(object dataValue, bool encode)
        {
            if ( OnFormatDataValue != null )
            {
                CallbackEventArgs args = new CallbackEventArgs { DataValue = dataValue};
                OnFormatDataValue( this, args );
                return args.FormattedValue;
            }
            else
            {
                return base.FormatDataValue( dataValue, encode );
            }
        }

        /// <summary>
        /// Occurs when [on format data value].
        /// This is the callback event that you can use to use custom logic to set the formatted value
        /// </summary>
        public event EventHandler<CallbackEventArgs> OnFormatDataValue;

        /// <summary>
        /// 
        /// </summary>
        public class CallbackEventArgs : EventArgs
        {
            /// <summary>
            /// This is the DataValue. Use this to set the Formatted Value in the OnFormatDataValue callback
            /// </summary>
            /// <value>
            /// The data value.
            /// </value>
            public object DataValue { get; set; }

            /// <summary>
            /// Set the formatted value in the OnFormatDataValue callback
            /// </summary>
            /// <value>
            /// The formatted value.
            /// </value>
            public string FormattedValue { get; set; }
        }
    }
}
