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
        public ActionComponent() : base( false )
        {
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
        /// <param name="action">The action.</param>
        [RockObsolete( "1.7" )]
        [Obsolete("Don't Use this. The ActionTypeCache will already have the attributes loaded automatically", true )]
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
            return GetAttributeValue( action, key, false );
        }

        /// <summary>
        /// Gets a model from the attribute value, which should be the GUID
        /// </summary>
        /// <typeparam name="T">The entity, like FinancialAccount or Group</typeparam>
        /// <param name="action">The action</param>
        /// <param name="key">The key</param>
        /// <param name="checkWorkflowAttributeValue">if set to <c>true</c> and the returned value is a guid, check to see if the workflow 
        /// or activity contains an attribute with that guid. This is useful when using the WorkflowTextOrAttribute field types to get the 
        /// actual value or workflow value.</param>
        /// <param name="rockContext">The context</param>
        /// <returns>The model if it can be resolved or null</returns>
        protected T GetEntityFromAttributeValue<T>( WorkflowAction action, string key, bool checkWorkflowAttributeValue, RockContext rockContext ) where T : Entity<T>, new()
        {
            var modelGuid = GetAttributeValue( action, key, checkWorkflowAttributeValue ).AsGuid();

            if ( modelGuid.IsEmpty() )
            {
                return default( T );
            }

            return new Service<T>( rockContext ).Get( modelGuid );
        }

        /// <summary>
        /// Gets a person from the attribute value, which should be the alias GUID
        /// </summary>
        /// <param name="action">The action</param>
        /// <param name="key">The key</param>
        /// <param name="checkWorkflowAttributeValue">if set to <c>true</c> and the returned value is a guid, check to see if the workflow 
        /// or activity contains an attribute with that guid. This is useful when using the WorkflowTextOrAttribute field types to get the 
        /// actual value or workflow value.</param>
        /// <param name="rockContext">The context</param>
        /// <returns>The person if it can be resolved or null</returns>
        protected Person GetPersonFromAttributeValue( WorkflowAction action, string key, bool checkWorkflowAttributeValue, RockContext rockContext )
        {
            var personAlias = GetEntityFromAttributeValue<PersonAlias>( action, key, checkWorkflowAttributeValue, rockContext );
            return personAlias == null ? ( Person ) null : personAlias.Person;
        }

        /// <summary>
        /// Gets the attribute value.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="key">The key.</param>
        /// <param name="checkWorkflowAttributeValue">if set to <c>true</c> and the returned value is a guid, check to see if the workflow 
        /// or activity contains an attribute with that guid. This is useful when using the WorkflowTextOrAttribute field types to get the 
        /// actual value or workflow value.</param>
        /// <returns></returns>
        protected string GetAttributeValue( WorkflowAction action, string key, bool checkWorkflowAttributeValue )
        {
            string value = GetActionAttributeValue( action, key );
            if ( checkWorkflowAttributeValue )
            {
                Guid? attributeGuid = value.AsGuidOrNull();
                if ( attributeGuid.HasValue )
                {
                    var attribute = AttributeCache.Get( attributeGuid.Value );
                    if ( attribute != null )
                    {
                        value = action.GetWorklowAttributeValue( attributeGuid.Value );
                        if ( !string.IsNullOrWhiteSpace( value ) )
                        {
                            if ( attribute.FieldTypeId == FieldTypeCache.Get( SystemGuid.FieldType.ENCRYPTED_TEXT.AsGuid() ).Id )
                            {
                                value = Security.Encryption.DecryptString( value );
                            }
                            else if ( attribute.FieldTypeId == FieldTypeCache.Get( SystemGuid.FieldType.SSN.AsGuid() ).Id )
                            {
                                value = Rock.Field.Types.SSNFieldType.UnencryptAndClean( value );
                            }
                        }
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// Gets the action attribute value.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetActionAttributeValue( WorkflowAction action, string key )
        {
            var actionType = action.ActionTypeCache;

            if ( actionType != null )
            {
                if ( actionType.Attributes == null )
                {
                    // shouldn't happen since actionType is a ModelCache<,> type 
                    return string.Empty;
                }

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
                    actionType.Attributes.ContainsKey( key ) )
                {
                    return actionType.Attributes[key].DefaultValue;
                }
            }

            return string.Empty;
        }


        /// <summary>
        /// Sets the workflow attribute value.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        protected AttributeCache SetWorkflowAttributeValue( WorkflowAction action, string key, int? value )
        {
            return SetWorkflowAttributeValue( action, key, value.ToString() );
        }

        /// <summary>
        /// Sets the workflow attribute value.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        protected AttributeCache SetWorkflowAttributeValue( WorkflowAction action, string key, decimal? value )
        {
            return SetWorkflowAttributeValue( action, key, value.ToString() );
        }

        /// <summary>
        /// Sets the workflow attribute value.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        protected AttributeCache SetWorkflowAttributeValue( WorkflowAction action, string key, Guid? value )
        {
            return SetWorkflowAttributeValue( action, key, value.ToString() );
        }

        /// <summary>
        /// Sets the workflow attribute value.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        protected AttributeCache SetWorkflowAttributeValue( WorkflowAction action, string key, string value )
        {
            Guid? attrGuid = GetAttributeValue( action, key ).AsGuidOrNull();
            if ( attrGuid.HasValue )
            {
                return SetWorkflowAttributeValue( action, attrGuid.Value, value );
            }
            return null;
        }

        /// <summary>
        /// Sets the workflow attribute value.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="value">The value.</param>
        protected AttributeCache SetWorkflowAttributeValue( WorkflowAction action, Guid guid, string value )
        {
            var attr = AttributeCache.Get( guid );
            if ( attr != null )
            {
                if ( attr.EntityTypeId == new Rock.Model.Workflow().TypeId )
                {
                    action.Activity.Workflow.SetAttributeValue( attr.Key, value );
                }
                else if ( attr.EntityTypeId == new Rock.Model.WorkflowActivity().TypeId )
                {
                    action.Activity.SetAttributeValue( attr.Key, value );
                }
            }

            return attr;
        }

        /// <summary>
        /// Resolves the merge fields.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        protected Dictionary<string, object> GetMergeFields( WorkflowAction action )
        {
            var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null );
            mergeFields.Add( "Action", action );
            mergeFields.Add( "Activity", action.Activity );
            mergeFields.Add( "Workflow", action.Activity.Workflow );

            return mergeFields;
        }

    }
}