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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Web.Cache;
using Rock.Workflow;

namespace Rock.Model
{
    /// <summary>
    /// Represents a persisted WorkflowAction in Rock.
    /// </summary>
    [RockDomain( "Workflow" )]
    [Table( "WorkflowAction" )]
    [DataContract]
    public partial class WorkflowAction : Model<WorkflowAction>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the WorkflowActivityId of the <see cref="Rock.Model.WorkflowActivity"/> that this WorkflowAction is a part of.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> represents the WorflowActivityId that this WorkflowAction is a part of.
        /// </value>
        [DataMember]
        public int ActivityId { get; set; }

        /// <summary>
        /// Gets or sets the ActionTypeId of the <see cref="Rock.Model.WorkflowAction"/> that is being executed by this instance.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the ActionTypeId of the <see cref="Rock.Model.WorkflowActionType"/> that is being executed on this instance.
        /// </value>
        [DataMember]
        public int ActionTypeId { get; set; }

        /// <summary>
        /// Gets or sets the date and time that this WorkflowAction was last processed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that this WorkflowAction was last processed.
        /// </value>
        [DataMember]
        [NotAudited]
        public DateTime? LastProcessedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the WorkflowAction completed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that the WorkflowAction completed.
        /// </value>
        [DataMember]
        public DateTime? CompletedDateTime { get; set; }

