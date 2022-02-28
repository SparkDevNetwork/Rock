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

using Rock.Attribute;

namespace Rock.ViewModel
{
    /// <summary>
    /// Identifies this <see cref="IViewModelHelper"/> or <see cref="IViewModel"/> as
    /// the default one for the model type specified.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal]
    [AttributeUsage( AttributeTargets.Class )]
    public class DefaultViewModelHelperAttribute : System.Attribute
    {
        /// <summary>
        /// Gets the model type.
        /// </summary>
        /// <value>
        /// The model type.
        /// </value>
        public Type Type { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewModelHelperAttribute"/> class.
        /// </summary>
        /// <param name="type">The model type.</param>
        public DefaultViewModelHelperAttribute( Type type )
        {
            Type = type;
        }
    }
}
