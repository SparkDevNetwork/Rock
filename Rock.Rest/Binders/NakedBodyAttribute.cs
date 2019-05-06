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
using System.Web.Http.Controllers;

using Westwind.Web.WebApi;

namespace System.Web.Http
{
    /// <summary>
    /// An attribute that captures the entire content body and stores it
    /// into the parameter of type string or byte[].
    /// </summary>
    /// <remarks>
    /// The parameter marked up with this attribute should be the only parameter as it reads the
    /// entire request body and assigns it to that parameter.    
    /// </remarks>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true )]
    public sealed class NakedBodyAttribute : ParameterBindingAttribute
    {
        /// <summary>
        /// Gets the parameter binding.
        /// </summary>
        /// <param name="parameter">The parameter description.</param>
        /// <returns>
        /// The parameter binding.
        /// </returns>
        /// <exception cref="System.ArgumentException">Invalid parameter</exception>
        public override HttpParameterBinding GetBinding( HttpParameterDescriptor parameter )
        {
            if ( parameter == null )
                throw new ArgumentException( "Invalid parameter" );

            return new NakedBodyParameterBinding( parameter );
        }
    }
}