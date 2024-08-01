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

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Contains the information needed to validate a target value.
    /// </summary>
    /// <typeparam name="TTarget">The target type to validate.</typeparam>
    internal class ValidationContext<TTarget>
    {
        /// <summary>
        /// Gets the validation target.
        /// </summary>
        internal TTarget Target { get; }

        /// <summary>
        /// Gets the friendly name for the validation target.
        /// </summary>
        /// <remarks>
        /// This is used to generate user-friendly error messages.
        /// </remarks>
        internal string FriendlyName { get; }

        /// <summary>
        /// Gets or sets the delegate that generates the error message if validation fails.
        /// </summary>
        internal Func<ValidationContext<TTarget>, string> ErrorMessageBuilder { get; set; }

        /// <summary>
        /// Creates a new <see cref="ValidationContext{TTarget}"/> instance.
        /// </summary>
        /// <param name="target">The validation target.</param>
        /// <param name="friendlyName">The validation target friendly name that will be used in error messages if validation fails.</param>
        internal ValidationContext( TTarget target, string friendlyName )
        {
            Target = target;
            FriendlyName = friendlyName;
        }
    }
}
