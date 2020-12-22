/**
* Make an API call. This is only place Axios (or AJAX library) should be referenced to allow tools like performance metrics to provide
* better insights.
* @param {string} method The HTTP method, such as GET
* @param {string} url The endpoint to access, such as /api/campuses/
* @param {object} params Query parameter object.  Will be converted to ?key1=value1&key2=value2 as part of the URL.
* @param {any} data This will be the body of the request
*/
export async function doApiCall(method, url, params, data) {
    // eslint-disable-next-line
    // @ts-ignore
    return await axios({ method, url, data, params });
}

/**
* Make a GET HTTP request
* @param {string} url The endpoint to access, such as /api/campuses/
* @param {object} params Query parameter object.  Will be converted to ?key1=value1&key2=value2 as part of the URL.
*/
export async function get(url, params = undefined) {
    return await doApiCall('GET', url, params, undefined);
}

/**
* Make a POST HTTP request
* @param {string} url The endpoint to access, such as /api/campuses/
* @param {object} params Query parameter object.  Will be converted to ?key1=value1&key2=value2 as part of the URL.
* @param {any} data This will be the body of the request
*/
export async function post(url, params, data) {
    return await doApiCall('POST', url, params, data);
}

export default {
    doApiCall,
    post,
    get
};