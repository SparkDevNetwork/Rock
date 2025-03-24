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
using System.Data.Entity;
using System.Linq;
#if WEBFORMS
using System.Web.UI;
#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to display or upload a new binary file of a specific type
    /// Stored as BinaryFile.Guid
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.Advanced )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [IconSvg( @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><path d=""M13.47,9.75a.66.66,0,0,1,.66.66v2A2.63,2.63,0,0,1,11.5,15h-7a2.63,2.63,0,0,1-2.63-2.63v-2a.66.66,0,0,1,1.32,0v2A1.32,1.32,0,0,0,4.5,13.69h7a1.32,1.32,0,0,0,1.31-1.32v-2A.66.66,0,0,1,13.47,9.75ZM7.55,1.18,3.83,4.65a.69.69,0,0,0,0,.95.65.65,0,0,0,.93,0L7.34,3.17V10a.66.66,0,0,0,1.32,0V3.17l2.61,2.46a.65.65,0,0,0,.93,0,.63.63,0,0,0,.18-.45.64.64,0,0,0-.21-.47L8.45,1.2A.63.63,0,0,0,7.55,1.18Z""/></svg>" )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.FILE )]
    public class FileFieldType : BinaryFileFieldType
    {

        #region Edit Control

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
            var control = new Rock.Web.UI.Controls.FileUploader { ID = id };

            if ( configurationValues != null && configurationValues.ContainsKey( "binaryFileType" ) )
            {
                control.BinaryFileTypeGuid = configurationValues["binaryFileType"].Value.AsGuid();
            }

            return control;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var fileUploader = control as Rock.Web.UI.Controls.FileUploader;
            if ( fileUploader != null )
            {
                int? binaryFileId = fileUploader.BinaryFileId;
                Guid? binaryFileGuid = null;
                if ( binaryFileId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        binaryFileGuid = new BinaryFileService( rockContext ).Queryable().AsNoTracking().Where( a => a.Id == binaryFileId.Value ).Select( a => ( Guid? ) a.Guid ).FirstOrDefault();
                    }
                }

                return binaryFileGuid?.ToString() ?? string.Empty;
            }

            return null;
        }

        /// <summary>
        /// Sets the value where value is BinaryFile.Guid as a string (or null)
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var fileUploader = control as Rock.Web.UI.Controls.FileUploader;
            if ( fileUploader != null )
            {
                int? binaryFileId = null;
                Guid? binaryFileGuid = value.AsGuidOrNull();
                if ( binaryFileGuid.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        binaryFileId = new BinaryFileService( rockContext ).Queryable().Where( a => a.Guid == binaryFileGuid.Value ).Select( a => ( int? ) a.Id ).FirstOrDefault();
                    }
                }

                fileUploader.BinaryFileId = binaryFileId;
            }
        }

#endif
        #endregion
    }
}