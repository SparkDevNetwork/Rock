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
namespace Rock.Mobile.JsonFields
{
    /// <summary>
    /// Determines the field's format. This will be used to properly format the Json sent to the client.
    /// This is copied to Rock.Enums/Controls/FieldFormat.cs. If any changes are made here,
    /// they may need to be copied there as well.
    /// </summary>
    public enum FieldFormat
    {
        /// <summary>
        /// The value will be formatted as a string.
        /// </summary>
        String = 0,

        /// <summary>
        /// The value will be formatted as a number.
        /// </summary>
        Number = 1,

        /// <summary>
        /// The value will be formatted as a datetime.
        /// </summary>
        Date = 2,

        /// <summary>
        /// The value will be formatted as a boolean.
        /// </summary>
        Boolean = 3
    }

}
