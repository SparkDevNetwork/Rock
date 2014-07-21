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
using System.Collections.Generic;
using System.Runtime.Serialization;

using Rock.Model;

namespace Rock.CheckIn
{
    /// <summary>
    /// A person option for the current check-in
    /// </summary>
    [DataContract]
    public class CheckInPerson : DotLiquid.ILiquidizable
    {
        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        [DataMember]
        public Person Person { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this person is a part of the family (vs. from a relationship).
        /// </summary>
        /// <value>
        ///   <c>true</c> if family member; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool FamilyMember { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [excluded by filter].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [excluded by filter]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ExcludedByFilter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CheckInPerson" /> was pre-selected by a check-in action.
        /// </summary>
        /// <value>
        ///   <c>true</c> if preselected; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool PreSelected { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CheckInPerson" /> was selected for check-in.
        /// </summary>
        /// <value>
        ///   <c>true</c> if selected; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool Selected { get; set; }

        /// <summary>
        /// Gets or sets the last time person checked in to any of the GroupTypes
        /// </summary>
        /// <value>
        /// The last check-in.
        /// </value>
        [DataMember]
        public DateTime? LastCheckIn { get; set; }

        /// <summary>
        /// Gets or sets the the unique code for check-in labels
        /// </summary>
        /// <value>
        /// The security code.
        /// </value>
        [DataMember]
        public string SecurityCode { get; set; }

        /// <summary>
        /// Gets or sets the group types available for the current person.
        /// </summary>
        /// <value>
        /// The group types.
        /// </value>
        [DataMember]
        public List<CheckInGroupType> GroupTypes { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInPerson" /> class.
        /// </summary>
        public CheckInPerson()
            : base()
        {
            GroupTypes = new List<CheckInGroupType>();
        }

        /// <summary>
        /// Clears the filtered exclusions.
        /// </summary>
        public void ClearFilteredExclusions()
        {
            ExcludedByFilter = false;
            foreach ( var groupType in GroupTypes )
            {
                groupType.ClearFilteredExclusions();
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Person != null ? Person.ToString() : string.Empty;
        }

        /// <summary>
        /// To the liquid.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object ToLiquid()
        {
            var dictionary = Person.ToLiquid() as Dictionary<string, object>;
            if ( dictionary != null )
            {
                dictionary.Add( "FamilyMember", FamilyMember );
                dictionary.Add( "LastCheckIn", LastCheckIn );
                dictionary.Add( "SecurityCode", SecurityCode );
                return dictionary;
            }

            return new Dictionary<string, object>();
        }
    }
}