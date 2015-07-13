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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Security;

namespace Rock.Model
{
    /// <summary>
    /// Represents an event calendar.
    /// </summary>
    [Table( "EventCalendar" )]
    [DataContract]
    public partial class EventCalendar : Model<EventCalendar>, ISecured
    {
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
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }
        private bool _isActive = true;

        #region Virtual Properties

        /// <summary>
        /// Gets or sets a collection of the <see cref="Rock.Model.Group">EventCalendarItems</see> that belong to this EventCalendar.
        /// </summary>
        /// <value>
        /// A collection containing a collection of the <see cref="Rock.Model.Group">EventCalendarItems</see> that belong to this EventCalendar.
        /// </value>
        public virtual ICollection<EventCalendarItem> EventCalendarItems
        {
            get { return _eventCalenderItems ?? ( _eventCalenderItems = new Collection<EventCalendarItem>() ); }
            set { _eventCalenderItems = value; }
        }
        private ICollection<EventCalendarItem> _eventCalenderItems;

        #endregion

        #region Methods

        /// <summary>
        /// Gets the supported actions.
        /// </summary>
        /// <value>
        /// The supported actions.
        /// </value>
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