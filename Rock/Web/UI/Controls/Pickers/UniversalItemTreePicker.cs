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
using System.Web.UI;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// This is a special use tree item picker for use with
    /// <see cref="Rock.Field.Types.UniversalItemTreePickerFieldType"/>.
    /// </summary>
    internal sealed class UniversalItemTreePicker : ItemPicker
    {
        private string _itemRestUrl;

        /// <inheritdoc/>
        public override string ItemRestUrl => _itemRestUrl;

        /// <inheritdoc/>
        protected override void RegisterJavaScript()
        {
            string treeViewScript =
$@"Rock.controls.itemPicker.initialize({{
    controlId: '{ClientID}',
    universalItemPicker: true,
    restUrl: '{ResolveUrl( ItemRestUrl )}',
    allowMultiSelect: {AllowMultiSelect.ToString().ToLower()},
    allowCategorySelection: false,
    categoryPrefix: '',
    defaultText: '',
    expandedIds: [{InitialItemParentIds}],
    showSelectChildren: false
}});
";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "item_picker-treeviewscript_" + this.ClientID, treeViewScript, true );

            // Search Control

        }

        public void SetItemRestUrl( string url )
        {
            _itemRestUrl = url;
        }

        /// <inheritdoc/>
        protected override void SetValueOnSelect()
        {
        }

        /// <inheritdoc/>
        protected override void SetValuesOnSelect()
        {
        }
    }
}
