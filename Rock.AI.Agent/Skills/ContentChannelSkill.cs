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
using System.ComponentModel;
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.ContentChannelSkill;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

/// <summary>
/// This skill provides access to content channel and item details.
/// </summary>

[Description( "This skill provides access to content channel and item details." )]
[AgentSkillGuid( "d450c4d3-1deb-4b2d-a8e6-4f46cd722a0b" )]
[EntityTypeGuid( "38fa0fc0-cf91-4f68-8b32-88db87058953" )]
internal sealed partial class ContentChannelSkill : AgentSkillComponent
{
    #region Fields

    /// <summary>
    /// The logger for this instance.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    #region Constructors

    /// <summary>
    /// The constructor for the Content Channel Type Skill.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and error reporting.</param>
    public ContentChannelSkill( ILogger<ContentChannelSkill> logger )
    {
        _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
    }

    #endregion

    #region Methods

    private ContentChannelItemResult GetFullContentChannelItemResult( ContentChannelItem contentChannelItem )
    {
        return new ContentChannelItemResult
        {
            Id = contentChannelItem.Id,
            Guid = contentChannelItem.Guid,
            Name = contentChannelItem.Title,
            ContentChannel = new ContentChannelResult
            {
                Id = contentChannelItem.ContentChannelId,
                Name = contentChannelItem.ContentChannel.Name,
            },
            ContentAsHtml = contentChannelItem.Content,
            Status = contentChannelItem.ContentChannel.RequiresApproval
                ? contentChannelItem.Status
                : ContentChannelItemStatus.Approved,
            ApprovedByPerson = PersonResult.NameOnly( contentChannelItem.ApprovedByPersonAlias ),
            StartDateTime = contentChannelItem.StartDateTime,
            ExpireDateTime = contentChannelItem.ExpireDateTime,
            PermanentLink = contentChannelItem.Permalink,
            AttributeValues = contentChannelItem.GetAttributeValueResults( AgentRequestContext ).ToList(),
        };
    }

    #endregion
}
