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
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class AttributeValueHistorical
    {
        #region Methods

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
}
