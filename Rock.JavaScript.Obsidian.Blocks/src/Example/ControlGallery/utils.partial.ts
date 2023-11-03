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
import { Ref, isRef } from "vue";
import { PickerDisplayStyle } from "@Obsidian/Enums/Controls/pickerDisplayStyle";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

/**
 * Generate a string of an import statement that imports the control will the given file name.
 * The control's name will be based off the filename
 *
 * @param fileName Name of the control's file
 * @returns A string of code that can be used to import the given control file
 */
export function getControlImportPath(fileName: string): string {
    return `import ${upperCaseFirstCharacter(fileName)} from "@Obsidian/Controls/${fileName}";`;
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

/**
 * Generate a string of an import statement that imports the template will the given file name.
 * The template's name will be based off the filename
 *
 * @param fileName Name of the control's file
 * @returns A string of code that can be used to import the given control file
 */
export function getTemplateImportPath(fileName: string): string {
    return `import ${upperCaseFirstCharacter(fileName)} from "@Obsidian/Templates/${fileName}";`;
}

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

    return name.replace(/[A-Z]/g, " $&")
        .replace(/^[a-z]/, m => m.toUpperCase())
        .replace(/\.partial$/, "")
        .replace(/Gallery$/, "")
        .trim();
}

/**
 * Takes an element name and a collection of attribute keys and values and
 * constructs the example code. This can be used inside a computed call to
 * have the example code dynamically match the selected settings.
 *
 * @param elementName The name of the element to use in the example code.
 * @param attributes The attribute names and values to append to the element name.
 *
 * @returns A string of valid HTML content for how to use the component.
 */
export function buildExampleCode(elementName: string, attributes: Record<string, Ref<unknown> | unknown>): string {
    const attrs: string[] = [];

    for (const attr in attributes) {
        let value = attributes[attr];
        console.log("attributes", attr, value);

        if (isRef(value)) {
            value = value.value;
        }

        if (typeof value === "string") {
            attrs.push(`${attr}="${value}"`);
        }
        else if (typeof value === "number") {
            attrs.push(`:${attr}="${value}"`);
        }
        else if (typeof value === "boolean") {
            attrs.push(`:${attr}="${value ? "true" : "false"}"`);
        }
        else if (value === undefined || value === null) {
            /* Do nothing */
        }
    }

    console.log(attrs);

    return `<${elementName} ${attrs.join(" ")} />`;
}

export const displayStyleItems: ListItemBag[] = [
    {
        value: PickerDisplayStyle.Auto,
        text: "Auto"
    },
    {
        value: PickerDisplayStyle.List,
        text: "List"
    },
    {
        value: PickerDisplayStyle.Condensed,
        text: "Condensed"
    }
];