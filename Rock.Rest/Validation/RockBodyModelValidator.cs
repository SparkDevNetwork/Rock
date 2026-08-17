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
using System.Web.Http.Validation;

namespace Rock.Rest.Validation
{
    /// <summary>
    /// Recursively validates a body-bound object, skipping types that are
    /// resolved server-side rather than bound from the request. Those types
    /// carry no request input to validate, and walking them is expensive.
    /// </summary>
    public class RockBodyModelValidator : DefaultBodyModelValidator
    {
        /// <inheritdoc/>
        public override bool ShouldValidateType( Type type )
        {
            // These are resolved server-side and never populated from the request body, so validating
            // them produces no meaningful ModelState errors, and recursing into them is unboundedly
            // expensive (it reaches live singletons and reflection metadata that hash whole assemblies).
            if ( typeof( Rock.Extension.Component ).IsAssignableFrom( type )
                || typeof( Rock.Web.Cache.IEntityCache ).IsAssignableFrom( type )
                || typeof( System.Reflection.MemberInfo ).IsAssignableFrom( type )
                || typeof( System.Reflection.Module ).IsAssignableFrom( type )
                || typeof( System.Reflection.Assembly ).IsAssignableFrom( type ) )
            {
                return false;
            }

            return base.ShouldValidateType( type );
        }
    }
}
