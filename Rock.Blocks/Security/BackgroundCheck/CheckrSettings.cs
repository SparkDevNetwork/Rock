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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Security.BackgroundCheck.CheckrSettings;
using Rock.Web.Cache;

namespace Rock.Blocks.Security.BackgroundCheck
{
    /// <summary>
    /// Block for updating the settings used by the Checkr integration.
    /// </summary>
    [DisplayName( "Checkr Settings" )]
    [Category( "Security > Background Check" )]
    [Description( "Block for updating the settings used by the Checkr integration." )]
    [IconCssClass( "ti ti-shield-half" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "79fdd794-c93e-4674-ac70-6a3890e5574a" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "675755e7-4342-4bf1-9245-0f5e6b569074" )]
    [Rock.SystemGuid.BlockTypeGuid( "562A5CA4-1697-40E3-A54A-C451291A3251" )]
    public class CheckrSettings : RockBlockType
    {
        #region Constants

        /// <summary>
        /// The fully qualified type name of the Checkr background check component.
        /// </summary>
        private const string CheckrTypeName = "Rock.Checkr.Checkr";

        /// <summary>
        /// The prefix added to Checkr package defined value names (e.g., "Checkr - ").
        /// </summary>
        private const string CheckrTypeNamePrefix = "Checkr - ";

        /// <summary>
        /// The Checkr workflow type GUID.
        /// </summary>
        private const string CheckrWorkflowTypeGuid = "9BC07356-3B2F-4BFF-9320-FA8F3A28FC39";

        /// <summary>
        /// The renamed PMM workflow type name used when Checkr becomes the default provider.
        /// </summary>
        private const string PmmWorkflowTypeRenamed = "Background Check (PMM)";

        #endregion Constants

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return GetBoxOptions();
        }

        /// <summary>
        /// Gets the options bag that describes the current state of the Checkr configuration.
        /// </summary>
        /// <param name="warningMessage">An optional warning message to include in the response.</param>
        /// <returns>A <see cref="CheckrSettingsOptionsBag"/> containing the current settings.</returns>
        private CheckrSettingsOptionsBag GetBoxOptions( string warningMessage = null )
        {
            var accessToken = GetAccessToken();
            var isConfigured = accessToken.IsNotNullOrWhiteSpace();

            return new CheckrSettingsOptionsBag
            {
                IsConfigured = isConfigured,
                IsDefaultProvider = IsDefaultProvider(),
                AccessToken = accessToken,
                Packages = isConfigured ? GetActivePackages() : new List<string>(),
                OrganizationName = GlobalAttributesCache.Get().GetValue( "OrganizationName" ),
                WarningMessage = warningMessage
            };
        }

        /// <summary>
        /// Gets the decrypted Checkr access token from the component's attribute values.
        /// </summary>
        /// <returns>The decrypted access token, or <c>null</c> if not configured.</returns>
        private string GetAccessToken()
        {
            var settings = GetSettings( RockContext );
            if ( settings == null )
            {
                return null;
            }

            return GetSettingValue( settings, "AccessToken", true );
        }

        /// <summary>
        /// Determines whether Checkr is the default background check provider.
        /// </summary>
        /// <returns><c>true</c> if Checkr is the default provider; otherwise, <c>false</c>.</returns>
        private bool IsDefaultProvider()
        {
            var defaultProvider = Web.SystemSettings.GetValue( SystemKey.SystemSetting.DEFAULT_BACKGROUND_CHECK_PROVIDER ) ?? string.Empty;
            return CheckrTypeName == defaultProvider;
        }

        /// <summary>
        /// Gets the list of active Checkr package names with the type prefix stripped.
        /// </summary>
        /// <returns>A list of package display names.</returns>
        private List<string> GetActivePackages()
        {
            return new DefinedValueService( RockContext )
                .GetByDefinedTypeGuid( SystemGuid.DefinedType.BACKGROUND_CHECK_TYPES.AsGuid() )
                .Where( v => v.ForeignId == 2 && v.IsActive )
                .Select( v => v.Value.Substring( CheckrTypeNamePrefix.Length ) )
                .ToList();
        }

        /// <summary>
        /// Gets the Checkr component's attribute values.
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        /// <returns>A list of attribute values for the Checkr entity type, or <c>null</c>.</returns>
        private List<AttributeValue> GetSettings( RockContext rockContext )
        {
            var checkrEntityType = EntityTypeCache.Get( CheckrTypeName );
            if ( checkrEntityType != null )
            {
                var attributeValueService = new AttributeValueService( rockContext );
                return attributeValueService.Queryable()
                    .Include( v => v.Attribute )
                    .Where( v => v.Attribute.EntityTypeId == checkrEntityType.Id )
                    .ToList();
            }

            return null;
        }

