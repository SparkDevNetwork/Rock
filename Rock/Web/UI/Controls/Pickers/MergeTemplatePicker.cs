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

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class MergeTemplatePicker : ItemPicker
    {

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            UpdateItemRestUrlExtraParams();
            this.IconCssClass = "fa fa-files-o";
            base.OnInit( e );
        }

        /// <summary>
        /// Updates the item rest URL extra parameters.
        /// </summary>
        private void UpdateItemRestUrlExtraParams()
        {
            ItemRestUrlExtraParams = "?getCategorizedItems=true&showUnnamedEntityItems=false&showCategoriesThatHaveNoChildren=false";
            if ( this.MergeTemplateOwnership == MergeTemplateOwnership.Global )
            {
                ItemRestUrlExtraParams += string.Format( "&excludedCategoryIds={0}", CategoryCache.Get( Rock.SystemGuid.Category.PERSONAL_MERGE_TEMPLATE.AsGuid() ).Id );
            }
            else if ( this.MergeTemplateOwnership == MergeTemplateOwnership.Personal )
            {
                ItemRestUrlExtraParams += string.Format( "&includedCategoryIds={0}", CategoryCache.Get( Rock.SystemGuid.Category.PERSONAL_MERGE_TEMPLATE.AsGuid() ).Id );
            }
            else if ( this.MergeTemplateOwnership == MergeTemplateOwnership.PersonalAndGlobal )
            {
                //
            }

            ItemRestUrlExtraParams += "&entityTypeId=" + EntityTypeCache.Get( Rock.SystemGuid.EntityType.MERGE_TEMPLATE.AsGuid() ).Id;
        }

        /// <summary>
        /// Gets or sets the personal merge templates.
        /// </summary>
        /// <value>
        /// The personal merge templates.
        /// </value>
        public MergeTemplateOwnership MergeTemplateOwnership
        {
            get
            {
                return _mergeTemplateOwnership;
            }
            set
            {
                _mergeTemplateOwnership = value;
                UpdateItemRestUrlExtraParams();
            }
        }
        private MergeTemplateOwnership _mergeTemplateOwnership;

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="mergeTemplate">The merge template.</param>
        public void SetValue( MergeTemplate mergeTemplate )
        {
            if ( mergeTemplate != null )
            {
                ItemId = mergeTemplate.Id.ToString();

                var parentCategoryIds = new List<string>();
                var parentCategory = CategoryCache.Get( mergeTemplate.CategoryId );
                while ( parentCategory != null )
                {
                    if ( !parentCategoryIds.Contains( parentCategory.Id.ToString() ) )
                    {
                        parentCategoryIds.Insert( 0, parentCategory.Id.ToString() );
                    }
                    else
                    {
                        // infinite recursion
                        break;
                    }

                    parentCategory = parentCategory.ParentCategory;
                }

                InitialItemParentIds = parentCategoryIds.AsDelimited( "," );
                ItemName = mergeTemplate.Name;
            }
            else
            {
                ItemId = Constants.None.IdValue;
                ItemName = Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Sets the values.
        /// </summary>
        /// <param name="mergeTemplates">The merge templates.</param>
        public void SetValues( IEnumerable<MergeTemplate> mergeTemplates )
        {
            var mergeTemplateList = mergeTemplates.ToList();

            if ( mergeTemplateList.Any() )
            {
                var ids = new List<string>();
                var names = new List<string>();
                var parentCategoryIds = string.Empty;

                foreach ( var mergeTemplate in mergeTemplateList )
                {
                    if ( mergeTemplate != null )
                    {
                        ids.Add( mergeTemplate.Id.ToString() );
                        names.Add( mergeTemplate.Name );
                        var parentCategory = mergeTemplate.Category;

                        while ( parentCategory != null )
                        {
                            parentCategoryIds += parentCategory.Id.ToString() + ",";
                            parentCategory = parentCategory.ParentCategory;
                        }
                    }
                }

                InitialItemParentIds = parentCategoryIds.TrimEnd( new[] { ',' } );
                ItemIds = ids;
                ItemNames = names;
            }
            else
            {
                ItemId = Constants.None.IdValue;
                ItemName = Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Sets the value on select.
        /// </summary>
        protected override void SetValueOnSelect()
        {
            var item = new MergeTemplateService( new RockContext() ).Get( ItemId.AsInteger() );
            this.SetValue( item );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected override void SetValuesOnSelect()
        {
            var itemIds = ItemIds.Select( int.Parse );
            var items = new MergeTemplateService( new RockContext() ).Queryable().Where( i => itemIds.Contains( i.Id ) );
            this.SetValues( items );
        }

        /// <summary>
        /// Gets the item rest URL.
        /// </summary>
        /// <value>
        /// The item rest URL.
        /// </value>
        public override string ItemRestUrl
        {
            get { return "~/api/Categories/GetChildren/"; }
        }
    }
}
