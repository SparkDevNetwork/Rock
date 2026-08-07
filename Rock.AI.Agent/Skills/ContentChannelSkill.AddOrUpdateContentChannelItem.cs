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
using System.Collections.Generic;
using System.ComponentModel;

using Rock.AI.Agent.Classes;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Utilities;
using Rock.Cms.StructuredContent;
using Rock.Configuration;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class ContentChannelSkill
{
    #region Tool(s)

    [Description( "Adds new or updates existing content channel item." )]
    [AgentToolGuid( "90023821-ba55-4de3-99c1-3da8e8f123bd" )]
    public AgentToolResult AddOrUpdateContentChannelItem(
        [Description( "Required when editing an existing content channel item." )]
        string contentChannelItemIdKey = null,

        [Description( "Only valid when adding new content channel item." )]
        string contentChannelIdKey = null,

        SetOrClear<string> name = null,
        [Description( "The content of the item in CommonMark format. Only supports headers, paragraphs, lists, bold and italics." )]
        SetOrClear<string> contentAsMarkdown = null,
        [Description( "Only set value is specifically requested, otherwise allow the default to be automatically set.")]
        ContentChannelItemStatus? contentChannelItemStatus = null,
        DateTime? startDateTime = null,
        SetOrClear<DateTime> expireDateTime = null,

        List<AttributeValueResult> attributeValues = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        ContentChannelItem contentChannelItem;

        if ( contentChannelItemIdKey.IsNotNullOrWhiteSpace() )
        {
            contentChannelItem = helper.GetRequiredEntity<ContentChannelItem>( contentChannelItemIdKey, checkSecurity: true );

            if ( contentChannelIdKey.IsNotNullOrWhiteSpace() )
            {
                helper.AddError( $"A content channel item cannot be moved to a new content channel, do not provide a {nameof( contentChannelIdKey )} when editing." );
            }
        }
        else
        {
            contentChannelItem = rockContext.Set<ContentChannelItem>().Create();
            new ContentChannelItemService( rockContext ).Add( contentChannelItem );

            var contentChannel = helper.GetOptionalEntity<ContentChannel>( contentChannelIdKey, checkSecurity: true );

            if ( contentChannel != null )
            {
                contentChannelItem.ContentChannel = contentChannel;
                contentChannelItem.ContentChannelId = contentChannel.Id;
                contentChannelItem.ContentChannelType = contentChannel.ContentChannelType;
                contentChannelItem.ContentChannelTypeId = contentChannel.ContentChannelTypeId;

                if ( contentChannelItem.ContentChannelType.DisableStatus )
                {
                    contentChannelItem.Status = ContentChannelItemStatus.Approved;
                }
            }
            else
            {
                helper.AddError( $"You must provide either a {nameof( contentChannelItemIdKey )} to update an existing content channel item or a {nameof( contentChannelIdKey )} to add a new content channel item." );
            }
        }

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        helper.UpdateProperty( contentChannelItem, cci => cci.Title, name );
        helper.UpdateProperty( contentChannelItem, cci => cci.StartDateTime, startDateTime );
        helper.UpdateProperty( contentChannelItem, cci => cci.ExpireDateTime, expireDateTime );
        helper.SetAttributeValues( contentChannelItem, attributeValues );

        UpdateItemContent( helper, contentChannelItem, contentAsMarkdown );

        var disableStatus = contentChannelItem.ContentChannelType.DisableStatus;
        var isAuthorizedToApprove = contentChannelItem.IsAuthorized( Authorization.APPROVE, AgentRequestContext.CurrentPerson );

        if ( contentChannelItemStatus.HasValue )
        {
            if ( disableStatus )
            {
                helper.AddError( $"The content channel type associated with this content channel item has status disabled, so the status can't be changed." );
            }
            else if ( !isAuthorizedToApprove )
            {
                helper.AddError( $"You are not authorized to change the status of this content channel item." );
            }
            else
            {
                contentChannelItem.Status = contentChannelItemStatus.Value;
            }
        }
        else if ( !disableStatus && !isAuthorizedToApprove )
        {
            // If anything changed and they don't have approval permissions, then
            // force it to be unapproved. This matches behavior of the detail block.
            contentChannelItem.Status = ContentChannelItemStatus.PendingApproval;
            contentChannelItem.ApprovedByPersonAliasId = null;
            contentChannelItem.ApprovedDateTime = null;
        }

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        return Success( GetFullContentChannelItemResult( contentChannelItem ) )
            .WithHistoryContent( new KeyNameResult
            {
                Id = contentChannelItem.Id,
                Name = contentChannelItem.ToString()
            } )
            .WithInstructions( $"The content channel item has been {( contentChannelItemIdKey.IsNullOrWhiteSpace() ? "created" : "updated" )}." );
    }

    #endregion

    #region Methods

    private static void UpdateItemContent( AgentToolHelper helper, ContentChannelItem contentChannelItem, SetOrClear<string> contentAsMarkdown )
    {
        if ( contentAsMarkdown == null )
        {
            return;
        }

        if ( contentChannelItem.ContentChannel.IsStructuredContent )
        {
            if ( contentAsMarkdown.ClearValue )
            {
                contentChannelItem.Content = string.Empty;
                contentChannelItem.StructuredContent = string.Empty;
            }
            else
            {
                try
                {
                    var contentConverter = new MarkdownConverter();
                    var jsonContent = contentConverter.ConvertToEditorJs( contentAsMarkdown.Value );

                    contentChannelItem.StructuredContent = jsonContent;

                    var contentRenderer = new StructuredContentHelper( jsonContent );
                    contentChannelItem.Content = contentRenderer.Render();
                }
                catch ( InvalidMarkdownTagException ex )
                {
                    helper.AddError( $"The provided markdown content contains an unsupported tag: {ex.TagType}" );
                }
                catch ( InvalidMarkdownException ex )
                {
                    helper.AddError( $"The provided markdown content is invalid: {ex.Message}" );
                }
            }
        }
        else
        {
            if ( contentAsMarkdown.ClearValue )
            {
                contentChannelItem.Content = string.Empty;
            }
            else
            {
                contentChannelItem.Content = contentAsMarkdown.Value?.ConvertMarkdownToHtml() ?? string.Empty;
            }
        }
    }

    #endregion
}
