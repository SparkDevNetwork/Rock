﻿// <copyright>
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
using System.Linq;
using System.Web.UI.WebControls;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.ViewModels.Controls;

namespace Rock.Reporting.DataSelect.Group
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Show group's name as a optional link that navigates to the group's record" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select Group Name" )]

    [BooleanField( "Show As Link", "", true )]
    [Rock.SystemGuid.EntityTypeGuid( "CACA75F2-29FE-4A52-AD4D-22E7314A35EB" )]
    public class GroupLinkSelect : DataSelectComponent
    {
        /// <summary>
        /// Gets the name of the entity type. Filter should be an empty string
        /// if it applies to all entities
        /// </summary>
        /// <value>
        /// The name of the entity type.
        /// </value>
        public override string AppliesToEntityType
        {
            get
            {
                return typeof( Rock.Model.Group ).FullName;
            }
        }

        /// <summary>
        /// The PropertyName of the property in the anonymous class returned by the SelectExpression
        /// </summary>
        /// <value>
        /// The name of the column property.
        /// </value>
        public override string ColumnPropertyName
        {
            get
            {
                return "Name";
            }
        }

        /// <summary>
        /// Gets the type of the column field.
        /// </summary>
        /// <value>
        /// The type of the column field.
        /// </value>
        public override Type ColumnFieldType
        {
            get
            {
                return typeof( string );
            }
        }

        /// <summary>
        /// Gets the default column header text.
        /// </summary>
        /// <value>
        /// The default column header text.
        /// </value>
        public override string ColumnHeaderText
        {
            get
            {
                return "Name";
            }
        }

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        /// </value>
        public override string GetTitle( Type entityType )
        {
            return "Name Link";
        }

        /// <summary>
        /// Gets the section that this will appear in in the Field Selector
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get
            {
                return "Common";
            }
        }

        #region Configuration

        /// <inheritdoc/>
        public override DynamicComponentDefinitionBag GetComponentDefinition( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Reporting/DataSelects/Group/groupLinkSelect.obs" )
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var settings = selection.FromJsonOrNull<Dictionary<string, string>>() ?? new Dictionary<string, string>();

            return new Dictionary<string, string>
            {
                ["showAsLink"] = settings.GetValueOrDefault( "ShowAsLink", string.Empty ).AsBooleanOrNull().ToStringSafe()
            };
        }

        /// <inheritdoc/>
        public override string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            return new Dictionary<string, string>
            {
                ["ShowAsLink"] = data.GetValueOrDefault( "showAsLink", string.Empty ).AsBooleanOrNull().ToStringSafe()
            }.ToJson();
        }

        #endregion
#if WEBFORMS
        /// <summary>
        /// Gets the grid field.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override System.Web.UI.WebControls.DataControlField GetGridField( Type entityType, string selection )
        {
            var result = new BoundField();
            result.HtmlEncode = false;
            return result;
        }
#endif

        /// <summary>
        /// Comma-delimited list of the Entity properties that should be used for Sorting. Normally, you should leave this as null which will make it sort on the returned field
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        /// <value>
        /// The sort expression.
        /// </value>
        public override string SortProperties( string selection )
        {
            return "Name";
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override System.Linq.Expressions.Expression GetExpression( Data.RockContext context, System.Linq.Expressions.MemberExpression entityIdProperty, string selection )
        {
            bool showAsLink = this.GetAttributeValueFromSelection( "ShowAsLink", selection ).AsBooleanOrNull() ?? false;
            var groupQry = new GroupService( context ).Queryable();

            IQueryable<string> groupLinkQry;
            string baseGroupUrl = System.Web.VirtualPathUtility.ToAbsolute( "~/Group/" );

            if ( showAsLink )
            {
                // return string in format: <a href='/group/{groupId}'>Name</a>
                groupLinkQry = groupQry.Select( p => "<a href='" + baseGroupUrl + p.Id.ToString() + "'>" + p.Name + "</a>" );
            }
            else
            {
                groupLinkQry = groupQry.Select( p => p.Name );
            }

            return SelectExpressionExtractor.Extract( groupLinkQry, entityIdProperty, "p" );
        }
    }
}
