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

import { Guid } from "@Obsidian/Types";
import { HttpBodyData, HttpResult, HttpUrlParams } from "../Util/http";
import { inject } from "vue";

export type ConfigurationValues = Record<string, unknown>;

export type BlockConfig = {
    blockFileUrl: string;
    rootElement: Element;
    blockGuid: Guid;
    configurationValues: ConfigurationValues;
};

export type InvokeBlockActionFunc = <T>(actionName: string, data?: HttpBodyData) => Promise<HttpResult<T>>;

export type BlockHttpGet = <T>(url: string, params?: HttpUrlParams) => Promise<HttpResult<T>>;

export type BlockHttpPost = <T>(url: string, params?: HttpUrlParams, data?: HttpBodyData) => Promise<HttpResult<T>>;

export type BlockHttp = {
    get: BlockHttpGet;
    post: BlockHttpPost;
};



// TODO: Change these to use symbols

/**
 * Maps the block configuration values to the expected type.
 * 
 * @returns The configuration values for the block.
 */
export function useConfigurationValues<T>(): T {
    const result = inject<T>("configurationValues");

    if (result === undefined) {
        throw "Attempted to access block configuration outside of a RockBlock.";
    }

    return result;
}

/**
 * Gets the function that will be used to invoke block actions.
 *
 * @returns An instance of @see {@link InvokeBlockActionFunc}.
 */
export function useInvokeBlockAction(): InvokeBlockActionFunc {
    const result = inject<InvokeBlockActionFunc>("invokeBlockAction");

    if (result === undefined) {
        throw "Attempted to access block action invocation outside of a RockBlock.";
    }

    return result;
}
