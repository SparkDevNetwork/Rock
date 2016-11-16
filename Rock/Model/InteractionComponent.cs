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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents Component for <see cref="Rock.Model.Interaction">Interaction</see>
    /// </summary>
    [Table( "InteractionComponent" )]
    [DataContract]
    public partial class InteractionComponent : Model<InteractionComponent>
    {

        /// <summary>
        /// Gets or sets the interaction component data.
        /// </summary>
        /// <value>
        /// The interaction component data.
        /// </value>
        [DataMember]
        public string ComponentData { get; set; }

        /// <summary>
        /// Gets or sets the Id of the entity that this history is related to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the entity (object) that this history is related to.
        /// </value>
        [Required]
        [DataMember]
        public int EntityId { get; set; }

    }
}
