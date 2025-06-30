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
    /// Stored as:
    ///    * BinaryFile.Guid              (for Protect My Ministry, aka PMM)
    ///    * EntityTypeId,RecordKey       (for Checkr)
    ///    * EntityTypeId,BinaryFile.Guid (for other providers)
    ///
    /// Rock's core <b>Background Check Document</b> person attribute—along with this
    /// <b>Background Check FieldType</b>—was originally designed to support only Protect My Ministry
    /// and Checkr. However, other providers can integrate with this system by following a specific
    /// format when saving attribute values.
    ///
    /// To use the core attribute and field type with other third-party providers, the value saved into
    /// the Background Check Document attribute must include:
    /// <list type="bullet">
    /// <item>
    /// The <c>EntityTypeId</c> of the provider's <see cref="BackgroundCheckComponent"/> implementation.
    /// </item>
    /// <item>
    /// The <c>BinaryFile.Guid</c> of the resulting background check document.
    /// </item>
    /// </list>
    /// These two values should be comma-separated, e.g., <c>2240,61c0440a-3524-4528-9a56-327deaff5f4a</c>.
    ///
    /// <para>
    /// The system will use the provided EntityTypeId to identify the correct provider when
    /// accessing the stored document.
    /// </para>
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.Administrative )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.BACKGROUNDCHECK )]
    public class BackgroundCheckFieldType : BinaryFileFieldType, IEntityReferenceFieldType
    {
        private const string FILE_PATH = "filePath";
        private const string BINARY_FILE_TYPE = "binaryFileType";
        private const string ORIGINAL_PROVIDER_ENTITY_TYPE_GUID = "originalProviderEntityTypeGuid";

        #region Edit Control

        /// <inheritdoc />
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            //return GetTextValue( privateValue, privateConfigurationValues );
            var internalValue = ParseForPublicValue( privateValue );
            if ( internalValue == null )
            {
                return string.Empty;
            }

            return internalValue.ToCamelCaseJson( false, true );
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
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string privateValue )
        {
            var publicConfigurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, privateValue );

            var internalValue = ParseForPublicValue( privateValue );
            if ( internalValue != null )
            {
                publicConfigurationValues[ORIGINAL_PROVIDER_ENTITY_TYPE_GUID] = internalValue.ProviderEntityTypeGuid.ToString();
            }

            publicConfigurationValues[FILE_PATH] = System.Web.VirtualPathUtility.ToAbsolute( "~/GetBackgroundCheck.ashx" );

            // There must be a case where the publicConfigurationValues[BINARY_FILE_TYPE] is a simple Guid, and in this case
            // we need to turn it into a a ListItemBag.
            if ( usage != ConfigurationValueUsage.View && publicConfigurationValues.ContainsKey( BINARY_FILE_TYPE ) && Guid.TryParse( publicConfigurationValues[BINARY_FILE_TYPE], out Guid binaryFileTypeGuid ) )
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
            if ( string.IsNullOrWhiteSpace( privateValue ) )
            {
                return string.Empty;
            }

            var internalValue = ParseForPublicValue( privateValue );
            if ( internalValue == null )
            {
                return string.Empty;
            }

            // If the value is a single guid, the value we'll return to the editor will include the:
            //    {Provider EntityType's Guid},{Providers Name},{BinaryFileGuid},{filename (if any)}
            if ( internalValue.IsLegacyProtectMyMinistry )
            {
                return $"{internalValue.ProviderEntityTypeGuid},{internalValue.ProviderName},{privateValue},{internalValue.FileName}";
            }
            else
            {
                // If the value is a comma delimited string of <int>,<key|guid>, the value we'll return
                // to the editor will include the:
                //    {Provider EntityType's Guid},{Providers Name},{Remote Record Key|BinaryFileGuid},{filename (if any)}
                if ( internalValue.IsFileBased )
                {
                    return $"{internalValue.ProviderEntityTypeGuid},{internalValue.ProviderName},{internalValue.BinaryFileGuid},{internalValue.FileName}";
                }
                else
                {
                    return $"{internalValue.ProviderEntityTypeGuid},{internalValue.ProviderName},{internalValue.RecordKey},{string.Empty}";
                }
            }
        }

        /// <inheritdoc />
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( string.IsNullOrWhiteSpace( publicValue ) )
            {
                return publicValue;
            }

            // The public value we're given will be in the format of:
            //    {Provider EntityType's Guid},{Providers Name},{Remote Record Key|BinaryFileGuid},{filename (if any)}
            var valueSplit = publicValue.Split( ',' );

            if ( valueSplit.Length <= 2 || !Guid.TryParse( valueSplit[0], out Guid entityTypeGuid ) )
            {
                return publicValue;
            }

            var entityType = EntityTypeCache.Get( entityTypeGuid );
            if ( entityType == null )
            {
                return publicValue;
            }

            // All providers except PMM must include EntityTypeId
            return entityType.Guid == SystemGuid.EntityType.PROTECT_MY_MINISTRY_PROVIDER.AsGuid()
                ? valueSplit[2]
                : $"{entityType.Id},{valueSplit[2]}";
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
            var internalValue = ParseForPublicValue( privateValue );
            if ( internalValue == null )
            {
                return string.Empty;
            }

            var filePath = System.Web.VirtualPathUtility.ToAbsolute( "~/GetBackgroundCheck.ashx" );

            if ( internalValue.IsFileBased )
            {
                return $"<a href='{filePath}?EntityTypeId={internalValue.ProviderEntityTypeId}&RecordKey={internalValue.BinaryFileGuid}' title='{HtmlEncodeFileName( internalValue.FileName )}' class='btn btn-xs btn-default'>View</a>";
            }
            else
            {
                return $"<a href='{filePath}?EntityTypeId={internalValue.ProviderEntityTypeId}&RecordKey={internalValue.RecordKey}' title='Report' class='btn btn-xs btn-default'>View</a>";
            }
        }

        /// <inheritdoc />
        public override string GetCondensedHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetHtmlValue( privateValue, privateConfigurationValues );
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <param name="binaryFileGuid">The binary file guid.</param>
        /// <returns></returns>
        private static string GetFileName( Guid? binaryFileGuid )
        {
            if ( binaryFileGuid.HasValue && !binaryFileGuid.Value.IsEmpty() )
            {
                using ( var rockContext = new RockContext() )
                {
                    var fileName = new BinaryFileService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( f => f.Guid == binaryFileGuid.Value )
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

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <param name="privateValue">The private value.</param>
        /// <returns></returns>
        private static string GetFileName( string privateValue )
        {
            var parts = ( privateValue ?? "" ).Split( ',' );
            Guid? guid = privateValue.AsGuidOrNull() ?? ( parts.Length > 1 ? parts[1].AsGuidOrNull() : null );

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

        /// <summary>
        /// Parses the internal, private value into an object that is easier to deal with; hiding
        /// the splitting/parsing into this one method.  This will return null if the given value is not valid.
        /// </summary>
        /// <param name="privateValue">The internally stored internal value for this field type.</param>
        /// <returns>A value suitable for public usage or null if the private value is not valid or empty .</returns>
        private static PublicValueItem ParseForPublicValue( string privateValue )
        {
            if ( string.IsNullOrWhiteSpace( privateValue ) )
            {
                return null;
            }

            var internalValue = new PublicValueItem();

            // If the value is a single guid, the privateValue is just the BinaryFileGuid and
            // the provider is the PMM legacy provider.
            if ( Guid.TryParse( privateValue, out Guid binaryFileGuid ) )
            {
                var entityType = EntityTypeCache.Get( Rock.SystemGuid.EntityType.PROTECT_MY_MINISTRY_PROVIDER.AsGuid() );
                internalValue.BinaryFileGuid = binaryFileGuid;
                internalValue.ProviderEntityTypeGuid = entityType.Guid;
                internalValue.ProviderEntityTypeId = entityType.Id;
                internalValue.ProviderName = entityType.FriendlyName;
                internalValue.FileName = GetFileName( binaryFileGuid );

                return internalValue;
            }

            // If the value is a comma delimited string of <int>,<key|guid>, the int is the provider's
            // EntityTypeId and the key|guid is either the remote record key (Checkr) or the BinaryFileGuid (other providers).
            var valueSplit = privateValue.Split( ',' );
            if ( valueSplit?.Length != 2 )
            {
                // Anything other than two parts are not a valid value
                return null;
            }

            var entityTypeId = valueSplit[0];
            var recordKeyOrGuid = valueSplit[1];
            var entityTypeParsed = EntityTypeCache.Get( entityTypeId.AsInteger() );

            // An unknown entity type / provider? Not legal.
            if ( entityTypeParsed == null )
            {
                return null;
            }
            
            internalValue.ProviderEntityTypeId = entityTypeParsed.Id;
            internalValue.ProviderEntityTypeGuid = entityTypeParsed.Guid;
            internalValue.ProviderName = entityTypeParsed.FriendlyName;

            if ( Guid.TryParse( recordKeyOrGuid, out binaryFileGuid ) )
            {
                internalValue.BinaryFileGuid = binaryFileGuid;
                internalValue.FileName = GetFileName( binaryFileGuid );
            }
            else
            {
                internalValue.FileName = "Report";
                internalValue.RecordKey = recordKeyOrGuid;
            }

            return internalValue;
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

            bool includeInactive = true;
            var control = new BackgroundCheckDocument( includeInactive )
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
                    string binaryFileGuidString = string.Empty;
                    using ( var rockContext = new RockContext() )
                    {
                        Guid? binaryFileGuid = new BinaryFileService( rockContext ).Queryable().AsNoTracking().Where( a => a.Id == binaryFileId.Value ).Select( a => ( Guid? ) a.Guid ).FirstOrDefault();
                        if ( binaryFileGuid.HasValue )
                        {
                            binaryFileGuidString = binaryFileGuid?.ToString();
                        }
                    }

                    // Only the legacy PMM provider can store only the Guid,
                    // everything else must store the <provider EntityTypeId>,<Binary File Guid|RecordKey>
                    Guid? entityTypeGuid = backgroundCheckDocument.ProviderEntityTypeGuid;
                    if ( entityTypeGuid.HasValue && entityTypeGuid.Value == Rock.SystemGuid.EntityType.PROTECT_MY_MINISTRY_PROVIDER.AsGuid() )
                    {
                        return binaryFileGuidString;
                    }
                    else if ( entityTypeGuid.HasValue )
                    {
                        var entityTypeId = EntityTypeCache.Get( entityTypeGuid.Value ).Id;
                        return $"{entityTypeId},{binaryFileGuidString}";
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
        /// Sets the value into the corresponding properties of the BackgroundCheckDocument control.
        /// For PMM, the backgroundCheckDocument.BinaryFileId is set.
        /// For Checkr, backgroundCheckDocument.Text is set to the full stored value because the
        /// control knows how to split it.
        /// For other providers, the backgroundCheckDocument.BinaryFileId and the
        /// backgroundCheckDocument.ProviderEntityTypeGuid are set.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var backgroundCheckDocument = control as BackgroundCheckDocument;
            if ( backgroundCheckDocument == null )
            {
                return;
            }

            // Legacy PMM Background Check Documents are stored with only the Guid.
            Guid? binaryFileGuid = value.AsGuidOrNull();
            if ( binaryFileGuid.HasValue )
            {
                int? binaryFileId = null;
                using ( var rockContext = new RockContext() )
                {
                    binaryFileId = new BinaryFileService( rockContext )
                        .Queryable()
                        .Where( f => f.Guid == binaryFileGuid )
                        .Select( f => ( int? ) f.Id )
                        .FirstOrDefault();
                }

                backgroundCheckDocument.BinaryFileId = binaryFileId;
                backgroundCheckDocument.ProviderEntityTypeGuid = Rock.SystemGuid.EntityType.PROTECT_MY_MINISTRY_PROVIDER.AsGuid();
                return;
            }

            var parts = value.Split( ',' );
            if ( parts == null || parts.Length != 2 )
            {
                return;
            }

            var providerEntityTypeId = parts[0].AsInteger();
            var providerEntityType = EntityTypeCache.Get( providerEntityTypeId );

            // At this point, we must have a provider component to continue.
            if ( providerEntityType == null )
            {
                return;
            }

            // If it's Checkr, then we just put the value in the backgroundCheckDocument's Text property.
            if ( providerEntityType.Guid == Rock.SystemGuid.EntityType.CHECKR_PROVIDER.AsGuid() )
            {
                backgroundCheckDocument.Text = value;
                return;
            }

            // Otherwise, we assume it's an 'other' provider and set the BinaryFileId and ProviderEntityTypeGuid.
            var binaryFileGuidFromValue = parts[1].AsGuidOrNull();
            if ( binaryFileGuidFromValue.HasValue )
            {
                int? binaryFileId = null;
                using ( var rockContext = new RockContext() )
                {
                    binaryFileId = new BinaryFileService( rockContext )
                        .Queryable()
                        .Where( f => f.Guid == binaryFileGuidFromValue.Value )
                        .Select( f => ( int? ) f.Id )
                        .FirstOrDefault();
                }

                backgroundCheckDocument.BinaryFileId = binaryFileId;
                backgroundCheckDocument.ProviderEntityTypeGuid = providerEntityType.Guid;
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

        private string HtmlEncodeFileName( string fileName )
        {
            return System.Web.HttpUtility.HtmlEncode( fileName );
        }

#endif
        #endregion
        private class PublicValueItem
        {
            public bool IsLegacyProtectMyMinistry { get; set; } = false;
            public bool IsFileBased => BinaryFileGuid.HasValue;
            public string ProviderName { get; set; }
            public Guid? BinaryFileGuid { get; set; }
            public string FileName { get; set; }
            public string RecordKey { get; set; }
            public Guid? ProviderEntityTypeGuid { get; set; }
            public int ProviderEntityTypeId { get; set; }
        }
    }
}