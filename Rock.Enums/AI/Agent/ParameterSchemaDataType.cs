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

namespace Rock.Enums.AI.Agent
{
    /// <summary>
    /// The data type of a parameter to use in a JSON schema.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    public enum ParameterSchemaDataType
    {
        /// <summary>
        /// The parameter is defined as a string type.
        /// </summary>
        String = 0,

        /// <summary>
        /// The parameter is defined as a numeric type. This will be represented
        /// as a double-precision value in Lava.
        /// </summary>
        Number = 1,

        /// <summary>
        /// The parameter is defined as a boolean type.
        /// </summary>
        Boolean = 2,

        /// <summary>
        /// The parameter is defined as a Date type. This will be represented
        /// as a DateTime value in Lava if possible, otherwise as a string.
        /// </summary>
        Date = 3,

        /// <summary>
        /// The parameter is defined as a DateTime type. This will be represented
        /// as a DateTime value in Lava if possible, otherwise as a string.
        /// </summary>
        DateTime = 4,
    }
}
