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

/** How a step back was made. */
export type BackStepOptions = {
    /** The browser drew the step itself, as an edge swipe does, so the chat draws it at once. */
    isDrawnByBrowser: boolean;
};

/** The one owner of the chat's history entries on a phone. */
export type BackStack = {
    /**
     * The frame's screens, registered once: how many steps deep they are, and how to step back.
     *
     * @param count How many screen steps are drawn over the list.
     * @param back Steps back by the count given.
     */
    screens: (count: () => number, back: (steps: number, options: BackStepOptions) => void) => void;

    /**
     * A layer opened over the screens, as a sheet or a turned page.
     *
     * @param back Asks the layer's owner to close it.
     *
     * @returns Lets the layer go, once it has closed.
     */
    hold: (back: () => void) => () => void;
};

/** What the back stack reads of the window, a fake for a spec. */
export type BackStackHost = {
    history: Pick<History, "state" | "pushState" | "replaceState" | "go">;
    addEventListener: (type: "popstate", listener: (event: PopStateEvent) => void) => void;
    removeEventListener: (type: "popstate", listener: (event: PopStateEvent) => void) => void;
    setTimeout: (run: () => void, ms: number) => number;
    clearTimeout: (id: number | undefined) => void;
};

export function createBackStack(_isActive: () => boolean, _host: BackStackHost): BackStack {
    return { screens: () => undefined, hold: () => () => undefined };
}
