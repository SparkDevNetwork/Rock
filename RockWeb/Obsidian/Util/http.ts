import axios from "../Vendor/Axios/index.js";

export type HttpMethod = 'GET' | 'POST';
export type HttpUrlParams = object | undefined | null;
export type HttpBodyData = object | undefined | null;

export type HttpResult<T> = {
    statusCode: number;
    data: T | null;
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
        data,
        params
    });
}

/**
* Make an API call
* @param {string} method The HTTP method, such as GET
* @param {string} url The endpoint to access, such as /api/campuses/
* @param {object} params Query parameter object.  Will be converted to ?key1=value1&key2=value2 as part of the URL.
* @param {any} data This will be the body of the request
*/
export async function doApiCall<T>(method: HttpMethod, url: string, params: HttpUrlParams = undefined, data: HttpBodyData = undefined): Promise<HttpResult<T>> {
    try {
        const result = await doApiCallRaw(method, url, data, params);

        return {
            data: result.data as T,
            isError: false,
            statusCode: result.status
        } as HttpResult<T>;
    }
    catch (e) {
        if (e && e.response && e.response.data && e.response.data.Message) {
            return {
                data: null,
                isError: true,
                statusCode: e.response.status,
                errorMessage: e.response.data.Message
            } as HttpResult<T>;
        }

        return {
            data: null,
            isError: true,
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
export async function get<T>(url: string, params: HttpUrlParams = undefined) {
    return await doApiCall<T>('GET', url, params, undefined);
}

/**
* Make a POST HTTP request
* @param {string} url The endpoint to access, such as /api/campuses/
* @param {object} params Query parameter object.  Will be converted to ?key1=value1&key2=value2 as part of the URL.
* @param {any} data This will be the body of the request
*/
export async function post<T>(url: string, params: HttpUrlParams = undefined, data: HttpBodyData = undefined) {
    return await doApiCall<T>('POST', url, params, data);
}

export default {
    doApiCall,
    post,
    get
};