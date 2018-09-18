using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;

using Amazon;
using Amazon.S3;
using Amazon.S3.IO;
using Amazon.S3.Model;

using Rock.Model;
using Rock.Attribute;
using Rock.Data;
using Rock.Web.Cache;
using System.Threading.Tasks;

namespace Rock.Storage.AssetStorage
{
    [Description( "Amazon S3 Storage Service" )]
    [Export( typeof( AssetStorageComponent ) )]
    [ExportMetadata( "ComponentName", "AmazonS3" )]

    [TextField( name: "AWS Region", description: "The AWS S3 Region in which the bucket is located. e.g. us-east-1", required: true, defaultValue: "", category: "", order: 0, key: "AWSRegion" )]
    [TextField( name: "Bucket", description: "The name of the AWS S3 bucket where the files are stored.", required: true, defaultValue: "", category: "", order: 1, key: "Bucket" )]
    [TextField( name: "Root Folder", description: "Optional root folder. Must be the full path to the root folder starting from the first after the bucket name.", required: false, defaultValue: "", category: "", order: 2, key: "RootFolder" )]
    [IntegerField( name: "Expiration", description: "The time in minutes that the created public URL is available before being expired.", required: false, defaultValue: 525600, category: "", order: 3, key: "Expiration" )]
    [TextField( name: "AWS Profile Name", description: "Should be an AWS IAM user.", required: true, defaultValue: "", category: "", order: 4, key: "AWSProfileName" )]
    [TextField( name: "AWS Access Key", description: "The access key for the user.", required: true, defaultValue: "", category: "", order: 5, key: "AWSAccessKey" )]
    [TextField( name: "AWS Secret Key", description: "The seceret key for the user. Amazon only gives this when the user is created. If lost then a new user will need to be created.", required: true, defaultValue: "", category: "", order: 6, key: "AWSSecretKey" )]
    
    public class AmazonS3Component : AssetStorageComponent
    {

        #region Properties

        public override string IconCssClass
        {
            get
            {
                return "fa fa-aws";
            }
        }

        #endregion Properties

        #region Contstructors
        public AmazonS3Component() : base()
        {
        }

        #endregion Constructors

        #region Override Methods

        /// <summary>
        /// Lists the objects from the current root folder.
        /// </summary>
        /// <param name="assetStorageSystem"></param>
        /// <returns></returns>
        public override List<Asset> ListObjects( AssetStorageSystem assetStorageSystem )
        {
            return ListObjects( assetStorageSystem, new Asset { Type = AssetType.Folder } );
        }

        /// <summary>
        /// Lists the objects. If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If key and name are not provided then list all objects from the current RootFolder.
        /// If a key is provided it MUST use the full path, RootFolder is not used, and Name is not used.
        /// The last segment in Key is treated as a begins with search if it does not end in a '/'. e.g. to get all
        /// files starting with 'mr' in folder 'pictures/cats/' set key = 'pictures/cats/mr' to get 'mr. whiskers'
        /// and 'mrs. whiskers' but not 'fluffy' or 'carnage the attack cat'.
        /// </summary>
        /// <param name="assetStorageSystem"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        public override List<Asset> ListObjects( AssetStorageSystem assetStorageSystem, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageSystem, "RootFolder" ) );
            asset.Key = FixKey( asset, rootFolder );

