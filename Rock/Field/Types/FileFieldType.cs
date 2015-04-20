// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;
using System.Web.UI;

using Rock.Data;
using Rock.Model;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to display or upload a new binary file of a specific type
    /// Stored as BinaryFile.Guid
    /// </summary>
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
                var binaryFile = new BinaryFileService( new RockContext() ).Get( fileUploader.BinaryFileId ?? 0 );
                if ( binaryFile != null )
                {
                    return binaryFile.Guid.ToString();
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var fileUploader = control as Rock.Web.UI.Controls.FileUploader;
            if ( fileUploader != null )
            {
                var binaryFile = new BinaryFileService( new RockContext() ).Get( value.AsGuid() );
                if (binaryFile != null)
                {
                    fileUploader.BinaryFileId = binaryFile.Id; 
                }
            }
        }

        #endregion

    }
}