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

namespace Rock.Enums.Core.Automation.Triggers
{
    /// <summary>
    /// The type of change that will trigger the criteria to match.
    /// </summary>
    public enum EntityChangeSimpleChangeType
    {
        /// <summary>
        /// The criteria will match if the specified property has changed its
        /// value in any way. The OriginalValue and UpdatedValue properties
        /// are ignored.
        /// </summary>
        AnyChange = 0,

        /// <summary>
        /// The critieria rull match if the old value or the new value are
        /// equal to the UpdatedValue property. The OriginalValue property is
        /// ignored. This does not require that the value has changed.
        /// </summary>
        HasSpecificValue = 1,

        /// <summary>
        /// The criteria will match if the old value is equal to the OriginalValue
        /// property and the property has been changed to a different value. The
        /// UpdatedValue property is ignored.
        /// </summary>
        ChangedFromValue = 2,

        /// <summary>
        /// The criteria will match if the new value is equal to the UpdatedValue
        /// property and the property has been changed from a different value. The
        /// OriginalValue property is ignored.
        /// </summary>
        ChangedToValue = 3,

        /// <summary>
        /// The critieria will match if the old value is equal to the OriginalValue
        /// property and the new/current value is equal to the UpdatedValue
        /// property. This requires that the property value has actually been
        /// changed.
        /// </summary>
        ChangedFromValueToValue = 4,
    }
}
