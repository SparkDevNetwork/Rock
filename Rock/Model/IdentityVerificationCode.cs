﻿// <copyright>
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

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// This class represents all of the verification codes that can be used by the phone verification system.
    /// </summary>
    public partial class IdentityVerificationCode : Model<IdentityVerificationCode>
    {
        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [MaxLength( 6 )]
        [Index( IsUnique = true )]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the last issue date time.
        /// </summary>
        /// <value>
        /// The last issue date time.
        /// </value>
        [DataMember]
        public DateTime? LastIssueDateTime { get; set; }
    }
}
