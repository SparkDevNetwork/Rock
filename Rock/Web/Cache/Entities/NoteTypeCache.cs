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
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a NoteType that is cached by Rock. 
    /// </summary>
    [Serializable]
    [DataContract]
    public class NoteTypeCache : ModelCache<NoteTypeCache, NoteType>
    {

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; private set; }

        /// <summary>
        /// Gets or sets the entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the entity type qualifier column.
        /// </summary>
        /// <value>
        /// The entity type qualifier column.
        /// </value>
        [DataMember]
        public string EntityTypeQualifierColumn { get; private set; }

        /// <summary>
        /// Gets or sets the entity type qualifier value.
        /// </summary>
        /// <value>
        /// The entity type qualifier value.
        /// </value>
        [DataMember]
        public string EntityTypeQualifierValue { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [user selectable].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [user selectable]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool UserSelectable { get; private set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        [RockObsolete( "1.8" )]
        [Obsolete( "No Longer Supported" )]
        public string CssClass { get; private set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [DataMember]
        public string IconCssClass { get; private set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [requires approvals].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [requires approvals]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool RequiresApprovals { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allows watching].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allows watching]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowsWatching { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allows replies].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allows replies]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowsReplies { get; set; }

        /// <summary>
        /// Gets or sets the maximum reply depth.
        /// </summary>
        /// <value>
        /// The maximum reply depth.
        /// </value>
        [DataMember]
        public int? MaxReplyDepth { get; set; }

        /// <summary>
        /// Gets or sets the background color of each note
        /// </summary>
        /// <value>
        /// The color of the background.
        /// </value>
        [DataMember]
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the font color of the note text
        /// </summary>
        /// <value>
        /// The color of the font.
        /// </value>
        [DataMember]
        public string FontColor { get; set; }

        /// <summary>
        /// Gets or sets the border color of each note
        /// </summary>
        /// <value>
        /// The color of the border.
        /// </value>
        [DataMember]
        public string BorderColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [send approval notifications].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [send approval notifications]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool SendApprovalNotifications { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [automatic watch authors].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [automatic watch authors]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AutoWatchAuthors { get; set; }

        /// <summary>
        /// Gets or sets the approval URL template.
        /// </summary>
        /// <value>
        /// The approval URL template.
        /// </value>
        [DataMember]
        public string ApprovalUrlTemplate
        {
            get; set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// A dictionary of actions that this class supports and the description of each.
        /// </summary>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                var supportedActions = base.SupportedActions;
                supportedActions.AddOrReplace( Rock.Security.Authorization.APPROVE, "The roles and/or users that have access to approve notes." );
                return supportedActions;
            }
        }

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var noteType = entity as NoteType;
            if ( noteType == null )
                return;

            IsSystem = noteType.IsSystem;
            EntityTypeId = noteType.EntityTypeId;
            EntityTypeQualifierColumn = noteType.EntityTypeQualifierColumn;
            EntityTypeQualifierValue = noteType.EntityTypeQualifierValue;
            Name = noteType.Name;
            UserSelectable = noteType.UserSelectable;
            IconCssClass = noteType.IconCssClass;
            Order = noteType.Order;
            RequiresApprovals = noteType.RequiresApprovals;
            AllowsWatching = noteType.AllowsWatching;
            AllowsReplies = noteType.AllowsReplies;
            MaxReplyDepth = noteType.MaxReplyDepth;
            BackgroundColor = noteType.BackgroundColor;
            FontColor = noteType.FontColor;
            BorderColor = noteType.BorderColor;
            SendApprovalNotifications = noteType.SendApprovalNotifications;
            AutoWatchAuthors = noteType.AutoWatchAuthors;
            ApprovalUrlTemplate = noteType.ApprovalUrlTemplate;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Entity Note Types Cache

        /// <summary>
        /// Gets the by entity.
        /// </summary>
        /// <param name="entityTypeid">The entity typeid.</param>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <param name="includeNonSelectable">if set to <c>true</c> [include non selectable].</param>
        /// <returns></returns>
        public static List<NoteTypeCache> GetByEntity( int? entityTypeid, string entityTypeQualifierColumn, string entityTypeQualifierValue, bool includeNonSelectable = false )
        {
            var allEntityNoteTypes = EntityNoteTypesCache.Get();

            if ( allEntityNoteTypes == null )
                return new List<NoteTypeCache>();

            var matchingNoteTypeIds = allEntityNoteTypes.EntityNoteTypes
                .Where( a => a.EntityTypeId.Equals( entityTypeid ) )
                .ToList()
                .Where( a =>
                    ( a.EntityTypeQualifierColumn ?? string.Empty ) == ( entityTypeQualifierColumn ?? string.Empty ) &&
                    ( a.EntityTypeQualifierValue ?? string.Empty ) == ( entityTypeQualifierValue ?? string.Empty ) )
                .SelectMany( a => a.NoteTypeIds )
                .ToList();

            var noteTypes = new List<NoteTypeCache>();
            foreach ( var noteTypeId in matchingNoteTypeIds )
            {
                var noteType = Get( noteTypeId );
                if ( noteType != null && ( includeNonSelectable || noteType.UserSelectable ) )
                {
                    noteTypes.Add( noteType );
                }
            }

            return noteTypes;
        }

        /// <summary>
        /// Flushes the entity noteTypes.
        /// </summary>
        public static void RemoveEntityNoteTypes()
        {
            EntityNoteTypesCache.Remove();
        }

        #endregion
    }


}