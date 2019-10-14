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

using Rock.Extension;

namespace Rock.Address
{
    /// <summary>
    /// The base class for all address verification components
    /// </summary>
    public abstract class VerificationComponent : Component
    {
        /// <summary>
        /// Gets a value indicating whether verification component supports standardization.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [supports standardization]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool SupportsStandardization
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether verification component supports geocoding].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [supports geocoding]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool SupportsGeocoding
        {
            get { return true; }
        }

        /// <summary>
        /// Verifies the specified location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="resultMsg">The result MSG.</param>
        /// <returns></returns>
        public virtual VerificationResult Verify( Rock.Model.Location location, out string resultMsg )
        {
            throw new NotImplementedException();
        }

    }

    #region Enumerations

    /// <summary>
    /// The verification result
    /// </summary>
    [Flags]
    public enum VerificationResult
    {
        /// <summary>
        /// Location was not standardized or geocoded
        /// </summary>
        None = 0,

        /// <summary>
        /// Location was standardized
        /// </summary>
        Standardized = 1,

        /// <summary>
        /// Location was geocoded
        /// </summary>
        Geocoded = 2,

        /// <summary>
        /// An error occurred when trying to connect to verification service
        /// </summary>
        ConnectionError = 4,
    }

    #endregion

}
