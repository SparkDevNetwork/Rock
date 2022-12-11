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
    /// This is determined by the values for EntityTypeQualifierValue and EntityTypeQualifierValuePrevious
    /// </summary>
    [Enums.EnumDomain( "Workflow" )]
    public enum WorkflowTriggerValueChangeType
    {
        /// <summary>
        /// EntityTypeQualifierValue and EntityTypeQualifierValuePrevious are different, so we are looking to see if the value Changed From/To
        /// </summary>
        ChangeFromTo = 0,

        /// <summary>
        /// EntityTypeQualifierValue and EntityTypeQualifierValuePrevious are the same value, so the trigger is simply looking to see if the value is equal to the specified value
        /// </summary>
        ValueEqual = 1
    }
}
