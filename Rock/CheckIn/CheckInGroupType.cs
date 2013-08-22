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
    /// A group type option for the current check-in
    /// </summary>
    [DataContract]
    public class CheckInGroupType : DotLiquid.ILiquidizable
    {
        /// <summary>
        /// Gets or sets the type of the group.
        /// </summary>
        /// <value>
        /// The type of the group.
        /// </value>
        [DataMember]
        public GroupType GroupType { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CheckInGroupType" /> is selected for check-in
        /// </summary>
        /// <value>
        ///   <c>true</c> if selected; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool Selected { get; set; }

        /// <summary>
        /// Gets or sets the last time person checked in to any of the Locations for this group type
        /// </summary>
        /// <value>
        /// The last check-in.
        /// </value>
        [DataMember]
        public DateTime? LastCheckIn { get; set; }

        /// <summary>
        /// Gets or sets the groups that are of the current group type
        /// </summary>
        /// <value>
        /// The locations.
        /// </value>
        [DataMember]
        public List<CheckInGroup> Groups { get; set; }

        /// <summary>
        /// Gets or sets the labels to be printed after succesful check-in
        /// </summary>
        /// <value>
        /// The labels.
        /// </value>
        [DataMember]
        public List<CheckInLabel> Labels { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInGroupType" /> class.
        /// </summary>
        public CheckInGroupType()
            : base()
        {
            Groups = new List<CheckInGroup>();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return GroupType != null ? GroupType.ToString() : string.Empty;
        }

        /// <summary>
        /// To the liquid.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object ToLiquid()
        {
            var dictionary = new Dictionary<string, object>();
            dictionary.Add( "GroupType", GroupType );
            dictionary.Add( "Selected", Selected );
            dictionary.Add( "LastCheckIn", LastCheckIn );
            dictionary.Add( "Groups", Groups );
            dictionary.Add( "Labels", Labels );
            return dictionary;
        }
    }
}