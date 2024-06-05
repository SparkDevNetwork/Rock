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

namespace Rock.CheckIn.v2.Labels.Formatters
{
    /// <summary>
    /// A data formatter that uses a callback function to perform the conversion
    /// of each value.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    internal class CallbackDataFormatter<TValue> : DataFormatter<TValue>
    {
        /// <summary>
        /// The function to call when formatting a single value.
        /// </summary>
        public Func<TValue, string, PrintLabelRequest, string> FormatValueFunc { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="CallbackDataFormatter{TValue}"/>.
        /// </summary>
        /// <param name="formatValueFunc">The function used to format each value.</param>
        public CallbackDataFormatter( Func<TValue, string, PrintLabelRequest, string> formatValueFunc )
        {
            FormatValueFunc = formatValueFunc;
        }

        /// <inheritdoc/>
        protected override string GetFormattedValue( TValue value, string optionValue, LabelField field, PrintLabelRequest printRequest )
        {
            if ( FormatValueFunc == null )
            {
                return value.ToStringSafe();
            }

            return FormatValueFunc( value, optionValue, printRequest );
        }
    }
}
