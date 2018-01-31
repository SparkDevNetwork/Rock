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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a log entry from when Rock makes a call to an external service.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "ServiceLog" )]
    [DataContract]
    public partial class ServiceLog : Model<ServiceLog>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the date and time that the log entry was created 
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime" /> representing when the log entry was created.
        /// </value>
        [DataMember]
        public DateTime? LogDateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the data that was sent to the service.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the data that was sent to the service.
        /// </value>
        [DataMember]
        public string Input { get; set; }
        
        /// <summary>
        /// Gets or sets the type of service that was run
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the type of service that was run.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string Type { get; set; }
        
        /// <summary>
        /// Gets or sets the component name for the service.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the component name for the service.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the result that was returned from the service.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the result that was returned from the service.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string Result { get; set; }
        
        /// <summary>
        /// Gets or sets a flag indicating if the service returned a successful result. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if the service returned a successful result; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool Success { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

    }

    #region Entity Configuration
    
    /// <summary>
    /// Service Log Configuration class.
    /// </summary>
    public partial class ServiceLogConfiguration : EntityTypeConfiguration<ServiceLog>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceLogConfiguration"/> class.
        /// </summary>
        public ServiceLogConfiguration()
        {
        }
    }

    #endregion

}
