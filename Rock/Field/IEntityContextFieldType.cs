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

using Rock.Data;

namespace Rock.Field
{
    /// <summary>
    /// Fields that want entity context (usually the entity the attribute is
    /// attached to) when they render the formatted values.
    /// </summary>
    internal interface IEntityContextFieldType
    {
        /// <summary>
        /// Formats the value into a user-friendly string of plain text.
        /// </summary>
        /// <param name="privateValue">The private (database) value.</param>
        /// <param name="entityTypeId">The identifier of the type of entity providing context to this attribute value, this is usually the entity the attribute is attached to.</param>
        /// <param name="entityId">The identifier of the entity providing context to this attribute value, this is usually the entity the attribute is attached to.</param>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns>A plain string of text.</returns>
        string GetTextValue( string privateValue, int entityTypeId, int entityId, Dictionary<string, string> privateConfigurationValues );

        /// <summary>
        /// Formats the value into a user-friendly string of plain text.
        /// </summary>
        /// <param name="privateValue">The private (database) value.</param>
        /// <param name="entity">The entity providing context to this attribute value, this is usually the entity the attribute is attached to.</param>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns>A plain string of text.</returns>
        string GetTextValue( string privateValue, IEntity entity, Dictionary<string, string> privateConfigurationValues );

        /// <summary>
        /// Formats the value into a string of HTML text that can be rendered
        /// on a web page.
        /// </summary>
        /// <param name="privateValue">The private (database) value.</param>
        /// <param name="entityTypeId">The identifier of the type of entity providing context to this attribute value, this is usually the entity the attribute is attached to.</param>
        /// <param name="entityId">The identifier of the entity providing context to this attribute value, this is usually the entity the attribute is attached to.</param>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns>A string of HTML text.</returns>
        string GetHtmlValue( string privateValue, int entityTypeId, int entityId, Dictionary<string, string> privateConfigurationValues );

        /// <summary>
        /// Formats the value into a string of HTML text that can be rendered
        /// on a web page.
        /// </summary>
        /// <param name="privateValue">The private (database) value.</param>
        /// <param name="entity">The entity providing context to this attribute value, this is usually the entity the attribute is attached to.</param>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns>A string of HTML text.</returns>
        string GetHtmlValue( string privateValue, IEntity entity, Dictionary<string, string> privateConfigurationValues );

        /// <summary>
        /// Formats the value into a condensed user-friendly string of plain text.
        /// This value will be used when space is limited.
        /// </summary>
        /// <param name="privateValue">The private (database) value.</param>
        /// <param name="entityTypeId">The identifier of the type of entity providing context to this attribute value, this is usually the entity the attribute is attached to.</param>
        /// <param name="entityId">The identifier of the entity providing context to this attribute value, this is usually the entity the attribute is attached to.</param>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns>A plain string of text.</returns>
        string GetCondensedTextValue( string privateValue, int entityTypeId, int entityId, Dictionary<string, string> privateConfigurationValues );

        /// <summary>
        /// Formats the value into a condensed user-friendly string of plain text.
        /// This value will be used when space is limited.
        /// </summary>
        /// <param name="privateValue">The private (database) value.</param>
        /// <param name="entity">The entity providing context to this attribute value, this is usually the entity the attribute is attached to.</param>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns>A plain string of text.</returns>
        string GetCondensedTextValue( string privateValue, IEntity entity, Dictionary<string, string> privateConfigurationValues );

        /// <summary>
        /// Formats the value into a string of HTML text that can be rendered
        /// on a web page. This value will be used when space is limited.
        /// </summary>
        /// <param name="privateValue">The private (database) value.</param>
        /// <param name="entityTypeId">The identifier of the type of entity providing context to this attribute value, this is usually the entity the attribute is attached to.</param>
        /// <param name="entityId">The identifier of the entity providing context to this attribute value, this is usually the entity the attribute is attached to.</param>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns>A string of HTML text.</returns>
        string GetCondensedHtmlValue( string privateValue, int entityTypeId, int entityId, Dictionary<string, string> privateConfigurationValues );

        /// <summary>
        /// Formats the value into a string of HTML text that can be rendered
        /// on a web page. This value will be used when space is limited.
        /// </summary>
        /// <param name="privateValue">The private (database) value.</param>
        /// <param name="entity">The entity providing context to this attribute value, this is usually the entity the attribute is attached to.</param>
        /// <param name="privateConfigurationValues">The private (database) configuration values.</param>
        /// <returns>A string of HTML text.</returns>
        string GetCondensedHtmlValue( string privateValue, IEntity entity, Dictionary<string, string> privateConfigurationValues );
    }
}
