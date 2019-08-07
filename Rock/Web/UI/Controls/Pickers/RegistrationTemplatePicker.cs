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
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class RegistrationTemplatePicker : ItemPicker
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            ItemRestUrlExtraParams = "?getCategorizedItems=true&showUnnamedEntityItems=false&showCategoriesThatHaveNoChildren=false";
            ItemRestUrlExtraParams += "&entityTypeId=" + EntityTypeCache.Get( Rock.SystemGuid.EntityType.REGISTRATION_TEMPLATE.AsGuid() ).Id;
            this.IconCssClass = "fa fa-calendar";
            base.OnInit( e );
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="registrationTemplate">The registrationTemplate.</param>
        public void SetValue( RegistrationTemplate registrationTemplate )
        {
            if ( registrationTemplate != null )
            {
                ItemId = registrationTemplate.Id.ToString();

                string parentCategoryIds = string.Empty;
                var parentCategory = registrationTemplate.Category;
                while ( parentCategory != null )
                {
                    parentCategoryIds = parentCategory.Id + "," + parentCategoryIds;
                    parentCategory = parentCategory.ParentCategory;
                }

                InitialItemParentIds = parentCategoryIds.TrimEnd( new[] { ',' } );
                ItemName = registrationTemplate.Name;
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
        /// <param name="registrationTemplates">The registrationTemplates.</param>
        public void SetValues( IEnumerable<RegistrationTemplate> registrationTemplates )
        {
            var registrationTemplateList = registrationTemplates.ToList();

            if ( registrationTemplateList.Any() )
            {
                var ids = new List<string>();
                var names = new List<string>();
                var parentCategoryIds = string.Empty;

                foreach ( var registrationTemplate in registrationTemplateList )
                {
                    if ( registrationTemplate != null )
                    {
                        ids.Add( registrationTemplate.Id.ToString() );
                        names.Add( registrationTemplate.Name );
                        var parentCategory = registrationTemplate.Category;

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
            var registrationTemplate = new RegistrationTemplateService( new RockContext() ).Get( int.Parse( ItemId ) );
            SetValue( registrationTemplate );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected override void SetValuesOnSelect()
        {
            var registrationTemplates = new RegistrationTemplateService( new RockContext() ).Queryable().Where( g => ItemIds.Contains( g.Id.ToString() ) );
            this.SetValues( registrationTemplates );
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