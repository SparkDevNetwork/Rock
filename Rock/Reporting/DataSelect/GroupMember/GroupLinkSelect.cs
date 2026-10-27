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
using System.Linq;
using System.Web.UI.WebControls;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.ViewModels.Controls;

namespace Rock.Reporting.DataSelect.GroupMember
{
    /// <summary>
    /// A tabular report field that displays the name of a Group as a hyperlink.
    /// </summary>
    [Description( "Show Group Name" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select Group Name" )]
    [BooleanField( "Show As Link", "", true )]
    [Rock.SystemGuid.EntityTypeGuid( "9CF6163A-6E6A-47F3-B9E3-4558E75B1B9F" )]
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
                return typeof( Rock.Model.GroupMember ).FullName;
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
                return "Group Name";
            }
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
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Reporting/DataSelects/GroupMember/groupLinkSelect.obs" )
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

        /// <summary>
        /// Gets the grid field.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override System.Web.UI.WebControls.DataControlField GetGridField( Type entityType, string selection )
        {
            bool showAsLink = this.GetAttributeValueFromSelection( "ShowAsLink", selection ).AsBooleanOrNull() ?? false;

            /*
                10/27/26 - JMH

                When the value is a link, the markup (and the encoded name inside it) is built in
                GetExpression, so the grid must not encode it again. Otherwise the value is the raw
                name, so the grid encodes it.

                Reason: Display group names as text in reports.
            */
            var result = new BoundField();
            result.HtmlEncode = !showAsLink;

            return result;
        }

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
            return "Group.Name";
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

            var memberQuery = new GroupMemberService( context ).Queryable();

            IQueryable<string> groupLinkQuery;

            if ( showAsLink )
            {
                /*
                    10/27/26 - JMH

                    The link markup is built in SQL, so the name is HTML encoded there (with REPLACE)
                    before it is placed inside the anchor, so the grid does not encode it again.
                    Only &, < and > are encoded because the name is element content and the href is built from the Id only.

                    Reason: Display group names as text in reports.
                */
                // Return a string in the format: <a href='/group/{groupId}'>Group Name</a>
                groupLinkQuery = memberQuery.Select( gm => "<a href='/group/" + gm.GroupId.ToString() + "'>" + gm.Group.Name.Replace( "&", "&amp;" ).Replace( "<", "&lt;" ).Replace( ">", "&gt;" ) + "</a>" );
            }
            else
            {
                groupLinkQuery = memberQuery.Select( gm => gm.Group.Name );
            }

            var exp = SelectExpressionExtractor.Extract( groupLinkQuery, entityIdProperty, "gm" );

            return exp;
        }
    }
}
