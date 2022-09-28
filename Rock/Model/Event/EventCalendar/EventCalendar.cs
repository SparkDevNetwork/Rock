﻿// <copyright>
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents an event calendar.
    /// </summary>
    [RockDomain( "Event" )]
    [Table( "EventCalendar" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "E67D8D6D-4FE6-48D5-A940-A39213047314")]
    public partial class EventCalendar : Model<EventCalendar>, ISecured, IHasActiveFlag, ICacheable, ICampusFilterable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Name of the EventCalendar. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the EventCalendar.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Description of the EventCalendar.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the EventCalendar.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class name for a font vector based icon.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the CSS class name of a font based icon.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the is active.
        /// </summary>
        /// <value>
        /// The is active.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is index enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is index enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsIndexEnabled { get; set; } = false;

        #endregion
        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EventCalendarItem">event calendar items</see>.
        /// </summary>
        /// <value>
        /// The event calendar items.
        /// </value>
        public virtual ICollection<EventCalendarItem> EventCalendarItems
        {
            get { return _eventCalenderItems ?? ( _eventCalenderItems = new Collection<EventCalendarItem>() ); }
            set { _eventCalenderItems = value; }
        }

        private ICollection<EventCalendarItem> _eventCalenderItems;

        /// <summary>
        /// Gets or sets the content channels.
        /// </summary>
        /// <value>
        /// The content channels.
        /// </value>
        public virtual ICollection<EventCalendarContentChannel> ContentChannels
        {
            get { return _contentChannels ?? ( _contentChannels = new Collection<EventCalendarContentChannel>() ); }
            set { _contentChannels = value; }
        }

        private ICollection<EventCalendarContentChannel> _contentChannels;

        /// <summary>
        /// Provides a <see cref="Dictionary{TKey, TValue}"/> of actions that this model supports, and the description of each.
        /// </summary>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                var supportedActions = base.SupportedActions;
                supportedActions.AddOrReplace( Rock.Security.Authorization.APPROVE, "The roles and/or users that have access to approve calendar items." );
                return supportedActions;
            }
        }

        #endregion
        #region Public Methods

        /// <summary>
        /// Returns the default authorization for a specific action.
        /// </summary>
        /// <param name="action">A <see cref="System.String"/> representing the name of the action.</param>
        /// <returns>A <see cref="System.Boolean"/> that is <c>true</c> if the specified action is allowed by default; otherwise <c>false</c>.</returns>
        public override bool IsAllowedByDefault( string action )
        {
            return false;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// EventCalendar Configuration class.
    /// </summary>
    public partial class EventCalendarConfiguration : EntityTypeConfiguration<EventCalendar>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventCalendarConfiguration" /> class.
        /// </summary>
        public EventCalendarConfiguration()
        {
        }
    }

    #endregion
}