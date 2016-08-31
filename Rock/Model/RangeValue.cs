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
namespace Rock.Model
{
    /// <summary>
    /// Represents a Range of objects in Rock.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RangeValue<T>
    {

        /// <summary>
        /// Initializes a new instanceof the <see cref="RangeValue{T}"/> class.
        /// </summary> 
        /// <param name="from">The From/minimum value of the range.</param>
        /// <param name="to">The To/maximum value of the range.</param>
        public RangeValue(T from, T to)
        {
            this.From = from;
            this.To = to;
        }

        /// <summary>
        /// Gets or sets the From/mininimum value of the range.
        /// </summary>
        /// <value>
        /// The from/minimum value of the range.
        /// </value>
        public T From { get; set; }

        /// <summary>
        /// Gets or sets To/maximum value of the range.
        /// </summary>
        /// <value>
        /// The to/maximum value of the range.
        /// </value>
        public T To { get; set; }
    }
}