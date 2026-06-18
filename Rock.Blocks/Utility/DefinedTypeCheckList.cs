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
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Utility.DefinedTypeCheckList;
using Rock.Web.Cache;

namespace Rock.Blocks.Utility
{
    /// <summary>
    /// Displays the values of a Defined Type as a checklist, persisting each item's
    /// completion state in a boolean attribute on the Defined Value.
    /// </summary>
    [DisplayName( "Defined Type Check List" )]
    [Category( "Utility" )]
    [Description( "Used for managing the values of a defined type as a checklist." )]

    #region Block Attributes

    [DefinedTypeField(
        "Defined Type",
        Key = AttributeKey.DefinedType,
        Description = "The Defined Type to display values for.",
        Order = 0 )]

    [TextField(
        "Attribute Key",
        Key = AttributeKey.ItemAttributeKey,
        Description = "The attribute key on the Defined Type that is used to store whether item has been completed (should be a boolean field type).",
        Order = 1 )]

    [BooleanField(
        "Hide Checked Items",
        Key = AttributeKey.HideCheckedItems,
        Description = "Hide items that are already checked.",
        DefaultBooleanValue = false,
        Order = 2 )]

    [BooleanField(
        "Hide Block When Empty",
        Key = AttributeKey.HideBlockWhenEmpty,
        Description = "Hides entire block if no checklist items are available.",
        DefaultBooleanValue = false,
        Order = 3 )]

    [TextField(
        "Checklist Title",
        Key = AttributeKey.ChecklistTitle,
        Description = "Title for your checklist.",
        IsRequired = false,
        DefaultValue = "",
        Category = "Description",
        Order = 4 )]

    [CodeEditorField(
        "Checklist Description",
        Key = AttributeKey.ChecklistDescription,
        Description = "Description for your checklist. Leave this blank and nothing will be displayed.",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Html,
        EditorHeight = 100,
        IsRequired = false,
        Category = "Description",
        Order = 5 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "08DC63DC-FF37-4284-8DF9-0600C2AE7EC6" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "6B7690DE-F2D1-4F6C-9634-73DE548FA505" )]
    [Rock.SystemGuid.BlockTypeGuid( "15572974-DD86-43C8-BBBF-5181EE76E2C9" )]
    public class DefinedTypeCheckList : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DefinedType = "DefinedType";
            public const string ItemAttributeKey = "AttributeKey"; // can't do AttributeKey.AttributeKey
            public const string HideCheckedItems = "HideCheckedItems";
            public const string HideBlockWhenEmpty = "HideBlockWhenEmpty";
            public const string ChecklistTitle = "ChecklistTitle";
            public const string ChecklistDescription = "ChecklistDescription";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<DefinedTypeCheckListBag, DefinedTypeCheckListOptionsBag>();

            box.Bag = new DefinedTypeCheckListBag
            {
                Title = GetAttributeValue( AttributeKey.ChecklistTitle ),
                Description = GetAttributeValue( AttributeKey.ChecklistDescription ),
                Items = GetChecklistItems()
            };

            box.Options = new DefinedTypeCheckListOptionsBag
            {
                AreCheckedItemsHidden = GetAttributeValue( AttributeKey.HideCheckedItems ).AsBoolean(),
                IsBlockHiddenWhenEmpty = GetAttributeValue( AttributeKey.HideBlockWhenEmpty ).AsBoolean()
            };

            return box;
        }

        /// <summary>
        /// Builds the checklist items for the configured Defined Type, reading each value's
        /// completion state from the configured attribute. Returns an empty list when the
        /// block is not fully configured.
        /// </summary>
        /// <returns>The checklist items ordered for display.</returns>
        private List<DefinedTypeCheckListItemBag> GetChecklistItems()
        {
            var attributeKey = GetAttributeValue( AttributeKey.ItemAttributeKey );
            var definedTypeGuid = GetAttributeValue( AttributeKey.DefinedType ).AsGuidOrNull();

            if ( !definedTypeGuid.HasValue || attributeKey.IsNullOrWhiteSpace() )
            {
                return new List<DefinedTypeCheckListItemBag>();
            }

            var definedType = DefinedTypeCache.Get( definedTypeGuid.Value );

            if ( definedType == null )
            {
                return new List<DefinedTypeCheckListItemBag>();
            }

            return definedType.DefinedValues
                .OrderBy( v => v.Order )
                .Select( v => new DefinedTypeCheckListItemBag
                {
                    IdKey = v.IdKey,
                    Text = v.Value,
                    Description = v.Description,
                    IsChecked = v.GetAttributeValue( attributeKey ).AsBoolean()
                } )
                .ToList();
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Persists the completion state for a single checklist item.
        /// </summary>
        /// <param name="key">The IdKey of the Defined Value to update.</param>
        /// <param name="isChecked">Whether the item is checked (complete).</param>
        /// <returns>An empty success result, or a bad-request result describing the problem.</returns>
        [BlockAction]
        public BlockActionResult SetItemCompletion( string key, bool isChecked )
        {
            var attributeKey = GetAttributeValue( AttributeKey.ItemAttributeKey );

            if ( attributeKey.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Attribute key is not configured." );
            }

            var definedTypeGuid = GetAttributeValue( AttributeKey.DefinedType ).AsGuidOrNull();
            var definedType = definedTypeGuid.HasValue ? DefinedTypeCache.Get( definedTypeGuid.Value ) : null;

            if ( definedType == null )
            {
                return ActionBadRequest( "Defined Type is not configured." );
            }

            var definedValue = new DefinedValueService( RockContext ).Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            // Only allow updating values that belong to the configured Defined Type.
            if ( definedValue == null || definedValue.DefinedTypeId != definedType.Id )
            {
                return ActionBadRequest( "Item not found." );
            }

            definedValue.LoadAttributes( RockContext );
            definedValue.SetAttributeValue( attributeKey, isChecked.ToString() );
            definedValue.SaveAttributeValues( RockContext );

            return ActionOk();
        }

        #endregion Block Actions
    }
}
