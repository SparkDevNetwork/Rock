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
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// <see cref="Grid"/> Column for displaying a badge
    /// </summary>
    [ToolboxData( "<{0}:BadgeField runat=server></{0}:BadgeField>" )]
    public class BadgeField : RockBoundField
    {
        /// <summary>
        /// Gets or sets the danger minimum value rule.
        /// </summary>
        /// <value>
        /// The minimum value to be considered Danger.
        /// </value>
        public int DangerMin
        {
            get
            {
                int? i = ViewState["DangerMin"] as int?;
                return ( i == null ) ? int.MaxValue : i.Value;
            }
            set
            {
                ViewState["DangerMin"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the danger max.
        /// </summary>
        /// <value>
        /// The maximum value to be considered Danger.
        /// </value>
        public int DangerMax
        {
            get
            {
                int? i = ViewState["DangerMax"] as int?;
                return ( i == null ) ? int.MaxValue : i.Value;
            }
            set
            {
                ViewState["DangerMax"] = value;
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
        /// Gets or sets the Hide minimum value rule.
        /// </summary>
        /// <value>
        /// The minimum value to be considered Hide.
        /// </value>
        public int HideMin
        {
            get
            {
                int? i = ViewState["HideMin"] as int?;
                return ( i == null ) ? int.MaxValue : i.Value;
            }
            set
            {
                ViewState["HideMin"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the Hide maximum value rule.
        /// </summary>
        /// <value>
        /// The maximum value to be considered Hide.
        /// </value>
        public int HideMax
        {
            get
            {
                int? i = ViewState["HideMax"] as int?;
                return ( i == null ) ? int.MaxValue : i.Value;
            }
            set
            {
                ViewState["HideMax"] = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadgeField" /> class.
        /// </summary>
        public BadgeField()
            : base()
        {
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            this.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
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

            if ( eventArg.BadgeType == "Hide" )
            {
                return string.Empty;
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

            // Remove ImportantMin and ImportanMax once after deprecation period.
            if ( DangerMin <= count && count <= DangerMax ) 
            {
                e.BadgeType = "Danger";
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
            else if ( HideMin <= count && count <= HideMax )
            {
                e.BadgeType = "Hide";
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