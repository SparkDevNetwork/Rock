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

import { useHttp } from "./http";
import { StructuredContentEditorConfigurationBag } from "@Obsidian/ViewModels/Rest/Controls/structuredContentEditorConfigurationBag";
import { StructuredContentEditorGetConfigurationOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/structuredContentEditorGetConfigurationOptionsBag";

const http = useHttp();

/** Fetches the configuration for the structured content editor. */
export async function getStructuredContentEditorConfiguration(options: StructuredContentEditorGetConfigurationOptionsBag): Promise<StructuredContentEditorConfigurationBag> {
    const result = await http.post<StructuredContentEditorConfigurationBag>("/api/v2/Controls/StructuredContentEditorGetConfiguration", undefined, options);

    if (result.isSuccess && result.data) {
        return result.data;
    }

    throw new Error(result.errorMessage || "Error fetching structured content editor configuration");
}

export default {
    getStructuredContentEditorConfiguration
};