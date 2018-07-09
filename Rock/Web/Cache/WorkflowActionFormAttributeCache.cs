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

using Rock.Cache;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Cached WorkflowActionFormAttribute
    /// </summary>
    [Serializable]
    [Obsolete( "Use Rock.Cache.CacheWorkflowActionFormAttribute instead" )]
    public class WorkflowActionFormAttributeCache : CachedModel<WorkflowActionFormAttribute>
    {
        #region Constructors

        private WorkflowActionFormAttributeCache( CacheWorkflowActionFormAttribute cacheItem )
        {
            CopyFromNewCache( cacheItem );
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the form Id
        /// </summary>
        /// <value>
        /// The Form Id.
        /// </value>
        public int WorkflowActionFormId { get; set; }

        /// <summary>
        /// Gets or sets the attribute identifier.
        /// </summary>
        /// <value>
        /// The attribute identifier.
        /// </value>
        public int AttributeId { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is visible].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is visible]; otherwise, <c>false</c>.
        /// </value>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is read only].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is read only]; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is required].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is required]; otherwise, <c>false</c>.
        /// </value>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [hide label].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hide label]; otherwise, <c>false</c>.
        /// </value>
        public bool HideLabel { get; set; }

        /// <summary>
        /// Gets or sets the pre HTML.
        /// </summary>
        /// <value>
        /// The pre HTML.
        /// </value>
        public string PreHtml { get; set; }

        /// <summary>
        /// Gets or sets the post HTML.
        /// </summary>
        /// <value>
        /// The post HTML.
        /// </value>
        public string PostHtml { get; set; }

        /// <summary>
        /// Gets the workflow action form.
        /// </summary>
        /// <value>
        /// The workflow action form.
        /// </value>
        public WorkflowActionFormCache WorkflowActionForm => WorkflowActionFormCache.Read( WorkflowActionFormId );

        /// <summary>
        /// Gets the attribute.
        /// </summary>
        /// <value>
        /// The attribute.
        /// </value>
        public AttributeCache Attribute => AttributeCache.Read( AttributeId );

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( IEntity model )
        {
            base.CopyFromModel( model );

            if ( !( model is WorkflowActionFormAttribute ) ) return;

            var workflowActionFormAttribute = (WorkflowActionFormAttribute)model;
            WorkflowActionFormId = workflowActionFormAttribute.WorkflowActionFormId;
            AttributeId = workflowActionFormAttribute.AttributeId;
            Order = workflowActionFormAttribute.Order;
            IsVisible = workflowActionFormAttribute.IsVisible;
            IsReadOnly = workflowActionFormAttribute.IsReadOnly;
            IsRequired = workflowActionFormAttribute.IsRequired;
            HideLabel = workflowActionFormAttribute.HideLabel;
            PreHtml = workflowActionFormAttribute.PreHtml;
            PostHtml = workflowActionFormAttribute.PostHtml;
        }

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected sealed override void CopyFromNewCache( IEntityCache cacheEntity )
        {
            base.CopyFromNewCache( cacheEntity );

            if ( !( cacheEntity is CacheWorkflowActionFormAttribute ) ) return;

            var workflowActionFormAttribute = (CacheWorkflowActionFormAttribute)cacheEntity;
            WorkflowActionFormId = workflowActionFormAttribute.WorkflowActionFormId;
            AttributeId = workflowActionFormAttribute.AttributeId;
            Order = workflowActionFormAttribute.Order;
            IsVisible = workflowActionFormAttribute.IsVisible;
            IsReadOnly = workflowActionFormAttribute.IsReadOnly;
            IsRequired = workflowActionFormAttribute.IsRequired;
            HideLabel = workflowActionFormAttribute.HideLabel;
            PreHtml = workflowActionFormAttribute.PreHtml;
            PostHtml = workflowActionFormAttribute.PostHtml;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Returns WorkflowActionFormAttribute object from cache.  If workflowActionFormAttribute does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowActionFormAttributeCache Read( int id, RockContext rockContext = null )
        {
            return new WorkflowActionFormAttributeCache( CacheWorkflowActionFormAttribute.Get( id, rockContext ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowActionFormAttributeCache Read( Guid guid, RockContext rockContext = null )
        {
            return new WorkflowActionFormAttributeCache( CacheWorkflowActionFormAttribute.Get( guid, rockContext ) );
        }

        /// <summary>
        /// Reads the specified defined type model.
        /// </summary>
        /// <param name="workflowActionFormAttributeModel">The defined type model.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowActionFormAttributeCache Read( WorkflowActionFormAttribute workflowActionFormAttributeModel, RockContext rockContext = null )
        {
            return new WorkflowActionFormAttributeCache( CacheWorkflowActionFormAttribute.Get( workflowActionFormAttributeModel ) );
        }

        /// <summary>
        /// Removes workflowActionFormAttribute from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            CacheWorkflowActionFormAttribute.Remove( id );
        }

        #endregion
    }
}