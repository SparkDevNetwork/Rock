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

namespace Rock.ViewModels.Utility
{
    /// <summary>
    /// Describes the data sent to and from remote systems to allow editing
    /// of attributes (not values, the attributes themselves).
    /// </summary>
    public class PublicEditableAttributeBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the existing attribute. If
        /// this is a new attribute the value should be <c>null</c>.
        /// </summary>
        /// <value>The unique identifier of the existing attribute.</value>
        public Guid? Guid { get; set; }

        /// <summary>
        /// Gets or sets the name of the attribute.
        /// </summary>
        /// <value>The name of the attribute.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the key that identifies the attribute.
        /// </summary>
        /// <value>
        /// The key that identifies the attribute.
        /// </value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the abbreviated name of the attribute.
        /// </summary>
        /// <value>The abbreviated name of the attribute.</value>
        public string AbbreviatedName { get; set; }

        /// <summary>
        /// Gets or sets the description of the attribute. This is usually used
        /// in the help bubble when the edit control is rendered.
        /// </summary>
        /// <value>The description of the attribute.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the attribute is active.
        /// </summary>
        /// <value><c>true</c> if the attribute is active; otherwise, <c>false</c>.</value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the attribute is publicly visible.
        /// </summary>
        /// <value><c>true</c> if the attribute is publicly visible; otherwise, <c>false</c>.</value>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the attribute is required
        /// to be filled in when the edit control is rendered on a page.
        /// </summary>
        /// <value><c>true</c> if the attribute is required; otherwise, <c>false</c>.</value>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the attribute is shown on
        /// bulk update screens.
        /// </summary>
        /// <value><c>true</c> if the attribute is shown on bulk update screens; otherwise, <c>false</c>.</value>
        public bool IsShowOnBulk { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the attribute is shown on grids.
        /// </summary>
        /// <value><c>true</c> if the attribute is shown on grids; otherwise, <c>false</c>.</value>
        public bool IsShowInGrid { get; set; }

        /// <summary>
        /// Gets or sets the categories the attribute is associated with.
        /// </summary>
        /// <value>The categories the attribute is associated with.</value>
        public List<ListItemBag> Categories { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this attribute is used for analytics.
        /// </summary>
        /// <value><c>true</c> if this attribute is used for analytics.</value>
        public bool IsAnalytic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this attribute records analytic history.
        /// </summary>
        /// <value><c>true</c> if this attribute records analytic history.</value>
        public bool IsAnalyticHistory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this attribute should be made available
        /// when performing searches.
        /// </summary>
        /// <value><c>true</c> if this attribute should be made available when performing searches.</value>
        public bool IsAllowSearch { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether changes to this attribute's values should
        /// be saved for historical purposes.
        /// </summary>
        /// <value><c>true</c> if changes to this attribute's values should be saved.</value>
        public bool IsEnableHistory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this attribute is indexed by universal search.
        /// </summary>
        /// <value><c>true</c> if this attribute is indexed by universal search.</value>
        public bool IsIndexEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this attribute is a system
        /// attribute, which implies limited editing ability.
        /// </summary>
        /// <value><c>true</c> if this attribute is a system attribute.</value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets any HTML to be rendered before the attribute's edit control.
        /// </summary>
        /// <value>Any HTML to be rendered before the attribute's edit control.</value>
        public string PreHtml { get; set; }

        /// <summary>
        /// Gets or sets any HTML to be rendered after the attribute's edit control.
        /// </summary>
        /// <value>Any HTML to be rendered after the attribute's edit control.</value>
        public string PostHtml { get; set; }

        /// <summary>
        /// Gets or sets the field type unique identifier that defines the behavior
        /// of the attribute. A value of <c>null</c> is only valid when transmitted
        /// from the server to remote systems so that it can provide default values
        /// in other properties.
        /// </summary>
        /// <value>The field type unique identifier.</value>
        public Guid? FieldTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the field type unique identifier as it is known by the
        /// server. If this value is <c>null</c> then the value from the
        /// FieldTypeGuid property should be used instead. This is only used
        /// by special field types that use a shared client control but have
        /// distinct server features.
        /// </summary>
        /// <value>The server field type unique identifier.</value>
        public Guid? RealFieldTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the configuration values for the attribute.
        /// </summary>
        /// <value>The configuration values for the attribute.</value>
        public Dictionary<string, string> ConfigurationValues { get; set; }

        /// <summary>
        /// Gets or sets the default value of the attribute.
        /// </summary>
        /// <value>The default value of the attribute.</value>
        public string DefaultValue { get; set; }
    }
}
