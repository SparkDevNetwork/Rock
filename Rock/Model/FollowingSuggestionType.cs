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
using Rock.Follow;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a following suggestion type
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "FollowingSuggestionType" )]
    [DataContract]
    public partial class FollowingSuggestionType : Model<FollowingSuggestionType>, IOrdered, IHasActiveFlag
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the (internal) Name of the FollowingSuggestion. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the (internal) name of the FollowingSuggestion.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user defined description of the FollowingSuggestion.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the user defined description of the FollowingSuggestion.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the reason note to use when suggesting an entity be followed
        /// </summary>
        /// <value>
        /// The reason note.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember]
        public string ReasonNote { get; set; }

        /// <summary>
        /// Gets or sets the reminder days.
        /// </summary>
        /// <value>
        /// The reminder days.
        /// </value>
        [DataMember]
        public int? ReminderDays { get; set; }

        /// <summary>
        /// Gets or sets the suggestion entity type identifier.
        /// </summary>
        /// <value>
        /// The suggestion entity type identifier.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }
        private bool _isActive = true;

        /// <summary>
        /// Gets or sets how an entity should be formatted when included in the suggestion notification to follower.
        /// </summary>
        /// <value>
        /// The item notification lava.
        /// </value>
        [DataMember]
        public string EntityNotificationFormatLava { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the suggestion entity.
        /// </summary>
        /// <value>
        /// The type of the suggestion entity.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FollowingSuggestionType"/> class.
        /// </summary>
        public FollowingSuggestionType()
        {
            IsActive = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the suggestion component.
        /// </summary>
        /// <returns></returns>
        public virtual SuggestionComponent GetSuggestionComponent()
        {
            if ( EntityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Get( EntityTypeId.Value );
                if ( entityType != null )
                {
                    return SuggestionContainer.GetComponent( entityType.Name );
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this FollowingSuggestion.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this FollowingSuggestion.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// FollowingSuggestion Configuration class.
    /// </summary>
    public partial class FollowingSuggestionConfiguration : EntityTypeConfiguration<FollowingSuggestionType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FollowingSuggestionConfiguration"/> class.
        /// </summary>
        public FollowingSuggestionConfiguration()
        {
            this.HasRequired( g => g.EntityType).WithMany().HasForeignKey( a => a.EntityTypeId).WillCascadeOnDelete( false );
        }
    }

    #endregion

}