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

using System.Linq.Expressions;

namespace Rock.Data
{
    /// <summary>
    /// Defines a single ordering criterion for cursor-based pagination.
    /// </summary>
    internal sealed class CursorOrderInfo
    {
        /// <summary>
        /// The lambda expression that selects the key to be used for ordering
        /// and cursor generation.
        /// </summary>
        public LambdaExpression KeySelector { get; set; }

        /// <summary>
        /// Determines if the ordering for this key is descending instead of
        /// ascending.
        /// </summary>
        public bool Descending { get; set; }

        /// <summary>
        /// The full property path represented by the key selector.
        /// </summary>
        public string PropertyPath { get; set; }
    }
}
