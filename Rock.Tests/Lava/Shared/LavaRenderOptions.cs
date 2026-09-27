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
using System.Globalization;

using Rock.Lava;

namespace Rock.Tests.Lava.Shared
{
    /// <summary>
    /// Configuration for rendering a Lava template in a test.
    /// </summary>
    /// <remarks>
    /// This type describes how a template is rendered and nothing about how the
    /// result is compared. Comparison is the job of the assertion the test makes
    /// against the returned string, which keeps the matching rules visible at the
    /// call site rather than hidden behind a default on an options object.
    /// </remarks>
    public class LavaRenderOptions
    {
        /// <summary>
        /// The merge fields made available to the template.
        /// </summary>
        public IDictionary<string, object> MergeFields { get; set; }

        /// <summary>
        /// The Lava commands enabled for the render, as a delimited list
        /// (for example <c>"execute,sql"</c>).
        /// </summary>
        public string EnabledCommands { get; set; }

        /// <summary>
        /// The delimiter used to separate the values in <see cref="EnabledCommands"/>.
        /// </summary>
        public string EnabledCommandsDelimiter { get; set; } = ",";

        /// <summary>
        /// Overrides how the engine handles an exception raised while rendering.
        /// When not set, the engine's configured strategy applies.
        /// </summary>
        public ExceptionHandlingStrategySpecifier? ExceptionHandlingStrategy { get; set; }

        /// <summary>
        /// The time zone dates are rendered in.
        /// </summary>
        /// <remarks>
        /// The engine copies the organization time zone into its template options
        /// when it is initialized, so a test that changes the organization time
        /// zone afterwards must set this to render in the new one.
        /// </remarks>
        public TimeZoneInfo TimeZone { get; set; }

        /// <summary>
        /// The culture dates and numbers are formatted for. The current culture
        /// applies when this is not set.
        /// </summary>
        public CultureInfo Culture { get; set; }

        /// <summary>
        /// Whether string values are XML-encoded as they are written to the output.
        /// </summary>
        public bool ShouldEncodeStringsAsXml { get; set; }
    }
}
