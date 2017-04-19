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
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Cached WorkflowActionFormAttribute
    /// </summary>
    [Serializable]
    public class WorkflowActionFormAttributeCache : CachedModel<WorkflowActionFormAttribute>
    {
        #region Constructors

        private WorkflowActionFormAttributeCache( WorkflowActionFormAttribute model )
        {
            CopyFromModel( model );
        }

        #endregion

        #region Properties

        private object _obj = new object();

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
        public WorkflowActionFormCache  WorkflowActionForm
        {
            get
            {
                return WorkflowActionFormCache.Read( WorkflowActionFormId );
            }
        }

        /// <summary>
        /// Gets the attribute.
        /// </summary>
        /// <value>
        /// The attribute.
        /// </value>
        public AttributeCache Attribute
        {
            get
            {
                return AttributeCache.Read( AttributeId );
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is WorkflowActionFormAttribute )
            {
                var workflowActionFormAttribute = (WorkflowActionFormAttribute)model;

                this.WorkflowActionFormId = workflowActionFormAttribute.WorkflowActionFormId;
                this.AttributeId = workflowActionFormAttribute.AttributeId;
                this.Order = workflowActionFormAttribute.Order;
                this.IsVisible = workflowActionFormAttribute.IsVisible;
                this.IsReadOnly = workflowActionFormAttribute.IsReadOnly;
                this.IsRequired = workflowActionFormAttribute.IsRequired;
                this.HideLabel = workflowActionFormAttribute.HideLabel;
                this.PreHtml = workflowActionFormAttribute.PreHtml;
                this.PostHtml = workflowActionFormAttribute.PostHtml;
            }
        }

        #endregion

        #region Static Methods

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:WorkflowActionFormAttribute:{0}", id );
        }

        /// <summary>
        /// Returns WorkflowActionFormAttribute object from cache.  If workflowActionFormAttribute does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowActionFormAttributeCache Read( int id, RockContext rockContext = null )
        {
            return GetOrAddExisting( WorkflowActionFormAttributeCache.CacheKey( id ),
                () => LoadById( id, rockContext ) );
        }

        private static WorkflowActionFormAttributeCache LoadById( int id, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return LoadById2( id, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return LoadById2( id, rockContext2 );
            }
        }

        private static WorkflowActionFormAttributeCache LoadById2( int id, RockContext rockContext )
        {
            var workflowActionFormAttributeService = new WorkflowActionFormAttributeService( rockContext );
            var workflowActionFormAttributeModel = workflowActionFormAttributeService
                .Queryable()
                .Where( t => t.Id == id )
                .FirstOrDefault();

            if ( workflowActionFormAttributeModel != null )
            {
                return new WorkflowActionFormAttributeCache( workflowActionFormAttributeModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowActionFormAttributeCache Read( Guid guid, RockContext rockContext = null )
        {
            int id = GetOrAddExisting( guid.ToString(),
                () => LoadByGuid( guid, rockContext ) );

            return Read( id, rockContext );
        }

        private static int LoadByGuid( Guid guid, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return LoadByGuid2( guid, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return LoadByGuid2( guid, rockContext2 );
            }
        }

        private static int LoadByGuid2( Guid guid, RockContext rockContext )
        {
            var workflowActionFormAttributeService = new WorkflowActionFormAttributeService( rockContext );
            return workflowActionFormAttributeService
                .Queryable().AsNoTracking()
                .Where( c => c.Guid.Equals( guid ) )
                .Select( c => c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Reads the specified defined type model.
        /// </summary>
        /// <param name="workflowActionFormAttributeModel">The defined type model.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowActionFormAttributeCache Read( WorkflowActionFormAttribute workflowActionFormAttributeModel, RockContext rockContext = null )
        {
            return GetOrAddExisting( WorkflowActionFormAttributeCache.CacheKey( workflowActionFormAttributeModel.Id ),
                () => LoadByModel( workflowActionFormAttributeModel ) );
        }

        private static WorkflowActionFormAttributeCache LoadByModel( WorkflowActionFormAttribute workflowActionFormAttributeModel )
        {
            if ( workflowActionFormAttributeModel != null )
            {
                return new WorkflowActionFormAttributeCache( workflowActionFormAttributeModel );
            }
            return null;
        }

        /// <summary>
        /// Removes workflowActionFormAttribute from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            FlushCache( WorkflowActionFormAttributeCache.CacheKey( id ) );
        }

        #endregion
    }
}