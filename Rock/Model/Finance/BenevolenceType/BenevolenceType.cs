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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Represents a benevolence type that will be associated with a benevolence request.
    /// </summary>
    [RockDomain( "Finance" )]
    [Table( "BenevolenceType" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "9DB5D35A-F2DF-4AFF-AB9F-06C2EB587C0D")]
    public partial class BenevolenceType : Model<BenevolenceType>
    {
        #region Entity Properties
        /// <summary>
        /// Gets or sets the <see cref="Name"/> value on the <see cref="BenevolenceType"/>. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> containing the <see cref="Name"/>.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        [Previewable]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Description"/> value on the <see cref="BenevolenceType"/>. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> containing the <see cref="Description"/>.
        /// </value>
        [DataMember]
        [Previewable]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IsActive"/> value on the <see cref="BenevolenceType"/>. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean" /> containing the <see cref="IsActive"/>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Previewable]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="RequestLavaTemplate"/> value on the <see cref="BenevolenceType"/>. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> containing the <see cref="RequestLavaTemplate"/>.
        /// </value>
        [DataMember]
        public string RequestLavaTemplate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show financial results].
        /// </summary>
        /// <value><c>true</c> if [show financial results]; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool ShowFinancialResults { get; set; }

        /// <summary>
        /// Gets or sets the additional settings json.
        /// </summary>
        /// <value>
        /// The additional settings json.
        /// </value>
        [DataMember]
        public string AdditionalSettingsJson { get; set; }

        #endregion Entity Properties

        #region Navigation Properties
        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.BenevolenceWorkflow"></see> that is associated with a <see cref="Rock.Model.BenevolenceType"></see>.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.BenevolenceWorkflow"></see> that are associated with a <see cref="Rock.Model.BenevolenceType"></see>.
        /// </value>
        [LavaVisible]
        public virtual ICollection<BenevolenceWorkflow> BenevolenceWorkflows
        {
            get { return _benevolenceWorkflows ?? ( _benevolenceWorkflows = new Collection<BenevolenceWorkflow>() ); }
            set { _benevolenceWorkflows = value; }
        }

        private ICollection<BenevolenceWorkflow> _benevolenceWorkflows;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.BenevolenceRequest"></see> that is associated with a <see cref="Rock.Model.BenevolenceType"></see>.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.BenevolenceRequest"></see> that are associated with a <see cref="Rock.Model.BenevolenceType"></see>.
        /// </value>
        [LavaVisible]
        public virtual ICollection<BenevolenceRequest> BenevolenceRequests
        {
            get { return _benevolenceRequests ?? ( _benevolenceRequests = new Collection<BenevolenceRequest>() ); }
            set { _benevolenceRequests = value; }
        }

        private ICollection<BenevolenceRequest> _benevolenceRequests;
        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// BenevolenceType Configuration Class
    /// </summary>
    public partial class BenevolenceTypeConfiguration : EntityTypeConfiguration<BenevolenceType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BenevolenceTypeConfiguration"/> class.
        /// </summary>
        public BenevolenceTypeConfiguration()
        {
            // Empty constructor. This is required to tell EF that this model exists.
        }
    }

    #endregion Entity Configuration
}
