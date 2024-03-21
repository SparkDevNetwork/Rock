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

using Rock.Lava;

namespace Rock.Tests.Shared.Lava
{
    /// <summary>
    /// A set of options for testing the rendering of a Lava template.
    /// </summary>
    public class LavaTestRenderOptions
    {
        public IDictionary<string, object> MergeFields = null;
        public string EnabledCommands = null;
        public string EnabledCommandsDelimiter = ",";

        public bool IgnoreWhiteSpace = true;
        public bool IgnoreCase = false;

        public List<string> Wildcards = new List<string>();

        public LavaTestOutputMatchTypeSpecifier OutputMatchType = LavaTestOutputMatchTypeSpecifier.Equal;

        public ExceptionHandlingStrategySpecifier? ExceptionHandlingStrategy;

        public List<Type> LavaEngineTypes = new List<Type>();
    }
}
