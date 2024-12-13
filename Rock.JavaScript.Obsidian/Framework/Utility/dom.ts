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
 * Get a unique CSS selector for any DOM element.
 */
export function getUniqueCssSelector(el: Element): string {
    const path: string[] = [];
    let parent: Element | null = el.parentNode as Element;

    while (parent) {
        path.unshift(`${el.tagName}:nth-child(${([] as Element[]).indexOf.call(parent.children, el) + 1})`);
        el = parent;
        parent = el.parentNode as Element;
    }
    return `${path.join(" > ")}`.toLowerCase();
}