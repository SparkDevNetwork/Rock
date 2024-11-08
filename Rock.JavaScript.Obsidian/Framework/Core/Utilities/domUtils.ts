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
 * Sets an element's innerHTML property value in a way that allows admin-defined
 * JavaScript to run. This should only be used with trusted sources from the
 * server and not with content collected from non-authorized individuals.
 * https://stackoverflow.com/a/47614491
 *
 * This API is internal to Rock, and is not subject to the same compatibility
 * standards as public APIs. It may be changed or removed without notice in any
 * release. You should not use this API directly in any plug-ins. Doing so can
 * result in application failures when updating to a new Rock release.
 */
export function setInnerHTML(element: HTMLElement, html: string): void {
    if (!element) {
        return;
    }

    element.innerHTML = html;

    Array.from(element.querySelectorAll("script"))
        .forEach(oldScriptEl => {
            const newScriptEl = document.createElement("script");

            Array.from(oldScriptEl.attributes).forEach(attr => {
                newScriptEl.setAttribute(attr.name, attr.value);
            });

            const scriptText = document.createTextNode(oldScriptEl.innerHTML);
            newScriptEl.appendChild(scriptText);

            oldScriptEl.parentNode?.replaceChild(newScriptEl, oldScriptEl);
        });
}
