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
namespace Rock.Data
{
    /// <summary>
    /// Arguments object for <see cref="DbContext.SaveChanges(SaveChangesArgs)"/>
    /// </summary>
    public sealed class SaveChangesArgs
    {
        /// <summary>
        /// if set to <c>true</c> disables
        /// the Pre and Post processing from being run. This should only be disabled
        /// when updating a large number of records at a time (e.g. importing records).
        /// </summary>
        public bool DisablePrePostProcessing { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether this instance checks for earned achievements on save.
        /// True by default.
        /// Set to false for faster performance.
        /// If <see cref="DisablePrePostProcessing"/> is true, then achievements are disabled no matter what this value is.
        /// </summary>
        public bool IsAchievementsEnabled { get; set; } = true;
    }
}
