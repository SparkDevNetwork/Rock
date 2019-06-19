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
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Cached WorkflowActionFormAttribute
    /// </summary>
    [Serializable]
    [DataContract]
    public class WorkflowActionFormAttributeCache : ModelCache<WorkflowActionFormAttributeCache, WorkflowActionFormAttribute>
    {

        #region Properties

        /// <summary>
        /// Gets or sets the form Id
        /// </summary>
        /// <value>
        /// The Form Id.
        /// </value>
        [DataMember]
        public int WorkflowActionFormId { get; private set; }

        /// <summary>
        /// Gets or sets the attribute identifier.
        /// </summary>
        /// <value>
        /// The attribute identifier.
        /// </value>
        [DataMember]
        public int AttributeId { get; private set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is visible].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is visible]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsVisible { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is read only].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is read only]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsReadOnly { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is required].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is required]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsRequired { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [hide label].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hide label]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool HideLabel { get; private set; }

        /// <summary>
        /// Gets or sets the pre HTML.
        /// </summary>
        /// <value>
        /// The pre HTML.
        /// </value>
        [DataMember]
        public string PreHtml { get; private set; }

        /// <summary>
        /// Gets or sets the post HTML.
        /// </summary>
        /// <value>
        /// The post HTML.
        /// </value>
        [DataMember]
        public string PostHtml { get; private set; }

        /// <summary>
        /// Gets the workflow action form.
        /// </summary>
        /// <value>
        /// The workflow action form.
        /// </value>
        public WorkflowActionFormCache WorkflowActionForm => WorkflowActionFormCache.Get( WorkflowActionFormId );

        /// <summary>
        /// Gets the attribute.
        /// </summary>
        /// <value>
        /// The attribute.
        /// </value>
        public AttributeCache Attribute => AttributeCache.Get( AttributeId );

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var workflowActionFormAttribute = entity as WorkflowActionFormAttribute;
            if ( workflowActionFormAttribute == null ) return;

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
    }
}