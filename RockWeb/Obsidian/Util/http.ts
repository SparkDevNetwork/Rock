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
import axios from 'axios';

export type HttpMethod = 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH';
export type HttpUrlParams = Record<string, unknown> | undefined | null;
export type HttpBodyData = Record<string, unknown> | undefined | null;

export type HttpResult<T> = {
    statusCode: number;
    data: T | null;
    isSuccess: boolean;
    isError: boolean;
    errorMessage: string | null;
};

/**
 * Make an API call. This is only place Axios (or AJAX library) should be referenced to allow tools like performance metrics to provide
* better insights.
 * @param method
 * @param url
 * @param params
 * @param data
 */
async function doApiCallRaw(method: HttpMethod, url: string, params: HttpUrlParams, data: HttpBodyData) {
    return await axios({
        method,
        url,
        params,
        data
    });
}

/**
* Make an API call
* @param {string} method The HTTP method, such as GET
* @param {string} url The endpoint to access, such as /api/campuses/
* @param {object} params Query parameter object.  Will be converted to ?key1=value1&key2=value2 as part of the URL.
* @param {any} data This will be the body of the request
*/
export async function doApiCall<T>( method: HttpMethod, url: string, params: HttpUrlParams = undefined, data: HttpBodyData = undefined ): Promise<HttpResult<T>>
{
    try
    {
        const result = await doApiCallRaw( method, url, params, data );

        return {
            data: result.data as T,
            isError: false,
            isSuccess: true,
            statusCode: result.status,
            errorMessage: null
        } as HttpResult<T>;
    }
    catch ( e )
    {
        if ( e?.response?.data?.Message )
        {
            return {
                data: null,
                isError: true,
                isSuccess: false,
                statusCode: e.response.status,
                errorMessage: e.response.data.Message
            } as HttpResult<T>;
        }

        return {
            data: null,
            isError: true,
            isSuccess: false,
            statusCode: e.response.status,
            errorMessage: null
        } as HttpResult<T>;
    }
}

/**
* Make a GET HTTP request
* @param {string} url The endpoint to access, such as /api/campuses/
* @param {object} params Query parameter object.  Will be converted to ?key1=value1&key2=value2 as part of the URL.
*/
export async function get<T>(url: string, params: HttpUrlParams = undefined): Promise<HttpResult<T>> {
    return await doApiCall<T>('GET', url, params, undefined);
}

/**
* Make a POST HTTP request
* @param {string} url The endpoint to access, such as /api/campuses/
* @param {object} params Query parameter object.  Will be converted to ?key1=value1&key2=value2 as part of the URL.
* @param {any} data This will be the body of the request
*/
export async function post<T>(url: string, params: HttpUrlParams = undefined, data: HttpBodyData = undefined): Promise<HttpResult<T>> {
    return await doApiCall<T>('POST', url, params, data);
}

export default {
    doApiCall,
    post,
    get
};