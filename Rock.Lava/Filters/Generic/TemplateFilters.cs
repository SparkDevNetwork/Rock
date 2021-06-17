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

namespace Rock.Lava.Filters
{
    /// <summary>
    /// A set of functions that can be used to perform filter and transformation operations on a text stream.
    /// </summary>
    /// <remarks>
    /// These filters are intended to be used in the context of a text templating engine, however their implementation should be engine-agnostic.
    /// This will allow the functions to be more easily wrapped and implemented elsewhere for specific templating engines.
    /// If these filters are found to have more general application, they should be moved to the Rock.Common library.
    ///
    /// Template filters must have the following properties:
    /// 1. The filter function must have a return type of string.
    /// 2. Input parameters should be of type string or object. Any parameter conversion should be performed in the function itself.
    /// 3. No optional parameters. Some Liquid templating frameworks do not handle these correctly, so use an explicit function overloads to define different parameter sets.
    /// </remarks>
    public static partial class TemplateFilters
    {
        //
    }
}
