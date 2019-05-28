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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Model;

namespace Rock.CheckIn
{
    /// <summary>
    /// A family option for the current check-in
    /// </summary>
    [DataContract]
    public class CheckInFamily : DotLiquid.Drop
    {
        /// <summary>
        /// A list of attendance records for the family check-in.
        /// </summary>
        /// <value>
        /// A list of attendance Ids.
        /// </value>
        [DataMember]
        public List<int> AttendanceIds { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [DataMember]
        public Group Group { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CheckInFamily" /> is selected for check-in
        /// </summary>
        /// <value>
        ///   <c>true</c> if selected; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool Selected { get; set; }

        /// <summary>
        /// Gets or sets the people that this family can check-in
        /// </summary>
        /// <value>
        /// The people.
        /// </value>
        [DataMember]
        public List<CheckInPerson> People { get; set; }

        /// <summary>
        /// Gets the current person if using family check-in mode
        /// </summary>
        /// <value>
        /// The current person.
        /// </value>
        public CheckInPerson CurrentPerson
        {
            get
            {
                return GetPeople( true ).FirstOrDefault( p => !p.Processed );
            }
        }

        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        [DataMember]
        public CheckinAction Action { get; set; }

        /// <summary>
        /// Gets or sets the check out people.
        /// </summary>
        /// <value>
        /// The check out people.
        /// </value>
        [DataMember]
        public List<CheckOutPerson> CheckOutPeople { get; set; }

        /// <summary>
        /// An optional value that can be set to display family name.  If not set, the Group name will be used
        /// </summary>
        [DataMember]
        public string Caption { get; set; }

        /// <summary>
        /// Gets or sets the sub caption.
        /// </summary>
        /// <value>
        /// The sub caption.
        /// </value>
        [DataMember]
        public string SubCaption { get; set; }

        /// <summary>
        /// Gets or sets the first names of the people in the Family
        /// </summary>
        /// <value>
        /// The first names.
        /// </value>
        [DataMember]
        public List<string> FirstNames { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInFamily" /> class.
        /// </summary>
        public CheckInFamily()
            : base()
        {
            AttendanceIds = new List<int>();
            People = new List<CheckInPerson>();
            Action = CheckinAction.CheckIn;
            CheckOutPeople = new List<CheckOutPerson>();
        }

        /// <summary>
        /// Clears the filtered exclusions.
        /// </summary>
        public void ClearFilteredExclusions()
        {
            foreach( var person in People)
            {
                person.ClearFilteredExclusions();
            }
        }

        /// <summary>
        /// Gets the people.
        /// </summary>
        /// <param name="selectedOnly">if set to <c>true</c> [selected only].</param>
        /// <returns></returns>
        public List<CheckInPerson> GetPeople( bool selectedOnly )
        {
            if ( selectedOnly )
            {
                return People.Where( p => p.Selected ).ToList();
            }
            return People;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( !string.IsNullOrWhiteSpace( Caption ) )
            {
                return Caption;
            }
            else
            {
                return Group != null ? Group.ToString() : string.Empty;
            }
        }

    }

    /// <summary>
    /// The type of checkin
    /// </summary>
    public enum CheckinAction
    {
        /// <summary>
        /// The individual
        /// </summary>
        CheckIn = 0,

        /// <summary>
        /// The family
        /// </summary>
        CheckOut = 1
    }
}