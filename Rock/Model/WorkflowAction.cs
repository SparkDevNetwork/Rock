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
        [MaxLength( 20 )]
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
        public virtual WorkflowActivity Activity { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WorkflowActionType"/> that is being executed by this WorkflowAction.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.WorkflowActionType"/> that is being executed.
        /// </value>
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
        /// Gets the parent security authority for this WorkflowAction.
        /// </summary>
        /// <value>
        /// The parent security authority for this WorkflowAction.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.Activity;
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
            AddSystemLogEntry( "Processing..." );

            ActionComponent workflowAction = this.ActionType.WorkflowAction;
            if ( workflowAction == null )
            {
                throw new SystemException( string.Format( "The '{0}' component does not exist, or is not active", workflowAction));
            }

            this.ActionType.LoadAttributes();

            if ( TestCriteria() )
            {
                bool success = workflowAction.Execute( rockContext, this, entity, out errorMessages );

                this.LastProcessedDateTime = RockDateTime.Now;

                AddSystemLogEntry( string.Format( "Processing Complete (Success:{0})", success.ToString() ) );

                if ( success )
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

                AddSystemLogEntry( "Criteria test failed. Action was not processed. Processing continued." );

                return true;
            }
        }

        /// <summary>
        /// Tests the criteria.
        /// </summary>
        /// <returns></returns>
        private bool TestCriteria()
        {
            bool result = true;

            if ( ActionType != null &&
                ActionType.CriteriaAttributeGuid.HasValue )
            {
                result = false;

                string criteria = GetWorklowAttributeValue( ActionType.CriteriaAttributeGuid.Value ) ?? string.Empty;

                Guid guid = ActionType.CriteriaValue.AsGuid();
                if ( guid.IsEmpty() )
                {
                    return criteria.CompareTo( ActionType.CriteriaValue, ActionType.CriteriaComparisonType );
                }
                else
                {
                    string value = GetWorklowAttributeValue( guid );
                    return criteria.CompareTo( value, ActionType.CriteriaComparisonType );
                }
            }

            return result;
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
        /// Adds a <see cref="Rock.Model.WorkflowLog"/> entry.
        /// </summary>
        /// <param name="logEntry">A <see cref="System.String"/> representing the  log entry.</param>
        public virtual void AddLogEntry( string logEntry )
        {
            if ( this.Activity != null &&
                this.Activity.Workflow != null )
            {
                string activityIdStr = this.Activity.Id > 0 ? "(" + this.Activity.Id.ToString() + ")" : "";
                string idStr = Id > 0 ? "(" + Id.ToString() + ")" : "";

                this.Activity.Workflow.AddLogEntry( string.Format( "{0} Activity {1} > {2} Action {3}: {4}",
                    this.Activity.ToString(), activityIdStr, this.ToString(), idStr, logEntry ) );
            }
        }

        /// <summary>
        /// Marks this WorkflowAction as complete.
        /// </summary>
        public virtual void MarkComplete()
        {
            CompletedDateTime = RockDateTime.Now;
            AddSystemLogEntry( "Completed" );
        }

        /// <summary>
        /// Creates a DotLiquid compatible dictionary that represents the current entity object.
        /// </summary>
        /// <param name="debug">if set to <c>true</c> the entire object tree will be parsed immediately.</param>
        /// <returns>
        /// DotLiquid compatible dictionary.
        /// </returns>
        public override object ToLiquid( bool debug )
        {
            var mergeFields = base.ToLiquid( debug ) as Dictionary<string, object>;

            if ( debug )
            {
                mergeFields.Add( "Activity", Activity.ToLiquid( true ) );
                mergeFields.Add( "ActionType", this.ActionType.ToLiquid( true ) );
            }
            else
            {
                mergeFields.Add( "Activity", Activity );
                mergeFields.Add( "ActionType", this.ActionType );
            }

            mergeFields.Add( "FormAttributes", GetFormAttributesLiquid() );

            return mergeFields;
        }


        private List<Dictionary<string, object>> GetFormAttributesLiquid()
        {
            var attributeList = new List<Dictionary<string, object>>();

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

                        if ( !string.IsNullOrWhiteSpace( value ) )
                        {
                            var field = attribute.FieldType.Field;

                            string formattedValue = field.FormatValue( null, value, attribute.QualifierValues, false );
                            var attributeLiquid = new Dictionary<string, object>();
                            attributeLiquid.Add( "Name", attribute.Name );
                            attributeLiquid.Add( "Key", attribute.Key );
                            attributeLiquid.Add( "Value", formattedValue );
                            attributeLiquid.Add( "IsRequired", formAttribute.IsRequired );
                            if ( field is Rock.Field.ILinkableFieldType )
                            {
                                attributeLiquid.Add( "Url", "~/" + ( (Rock.Field.ILinkableFieldType)field ).UrlLink( value, attribute.QualifierValues ) );
                            }

                            attributeList.Add( attributeLiquid );
                        }
                    }
                }
            }

            return attributeList;

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

        #region Private Methods

        /// <summary>
        /// Logs a system event.
        /// </summary>
        /// <param name="logEntry">A <see cref="System.String"/>representing the log entry.</param>
        private void AddSystemLogEntry( string logEntry )
        {
            if ( this.Activity != null &&
                this.Activity.Workflow != null &&
                this.Activity.Workflow.WorkflowType != null &&
                this.Activity.Workflow.WorkflowType.LoggingLevel == WorkflowLoggingLevel.Action )
            {
                AddLogEntry( logEntry );
            }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Activates the specified <see cref="Rock.Model.WorkflowAction"/>.
        /// </summary>
        /// <param name="actionType">The <see cref="Rock.Model.WorkflowActionType"/> to be activated.</param>
        /// <param name="activity">The <see cref="Rock.Model.WorkflowActivity"/> that this WorkflowAction belongs to..</param>
        /// <returns>The <see cref="Rock.Model.WorkflowAction"/></returns>
        internal static WorkflowAction Activate( WorkflowActionType actionType, WorkflowActivity activity)
        {
            var action = new WorkflowAction();
            action.Activity = activity;
            action.ActionType = actionType;
            action.LoadAttributes();

            action.AddSystemLogEntry( "Activated" );

            return action;
        }

        #endregion

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

