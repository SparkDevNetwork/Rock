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
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents an <see cref="Rock.Model.WorkflowActionFormAttribute"/> used by a <see cref="Rock.Model.WorkflowActionForm"/>.
    /// </summary>
    [RockDomain( "Workflow" )]
    [Table( "WorkflowActionFormAttribute" )]
    [DataContract]
    public partial class WorkflowActionFormAttribute : Model<WorkflowActionFormAttribute>, IOrdered, ICacheable
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        /// <value>
        /// The header.
        /// </value>
        [Required]
        [DataMember]
        public int WorkflowActionFormId { get; set; }

        /// <summary>
        /// Gets or sets the attribute identifier.
        /// </summary>
        /// <value>
        /// The attribute identifier.
        /// </value>
        [Required]
        [DataMember]
        public int AttributeId { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [Required]
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is visible].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is visible]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is read only].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is read only]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is required].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is required]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [hide label].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hide label]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool HideLabel { get; set; }

        /// <summary>
        /// Gets or sets the pre HTML.
        /// </summary>
        /// <value>
        /// The pre HTML.
        /// </value>
        [DataMember]
        public string PreHtml { get; set; }

        /// <summary>
        /// Gets or sets the post HTML.
        /// </summary>
        /// <value>
        /// The post HTML.
        /// </value>
        [DataMember]
        public string PostHtml { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the workflow action form.
        /// </summary>
        /// <value>
        /// The workflow action form.
        /// </value>
        [LavaInclude]
        public virtual WorkflowActionForm WorkflowActionForm { get; set; }

        /// <summary>
        /// Gets or sets the attribute.
        /// </summary>
        /// <value>
        /// The attribute.
        /// </value>
        [DataMember]
        public virtual Attribute Attribute { get; set; }

        #endregion

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return WorkflowActionFormAttributeCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Data.DbContext dbContext )
        {
            WorkflowActionFormCache.UpdateCachedEntity( this.WorkflowActionFormId, EntityState.Modified );
            WorkflowActionFormAttributeCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion

        #region Methods

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Workflow Form attribute Configuration class.
    /// </summary>
    public partial class WorkflowActionFormAttributeConfiguration : EntityTypeConfiguration<WorkflowActionFormAttribute>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowActionFormAttributeConfiguration"/> class.
        /// </summary>
        public WorkflowActionFormAttributeConfiguration()
        {
            this.HasRequired( a => a.WorkflowActionForm ).WithMany( f => f.FormAttributes).HasForeignKey( a => a.WorkflowActionFormId ).WillCascadeOnDelete( true );
            this.HasRequired( a => a.Attribute ).WithMany().HasForeignKey( a => a.AttributeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

}

