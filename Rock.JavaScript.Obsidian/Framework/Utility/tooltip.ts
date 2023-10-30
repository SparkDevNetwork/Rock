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
// will likely move to a different place and be merged with the popover
// concept code as well.
type TooltipOptions = {
    /** Allow HTML content in the tooltip. */
    html?: boolean;

    /** Enables santization of HTML content. */
    sanitize?: boolean;
};

// eslint-disable-next-line @typescript-eslint/no-explicit-any
declare const $: any;

/**
 * Configure a tooltip for the specified node or nodes to show on hover. This
 * currently uses Bootstrap tooltips but may be changed to use a different
 * method later.
 * 
 * @param node The node or nodes to have tooltips configured on.
 * @param options The options that describe how the tooltips should behave.
 */
export function tooltip(node: Element | Element[], options?: TooltipOptions): void {
    // If we got an array of elements then activate each one.
    if (Array.isArray(node)) {
        for (const n of node) {
            tooltip(n, options);
        }

        return;
    }

    $(node).tooltip({
        html: options?.html,
        sanitize: options?.sanitize ?? true
    });
}

/**
 * Manually show a previously-configured tooltip for the specified node.
 *
 * @param node The node for which to show a tooltip
 */
export function showTooltip(node: Element): void {
    $(node).tooltip("show");
}
