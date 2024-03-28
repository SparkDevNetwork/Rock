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
using Azure.Storage.Blobs;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Storage.Common;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace Rock.Storage.Provider
{
    /// <summary>
    /// Storage provider for Azure Blob Storage
    /// </summary>
    [Description( "Azure Blob Storage" )]
    [Export( typeof( ProviderComponent ) )]
    [ExportMetadata( "ComponentName", "Azure Blob Storage" )]

    #region Storage Provider Attributes

    [TextField( "Account Name",
        Description = "The Azure account name.",
        Key = AttributeKey.AccountName,
        IsRequired = true,
        Order = 1 )]

    [TextField("Account Key",
        Description = "The Azure account key.",
        Key = AttributeKey.AccountKey,
        IsRequired = true,
        Order = 2 )]

    [UrlLinkField( "Custom Domain",
        Description = "If you have configured the Azure container with a custom domain name that you'd like to use, set that value here (e.g. 'http://storage.yourorganization.com').",
        Key = AttributeKey.CustomDomain,
        IsRequired = false,
        Order = 3 )]

    [TextField( "Default Container Name",
        Description = "The default Azure blob container to use for file types that do not provide their own.",
        Key = AttributeKey.DefaultContainerName,
        IsRequired = true,
        Order = 4 )]

    #endregion Storage Provider Attributes

    [Rock.SystemGuid.EntityTypeGuid( "9925A20A-7262-4FC7-B86E-856F6D98BE17")]
    public class AzureBlobStorage : ProviderComponent
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string AccountName = "AccountName";
            public const string AccountKey = "AccountKey";
            public const string CustomDomain = "CustomDomain";
            public const string DefaultContainerName = "DefaultContainerName";
        }

        #endregion Attribute Keys

        #region ProviderComponent Implementation

        /// <summary>
        /// Saves the file.
        /// </summary>
        /// <param name="binaryFile">The <see cref="BinaryFile"/>.</param>
        public override void SaveContent( BinaryFile binaryFile )
        {
            SaveContent( binaryFile, out _ );
        }

        /// <summary>
        /// Saves the file.
        /// </summary>
        /// <param name="binaryFile">The <see cref="BinaryFile"/>.</param>
        /// <param name="fileSize">Size of the file.</param>
        public override void SaveContent( BinaryFile binaryFile, out long? fileSize )
        {
            var blobClient = GetBlobClient( binaryFile );

            /*                
                1/22/2024 - JMH

                Azure Blob Storage upload was throwing an exception when adding an SMS image attachment
                in the Communication Entry Wizard block that was wider than the Max SMS Image Width block setting.
                Two file uploads occur in this case:
                 1. By the FileUploader control when the attachment is first added.
                 2. By the CommunicationEntryWizard block if the image is resized.
                The exception was being thrown by the second call to BlobClient.Upload() when reuploading the
                resized image because the `overwrite: true` argument was not provided to allow for updating existing files.
            
                Reason: Wide SMS image attachments stored in Azure cause exceptions in Communication Wizard.
                https://github.com/SparkDevNetwork/Rock/issues/5719
             */
            blobClient.Upload( binaryFile.ContentStream, overwrite: true );

            fileSize = binaryFile.ContentStream.Length;
        }

        /// <summary>
        /// Removes the file.
        /// </summary>
        /// <param name="binaryFile">The <see cref="BinaryFile"/>.</param>
        public override void DeleteContent( BinaryFile binaryFile )
        {
            var blobClient = GetBlobClient( binaryFile );
            blobClient.DeleteIfExists();
        }

        /// <summary>
        /// Gets the content stream of a file.
        /// </summary>
        /// <param name="binaryFile">The <see cref="BinaryFile"/>.</param>
        /// <returns></returns>
        public override System.IO.Stream GetContentStream( BinaryFile binaryFile )
        {
            var blobClient = GetBlobClient( binaryFile );
            if ( blobClient.Exists() )
            {
                return blobClient.OpenRead();
            }
            return null;
        }

        /// <summary>
        /// Gets the path of a file.
        /// </summary>
        /// <param name="binaryFile">The <see cref="BinaryFile"/>.</param>
        /// <returns></returns>
        public override string GetPath( BinaryFile binaryFile )
        {
            if ( binaryFile != null && ( binaryFile.BinaryFileType == null || !binaryFile.BinaryFileType.RequiresViewSecurity ) )
            {
                var blobClient = GetBlobClient( binaryFile );
                if ( blobClient.Exists() )
                {
                    return blobClient.Uri.AbsoluteUri;
                }
            }

            return base.GetPath( binaryFile );
        }

        /// <summary>
        /// Gets the URL of a file.
        /// </summary>
        /// <param name="binaryFile">The <see cref="BinaryFile"/>.</param>
        /// <returns></returns>
        public override string GetUrl( BinaryFile binaryFile )
        {
            return binaryFile.Path;
        }

        #endregion ProviderComponent Implementation

        #region Private Methods

        /// <summary>
        /// Get the Blob Client.
        /// </summary>
        /// <param name="binaryFile">The <see cref="BinaryFile"/>.</param>
        /// <returns></returns>
        private BlobClient GetBlobClient( BinaryFile binaryFile )
        {
            if ( binaryFile == null )
            {
                return null;
            }

            FileTypeSettings settings;
            if ( binaryFile.StorageSettings != null && binaryFile.StorageSettings.ContainsKey( "AzureBlobContainerName" ) && binaryFile.StorageSettings.ContainsKey( "AzureBlobContainerFolderPath" ) )
            {
                settings = new FileTypeSettings
                {
                    ContainerName = binaryFile.StorageSettings["AzureBlobContainerName"],
                    Folder = binaryFile.StorageSettings["AzureBlobContainerFolderPath"]
                };
            }
            else
            {
                settings = GetSettingsFromFileType( binaryFile );
            }

            string rawGuid = binaryFile.Guid.ToString().Replace( "-", "" );
            string fileName = $"{rawGuid}_{binaryFile.FileName}";
            string blobName = string.IsNullOrWhiteSpace( settings.Folder ) ? fileName : $"{settings.Folder}/{fileName}";

            var accountName = GetAttributeValue( AttributeKey.AccountName );
            var accountKey = GetAttributeValue( AttributeKey.AccountKey );
            var customDomain = GetAttributeValue( AttributeKey.CustomDomain );
            var containerName = settings.ContainerName.IsNotNullOrWhiteSpace() ? settings.ContainerName : GetAttributeValue( AttributeKey.DefaultContainerName );

            return AzureBlobStorageClient.Instance.GetBlobClient( accountName, accountKey, customDomain, containerName, blobName );
        }

        /// <summary>
        /// Gets the <see cref="FileTypeSettings"/> from attributes of the <see cref="BinaryFileType"/> associated with a file.
        /// </summary>
        /// <param name="binaryFile">The <see cref="BinaryFile"/>.</param>
        /// <returns></returns>
        private FileTypeSettings GetSettingsFromFileType( BinaryFile binaryFile )
        {
            var settings = new FileTypeSettings();
            if ( binaryFile == null || !binaryFile.BinaryFileTypeId.HasValue )
            {
                return settings;
            }

            var binaryFileType = binaryFile.BinaryFileType;
            if ( binaryFileType == null && binaryFile.BinaryFileTypeId.HasValue )
            {
                binaryFileType = new BinaryFileTypeService( new RockContext() ).Get( binaryFile.BinaryFileTypeId.Value );
            }
            if ( binaryFileType == null )
            {
                return settings;
            }

            if ( binaryFileType.Attributes == null )
            {
                binaryFileType.LoadAttributes();
            }

            settings.ContainerName = binaryFileType.GetAttributeValue( "AzureBlobContainerName" );
            settings.Folder = ( binaryFileType.GetAttributeValue( "AzureBlobContainerFolderPath" ) ?? string.Empty )
                .Replace( @"\", "/" )
                .TrimEnd( "/".ToCharArray() );

            return settings;
        }

        /// <summary>
        /// File Type Settings POCO.
        /// </summary>
        private class FileTypeSettings
        {
            public string ContainerName;
            public string Folder;
        }

        #endregion Private Methods
    }
}