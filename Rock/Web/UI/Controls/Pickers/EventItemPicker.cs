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
using System.Linq;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class EventItemPicker : RockDropDownList
    {
        /// <summary>
        /// Gets or sets a value indicating whether [display active only].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [display active only]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeInactive
        {
            get
            {
                return ViewState["IncludeInactive"] as bool? ?? true;
            }

            set
            {
                bool valueChanged = ( IncludeInactive != value );
                if ( valueChanged )
                {
                    ViewState["IncludeInactive"] = value;
                    LoadDropDownItems();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventItemPicker" /> class.
        /// </summary>
        public EventItemPicker()
        {
            LoadDropDownItems();
        }

        /// <summary>
        /// Loads the drop down items.
        /// </summary>
        private void LoadDropDownItems()
        {
            this.Items.Clear();
            this.Items.Add( new ListItem() );

            using ( var rockContext = new RockContext() )
            {

                var calendarItems = new EventCalendarItemService( rockContext ).Queryable()
                                .Select( i => new
                                    {
                                        Calendar = i.EventCalendar.Name,
                                        Id = i.EventItem.Id,
                                        Name = i.EventItem.Name,
                                        IsActive = i.EventItem.IsActive
                                    } )
                                .OrderBy( i => i.Calendar )
                                .ThenBy( i => i.Name )
                                .ToList();

                if ( !IncludeInactive )
                {
                    calendarItems = calendarItems.Where( i => i.IsActive ).ToList();
                }

                foreach ( var calendarItem in calendarItems )
                {
                    ListItem listItem = new ListItem( calendarItem.Name, calendarItem.Id.ToString() );
                    listItem.Attributes["OptionGroup"] = calendarItem.Calendar;
                    this.Items.Add( listItem );
                }
            }
        }
    }
}