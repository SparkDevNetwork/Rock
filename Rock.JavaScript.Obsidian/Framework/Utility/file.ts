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

import { useHttp } from "./http";
import Cache from "./cache";

/**
 * Triggers an automatic download of the data so it can be saved to the
 * filesystem.
 *
 * @param data The data to be downloaded by the browser.
 * @param filename The name of the filename to suggest to the browser.
 */
export async function downloadFile(data: Blob, filename: string): Promise<void> {
    // Create the URL that contains the file data.
    const url = URL.createObjectURL(data);

    // Create a fake hyperlink to simulate an attempt to download a file.
    const element = document.createElement("a");
    element.innerText = "Download";
    element.style.position = "absolute";
    element.style.top = "-100px";
    element.style.left = "0";
    element.href = url;
    element.download = filename;
    document.body.appendChild(element);
    element.click();
    document.body.removeChild(element);

    setTimeout(() => URL.revokeObjectURL(url), 100);
}

/**
 * Fetch the global attribute configuration for image file extensions from the server as an array of
 * file extension strings.
 */
async function fetchImageFileExtensions(): Promise<string[] | null> {
    const http = useHttp();
    const result = await http.post<string>("/api/v2/Utilities/GetImageFileExtensions");

    if (result.isSuccess && result.data) {
        return result.data.split(",");
    }

    return null;
}

/**
 * Fetch the global attribute configuration for image file extensions from the server as an array of
 * file extension strings.
 *
 * Cacheable version of fetchImageFileExtensions
 */
export const getImageFileExtensions = Cache.cachePromiseFactory("imageFileExtensions", fetchImageFileExtensions);

/**
 * Determine, based on the file's extension and the list of image file extensions (from the
 * server's configuration), whether the file is an image.
 */
export async function isImage(filename: string): Promise<boolean> {
    const imageExtensions = await getImageFileExtensions();
    const extension = filename.split(".").pop();
    return !!extension && (imageExtensions?.includes(extension.toLowerCase()) ?? false);
}
