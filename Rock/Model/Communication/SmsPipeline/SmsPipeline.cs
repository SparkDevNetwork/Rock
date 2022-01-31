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

namespace Rock.Model
{
    /// <summary>
    /// Represents a single pipeline for processing an SMS pipeline.
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "SmsPipeline" )]
    [DataContract]
    public class SmsPipeline : Model<SmsPipeline>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the pipeline.
        /// </summary>
        /// <value>
        /// The name of the pipeline.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [MaxLength( 100 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this pipeline is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this pipeline is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.SmsAction">SmsActions</see> which are associated with the SmsPipline.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.SmsAction">SmsActions</see> which are associated with the SmsPipline.
        /// </value>
        [DataMember]
        public virtual ICollection<SmsAction> SmsActions { get; set; } = new Collection<SmsAction>();

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// File Configuration class.
    /// </summary>
    public partial class SmsPipelineConfiguration : EntityTypeConfiguration<SmsPipeline>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsPipelineConfiguration"/> class.
        /// </summary>
        public SmsPipelineConfiguration()
        {
        }
    }

    #endregion
}
