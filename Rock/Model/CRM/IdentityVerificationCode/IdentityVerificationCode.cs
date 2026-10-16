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

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Enums.Security;
using Rock.Security;
using Rock.Utility;

namespace Rock.Model
{
    /*
        10/15/2026 - MSE

        These are live sign-in codes for phone number lookup, so the generic
        v1 REST controller (secured only by the controller VIEW rule) must
        not exist for this model.

        Reason: Keep sign-in verification codes private.
    */
    /// <summary>
    /// This class represents all of the verification codes that can be used by the phone verification system.
    /// </summary>
    [RockDomain( "CRM" )]
    [Table( "IdentityVerificationCode" )]
    [CodeGenExclude( CodeGenFeature.DefaultRestController )]
    [CodeGenerateRest( Enums.CodeGenerateRestEndpoint.ReadOnly, DisableEntitySecurity = true )]
    [Rock.SystemGuid.EntityTypeGuid( "3FCB8972-C319-4262-9D6E-3D60E1C4E463")]
    public partial class IdentityVerificationCode : Model<IdentityVerificationCode>
    {
        #region Properties
        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        /*
            10/15/2026 - MSE

            The code is a live sign-in secret that must only reach the
            individual it was sent to, so it is never serialized, rendered
            by Lava or shown in reporting. Server code reads the property
            directly.

            Reason: Keep sign-in verification codes private.
        */
        [Required]
        [IgnoreDataMember]
        [Newtonsoft.Json.JsonIgnore]
        [HideFromReporting]
        [MaxLength( 6 )]
        [Index( IsUnique = true )]
        [StringValidation( StringValidationProfile.PlainText )]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the last issue date time.
        /// </summary>
        /// <value>
        /// The last issue date time.
        /// </value>
        [DataMember]
        public DateTime? LastIssueDateTime { get; set; }
        #endregion Properties
    }

    #region Entity Configuration

    /// <summary>
    /// IdentityVerificationCode Configuration Class
    /// </summary>
    public partial class IdentityVerificationCodeConfiguration : EntityTypeConfiguration<IdentityVerificationCode>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityVerificationCodeConfiguration"/> class.
        /// </summary>
        public IdentityVerificationCodeConfiguration()
        {
            // Empty constructor. This is required to tell EF that this model exists.
        }
    }

    #endregion Entity Configuration
}
