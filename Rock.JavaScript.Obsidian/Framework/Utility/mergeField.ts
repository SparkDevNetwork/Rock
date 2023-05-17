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

import { MergeFieldPickerFormatSelectedValueOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/mergeFieldPickerFormatSelectedValueOptionsBag";
import { useHttp } from "./http";

/**
 * Take a given mergeFieldPicker value and format it for Lava
 *
 * @param value The merge field to be formatted
 *
 * @returns The formatted string in a Promise
 */
export async function formatValue(value: string): Promise<string> {
    const http = useHttp();

    const options: MergeFieldPickerFormatSelectedValueOptionsBag = {
        selectedValue: value
    };

    const response = await http.post<string>("/api/v2/Controls/MergeFieldPickerFormatSelectedValue", {}, options);

    if (response.isSuccess && response.data) {
        return response.data;
    }
    else {
        console.error("Error", response.errorMessage || `Error formatting '${value}'.`);
        return "";
    }
}