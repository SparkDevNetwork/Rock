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
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a snapshot of an Attribute's value at a point in history (if Attribute.EnableHistory is enabled)
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "AttributeValueHistorical" )]
    [DataContract]
    public class AttributeValueHistorical : Model<AttributeValueHistorical>, IHistoricalTracking
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the AttributeValueId of the <see cref="Rock.Model.AttributeValue" /> that this AttributeValueHistorical provides a historical value for.
        /// </summary>
        /// <value>
        /// The attribute value identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int AttributeValueId { get; set; }

        /// <summary>
        /// Gets or sets the value of the AttributeValue at this point in history
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [DataMember]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the formatted value of the AttributeValue at this point in history
        /// </summary>
        /// <value>
        /// The value formatted.
        /// </value>
        [DataMember]
        public string ValueFormatted { get; set; }

        /// <summary>
        /// Gets or sets the value as numeric at this point in history
        /// </summary>
        /// <value>
        /// The value as numeric.
        /// </value>
        [DataMember]
        public decimal? ValueAsNumeric { get; set; }

        /// <summary>
        /// Gets or sets the value as date time at this point in history
        /// </summary>
        /// <value>
        /// The value as date time.
        /// </value>
        [DataMember]
        public DateTime? ValueAsDateTime { get; set; }

        /// <summary>
        /// Gets or sets the value as boolean at this point in history
        /// </summary>
        /// <value>
        /// The value as boolean.
        /// </value>
        [DataMember]
        public bool? ValueAsBoolean { get; set; }

        /// <summary>
        /// Gets or sets the value as person identifier.
        /// </summary>
        /// <value>
        /// The value as person identifier.
        /// </value>
        [DataMember]
        public int? ValueAsPersonId { get; set; }

        #endregion

        #region IHistoricalTracking

        /// <summary>
        /// Gets or sets the effective date.
        /// This is the starting date that the tracked record had the values reflected in this record
        /// </summary>
        /// <value>
        /// The effective date.
        /// </value>
        [DataMember]
        public DateTime EffectiveDateTime { get; set; }

        /// <summary>
        /// Gets or sets the expire date time
        /// This is the last date that the tracked record had the values reflected in this record
        /// For example, if a tracked record's Name property changed on '2016-07-14', the ExpireDate of the previously current record will be '2016-07-13', and the EffectiveDate of the current record will be '2016-07-14'
        /// If this is most current record, the ExpireDate will be '9999-01-01'
        /// </summary>
        /// <value>
        /// The expire date.
        /// </value>
        [DataMember]
        public DateTime ExpireDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [current row indicator].
        /// This will be True if this represents the same values as the current tracked record for this
        /// </summary>
        /// <value>
        ///   <c>true</c> if [current row indicator]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool CurrentRowIndicator { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the AttributeValue <see cref="Rock.Model.AttributeValue" /> that this AttributeValueHistorical provides a historical value for.
        /// </summary>
        /// <value>
        /// The attribute value.
        /// </value>
        [DataMember]
        public virtual AttributeValue AttributeValue { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates an AttributeValueHistory with CurrentRowIndicator = true for the specified attributeValue
        /// </summary>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="effectiveDateTime">The effective date time.</param>
        /// <returns></returns>
        public static AttributeValueHistorical CreateCurrentRowFromAttributeValue( AttributeValue attributeValue, DateTime effectiveDateTime )
        {
            var attributeCache = AttributeCache.Get( attributeValue.AttributeId );
            string formattedValue = attributeCache.FieldType.Field.FormatValue( null, attributeValue.Value, attributeCache.QualifierValues, true );
            var attributeValueHistoricalCurrent = new AttributeValueHistorical
            {
                AttributeValueId = attributeValue.Id,
                Value = attributeValue.Value,
                ValueFormatted = formattedValue,
                ValueAsNumeric = attributeValue.ValueAsNumeric,
                ValueAsDateTime = attributeValue.ValueAsDateTime,
                ValueAsBoolean = attributeValue.ValueAsBoolean,
                ValueAsPersonId = attributeValue.ValueAsPersonId,
                CurrentRowIndicator = true,
                EffectiveDateTime = effectiveDateTime,
                ExpireDateTime = HistoricalTracking.MaxExpireDateTime
            };

            return attributeValueHistoricalCurrent;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class AttributeValueHistoricalConfiguration : EntityTypeConfiguration<AttributeValueHistorical>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValueHistoricalConfiguration"/> class.
        /// </summary>
        public AttributeValueHistoricalConfiguration()
        {
            this.HasRequired( t => t.AttributeValue ).WithMany( t => t.AttributeValuesHistorical ).HasForeignKey( t => t.AttributeValueId );
        }
    }

    #endregion
}
