//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Runtime.Serialization;

using Rock.Model;

namespace Rock.CheckIn
{
    /// <summary>
    /// A family option for the current check-in
    /// </summary>
    [DataContract]
    public class CheckInFamily : DotLiquid.ILiquidizable
    {
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
        /// Initializes a new instance of the <see cref="CheckInFamily" /> class.
        /// </summary>
        public CheckInFamily()
            : base()
        {
            People = new List<CheckInPerson>();
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

        /// <summary>
        /// To the liquid.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object ToLiquid()
        {
            var dictionary = new Dictionary<string, object>();
            dictionary.Add( "Group", Group );
            dictionary.Add( "Selected", Selected );
            dictionary.Add( "People", People );
            dictionary.Add( "Caption", Caption );
            dictionary.Add( "SubCaption", SubCaption );
            return dictionary;
        }
    }
}