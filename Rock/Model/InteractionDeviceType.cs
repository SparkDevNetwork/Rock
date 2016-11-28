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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{

    /// <summary>
    /// Represents Device Type for <see cref="Rock.Model.Interaction">Interaction</see>
    /// </summary>
    [NotAudited]
    [Table( "InteractionDeviceType" )]
    [DataContract]
    public partial class InteractionDeviceType : Model<InteractionDeviceType>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the interaction device type name.
        /// </summary>
        /// <value>
        /// The interaction device type name.
        /// </value>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the device type data.
        /// </summary>
        /// <value>
        /// The device type data.
        /// </value>
        [DataMember]
        public string DeviceTypeData { get; set; }

        /// <summary>
        /// Gets or sets the type of client.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> client type.
        /// </value>
        [DataMember]
        [MaxLength( 25 )]
        public string ClientType { get; set; }

        /// <summary>
        /// Gets or sets the operating system.
        /// </summary>
        /// <value>
        /// The operating system.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string OperatingSystem { get; set; }

        /// <summary>
        /// Gets or sets the operating system.
        /// </summary>
        /// <value>
        /// The operating system.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string Application { get; set; }

        #endregion

        #region Virtual Properties

        #endregion

        #region Public Methods

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class InteractionDeviceTypeConfiguration : EntityTypeConfiguration<InteractionDeviceType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionDeviceTypeConfiguration"/> class.
        /// </summary>
        public InteractionDeviceTypeConfiguration()
        {
          
        }
    }

    #endregion

}
