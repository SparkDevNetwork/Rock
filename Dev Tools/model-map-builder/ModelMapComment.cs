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

namespace Rock.ModelMapBuilder
{
    /// <summary>
    /// The XML documentation comment sections for a model, property, or method
    /// (as HTML). Empty sections are omitted from the output.
    /// </summary>
    internal class ModelMapComment
    {
        /// <summary>
        /// Gets or sets the summary section.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the value section.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the remarks section.
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// Gets or sets the returns section.
        /// </summary>
        public string Returns { get; set; }

        /// <summary>
        /// Gets or sets the example section.
        /// </summary>
        public string Example { get; set; }

        /// <summary>
        /// Gets a value indicating whether every section is empty.
        /// </summary>
        public bool IsEmpty => Summary.IsNullOrWhiteSpace()
            && Value.IsNullOrWhiteSpace()
            && Remarks.IsNullOrWhiteSpace()
            && Returns.IsNullOrWhiteSpace()
            && Example.IsNullOrWhiteSpace();

        /// <summary>
        /// Determines whether <see cref="Summary"/> should be serialized.
        /// </summary>
        public bool ShouldSerializeSummary()
        {
            return Summary.IsNotNullOrWhiteSpace();
        }

        /// <summary>
        /// Determines whether <see cref="Value"/> should be serialized.
        /// </summary>
        public bool ShouldSerializeValue()
        {
            return Value.IsNotNullOrWhiteSpace();
        }

        /// <summary>
        /// Determines whether <see cref="Remarks"/> should be serialized.
        /// </summary>
        public bool ShouldSerializeRemarks()
        {
            return Remarks.IsNotNullOrWhiteSpace();
        }

        /// <summary>
        /// Determines whether <see cref="Returns"/> should be serialized.
        /// </summary>
        public bool ShouldSerializeReturns()
        {
            return Returns.IsNotNullOrWhiteSpace();
        }

        /// <summary>
        /// Determines whether <see cref="Example"/> should be serialized.
        /// </summary>
        public bool ShouldSerializeExample()
        {
            return Example.IsNotNullOrWhiteSpace();
        }
    }
}
