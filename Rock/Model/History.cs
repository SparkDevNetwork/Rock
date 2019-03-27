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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using System.Text;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a history that is entered in Rock and is associated with a specific entity. For example, a history could be entered on a person, GroupMember, a device, etc or for a specific subset of an entity type.
    /// </summary>
    [RockDomain( "Core" )]
    [NotAudited]
    [Table( "History" )]
    [DataContract]
    public partial class History : Model<History>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this history is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this history is part of the Rock core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Category"/>. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Category"/>
        /// </value>
        [Required]
        [DataMember]
        public int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.EntityType"/>. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.EntityType"/>
        /// </value>
        [Required]
        [DataMember]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the entity that this history is related to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the entity (object) that this history is related to.
        /// </value>
        [Required]
        [DataMember]
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the verb which is a structured (for querying) field to describe what the action is (ADD, DELETE, UPDATE, VIEW, WATCHED,  etc).
        /// <see cref="History.HistoryVerb"/> constants for common verbs
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the verb of the History.
        /// </value>
        [MaxLength( 20 )]
        [DataMember]
        public string Verb { get; set; }

        /// <summary>
        /// Gets or sets the caption
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the caption of the History.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string Caption { get; set; }

        /// <summary>
        /// Gets or sets the summary.
        /// </summary>
        /// <value>
        /// The summary.
        /// </value>
        [DataMember]
        [RockObsolete( "1.8" )]
        [Obsolete( "Use SummaryHtml instead to get the Summary, or use HistoryChangeList related functions to log history " )]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the related entity type identifier.
        /// </summary>
        /// <value>
        /// The related entity type identifier.
        /// </value>
        /// 
        [DataMember]
        public int? RelatedEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the related entity identifier.
        /// </summary>
        /// <value>
        /// The related entity identifier.
        /// </value>
        [DataMember]
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// Gets or sets the related data.
        /// </summary>
        /// <value>
        /// The related data.
        /// </value>
        [DataMember]
        public string RelatedData { get; set; }

        /// <summary>
        /// Gets or sets the ChangeType which is a structured (for querying) field to describe what type of data was changed (Record, Property, Attribute, Location, Schedule, etc)
        /// <see cref="History.HistoryChangeType"/> constants for common change types
        /// </summary>
        /// <value>
        /// The type of the change.
        /// </value>
        [MaxLength( 20 )]
        [DataMember]
        public string ChangeType { get; set; }

        /// <summary>
        /// Gets or sets the name of the value depending on ChangeType: ChangeTypeName.Property => Property Friendly Name, ChangeType.Attribute => Attribute Name, ChangeType.Record => the ToString of the record
        /// </summary>
        /// <value>
        /// The name of the value.
        /// </value>
        [MaxLength( 250 )]
        [DataMember]
        public string ValueName { get; set; }

        /// <summary>
        /// Gets or sets the new value.
        /// </summary>
        /// <value>
        /// The new value.
        /// </value>
        [DataMember]
        public string NewValue { get; set; }

        /// <summary>
        /// Creates new rawvalue.
        /// </summary>
        /// <value>
        /// The new raw value.
        /// </value>
        [MaxLength( 250 )]
        [DataMember]
        public string NewRawValue { get; set; }

        /// <summary>
        /// Gets or sets the old value.
        /// </summary>
        /// <value>
        /// The old value.
        /// </value>
        [DataMember]
        public string OldValue { get; set; }

        /// <summary>
        /// Gets or sets the old raw value.
        /// </summary>
        /// <value>
        /// The old raw value.
        /// </value>
        [MaxLength( 250 )]
        [DataMember]
        public string OldRawValue { get; set; }


        /// <summary>
        /// Gets or sets whether the NewValue and/or OldValue is null because the value is sensitive data that shouldn't be logged
        /// If "IsSensitive" doesn't apply to this, it can be left null
        /// </summary>
        /// <value>
        /// IsSensitive.
        /// </value>
        [DataMember]
        public bool? IsSensitive { get; set; }

        /// <summary>
        /// Optional: Gets or sets name of the tool or process that changed the value
        /// </summary>
        /// <value>
        /// The source of change.
        /// </value>
        [DataMember]
        public string SourceOfChange { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the entity type this history is associated with
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> of this history.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets the type of the related entity.
        /// </summary>
        /// <value>
        /// The type of the related entity.
        /// </value>
        [DataMember]
        public virtual EntityType RelatedEntityType { get; set; }

        /// <summary>
        /// Gets the parent security authority of this History. Where security is inherited from.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.Category != null ? this.Category : base.ParentAuthority;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.SummaryHtml;
        }

        /// <summary>
        /// Calculates and returns a formatted summary
        /// </summary>
        /// <returns></returns>
        [NotMapped]
        [LavaInclude]
        public string SummaryHtml
        {
            get
            {
                HistoryVerb? historyVerb = this.Verb.ConvertToEnumOrNull<HistoryVerb>();
                if ( !string.IsNullOrEmpty( this.ChangeType ) )
                {
                    if ( historyVerb.HasValue )
                    {
                        switch ( historyVerb.Value )
                        {
                            case HistoryVerb.Add:
                                {
                                    if ( this.IsSensitive == true )
                                    {
                                        return $"Added <span class='field-name'>{this.ValueName}</span> value (Sensitive attribute values are not logged in history).";
                                    }
                                    else
                                    {
                                        StringBuilder addSummary = new StringBuilder( $"Added <span class='field-name'>{this.ValueName}</span>" );
                                        if ( !string.IsNullOrEmpty( this.NewValue ) )
                                        {
                                            addSummary.Append( $" value of <span class='field-value'>{this.NewValue}</span>" );
                                        }

                                        addSummary.Append( "." );
                                        return addSummary.ToString();
                                    }
                                }

                            case HistoryVerb.Modify:
                                {
                                    if ( this.IsSensitive == true )
                                    {
                                        return $"Modified <span class='field-name'>{this.ValueName}</span> value (Sensitive attribute values are not logged in history).";
                                    }
                                    else
                                    {
                                        string summaryVerb;
                                        string summaryValue;

                                        if ( this.OldValue == null && this.NewValue != null )
                                        {
                                            // pre-V8 when a property was modified from null, it would get summarized as 'Added ..."
                                            summaryVerb = "Added";
                                            summaryValue = $"value of <span class='field-value'>{this.NewValue}</span>";
                                        }
                                        else if ( this.OldValue != null && this.NewValue == null )
                                        {
                                            // pre-V8 when a property was modified to null, it would get summarized as 'Deleted ..."
                                            summaryVerb = "Deleted";
                                            summaryValue = $"value of <span class='field-value'>{this.OldValue}</span>";
                                        }
                                        else
                                        {
                                            summaryVerb = "Modified";
                                            summaryValue = $"value from <span class='field-value'>{this.OldValue}</span> to <span class='field-value'>{this.NewValue}</span>";
                                        }

                                        string modifySummary = $"{summaryVerb} <span class='field-name'>{this.ValueName}</span> {summaryValue}";

                                        return modifySummary;
                                    }
                                }

                            case HistoryVerb.Delete:
                                {
                                    if ( this.IsSensitive == true )
                                    {
                                        return $"Deleted <span class='field-name'>{this.ValueName}</span> value (Sensitive attribute values are not logged in history).";
                                    }
                                    else
                                    {
                                        StringBuilder deleteSummary = new StringBuilder( $"Deleted <span class='field-name'>{this.ValueName}</span>" );

                                        if ( !string.IsNullOrEmpty( this.OldValue ) )
                                        {
                                            deleteSummary.Append( $" value of <span class='field-value'>{this.OldValue}</span>" );
                                        }

                                        deleteSummary.Append( "." );

                                        return deleteSummary.ToString();
                                    }
                                }

                            case HistoryVerb.Process:
                                {
                                    return $"Processed refund for {this.ValueName}";
                                }

                            case HistoryVerb.Matched:
                            case HistoryVerb.Unmatched:
                                {
                                    return $"{historyVerb.Value.ConvertToString( false )} {this.ValueName}";
                                }

                            case HistoryVerb.Registered:
                                {
                                    return $"Registered {this.ValueName} for";
                                }

                            case HistoryVerb.Login:
                                {
                                    StringBuilder loginSummaryBuilder = new StringBuilder();
                                    loginSummaryBuilder.Append( $"User logged in with <span class='field-name'>{this.ValueName}</span> username" );

                                    // if Related Data has data, it could be additional info about the HostAddress the person logged in from and the url of the page when they logged in
                                    if ( !string.IsNullOrEmpty( this.RelatedData ) )
                                    {
                                        loginSummaryBuilder.Append( $", {this.RelatedData}" );
                                    }

                                    var loginSummary = loginSummaryBuilder.ToString();
                                    return loginSummary;
                                }

                            case HistoryVerb.Merge:
                                {
                                    return $"Merged <span class='field-value'>{this.ValueName}</span> with this record.";
                                }

                            case HistoryVerb.AddedToGroup:
                                {
                                    return $"Added to {this.ValueName}.";
                                }

                            case HistoryVerb.RemovedFromGroup:
                                {
                                    return $"Removed from {this.ValueName}.";
                                }

                            case HistoryVerb.Sent:
                                {
                                    var sentSummaryBuilder = new StringBuilder( $"Sent {this.ValueName}" );

                                    // if RelatedData is not NULL is is most likely message.FromName. NOTE: if it is string.Empty, still append the field-value span so that it renders like pre-v8
                                    if ( this.RelatedData != null )
                                    {
                                        sentSummaryBuilder.Append( $" <span class='field-value'>{this.RelatedData}</span>." );
                                    }

                                    var sentSummary = sentSummaryBuilder.ToString();
                                    return sentSummary;
                                }
                        }
                    }

                    // some unexpected verb was used to make a custom summary 
                    var stringBuilder = new StringBuilder();

                    // Start with whatever custom verb was used. For example 'WATCHED' => 'Watched'
                    stringBuilder.Append( this.Verb.FixCase() );

                    // include the value name (For example 'First Name') that was affected
                    if ( !string.IsNullOrEmpty( this.ValueName ) )
                    {
                        stringBuilder.Append( $" <span class='field-name'>{this.ValueName}</span>." );
                    }

                    if ( this.IsSensitive != true )
                    {
                        if ( !string.IsNullOrEmpty( this.OldValue ) )
                        {
                            stringBuilder.Append( $" old value of <span class='field-name'>{this.OldValue}</span>, " );
                        }

                        if ( !string.IsNullOrEmpty( this.NewValue ) )
                        {
                            stringBuilder.Append( $" new value of <span class='field-name'>{this.NewValue}</span>, " );
                        }
                    }

                    var customSummary = stringBuilder.ToString();
                    if ( !string.IsNullOrEmpty( customSummary ) )
                    {
                        return customSummary;
                    }
                }

                // fallback to Summary if summary couldn't be built from Verb, ChangeType, etc
#pragma warning disable 612, 618
                return this.Summary;
#pragma warning restore 612, 618
            }
        }

        #endregion

        #region Constants

        /// <summary>
        /// Common Verbs are used in the HistoryChange helpers. These get turned in the UpperCase strings when saving to History.Verb
        /// Custom Verbs can still be used by History.Verb manually
        /// </summary>
        public enum HistoryVerb
        {
            /// <summary>
            /// New record (or child record) was added
            /// </summary>
            Add,

            /// <summary>
            /// field (or attribute) values were modified
            /// </summary>
            Modify,

            /// <summary>
            /// The record (or child record) was deleted
            /// </summary>
            Delete,

            /// <summary>
            /// Something was Registered. For example a Person was registered for an event.
            /// </summary>
            Registered,

            /// <summary>
            /// Something was Processed. For example, a transaction payment or refund for a person or event was processed
            /// </summary>
            Process,

            /// <summary>
            /// Something was matched. For example, a transaction was matched to a person
            /// </summary>
            Matched,

            /// <summary>
            /// Something was un-matched. For example, a transaction was un-matched from a person
            /// </summary>
            Unmatched,

            /// <summary>
            /// Something was Sent. For example, a communication was sent from a person.
            /// </summary>
            Sent,

            /// <summary>
            /// The record (probably person) was logged in
            /// </summary>
            Login,

            /// <summary>
            /// The record (probably person) was merged
            /// </summary>
            Merge,

            /// <summary>
            /// a person/groupmember was added to a group
            /// </summary>
            AddedToGroup,

            /// <summary>
            /// a person/groupmember was removed (or archived) from a group
            /// </summary>
            RemovedFromGroup,
        }

        /// <summary>
        /// Common Change Types. This get saved to History.ChangeType as a string so that custom change types can be used
        /// </summary>
        public enum HistoryChangeType
        {
            /// <summary>
            /// The Change affected an entire record (for example, it was DELETED or ADDED), or is a child record of the item we are logging history for
            /// </summary>
            Record,

            /// <summary>
            /// The Change affected a property on the record
            /// </summary>
            Property,

            /// <summary>
            /// The Change affected an attribute value on the record
            /// </summary>
            Attribute
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyChangeList">The history change list.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="isSensitive">if set to <c>true</c> [is sensitive].</param>
        /// <param name="oldRawValue">The old raw value.</param>
        /// <param name="newRawValue">The new raw value.</param>
        private static void EvaluateChange( HistoryChangeList historyChangeList, string propertyName, string oldValue, string newValue, bool isSensitive = false, string oldRawValue = null, string newRawValue = null )
        {
            if ( !string.IsNullOrWhiteSpace( oldValue ) )
            {
                if ( !string.IsNullOrWhiteSpace( newValue ) )
                {
                    if ( oldValue.Trim() != newValue.Trim() )
                    {
                        if ( isSensitive )
                        {
                            historyChangeList.AddChange( HistoryVerb.Modify, HistoryChangeType.Property, propertyName ).SetSensitive();
                        }
                        else
                        {

                            historyChangeList.AddChange( HistoryVerb.Modify, HistoryChangeType.Property, propertyName ).SetNewValue( newValue ).SetOldValue( oldValue ).SetRawValues( oldRawValue, newRawValue );
                        }
                    }
                }
                else
                {
                    if ( isSensitive )
                    {
                        historyChangeList.AddChange( HistoryVerb.Modify, HistoryChangeType.Property, propertyName ).SetSensitive();
                    }
                    else
                    {
                        historyChangeList.AddChange( HistoryVerb.Modify, HistoryChangeType.Property, propertyName ).SetOldValue( oldValue );
                    }
                }
            }
            else if ( !string.IsNullOrWhiteSpace( newValue ) )
            {
                if ( isSensitive )
                {
                    historyChangeList.AddChange( HistoryVerb.Modify, HistoryChangeType.Property, propertyName ).SetSensitive();
                }
                else
                {
                    historyChangeList.AddChange( HistoryVerb.Modify, HistoryChangeType.Property, propertyName ).SetNewValue( newValue );
                }
            }
        }


        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyChangeList">The history change list.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="isSensitive">if set to <c>true</c> [is sensitive].</param>
        public static void EvaluateChange( HistoryChangeList historyChangeList, string propertyName, string oldValue, string newValue, bool isSensitive = false )
        {
            EvaluateChange( historyChangeList, propertyName, oldValue, newValue, isSensitive, null, null );
        }

        /// <summary>
        /// Evaluates the change, and adds a summary string of what if anything changed
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="isSensitive">Indicator of whether the values are sensitive in nature and should not be logged.</param>
        [RockObsolete( "1.8" )]
        [Obsolete( HISTORY_METHOD_OBSOLETE_MESSAGE )]
        public static void EvaluateChange( List<string> historyMessages, string propertyName, string oldValue, string newValue, bool isSensitive = false )
        {
            if ( !string.IsNullOrWhiteSpace( oldValue ) )
            {
                if ( !string.IsNullOrWhiteSpace( newValue ) )
                {
                    if ( oldValue.Trim() != newValue.Trim() )
                    {
                        if ( isSensitive )
                        {
                            historyMessages.Add( string.Format( "Modified <span class='field-name'>{0}</span> value (Sensitive attribute values are not logged in history).", propertyName ) );
                        }
                        else
                        {
                            historyMessages.Add( string.Format( "Modified <span class='field-name'>{0}</span> value from <span class='field-value'>{1}</span> to <span class='field-value'>{2}</span>.", propertyName, oldValue, newValue ) );
                        }
                    }
                }
                else
                {
                    if ( isSensitive )
                    {
                        historyMessages.Add( string.Format( "Deleted <span class='field-name'>{0}</span> value (Sensitive attribute values are not logged in history).", propertyName ) );
                    }
                    else
                    {
                        historyMessages.Add( string.Format( "Deleted <span class='field-name'>{0}</span> value of <span class='field-value'>{1}</span>.", propertyName, oldValue ) );
                    }
                }
            }
            else if ( !string.IsNullOrWhiteSpace( newValue ) )
            {
                if ( isSensitive )
                {
                    historyMessages.Add( string.Format( "Added <span class='field-name'>{0}</span> value (Sensitive attribute values are not logged in history).", propertyName ) );
                }
                else
                {
                    historyMessages.Add( string.Format( "Added <span class='field-name'>{0}</span> value of <span class='field-value'>{1}</span>.", propertyName, newValue ) );
                }
            }
        }

        /// <summary>
        /// Evaluates the change, and adds a summary string of what if anything changed
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="isSensitive">Indicator of whether the values are sensitive in nature and should not be logged.</param>
        [RockObsolete( "1.8" )]
        [Obsolete( HISTORY_METHOD_OBSOLETE_MESSAGE )]
        public static void EvaluateChange( List<string> historyMessages, string propertyName, int? oldValue, int? newValue, bool isSensitive = false )
        {
            EvaluateChange(
                historyMessages,
                propertyName,
                oldValue.HasValue ? oldValue.Value.ToString() : string.Empty,
                newValue.HasValue ? newValue.Value.ToString() : string.Empty,
                isSensitive );
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyChangeList">The history change list.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="isSensitive">if set to <c>true</c> [is sensitive].</param>
        public static void EvaluateChange( HistoryChangeList historyChangeList, string propertyName, int? oldValue, int? newValue, bool isSensitive = false )
        {
            EvaluateChange(
                historyChangeList,
                propertyName,
                oldValue.HasValue ? oldValue.Value.ToString() : string.Empty,
                newValue.HasValue ? newValue.Value.ToString() : string.Empty,
                isSensitive );
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="isSensitive">Indicator of whether the values are sensitive in nature and should not be logged.</param>
        [RockObsolete( "1.8" )]
        [Obsolete( HISTORY_METHOD_OBSOLETE_MESSAGE )]
        public static void EvaluateChange( List<string> historyMessages, string propertyName, decimal? oldValue, decimal? newValue, bool isSensitive = false )
        {
            EvaluateChange(
                historyMessages,
                propertyName,
                oldValue.HasValue ? oldValue.Value.ToString( "N2" ) : string.Empty,
                newValue.HasValue ? newValue.Value.ToString( "N2" ) : string.Empty,
                isSensitive );
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyChangeList">The history change list.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="isSensitive">if set to <c>true</c> [is sensitive].</param>
        public static void EvaluateChange( HistoryChangeList historyChangeList, string propertyName, decimal? oldValue, decimal? newValue, bool isSensitive = false )
        {
            EvaluateChange(
                historyChangeList,
                propertyName,
                oldValue.HasValue ? oldValue.Value.ToString( "N2" ) : string.Empty,
                newValue.HasValue ? newValue.Value.ToString( "N2" ) : string.Empty,
                isSensitive );
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="includeTime">if set to <c>true</c> [include time].</param>
        /// <param name="isSensitive">Indicator of whether the values are sensitive in nature and should not be logged.</param>
        [RockObsolete( "1.8" )]
        [Obsolete( HISTORY_METHOD_OBSOLETE_MESSAGE )]
        public static void EvaluateChange( List<string> historyMessages, string propertyName, DateTime? oldValue, DateTime? newValue, bool includeTime = false, bool isSensitive = false )
        {
            string oldStringValue = string.Empty;
            if ( oldValue.HasValue )
            {
                oldStringValue = includeTime ? oldValue.Value.ToString() : oldValue.Value.ToShortDateString();
            }

            string newStringValue = string.Empty;
            if ( newValue.HasValue )
            {
                newStringValue = includeTime ? newValue.Value.ToString() : newValue.Value.ToShortDateString();
            }

            EvaluateChange( historyMessages, propertyName, oldStringValue, newStringValue, isSensitive );
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyChangeList">The history change list.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="includeTime">if set to <c>true</c> [include time].</param>
        /// <param name="isSensitive">if set to <c>true</c> [is sensitive].</param>
        public static void EvaluateChange( HistoryChangeList historyChangeList, string propertyName, DateTime? oldValue, DateTime? newValue, bool includeTime = false, bool isSensitive = false )
        {
            string oldStringValue = string.Empty;
            if ( oldValue.HasValue )
            {
                oldStringValue = includeTime ? oldValue.Value.ToString() : oldValue.Value.ToShortDateString();
            }

            string newStringValue = string.Empty;
            if ( newValue.HasValue )
            {
                newStringValue = includeTime ? newValue.Value.ToString() : newValue.Value.ToShortDateString();
            }

            EvaluateChange( historyChangeList, propertyName, oldStringValue, newStringValue, isSensitive );
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">if set to <c>true</c> [old value].</param>
        /// <param name="newValue">if set to <c>true</c> [new value].</param>
        /// <param name="isSensitive">Indicator of whether the values are sensitive in nature and should not be logged.</param>
        [RockObsolete( "1.8" )]
        [Obsolete( HISTORY_METHOD_OBSOLETE_MESSAGE )]
        public static void EvaluateChange( List<string> historyMessages, string propertyName, bool? oldValue, bool? newValue, bool isSensitive = false )
        {
            EvaluateChange(
                historyMessages,
                propertyName,
                oldValue.HasValue ? oldValue.Value.ToString() : string.Empty,
                newValue.HasValue ? newValue.Value.ToString() : string.Empty,
                isSensitive );
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyChangeList">The history change list.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="isSensitive">if set to <c>true</c> [is sensitive].</param>
        public static void EvaluateChange( HistoryChangeList historyChangeList, string propertyName, bool? oldValue, bool? newValue, bool isSensitive = false )
        {
            EvaluateChange(
                historyChangeList,
                propertyName,
                oldValue.HasValue ? oldValue.Value.ToString() : string.Empty,
                newValue.HasValue ? newValue.Value.ToString() : string.Empty,
                isSensitive );
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="isSensitive">Indicator of whether the values are sensitive in nature and should not be logged.</param>
        [RockObsolete( "1.8" )]
        [Obsolete( HISTORY_METHOD_OBSOLETE_MESSAGE )]
        public static void EvaluateChange( List<string> historyMessages, string propertyName, Enum oldValue, Enum newValue, bool isSensitive = false )
        {
            string oldStringValue = oldValue != null ? oldValue.ConvertToString() : string.Empty;
            string newStringValue = newValue != null ? newValue.ConvertToString() : string.Empty;
            EvaluateChange( historyMessages, propertyName, oldStringValue, newStringValue, isSensitive );
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyChangeList">The history change list.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="isSensitive">if set to <c>true</c> [is sensitive].</param>
        public static void EvaluateChange( HistoryChangeList historyChangeList, string propertyName, Enum oldValue, Enum newValue, bool isSensitive = false )
        {
            string oldStringValue = oldValue != null ? oldValue.ConvertToString() : string.Empty;
            string newStringValue = newValue != null ? newValue.ConvertToString() : string.Empty;
            EvaluateChange( historyChangeList, propertyName, oldStringValue, newStringValue, isSensitive );
        }

        /// <summary>
        /// Evaluates the defined value change.
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldDefinedValueId">The old defined value identifier.</param>
        /// <param name="newDefinedValue">The new defined value.</param>
        /// <param name="newDefinedValueId">The new defined value identifier.</param>
        [RockObsolete( "1.8" )]
        [Obsolete( HISTORY_METHOD_OBSOLETE_MESSAGE )]
        public static void EvaluateChange( List<string> historyMessages, string propertyName, int? oldDefinedValueId, DefinedValue newDefinedValue, int? newDefinedValueId )
        {
            EvaluateChange( historyMessages, propertyName, oldDefinedValueId, newDefinedValue, newDefinedValueId, string.Empty, false );
        }

        /// <summary>
        /// Evaluates the defined value change.
        /// </summary>
        /// <param name="historyChangeList">The history change list.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldDefinedValueId">The old defined value identifier.</param>
        /// <param name="newDefinedValue">The new defined value.</param>
        /// <param name="newDefinedValueId">The new defined value identifier.</param>
        public static void EvaluateChange( HistoryChangeList historyChangeList, string propertyName, int? oldDefinedValueId, DefinedValue newDefinedValue, int? newDefinedValueId )
        {
            EvaluateChange( historyChangeList, propertyName, oldDefinedValueId, newDefinedValue, newDefinedValueId, string.Empty, false );
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldDefinedValueId">The old defined value identifier.</param>
        /// <param name="newDefinedValue">The new defined value.</param>
        /// <param name="newDefinedValueId">The new defined value identifier.</param>
        /// <param name="blankValue">The blank value.</param>
        /// <param name="isSensitive">if set to <c>true</c> [is sensitive].</param>
        [RockObsolete( "1.8" )]
        [Obsolete( HISTORY_METHOD_OBSOLETE_MESSAGE )]
        public static void EvaluateChange( List<string> historyMessages, string propertyName, int? oldDefinedValueId, DefinedValue newDefinedValue, int? newDefinedValueId, string blankValue, bool isSensitive )
        {
            if ( !oldDefinedValueId.Equals( newDefinedValueId ) )
            {
                string oldStringValue = GetDefinedValueValue( null, oldDefinedValueId, blankValue );
                string newStringValue = GetDefinedValueValue( newDefinedValue, newDefinedValueId, blankValue );
                EvaluateChange( historyMessages, propertyName, oldStringValue, newStringValue, isSensitive );
            }
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyChangeList">The history change list.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldDefinedValueId">The old defined value identifier.</param>
        /// <param name="newDefinedValue">The new defined value.</param>
        /// <param name="newDefinedValueId">The new defined value identifier.</param>
        /// <param name="blankValue">The blank value.</param>
        /// <param name="isSensitive">if set to <c>true</c> [is sensitive].</param>
        public static void EvaluateChange( HistoryChangeList historyChangeList, string propertyName, int? oldDefinedValueId, DefinedValue newDefinedValue, int? newDefinedValueId, string blankValue, bool isSensitive )
        {
            if ( !oldDefinedValueId.Equals( newDefinedValueId ) )
            {
                string oldStringValue = GetDefinedValueValue( null, oldDefinedValueId, blankValue );
                string newStringValue = GetDefinedValueValue( newDefinedValue, newDefinedValueId, blankValue );

                EvaluateChange( historyChangeList, propertyName, oldStringValue, newStringValue, isSensitive, oldDefinedValueId.ToStringSafe(), newDefinedValueId.ToStringSafe() );
            }
        }

        /// <summary>
        /// Evaluates a group type value change.
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldGroupTypeId">The old defined value identifier.</param>
        /// <param name="newGroupType">The new defined value.</param>
        /// <param name="newGroupTypeId">The new defined value identifier.</param>
        [RockObsolete( "1.8" )]
        [Obsolete( HISTORY_METHOD_OBSOLETE_MESSAGE )]
        public static void EvaluateChange( List<string> historyMessages, string propertyName, int? oldGroupTypeId, GroupType newGroupType, int? newGroupTypeId )
        {
            EvaluateChange( historyMessages, propertyName, oldGroupTypeId, newGroupType, newGroupTypeId, string.Empty, false );
        }

        /// <summary>
        /// Evaluates a group type value change.
        /// </summary>
        /// <param name="historyChangeList">The history change list.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldGroupTypeId">The old group type identifier.</param>
        /// <param name="newGroupType">New type of the group.</param>
        /// <param name="newGroupTypeId">The new group type identifier.</param>
        public static void EvaluateChange( HistoryChangeList historyChangeList, string propertyName, int? oldGroupTypeId, GroupType newGroupType, int? newGroupTypeId )
        {
            EvaluateChange( historyChangeList, propertyName, oldGroupTypeId, newGroupType, newGroupTypeId, string.Empty, false );
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldGroupTypeId">The old defined value identifier.</param>
        /// <param name="newGroupType">The new defined value.</param>
        /// <param name="newGroupTypeId">The new defined value identifier.</param>
        /// <param name="blankValue">The blank value.</param>
        /// <param name="isSensitive">if set to <c>true</c> [is sensitive].</param>
        [RockObsolete( "1.8" )]
        [Obsolete( HISTORY_METHOD_OBSOLETE_MESSAGE )]
        public static void EvaluateChange( List<string> historyMessages, string propertyName, int? oldGroupTypeId, GroupType newGroupType, int? newGroupTypeId, string blankValue, bool isSensitive )
        {
            if ( !oldGroupTypeId.Equals( newGroupTypeId ) )
            {
                string oldStringValue = GetGroupTypeValue( null, oldGroupTypeId, blankValue );
                string newStringValue = GetGroupTypeValue( newGroupType, newGroupTypeId, blankValue );
                EvaluateChange( historyMessages, propertyName, oldStringValue, newStringValue, isSensitive );
            }
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyChangeList">The history change list.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldGroupTypeId">The old group type identifier.</param>
        /// <param name="newGroupType">New type of the group.</param>
        /// <param name="newGroupTypeId">The new group type identifier.</param>
        /// <param name="blankValue">The blank value.</param>
        /// <param name="isSensitive">if set to <c>true</c> [is sensitive].</param>
        public static void EvaluateChange( HistoryChangeList historyChangeList, string propertyName, int? oldGroupTypeId, GroupType newGroupType, int? newGroupTypeId, string blankValue, bool isSensitive )
        {
            if ( !oldGroupTypeId.Equals( newGroupTypeId ) )
            {
                string oldStringValue = GetGroupTypeValue( null, oldGroupTypeId, blankValue );
                string newStringValue = GetGroupTypeValue( newGroupType, newGroupTypeId, blankValue );
                EvaluateChange( historyChangeList, propertyName, oldStringValue, newStringValue, isSensitive );
            }
        }

        /// <summary>
        /// Evaluates a campus change.
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldCampusId">The old defined value identifier.</param>
        /// <param name="newCampus">The new defined value.</param>
        /// <param name="newCampusId">The new defined value identifier.</param>
        [RockObsolete( "1.8" )]
        [Obsolete( HISTORY_METHOD_OBSOLETE_MESSAGE )]
        public static void EvaluateChange( List<string> historyMessages, string propertyName, int? oldCampusId, Campus newCampus, int? newCampusId )
        {
            EvaluateChange( historyMessages, propertyName, oldCampusId, newCampus, newCampusId, string.Empty, false );
        }

        /// <summary>
        /// Evaluates a campus change.
        /// </summary>
        /// <param name="historyChangeList">The history change list.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldCampusId">The old campus identifier.</param>
        /// <param name="newCampus">The new campus.</param>
        /// <param name="newCampusId">The new campus identifier.</param>
        public static void EvaluateChange( HistoryChangeList historyChangeList, string propertyName, int? oldCampusId, Campus newCampus, int? newCampusId )
        {
            EvaluateChange( historyChangeList, propertyName, oldCampusId, newCampus, newCampusId, string.Empty, false );
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldCampusId">The old defined value identifier.</param>
        /// <param name="newCampus">The new defined value.</param>
        /// <param name="newCampusId">The new defined value identifier.</param>
        /// <param name="blankValue">The blank value.</param>
        /// <param name="isSensitive">if set to <c>true</c> [is sensitive].</param>
        [RockObsolete( "1.8" )]
        [Obsolete( HISTORY_METHOD_OBSOLETE_MESSAGE )]
        public static void EvaluateChange( List<string> historyMessages, string propertyName, int? oldCampusId, Campus newCampus, int? newCampusId, string blankValue, bool isSensitive )
        {
            if ( !oldCampusId.Equals( newCampusId ) )
            {
                string oldStringValue = GetCampusValue( null, oldCampusId, blankValue );
                string newStringValue = GetCampusValue( newCampus, newCampusId, blankValue );
                EvaluateChange( historyMessages, propertyName, oldStringValue, newStringValue, isSensitive );
            }
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyChangeList">The history change list.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldCampusId">The old campus identifier.</param>
        /// <param name="newCampus">The new campus.</param>
        /// <param name="newCampusId">The new campus identifier.</param>
        /// <param name="blankValue">The blank value.</param>
        /// <param name="isSensitive">if set to <c>true</c> [is sensitive].</param>
        public static void EvaluateChange( HistoryChangeList historyChangeList, string propertyName, int? oldCampusId, Campus newCampus, int? newCampusId, string blankValue, bool isSensitive )
        {
            if ( !oldCampusId.Equals( newCampusId ) )
            {
                string oldStringValue = GetCampusValue( null, oldCampusId, blankValue );
                string newStringValue = GetCampusValue( newCampus, newCampusId, blankValue );
                EvaluateChange( historyChangeList, propertyName, oldStringValue, newStringValue, isSensitive );
            }
        }

        /// <summary>
        /// Evaluates the person alias change.
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldPersonAliasId">The old person alias identifier.</param>
        /// <param name="newPersonAlias">The new person alias.</param>
        /// <param name="newPersonAliasId">The new person alias identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        [RockObsolete( "1.8" )]
        [Obsolete( HISTORY_METHOD_OBSOLETE_MESSAGE )]
        public static void EvaluateChange( List<string> historyMessages, string propertyName, int? oldPersonAliasId, PersonAlias newPersonAlias, int? newPersonAliasId, RockContext rockContext )
        {
            EvaluateChange( historyMessages, propertyName, oldPersonAliasId, newPersonAlias, newPersonAliasId, rockContext, string.Empty, false );
        }

        /// <summary>
        /// Evaluates the person alias change.
        /// </summary>
        /// <param name="historyChangeList">The history change list.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldPersonAliasId">The old person alias identifier.</param>
        /// <param name="newPersonAlias">The new person alias.</param>
        /// <param name="newPersonAliasId">The new person alias identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void EvaluateChange( HistoryChangeList historyChangeList, string propertyName, int? oldPersonAliasId, PersonAlias newPersonAlias, int? newPersonAliasId, RockContext rockContext )
        {
            EvaluateChange( historyChangeList, propertyName, oldPersonAliasId, newPersonAlias, newPersonAliasId, rockContext, string.Empty, false );
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldPersonAliasId">The old person alias identifier.</param>
        /// <param name="newPersonAlias">The new person alias.</param>
        /// <param name="newPersonAliasId">The new person alias identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="blankValue">The blank value.</param>
        /// <param name="isSensitive">if set to <c>true</c> [is sensitive].</param>
        [RockObsolete( "1.8" )]
        [Obsolete( HISTORY_METHOD_OBSOLETE_MESSAGE )]
        public static void EvaluateChange( List<string> historyMessages, string propertyName, int? oldPersonAliasId, PersonAlias newPersonAlias, int? newPersonAliasId, RockContext rockContext, string blankValue, bool isSensitive )
        {
            if ( !oldPersonAliasId.Equals( newPersonAliasId ) )
            {
                string oldStringValue = GetValue<PersonAlias>( null, oldPersonAliasId, rockContext, blankValue );
                string newStringValue = GetValue<PersonAlias>( newPersonAlias, newPersonAliasId, rockContext, blankValue );
                EvaluateChange( historyMessages, propertyName, oldStringValue, newStringValue, isSensitive );
            }
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyChangeList">The history change list.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldPersonAliasId">The old person alias identifier.</param>
        /// <param name="newPersonAlias">The new person alias.</param>
        /// <param name="newPersonAliasId">The new person alias identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="blankValue">The blank value.</param>
        /// <param name="isSensitive">if set to <c>true</c> [is sensitive].</param>
        public static void EvaluateChange( HistoryChangeList historyChangeList, string propertyName, int? oldPersonAliasId, PersonAlias newPersonAlias, int? newPersonAliasId, RockContext rockContext, string blankValue, bool isSensitive )
        {
            if ( !oldPersonAliasId.Equals( newPersonAliasId ) )
            {
                string oldStringValue = GetValue<PersonAlias>( null, oldPersonAliasId, rockContext, blankValue );
                string newStringValue = GetValue<PersonAlias>( newPersonAlias, newPersonAliasId, rockContext, blankValue );
                EvaluateChange( historyChangeList, propertyName, oldStringValue, newStringValue, isSensitive );
            }
        }

        /// <summary>
        /// Evaluates the person alias change.
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldGroupId">The old person alias identifier.</param>
        /// <param name="newGroup">The new person alias.</param>
        /// <param name="newGroupId">The new person alias identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        [RockObsolete( "1.8" )]
        [Obsolete( HISTORY_METHOD_OBSOLETE_MESSAGE )]
        public static void EvaluateChange( List<string> historyMessages, string propertyName, int? oldGroupId, Group newGroup, int? newGroupId, RockContext rockContext )
        {
            EvaluateChange( historyMessages, propertyName, oldGroupId, newGroup, newGroupId, rockContext, string.Empty, false );
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldGroupId">The old person alias identifier.</param>
        /// <param name="newGroup">The new person alias.</param>
        /// <param name="newGroupId">The new person alias identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="blankValue">The blank value.</param>
        /// <param name="isSensitive">if set to <c>true</c> [is sensitive].</param>
        [RockObsolete( "1.8" )]
        [Obsolete( HISTORY_METHOD_OBSOLETE_MESSAGE )]
        public static void EvaluateChange( List<string> historyMessages, string propertyName, int? oldGroupId, Group newGroup, int? newGroupId, RockContext rockContext, string blankValue, bool isSensitive )
        {
            if ( !oldGroupId.Equals( newGroupId ) )
            {
                string oldStringValue = GetValue<Group>( null, oldGroupId, rockContext, blankValue );
                string newStringValue = GetValue<Group>( newGroup, newGroupId, rockContext, blankValue );
                EvaluateChange( historyMessages, propertyName, oldStringValue, newStringValue, isSensitive );
            }
        }

        /// <summary>
        /// Evaluates the person alias change.
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldGroupTypeRoleId">The old person alias identifier.</param>
        /// <param name="newGroupTypeRole">The new person alias.</param>
        /// <param name="newGroupTypeRoleId">The new person alias identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        [RockObsolete( "1.8" )]
        [Obsolete( HISTORY_METHOD_OBSOLETE_MESSAGE )]
        public static void EvaluateChange( List<string> historyMessages, string propertyName, int? oldGroupTypeRoleId, GroupTypeRole newGroupTypeRole, int? newGroupTypeRoleId, RockContext rockContext )
        {
            EvaluateChange( historyMessages, propertyName, oldGroupTypeRoleId, newGroupTypeRole, newGroupTypeRoleId, rockContext, string.Empty, false );
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyChangeList">The history change list.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldGroupTypeRoleId">The old group type role identifier.</param>
        /// <param name="newGroupTypeRole">The new group type role.</param>
        /// <param name="newGroupTypeRoleId">The new group type role identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void EvaluateChange( HistoryChangeList historyChangeList, string propertyName, int? oldGroupTypeRoleId, GroupTypeRole newGroupTypeRole, int? newGroupTypeRoleId, RockContext rockContext )
        {
            EvaluateChange( historyChangeList, propertyName, oldGroupTypeRoleId, newGroupTypeRole, newGroupTypeRoleId, rockContext, string.Empty, false );
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldGroupTypeRoleId">The old person alias identifier.</param>
        /// <param name="newGroupTypeRole">The new person alias.</param>
        /// <param name="newGroupTypeRoleId">The new person alias identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="blankValue">The blank value.</param>
        /// <param name="isSensitive">if set to <c>true</c> [is sensitive].</param>
        [RockObsolete( "1.8" )]
        [Obsolete( HISTORY_METHOD_OBSOLETE_MESSAGE )]
        public static void EvaluateChange( List<string> historyMessages, string propertyName, int? oldGroupTypeRoleId, GroupTypeRole newGroupTypeRole, int? newGroupTypeRoleId, RockContext rockContext, string blankValue, bool isSensitive )
        {
            if ( !oldGroupTypeRoleId.Equals( newGroupTypeRoleId ) )
            {
                string oldStringValue = GetValue<GroupTypeRole>( null, oldGroupTypeRoleId, rockContext, blankValue );
                string newStringValue = GetValue<GroupTypeRole>( newGroupTypeRole, newGroupTypeRoleId, rockContext, blankValue );
                EvaluateChange( historyMessages, propertyName, oldStringValue, newStringValue, isSensitive );
            }
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyChangeList">The history change list.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldGroupTypeRoleId">The old group type role identifier.</param>
        /// <param name="newGroupTypeRole">The new group type role.</param>
        /// <param name="newGroupTypeRoleId">The new group type role identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="blankValue">The blank value.</param>
        /// <param name="isSensitive">if set to <c>true</c> [is sensitive].</param>
        public static void EvaluateChange( HistoryChangeList historyChangeList, string propertyName, int? oldGroupTypeRoleId, GroupTypeRole newGroupTypeRole, int? newGroupTypeRoleId, RockContext rockContext, string blankValue, bool isSensitive )
        {
            if ( !oldGroupTypeRoleId.Equals( newGroupTypeRoleId ) )
            {
                string oldStringValue = GetValue<GroupTypeRole>( null, oldGroupTypeRoleId, rockContext, blankValue );
                string newStringValue = GetValue<GroupTypeRole>( newGroupTypeRole, newGroupTypeRoleId, rockContext, blankValue );
                EvaluateChange( historyChangeList, propertyName, oldStringValue, newStringValue, isSensitive );
            }
        }

        /// <summary>
        /// Evaluates the person alias change.
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldLocationId">The old person alias identifier.</param>
        /// <param name="newLocation">The new person alias.</param>
        /// <param name="newLocationId">The new person alias identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        [RockObsolete( "1.8" )]
        [Obsolete( HISTORY_METHOD_OBSOLETE_MESSAGE )]
        public static void EvaluateChange( List<string> historyMessages, string propertyName, int? oldLocationId, Location newLocation, int? newLocationId, RockContext rockContext )
        {
            EvaluateChange( historyMessages, propertyName, oldLocationId, newLocation, newLocationId, rockContext, string.Empty, false );
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyChangeList">The history change list.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldLocationId">The old location identifier.</param>
        /// <param name="newLocation">The new location.</param>
        /// <param name="newLocationId">The new location identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void EvaluateChange( HistoryChangeList historyChangeList, string propertyName, int? oldLocationId, Location newLocation, int? newLocationId, RockContext rockContext )
        {
            EvaluateChange( historyChangeList, propertyName, oldLocationId, newLocation, newLocationId, rockContext, string.Empty, false );
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldLocationId">The old person alias identifier.</param>
        /// <param name="newLocation">The new person alias.</param>
        /// <param name="newLocationId">The new person alias identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="blankValue">The blank value.</param>
        /// <param name="isSensitive">if set to <c>true</c> [is sensitive].</param>
        [RockObsolete( "1.8" )]
        [Obsolete( HISTORY_METHOD_OBSOLETE_MESSAGE )]
        public static void EvaluateChange( List<string> historyMessages, string propertyName, int? oldLocationId, Location newLocation, int? newLocationId, RockContext rockContext, string blankValue, bool isSensitive )
        {
            if ( !oldLocationId.Equals( newLocationId ) )
            {
                string oldStringValue = GetValue<Location>( null, oldLocationId, rockContext, blankValue );
                string newStringValue = GetValue<Location>( newLocation, newLocationId, rockContext, blankValue );
                EvaluateChange( historyMessages, propertyName, oldStringValue, newStringValue, isSensitive );
            }
        }

        /// <summary>
        /// Evaluates the change.
        /// </summary>
        /// <param name="historyChangeList">The history change list.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldLocationId">The old location identifier.</param>
        /// <param name="newLocation">The new location.</param>
        /// <param name="newLocationId">The new location identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="blankValue">The blank value.</param>
        /// <param name="isSensitive">if set to <c>true</c> [is sensitive].</param>
        public static void EvaluateChange( HistoryChangeList historyChangeList, string propertyName, int? oldLocationId, Location newLocation, int? newLocationId, RockContext rockContext, string blankValue, bool isSensitive )
        {
            if ( !oldLocationId.Equals( newLocationId ) )
            {
                string oldStringValue = GetValue<Location>( null, oldLocationId, rockContext, blankValue );
                string newStringValue = GetValue<Location>( newLocation, newLocationId, rockContext, blankValue );
                EvaluateChange( historyChangeList, propertyName, oldStringValue, newStringValue, isSensitive );
            }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static string GetValue<T>( T entity, int? id, RockContext rockContext ) where T : Rock.Data.Entity<T>, new()
        {
            return GetValue<T>( entity, id, rockContext, string.Empty );
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="blankValue">The blank value.</param>
        /// <returns></returns>
        public static string GetValue<T>( T entity, int? id, RockContext rockContext, string blankValue ) where T : Rock.Data.Entity<T>, new()
        {
            if ( typeof( T ) == typeof( DefinedValue ) )
            {
                return GetDefinedValueValue( entity as DefinedValue, id, blankValue );
            }

            if ( typeof( T ) == typeof( GroupType ) )
            {
                return GetGroupTypeValue( entity as GroupType, id, blankValue );
            }

            if ( typeof( T ) == typeof( Campus ) )
            {
                return GetCampusValue( entity as Campus, id, blankValue );
            }

            if ( typeof( T ) == typeof( PersonAlias ) )
            {
                return GetPersonAliasValue( entity as PersonAlias, id, rockContext, blankValue );
            }

            if ( typeof( T ) == typeof( Location ) )
            {
                return GetLocationValue( entity as Location, id, rockContext, blankValue );
            }

            if ( typeof( T ) == typeof( Group ) )
            {
                return GetGroupValue( entity as Group, id, rockContext, blankValue );
            }

            if ( typeof( T ) == typeof( GroupTypeRole ) )
            {
                return GetGroupTypeRoleValue( entity as GroupTypeRole, id, rockContext, blankValue );
            }

            if ( entity == null && id.HasValue )
            {
                var service = new Service<T>( rockContext );
                if ( service != null )
                {
                    entity = service.GetNoTracking( id.Value );
                }
            }

            return entity != null ? string.Format( "{0} [{1}]", entity.ToString(), entity.Id ) : blankValue;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="definedValue">The defined value.</param>
        /// <param name="definedValueId">The defined value identifier.</param>
        /// <returns></returns>
        public static string GetDefinedValueValue( DefinedValue definedValue, int? definedValueId )
        {
            return GetDefinedValueValue( definedValue, definedValueId, string.Empty );
        }

        /// <summary>
        /// Gets the defined value value.
        /// </summary>
        /// <param name="definedValue">The defined value.</param>
        /// <param name="definedValueId">The defined value identifier.</param>
        /// <param name="blankValue">The blank value.</param>
        /// <returns></returns>
        public static string GetDefinedValueValue( DefinedValue definedValue, int? definedValueId, string blankValue )
        {
            if ( definedValue != null )
            {
                return definedValue.Value;
            }

            if ( definedValueId.HasValue )
            {
                var dv = DefinedValueCache.Get( definedValueId.Value );
                if ( dv != null )
                {
                    return dv.Value;
                }
            }

            return blankValue;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="personAlias">The person alias.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static string GetPersonAliasValue( PersonAlias personAlias, int? personAliasId, RockContext rockContext )
        {
            return GetPersonAliasValue( personAlias, personAliasId, rockContext, string.Empty );
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="personAlias">The person alias.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="blankValue">The blank value.</param>
        /// <returns></returns>
        private static string GetPersonAliasValue( PersonAlias personAlias, int? personAliasId, RockContext rockContext, string blankValue )
        {
            Person person = null;
            if ( personAlias != null && personAlias.Person != null )
            {
                person = personAlias.Person;
            }
            else if ( personAliasId.HasValue )
            {
                person = new PersonAliasService( rockContext ).GetPersonNoTracking( personAliasId.Value );
            }

            return person != null ? string.Format( "{0} [{1}]", person.FullName, person.Id ) : blankValue;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="groupType">The defined value.</param>
        /// <param name="groupTypeId">The defined value identifier.</param>
        /// <returns></returns>
        public static string GetGroupTypeValue( GroupType groupType, int? groupTypeId )
        {
            return GetGroupTypeValue( groupType, groupTypeId, string.Empty );
        }

        /// <summary>
        /// Gets the defined value value.
        /// </summary>
        /// <param name="groupType">The defined value.</param>
        /// <param name="groupTypeId">The defined value identifier.</param>
        /// <param name="blankValue">The blank value.</param>
        /// <returns></returns>
        public static string GetGroupTypeValue( GroupType groupType, int? groupTypeId, string blankValue )
        {
            if ( groupType != null )
            {
                return groupType.Name;
            }

            if ( groupTypeId.HasValue )
            {
                var dv = GroupTypeCache.Get( groupTypeId.Value );
                if ( dv != null )
                {
                    return dv.Name;
                }
            }

            return blankValue;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="campus">The defined value.</param>
        /// <param name="campusId">The defined value identifier.</param>
        /// <returns></returns>
        public static string GetCampusValue( Campus campus, int? campusId )
        {
            return GetCampusValue( campus, campusId, string.Empty );
        }

        /// <summary>
        /// Gets the defined value value.
        /// </summary>
        /// <param name="campus">The defined value.</param>
        /// <param name="campusId">The defined value identifier.</param>
        /// <param name="blankValue">The blank value.</param>
        /// <returns></returns>
        public static string GetCampusValue( Campus campus, int? campusId, string blankValue )
        {
            if ( campus != null )
            {
                return campus.Name;
            }

            if ( campusId.HasValue )
            {
                var dv = CampusCache.Get( campusId.Value );
                if ( dv != null )
                {
                    return dv.Name;
                }
            }

            return blankValue;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="location">The person alias.</param>
        /// <param name="locationId">The person alias identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static string GetLocationValue( Location location, int? locationId, RockContext rockContext )
        {
            return GetLocationValue( location, locationId, rockContext, string.Empty );
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="location">The person alias.</param>
        /// <param name="locationId">The person alias identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="blankValue">The blank value.</param>
        /// <returns></returns>
        private static string GetLocationValue( Location location, int? locationId, RockContext rockContext, string blankValue )
        {
            if ( location != null )
            {
                return location.ToString();
            }

            if ( locationId.HasValue )
            {
                var loc = new LocationService( rockContext ).Get( locationId.Value );
                if ( loc != null )
                {
                    return loc.ToString();
                }
            }

            return blankValue;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="group">The person alias.</param>
        /// <param name="groupId">The person alias identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static string GetGroupValue( Group group, int? groupId, RockContext rockContext )
        {
            return GetGroupValue( group, groupId, rockContext, string.Empty );
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="group">The person alias.</param>
        /// <param name="groupId">The person alias identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="blankValue">The blank value.</param>
        /// <returns></returns>
        private static string GetGroupValue( Group group, int? groupId, RockContext rockContext, string blankValue )
        {
            if ( group != null )
            {
                return group.Name;
            }

            if ( groupId.HasValue )
            {
                var grp = new GroupService( rockContext ).Get( groupId.Value );
                if ( grp != null )
                {
                    return grp.Name;
                }
            }

            return blankValue;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="groupTypeRole">The person alias.</param>
        /// <param name="groupTypeRoleId">The person alias identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static string GetGroupTypeRoleValue( GroupTypeRole groupTypeRole, int? groupTypeRoleId, RockContext rockContext )
        {
            return GetGroupTypeRoleValue( groupTypeRole, groupTypeRoleId, rockContext, string.Empty );
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="groupTypeRole">The person alias.</param>
        /// <param name="groupTypeRoleId">The person alias identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="blankValue">The blank value.</param>
        /// <returns></returns>
        private static string GetGroupTypeRoleValue( GroupTypeRole groupTypeRole, int? groupTypeRoleId, RockContext rockContext, string blankValue )
        {
            if ( groupTypeRole != null )
            {
                return groupTypeRole.Name;
            }

            if ( groupTypeRoleId.HasValue )
            {
                var roleName = new GroupTypeRoleService( rockContext ).GetSelect( groupTypeRoleId.Value, a=> a.Name );
                if ( roleName != null )
                {
                    return roleName;
                }
            }

            return blankValue;
        }

        #endregion

        /// <summary>
        /// The history method obsolete message
        /// </summary>
        internal const string HISTORY_METHOD_OBSOLETE_MESSAGE = "In most cases, the service layer now takes care of logging history. So this no longer needs to be called. Otherwise use the new version of this method that takes HistoryChangeList as a parameter.";

        #region History Classes

        /// <summary>
        /// Helper class that can be used to keep add and track of History Changes and can be passed around to the various History methods
        /// </summary>
        public class HistoryChangeList : List<HistoryChange>
        {
            /// <summary>
            /// Adds a HistoryChange record to the list 
            /// Returns the HistoryChange object so caller can set additional property values if needed
            /// </summary>
            /// <param name="historyVerb">The history verb.</param>
            /// <param name="historyChangeType">Whether this is a property change, attribute change, or something else</param>
            /// <param name="valueName">Depending on  HistoryChangeType.Property: Property Friendly Name, Attribute =&gt; Attribute.Name, etc</param>
            /// <param name="oldValue">The value of the property prior to the change (Set this when doing a DELETE or MODIFY)</param>
            /// <param name="newValue">The value of the property after the change (Set this when doing an ADD or MODIFY)</param>
            /// <returns></returns>
            public HistoryChange AddChange( HistoryVerb historyVerb, HistoryChangeType historyChangeType, string valueName, string oldValue, string newValue )
            {
                var historyChange = new HistoryChange( historyVerb, historyChangeType, valueName, newValue, oldValue );
                this.Add( historyChange );
                return historyChange;
            }

            /// <summary>
            /// Adds a HistoryChange record to the list.
            /// Returns the HistoryChange object so caller can set additional property values if needed
            /// </summary>
            /// <param name="historyVerb">The history verb.</param>
            /// <param name="historyChangeType">Type of the history change.</param>
            /// <param name="valueName">Name of the value.</param>
            /// <returns></returns>
            public HistoryChange AddChange( HistoryVerb historyVerb, HistoryChangeType historyChangeType, string valueName )
            {
                return AddChange( historyVerb, historyChangeType, valueName, null, null );
            }

            /// <summary>
            /// Adds a custom history change that doesn't use any of the common HistoryVerbs and/or HistoryChangeTypes
            /// </summary>
            /// <param name="customVerb">The custom verb.</param>
            /// <param name="customChangeType">Type of the custom change.</param>
            /// <param name="valueName">Name of the value.</param>
            public HistoryChange AddCustom( string customVerb, string customChangeType, string valueName )
            {
                var historyChange = new HistoryChange { Verb = customVerb, ChangeType = customChangeType, ValueName = valueName };
                this.Add( historyChange );
                return historyChange;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class HistoryChange
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="HistoryChange"/> class.
            /// </summary>
            /// <param name="summary">The summary.</param>
            [RockObsolete( "1.8" )]
            [Obsolete]
            public HistoryChange( string summary )
            {
                this.Summary = summary;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="HistoryChange"/> class.
            /// </summary>
            public HistoryChange()
            {
                //
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="HistoryChange"/> class.
            /// </summary>
            /// <param name="historyVerb">The history verb.</param>
            /// <param name="historyChangeType">Type of the history change.</param>
            /// <param name="valueName">Name of the value.</param>
            public HistoryChange( HistoryVerb historyVerb, HistoryChangeType historyChangeType, string valueName )
                : this( historyVerb, historyChangeType, valueName, null, null )
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="HistoryChange"/> class.
            /// </summary>
            /// <param name="historyVerb">The history verb.</param>
            /// <param name="historyChangeType">Whether this is a property change, attribute change, or something else</param>
            /// <param name="valueName">Depending on  HistoryChangeType.Property: Property Friendly Name, Attribute => Attribute.Name, etc </param>
            /// <param name="newValue">The new value.</param>
            /// <param name="oldValue">The old value.</param>
            public HistoryChange( HistoryVerb historyVerb, HistoryChangeType historyChangeType, string valueName, string oldValue, string newValue )
            {
                this.Verb = historyVerb.ConvertToString( false ).ToUpper();
                this.ChangeType = historyChangeType.ConvertToString( false );
                this.ValueName = valueName;
                this.NewValue = newValue;
                this.OldValue = oldValue;
            }

            /// <summary>
            /// Gets or sets the verb.
            /// </summary>
            /// <value>
            /// The verb.
            /// </value>
            public string Verb { get; set; }

            /// <summary>
            /// Gets or sets the type of the change.
            /// </summary>
            /// <value>
            /// The type of the change.
            /// </value>
            public string ChangeType { get; set; }

            /// <summary>
            /// Gets or sets the name of the value.
            /// </summary>
            /// <value>
            /// The name of the value.
            /// </value>
            public string ValueName { get; set; }

            /// <summary>
            /// Gets or sets the new value.
            /// </summary>
            /// <value>
            /// The new value.
            /// </value>
            public string NewValue { get; set; }

            /// <summary>
            /// Creates new rawvalue.
            /// </summary>
            /// <value>
            /// The new raw value.
            /// </value>
            public string NewRawValue { get; set; }

            /// <summary>
            /// Gets or sets the old value.
            /// </summary>
            /// <value>
            /// The old value.
            /// </value>
            public string OldValue { get; set; }

            /// <summary>
            /// Gets or sets the old raw value.
            /// </summary>
            /// <value>
            /// The old raw value.
            /// </value>
            public string OldRawValue { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is sensitive.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is sensitive; otherwise, <c>false</c>.
            /// </value>
            public bool IsSensitive { get; set; }

            /// <summary>
            /// Optional: Gets or sets name of the tool or process that changed the value
            /// </summary>
            /// <value>
            /// The source of change.
            /// </value>
            public string SourceOfChange { get; set; }

            /// <summary>
            /// Gets or sets the related entity type identifier.
            /// </summary>
            /// <value>
            /// The related entity type identifier.
            /// </value>
            public int? RelatedEntityTypeId { get; set; }

            /// <summary>
            /// Gets or sets the related entity identifier.
            /// </summary>
            /// <value>
            /// The related entity identifier.
            /// </value>
            public int? RelatedEntityId { get; set; }

            /// <summary>
            /// Gets or sets the related data.
            /// </summary>
            /// <value>
            /// The related data.
            /// </value>
            public string RelatedData { get; set; }

            /// <summary>
            /// Gets the caption.
            /// </summary>
            /// <value>
            /// The caption.
            /// </value>
            public string Caption { get; set; }

            /// <summary>
            /// Gets or sets the summary.
            /// </summary>
            /// <value>
            /// The summary.
            /// </value>
            [RockObsolete( "1.8" )]
            [Obsolete]
            public string Summary { get; set; }

            /// <summary>
            /// Sets name of the tool or process that changed the value
            /// </summary>
            /// <param name="sourceOfChange">The source of change.</param>
            /// <returns></returns>
            public HistoryChange SetSourceOfChange( string sourceOfChange )
            {
                this.SourceOfChange = sourceOfChange;
                return this;
            }

            /// <summary>
            /// Sets History Record as Sensitive and ensures that OldValue and NewValue are not included
            /// </summary>
            /// <returns></returns>
            public HistoryChange SetSensitive()
            {
                this.IsSensitive = true;
                this.OldValue = null;
                this.NewValue = null;
                return this;
            }

            /// <summary>
            /// Sets the related data.
            /// </summary>
            /// <param name="relatedData">The related data.</param>
            /// <param name="relatedEntityTypeId">The related entity type identifier.</param>
            /// <param name="relatedEntityId">The related entity identifier.</param>
            /// <returns></returns>
            public HistoryChange SetRelatedData( string relatedData, int? relatedEntityTypeId, int? relatedEntityId )
            {
                this.RelatedData = relatedData;
                this.RelatedEntityTypeId = relatedEntityTypeId;
                this.RelatedEntityId = relatedEntityId;
                return this;
            }

            /// <summary>
            /// Sets the caption.
            /// </summary>
            /// <param name="caption">The caption.</param>
            /// <returns></returns>
            public HistoryChange SetCaption( string caption )
            {
                this.Caption = caption;
                return this;
            }

            /// <summary>
            /// Sets the value of the property after the change (set this if this is an ADD or MODIFY)
            /// </summary>
            /// <param name="newValue">The new value.</param>
            /// <returns></returns>
            public HistoryChange SetNewValue( string newValue )
            {
                this.NewValue = newValue;
                return this;
            }

            /// <summary>
            /// Sets the value of the property prior to the change (set this if this is a DELETE or MODIFY)
            /// </summary>
            /// <param name="oldValue">The old value.</param>
            /// <returns></returns>
            public HistoryChange SetOldValue( string oldValue )
            {
                this.OldValue = oldValue;
                return this;
            }

            /// <summary>
            /// Sets the raw values.
            /// </summary>
            /// <param name="oldRawValue">The old raw value.</param>
            /// <param name="newRawvValue">The new rawv value.</param>
            /// <returns></returns>
            public HistoryChange SetRawValues( string oldRawValue, string newRawvValue )
            {
                this.OldRawValue = oldRawValue;
                this.NewRawValue = newRawvValue;
                return this;
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                // create a temporary history object and set it's properties so that we can get the ToString() (the formatted summary)
                History history = new History();
                this.CopyToHistory( history );
                return history.ToString();
            }

            /// <summary>
            /// Copies the HistoryChange properties to the history record
            /// </summary>
            /// <param name="history">The history.</param>
            internal void CopyToHistory( History history )
            {
                if ( !string.IsNullOrEmpty( this.Caption ) )
                {
                    // if this individual change has a Caption, use that instead of the one applied to the HistoryChangeList
                    history.Caption = this.Caption.Truncate( 200 );
                }

                // for backwards compatibility, still store summary (and we can ignore the Obsolete warning here)
#pragma warning disable 612, 618
                history.Summary = this.Summary;
#pragma warning restore 612, 618

                history.Verb = this.Verb;
                history.ChangeType = this.ChangeType;
                history.ValueName = this.ValueName.Truncate( 250 );
                history.SourceOfChange = this.SourceOfChange;
                history.IsSensitive = this.IsSensitive;
                history.OldValue = this.OldValue;
                history.NewValue = this.NewValue;
                history.NewRawValue = this.NewRawValue;
                history.OldRawValue = this.OldRawValue;
                if ( this.RelatedEntityTypeId.HasValue )
                {
                    // if this individual change has a RelatedEntityTypeId, use that instead of the one applied to the HistoryChangeList
                    history.RelatedEntityTypeId = this.RelatedEntityTypeId;
                }

                if ( this.RelatedEntityId.HasValue )
                {
                    // if this individual change has a RelatedEntityId, use that instead of the one applied to the HistoryChangeList
                    history.RelatedEntityId = this.RelatedEntityId;
                }

                history.RelatedData = this.RelatedData;
            }
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// History Configuration class.
    /// </summary>
    public partial class HistoryConfiguration : EntityTypeConfiguration<History>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryConfiguration"/> class.
        /// </summary>
        public HistoryConfiguration()
        {
            this.HasRequired( p => p.EntityType ).WithMany().HasForeignKey( p => p.EntityTypeId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.Category ).WithMany().HasForeignKey( p => p.CategoryId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.RelatedEntityType ).WithMany().HasForeignKey( p => p.RelatedEntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}