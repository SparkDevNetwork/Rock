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

import { PropType } from "vue";

/**
 * A custom error that indicates an error occurred while processing the
 * interaction action data.
 */
export class InteractiveActionError extends Error {
    public readonly code: number;

    /**
     * Creates a new instance of {@link InteractiveActionError}.
     *
     * @param code The error code from the interaction action.
     * @param message The message describing how the state is invalid.
     */
    constructor(code: number, message: string) {
        super(message);
        this.name = "InteractiveActionError";
        this.code = code;
    }
}

/** The standard properties available on header cells. */
export type InteractiveActionProps = {
    /** The configuration options provided by the C# component. */
    configuration: {
        type: PropType<Record<string, string | null | undefined>>,
        required: true
    },

    /** The initial data values provided by the C# component. */
    data: {
        type: PropType<Record<string, string | null | undefined>>,
        required: true
    },

    /**
     * The function to call when the action should submit the data back
     * to the server. If the C# component returns an InteractiveActionExceptionBag
     * then it will be thrown as an InteractiveActionError.
     */
    submit: {
        type: PropType<(data: Record<string, string | null | undefined>) => Promise<void>>,
        required: true
    }
};

/** The standard properties available on header cells. */
export const interactiveActionProps: InteractiveActionProps = {
    configuration: {
        type: Object as PropType<Record<string, string | null | undefined>>,
        required: true
    },

    data: {
        type: Object as PropType<Record<string, string | null | undefined>>,
        required: true
    },

    submit: {
        type: Function as PropType<(data: Record<string, string | null | undefined>) => Promise<void>>,
        required: true
    }
};