            try
            {
                AmazonS3Client Client = GetAmazonS3Client( assetStorageSystem );

                ListObjectsV2Request request = new ListObjectsV2Request();
                request.BucketName = GetAttributeValue( assetStorageSystem, "Bucket" );
                request.Prefix = asset.Key == "/" ? GetAttributeValue( assetStorageSystem, "RootFolder" ) : asset.Key;

                var assets = new List<Asset>();

                ListObjectsV2Response response;

                do
                {
                    response = Client.ListObjectsV2( request );
                    foreach ( S3Object s3Object in response.S3Objects )
                    {
                        if ( s3Object.Key == null )
                        {
                            continue;
                        }

                        var responseAsset = CreateAssetFromS3Object( s3Object, Client.Config.RegionEndpoint.SystemName );
                        assets.Add( responseAsset );
                    }

                    request.ContinuationToken = response.NextContinuationToken;
                } while ( response.IsTruncated );

                return assets;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Lists the files in AssetStorageSystem.RootFolder.
        /// </summary>
        /// <param name="assetStorageSystem"></param>
        /// <returns></returns>
        public override List<Asset> ListFilesInFolder( AssetStorageSystem assetStorageSystem )
        {
            return ListFilesInFolder( assetStorageSystem, new Asset { Type = AssetType.Folder } );
        }

        /// <summary>
        /// Lists the files in folder. Asset.Key or Asset.Name is the folder.
        /// If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If Key and Name are not provided then list all files in the current RootFolder.
        /// If a key is provided it MUST use the full path, RootFolder and Name are not used.
        /// The last segment in the key is the folder name.
        /// </summary>
        /// <param name="assetStorageSystem"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        public override List<Asset> ListFilesInFolder( AssetStorageSystem assetStorageSystem, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageSystem, "RootFolder" ) );
            asset.Key = FixKey( asset, rootFolder );
            HasRequirementsFolder( asset );
            
            try
            {
                AmazonS3Client Client = GetAmazonS3Client( assetStorageSystem );

                ListObjectsV2Request request = new ListObjectsV2Request();
                request.BucketName = GetAttributeValue( assetStorageSystem, "Bucket" );
                request.Prefix = asset.Key == "/" ? string.Empty : asset.Key;
                request.Delimiter = "/";

                var assets = new List<Asset>();

                ListObjectsV2Response response;

                // S3 will only return 1,000 keys per response and sets IsTruncated = true, the do-while loop will run and fetch keys until IsTruncated = false.
                do
                {
                    response = Client.ListObjectsV2( request );
                    foreach ( S3Object s3Object in response.S3Objects )
                    {
                        if ( s3Object.Key == null || s3Object.Key.EndsWith("/") )
                        {
                            continue;
                        }

                        var responseAsset = CreateAssetFromS3Object( s3Object, Client.Config.RegionEndpoint.SystemName );
                        assets.Add( responseAsset );
                    }

                    request.ContinuationToken = response.NextContinuationToken;

                } while ( response.IsTruncated );

                return assets;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Lists the folders in AssetStorageSystem.Rootfolder.
        /// </summary>
        /// <param name="assetStorageSystem"></param>
        /// <returns></returns>
        public override List<Asset> ListFoldersInFolder( AssetStorageSystem assetStorageSystem )
        {
            return ListFoldersInFolder( assetStorageSystem, new Asset { Type = AssetType.Folder } );
        }

        /// <summary>
        /// Lists the folder in folder. Asset.Key or Asset.Name is the folder.
        /// If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If Key and Name are not provided the list then list all files in the current RootFolder.
        /// If a key is provided it MUST use the full path, RootFolder and Name are not used.
        /// The last segment in the key is the folder name.
        /// </summary>
        /// <param name="assetStorageSystem"></param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override List<Asset> ListFoldersInFolder( AssetStorageSystem assetStorageSystem, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageSystem, "RootFolder" ) );
            string bucketName = GetAttributeValue( assetStorageSystem, "Bucket" );

            asset.Key = FixKey( asset, rootFolder );
            HasRequirementsFolder( asset );
            
            try
            {
                AmazonS3Client Client = GetAmazonS3Client( assetStorageSystem );

                ListObjectsV2Request request = new ListObjectsV2Request();
                request.BucketName = bucketName;
                request.Prefix = asset.Key == "/" ? string.Empty : asset.Key;
                request.Delimiter = "/";

                var assets = new List<Asset>();
                var subFolders = new HashSet<string>();

                ListObjectsV2Response response;

                // S3 will only return 1,000 keys per response and sets IsTruncated = true, the do-while loop will run and fetch keys until IsTruncated = false.
                do
                {
                    response = Client.ListObjectsV2( request );

                    foreach ( string subFolder in response.CommonPrefixes )
                    {
                        if ( subFolder.IsNotNullOrWhiteSpace() )
                        {
                            subFolders.Add( subFolder );
                        }
                    }

                    request.ContinuationToken = response.NextContinuationToken;

                } while ( response.IsTruncated );

                // Add the subfolders to the asset collection
                foreach ( string subFolder in subFolders )
                {
                    var subFolderAsset = CreateAssetFromCommonPrefix( subFolder, Client.Config.RegionEndpoint.SystemName, bucketName );
                    assets.Add( subFolderAsset );
                }

                return assets;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Returns a stream of the specified file.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override Asset GetObject( AssetStorageSystem assetStorageSystem, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageSystem, "RootFolder" ) );
            asset.Key = FixKey( asset, rootFolder );
            HasRequirementsFile( asset );

            try
            {
                AmazonS3Client Client = GetAmazonS3Client( assetStorageSystem );

                GetObjectResponse response = Client.GetObject( GetAttributeValue( assetStorageSystem, "Bucket" ), asset.Key );
                return CreateAssetFromGetObjectResponse( response, Client.Config.RegionEndpoint.SystemName );
                
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Uploads a file. If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If a key is provided it MUST use the full path, RootFolder is not used.
        /// </summary>
        /// <param name="assetStorageSystem"></param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override bool UploadObject( AssetStorageSystem assetStorageSystem, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageSystem, "RootFolder" ) );
            asset.Key = FixKey( asset, rootFolder );

            try
            {
                AmazonS3Client Client = GetAmazonS3Client( assetStorageSystem );

                PutObjectRequest request = new PutObjectRequest();
                request.BucketName = GetAttributeValue( assetStorageSystem, "Bucket" );
                request.Key = asset.Key;
                request.InputStream = asset.AssetStream;

                PutObjectResponse response = Client.PutObject( request );
                if ( response.HttpStatusCode == System.Net.HttpStatusCode.OK )
                {
                    return true;
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }

            return false;
        }

        /// <summary>
        /// Creates a folder. If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If Key is provided it MUST use the full path, RootFolder is not used.
        /// </summary>
        /// <param name="assetStorageSystem"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        public override bool CreateFolder( AssetStorageSystem assetStorageSystem, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageSystem, "RootFolder" ) );
            asset.Key = FixKey( asset, rootFolder );

            try
            {
                AmazonS3Client Client = GetAmazonS3Client( assetStorageSystem );

                PutObjectRequest request = new PutObjectRequest();
                request.BucketName = GetAttributeValue( assetStorageSystem, "Bucket" );
                request.Key = asset.Key;

                PutObjectResponse response = Client.PutObject( request );
                if ( response.HttpStatusCode == System.Net.HttpStatusCode.OK )
                {
                    return true;
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }

            return false;
        }

        /// <summary>
        /// Deletes the asset. If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If Key is provided then it MUST use the full path, RootFolder is not used.
        /// </summary>
        /// <param name="assetStorageSystem"></param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override bool DeleteAsset( AssetStorageSystem assetStorageSystem, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageSystem, "RootFolder" ) );
            asset.Key = FixKey( asset, rootFolder );
            AmazonS3Client client = GetAmazonS3Client( assetStorageSystem );

            if ( asset.Type == AssetType.File )
            {
                try
                {
                    DeleteObjectRequest request = new DeleteObjectRequest()
                    {
                        BucketName = GetAttributeValue( assetStorageSystem, "Bucket" ),
                        Key = asset.Key
                    };

                    DeleteObjectResponse response = client.DeleteObject( request );
                    return true;
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                    throw;
                }
            }
            else
            {
                try
                {
                    return MultipleObjectDelete( client, assetStorageSystem, asset );
                }
                catch (Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                    throw;
                }
            }
        }

        /// <summary>
        /// Renames the asset.
        /// </summary>
        /// <param name="assetStorageSystem"></param>
        /// <param name="asset">The asset.</param>
        /// <param name="newName">The new name.</param>
        /// <returns></returns>
        public override bool RenameAsset( AssetStorageSystem assetStorageSystem, Asset asset, string newName )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageSystem, "RootFolder" ) );
            asset.Key = asset.Key.IsNullOrWhiteSpace() ? rootFolder + asset.Name : asset.Key;
            string bucket = GetAttributeValue( assetStorageSystem, "Bucket" );
            try
            {
                AmazonS3Client Client = GetAmazonS3Client( assetStorageSystem );

                CopyObjectRequest copyRequest = new CopyObjectRequest();
                copyRequest.SourceBucket = bucket;
                copyRequest.DestinationBucket = bucket;
                copyRequest.SourceKey = asset.Key;
                copyRequest.DestinationKey = GetPathFromKey( asset.Key ) + newName;
                CopyObjectResponse copyResponse = Client.CopyObject( copyRequest );
                if ( copyResponse.HttpStatusCode != System.Net.HttpStatusCode.OK )
                {
                    return false;
                }

                if ( DeleteAsset( assetStorageSystem, asset ) )
                {
                    return true;
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }

            return false;
        }

