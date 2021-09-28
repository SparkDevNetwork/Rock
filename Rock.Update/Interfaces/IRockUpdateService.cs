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

using System;
using System.Collections.Generic;
using Rock.Update.Models;

namespace Rock.Update.Interfaces
{
    /// <summary>
    /// This interface is implemented by RockUpdateService and is used so we can mock the service for testing.
    /// </summary>
    public interface IRockUpdateService
    {
        /// <summary>
        /// Gets the releases list.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <returns></returns>
        List<RockRelease> GetReleasesList( Version version );

        /// <summary>
        /// Gets the rock early access request URL.
        /// </summary>
        /// <returns></returns>
        string GetRockEarlyAccessRequestUrl();

        /// <summary>
        /// Gets the rock release program.
        /// </summary>
        /// <returns></returns>
        RockReleaseProgram GetRockReleaseProgram();

        /// <summary>
        /// Determines whether [is early access instance].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is early access instance]; otherwise, <c>false</c>.
        /// </returns>
        bool IsEarlyAccessInstance();
    }
}