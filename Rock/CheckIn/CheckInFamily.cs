//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

using Rock.Model;

namespace Rock.CheckIn
{
    /// <summary>
    /// A family option for the current check-in
    /// </summary>
    [DataContract]
    public class CheckInFamily
    {
        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [DataMember]
        public Group group { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CheckInFamily" /> is selected for check-in
        /// </summary>
        /// <value>
        ///   <c>true</c> if selected; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool Selected { get; set; }

        /// <summary>
        /// Gets or sets the members of the family
        /// </summary>
        /// <value>
        /// The members.
        /// </value>
        [DataMember]
        public List<CheckInPerson> FamilyMembers { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInFamily" /> class.
        /// </summary>
        public CheckInFamily()
            : base()
        {
            FamilyMembers = new List<CheckInPerson>();
        }
    }
}