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
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Rock.Utility.Enums;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control that can be used to select multiple days of the week
    /// </summary>
    public class DaysOfWeekPicker : RockCheckBoxList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DaysOfWeekPicker"/> class.
        /// </summary>
        public DaysOfWeekPicker()
            : base()
        {
            Label = "Days of Week";
            this.Items.Clear();
            foreach ( var dow in Enum.GetValues( typeof( DayOfWeek ) ).OfType<DayOfWeek>().ToList() )
            {
                this.Items.Add( new ListItem( dow.ConvertToString(), dow.ConvertToInt().ToString() ) );
            }
        }

        /// <summary>
        /// Gets the selected days of the week
        /// </summary>
        /// <value>
        /// The selected days of the week
        /// </value>
        public List<DayOfWeek> SelectedDaysOfWeek
        {
            get
            {
                return this.Items.OfType<ListItem>().Where( l => l.Selected ).Select( a => ( DayOfWeek ) int.Parse( a.Value ) ).ToList();
            }

            set
            {
                foreach ( ListItem item in this.Items )
                {
                    item.Selected = value?.Exists( a => a.Equals( ( DayOfWeek ) int.Parse( item.Value ) ) ) == true;
                }
            }
        }

        /// <summary>
        /// Gets the selected days of week as flags (bit per day of the week).
        /// </summary>
        /// <returns></returns>
        public DayOfWeekFlag SelectedDaysOfWeekAsFlags()
        {
            var flags = DayOfWeekFlag.None;

            foreach ( var dayOfWeek in SelectedDaysOfWeek )
            {
                var asFlag = dayOfWeek.AsFlag();
                flags |= asFlag;
            }

            return flags;
        }
    }
}