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

namespace Rock
{
    /// <summary>
    /// Defined Value Extensions
    /// </summary>
    public static class DefinedValueExtensions
    {
        #region Int Extensions

        /// <summary>
        /// Gets the Defined Value name associated with this id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static string DefinedValue( this int? id )
        {
            if ( !id.HasValue )
                return string.Empty;

            var definedValue = Rock.Web.Cache.DefinedValueCache.Read( id.Value );
            if ( definedValue != null )
                return definedValue.Value;
            else
                return string.Empty;
        }

        #endregion Int Extensions
    }
}
