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
    const controlName = upperCaseFirstCharacter(fileName.replace("internal/", ""));
    return `import ${controlName} from "@Obsidian/Controls/${fileName}";`;
}

/**
 * Generate a string of an import statement that imports the SFC control will the given file name.
 * The control's name will be based off the filename
 *
 * @param fileName Name of the control's file
 * @returns A string of code that can be used to import the given control file
 */
export function getSfcControlImportPath(fileName: string): string {
    const controlName = upperCaseFirstCharacter(fileName.replace("internal/", ""));
    return `import ${controlName} from "@Obsidian/Controls/${fileName}.obs";`;
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

/**
 * Converts a number to its corresponding word representation.
 * This is useful for generating labels or titles based on a number.
 *
 * @param index The index of the number to convert to a word. Should be between 1 and 10.
 *
 * @returns The word representation of the number.
 */
export function getNumberWord(index: number): string {
    const words = ["First", "Second", "Third", "Fourth", "Fifth", "Sixth", "Seventh", "Eighth", "Ninth", "Tenth"];

    return words[index - 1] || `${index}`;
}

/**
 * Returns a string representing an icon class based on the given index.
 * This is useful for generating icon classes dynamically in a gallery.
 *
 * @param index The index of the icon to retrieve. Should be between 1 and 10.
 *
 * @returns The icon class string.
 */
export function getIconNumber(index: number): string {
    const icons = ["ti ti-home", "ti ti-user", "ti ti-settings", "ti ti-star", "ti ti-bell", "ti ti-calendar", "ti ti-chart-bar", "ti ti-heart", "ti ti-search", "ti ti-file"];

    return icons[index - 1] || "ti ti-icons";
}

/**
 * Represents a component usage that can be used to generate example code for a
 * control. This handles the attributes and body of the component and properly
 * formats and indents the code for display.
 */
export class ComponentUsage {
    private readonly name: string;
    private readonly attributes: { value: string | boolean | number | null | undefined, name: string }[];
    private body: string = "";

    /**
     * Creates a new instance of the ComponentUsage class.
     *
     * @param name The name of the component to use in the generated code.
     */
    constructor(name: string) {
        this.name = name;
        this.attributes = [];
    }

    /**
     * Adds an attribute to the component usage.
     *
     * @param key The name of the attribute.
     * @param value The value of the attribute.
     * @param defaultValue The default value of the attribute. If the value matches this, it will not be added. Defaults to undefined.
     */
    public addAttribute(key: string, value: string | boolean | number | undefined | null, defaultValue: string | boolean | number | undefined = undefined): void {
        if (value === defaultValue) {
            return;
        }

        if (value === true) {
            this.attributes.push({
                value: undefined,
                name: key
            });
        }
        else {
            this.attributes.push({
                value: value,
                name: typeof value === "number" ? `:${key}` : key
            });
        }
    }

    /**
     * Converts an attribute to a string representation.
     * This handles different types of values and formats them appropriately.
     *
     * @param attribute The attribute to convert to a string.
     * @param index The index of the attribute in the list.
     *
     * @returns The string representation of the attribute.
     */
    private getAttributeString(attribute: { value: string | boolean | number | null | undefined, name: string }, index: number): string {
        let str = "";

        if (index > 0) {
            str += " ".repeat(this.name.length + 2);
        }

        if (attribute.value === undefined) {
            str += attribute.name;
        }
        else if (attribute.value === null) {
            str += `:${attribute.name}="null"`;
        }
        else if (typeof attribute.value === "number" || typeof attribute.value === "boolean") {
            str += `:${attribute.name}="${attribute.value}"`;

        }
        else {
            str += `${attribute.name}="${attribute.value}"`;
        }

        return str;
    }

    /**
     * Adds content to the body of the component usage.
     * If there is existing content, a newline is added before the new content.
     *
     * @param content The content to add to the body.
     */
    public addBody(content: string | ComponentUsage): void {
        if (this.body) {
            this.body += "\n";
        }

        if (typeof content === "string") {
            this.body += content;
        }
        else {
            this.body += content.toString();
        }
    }

    /**
     * Converts the component usage to a string representation.
     * This generates the full code for the component, including its attributes
     * and body.
     *
     * @returns The string representation of the component usage.
     */
    public toString(): string {
        let code = `<${this.name}`;

        const attributesString = this.attributes
            .map((attr, index) => `${this.getAttributeString(attr, index)}`)
            .join("\n");

        if (attributesString.length > 0) {
            code += ` ${attributesString}`;
        }

        if (this.body) {
            const body = this.body
                .split("\n")
                .map(line => `    ${line}`)
                .join("\n");

            code += `>\n${body}\n</${this.name}>`;
        }
        else {
            if (this.attributes.length > 0) {
                code += " ";
            }

            code += "/>";
        }

        return code;
    }
}
