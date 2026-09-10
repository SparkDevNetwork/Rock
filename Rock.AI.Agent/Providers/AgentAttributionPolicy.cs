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

using System;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rock.AI.Agent.Providers;

/// <summary>
/// Adds attribution headers to every outgoing request so the upstream
/// service can identify which Rock agent made the call.
/// </summary>
internal class AgentAttributionPolicy : PipelinePolicy
{
    /// <summary>
    /// Prefix for the referer header. The agent identifier is appended to it.
    /// </summary>
    private const string RefererBase = "https://www.rockrms.com/agent/";

    /// <summary>
    /// The agent identifier for the kernel that owns this policy.
    /// </summary>
    private readonly Guid _agentGuid;

    /// <summary>
    /// Creates a new instance of the <see cref="AgentAttributionPolicy"/> class.
    /// </summary>
    /// <param name="agentGuid">The context describing the agent making the request.</param>
    public AgentAttributionPolicy( Guid agentGuid )
    {
        _agentGuid = agentGuid;
    }

    /// <inheritdoc/>
    public override void Process( PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex )
    {
        SetAttributionHeaders( message );

        ProcessNext( message, pipeline, currentIndex );
    }

    /// <inheritdoc/>
    public override ValueTask ProcessAsync( PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex )
    {
        SetAttributionHeaders( message );

        return ProcessNextAsync( message, pipeline, currentIndex );
    }

    /// <summary>
    /// Writes the attribution headers onto the outgoing request. Both values
    /// are derived from the agent identifier so that no staff authored text is
    /// sent to the upstream service.
    /// </summary>
    /// <param name="message">The message about to be sent.</param>
    private void SetAttributionHeaders( PipelineMessage message )
    {
        if ( message.Request == null )
        {
            return;
        }

        var identifier = _agentGuid.ToString( "D" );

        // Set replaces any existing value, so there is no separate remove step.
        message.Request.Headers.Set( "HTTP-Referer", $"{RefererBase}{identifier}" );
        message.Request.Headers.Set( "X-Title", identifier );
    }
}

