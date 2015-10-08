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

                if ( ActionType != null && ActionType.CriteriaAttributeGuid.HasValue )
                {
                    result = false;

                    string criteria = GetWorklowAttributeValue( ActionType.CriteriaAttributeGuid.Value ) ?? string.Empty;
                    string value = ActionType.CriteriaValue;

                    if ( IsValueAnAttribute( value ) )
                    {
                        value = GetWorklowAttributeValue( ActionType.CriteriaValue.AsGuid() );
                    }

                    return criteria.CompareTo( value, ActionType.CriteriaComparisonType );
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

            ActionComponent workflowAction = this.ActionType.WorkflowAction;
            if ( workflowAction == null )
            {
                throw new SystemException( string.Format( "The '{0}' component does not exist, or is not active", workflowAction));
            }

            this.ActionType.LoadAttributes( rockContext );

            if ( IsCriteriaValid )
            {
                bool success = workflowAction.Execute( rockContext, this, entity, out errorMessages );

                this.LastProcessedDateTime = RockDateTime.Now;

                AddLogEntry( string.Format( "Processing Complete (Success:{0})", success.ToString() ) );

                if ( success && this.ActionType != null )
                {
                    if ( this.ActionType.IsActionCompletedOnSuccess )
                    {
                        this.MarkComplete();
                    }

                    if ( this.ActionType.IsActivityCompletedOnSuccess )
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
        /// Gets a worklow attribute value.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="formatted">if set to <c>true</c> [formatted].</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public string GetWorklowAttributeValue( Guid guid, bool formatted = false, bool condensed = false )
        {
            var attribute = AttributeCache.Read( guid );
            if ( attribute != null && Activity != null )
            {
                string value = string.Empty;

                if ( attribute.EntityTypeId == new Rock.Model.Workflow().TypeId && Activity.Workflow != null )
                {
                    value = Activity.Workflow.GetAttributeValue( attribute.Key );
                }
                else if ( attribute.EntityTypeId == new Rock.Model.WorkflowActivity().TypeId )
                {
                    value = Activity.GetAttributeValue( attribute.Key );
                }

                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (formatted)
                    {
                        value = attribute.FieldType.Field.FormatValue( null, value, attribute.QualifierValues, condensed );
                    }

                    return value;
                }
            }

            return null;
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
                var attribute = AttributeCache.Read( guid.Value );

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
            if ( this.Activity != null &&
                this.Activity.Workflow != null &&
                ( force || (
                this.Activity.Workflow.WorkflowType != null &&
                this.Activity.Workflow.WorkflowType.LoggingLevel == WorkflowLoggingLevel.Action ) ) )
            {
                string activityIdStr = this.Activity.Id > 0 ? "(" + this.Activity.Id.ToString() + ")" : "";
                string idStr = Id > 0 ? "(" + Id.ToString() + ")" : "";

                this.Activity.Workflow.AddLogEntry( string.Format( "{0} Activity {1} > {2} Action {3}: {4}",
                    this.Activity.ToString(), activityIdStr, this.ToString(), idStr, logEntry ), force );
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

                if ( ActionType != null && ActionType.WorkflowForm != null )
                {
                    foreach ( var formAttribute in ActionType.WorkflowForm.FormAttributes.OrderBy( a => a.Order ) )
                    {
                        var attribute = AttributeCache.Read( formAttribute.AttributeId );
                        if ( attribute != null && Activity != null )
                        {
                            string value = string.Empty;

                            if ( attribute.EntityTypeId == new Rock.Model.Workflow().TypeId && Activity.Workflow != null )
                            {
                                value = Activity.Workflow.GetAttributeValue( attribute.Key );
                            }
                            else if ( attribute.EntityTypeId == new Rock.Model.WorkflowActivity().TypeId )
                            {
                                value = Activity.GetAttributeValue( attribute.Key );
                            }

                            var field = attribute.FieldType.Field;

                            string formattedValue = field.FormatValue( null, value, attribute.QualifierValues, false );

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
            return this.ActionType.ToStringSafe();
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Activates the specified <see cref="Rock.Model.WorkflowAction" />.
        /// </summary>
        /// <param name="actionType">The <see cref="Rock.Model.WorkflowActionType" /> to be activated.</param>
        /// <param name="activity">The <see cref="Rock.Model.WorkflowActivity" /> that this WorkflowAction belongs to..</param>
        /// <returns>
        /// The <see cref="Rock.Model.WorkflowAction" />
        /// </returns>        
        internal static WorkflowAction Activate( WorkflowActionType actionType, WorkflowActivity activity )
        {
            using ( var rockContext = new RockContext() )
            {
                return Activate( actionType, activity, rockContext );
            }
        }

        /// <summary>
        /// Activates the specified <see cref="Rock.Model.WorkflowAction" />.
        /// </summary>
        /// <param name="actionType">The <see cref="Rock.Model.WorkflowActionType" /> to be activated.</param>
        /// <param name="activity">The <see cref="Rock.Model.WorkflowActivity" /> that this WorkflowAction belongs to..</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        /// The <see cref="Rock.Model.WorkflowAction" />
        /// </returns>
        internal static WorkflowAction Activate( WorkflowActionType actionType, WorkflowActivity activity, RockContext rockContext )
        {
            var action = new WorkflowAction();
            action.Activity = activity;
            action.ActionType = actionType;
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

