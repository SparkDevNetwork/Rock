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
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
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
        /// Gets or sets the printer device id.
        /// </summary>
        /// <value>
        /// The printer address.
        /// </value>
        [DataMember]
        public int? PrinterDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the printer address.
        /// </summary>
        /// <value>
        /// The printer address.
        /// </value>
        [DataMember]
        public string PrinterAddress { get; set; }

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
        /// Gets or sets the file unique identifier.
        /// </summary>
        /// <value>
        /// The file unique identifier.
        /// </value>
        [DataMember]
        public Guid FileGuid { get; set; }

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
        public string LabelKey { get; set; }

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
        public CheckInLabel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInLabel" /> class.
        /// </summary>
        /// <param name="kioskLabel">The label.</param>
        /// <param name="mergeObjects">The merge objects.</param>
        public CheckInLabel( KioskLabel kioskLabel, Dictionary<string, object> mergeObjects )
        {
            LabelKey = kioskLabel.Guid.ToString();
            LabelFile = kioskLabel.Url;

            MergeFields = new Dictionary<string, string>();
            foreach ( var item in kioskLabel.MergeFields )
            {
                MergeFields.Add( item.Key, item.Value.ResolveMergeFields( mergeObjects ) );
                MergeFields[item.Key] = HttpUtility.HtmlDecode( MergeFields[item.Key] );
            }
        }

    }
}