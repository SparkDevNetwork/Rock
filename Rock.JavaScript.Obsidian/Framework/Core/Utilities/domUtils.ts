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

    // Set the innerHTML of the target element. Vue will not execute scripts
    // that are added this way, so we need to handle them manually.
    element.innerHTML = html;

    // Collect all external scripts (those with a `src` attribute).
    const externalScriptEls: HTMLScriptElement[] = [];

    // Collect inline scripts (those without a `src` attribute).
    const inlineScriptEls: HTMLScriptElement[] = [];

    Array.from(element.querySelectorAll("script"))
        .forEach(scriptEl => {
            if (scriptEl.src) {
                externalScriptEls.push(scriptEl);
            }
            else {
                inlineScriptEls.push(scriptEl);
            }
        });

    // Load the external scripts one by one and wait for them to load.
    const loadPromises = externalScriptEls.map(oldScriptEl => {
        return new Promise<void>((resolve, reject) => {
            const newScriptEl = document.createElement("script");
            Array.from(oldScriptEl.attributes).forEach(attr => {
                newScriptEl.setAttribute(attr.name, attr.value);
            });

            // When the script has loaded, resolve the promise.
            newScriptEl.onload = () => resolve();
            newScriptEl.onerror = () => reject(new Error(`Failed to load script: ${oldScriptEl.src}`));

            // Append the new script to the target element to trigger loading.
            oldScriptEl.parentNode?.replaceChild(newScriptEl, oldScriptEl);
        });
    });

    // Once all external scripts are loaded, execute the inline scripts.
    Promise.all(loadPromises)
        .then(() => {
            inlineScriptEls.forEach(oldScriptEl => {
                const newScriptEl = document.createElement("script");
                Array.from(oldScriptEl.attributes).forEach(attr => {
                    newScriptEl.setAttribute(attr.name, attr.value);
                });

                const scriptText = document.createTextNode(oldScriptEl.innerHTML);
                newScriptEl.appendChild(scriptText);

                // Append the inline script to the target element.
                oldScriptEl.parentNode?.replaceChild(newScriptEl, oldScriptEl);
            });
        })
        .catch(error => {
            console.error(error);
        });
}
