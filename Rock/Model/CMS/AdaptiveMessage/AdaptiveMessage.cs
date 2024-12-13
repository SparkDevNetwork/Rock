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
using Newtonsoft.Json;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model

{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "AdaptiveMessage" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.ADAPTIVE_MESSAGE )]
    public partial class AdaptiveMessage : Model<AdaptiveMessage>, IHasActiveFlag, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
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
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; } = true;

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the collection of AdaptiveMessageAdaptations.
        /// </summary>
        /// <value>
        /// A collection of AdaptiveMessageAdaptations.
        /// </value>
        [DataMember, JsonIgnore]
        public virtual ICollection<AdaptiveMessageAdaptation> AdaptiveMessageAdaptations
        {
            get { return _adaptiveMessageAdaptations ?? ( _adaptiveMessageAdaptations = new Collection<AdaptiveMessageAdaptation>() ); }
            set { _adaptiveMessageAdaptations = value; }
        }

        private ICollection<AdaptiveMessageAdaptation> _adaptiveMessageAdaptations;

        /// <summary>
        /// Gets or sets the collection of <see cref="Rock.Model.Category">Categories</see> that this <see cref="AdaptiveMessage"/> is associated with.
        /// NOTE: Since changes to Categories isn't tracked by ChangeTracker, set the ModifiedDateTime if Categories are modified.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.Category">Categories</see> that this Content Channel is associated with.
        /// </value>
        [DataMember]
        public virtual ICollection<AdaptiveMessageCategory> AdaptiveMessageCategories
        {
            get { return _adaptiveMessageCategories ?? ( _adaptiveMessageCategories = new Collection<AdaptiveMessageCategory>() ); }
            set { _adaptiveMessageCategories = value; }
        }

        private ICollection<AdaptiveMessageCategory> _adaptiveMessageCategories;

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class AdaptiveMessageConfiguration : EntityTypeConfiguration<AdaptiveMessage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdaptiveMessageConfiguration" /> class.
        /// </summary>
        public AdaptiveMessageConfiguration()
        {
        }
    }

    #endregion Entity Configuration
}