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
namespace Rock.Lava
{
    /// <summary>
    /// Extension methods for Lava Exception objects.
    /// </summary>
    public static class LavaExceptionExtensions
    {
        /// <summary>
        /// Sets a parameter for the target Exception.
        /// </summary>
        /// <param name="exception">The input exception to be modified.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="parameterValue">The parameter value.</param>
        /// <returns></returns>
        public static LavaElementRenderException WithParameter( this LavaElementRenderException exception, string parameterName, string parameterValue )
        {
            if ( exception != null )
            {
                exception.AddParameter( parameterName, parameterValue );
            }

            return exception;
        }
    }
}