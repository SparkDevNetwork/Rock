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
using System.Web.UI.WebControls;

using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class EventCalendarPicker : RockDropDownList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventCalendarPicker" /> class.
        /// </summary>
        public EventCalendarPicker()
            : base()
        {
            Label = "EventCalendar";
        }

        /// <summary>
        /// Gets or sets the eventCalendars.
        /// </summary>
        /// <value>
        /// The eventCalendars.
        /// </value>
        public List<EventCalendarCache> EventCalendars
        {
            set
            {
                this.Items.Clear();
                this.Items.Add( new ListItem() );

                foreach ( EventCalendarCache eventCalendar in value )
                {
                    this.Items.Add( new ListItem( eventCalendar.Name, eventCalendar.Id.ToString() ) );
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected eventCalendar identifier.
        /// </summary>
        /// <value>
        /// The selected eventCalendar identifier.
        /// </value>
        public int? SelectedEventCalendarId
        {
            get
            {
                return this.SelectedValueAsInt();
            }

            set
            {
                int id = value.HasValue ? value.Value : 0;
                var li = this.Items.FindByValue( id.ToString() );
                if ( li != null )
                {
                    li.Selected = true;
                }
            }
        }
    }
}