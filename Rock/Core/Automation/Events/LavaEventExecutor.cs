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
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using Rock.Logging;

namespace Rock.Core.Automation.Events
{
    /// <summary>
    /// Handles execution for the <see cref="LavaEvent"/> event component.
    /// </summary>
    class LavaEventExecutor : AutomationEventExecutor
    {
        #region Fields

        /// <summary>
        /// The identifier of the automation event that will be executed.
        /// </summary>
        private readonly int _automationEventId;

        /// <summary>
        /// The lava template to be executed.
        /// </summary>
        private readonly string _template;

        /// <summary>
        /// The lava commands that are enabled for this template as a comma separated list.
        /// </summary>
        private readonly string _enabledLavaCommands;

        /// <summary>
        /// The logger instance that will handle logging diagnostic messages.
        /// </summary>
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LavaEventExecutor"/> class.
        /// </summary>
        /// <param name="automationEventId">The identifier of the automation event that will be executed.</param>
        /// <param name="template">The Lava template to be executed.</param>
        /// <param name="enabledLavaCommands"> The Lava commands that are enabled for this template as a comma separated list.</param>
        public LavaEventExecutor( int automationEventId, string template, string enabledLavaCommands )
        {
            _automationEventId = automationEventId;
            _logger = RockLogger.LoggerFactory.CreateLogger<LavaEventExecutor>();
            _template = template;
            _enabledLavaCommands = enabledLavaCommands;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void Execute( AutomationRequest request )
        {
            var mergeFields = new Dictionary<string, object>();

            foreach ( var value in request.Values )
            {
                mergeFields[value.Key] = value.Value;
            }

            var result = _template.ResolveMergeFields( mergeFields, enabledLavaCommands: _enabledLavaCommands );

            _logger.LogDebug( "Lava template for event #{AutomationEventId} executed with result: {Result}", _automationEventId, result );
        }

        #endregion
    }
}
