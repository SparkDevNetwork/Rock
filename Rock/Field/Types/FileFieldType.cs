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
using System.Web.UI;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to display or upload a new binary file of a specific type
    /// Stored as BinaryFile.Guid
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    public class FileFieldType : BinaryFileFieldType
    {

        #region Edit Control 

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

        #endregion

    }
}