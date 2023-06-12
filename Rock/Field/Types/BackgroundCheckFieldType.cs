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
using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.ViewModels.Utility;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to display or upload a new binary file of a specific type.
    /// Stored as BinaryFile.Guid.
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.BACKGROUNDCHECK )]
    public class BackgroundCheckFieldType : BinaryFileFieldType, IEntityReferenceFieldType
    {
        private const string BINARY_FILE_TYPE = "binaryFileType";

        #region Edit Control

        /// <inheritdoc />
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc />
        public override Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues )
        {
            var privateConfigurationValues = base.GetPrivateConfigurationValues( publicConfigurationValues );

            if ( privateConfigurationValues.ContainsKey( BINARY_FILE_TYPE ) )
            {
                var binaryFileTypeValue = publicConfigurationValues[BINARY_FILE_TYPE].FromJsonOrNull<ListItemBag>();

                if ( binaryFileTypeValue != null )
                {
                    privateConfigurationValues[BINARY_FILE_TYPE] = binaryFileTypeValue.Value;
                }
            }

            return privateConfigurationValues;
        }

        /// <inheritdoc />
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string value )
        {
            var publicConfigurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, value );

            if ( publicConfigurationValues.ContainsKey( BINARY_FILE_TYPE ) && Guid.TryParse( publicConfigurationValues[BINARY_FILE_TYPE], out Guid binaryFileTypeGuid ) )
            {
                publicConfigurationValues[BINARY_FILE_TYPE] = new ListItemBag()
                {
                    Text = BinaryFileTypeCache.Get( binaryFileTypeGuid )?.Name,
                    Value = binaryFileTypeGuid.ToString()
                }.ToCamelCaseJson( false, true );
            }

            return publicConfigurationValues;
        }

        /// <inheritdoc />
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( !string.IsNullOrWhiteSpace( privateValue ) )
            {
                if ( Guid.TryParse( privateValue, out Guid binaryFileGuid ) )
                {
                    var entityType = EntityTypeCache.Get( Rock.SystemGuid.EntityType.PROTECT_MY_MINISTRY_PROVIDER.AsGuid() );
                    return $"{entityType.Guid},{entityType.FriendlyName},{privateValue},{GetFileName( privateValue )}";
                }

                var valueSplit = privateValue.Split( ',' );
                if ( valueSplit?.Length == 2 )
                {
                    var entityTypeId = valueSplit[0];
                    var entityType = EntityTypeCache.Get( entityTypeId.AsInteger() );

                    if ( entityType != null )
                    {
                        return $"{entityType.Guid},{entityType.FriendlyName},{valueSplit[1]}";
                    }
                }
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( !string.IsNullOrWhiteSpace( publicValue ) )
            {
                var valueSplit = publicValue.Split( ',' );

                if ( valueSplit.Length > 2 && Guid.TryParse( valueSplit[0], out Guid entityTypeGuid ) )
                {
                    var entityType = EntityTypeCache.Get( entityTypeGuid );

                    if ( entityType != null )
                    {
                        if ( entityType.Guid == SystemGuid.EntityType.CHECKR_PROVIDER.AsGuid() )
                        {
                            return $"{entityType.Id},{valueSplit[2]}";
                        }
                        else
                        {
                            return $"{valueSplit[2]}";
                        }
                    }
                }
            }

            return publicValue;
        }

        #endregion

        #region Formatting

        /// <inheritdoc />
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetFileName( privateValue );
        }

        /// <inheritdoc />
        public override string GetHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            Guid? guid = privateValue.AsGuidOrNull();
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
                        var filePath = System.Web.VirtualPathUtility.ToAbsolute( "~/GetBackgroundCheck.ashx" );
                        return string.Format( "<a href='{0}?EntityTypeId={1}&RecordKey={2}' title='{3}' class='btn btn-xs btn-default'>View</a>", filePath, EntityTypeCache.Get( typeof( Security.BackgroundCheck.ProtectMyMinistry ) ).Id, binaryFileInfo.Guid, System.Web.HttpUtility.HtmlEncode( binaryFileInfo.FileName ) );
                    }
                }
            }
            else if ( privateValue != null )
            {
                var valueArray = privateValue.Split( ',' );
                if ( valueArray.Length == 2 )
                {
                    var filePath = System.Web.VirtualPathUtility.ToAbsolute( "~/GetBackgroundCheck.ashx" );
                    return string.Format( "<a href='{0}?EntityTypeId={1}&RecordKey={2}' title='{3}' class='btn btn-xs btn-default'>View</a>", filePath, valueArray[0], valueArray[1], "Report" );
                }
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override string GetCondensedHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetHtmlValue( privateValue, privateConfigurationValues );
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <param name="privateValue">The private value.</param>
        /// <returns></returns>
        private static string GetFileName( string privateValue )
        {
            Guid? guid = privateValue.AsGuidOrNull();
            if ( guid.HasValue && !guid.Value.IsEmpty() )
            {
                using ( var rockContext = new RockContext() )
                {
                    var fileName = new BinaryFileService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( f => f.Guid == guid.Value )
                    .Select( f => f.FileName )
                    .FirstOrDefault();

                    if ( fileName.IsNotNullOrWhiteSpace() )
                    {
                        return fileName;
                    }
                }
            }

            return string.Empty;
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            Guid? guid = privateValue.AsGuidOrNull();

            if ( !guid.HasValue )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var fileId = new BinaryFileService( rockContext ).GetId( guid.Value );

                if ( !fileId.HasValue )
                {
                    return null;
                }

                return new List<ReferencedEntity>
                {
                    new ReferencedEntity( EntityTypeCache.GetId<BinaryFile>().Value, fileId.Value )
                };
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<BinaryFile>().Value, nameof( BinaryFile.FileName ) ),
            };
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            return base.ConfigurationControls();
        }

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
                        Guid? binaryFileGuid = new BinaryFileService( rockContext ).Queryable().AsNoTracking().Where( a => a.Id == binaryFileId.Value ).Select( a => ( Guid? ) a.Guid ).FirstOrDefault();
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
                        binaryFileId = new BinaryFileService( rockContext ).Queryable().Where( a => a.Guid == binaryFileGuid.Value ).Select( a => ( int? ) a.Id ).FirstOrDefault();
                    }

                    backgroundCheckDocument.BinaryFileId = binaryFileId;
                }
                else
                {
                    backgroundCheckDocument.Text = value;
                }
            }
        }

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
            // For compatibility reasons condensed value is always the text value encoded for HTML.
            return !condensed
                ? GetHtmlValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )?.EncodeHtml();
        }

#endif
        #endregion
    }
}