//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
    public class CheckInPerson 
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
        public bool FamilyMember { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CheckInPerson" /> is selected for check-in.
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
        /// The last check in.
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
        /// Gets or sets the labels to be printed after succesful checkin
        /// </summary>
        /// <value>
        /// The labels.
        /// </value>
        [DataMember]
        public List<CheckInLabel> Labels { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInPerson" /> class.
        /// </summary>
        public CheckInPerson()
            : base()
        {
            GroupTypes = new List<CheckInGroupType>();
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
    }
}