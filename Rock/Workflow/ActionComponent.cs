// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Web;
using Rock.Data;
using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow
{
    /// <summary>
    /// Base class for components that perform actions for a workflow
    /// </summary>
    public abstract class ActionComponent : Component
    {
        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults
        {
            get
            {
                var defaults = new Dictionary<string, string>();
                defaults.Add( "Active", "True" );
                defaults.Add( "Order", "0" );
                return defaults;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionComponent" /> class.
        /// </summary>
        public ActionComponent()
        {
            // Override default constructor of Component that loads attributes (not needed for workflow actions, needs to be done by each action)
        }

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public abstract Boolean Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages );

        /// <summary>
        /// Loads the attributes.
        /// </summary>
        /// <exception cref="System.Exception">Workflow Action attributes are saved specific to the current action, which requires that the current action is included in order to load or retrieve values.  Use the LoadAttributes( WorkflowAction action ) method instead.</exception>
        [Obsolete("Use LoadAttributes( WorkflowAction action ) instead", true)]
        public void LoadAttributes()
        {
            // Compiler should generate error if referencing this method, so exception should never be thrown
            // but method is needed to "override" the extension method for IHasAttributes objects
            throw new Exception( "Workflow Action attributes are saved specific to the current action, which requires that the current action is included in order to load or retrieve values.  Use the LoadAttributes( WorkflowAction action ) method instead." );
        }

        /// <summary>
        /// Loads the attributes for the action.  The attributes are loaded by the framework prior to executing the action, 
        /// so typically workflow actions do not need to load the attributes
        /// </summary>
        /// <param name="action">The action.</param>
        public void LoadAttributes( WorkflowAction action )
        {
            action.ActionType.LoadAttributes();
        }

        /// <summary>
        /// Use GetAttributeValue( WorkflowAction action, string key) instead.  Workflow action attribute values are 
        /// specific to the action instance (rather than global).  This method will throw an exception
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Workflow Action attributes are saved specific to the current action, which requires that the current action is included in order to load or retrieve values.  Use the GetAttributeValue( WorkflowAction action, string key ) method instead.</exception>
        public override string GetAttributeValue( string key )
        {
            throw new Exception( "Workflow Action attributes are saved specific to the current action, which requires that the current action is included in order to load or retrieve values.  Use the GetAttributeValue( WorkflowAction action, string key ) method instead." );
        }

        /// <summary>
        /// Always returns 0.  (Ordering of actions is configured through the workflow admin and stored as property of WorkflowActionType)
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public override int Order
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Always returns true.  (Activating of actions is configured through the workflow admin and stored as a WorkflowActionType)
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public override bool IsActive
        {
            get
            {
                return true; ;
            }
        }

        /// <summary>
        /// Gets the attribute value for the action
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected string GetAttributeValue( WorkflowAction action, string key )
        {
            var actionType = action.ActionType;

            var values = actionType.AttributeValues;
            if ( values.ContainsKey( key ) )
            {
                var keyValues = values[key];
                if ( keyValues != null )
                {
                    return keyValues.Value;
                }
            }

            if ( actionType.Attributes != null &&
                actionType.Attributes.ContainsKey( key ))
            {
                return actionType.Attributes[key].DefaultValue;
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the workflow attribute value.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="value">The value.</param>
        protected void SetWorkflowAttributeValue( WorkflowAction action, Guid guid, string value )
        {
            var testAttribute = AttributeCache.Read( guid );
            if ( testAttribute != null )
            {
                string testAttributeValue = string.Empty;
                if ( testAttribute.EntityTypeId == new Rock.Model.Workflow().TypeId )
                {
                    action.Activity.Workflow.SetAttributeValue( testAttribute.Key, value );
                }
                else if ( testAttribute.EntityTypeId == new Rock.Model.WorkflowActivity().TypeId )
                {
                    action.Activity.SetAttributeValue( testAttribute.Key, value );
                }
            }
        }

        /// <summary>
        /// Resolves the merge fields.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        protected Dictionary<string, object> GetMergeFields( WorkflowAction action )
        {
            var mergeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( null );
            mergeFields.Add( "Action", action );
            mergeFields.Add( "Activity", action.Activity );
            mergeFields.Add( "Workflow", action.Activity.Workflow );

            if ( HttpContext.Current != null && HttpContext.Current.Items.Contains( "CurrentPerson" ) )
            {
                var currentPerson = HttpContext.Current.Items["CurrentPerson"] as Person;
                if (currentPerson != null)
                {
                    mergeFields.Add( "CurrentPerson", currentPerson );
                }
            }

            return mergeFields;
        }

    }
}