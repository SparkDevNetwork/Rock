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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rock.CheckIn.v2.Labels
{
    /// <summary>
    /// Represents a data source for a multi-value field whose label data
    /// is expected to be an instance of <typeparamref name="TLabelData"/>.
    /// </summary>
    /// <typeparam name="TLabelData">The type of label data expected.</typeparam>
    internal class MultiValueFieldDataSource<TLabelData> : FieldDataSource
    {
        /// <summary>
        /// The function that will get the list of values from the label data.
        /// </summary>
        public Func<TLabelData, LabelField, PrintLabelRequest, IEnumerable> ValuesFunc { get; set; }

        /// <inheritdoc/>
        public sealed override bool IsCollection => true;

        /// <inheritdoc/>
        public override List<object> GetValues( LabelField field, PrintLabelRequest printRequest )
        {
            var values = ValuesFunc?.Invoke( ( TLabelData ) printRequest.LabelData, field, printRequest );

            if ( values == null )
            {
                return new List<object>();
            }

            return values.Cast<object>().ToList();
        }
    }
}
