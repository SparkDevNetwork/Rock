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
