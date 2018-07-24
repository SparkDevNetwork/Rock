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
using System.ComponentModel;
using System.Web.UI;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Tags" )]
    [Category( "Core" )]
    [Description( "Add tags to current context object." )]

    [ContextAware]
    [TextField( "Entity Qualifier Column", "The entity column to evaluate when determining if this attribute applies to the entity", false, "", "Filter", 0 )]
    [TextField( "Entity Qualifier Value", "The entity column value to evaluate.  Attributes will only apply to entities with this value", false, "", "Filter", 1 )]
    public partial class Tags : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                // Get the context entity
                Rock.Data.IEntity contextEntity = this.ContextEntity();

                tagEntityTags.Visible = contextEntity != null;
                if ( contextEntity != null )
                {
                    tagEntityTags.EntityTypeId = EntityTypeCache.Get( contextEntity.GetType() ).Id;
                    tagEntityTags.EntityGuid = contextEntity.Guid;
                    tagEntityTags.EntityQualifierColumn = GetAttributeValue( "EntityQualifierColumn" );
                    tagEntityTags.EntityQualifierValue = GetAttributeValue( "EntityQualifierValue" );
                    tagEntityTags.GetTagValues( CurrentPersonId );
                }
            }
        }
    }
}