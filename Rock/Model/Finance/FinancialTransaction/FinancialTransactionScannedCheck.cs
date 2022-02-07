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
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Special Class to use when uploading a FinancialTransaction from a Scanned Check thru the Rest API.
    /// The Rest Client can't be given access to the DataEncryptionKey, so they'll upload it (using SSL)
    /// with the plain text CheckMicr and the Rock server will encrypt prior to saving to database
    /// </summary>
    [DataContract]
    [NotMapped]
    [RockClientInclude( "Special Class to use when uploading a FinancialTransaction from a Scanned Check thru the Rest API" )]
    public class FinancialTransactionScannedCheck
    {
        /// <summary>
        /// Gets or sets the financial transaction.
        /// </summary>
        /// <value>
        /// The financial transaction.
        /// </value>
        [DataMember]
        public FinancialTransaction FinancialTransaction { get; set; }

        /// <summary>
        /// Gets or sets the scanned check MICR (the raw track data)
        /// </summary>
        /// <value>
        /// The scanned check MICR.
        /// </value>
        [DataMember]
        public string ScannedCheckMicrData { get; set; }

        /// <summary>
        /// Gets or sets the scanned check parsed MICR in the format {routingnumber}_{accountnumber}_{checknumber}
        /// </summary>
        /// <value>
        /// The scanned check micr parts.
        /// </value>
        [DataMember]
        public string ScannedCheckMicrParts { get; set; }
    }
}
