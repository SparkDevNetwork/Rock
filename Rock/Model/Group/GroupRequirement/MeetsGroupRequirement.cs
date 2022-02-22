﻿// <copyright>
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
    ///
    /// </summary>
    public enum MeetsGroupRequirement
    {
        /// <summary>
        ///  Meets requirements
        /// </summary>
        Meets,

        /// <summary>
        /// Doesn't meet requirements
        /// </summary>
        NotMet,

        /// <summary>
        /// The meets with warning
        /// </summary>
        MeetsWithWarning,

        /// <summary>
        /// The Requirement doesn't apply for the GroupRole we are checking against
        /// </summary>
        NotApplicable,

        /// <summary>
        /// The Requirement calculation resulted in an exception <see cref="GroupRequirementStatus.CalculationException"/>
        /// </summary>
        Error
    }
}
