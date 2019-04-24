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
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Creates a content channel item.
    /// </summary>
    [ActionCategory( "CMS" )]
    [Description( "Creates a content channel item." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Content Channel Item Add" )]

    [ContentChannelField( "Content Channel", "The content channel where items will be added.", true, null, "", 1, "ContentChannel" ) ]
    [TextField("Title", "The title of the content channel item. <span class='tip tip-lava'></span>", true, "", "", 2 )]
    [WorkflowTextOrAttribute( "Start Date Time", "Attribute Value", "Text (date time format) or datetime workflow attribute that contains the text to set the start date time. <span class='tip tip-lava'></span>", true, "", "", 3, "StartDateTime",
        new string[] { "Rock.Field.Types.DateTimeFieldType", "Rock.Field.Types.TextFieldType" } )]
    [WorkflowTextOrAttribute( "Expire Date Time", "Attribute Value", "An optional text (date time format) or datetime workflow attribute that contains the text to set the expiration date time. <span class='tip tip-lava'></span>", false, "", "", 4, "ExpireDateTime",
        new string[] { "Rock.Field.Types.DateTimeFieldType", "Rock.Field.Types.TextFieldType" } )]
    [WorkflowTextOrAttribute( "Content", "Attribute Value", "The content or a text/memo attribute that contains the content for the channel item. <span class='tip tip-lava'></span>", true, "", "", 5, "Content",
        new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.MemoFieldType" } )]
    [EnumField( "Status", "The  status for the new content channel item.", typeof( ContentChannelItemStatus ), true, "1", "", 6 )]
    [KeyValueListField( "Item Attribute Key", "Used to match the current workflow's attribute keys to the keys of the content channel item. The new content channel item will receive the values from this workflow's attributes.", false, keyPrompt: "Source Attribute", valuePrompt: "Target Attribute", order: 7 )]
    public class AddContentChannelItem : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow, setting the startDateTime to now (if none was given) and leaving
        /// the expireDateTime as null (if none was given).
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            var mergeFields = GetMergeFields( action );

            // Get the content channel
            Guid contentChannelGuid = GetAttributeValue( action, "ContentChannel" ).AsGuid();
            ContentChannel contentChannel = new ContentChannelService( rockContext ).Get( contentChannelGuid );
            if ( contentChannel == null )
            {
                errorMessages.Add( "Invalid Content Channel attribute or value!" );
                return false;
            }

            // Get the Content
            string contentValue = GetAttributeValue( action, "Content", true );
            string content = string.Empty;
            Guid? contentGuid = contentValue.AsGuidOrNull();
            if ( contentGuid.HasValue )
            {
                var attribute = AttributeCache.Get( contentGuid.Value, rockContext );
                if ( attribute != null )
                {
                    string contentAttributeValue = action.GetWorklowAttributeValue( contentGuid.Value );
                    if ( !string.IsNullOrWhiteSpace( contentAttributeValue ) )
                    {
                        if ( attribute.FieldType.Class == "Rock.Field.Types.TextFieldType" ||
                            attribute.FieldType.Class == "Rock.Field.Types.MemoFieldType" )
                        {
                            content = contentAttributeValue;
                        }
                    }
                }
            }
            else
            {
                content = contentValue;
            }

            // Get the Start Date Time (check if the attribute value is a guid first)
            DateTime startDateTime = RockDateTime.Now;
            string startAttributeValue = GetAttributeValue( action, "StartDateTime" );
            Guid startDateTimeAttributeGuid = startAttributeValue.AsGuid();
            if ( !startDateTimeAttributeGuid.IsEmpty() )
            {
                var attribute = AttributeCache.Get( startDateTimeAttributeGuid, rockContext );
                if ( attribute != null )
                {
                    string attributeValue = action.GetWorklowAttributeValue( startDateTimeAttributeGuid );
                    if ( !string.IsNullOrWhiteSpace( attributeValue ) )
                    {
                        if ( attribute.FieldType.Class == "Rock.Field.Types.TextFieldType" ||
                            attribute.FieldType.Class == "Rock.Field.Types.DateTimeFieldType" )
                        {
                            if ( !DateTime.TryParse( attributeValue, out startDateTime ) )
                            {
                                startDateTime = RockDateTime.Now;
                                errorMessages.Add( string.Format( "Could not parse the start date provided {0}.", attributeValue ) );
                            }
                        }
                    }
                }
            }
            // otherwise check just the plain value and then perform lava merge on it.
            else if ( !string.IsNullOrWhiteSpace( startAttributeValue ) )
            {
                string mergedStartAttributeValue = startAttributeValue.ResolveMergeFields( mergeFields );
                if ( ! DateTime.TryParse( mergedStartAttributeValue, out startDateTime ) )
                {
                    startDateTime = RockDateTime.Now;
                    errorMessages.Add( string.Format( "Could not parse the start date provided {0}.", startAttributeValue ) );
                }
            }

            // Get the Expire Date Time (check if the attribute value is a guid first)
            DateTime? expireDateTime = null;
            string expireAttributeValue = GetAttributeValue( action, "ExpireDateTime" );
            Guid expireDateTimeAttributeGuid = expireAttributeValue.AsGuid();
            if ( !expireDateTimeAttributeGuid.IsEmpty() )
            {
                var attribute = AttributeCache.Get( expireDateTimeAttributeGuid, rockContext );
                if ( attribute != null )
                {
                    DateTime aDateTime;
                    string attributeValue = action.GetWorklowAttributeValue( expireDateTimeAttributeGuid );
                    if ( !string.IsNullOrWhiteSpace( attributeValue ) )
                    {
                        if ( attribute.FieldType.Class == "Rock.Field.Types.TextFieldType" ||
                            attribute.FieldType.Class == "Rock.Field.Types.DateTimeFieldType" )
                        {
                            if ( DateTime.TryParse( attributeValue, out aDateTime ) )
                            {
                                expireDateTime = aDateTime;
                            }
                            else
                            {
                                errorMessages.Add( string.Format( "Could not parse the expire date provided {0}.", attributeValue ) );
                            }
                        }
                    }
                }
            }
            // otherwise check just the text value and then perform lava merge on it.
            else if ( ! string.IsNullOrWhiteSpace( expireAttributeValue ) ) 
            {
                string mergedExpireAttributeValue = expireAttributeValue.ResolveMergeFields( mergeFields );
                DateTime aDateTime;
                if ( DateTime.TryParse( mergedExpireAttributeValue, out aDateTime ) )
                {
                    expireDateTime = aDateTime;
                }
                else
                {
                    errorMessages.Add( string.Format( "Could not parse the expire date provided {0}.", expireAttributeValue ) );
                }
            }

            // Get the Content Channel Item Status
            var channelItemStatus = this.GetAttributeValue( action, "Status" ).ConvertToEnum<ContentChannelItemStatus>( ContentChannelItemStatus.PendingApproval );

            // Save the new content channel item
            var contentChannelItemService = new ContentChannelItemService( rockContext );
            var contentChannelItem = new ContentChannelItem();

            contentChannelItem.ContentChannelId = contentChannel.Id;
            contentChannelItem.ContentChannelTypeId = contentChannel.ContentChannelTypeId;

            contentChannelItem.Title = GetAttributeValue( action, "Title" ).ResolveMergeFields( mergeFields );
            contentChannelItem.Content = content.ResolveMergeFields( mergeFields );
            contentChannelItem.StartDateTime = startDateTime;
            contentChannelItem.ExpireDateTime = expireDateTime;
            contentChannelItem.Status = channelItemStatus;

            contentChannelItemService.Add( contentChannelItem );
            rockContext.SaveChanges();

            Dictionary<string, string> sourceKeyMap = null;
            var itemAttributeKeys = GetAttributeValue( action, "ItemAttributeKey" );
            if ( !string.IsNullOrWhiteSpace( itemAttributeKeys ) )
            {
                // TODO Find a way upstream to stop an additional being appended to the value
                sourceKeyMap = itemAttributeKeys.AsDictionaryOrNull();
            }

            sourceKeyMap = sourceKeyMap ?? new Dictionary<string, string>();

            // Load the content channel item attributes if we're going to add some values
            if ( sourceKeyMap.Count > 0 )
            {
                contentChannelItem.LoadAttributes( rockContext );

                foreach ( var keyPair in sourceKeyMap )
                {
                    // Does the source key exist as an attribute in the this workflow?
                    if ( action.Activity.Workflow.Attributes.ContainsKey( keyPair.Key ) )
                    {
                        if ( contentChannelItem.Attributes.ContainsKey( keyPair.Value ) )
                        {
                            var value = action.Activity.Workflow.AttributeValues[keyPair.Key].Value;
                            contentChannelItem.SetAttributeValue( keyPair.Value, value );
                        }
                        else
                        {
                            errorMessages.Add( string.Format( "'{0}' is not an attribute key in the content channel: '{1}'", keyPair.Value, contentChannel.Name ) );
                        }
                    }
                    else
                    {
                        errorMessages.Add( string.Format( "'{0}' is not an attribute key in this workflow: '{1}'", keyPair.Key, action.Activity.Workflow.Name ) );
                    }
                }

                contentChannelItem.SaveAttributeValues( rockContext );
            }

            return true;
        }
    }
}