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
    /// Stores the values for a group and demographic type
    /// </summary>
    [RockDomain( "Group" )]
    [Table( "GroupDemographicValue" )]
    [DataContract]
    public partial class GroupDemographicValue : Model<GroupDemographicValue>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Group ID that this GroupDemographicValue is for.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the GroupDemographicType ID that this GroupDemographicValue is for.
        /// </summary>
        /// <value>
        /// The group demographic type identifier.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
        public int GroupDemographicTypeId { get; set; }

        /// <summary>
        /// Gets or sets the related EntityTypeID this value if for. e.g. DefinedValue.
        /// </summary>
        /// <value>
        /// The related entity type identifier.
        /// </value>
        [DataMember]
        public int? RelatedEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the related entity identifier. e.g. the ID of the DefinedValue
        /// </summary>
        /// <value>
        /// The related entity identifier.
        /// </value>
        [DataMember]
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [DataMember]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the last date and time when this GroupDemographicValue was calculated.
        /// </summary>
        /// <value>
        /// The last calculated date time.
        /// </value>
        [DataMember]
        public DateTime? LastCalculatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the value as GUID.
        /// </summary>
        /// <value>
        /// The value as unique identifier.
        /// </value>
        [DataMember]
        [LavaHidden]
        public Guid? ValueAsGuid
        {
            get
            {
                // Just set to null if value is null, white space, or length is not a GUID length (has too many char if dashes included or too little chars if dashes not included)
                if ( this.Value.IsNullOrWhiteSpace() || this.Value.Trim().Length > 36 || this.Value.Trim().Length < 32 )
                {
                    _valueAsGuid = null;
                    return _valueAsGuid;
                }

                // Try to convert the value string to a GUID. Extension method uses Guid.TryParse()
                _valueAsGuid = Value.AsGuidOrNull();
                return _valueAsGuid;
            }

            set
            {
                _valueAsGuid = value;
            }

        }

        private Guid? _valueAsGuid;

        /// <summary>
        /// Gets or sets the value as numeric.
        /// </summary>
        /// <value>
        /// The value as numeric.
        /// </value>
        [DataMember]
        [LavaHidden]
        public decimal? ValueAsNumeric
        {
            get
            {
                // since this will get called on every save, don't spend time attempting to convert a large string to a decimal.
                // SQL Server type is decimal(18,2) so 18 digits max with 2 being the fractional. Including the possibility of 4 commas
                // and a decimal point to get a max string length of 24 that can be turned into the SQL number type.
                if ( this.Value.IsNull() || this.Value.Length > 24 )
                {
                    _valueAsNumeric = null;
                    return _valueAsNumeric;
                }

                _valueAsNumeric = this.Value.AsDecimalOrNull();

                // If this is true then we are probably dealing with a comma delimited list and not a number.
                // In either case it won't save to the DB and needs to be handled.  Don't do the
                // rounding trick since nonnumeric attribute values should be null here.
                if ( _valueAsNumeric != null && _valueAsNumeric > ( decimal ) 9999999999999999.99 )
                {
                    _valueAsNumeric = null;
                }

                return _valueAsNumeric;
            }

            set
            {
                _valueAsNumeric = value;
            }
        }

        private decimal? _valueAsNumeric;

        /// <summary>
        /// Gets or sets the value as boolean.
        /// </summary>
        /// <value>
        /// The value as boolean.
        /// </value>
        [DataMember]
        [LavaHidden]
        public bool? ValueAsBoolean
        {
            get
            {
                if ( Value.IsNullOrWhiteSpace() )
                {
                    _valueAsBoolean = null;
                }
                else
                {
                    _valueAsBoolean = Value.AsBooleanOrNull();
                }

                return _valueAsBoolean;
            }

            set
            {
                _valueAsBoolean = value;
            }
        }

        private bool? _valueAsBoolean;

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [DataMember]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the type of the group demographic.
        /// </summary>
        /// <value>
        /// The type of the group demographic.
        /// </value>
        [DataMember]
        public virtual GroupDemographicType GroupDemographicType { get; set; }

        /// <summary>
        /// Gets or sets the type of the related entity.
        /// </summary>
        /// <value>
        /// The type of the related entity.
        /// </value>
        [DataMember]
        public virtual EntityType RelatedEntityType { get; set; }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class GroupDemographicValueConfiguration : EntityTypeConfiguration<GroupDemographicValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupDemographicValueConfiguration"/> class.
        /// </summary>
        public GroupDemographicValueConfiguration()
        {
            this.HasRequired( x => x.Group ).WithMany().HasForeignKey( x => x.GroupId ).WillCascadeOnDelete( true );
            this.HasRequired( x => x.GroupDemographicType ).WithMany().HasForeignKey( x => x.GroupDemographicTypeId ).WillCascadeOnDelete( true );
            this.HasOptional( x => x.RelatedEntityType ).WithMany().HasForeignKey( x => x.RelatedEntityTypeId ).WillCascadeOnDelete( false );
        }
    }

}
