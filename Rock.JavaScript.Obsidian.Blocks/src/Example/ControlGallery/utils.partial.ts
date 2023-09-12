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

import { upperCaseFirstCharacter } from "@Obsidian/Utility/stringUtils";

/**
 * Takes a gallery component's name and converts it to a name that is useful for the header and
 * sidebar by adding spaces and stripping out the "Gallery" suffix
 *
 * @param name Name of the control
 * @returns A string of code that can be used to import the given control file
 */
export function convertComponentName(name: string | undefined | null): string {
    if (!name) {
        return "Unknown Component";
    }

    return name.replace(/[A-Z]/g, " $&").replace(/Gallery$/, "").trim();
}

/**
 * Generate a string of an import statement that imports the SFC control will the given file name.
 * The control's name will be based off the filename
 *
 * @param fileName Name of the control's file
 * @returns A string of code that can be used to import the given control file
 */
export function getSfcControlImportPath(fileName: string): string {
    return `import ${upperCaseFirstCharacter(fileName)} from "@Obsidian/Controls/${fileName}.obs";`;
}