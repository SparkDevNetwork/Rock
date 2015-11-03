// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using Rock.Extension;

namespace Rock.Address
{
    /// <summary>
    /// The base class for all address verification components
    /// </summary>
    public abstract class VerificationComponent : Component
    {
        /// <summary>
        /// Abstract method for verifying a location.  Derived classes should implement
        /// this method to perform an verification action on an address (i.e. standardize, geocode, etc.).
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="reVerify">Should location be reverified even if it has already been successfully verified</param>
        /// <param name="result">The result code unique to the service.</param>
        /// <returns>
        /// True/False value of whether the verification was successfull or not
        /// </returns>
        public abstract bool VerifyLocation( Rock.Model.Location location, bool reVerify, out string result );
    }

}
