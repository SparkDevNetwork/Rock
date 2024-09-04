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

import { Workbook } from "exceljs";
import { useHttp } from "./http";
import Cache from "./cache";

/**
 * Triggers an automatic download of the workbook so it can be saved to
 * the filesystem.
 *
 * @param workbook The workbook to be downloaded by the browser.
 * @param title The title of the workbook, this is used as the base for the filename.
 * @param format The format to use when downloading the workbook.
 */
export async function downloadWorkbook(workbook: Workbook, title: string, format: "csv" | "xlsx"): Promise<void> {
    // Get the export data.
    const buffer = format === "xlsx"
        ? await workbook.xlsx.writeBuffer()
        : await workbook.csv.writeBuffer();

    // Create the URL that contains the file data.
    const url = URL.createObjectURL(new Blob([buffer], {
        type: "application/octet-stream"
    }));

    // Create a fake hyperlink to simulate an attempt to download a file.
    const element = document.createElement("a");
    element.innerText = "Download";
    element.style.position = "absolute";
    element.style.top = "-100px";
    element.style.left = "0";
    element.href = url;
    element.download = `${title.replace(/[^a-zA-Z0-9\-_]/g, "")}.${format}`;
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