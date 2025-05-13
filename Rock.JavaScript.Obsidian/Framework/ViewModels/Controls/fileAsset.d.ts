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

/**
 * TODO
 */
export type FileAsset = {
    /** DO NOT TRUST. ID for the AssetStoragePriver this files resides in */
    assetStorageProviderId: number,

    /** The name of the file */
    name: string,

    /** An identifier for the file. It contains the full path to the file within the provider */
    key: string,

    /** Public URI to view/download the file. */
    uri: string,

    /** Whether it's a folder or a file. 0 is a file */
    type: 0,

    /** Relative URL for the thumbnail of this file */
    iconPath: string,

    /** Size of the file in bytes */
    fileSize: number,

    /** Size of the file as a formatted string */
    formattedFileSize: string,

    /** ISO Date/time the file was last modified */
    lastModifiedDateTime: string,

    /** Description of the file */
    description: string
};
