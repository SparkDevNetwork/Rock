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

import { InjectionKey, Ref, inject, provide } from "vue";

export type CodeBoxCharacterController = {
    focus(): void;
    clear(): void;
    boxIndex: number;
};

export type Breakpoint = "xs" | "sm" | "md" | "lg" | "xl" | "unknown";

/**
 * An injection key to provide a reactive bootstrap breakpoint to descendent components.
 */
const bootstrapBreakpointInjectionKey: InjectionKey<Ref<Breakpoint>> = Symbol("bootstrap-breakpoint");

/**
 * Provides the reactive breakpoint that can be used by child components.
 */
export function provideBreakpoint(breakpoint: Ref<Breakpoint>): void {
    provide(bootstrapBreakpointInjectionKey, breakpoint);
}

/**
 * Gets the breakpoint that can be used to provide responsive behavior.
 */
export function useBreakpoint(): Ref<Breakpoint> {
    const breakpoint = inject(bootstrapBreakpointInjectionKey);

    if (!breakpoint) {
        throw "provideBreakpoint must be invoked before useBreakpoint can be used. If useBreakpoint is in a component where <BreakpointObserver> is used in the template, then use the @breakpoint output binding to get the breakpoint.";
    }

    return breakpoint;
}