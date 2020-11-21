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
using System.Linq;
using System.Net;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using GoogleObject = Google.Apis.Storage.v1.Data.Object;

namespace Rock.Storage.Common
{
    internal static class GoogleCloudStorage
    {
        #region Static Methods

        /// <summary>
        /// Gets the storage client.
        /// </summary>
        /// <param name="accountKeyJson">The account key JSON.</param>
        /// <returns></returns>
        internal static StorageClient GetStorageClient( string accountKeyJson )
        {
            var googleCredential = GoogleCredential.FromJson( accountKeyJson );
            var storageClient = StorageClient.Create( googleCredential );
            return storageClient;
        }

        /// <summary>
        /// Gets the objects from Google.
        /// </summary>
        /// <param name="bucketName">Name of the bucket.</param>
        /// <param name="accountKeyJson">The account key JSON.</param>
        /// <param name="directory">The directory.</param>
        /// <param name="includeOnlyFiles">if set to <c>true</c> [include only files].</param>
        /// <param name="includeOnlyFolders">if set to <c>true</c> [include only folders].</param>
        /// <param name="allowRecursion">if set to <c>true</c> [allow recursion].</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Only one of {nameof( includeOnlyFiles )} or {nameof( includeOnlyFolders )} can be true</exception>
        internal static List<GoogleObject> GetObjectsFromGoogle( string bucketName, string accountKeyJson, string directory, bool includeOnlyFiles,
            bool includeOnlyFolders, bool allowRecursion )
        {
            if ( includeOnlyFiles && includeOnlyFolders )
            {
                throw new ArgumentException( $"Only one of {nameof( includeOnlyFiles )} or {nameof( includeOnlyFolders )} can be true" );
            }

            var delimiter = includeOnlyFiles ? "/" : string.Empty;

            var initialDepth = 0;

            // If the directory is root "/" then Google won't return anything
            if ( directory == "/" )
            {
                directory = string.Empty;
            }
            else
            {
                // The initial depth is for the things inside the directory, which means it's the depth of the directory plus 1
                initialDepth = GetKeyDepth( directory ) + 1;
            }

            using ( var client = GetStorageClient( accountKeyJson ) )
            {
                // Get the objects from Google and transform them into assets
                var response = client.ListObjects( bucketName, directory, new ListObjectsOptions
                {
                    Delimiter = delimiter
                } );

                var objects = response.ToList();

                if ( includeOnlyFolders )
                {
                    var parentFoldersToAdd = new List<GoogleObject>();

                    // Depending on how the folder was created, it may not have an actual object, just objects nested inside.
                    // That means we have to infer the existence of folders based on the paths of the objects within.
                    objects.ForEach( o =>
                    {
                        if ( !o.Name.EndsWith( "/" ) )
                        {
                            var indexOfLastSlash = o.Name.LastIndexOf( '/' );
                            o.Name = o.Name.Remove( indexOfLastSlash + 1 );
                        }

                        var folderPath = o.Name;
                        var folderDivider = folderPath.IndexOf( "/" );
                        while ( folderDivider > 0 )
                        {
                            folderPath = folderPath.Substring( 0, folderDivider );
                            parentFoldersToAdd.Add( new GoogleObject
                            {
                                Name = folderPath + "/",
                            } );
                            folderDivider = folderPath.IndexOf( "/" );
                        }
                    } );
                    objects.AddRange( parentFoldersToAdd );
                    objects = objects.GroupBy( o => o.Name ).Select( g => g.First() ).ToList();
                    objects.RemoveAll( o => !o.Name.EndsWith( "/" ) );
                }

                // If recursion is not allowed, then remove objects that are beyond the initial depth
                if ( !allowRecursion )
                {
                    objects.RemoveAll( o => GetKeyDepth( o.Name ) != initialDepth );
                }

                // Google includes the root directory of the listing request, but Rock does not expect to get "self" in the list
                directory = directory.EndsWith( "/" ) ? directory : $"{directory}/";
                objects.RemoveAll( o => o.Name == directory );

                return objects;
            }
        }

        /// <summary>
        /// Deletes the object.
        /// </summary>
        /// <param name="bucketName">Name of the bucket.</param>
        /// <param name="accountKeyJson">The account key JSON.</param>
        /// <param name="isFolder">if set to <c>true</c> [is folder].</param>
        /// <param name="path">The directory.</param>
        internal static void DeleteObject( string bucketName, string accountKeyJson, bool isFolder, string path )
        {
            var objectsToDelete = new List<GoogleObject>();

            using ( var client = GetStorageClient( accountKeyJson ) )
            {
                if ( isFolder )
                {
                    // To delete a folder from Google, delete everything inside as well
                    var objectsInDirectory = GetObjectsFromGoogle( bucketName, accountKeyJson, path, false, false, true );
                    objectsToDelete.AddRange( objectsInDirectory );
                }

                // Get the whole object so that the Generation property is set and the object is permanently deleted
                try
                {
                    var folderObject = client.GetObject( bucketName, path );
                    objectsToDelete.Add( folderObject );
                }
                catch ( GoogleApiException e )
                {
                    if ( e.HttpStatusCode == HttpStatusCode.NotFound )
                    {
                        // Sometimes there is no folder object, just files nested within
                    }
                    else
                    {
                        throw;
                    }
                }

                foreach ( var objectToDelete in objectsToDelete )
                {
                    client.DeleteObject( objectToDelete );
                }
            }
        }

        #endregion Static Methods

        #region Private Methods

        /// <summary>
        /// Gets the key depth. For example a/b/c/d.txt would have a depth of 3.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private static int GetKeyDepth( string key )
        {
            if ( key.IsNullOrWhiteSpace() )
            {
                return 0;
            }

            key = key.Trim();

            // Remove the last character because folders like "a/b/" are actually at depth 1 and sibling to files like "a/1.txt"
            if ( key.EndsWith( "/" ) )
            {
                key = key.Substring( 0, key.Trim().Length - 1 );
            }

            var depth = key.Count( c => c == '/' );
            return depth;
        }

        #endregion Private Methods
    }
}
