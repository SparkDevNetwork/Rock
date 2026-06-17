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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Represents a daily, per-<see cref="InteractionComponent"/> aggregate of interaction
    /// counts and session counts. Rows are populated by the Rock Cleanup job for channels
    /// where <see cref="InteractionChannel.EnableComponentDailyCounts"/> is <c>true</c>.
    /// <para>
    /// This is a lightweight aggregate; it does not inherit from <see cref="Model{T}"/> or
    /// <see cref="Entity{T}"/>. It has no surrogate <c>Id</c> and no audit columns. The
    /// natural key is the composite (<see cref="InteractionComponentId"/>,
    /// <see cref="InteractionDate"/>, <see cref="Operation"/>).
    /// </para>
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "InteractionComponentDailyCount" )]
    [DataContract]
    [NotAudited]
    public partial class InteractionComponentDailyCount
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.InteractionComponent"/> that
        /// this daily aggregate row belongs to. First column of the composite primary key.
        /// </summary>
        /// <value>
        /// The <c>Id</c> of the related <see cref="Rock.Model.InteractionComponent"/>
        /// </value>
        [Key]
        [Column( Order = 0 )]
        [DataMember]
        public int InteractionComponentId { get; set; }

        /// <summary>
        /// Gets or sets the date this aggregate row represents. Stored as a SQL <c>date</c>
        /// (no time component). Second column of the composite primary key.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> with the time portion truncated to midnight.
        /// </value>
        [Key]
        [Column( Order = 1, TypeName = "date" )]
        [DataMember]
        public DateTime InteractionDate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Interaction.Operation"/> value this aggregate row
        /// represents. An empty string is used when the source <c>Operation</c> is null,
        /// because <c>Operation</c> is part of the composite primary key and cannot be null.
        /// Third column of the composite primary key.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> matching <see cref="Interaction.Operation"/>, or
        /// the empty string when the source value was null.
        /// </value>
        [Key]
        [Column( Order = 2 )]
        [MaxLength( 25 )]
        [DataMember]
        public string Operation { get; set; }

        /// <summary>
        /// Gets or sets the date this aggregate row represents in <c>YYYYMMDD</c> integer
        /// form, mirroring <see cref="Interaction.InteractionDateKey"/>. Provides a fast
        /// integer-indexed lookup path for reporting consumers.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> in <c>YYYYMMDD</c> format (for example,
        /// <c>20260615</c> for June 15, 2026).
        /// </value>
        [DataMember]
        public int InteractionDateKey { get; set; }

        /// <summary>
        /// Gets or sets the count of interactions on this (component, date, operation)
        /// whose <see cref="Interaction.PersonAliasId"/> references a person other than
        /// the Anonymous Visitor.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> count of "logged in" interactions.
        /// </value>
        [DataMember]
        public int LoggedInInteractionCount { get; set; }

        /// <summary>
        /// Gets or sets the count of interactions on this (component, date, operation)
        /// whose <see cref="Interaction.PersonAliasId"/> is null or references the
        /// Anonymous Visitor person.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> count of anonymous interactions.
        /// </value>
        [DataMember]
        public int AnonymousInteractionCount { get; set; }

        /// <summary>
        /// Gets or sets the count of distinct <see cref="Interaction.InteractionSessionId"/>
        /// values on this (component, date, operation) from "logged in" interactions.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> count of distinct sessions belonging to non-anonymous persons.
        /// </value>
        [DataMember]
        public int LoggedInSessionCount { get; set; }

        /// <summary>
        /// Gets or sets the count of distinct <see cref="Interaction.InteractionSessionId"/>
        /// values on this (component, date, operation) from anonymous interactions.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> count of distinct sessions belonging to anonymous traffic.
        /// </value>
        [DataMember]
        public int AnonymousSessionCount { get; set; }

        /// <summary>
        /// Gets or sets the total interaction count, equal to
        /// <see cref="LoggedInInteractionCount"/> + <see cref="AnonymousInteractionCount"/>.
        /// Stored at write time so consumers do not need to recompute it.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> total interaction count.
        /// </value>
        [DataMember]
        public int TotalInteractionCount { get; set; }

        /// <summary>
        /// Gets or sets the total session count, equal to
        /// <see cref="LoggedInSessionCount"/> + <see cref="AnonymousSessionCount"/>.
        /// Stored at write time so consumers do not need to recompute it.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> total session count.
        /// </value>
        [DataMember]
        public int TotalSessionCount { get; set; }

        /// <summary>
        /// Gets or sets the average <see cref="Interaction.InteractionLength"/> for this
        /// (component, date, operation). Units vary by channel (seconds, minutes, percent
        /// watched, etc.); semantics match <see cref="Interaction.InteractionLength"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal"/> average interaction length, or null when no source
        /// interactions had a non-null <see cref="Interaction.InteractionLength"/>.
        /// </value>
        [DataMember]
        public decimal? AverageInteractionLength { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.InteractionComponent"/> that this daily
        /// aggregate row belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.InteractionComponent"/> referenced by
        /// <see cref="InteractionComponentId"/>.
        /// </value>
        [LavaVisible]
        public virtual InteractionComponent InteractionComponent { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// InteractionComponentDailyCount Configuration class.
    /// </summary>
    public partial class InteractionComponentDailyCountConfiguration : EntityTypeConfiguration<InteractionComponentDailyCount>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionComponentDailyCountConfiguration"/> class.
        /// </summary>
        public InteractionComponentDailyCountConfiguration()
        {
            /*
                6/15/26 - NA

                Cascade delete is intentional here. Daily-count rows are a derived
                aggregate that is meaningless without the parent InteractionComponent,
                so deleting a component should remove its rolled-up rows. This is the
                ownership exception called out in .claude/rules/data-model.md.

                Reason: Aggregate rows are owned by their component.
            */
            this.HasRequired( m => m.InteractionComponent )
                .WithMany()
                .HasForeignKey( m => m.InteractionComponentId )
                .WillCascadeOnDelete( true );
        }
    }

    #endregion Entity Configuration
}
