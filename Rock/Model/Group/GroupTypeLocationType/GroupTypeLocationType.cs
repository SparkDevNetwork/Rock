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
//
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Represents the a type of <see cref="Rock.Model.Location"/> that is supported by a <see cref="Rock.Model.GroupType"/>.
    /// </summary>
    [Table( "GroupTypeLocationType" )]
    [DataContract]
    public class GroupTypeLocationType: ILavaDataDictionarySource
    {
        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.GroupType"/>. This property is required, and is part of the key.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.GroupType"/>.
        /// </value>
        [Key]
        [Column(Order=0)]
        [DataMember]
        public int GroupTypeId { get; set; }


        /// <summary>
        /// Gets or sets the Id of the LocationType <see cref="Rock.Model.DefinedValue"/> that represents a type of <see cref="Rock.Model.Location"/> that is
        /// supported by a <see cref="Rock.Model.GroupType"/>. This property is required and is part of the key.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of a LocationType <see cref="Rock.Model.DefinedValue"/>  that is supported by a <see cref="Rock.Model.GroupType"/>.
        /// </value>
        [Key]
        [Column( Order = 1 )]
        [DataMember]
        public int LocationTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupType"/>.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.GroupType"/>.
        /// </value>
        [DataMember]
        public virtual GroupType GroupType { get; set; }

        /// <summary>
        /// Gets or sets the a <see cref="Rock.Model.DefinedType"/> that is supported by the <see cref="Rock.Model.GroupType"/>.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.GroupType" /> that is supported by the <see cref="Rock.Model.GroupType"/>.
        /// </value>
        [DataMember]
        public virtual Model.DefinedValue LocationTypeValue { get; set; }

        ILavaDataDictionary ILavaDataDictionarySource.GetLavaDataDictionary()
        {
            return new LavaDataDictionary( LocationTypeValue );
        }

        /// <summary>
        /// To the liquid.
        /// </summary>
        /// <returns></returns>
        [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
        [Rock.RockObsolete( "18.0" )]
        public object ToLiquid()
        {
            return LocationTypeValue;
        }
    }

    #region Entity Configuration

    /// <summary>
    /// GroupTypeLocationType Configuration Class
    /// </summary>
    public partial class GroupTypeLocationTypeConfiguration : EntityTypeConfiguration<GroupTypeLocationType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupTypeLocationTypeConfiguration"/> class.
        /// </summary>
        public GroupTypeLocationTypeConfiguration()
        {
            // Empty constructor. This is required to tell EF that this model exists.
        }
    }

    #endregion Entity Configuration
}
