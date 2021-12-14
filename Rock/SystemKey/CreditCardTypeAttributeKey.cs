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

namespace Rock.SystemKey
{
    /// <summary>
    /// Attribute keys for the <see cref="Rock.SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE">Credit Card Type</see>
    /// defined values.
    /// </summary>
    public static class CreditCardTypeAttributeKey
    {
        /// <summary>
        /// The regular expression pattern to use when evaluating a credit card
        /// number to see if it is of a particular type.
        /// </summary>
        public const string RegularExpressionPattern = "RegExPattern";

        /// <summary>
        /// When a financial transaction is processed by Rock, it will add the
        /// transaction to a new or existing batch with a specific name. This
        /// name can have a suffix defined specific to the type of credit card
        /// being used. This provides control over how specific credit card
        /// transactions are grouped into batches.
        /// </summary>
        public const string BatchNameSuffix = "BatchNameSuffix";

        /// <summary>
        /// When displaying payment information for this card type, this image
        /// may be used to provide a visual reference of the card. An SVG is
        /// strongly recommended.
        /// </summary>
        public const string IconImage = "core_IconImage";
    }
}
