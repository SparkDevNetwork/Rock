//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Model;

namespace Rock.CheckIn
{
    /// <summary>
    /// The label details
    /// </summary>
    [DataContract]
    public class CheckInLabel
    {
        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        [DataMember]
        public string PrinterAddress { get; set; }

        /// <summary>
        /// Gets or sets the the unique code for check-in labels
        /// </summary>
        /// <value>
        /// The security code.
        /// </value>
        [DataMember]
        public string LabelFile { get; set; }

        /// <summary>
        /// Gets or sets the label key.
        /// </summary>
        /// <value>
        /// The label key.
        /// </value>
        [DataMember]
        public Guid LabelKey { get; set; }

        /// <summary>
        /// Gets or sets the merge fields.
        /// </summary>
        /// <value>
        /// The merge fields.
        /// </value>
        [DataMember]
        public Dictionary<string, string> MergeFields { get; set; }
    }
}