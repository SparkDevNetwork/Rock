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


// NOTE: Do not make this public yet. This is essentially temporary and
// will likely move to a different place and be merged with the tooltip
// concept code as well.
type PopoverOptions = {
    /** Allow HTML content in the popover. */
    html?: boolean;

    /** Enables santization of HTML content. */
    sanitize?: boolean;
};

// eslint-disable-next-line @typescript-eslint/no-explicit-any
declare const $: any;

/**
 * Configure a popover for the specified node or nodes to show on hover. This
 * currently uses Bootstrap popovers but may be changed to use a different
 * method later.
 * 
 * @param node The node or nodes to have popovers configured on.
 * @param options The options that describe how the popovers should behave.
 */
export function popover(node: Element | Element[], options?: PopoverOptions): void {
    // If we got an array of elements then activate each one.
    if (Array.isArray(node)) {
        for (const n of node) {
            popover(n, options);
        }

        return;
    }

    $(node).popover({
        html: options?.html,
        sanitize: options?.sanitize ?? true
    });
}
