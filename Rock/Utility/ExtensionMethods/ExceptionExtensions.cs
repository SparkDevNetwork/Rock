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
using System.Collections.Generic;

namespace Rock
{
    /// <summary>
    /// Exception Extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        #region Exception Extensions

        /// <summary>
        /// Gets the exception's message and the messages for all internal exceptions.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns></returns>
        public static List<string> Messages( this Exception ex )
        {
            var messages = new List<string>();

            var exception = ex;
            while ( exception != null )
            {
                messages.Add( exception.Message );
                exception = exception.InnerException;
            }

            return messages;
        }

        #endregion
    }
}
