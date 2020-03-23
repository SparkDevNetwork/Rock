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
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class DayOfWeekPicker : RockDropDownList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DayOfWeekPicker"/> class.
        /// </summary>
        public DayOfWeekPicker()
            : base()
        {
            UpdateDayOfWeekItems();
        }

        /// <summary>
        /// Updates the day of week items.
        /// </summary>
        private void UpdateDayOfWeekItems()
        {
            this.Items.Clear();
            this.Items.Add( new ListItem() );
            foreach ( var dow in Enum.GetValues( typeof( DayOfWeek ) ).OfType<DayOfWeek>().ToList() )
            {
                string itemName = dow.ConvertToString();
                if ( dow == DefaultDayOfWeek )
                {
                    itemName = $"{dow.ConvertToString()} (default)";
                }

                this.Items.Add( new ListItem( itemName, dow.ConvertToInt().ToString() ) );
            }
        }

        /// <summary>
        /// Set this to indicate which Day of the Week is the default
        /// </summary>
        /// <value>
        /// The default day of week.
        /// </value>
        public DayOfWeek? DefaultDayOfWeek
        {
            get
            {
                return ViewState["DefaultDayOfWeek"] as DayOfWeek?;
            }

            set
            {
                ViewState["DefaultDayOfWeek"] = value;

                var selectedDayOfWeek = this.SelectedDayOfWeek;

                UpdateDayOfWeekItems();

                if ( selectedDayOfWeek.HasValue )
                {
                    this.SelectedDayOfWeek = selectedDayOfWeek;
                }

            }
        }

        /// <summary>
        /// Gets or sets the selected day of week.
        /// </summary>
        /// <value>
        /// The selected day of week.
        /// </value>
        public DayOfWeek? SelectedDayOfWeek
        {
            get
            {
                int? result = this.SelectedValue.AsIntegerOrNull();
                if ( result.HasValue )
                {
                    return ( DayOfWeek ) result.Value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if ( value != null )
                {
                    int id = value.ConvertToInt();
                    this.SetValue( id.ToString() );
                }
            }
        }

    }
}