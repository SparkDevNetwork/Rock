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
using System;
using System.ComponentModel.DataAnnotations;

namespace Rock.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class NotEmptyGuidAttribute : ValidationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotEmptyGuidAttribute"/> class.
        /// </summary>
        public NotEmptyGuidAttribute()
            : base( "Guid cannot be empty" )
        {
        }

        /// <summary>
        /// Determines whether the specified value of the object is valid.
        /// </summary>
        /// <param name="value">The value of the object to validate.</param>
        /// <returns>
        /// true if the specified value is valid; otherwise, false.
        /// </returns>
        public override bool IsValid( object value )
        {
            if ( value != null && value is Guid )
            {
                return (Guid)value != Guid.Empty;
            }
            else
            {
                return false;
            }
        }
    }
}
