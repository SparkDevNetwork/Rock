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

namespace Rock.Field
{
    /// <summary>
    /// Designates an <see cref="IFieldType"/> as supporting a method that will
    /// split multiple values into a collection of single values.
    /// </summary>
    public interface ISplitMultiValueFieldType
    {
        /// <summary>
        /// <para>
        ///     Splits the private (database) value of a field into a collection
        ///     of individual values.
        /// </para>
        /// <para>
        ///     For instance, if the field type supports multiple values and
        ///     stores them as a comma separated guid list, then this method
        ///     would split by comma and return the individual values.
        /// </para>
        /// <para>
        ///     Values returned by this method should be in a format that can
        ///     still be sent to the various value formatting methods.
        /// </para>
        /// </summary>
        /// <param name="privateValue">The private database value.</param>
        /// <returns>A collection of strings that represent individual values.</returns>
        ICollection<string> SplitMultipleValues( string privateValue );
    }
}
