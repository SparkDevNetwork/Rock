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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "Group" )]
    [Table( "GroupRequirementType" )]
    [DataContract]
    public partial class GroupRequirementType : Model<GroupRequirementType>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this requirement can expire.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can expire; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember]
        public bool CanExpire { get; set; }

        /// <summary>
        /// Gets or sets the number of days after the requirement is met before it expires (If CanExpire is true). NULL means never expires
        /// </summary>
        /// <value>
        /// The expire in days.
        /// </value>
        [DataMember]
        public int? ExpireInDays { get; set; }

        /// <summary>
        /// Gets or sets the type of the requirement check.
        /// </summary>
        /// <value>
        /// The type of the requirement check.
        /// </value>
        [DataMember]
        public RequirementCheckType RequirementCheckType { get; set; }

        /// <summary>
        /// Gets or sets the SQL expression.
        /// </summary>
        /// <value>
        /// The SQL expression.
        /// </value>
        [DataMember]
        public string SqlExpression { get; set; }

        /// <summary>
        /// Gets or sets the data view identifier.
        /// </summary>
        /// <value>
        /// The data view identifier.
        /// </value>
        [DataMember]
        public int? DataViewId { get; set; }

        /// <summary>
        /// Gets or sets the warning SQL expression.
        /// </summary>
        /// <value>
        /// The warning SQL expression.
        /// </value>
        [DataMember]
        public string WarningSqlExpression { get; set; }

        /// <summary>
        /// Gets or sets the warning data view identifier.
        /// </summary>
        /// <value>
        /// The warning data view identifier.
        /// </value>
        [DataMember]
        public int? WarningDataViewId { get; set; }

        /// <summary>
        /// Gets or sets the positive label. This is the text that is displayed when the requirement is met.
        /// </summary>
        /// <value>
        /// The positive label.
        /// </value>
        [MaxLength( 150 )]
        [DataMember]
        public string PositiveLabel { get; set; }

        /// <summary>
        /// Gets or sets the negative label. This is the text that is displayed when the requirement is not met.
        /// </summary>
        /// <value>
        /// The negative label.
        /// </value>
        [MaxLength( 150 )]
        [DataMember]
        public string NegativeLabel { get; set; }

        /// <summary>
        /// Gets or sets the warning label.
        /// </summary>
        /// <value>
        /// The warning label.
        /// </value>
        [DataMember]
        public string WarningLabel { get; set; }

        /// <summary>
        /// Gets or sets the checkbox label. This is the text that is used for the checkbox if this is a manually set requirement
        /// </summary>
        /// <value>
        /// The checkbox label.
        /// </value>
        [MaxLength( 150 )]
        [DataMember]
        public string CheckboxLabel { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the data view.
        /// </summary>
        /// <value>
        /// The data view.
        /// </value>
        [LavaInclude]
        public virtual DataView DataView { get; set; }

        /// <summary>
        /// Gets or sets the warning data view.
        /// </summary>
        /// <value>
        /// The warning data view.
        /// </value>
        [LavaInclude]
        public virtual DataView WarningDataView { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the merge objects that can be used in the SQL Expression
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        public Dictionary<string, object> GetMergeObjects( Group group )
        {
            Dictionary<string, object> mergeObjects = new Dictionary<string, object>();
            mergeObjects.Add( "Group", group );
            mergeObjects.Add( "GroupRequirementType", this );

            return mergeObjects;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }

    #region Enum

    /// <summary>
    /// The type of requirement check that is done. Sql, DataView or Manual.
    /// </summary>
    public enum RequirementCheckType
    {
        /// <summary>
        /// SQL
        /// </summary>
        Sql = 0,

        /// <summary>
        /// A dataview
        /// </summary>
        Dataview = 1,

        /// <summary>
        /// Manual
        /// </summary>
        Manual = 2
    }

    #endregion

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class GroupRequirementTypeConfiguration : EntityTypeConfiguration<GroupRequirementType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupRequirementTypeConfiguration"/> class.
        /// </summary>
        public GroupRequirementTypeConfiguration()
        {
            this.HasOptional( a => a.DataView ).WithMany().HasForeignKey( a => a.DataViewId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.WarningDataView ).WithMany().HasForeignKey( a => a.WarningDataViewId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
