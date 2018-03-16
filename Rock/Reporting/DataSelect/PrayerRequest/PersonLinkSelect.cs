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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI.WebControls;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Reporting.DataSelect.PrayerRequest
{
    /// <summary>
    /// Report Field for Group Member Person.
    /// </summary>
    [Description("Show person's name as an optional link that navigates to the person's record")]
    [Export(typeof(DataSelectComponent))]
    [ExportMetadata("ComponentName", "Select Person Name")]
    [BooleanField("Show As Link", "", true)]
    [CustomRadioListField("Display Order", "", "0^FirstName LastName,1^LastName&#44; FirstName", true, "0")]
    public class PersonLinkSelect : DataSelectComponent, IRecipientDataSelect
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
                return typeof(Rock.Model.PrayerRequest).FullName;
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
                return "Person Name";
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

        /// <summary>
        /// Gets the grid field.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override System.Web.UI.WebControls.DataControlField GetGridField(Type entityType, string selection)
        {
            var result = new BoundField();

            // Disable encoding of field content because the value contains markup.
            result.HtmlEncode = false;

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
        public override string SortProperties(string selection)
        {
            int displayOrder = this.GetAttributeValueFromSelection("DisplayOrder", selection).AsIntegerOrNull() ?? 0;

            if (displayOrder == 0)
            {
                return "RequestedByPersonAlias.Person.NickName,RequestedByPersonAlias.Person.LastName";
            }
            else
            {
                return "RequestedByPersonAlias.Person.LastName,RequestedByPersonAlias.Person.NickName";
            }
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression(RockContext context, MemberExpression entityIdProperty, string selection)
        {
            bool showAsLink = this.GetAttributeValueFromSelection("ShowAsLink", selection).AsBooleanOrNull() ?? false;
            int displayOrder = this.GetAttributeValueFromSelection("DisplayOrder", selection).AsIntegerOrNull() ?? 0;

            var memberQuery = new PrayerRequestService(context).Queryable();

            IQueryable<string> personLinkQuery;

            if (showAsLink)
            {
                // Return a string in the format: <a href='/person/{personId}'>LastName, NickName</a>
                if (displayOrder == 0)
                {
                    personLinkQuery = memberQuery.Select(gm => "<a href='/person/" + gm.RequestedByPersonAlias.AliasPersonId.ToString() + "'>" + gm.RequestedByPersonAlias.Person.NickName + " " + gm.RequestedByPersonAlias.Person.LastName + "</a>");
                }
                else
                {
                    personLinkQuery = memberQuery.Select(gm => "<a href='/person/" + gm.RequestedByPersonAlias.AliasPersonId.ToString() + "'>" + gm.RequestedByPersonAlias.Person.LastName + ", " + gm.RequestedByPersonAlias.Person.NickName + "</a>");
                }
            }
            else
            {
                if (displayOrder == 0)
                {
                    personLinkQuery = memberQuery.Select(gm => gm.RequestedByPersonAlias.Person.NickName + " " + gm.RequestedByPersonAlias.Person.LastName);
                }
                else
                {
                    personLinkQuery = memberQuery.Select(gm => gm.RequestedByPersonAlias.Person.LastName + ", " + gm.RequestedByPersonAlias.Person.NickName);
                }
            }

            var exp = SelectExpressionExtractor.Extract(personLinkQuery, entityIdProperty, "gm");

            return exp;
        }

        #region IRecipientDataSelect implementation

        /// <summary>
        /// Gets the type of the recipient column field.
        /// </summary>
        /// <value>
        /// The type of the recipient column field.
        /// </value>
        public Type RecipientColumnFieldType
        {
            get { return typeof(int); }
        }

        /// <summary>
        /// Gets the recipient person identifier expression.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public Expression GetRecipientPersonIdExpression(System.Data.Entity.DbContext dbContext, MemberExpression entityIdProperty, string selection)
        {
            var rockContext = dbContext as RockContext;
            if (rockContext != null)
            {
                var memberQuery = new PrayerRequestService(rockContext).Queryable().Select(p => p.RequestedByPersonAlias.AliasPersonId);
                var exp = SelectExpressionExtractor.Extract(memberQuery, entityIdProperty, "p");
                return exp;
            }

            return null;
        }

        #endregion
    }
}
