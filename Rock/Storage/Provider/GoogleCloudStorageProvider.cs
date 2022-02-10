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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using Google.Cloud.Storage.V1;
using Rock.Attribute;
using Rock.Model;
using Rock.Security;
using Rock.Storage.Common;
using GoogleObject = Google.Apis.Storage.v1.Data.Object;

namespace Rock.Storage.Provider
{
    /// <summary>
    /// Storage provider for saving binary files to Google Cloud
    /// </summary>
    [Description( "Google Cloud Storage" )]
    [Export( typeof( ProviderComponent ) )]
    [ExportMetadata( "ComponentName", "Google Cloud Storage" )]

    [TextField( "Bucket Name",
        Description = "The text name of your Google Cloud Storage bucket within the project. See https://console.cloud.google.com/storage/browser",
        IsRequired = true,
        Order = 1,
        Key = AttributeKey.BucketName )]

    [EncryptedTextField( "Service Account JSON Key",
        Description = "The Service Account key JSON file contents that is used to access Google Cloud Storage. See https://console.cloud.google.com/iam-admin/serviceaccounts",
        IsRequired = true,
        Order = 2,
        Key = AttributeKey.ServiceAccountKey )]

    [TextField( "Root Folder",
        Description = "Optional root folder. Must be the full path to the root folder starting from the first after the bucket name. This must end with a '/'.",
        IsRequired = false,
        Order = 3,
        Key = AttributeKey.RootFolder )]

    public class GoogleCloudStorageProvider : ProviderComponent
    {
        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The bucket name
            /// </summary>
            public const string BucketName = "BucketName";

            /// <summary>
            /// The API key
            /// </summary>
            public const string ServiceAccountKey = "ApiKey";

            /// <summary>
            /// The root folder
            /// </summary>
            public const string RootFolder = "RootFolder";
        }

        #endregion Keys

        #region Provider Component

        /// <summary>
        /// Saves the binary file contents to the external storage medium associated with the provider.
        /// </summary>
        /// <param name="binaryFile">The binary file.</param>
        public override void SaveContent( BinaryFile binaryFile )
        {
            SaveContent( binaryFile, out _ );
        }

        /// <summary>
        /// Saves the binary file contents to the external storage medium associated with the provider.
        /// </summary>
        /// <param name="binaryFile">The binary file.</param>
        /// <param name="fileSize">Size of the file.</param>
        /// <exception cref="System.ArgumentException">File Data must not be null.</exception>
        public override void SaveContent( BinaryFile binaryFile, out long? fileSize )
        {
            /*
               SK - 12/11/2021
               Path should always be reset while saving content otherwise new storage provider may still be refering the older path.
           */
            binaryFile.Path = null;
            var googleObject = TranslateBinaryFileToGoogleObject( binaryFile );

            using ( var client = GetStorageClient() )
            {
                client.UploadObject( googleObject, binaryFile.ContentStream );
            }

            fileSize = binaryFile.ContentStream.Length;
        }

        /// <summary>
        /// Deletes the content from the external storage medium associated with the provider.
        /// </summary>
        /// <param name="file">The file.</param>
        public override void DeleteContent( BinaryFile file )
        {
            var bucketName = GetBucketName();
            var accountKeyJson = GetServiceAccountKeyJson();
            var googleObject = TranslateBinaryFileToGoogleObject( file );

            GoogleCloudStorage.DeleteObject( bucketName, accountKeyJson, false, googleObject.Name );
        }

        /// <summary>
        /// Gets the contents from the external storage medium associated with the provider
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public override Stream GetContentStream( BinaryFile file )
        {
            var googleObject = TranslateBinaryFileToGoogleObject( file );
            var stream = new MemoryStream();

            using ( var client = GetStorageClient() )
            {
                client.DownloadObject( googleObject, stream );
            }

            stream.Position = 0;
            return stream;
        }

        #endregion Provider Component

        #region Overrides

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public override string GetPath( BinaryFile file )
        {
            var rootFolder = GetRootFolder();
            return $"{rootFolder}{file.Guid}_{file.FileName}";
        }

        /// <summary>
        /// Generate a URL for the file based on the rules of the StorageProvider
        /// </summary>
        /// <param name="binaryFile">The file.</param>
        /// <returns></returns>
        public override string GetUrl( BinaryFile binaryFile )
        {
            var bucketName = GetBucketName();
            var googleObject = TranslateBinaryFileToGoogleObject( binaryFile );

            using ( var client = GetStorageClient() )
            {
                var response = client.GetObject( bucketName, googleObject.Name );
                return response.MediaLink;
            }
        }

        #endregion Overrides

        #region Private Methods

        /// <summary>
        /// Gets the name of the bucket.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException">The Google bucket name setting is not valid</exception>
        private string GetBucketName()
        {
            var bucketName = GetAttributeValue( AttributeKey.BucketName );

            if ( bucketName.IsNullOrWhiteSpace() )
            {
                throw new ArgumentException( "The Google bucket name setting is not valid", AttributeKey.BucketName );
            }

            return bucketName;
        }

        /// <summary>
        /// Gets the service account key JSON.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException">The Google Service Account Key JSON setting is not valid</exception>
        private string GetServiceAccountKeyJson()
        {
            var encryptedJson = GetAttributeValue( AttributeKey.ServiceAccountKey );
            var serviceAccountKeyJson = Encryption.DecryptString( encryptedJson );

            if ( serviceAccountKeyJson.IsNullOrWhiteSpace() )
            {
                throw new ArgumentException( "The Google Service Account Key JSON setting is not valid", AttributeKey.ServiceAccountKey );
            }

            return serviceAccountKeyJson;
        }

        /// <summary>
        /// Gets the root folder.
        /// </summary>
        /// <returns></returns>
        public virtual string GetRootFolder()
        {
            var rawRootFolder = GetAttributeValue( AttributeKey.RootFolder ).Trim();
            return FixRootFolder( rawRootFolder );
        }

        /// <summary>
        /// Translates the binary file to Google object.
        /// </summary>
        /// <param name="binaryFile">The binary file.</param>
        /// <returns></returns>
        private GoogleObject TranslateBinaryFileToGoogleObject( BinaryFile binaryFile )
        {
            var bucketName = GetBucketName();
            var name = binaryFile.Path.IsNullOrWhiteSpace() ? GetPath( binaryFile ) : binaryFile.Path;

            return new GoogleObject
            {
                Name = name,
                Bucket = bucketName,
                Size = Convert.ToUInt64( binaryFile.FileSize ),
                Updated = binaryFile.ModifiedDateTime,
                ContentType = System.Web.MimeMapping.GetMimeMapping( binaryFile.FileName )
            };
        }

        /// <summary>
        /// Gets the storage client.
        /// </summary>
        /// <returns></returns>
        private StorageClient GetStorageClient()
        {
            var accountKeyJson = GetServiceAccountKeyJson();
            return GoogleCloudStorage.GetStorageClient( accountKeyJson );
        }

        /// <summary>
        /// Fixes the root folder.
        /// </summary>
        /// <param name="rootFolder">The root folder.</param>
        /// <returns></returns>
        private string FixRootFolder( string rootFolder )
        {
            if ( rootFolder.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }
            else if ( rootFolder.EndsWith( "/" ) )
            {
                return rootFolder;
            }
            else
            {
                return rootFolder + "/";
            }
        }

        #endregion Private Methods
    }
}
