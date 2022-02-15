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
using System.Linq;
using System.Web.UI;

using Rock.Attribute;
using Rock.Data;
using Rock.Media;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) <see cref="MediaElement"/>.
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    public class MediaElementFieldType : FieldType, IEntityFieldType
    {
        #region Configuration

        /// <summary>
        /// Configuration Key for MediaElement Picker Label
        /// </summary>
        public static readonly string CONFIG_MEDIA_PICKER_LABEL = "mediaPickerLabel";

        /// <summary>
        /// Configuration Key for limiting selection to specific MediaAccount.
        /// </summary>
        public static readonly string CONFIG_LIMIT_TO_ACCOUNT = "limitToAccount";

        /// <summary>
        /// Configuration Key for limiting selection to specific MediaFolder.
        /// </summary>
        public static readonly string CONFIG_LIMIT_TO_FOLDER = "limitToFolder";

        /// <summary>
        /// Configuration Key for when to enable <see cref="RockDropDownList.EnhanceForLongLists"/>.
        /// </summary>
        public static readonly string CONFIG_ENHANCE_FOR_LONG_LISTS_THRESHOLD = "enhanceForLongListsThreshold";

        /// <summary>
        /// Configuration Key for when to enable <see cref="MediaElementPicker.IsRefreshAllowed"/>.
        /// </summary>
        public static readonly string CONFIG_ALLOW_REFRESH = "allowRefresh";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            return new List<string>
            {
                CONFIG_MEDIA_PICKER_LABEL,
                CONFIG_LIMIT_TO_ACCOUNT,
                CONFIG_LIMIT_TO_FOLDER,
                CONFIG_ENHANCE_FOR_LONG_LISTS_THRESHOLD,
                CONFIG_ALLOW_REFRESH
            };
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = new List<Control>();

            var tbMediaElementPickerLabel = new RockTextBox();
            tbMediaElementPickerLabel.Label = "Media Element Picker Label";
            tbMediaElementPickerLabel.Help = "The label for the media element picker";
            tbMediaElementPickerLabel.Text = "Media";
            tbMediaElementPickerLabel.AutoPostBack = true;
            tbMediaElementPickerLabel.TextChanged += OnQualifierUpdated;
            controls.Add( tbMediaElementPickerLabel );

            var nbEnhanceForLongListsThreshold = new NumberBox();
            nbEnhanceForLongListsThreshold.Label = "Enhance For Long Lists Threshold";
            nbEnhanceForLongListsThreshold.Help = "When the number of items exceed this value then the picker will turn on enhanced for long lists.";
            nbEnhanceForLongListsThreshold.Text = "20";
            nbEnhanceForLongListsThreshold.MinimumValue = "0";
            nbEnhanceForLongListsThreshold.AutoPostBack = true;
            nbEnhanceForLongListsThreshold.NumberType = System.Web.UI.WebControls.ValidationDataType.Integer;
            nbEnhanceForLongListsThreshold.TextChanged += OnQualifierUpdated;
            controls.Add( nbEnhanceForLongListsThreshold );

            var mpLimits = new MediaElementPicker();
            mpLimits.Label = "Limit To";
            mpLimits.Help = "Enforces the account or folder selections and hides them from the user.";
            mpLimits.ShowMediaPicker = false;
            controls.Add( mpLimits );

            var cbAllowRefresh = new RockCheckBox();
            cbAllowRefresh.Label = "Allow Refresh";
            cbAllowRefresh.Help = "If enabled the user will be allowed to request a refresh of the folders and media items.";
            cbAllowRefresh.Checked = true;
            cbAllowRefresh.AutoPostBack = true;
            cbAllowRefresh.CheckedChanged += OnQualifierUpdated;
            controls.Add( cbAllowRefresh );

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( CONFIG_MEDIA_PICKER_LABEL, new ConfigurationValue( "Media Element Picker Label", "The label for the media element picker.", "Media" ) );
            configurationValues.Add( CONFIG_LIMIT_TO_ACCOUNT, new ConfigurationValue( "Limit to Account", "The account to use when displaying the media element picker.", string.Empty ) );
            configurationValues.Add( CONFIG_LIMIT_TO_FOLDER, new ConfigurationValue( "Limit to Folder", "The folder to use when displaying the media element picker.", string.Empty ) );
            configurationValues.Add( CONFIG_ENHANCE_FOR_LONG_LISTS_THRESHOLD, new ConfigurationValue( "Enhance For Long Lists Threshold", "When the number of items exceed this value then the picker will turn on enhanced for long lists.", "20" ) );
            configurationValues.Add( CONFIG_ALLOW_REFRESH, new ConfigurationValue( "Allow Refresh", "If enabled the user will be allowed to request a refresh of the folders and media items.", "True" ) );

            if ( controls == null )
            {
                return configurationValues;
            }

            if ( controls.Count >= 1 && controls[0] is RockTextBox tbMediaElementPickerLabel )
            {
                configurationValues[CONFIG_MEDIA_PICKER_LABEL].Value = tbMediaElementPickerLabel.Text;
            }

            if ( controls.Count >= 2 && controls[1] is NumberBox nbEnhanceForLongListsThreshold )
            {
                configurationValues[CONFIG_ENHANCE_FOR_LONG_LISTS_THRESHOLD].Value = nbEnhanceForLongListsThreshold.IntegerValue.ToStringSafe();
            }

            if ( controls.Count >= 3 && controls[2] is MediaElementPicker mpLimits )
            {
                configurationValues[CONFIG_LIMIT_TO_ACCOUNT].Value = mpLimits.MediaAccountId.ToStringSafe();
                configurationValues[CONFIG_LIMIT_TO_FOLDER].Value = mpLimits.MediaFolderId.ToStringSafe();
            }

            if ( controls.Count >= 4 && controls[3] is RockCheckBox cbAllowRefresh )
            {
                configurationValues[CONFIG_ALLOW_REFRESH].Value = cbAllowRefresh.Checked.ToString();
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="configurationValues">The configuration values.</param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls == null || configurationValues == null )
            {
                return;
            }

            if ( controls.Count >= 1 && controls[0] is RockTextBox tbMediaElementPickerLabel )
            {
                if ( configurationValues?.ContainsKey( CONFIG_MEDIA_PICKER_LABEL ) == true )
                {
                    tbMediaElementPickerLabel.Text = configurationValues[CONFIG_MEDIA_PICKER_LABEL].Value ?? "Media";
                }
            }

            if ( controls.Count >= 2 && controls[1] is NumberBox nbEnhanceForLongListsThreshold )
            {
                if ( configurationValues?.ContainsKey( CONFIG_ENHANCE_FOR_LONG_LISTS_THRESHOLD ) == true )
                {
                    nbEnhanceForLongListsThreshold.Text = configurationValues[CONFIG_ENHANCE_FOR_LONG_LISTS_THRESHOLD].Value ?? "20";
                }
            }

            if ( controls.Count >= 3 && controls[2] is MediaElementPicker mpLimits )
            {
                if ( configurationValues?.ContainsKey( CONFIG_LIMIT_TO_ACCOUNT ) == true )
                {
                    mpLimits.MediaAccountId = configurationValues[CONFIG_LIMIT_TO_ACCOUNT].Value.AsIntegerOrNull();
                }

                if ( configurationValues?.ContainsKey( CONFIG_LIMIT_TO_FOLDER ) == true )
                {
                    mpLimits.MediaFolderId = configurationValues[CONFIG_LIMIT_TO_FOLDER].Value.AsIntegerOrNull();
                }
            }

            if ( controls.Count >= 4 && controls[3] is RockCheckBox cbAllowRefresh )
            {
                if ( configurationValues?.ContainsKey( CONFIG_ALLOW_REFRESH ) == true )
                {
                    cbAllowRefresh.Checked = configurationValues[CONFIG_ALLOW_REFRESH].Value.AsBoolean( true );
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
            return FormatValueAsHtml( parentControl, value, configurationValues, condensed );
        }

        /// <summary>
        /// Formats the value as HTML.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> then the value will be displayed in a condensed area; otherwise <c>false</c>.</param>
        /// <returns></returns>
        public override string FormatValueAsHtml( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            var mediaElementGuid = value.AsGuidOrNull();

            if ( !mediaElementGuid.HasValue )
            {
                return string.Empty;
            }

            using ( var rockContext = new RockContext() )
            {
                var mediaInfo = new MediaElementService( rockContext ).Queryable()
                    .Where( a => a.Guid == mediaElementGuid.Value )
                    .Select( a => new
                    {
                        a.Name,
                        a.ThumbnailDataJson
                    } )
                    .SingleOrDefault();

                if ( mediaInfo == null )
                {
                    return string.Empty;
                }

                var thumbnails = mediaInfo.ThumbnailDataJson.FromJsonOrNull<List<MediaElementThumbnailData>>();
                var thumbnailUrl = string.Empty;

                if ( thumbnails != null )
                {
                    if ( condensed )
                    {
                        // Attempt to get the smallest thumbnail above 400px in
                        // width. If that fails then just get the largest
                        // thumbnail we have available.
                        thumbnailUrl = thumbnails.Where( t => t.Link.IsNotNullOrWhiteSpace() && t.Width >= 400 )
                            .OrderBy( t => t.Height )
                            .Select( t => t.Link )
                            .FirstOrDefault() ?? string.Empty;

                        if ( thumbnailUrl == string.Empty )
                        {
                            thumbnailUrl = thumbnails.Where( t => t.Link.IsNotNullOrWhiteSpace() )
                                .OrderByDescending( t => t.Height )
                                .Select( t => t.Link )
                                .FirstOrDefault() ?? string.Empty;
                        }
                    }
                    else
                    {
                        thumbnailUrl = thumbnails.Where( t => t.Link.IsNotNullOrWhiteSpace() )
                            .OrderByDescending( t => t.Height )
                            .Select( t => t.Link )
                            .FirstOrDefault() ?? string.Empty;
                    }
                }

                if ( condensed )
                {
                    return $"<img src='{thumbnailUrl}' alt='{mediaInfo.Name.EncodeXml( true )}' class='img-responsive grid-img' />";
                }
                else
                {
                    return $"<img src='{thumbnailUrl}' alt='{mediaInfo.Name.EncodeXml( true )}' class='img-responsive' />";
                }
            }
        }

        #endregion

        #region Edit Control

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The id.</param>
        /// <returns>
        /// The control
        /// </returns>
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var editControl = new MediaElementPicker
            {
                ID = id,
                Label = "Media"
            };

            if ( configurationValues?.ContainsKey( CONFIG_MEDIA_PICKER_LABEL ) == true )
            {
                editControl.MediaElementLabel = configurationValues[CONFIG_MEDIA_PICKER_LABEL].Value;
            }

            if ( configurationValues?.ContainsKey( CONFIG_ALLOW_REFRESH ) == true )
            {
                editControl.IsRefreshAllowed = configurationValues[CONFIG_ALLOW_REFRESH].Value.AsBoolean( true );
            }

            if ( configurationValues?.ContainsKey( CONFIG_ALLOW_REFRESH ) == true )
            {
                editControl.EnhanceForLongListsThreshold = configurationValues[CONFIG_ENHANCE_FOR_LONG_LISTS_THRESHOLD].Value.AsIntegerOrNull() ?? 20;
            }

            SetAccountAndFolderValues( editControl, configurationValues );

            return editControl;
        }

        /// <summary>
        /// Reads new values entered by the user for the field ( as Guid )
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control is MediaElementPicker mediaElementPicker )
            {
                var rockContext = new RockContext();

                if ( mediaElementPicker.MediaElementId.IsNotNullOrZero() )
                {
                    var mediaElementGuid = new MediaElementService( rockContext ).Queryable()
                        .Where( a => a.Id == mediaElementPicker.MediaElementId.Value )
                        .Select( a => a.Guid )
                        .SingleOrDefault();

                    if ( mediaElementGuid != null )
                    {
                        return mediaElementGuid.ToString();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the value. ( as Guid )
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( !( control is MediaElementPicker mediaElementPicker ) )
            {
                return;
            }

            var mediaElementGuid = value.AsGuidOrNull();

            if ( mediaElementGuid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var mediaElementInfo = new MediaElementService( rockContext ).Queryable()
                        .Where( a => a.Guid == mediaElementGuid.Value )
                        .Select( a => new
                        {
                            a.Id,
                            a.MediaFolderId,
                            a.MediaFolder.MediaAccountId
                        } )
                        .SingleOrDefault();

                    if ( mediaElementInfo != null )
                    {
                        var limitAccountId = GetLimitToAccountId( configurationValues );
                        var limitFolderId = GetLimitToFolderId( configurationValues );

                        bool accountOk = limitAccountId.IsNullOrZero() || limitAccountId.Value == mediaElementInfo.MediaAccountId;
                        bool folderOk = limitFolderId.IsNullOrZero() || limitFolderId.Value == mediaElementInfo.MediaFolderId;

                        // This is a little odd, but here is the logic.
                        // If the admin has limited to a specific folder or account
                        // then we need to enforce that. Which means if the old
                        // value is for a different folder/account then we
                        // basically just don't set the value because they won't
                        // be able to save it anyway. Similar logic to a custom
                        // drop down list where the admin removes one of the
                        // options. The next time the value is edited it won't
                        // be selected anymore because it doesn't exist.
                        if ( accountOk && folderOk )
                        {
                            mediaElementPicker.MediaElementId = mediaElementInfo.Id;

                            return;
                        }
                    }
                }
            }

            // We couldn't find the stored value or there wasn't one, set
            // the defaults to nothing selected and ensure the forced
            // account and folder are still selected.
            mediaElementPicker.MediaElementId = null;
            SetAccountAndFolderValues( mediaElementPicker, configurationValues );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the limit to account identifier.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        private int? GetLimitToAccountId( Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( configurationValues?.ContainsKey( CONFIG_LIMIT_TO_ACCOUNT ) == true )
            {
                return configurationValues[CONFIG_LIMIT_TO_ACCOUNT].Value.AsIntegerOrNull();
            }

            return null;
        }

        /// <summary>
        /// Gets the limit to folder identifier.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        private int? GetLimitToFolderId( Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( configurationValues?.ContainsKey( CONFIG_LIMIT_TO_FOLDER ) == true )
            {
                return configurationValues[CONFIG_LIMIT_TO_FOLDER].Value.AsIntegerOrNull();
            }

            return null;
        }

        /// <summary>
        /// Sets the account and folder values if they were provided in the
        /// configuration.
        /// </summary>
        /// <param name="mediaElementPicker">The media element picker.</param>
        /// <param name="configurationValues">The configuration values.</param>
        private void SetAccountAndFolderValues( MediaElementPicker mediaElementPicker, Dictionary<string, ConfigurationValue> configurationValues )
        {
            // Determine if we have limit values for either account or folder.
            var accountId = GetLimitToAccountId( configurationValues );
            var folderId = GetLimitToFolderId( configurationValues );

            // Set defaults for picker visibility.
            mediaElementPicker.ShowAccountPicker = true;
            mediaElementPicker.ShowFolderPicker = true;

            // Set default values and hide the controls we don't need.
            if ( folderId.IsNotNullOrZero() )
            {
                mediaElementPicker.MediaFolderId = folderId;
                mediaElementPicker.ShowFolderPicker = false;
                mediaElementPicker.ShowAccountPicker = false;
            }
            else if ( accountId.IsNotNullOrZero() )
            {
                mediaElementPicker.MediaAccountId = accountId;
                mediaElementPicker.ShowAccountPicker = false;
            }
        }

        #endregion

        #region IEntityFieldType Methods

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns>The identifier of the currently selected value or <c>null</c> if no value selected.</returns>
        public int? GetEditValueAsEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var mediaGuid = GetEditValue( control, configurationValues ).AsGuidOrNull();

            if ( !mediaGuid.HasValue )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                return new MediaElementService( rockContext ).GetId( mediaGuid.Value );
            }
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            if ( id.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var mediaGuid = new MediaElementService( rockContext ).GetGuid( id.Value );

                    SetEditValue( control, configurationValues, mediaGuid.ToStringSafe() );
                }
            }
            else
            {
                SetEditValue( control, configurationValues, string.Empty );
            }
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="IEntity"/> instance associated with the value.</returns>
        public IEntity GetEntity( string value )
        {
            return GetEntity( value, null );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="IEntity"/> instance associated with the value.</returns>
        public IEntity GetEntity( string value, RockContext rockContext )
        {
            var mediaGuid = value.AsGuidOrNull();

            if ( !mediaGuid.HasValue )
            {
                return null;
            }

            return new MediaElementService( rockContext ?? new RockContext() ).Get( mediaGuid.Value );
        }

        #endregion
    }
}
