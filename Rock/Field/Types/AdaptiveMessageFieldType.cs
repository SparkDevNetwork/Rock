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
using System.Linq;

using Rock.Attribute;
using Rock.ClientService.Core.Category;
using Rock.ClientService.Core.Category.Options;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Field.Types
{
    /// <summary>
    /// Adaptive Message Field Type. 
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.Administrative )]
    [FieldTypeGuid( SystemGuid.FieldType.ADAPTIVE_MESSAGE )]
    internal class AdaptiveMessageFieldType : UniversalItemTreePickerFieldType
    {
        /// <inheritdoc/>
        protected override List<ListItemBag> GetItemBags( IEnumerable<string> values, Dictionary<string, string> privateConfigurationValues )
        {
            var guids = values.Select( v => v.AsGuid() ).ToList();

            var messages = AdaptiveMessageCache.GetMany( guids );
            var msgBags = messages
                .Select( m => new ListItemBag
                {
                    Value = m.Guid.ToString(),
                    Text = m.Name
                } ).ToList();

            return msgBags;
        }

        /// <inheritdoc/>
        protected override List<string> GetSelectableItemTypes( Dictionary<string, string> privateConfigurationValues )
        {
            return new List<string> { "Item" };
        }

        /// <inheritdoc/>
        protected override List<TreeItemBag> GetTreeItems( UniversalItemTreePickerGetItemsOptions options )
        {
            using ( var rockContext = new RockContext() )
            {
                var ccService = new CategoryClientService( rockContext, options.RequestContext.CurrentPerson );
                var amcService = new AdaptiveMessageCategoryService( rockContext );
                var grant = SecurityGrant.FromToken( options.PickerOptions.SecurityGrantToken );
                var items = GetAdaptiveMessageChildren( options.PickerOptions.ParentValue.AsGuidOrNull(), ccService, amcService, grant );

                return items;
            }
        }

        private List<TreeItemBag> GetAdaptiveMessageChildren( Guid? parent, CategoryClientService ccService, AdaptiveMessageCategoryService amcService, SecurityGrant grant )
        {
            var items = ccService.GetCategorizedTreeItems( new CategoryItemTreeOptions
            {
                ParentGuid = parent,
                GetCategorizedItems = true,
                EntityTypeGuid = EntityTypeCache.Get<Rock.Model.AdaptiveMessageCategory>().Guid,
                IncludeUnnamedEntityItems = true,
                IncludeCategoriesWithoutChildren = false,
                DefaultIconCssClass = "ti ti-list-numbers",
                LazyLoad = true,
                SecurityGrant = grant
            } );

            var messages = new List<TreeItemBag>();

            // Not a folder, so is actually an AdaptiveMessage, except it was loaded as an
            // AdaptiveMessageCategory so we need to get the Guid of the actual AdaptiveMessage
            foreach ( var item in items )
            {
                if ( !item.IsFolder )
                {
                    item.Type = "Item";
                    // Load the AdaptiveMessageCategory.
                    var category = amcService.Get( item.Value.AsGuid() );
                    if ( category != null )
                    {
                        // Swap the Guid to the AdaptiveMessage Guid
                        item.Value = category.AdaptiveMessage.Guid.ToString();
                    }
                }
                else
                {
                    item.Type = "Category";
                }

                // Get Children
                if ( item.HasChildren )
                {
                    item.Children = new List<TreeItemBag>();
                    item.Children.AddRange( GetAdaptiveMessageChildren( item.Value.AsGuid(), ccService, amcService, grant ) );
                }

                messages.Add( item );
            }

            return messages;
        }
    }
}