        /// <summary>
        /// Gets a setting value from the Checkr attribute values.
        /// </summary>
        /// <param name="values">The attribute values.</param>
        /// <param name="key">The attribute key.</param>
        /// <param name="encryptedValue">If <c>true</c>, the value will be decrypted.</param>
        /// <returns>The setting value.</returns>
        private string GetSettingValue( List<AttributeValue> values, string key, bool encryptedValue = false )
        {
            var value = values
                .Where( v => v.AttributeKey == key )
                .Select( v => v.Value )
                .FirstOrDefault();

            if ( encryptedValue && !string.IsNullOrWhiteSpace( value ) )
            {
                try
                {
                    value = Encryption.DecryptString( value );
                }
                catch
                {
                    // Intentionally ignored: decryption failure returns the raw value.
                }
            }

            return value;
        }

        /// <summary>
        /// Sets a setting value in the Checkr attribute values, creating a new attribute value row if needed.
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        /// <param name="values">The attribute values.</param>
        /// <param name="key">The attribute key.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="encryptValue">If <c>true</c>, the value will be encrypted before saving.</param>
        private void SetSettingValue( RockContext rockContext, List<AttributeValue> values, string key, string value, bool encryptValue = false )
        {
            if ( encryptValue && !string.IsNullOrWhiteSpace( value ) )
            {
                try
                {
                    value = Encryption.EncryptString( value );
                }
                catch
                {
                    // Intentionally ignored: encryption failure stores the raw value.
                }
            }

            var attributeValue = values
                .Where( v => v.AttributeKey == key )
                .FirstOrDefault();

            if ( attributeValue != null )
            {
                attributeValue.Value = value;
            }
            else
            {
                var checkrEntityType = EntityTypeCache.Get( CheckrTypeName );
                if ( checkrEntityType != null )
                {
                    var attribute = new AttributeService( rockContext ).Queryable()
                        .Where( a =>
                            a.EntityTypeId == checkrEntityType.Id &&
                            a.Key == key
                        )
                        .FirstOrDefault();

                    if ( attribute != null )
                    {
                        attributeValue = new AttributeValue();
                        new AttributeValueService( rockContext ).Add( attributeValue );
                        attributeValue.AttributeId = attribute.Id;
                        attributeValue.Value = value;
                        attributeValue.EntityId = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Invokes the static <c>Rock.Checkr.Checkr.UpdatePackages</c> method via reflection
        /// to avoid a compile-time dependency on the Rock.Checkr assembly.
        /// </summary>
        /// <param name="errorMessages">The error messages list to pass to the method.</param>
        /// <returns><c>true</c> if the update succeeded; otherwise, <c>false</c>.</returns>
        private bool InvokeUpdatePackages( List<string> errorMessages )
        {
            var checkrType = Type.GetType( "Rock.Checkr.Checkr, Rock.Checkr" );
            if ( checkrType == null )
            {
                errorMessages.Add( "The Rock.Checkr assembly could not be found." );
                return false;
            }

            var updateMethod = checkrType.GetMethod( "UpdatePackages", BindingFlags.Public | BindingFlags.Static );
            if ( updateMethod == null )
            {
                errorMessages.Add( "The UpdatePackages method could not be found on the Checkr component." );
                return false;
            }

            var result = updateMethod.Invoke( null, new object[] { errorMessages } );
            return result is bool success && success;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Saves the provided access token, encrypting it before storage, and returns the updated options.
        /// </summary>
        /// <param name="bag">The request containing the new access token.</param>
        /// <returns>The updated <see cref="CheckrSettingsOptionsBag"/>.</returns>
        [BlockAction]
        public BlockActionResult SaveAccessToken( CheckrSettingsSaveTokenRequestBag bag )
        {
            if ( bag == null || bag.AccessToken.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "An access token is required." );
            }

            var settings = GetSettings( RockContext );
            SetSettingValue( RockContext, settings, "AccessToken", bag.AccessToken, true );

            RockContext.SaveChanges();

            BackgroundCheckContainer.Instance.Refresh();

            // If this is the default provider, update packages automatically.
            string warning = null;
            if ( IsDefaultProvider() )
            {
                var errorMessages = new List<string>();
                if ( !InvokeUpdatePackages( errorMessages ) )
                {
                    warning = string.Join( " ", errorMessages );
                    foreach ( var errorMessage in errorMessages )
                    {
                        ExceptionLogService.LogException( new Exception( errorMessage ), null );
                    }
                }
            }

            return ActionOk( GetBoxOptions( warning ) );
        }

        /// <summary>
        /// Calls the Checkr API to update the list of available packages.
        /// </summary>
        /// <returns>The updated <see cref="CheckrSettingsOptionsBag"/>, or an error message.</returns>
        [BlockAction]
        public BlockActionResult UpdatePackages()
        {
            var errorMessages = new List<string>();
            if ( !InvokeUpdatePackages( errorMessages ) )
            {
                foreach ( var errorMessage in errorMessages )
                {
                    ExceptionLogService.LogException( new Exception( errorMessage ), null );
                }

                return ActionBadRequest( string.Join( " ", errorMessages ) );
            }

            return ActionOk( GetBoxOptions() );
        }

        /// <summary>
        /// Enables Checkr as the default background check provider by updating
        /// the Bio block workflow actions, renaming workflows, and toggling packages.
        /// </summary>
        /// <returns>The updated <see cref="CheckrSettingsOptionsBag"/>.</returns>
        [BlockAction]
        public BlockActionResult EnableAsDefault()
        {
            // Update the Bio block's WorkflowActions attribute to include Checkr and remove PMM.
            var bioBlock = BlockCache.Get( Rock.SystemGuid.Block.BIO.AsGuid() );
            if ( bioBlock == null )
            {
                var errorMessage = string.Format( "Stock Bio block ({0}) is missing.", Rock.SystemGuid.Block.BIO );
                ExceptionLogService.LogException( new Exception( errorMessage ) );
            }
            else
            {
                var workflowActionGuidList = bioBlock.GetAttributeValues( "WorkflowActions" ).AsGuidList();
                if ( workflowActionGuidList == null || workflowActionGuidList.Count == 0 )
                {
                    bioBlock.SetAttributeValue( "WorkflowActions", CheckrWorkflowTypeGuid );
                }
                else
                {
                    var checkrGuid = CheckrWorkflowTypeGuid.AsGuid();
                    if ( !workflowActionGuidList.Any( w => w == checkrGuid ) )
                    {
                        workflowActionGuidList.Add( checkrGuid );
                    }

                    // Remove PMM from Bio Workflow Actions.
                    var pmmGuid = Rock.SystemGuid.WorkflowType.PROTECTMYMINISTRY.AsGuid();
                    workflowActionGuidList.RemoveAll( w => w == pmmGuid );
                    bioBlock.SetAttributeValue( "WorkflowActions", workflowActionGuidList.AsDelimited( "," ) );
                }

                bioBlock.SaveAttributeValue( "WorkflowActions" );
            }

            // Activate the Checkr component and set it as the default provider.
            var checkrComponent = BackgroundCheckContainer.Instance.Components.Values.FirstOrDefault( c => c.Value.TypeName == CheckrTypeName );
            if ( checkrComponent == null )
            {
                return ActionBadRequest( "The Checkr background check component is not available." );
            }

            checkrComponent.Value.SetAttributeValue( "Active", "True" );
            checkrComponent.Value.SaveAttributeValue( "Active" );
            Web.SystemSettings.SetValue( Rock.SystemKey.SystemSetting.DEFAULT_BACKGROUND_CHECK_PROVIDER, CheckrTypeName );

            var workflowTypeService = new WorkflowTypeService( RockContext );

            // Rename PMM Workflow.
            var pmmWorkflowAction = workflowTypeService.Get( Rock.SystemGuid.WorkflowType.PROTECTMYMINISTRY.AsGuid() );
            if ( pmmWorkflowAction != null )
            {
                pmmWorkflowAction.Name = PmmWorkflowTypeRenamed;
            }

            // Rename Checkr Workflow.
            var checkrWorkflowAction = workflowTypeService.Get( CheckrWorkflowTypeGuid.AsGuid() );
            if ( checkrWorkflowAction != null )
            {
                checkrWorkflowAction.Name = "Background Check";
            }

            RockContext.SaveChanges();

            // Disable all existing background check packages so Checkr ones can be freshly synced.
            var definedValueService = new DefinedValueService( RockContext );
            var packages = definedValueService
                .GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.BACKGROUND_CHECK_TYPES.AsGuid() )
                .ToList();

            foreach ( var package in packages )
            {
                package.IsActive = false;
            }

            RockContext.SaveChanges();

            // Refresh packages from Checkr API.
            string warning = null;
            var updateErrors = new List<string>();
            if ( !InvokeUpdatePackages( updateErrors ) )
            {
                warning = string.Join( " ", updateErrors );
                foreach ( var errorMessage in updateErrors )
                {
                    ExceptionLogService.LogException( new Exception( errorMessage ), null );
                }
            }

            return ActionOk( GetBoxOptions( warning ) );
        }

        #endregion Block Actions
    }
}
