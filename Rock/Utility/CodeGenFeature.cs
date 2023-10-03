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

namespace Rock.Utility
{
    /// <summary>
    /// The code generation features supported by the Rock Code Generation tool.
    /// </summary>
    [Flags]
    internal enum CodeGenFeature
    {
        /// <summary>
        /// No features of the code generation tool.
        /// </summary>
        None = 0,

        /// <summary>
        /// The view model file used by clients.
        /// </summary>
        ViewModelFile = 0x0001,

        /// <summary>
        /// Excludes the default REST controller that would be generated for a model.
        /// </summary>
        DefaultRestController = 0x0002,

        /// <summary>
        /// All features of the code generation tool.
        /// </summary>
        All = 0x7FFFFFFF
    }
}
