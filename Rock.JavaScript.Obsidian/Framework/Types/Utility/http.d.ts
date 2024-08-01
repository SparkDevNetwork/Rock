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

export type HttpMethod = "GET" | "POST" | "PUT" | "DELETE" | "PATCH";
export type HttpUrlParams = Record<string, unknown> | undefined | null;
export type HttpBodyData = Record<string, unknown> | undefined | null;

export type HttpResult<T> = {
    statusCode: number;
    data: T | null;
    isSuccess: boolean;
    isError: boolean;
    errorMessage: string | null;
};

export type HttpDoApiCallFunc = <T>(method: HttpMethod, url: string, params?: HttpUrlParams, data?: HttpBodyData) => Promise<HttpResult<T>>;

export type HttpGetFunc = <T>(url: string, params?: HttpUrlParams) => Promise<HttpResult<T>>;

export type HttpPostFunc = <T>(url: string, params?: HttpUrlParams, data?: HttpBodyData) => Promise<HttpResult<T>>;

export type HttpFunctions = {
    doApiCall: HttpDoApiCallFunc;
    get: HttpGetFunc;
    post: HttpPostFunc;
};
