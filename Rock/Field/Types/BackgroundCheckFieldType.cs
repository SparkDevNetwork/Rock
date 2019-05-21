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

using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to display or upload a new binary file of a specific type.
    /// Stored as BinaryFile.Guid.
    /// </summary>
    public class BackgroundCheckFieldType : BinaryFileFieldType
    {
        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            return base.ConfigurationControls();
        }



        #region Edit Control
        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            Guid? binaryFileTypeGuid = null;
            if ( configurationValues != null && configurationValues.ContainsKey( "binaryFileType" ) )
            {
                binaryFileTypeGuid = configurationValues["binaryFileType"].Value.AsGuid();
            }

            var control = new BackgroundCheckDocument()
            {
                ID = id,
                BinaryFileTypeGuid = binaryFileTypeGuid
            };

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
            var backgroundCheckDocument = control as BackgroundCheckDocument;
            if ( backgroundCheckDocument != null )
            {
                int? binaryFileId = backgroundCheckDocument.BinaryFileId;
                if ( binaryFileId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        Guid? binaryFileGuid = new BinaryFileService( rockContext ).Queryable().AsNoTracking().Where( a => a.Id == binaryFileId.Value ).Select( a => (Guid?)a.Guid ).FirstOrDefault();
                        if ( binaryFileGuid.HasValue )
                        {
                            return binaryFileGuid?.ToString();
                        }
                    }
                }
                else
                {
                    return backgroundCheckDocument.Text;
                }

                return string.Empty;
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
            var backgroundCheckDocument = control as BackgroundCheckDocument;
            if ( backgroundCheckDocument != null )
            {
                Guid? binaryFileGuid = value.AsGuidOrNull();
                if ( binaryFileGuid.HasValue )
                {
                    int? binaryFileId = null;
                    using ( var rockContext = new RockContext() )
                    {
                        binaryFileId = new BinaryFileService( rockContext ).Queryable().Where( a => a.Guid == binaryFileGuid.Value ).Select( a => (int?)a.Id ).FirstOrDefault();
                    }

                    backgroundCheckDocument.BinaryFileId = binaryFileId;
                }
                else
                {
                    backgroundCheckDocument.Text = value;
                }
            }
        }

        #endregion
        #region Formatting

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            string formattedValue = string.Empty;

            Guid? guid = value.AsGuidOrNull();
            if ( guid.HasValue && !guid.Value.IsEmpty() )
            {
                using ( var rockContext = new RockContext() )
                {

                    var binaryFileInfo = new BinaryFileService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( f => f.Guid == guid.Value )
                    .Select( f =>
                        new
                        {
                            f.Id,
                            f.FileName,
                            f.Guid
                        } )
                    .FirstOrDefault();

                    if ( binaryFileInfo != null )
                    {
                        if ( condensed )
                        {
                            return binaryFileInfo.FileName;
                        }
                        else
                        {
                            var filePath = System.Web.VirtualPathUtility.ToAbsolute( "~/GetBackgroundCheck.ashx" );
                            return string.Format( "<a href='{0}?EntityTypeId={1}&RecordKey={2}' title='{3}' class='btn btn-xs btn-default'>View</a>", filePath, EntityTypeCache.Get( typeof( Security.BackgroundCheck.ProtectMyMinistry ) ).Id, binaryFileInfo.Guid, System.Web.HttpUtility.HtmlEncode( binaryFileInfo.FileName ) );
                        }
                    }
                }
            }
            else if (value != null)
            {
                var valueArray = value.Split( ',' );
                if ( valueArray.Length == 2 )
                {
                    var filePath = System.Web.VirtualPathUtility.ToAbsolute( "~/GetBackgroundCheck.ashx" );
                    return string.Format( "<a href='{0}?EntityTypeId={1}&RecordKey={2}' title='{3}' class='btn btn-xs btn-default'>View</a>", filePath, valueArray[0], valueArray[1], "Report" );
                }
            }

            return base.FormatValue( parentControl, formattedValue, null, condensed );
        }

        #endregion
    }
}