        /// <summary>
        /// Creates the download link.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override string CreateDownloadLink( AssetStorageSystem assetStorageSystem, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageSystem, "RootFolder" ) );
            asset.Key = FixKey( asset, rootFolder );

            try
            {
                AmazonS3Client Client = GetAmazonS3Client( assetStorageSystem );

                GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
                {
                    BucketName = GetAttributeValue( assetStorageSystem, "Bucket" ),
                    Key = asset.Key,
                    Expires = DateTime.Now.AddMinutes( GetAttributeValue( assetStorageSystem, "Expiration" ).AsDouble() )
                };

                return Client.GetPreSignedURL( request );
            }
            catch( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Gets the objects in folder without recursion. i.e. will get the list of files
        /// and folders in the folder but not the contents of the subfolders. Subfolders
        /// will not have the ModifiedDate prop filled in as Amazon doesn't provide it in
        /// this context.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override List<Asset> ListObjectsInFolder( AssetStorageSystem assetStorageSystem, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageSystem, "RootFolder" ) );
            string bucketName = GetAttributeValue( assetStorageSystem, "Bucket" );
            asset.Key = FixKey( asset, rootFolder );
            HasRequirementsFolder( asset );
            
            try
            {
                AmazonS3Client Client = GetAmazonS3Client( assetStorageSystem );
                
                ListObjectsV2Request request = new ListObjectsV2Request();
                request.BucketName = bucketName;
                request.Prefix = asset.Key == "/" ? rootFolder : asset.Key;
                request.Delimiter = "/";

                var assets = new List<Asset>();
                var subFolders = new HashSet<string>();
                
                ListObjectsV2Response response;

                // S3 will only return 1,000 keys per response and sets IsTruncated = true, the do-while loop will run and fetch keys until IsTruncated = false.
                do
                {
                    response = Client.ListObjectsV2( request );
                    foreach ( S3Object s3Object in response.S3Objects )
                    {
                        if ( s3Object.Key == null )
                        {
                            continue;
                        }

                        var responseAsset = CreateAssetFromS3Object( s3Object, Client.Config.RegionEndpoint.SystemName );
                        assets.Add( responseAsset );
                    }

                    // After setting the delimiter S3 will filter out any prefixes below that in response.S3Objects.
                    // So we need to inspect response.CommonPrefixes to get the prefixes inside the folder.
                    foreach ( string subFolder in response.CommonPrefixes )
                    {
                        if ( subFolder.IsNotNullOrWhiteSpace() )
                        {
                            subFolders.Add( subFolder );
                        }
                    }

                    request.ContinuationToken = response.NextContinuationToken;

                } while ( response.IsTruncated ) ;

                // Add the subfolders to the asset collection
                foreach ( string subFolder in subFolders )
                {
                    var subFolderAsset = CreateAssetFromCommonPrefix( subFolder, Client.Config.RegionEndpoint.SystemName, bucketName );
                    assets.Add( subFolderAsset );
                }

                return assets;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        #endregion Override Methods

        #region Private Methods

        /// <summary>
        /// Deletes all of the S3Objects in the provided folder asset.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="assetStorageSystem">The asset storage system.</param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        private bool MultipleObjectDelete( AmazonS3Client client, AssetStorageSystem assetStorageSystem, Asset asset )
        {
            // The list of keys that will be passed into the multiple delete request
            List<KeyVersion> keys = new List<KeyVersion>();

            // Amazon only accepts 1000 keys per request, use this to keep track of how many already sent
            int keyIndex = 0;

            try
            {
                // Get a list of objest with prefix
                var assetDeleteList = ListObjects( assetStorageSystem, asset );

                // Create the list of keys
                foreach ( var assetDelete in assetDeleteList )
                {
                    keys.Add( new KeyVersion { Key = assetDelete.Key } );
                }

                while ( keyIndex < keys.Count() )
                {
                    int range = keys.Count() - keyIndex < 1000 ? keys.Count() - keyIndex : 1000;
                    var deleteObjectsRequest = new DeleteObjectsRequest
                    {
                        BucketName = GetAttributeValue( assetStorageSystem, "Bucket" ),
                        Objects = keys.GetRange( keyIndex, range )
                    };

                    DeleteObjectsResponse response = client.DeleteObjects( deleteObjectsRequest );
                    keyIndex += range;
                }

                return true;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
            
        }

        /// <summary>
        /// Creates the asset from the AWS S3Object.
        /// </summary>
        /// <param name="s3Object">The s3 object.</param>
        /// <param name="regionEndpoint">The region endpoint.</param>
        /// <returns></returns>
        private Asset CreateAssetFromS3Object( S3Object s3Object, string regionEndpoint )
        {
            string name = GetNameFromKey( s3Object.Key );
            string uriKey = System.Web.HttpUtility.UrlPathEncode( s3Object.Key );
            
            return new Asset
            {
                Name = name,
                Key = s3Object.Key,
                Uri = $"https://{s3Object.BucketName}.s3.{regionEndpoint}.amazonaws.com/{uriKey}",
                Type = GetAssetType( s3Object.Key ),
                IconPath = GetFileTypeIcon( s3Object.Key ),
                FileSize = s3Object.Size,
                LastModifiedDateTime = s3Object.LastModified,
                Description = s3Object.StorageClass == null ? string.Empty : s3Object.StorageClass.ToString(),
            };
        }

        /// <summary>
        /// Creates the asset from AWS S3 Client GetObjectResponse.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="regionEndpoint">The region endpoint.</param>
        /// <returns></returns>
        private Asset CreateAssetFromGetObjectResponse( GetObjectResponse response, string regionEndpoint )
        {
            string name = GetNameFromKey( response.Key );
            string uriKey = System.Web.HttpUtility.UrlPathEncode( response.Key );

            return new Asset
            {
                Name = name,
                Key = response.Key,
                Uri = $"https://{response.BucketName}.s3.{regionEndpoint}.amazonaws.com/{uriKey}",
                Type = GetAssetType( response.Key ),
                IconPath = GetFileTypeIcon( response.Key ),
                FileSize = response.ResponseStream.Length,
                LastModifiedDateTime = response.LastModified,
                Description = response.StorageClass == null ? string.Empty : response.StorageClass.ToString(),
                AssetStream = response.ResponseStream
            };
        }

        /// <summary>
        /// Creates a folder asset using a commonPrefix from AWS.
        /// </summary>
        /// <param name="commonPrefix">The common prefix.</param>
        /// <param name="regionEndpoint">The region endpoint.</param>
        /// <param name="bucketName">Name of the bucket.</param>
        /// <returns></returns>
        private Asset CreateAssetFromCommonPrefix( string commonPrefix, string regionEndpoint, string bucketName )
        {
            string uriKey = System.Web.HttpUtility.UrlPathEncode( commonPrefix );
            string name = GetNameFromKey( commonPrefix );

            return new Asset
            {
                Name = name,
                Key = commonPrefix,
                Uri = $"https://{bucketName}.s3.{regionEndpoint}.amazonaws.com/{uriKey}",
                Type = AssetType.Folder,
                IconPath = GetFileTypeIcon( commonPrefix ),
                FileSize = 0,
                LastModifiedDateTime = null,
                Description = string.Empty
            };
        }

        /// <summary>
        /// Determines whether Asset has the required elements for an AWS file.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <exception cref="Exception">Asset Type is set to 'Folder' instead of 'File.'</exception>
        private void HasRequirementsFile( Asset asset )
        {
            if ( asset.Type == AssetType.Folder )
            {
                throw new Exception( "Asset Type is set to 'Folder' instead of 'File.'" );
            }
        }

        /// <summary>
        /// Determines whether the Asset has the required elements for a AWS folder.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <exception cref="Exception">
        /// Asset Type is set to 'File' instead of 'Folder.'
        /// or
        /// Name and key cannot both be null or empty.
        /// </exception>
        private void HasRequirementsFolder( Asset asset )
        {
            if ( asset.Type == AssetType.File )
            {
                throw new Exception( "Asset Type is set to 'File' instead of 'Folder.'" );
            }

            if ( asset.Name.IsNullOrWhiteSpace() && asset.Key.IsNullOrWhiteSpace() )
            {
                throw new Exception( "Name and key cannot both be null or empty." );
            }
        }

        /// <summary>
        /// Makes adjustments to the Key string based on the root folder, the name, and the AssetType.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="rootFolder">The root folder.</param>
        /// <returns></returns>
        private string FixKey( Asset asset, string rootFolder )
        {
            if ( asset.Key.IsNullOrWhiteSpace() && asset.Name.IsNullOrWhiteSpace() )
            {
                asset.Key = "/";
            }
            else if ( asset.Key.IsNullOrWhiteSpace() && asset.Name.IsNotNullOrWhiteSpace() )
            {
                asset.Key = rootFolder + asset.Name;
            }

            if ( asset.Type == AssetType.Folder )
            {
                asset.Key = asset.Key.EndsWith( "/" ) == true ? asset.Key : asset.Key += "/";
            }

            return asset.Key;
        }

        /// <summary>
        /// Gets the name from key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string GetNameFromKey( string key )
        {
            if ( key.LastIndexOf('/') < 1)
            {
                return key;
            }

            string[] pathSegments = key.Split( '/' );

            if (key.EndsWith("/"))
            {
                return pathSegments[pathSegments.Length - 2];
            }

            return pathSegments[pathSegments.Length - 1];
        }

        /// <summary>
        /// Gets the path from key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The file prefix used by AWS to mimic a folder structure.</returns>
        private string GetPathFromKey( string key )
        {
            int i = key.LastIndexOf( '/' );
            if ( i < 1)
            {
                return string.Empty;
            }

            return key.Substring( 0, i + 1 );
        }

        /// <summary>
        /// Determine the correct AssetType based on the provided name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private AssetType GetAssetType( string name )
        {
            if ( name.EndsWith("/"))
            {
                return AssetType.Folder;
            }

            return AssetType.File;
        }

        /// <summary>
        /// Returns an Amazon S3 Client obj using the settings in the provided AssetStorageSystem obj.
        /// </summary>
        /// <param name="assetStorageSystem">The asset storage system.</param>
        /// <returns></returns>
        private AmazonS3Client GetAmazonS3Client( AssetStorageSystem assetStorageSystem )
        {

            string awsAccessKey = GetAttributeValue( assetStorageSystem, "AWSAccessKey" );
            string awsSecretKey = GetAttributeValue( assetStorageSystem, "AWSSecretKey" );
            string awsRegion = GetAttributeValue( assetStorageSystem, "AWSRegion" );
            RegionEndpoint regionEndPoint = Amazon.RegionEndpoint.GetBySystemName( awsRegion );

            return new AmazonS3Client( awsAccessKey, awsSecretKey, regionEndPoint );
        }

        #endregion Private Methods

    }
}
