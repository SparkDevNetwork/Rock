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
using System.Linq;

using Rock.ViewModels.Utility;

namespace Rock.Blocks
{
    public static class IValidPropertiesBoxExtensions
    {
        public static bool IsValidProperty( this IValidPropertiesBox box, string propertyName )
        {
            return box.ValidProperties.Contains( propertyName, StringComparer.OrdinalIgnoreCase );
        }

        public static void IfValidProperty( this IValidPropertiesBox box, string propertyName, Action executeIfValid )
        {
            if ( IsValidProperty( box, propertyName ) )
            {
                executeIfValid();
            }
        }

        public static TReturn IfValidProperty<TReturn>( this IValidPropertiesBox box, string propertyName, Func<TReturn> executeIfValid, TReturn defaultValue )
        {
            if ( IsValidProperty( box, propertyName ) )
            {
                return executeIfValid();
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
