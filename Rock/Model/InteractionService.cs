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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{

    /// <summary>
    /// Represents a Interation Service.
    /// </summary>
    [Table( "InteractionService" )]
    [DataContract]
    public partial class InteractionService : Model<InteractionService>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the interaction service data.
        /// </summary>
        /// <value>
        /// The interaction service data.
        /// </value>
        [DataMember]
        public string ServiceData { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of entity that was modified. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of the entity that was modified.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int ComponentEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of entity that was modified. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of the entity that was modified.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int InteractionEntityTypeId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the Component Entity.
        /// </summary>
        /// <value>
        /// The type of the component entity.
        /// </value>
        [DataMember]
        public virtual Model.EntityType ComponentEntityType { get; set; }

        /// <summary>
        /// Gets or sets the type of the interaction Entity.
        /// </summary>
        /// <value>
        /// The type of the interaction entity.
        /// </value>
        [DataMember]
        public virtual Model.EntityType InteractionEntityType { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Service Type <see cref="Rock.Model.DefinedValue" /> representing what type of Interaction Service this is.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.DefinedValue"/> identifying the interaction service type. If no value is selected this can be null.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.INTERACTION_SERVICE_TYPE )]
        public int? ServiceTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the retention days.
        /// </summary>
        /// <value>
        /// The retention days.
        /// </value>
        [DataMember]
        public int? RetentionDuration { get; set; }

        #endregion

        #region Public Methods


        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class InteractionServiceConfiguration : EntityTypeConfiguration<InteractionService>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionServiceConfiguration"/> class.
        /// </summary>
        public InteractionServiceConfiguration()
        {
           
        }
    }

    #endregion
}