        /// <summary>
        /// If ActionType is a UserEntryForm Gets or sets the form action.
        /// </summary>
        /// <value>
        /// The form action.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string FormAction { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WorkflowActivity"/> that contains the WorkflowAction.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.WorkflowActivity"/> that contains this WorkflowAction.
        /// </value>
        [LavaInclude]
        public virtual WorkflowActivity Activity { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WorkflowActionType"/> that is being executed by this WorkflowAction.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.WorkflowActionType"/> that is being executed.
        /// </value>
        [LavaInclude]
        public virtual WorkflowActionType ActionType { get; set; }

        /// <summary>
        /// Gets the action type cache.
        /// </summary>
        /// <value>
        /// The action type cache.
        /// </value>
        [LavaInclude]
        public virtual WorkflowActionTypeCache ActionTypeCache
        {
            get
            {
                if ( ActionTypeId > 0 )
                {
                    return WorkflowActionTypeCache.Get( ActionTypeId );
                }
                else if ( ActionType != null )
                {
                    return WorkflowActionTypeCache.Get( ActionType.Id );
                }
                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this WorkflowAction is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this WorkflowAction is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual bool IsActive
        {
            get
            {
                return !CompletedDateTime.HasValue;
            }
            private set { }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is criteria valid.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is criteria valid; otherwise, <c>false</c>.
        /// </value>
        [NotMapped]
        public virtual bool IsCriteriaValid
        {
            get
            {
                bool result = true;
                var actionType = this.ActionTypeCache;
                if ( actionType != null && actionType.CriteriaAttributeGuid.HasValue )
                {
                    result = false;

                    string criteria = GetWorkflowAttributeValue( actionType.CriteriaAttributeGuid.Value ) ?? string.Empty;
                    string value = actionType.CriteriaValue;

                    if ( IsValueAnAttribute( value ) )
                    {
                        value = GetWorkflowAttributeValue( actionType.CriteriaValue.AsGuid() );
                    }

                    return criteria.CompareTo( value, actionType.CriteriaComparisonType );
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the parent security authority for this WorkflowAction.
        /// </summary>
        /// <value>
        /// The parent security authority for this WorkflowAction.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.Activity != null ? this.Activity : base.ParentAuthority;
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public override object this[object key]
        {
            get
            {
                string propertyKey = key.ToStringSafe();
                if ( propertyKey == "ActionType" )
                {
                    return ActionTypeCache;
                }
                return base[key];
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Processes this WorkflowAction.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="entity">The entity that the WorkflowAction is operating against.</param>
        /// <param name="errorMessages">A <see cref="System.Collections.Generic.List{String}" /> that will contain any error messages that occur while processing the WorkflowAction.</param>
        /// <returns>
        /// A <see cref="System.Boolean" /> value that is <c>true</c> if the process completed successfully; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="System.SystemException"></exception>
        internal virtual bool Process( RockContext rockContext, Object entity, out List<string> errorMessages )
        {
            AddLogEntry( "Processing..." );

            var actionType = this.ActionTypeCache;
            if ( actionType == null )
            {
                throw new SystemException( string.Format( "ActionTypeId: {0} could not be loaded.", this.ActionTypeId ) );
            }

            ActionComponent workflowAction = actionType.WorkflowAction;
            if ( workflowAction == null )
            {
                throw new SystemException( string.Format( "The '{0}' component does not exist, or is not active", actionType.EntityType ) );
            }

            if ( IsCriteriaValid )
            {
                bool success = workflowAction.Execute( rockContext, this, entity, out errorMessages );

                this.LastProcessedDateTime = RockDateTime.Now;

                if ( errorMessages.Any() )
                {
                    foreach ( string errorMsg in errorMessages )
                    {
                        AddLogEntry( "Error Occurred: " + errorMsg, true );
                    }
                }

                AddLogEntry( string.Format( "Processing Complete (Success:{0})", success.ToString() ) );

                if ( success )
                {
                    if ( actionType.IsActionCompletedOnSuccess )
                    {
                        this.MarkComplete();
                    }

                    if ( actionType.IsActivityCompletedOnSuccess )
                    {
                        this.Activity.MarkComplete();
                    }
                }

                return success;
            }
            else
            {
                errorMessages = new List<string>();

                AddLogEntry( "Criteria test failed. Action was not processed. Processing continued." );

                return true;
            }
        }

        /// <summary>
        /// Gets a worklfow attribute value.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="formatted">if set to <c>true</c> [formatted].</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public string GetWorkflowAttributeValue( Guid guid, bool formatted = false, bool condensed = false )
        {
            var attribute = AttributeCache.Get( guid );
            if ( attribute != null && Activity != null )
            {
                string value = string.Empty;
                int? entityId = null;

                if ( attribute.EntityTypeId == new Rock.Model.Workflow().TypeId && Activity.Workflow != null )
                {
                    value = Activity.Workflow.GetAttributeValue( attribute.Key );
                    entityId = Activity.Workflow.Id;
                }
                else if ( attribute.EntityTypeId == new Rock.Model.WorkflowActivity().TypeId )
                {
                    value = Activity.GetAttributeValue( attribute.Key );
                    entityId = Activity.Id;
                }

                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (formatted)
                    {
                        value = attribute.FieldType.Field.FormatValue( null, attribute.EntityTypeId, entityId, value, attribute.QualifierValues, condensed );
                    }
                }

                return value ?? string.Empty;
            }

            return null;
        }

        /// <summary>
        /// Gets a entity from the attribute value -- but it only works for attributes who's field type is an IEntityFieldType.
        /// </summary>
        /// <param name="value">The action attribute value</param>
        /// <param name="rockContext">The context</param>
        /// <returns>The model if it can be resolved or null</returns>
        public IEntity GetEntityFromAttributeValue( string value, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();

            var attributeGuid = value.AsGuidOrNull();
            if ( attributeGuid.HasValue )
            {
                var attribute = AttributeCache.Get( attributeGuid.Value );
                if ( attribute != null )
                {
                    value = GetWorkflowAttributeValue( attributeGuid.Value );
                    if ( !string.IsNullOrWhiteSpace( value ) )
                    {
                        var field = attribute.FieldType.Field;
                        if ( field is Rock.Field.IEntityFieldType )
                        {
                            return ( ( Rock.Field.IEntityFieldType ) field ).GetEntity( value, rockContext );
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a worklow attribute value.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="formatted">if set to <c>true</c> [formatted].</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        [RockObsolete( "1.11" )]
        [Obsolete( "Use GetWorkflowAttributeValue instead (the one with the correct spelling)." )]
        public string GetWorklowAttributeValue( Guid guid, bool formatted = false, bool condensed = false )
        {
            return GetWorkflowAttributeValue( guid, formatted, condensed );
        }

        /// <summary>
        /// Determines whether [is unique identifier value an attribute] [the specified unique identifier value].
        /// </summary>
        /// <param name="guidValue">The unique identifier value.</param>
        /// <returns></returns>
        public virtual bool IsValueAnAttribute( string guidValue )
        {
            Guid? guid = guidValue.AsGuidOrNull();
            if ( guid.HasValue )
            {
                // Check to see if attribute exists with selected guid
                var attribute = AttributeCache.Get( guid.Value );

                // If so, check to see if the current workflow or activity contains that attribute
                if ( attribute != null && Activity != null )
                {
                    // Check for workflow attribute
                    if ( Activity.Workflow != null )
                    {
                        if ( Activity.Workflow.Attributes == null )
                        {
                            Activity.Workflow.LoadAttributes();
                        }
                        if ( Activity.Workflow.Attributes.ContainsKey( attribute.Key ) )
                        {
                            return true;
                        }
                    }

                    // Check for activity attribute
                    if ( Activity.Attributes == null )
                    {
                        Activity.LoadAttributes();
                    }
                    if ( Activity.Attributes.ContainsKey( attribute.Key ) )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Adds a <see cref="Rock.Model.WorkflowLog" /> entry.
        /// </summary>
        /// <param name="logEntry">A <see cref="System.String" /> representing the  log entry.</param>
        /// <param name="force">if set to <c>true</c> will ignore logging level and always add the entry.</param>
        public virtual void AddLogEntry( string logEntry, bool force = false )
        {
            if ( this.Activity != null && this.Activity.Workflow != null )
            {
                var workflowType = this.Activity.Workflow.WorkflowTypeCache;
                if ( force || ( workflowType != null && workflowType.LoggingLevel == WorkflowLoggingLevel.Action ) )
                {
                    string activityIdStr = this.Activity.Id > 0 ? "(" + this.Activity.Id.ToString() + ")" : "";
                    string idStr = Id > 0 ? "(" + Id.ToString() + ")" : "";

                    this.Activity.Workflow.AddLogEntry( string.Format( "{0} Activity {1} > {2} Action {3}: {4}",
                        this.Activity.ToString(), activityIdStr, this.ToString(), idStr, logEntry ), force );
                }
            }
        }

        /// <summary>
        /// Marks this WorkflowAction as complete.
        /// </summary>
        public virtual void MarkComplete()
        {
            CompletedDateTime = RockDateTime.Now;
            AddLogEntry( "Completed" );
        }

        /// <summary>
        /// Gets the form attributes.
        /// </summary>
        /// <value>
        /// The form attributes.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual List<LiquidFormAttribute> FormAttributes
        {
            get
            {
                var attributeList = new List<LiquidFormAttribute>();
                var actionType = this.ActionTypeCache;
                if ( actionType != null && actionType.WorkflowForm != null )
                {
                    foreach ( var formAttribute in actionType.WorkflowForm.FormAttributes.OrderBy( a => a.Order ) )
                    {
                        var attribute = AttributeCache.Get( formAttribute.AttributeId );
                        if ( attribute != null && Activity != null )
                        {
                            string value = string.Empty;
                            int? entityId = null;

                            if ( attribute.EntityTypeId == new Rock.Model.Workflow().TypeId && Activity.Workflow != null )
                            {
                                value = Activity.Workflow.GetAttributeValue( attribute.Key );
                                entityId = Activity.Workflow.Id;
                            }
                            else if ( attribute.EntityTypeId == new Rock.Model.WorkflowActivity().TypeId )
                            {
                                value = Activity.GetAttributeValue( attribute.Key );
                                entityId = Activity.Id;
                            }

                            var field = attribute.FieldType.Field;

                            string formattedValue = field.FormatValue( null, attribute.EntityTypeId, entityId, value, attribute.QualifierValues, false );

                            var liquidFormAttribute = new LiquidFormAttribute();
                            liquidFormAttribute.Name = attribute.Name;
                            liquidFormAttribute.Key = attribute.Key;
                            liquidFormAttribute.Value = formattedValue;
                            liquidFormAttribute.IsVisible = formAttribute.IsVisible;
                            liquidFormAttribute.IsReadOnly = formAttribute.IsReadOnly;
                            liquidFormAttribute.IsRequired = formAttribute.IsRequired;
                            liquidFormAttribute.HideLabel = formAttribute.HideLabel;
                            liquidFormAttribute.PreHtml = formAttribute.PreHtml;
                            liquidFormAttribute.PostHtml = formAttribute.PostHtml;
                            if ( field is Rock.Field.ILinkableFieldType )
                            {
                                liquidFormAttribute.Url = "~/" + ( (Rock.Field.ILinkableFieldType)field ).UrlLink( value, attribute.QualifierValues );
                            }

                            attributeList.Add( liquidFormAttribute );
                        }
                    }
                }

                return attributeList;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this WorkflowAction.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this WorkflowAction..
        /// </returns>
        public override string ToString()
        {
            var actionType = this.ActionTypeCache;
            if ( actionType != null )
            {
                return actionType.ToStringSafe();
            }
            return base.ToString();
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Activates the specified <see cref="Rock.Model.WorkflowAction" />.
        /// </summary>
        /// <param name="actionTypeCache">The action type cache.</param>
        /// <param name="activity">The <see cref="Rock.Model.WorkflowActivity" /> that this WorkflowAction belongs to..</param>
        /// <returns>
        /// The <see cref="Rock.Model.WorkflowAction" />
        /// </returns>
        internal static WorkflowAction Activate( WorkflowActionTypeCache actionTypeCache, WorkflowActivity activity )
        {
            using ( var rockContext = new RockContext() )
            {
                return Activate( actionTypeCache, activity, rockContext );
            }
        }

        /// <summary>
        /// Activates the specified <see cref="Rock.Model.WorkflowAction" />.
        /// </summary>
        /// <param name="actionTypeCache">The action type cache.</param>
        /// <param name="activity">The <see cref="Rock.Model.WorkflowActivity" /> that this WorkflowAction belongs to..</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        /// The <see cref="Rock.Model.WorkflowAction" />
        /// </returns>
        internal static WorkflowAction Activate( WorkflowActionTypeCache actionTypeCache, WorkflowActivity activity, RockContext rockContext )
        {
            var action = new WorkflowAction();
            action.Activity = activity;
            action.ActionTypeId = actionTypeCache.Id;
            action.LoadAttributes( rockContext );

            action.AddLogEntry( "Activated" );

            return action;
        }

        #endregion

        /// <summary>
        /// Special class for adding form attributes to liquid
        /// </summary>
        [DotLiquid.LiquidType( "Name", "Key", "Value", "IsVisible", "IsReadOnly", "IsRequired", "HideLabel", "PreHtml", "PostHtml", "Url" )]
        public class LiquidFormAttribute
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the key.
            /// </summary>
            /// <value>
            /// The key.
            /// </value>
            public string Key { get; set; }

            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public string Value { get; set; }

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
            /// Gets or sets a value indicating whether this instance is required.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is required; otherwise, <c>false</c>.
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
            /// Gets or sets the URL.
            /// </summary>
            /// <value>
            /// The URL.
            /// </value>
            public string Url { get; set; }
        }
    }

    #region Entity Configuration

    /// <summary>
    /// Action Configuration class.
    /// </summary>
    public partial class WorkflowActionConfiguration : EntityTypeConfiguration<WorkflowAction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowActionConfiguration"/> class.
        /// </summary>
        public WorkflowActionConfiguration()
        {
            this.HasRequired( m => m.Activity ).WithMany( m => m.Actions ).HasForeignKey( m => m.ActivityId ).WillCascadeOnDelete( true );
            this.HasRequired( m => m.ActionType ).WithMany().HasForeignKey( m => m.ActionTypeId).WillCascadeOnDelete( false );
        }
    }

    #endregion

}

