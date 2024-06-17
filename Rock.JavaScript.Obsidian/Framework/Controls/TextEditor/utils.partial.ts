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

import { Editor } from "@Obsidian/Libs/tinymce";

/** Gets a button element from the toolbar. This should only be called after the editor is initialized. */
export function getToolbarButton(tooltip: string, parent?: HTMLElement | undefined): HTMLElement | null | undefined {
    // The only identifier TinyMCE adds to a toolbar button is the title (tooltip) attribute.
    // This function will need to be updated if a newer version of the TinyMCE library
    // is used and does things differently (doesn't add the tox-tbtn class or set the title attribute).
    return (parent ?? document).querySelector(`.tox-tbtn[title='${tooltip}']`) as HTMLElement;
}

/** Sets the enabled/disabled state of the editor. */
export function setEditorEnabled(editor: Editor, isEnabled: boolean): void {
    // Enable/disable the toolbar and border.
    editor.ui?.setEnabled(isEnabled);

    // Enable/disable the content.
    const editorBody = editor.getBody();
    if (editorBody) {
        editorBody.setAttribute("contenteditable", `${isEnabled}`);
    }
}