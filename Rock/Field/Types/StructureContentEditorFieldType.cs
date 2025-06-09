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
#if WEBFORMS
using System.Web.UI;
#endif
using Rock.Attribute;
using Rock.Cms.StructuredContent;
using Rock.Reporting;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field type to encapsulate a structured content editor which allows
    /// the individual a nice UI interface to editing content.
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.Advanced )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.STRUCTURE_CONTENT_EDITOR )]
    [IconSvg( @"<svg viewBox=""0 0 16 16"" xmlns=""http://www.w3.org/2000/svg""><path d=""M1.525 9.25H7.475C7.76375 9.25 8 8.96875 8 8.625V3.625C8 3.28125 7.76375 3 7.475 3H1.525C1.23625 3 1 3.28125 1 3.625V8.625C1 8.96875 1.23625 9.25 1.525 9.25ZM2.75 5.08333H6.25V7.16667H2.75V5.08333ZM1 12.375V11.125C1 10.7813 1.23625 10.5 1.525 10.5H7.475C7.76375 10.5 8 10.7813 8 11.125V12.375C8 12.7188 7.76375 13 7.475 13H1.525C1.23625 13 1 12.7188 1 12.375ZM9.4 12.375V11.125C9.4 10.7813 9.63625 10.5 9.925 10.5H14.475C14.7637 10.5 15 10.7813 15 11.125V12.375C15 12.7188 14.7637 13 14.475 13H9.925C9.63625 13 9.4 12.7188 9.4 12.375ZM9.4 4.875V3.625C9.4 3.28125 9.63625 3 9.925 3H14.475C14.7637 3 15 3.28125 15 3.625V4.875C15 5.21875 14.7637 5.5 14.475 5.5H9.925C9.63625 5.5 9.4 5.21875 9.4 4.875ZM9.4 8.625V7.375C9.4 7.03125 9.63625 6.75 9.925 6.75H14.475C14.7637 6.75 15 7.03125 15 7.375V8.625C15 8.96875 14.7637 9.25 14.475 9.25H9.925C9.63625 9.25 9.4 8.96875 9.4 8.625Z"" /></svg>" )]
    public class StructureContentEditorFieldType : FieldType
    {
        #region Edit Control

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( publicValue.IsNullOrWhiteSpace() )
            {
                return "{}";
            }

            return base.GetPrivateEditValue( publicValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( privateValue.IsNullOrWhiteSpace() )
            {
                return "{}";
            }

            return privateValue;
        }

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var helper = new StructuredContentHelper( privateValue );

            return helper.Render();
        }

        /// <inheritdoc/>
        public override string GetHtmlValue( string value, Dictionary<string, string> configurationValues )
        {
            var helper = new StructuredContentHelper( value );

            return helper.Render();
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var editor = new StructureContentEditor { ID = id };

            return editor;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var structureContentEditor = control as StructureContentEditor;
            if ( structureContentEditor != null )
            {
                return structureContentEditor.StructuredContent;
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var structureContentEditor = control as StructureContentEditor;
            if ( structureContentEditor != null )
            {
                structureContentEditor.StructuredContent = value;
            }
        }

        /// <inheritdoc/>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return GetHtmlValue( value, configurationValues.ToDictionary( k => k.Key, k => k.Value.Value ) );
        }

        /// <summary>
        /// Formats the value as HTML.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public override string FormatValueAsHtml( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            return GetHtmlValue( value, configurationValues.ToDictionary( k => k.Key, k => k.Value.Value ) );
        }

        /// <summary>
        /// Formats the value as HTML.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public override string FormatValueAsHtml( Control parentControl, int? entityTypeId, int? entityId, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            return GetHtmlValue( value, configurationValues.ToDictionary( k => k.Key, k => k.Value.Value ) );
        }

#endif
        #endregion
    }
}