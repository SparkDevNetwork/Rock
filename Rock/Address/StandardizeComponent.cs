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
    /// The base class for all address standardization components
    /// </summary>
    public abstract class StandardizeComponent : Component
    {
        /// <summary>
        /// Abstract method for standardizing the specified address.  Derived classes should implement
        /// this method to standardize the address.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="result">The result code unique to the service.</param>
        /// <returns>
        /// True/False value of whether the address was standardized succesfully
        /// </returns>
        public abstract bool Standardize( Rock.Model.Location location, out string result );
    }
}
