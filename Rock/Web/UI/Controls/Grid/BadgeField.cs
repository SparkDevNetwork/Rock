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
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using Rock;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// <see cref="Grid"/> Column for displaying a badge
    /// </summary>
    [ToolboxData( "<{0}:BadgeField runat=server></{0}:BadgeField>" )]
    public class BadgeField : RockBoundField
    {
        /// <summary>
        /// Gets or sets the important minimum value rule.
        /// </summary>
        /// <value>
        /// The minimum value to be considered Important.
        /// </value>
        public int ImportantMin
        {
            get
            {
                int? i = ViewState["ImportantMin"] as int?;
                return ( i == null ) ? int.MaxValue : i.Value;
            }
            set
            {
                ViewState["ImportantMin"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the important max.
        /// </summary>
        /// <value>
        /// The maximum value to be considered Important.
        /// </value>
        public int ImportantMax
        {
            get
            {
                int? i = ViewState["ImportantMax"] as int?;
                return ( i == null ) ? int.MaxValue : i.Value;
            }
            set
            {
                ViewState["ImportantMax"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the Warning minimum value rule.
        /// </summary>
        /// <value>
        /// The minimum value to be considered Warning.
        /// </value>
        public int WarningMin
        {
            get
            {
                int? i = ViewState["WarningMin"] as int?;
                return ( i == null ) ? int.MaxValue : i.Value;
            }
            set
            {
                ViewState["WarningMin"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the Warning maximum value rule.
        /// </summary>
        /// <value>
        /// The maximum value to be considered Warning.
        /// </value>
        public int WarningMax
        {
            get
            {
                int? i = ViewState["WarningMax"] as int?;
                return ( i == null ) ? int.MaxValue : i.Value;
            }
            set
            {
                ViewState["WarningMax"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the Success minimum value rule.
        /// </summary>
        /// <value>
        /// The minimum value to be considered Success.
        /// </value>
        public int SuccessMin
        {
            get
            {
                int? i = ViewState["SuccessMin"] as int?;
                return ( i == null ) ? int.MaxValue : i.Value;
            }
            set
            {
                ViewState["SuccessMin"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the Success maximum value rule.
        /// </summary>
        /// <value>
        /// The maximum value to be considered Success.
        /// </value>
        public int SuccessMax
        {
            get
            {
                int? i = ViewState["SuccessMax"] as int?;
                return ( i == null ) ? int.MaxValue : i.Value;
            }
            set
            {
                ViewState["SuccessMax"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the Info minimum value rule.
        /// </summary>
        /// <value>
        /// The minimum value to be considered Info.
        /// </value>
        public int InfoMin
        {
            get
            {
                int? i = ViewState["InfoMin"] as int?;
                return ( i == null ) ? int.MaxValue : i.Value;
            }
            set
            {
                ViewState["InfoMin"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the Info maximum value rule.
        /// </summary>
        /// <value>
        /// The maximum value to be considered Info.
        /// </value>
        public int InfoMax
        {
            get
            {
                int? i = ViewState["InfoMax"] as int?;
                return ( i == null ) ? int.MaxValue : i.Value;
            }
            set
            {
                ViewState["InfoMax"] = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadgeField" /> class.
        /// </summary>
        public BadgeField()
            : base()
        {
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            this.HeaderStyle.CssClass = "span1";
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
            BadgeRowEventArgs eventArg = new BadgeRowEventArgs( dataValue );

            SetBadgeTypeByRules( eventArg );

            if ( SetBadgeType != null )
            {
                SetBadgeType( this, eventArg );
            }

            string css = "badge";
            if( !string.IsNullOrWhiteSpace( eventArg.BadgeType ) )
            {
                css += " badge-" + eventArg.BadgeType.ToLower();
            }

            string fieldValue = base.FormatDataValue( eventArg.FieldValue, encode );

            return string.Format( "<span class='{0}'>{1}</span>", css, fieldValue );
        }

        private void SetBadgeTypeByRules( BadgeRowEventArgs e )
        {
            if ( !( e.FieldValue is int ) )
                return;

            int count = (int)e.FieldValue;

            if ( ImportantMin <= count && count <= ImportantMax )
            {
                e.BadgeType = "Important";
            }
            else if ( WarningMin <= count && count <= WarningMax )
            {
                e.BadgeType = "Warning";
            }
            else if ( SuccessMin <= count && count <= SuccessMax )
            {
                e.BadgeType = "Success";
            }
            else if ( InfoMin <= count && count <= InfoMax )
            {
                e.BadgeType = "Info";
            }
        }
        
        /// <summary>
        /// Occurs when badge field is being formatted.  Use to set the badge type
        /// based on the current row's field value.
        /// </summary>
        public event EventHandler<BadgeRowEventArgs> SetBadgeType;
    }


    /// <summary>
    /// 
    /// </summary>
    public class BadgeRowEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the field value.
        /// </summary>
        /// <value>
        /// The field value.
        /// </value>
        public object FieldValue { get; private set; }

        /// <summary>
        /// Gets or sets the type of the badge.
        /// </summary>
        /// <value>
        /// The type of the badge.
        /// </value>
        public string BadgeType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadgeRowEventArgs"/> class.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        public BadgeRowEventArgs(object fieldValue)
        {
            FieldValue = fieldValue;
            BadgeType = string.Empty;
        }
    }
}