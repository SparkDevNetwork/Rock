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

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a history that is entered in Rock and is associated with a specific entity. For example, a history could be entered on a person, GroupMember, a device, etc or for a specific subset of an entity type.
    /// </summary>
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
            return this.Summary;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Evaluates the change, and adds a summary string of what if anything changed
        /// </summary>
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="isSensitive">Indicator of whether the values are sensitive in nature and should not be logged.</param>
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
        public static void EvaluateChange( List<string> historyMessages, string propertyName, int? oldValue, int? newValue, bool isSensitive = false)
        {
            EvaluateChange( historyMessages, propertyName,
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
        public static void EvaluateChange( List<string> historyMessages, string propertyName, decimal? oldValue, decimal? newValue, bool isSensitive = false )
        {
            EvaluateChange( historyMessages, propertyName,
                oldValue.HasValue ? oldValue.Value.ToString("N2") : string.Empty,
                newValue.HasValue ? newValue.Value.ToString("N2") : string.Empty,
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
        /// <param name="historyMessages">The history messages.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">if set to <c>true</c> [old value].</param>
        /// <param name="newValue">if set to <c>true</c> [new value].</param>
        /// <param name="isSensitive">Indicator of whether the values are sensitive in nature and should not be logged.</param>
        public static void EvaluateChange( List<string> historyMessages, string propertyName, bool? oldValue, bool? newValue, bool isSensitive = false )
        {
            EvaluateChange( historyMessages, propertyName,
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
        public static void EvaluateChange( List<string> historyMessages, string propertyName, Enum oldValue, Enum newValue, bool isSensitive = false )
        {
            string oldStringValue = oldValue != null ? oldValue.ConvertToString() : string.Empty;
            string newStringValue = newValue != null ? newValue.ConvertToString() : string.Empty;
            EvaluateChange( historyMessages, propertyName, oldStringValue, newStringValue, isSensitive );
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
