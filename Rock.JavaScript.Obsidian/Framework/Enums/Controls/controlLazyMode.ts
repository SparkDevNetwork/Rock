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

export const ControlLazyMode = {
    /**
     * Loading begins when the control is instantiated but does not delay
     * the page loading.
     */
    Lazy: "lazy",

    /**
     * Loading begins when the control is instantiated and the page loading
     * is delayed until the control has finished.
     */
    Eager: "eager",

    /**
     * Loading begins when the user interacts with the control for the
     * first time.
     */
    OnDemand: "onDemand"
} as const;

export type ControlLazyMode = typeof ControlLazyMode[keyof typeof ControlLazyMode];
