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
    public class CheckInLabel : DotLiquid.ILiquidizable
    {
        /// <summary>
        /// Gets or sets the printer device id.
        /// </summary>
        /// <value>
        /// The printer address.
        /// </value>
        [DataMember]
        public int? PrinterDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the print from.
        /// </summary>
        /// <value>
        /// The print from.
        /// </value>
        [DataMember]
        public PrintFrom PrintFrom { get; set; }

        /// <summary>
        /// Gets or sets the print to.
        /// </summary>
        /// <value>
        /// The print to.
        /// </value>
        [DataMember]
        public PrintTo PrintTo { get; set; }

        /// <summary>
        /// Gets or sets the label file.
        /// </summary>
        /// <value>
        /// The label file.
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

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInLabel" /> class.
        /// </summary>
        /// <param name="labelFile">The label file.</param>
        public CheckInLabel( string labelFile )
        {
            LabelFile = labelFile;
        }

        /// <summary>
        /// To the liquid.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object ToLiquid()
        {
            var dictionary = new Dictionary<string, object>();
            dictionary.Add( "PrinterDeviceId", PrinterDeviceId );
            dictionary.Add( "PrintFrom", PrintFrom.ConvertToString() );
            dictionary.Add( "PrintTo", PrintTo.ConvertToString() );
            dictionary.Add( "LabelFile", LabelFile );
            dictionary.Add( "LabelKey", LabelKey );
            dictionary.Add( "MergeFields", MergeFields );
            return dictionary;
        }
    }
}