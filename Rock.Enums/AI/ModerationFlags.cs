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

namespace Rock.Enums.AI
{
    /// <summary>
    /// Moderation categories
    /// </summary>
    [Flags]
    public enum ModerationFlags
    {
        /// <summary>
        /// No moderation categories set.
        /// </summary>
        None = 0,
        /// <summary>
        /// Moderation for hate.
        /// </summary>
        Hate = 1,
        /// <summary>
        /// Moderation for threats.
        /// </summary>
        Threat = 2,
        /// <summary>
        /// Moderation for self harm.
        /// </summary>
        SelfHarm = 4,
        /// <summary>
        /// Moderation for sexual content.
        /// </summary>
        Sexual = 8,
        /// <summary>
        /// Moderation for sexual content with minors.
        /// </summary>
        SexualMinor = 16,
        /// <summary>
        /// Moderation for violence.
        /// </summary>
        Violent = 32
    }
}
