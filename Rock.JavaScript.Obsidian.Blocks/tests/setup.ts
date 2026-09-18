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

/*
    8/27/26 - CLAUDE

    jsdom does not implement the layout-dependent portions of the DOM, so a
    handful of geometry and scrolling APIs that our controls call during normal
    operation are simply missing. Without these polyfills the missing functions
    throw ("document.elementsFromPoint is not a function"), which aborts the
    Vue render/update cycle and fails tests that never intended to exercise
    scrolling behavior. We stub them here so the controls run to completion; the
    layout results themselves are meaningless in jsdom, which is acceptable
    because these tests assert on component state, not scroll position.

    Reason: Polyfill jsdom layout/scroll gaps so tests are not broken by them.
*/

// Hit-testing APIs. jsdom has no layout engine, so nothing is ever "at a point".
if (typeof document.elementsFromPoint !== "function") {
    document.elementsFromPoint = (): Element[] => [];
}

if (typeof document.elementFromPoint !== "function") {
    document.elementFromPoint = (): Element | null => null;
}

// Scrolling APIs. jsdom leaves these unimplemented (it logs "Not implemented"
// errors), so we replace them with no-ops.
window.scrollTo = (): void => {
    // Intentionally does nothing: there is no viewport to scroll in jsdom.
};

window.scrollBy = (): void => {
    // Intentionally does nothing: there is no viewport to scroll in jsdom.
};

if (typeof Element.prototype.scrollIntoView !== "function") {
    Element.prototype.scrollIntoView = (): void => {
        // Intentionally does nothing: there is no viewport to scroll in jsdom.
    };
}
