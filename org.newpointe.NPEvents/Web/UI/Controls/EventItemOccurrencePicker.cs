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

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace org.newpointe.RockU.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class EventItemOccurrencePicker : RockDropDownList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventItemPicker" /> class.
        /// </summary>
        public EventItemOccurrencePicker()
        {
            Items.Clear();
            Items.Add( new ListItem() );

            using ( var rockContext = new RockContext() )
            {
                var calendarItemOccurrences = new EventItemOccurrenceService( rockContext ).Queryable()
                                .Where( i => i.EventItem.IsActive )
                                .ToList()
                                .Where( i => i.NextStartDateTime > DateTime.Now )
                                .Select( i => new
                                {
                                    Event = i.EventItem.Name,
                                    Id = i.EventItem.Id,
                                    Name = "[" + i.Id + "] " + i.NextStartDateTime
                                } )
                                .OrderBy( i => i.Event )
                                .ToList();

                foreach ( var calendarItemOccurrence in calendarItemOccurrences )
                {
                    ListItem listItem = new ListItem( calendarItemOccurrence.Name, calendarItemOccurrence.Id.ToString() );
                    listItem.Attributes["OptionGroup"] = calendarItemOccurrence.Event;
                    Items.Add( listItem );
                }
            }
        }
    }
}