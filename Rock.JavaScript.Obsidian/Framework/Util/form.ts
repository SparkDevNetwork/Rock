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

import { inject, provide } from "vue";

/** The unique symbol used when injecting the form state. */
const formStateSymbol = Symbol();

/**
 * Holds the state of a single form on the page along with any callback methods
 * that can be used to interact with the form.
 */
export type FormState = {
    /** The number of submissions the form has had. */
    submitCount: number;

    /** Sets the current error for the given field name. A blank error means no error. */
    setError: (name: string, error: string) => void;
};

/**
 * Provides the form state for any child components that need access to it.
 * 
 * @param state The state that will be provided to child components.
 */
export function provideFormState(state: FormState): void {
    provide(formStateSymbol, state);
}

/**
 * Makes use of the FormState that was previously provided by a parent component.
 *
 * @returns The form state or undefined if it was not available.
 */
export function useFormState(): FormState | undefined {
    return inject<FormState>(formStateSymbol);
}
