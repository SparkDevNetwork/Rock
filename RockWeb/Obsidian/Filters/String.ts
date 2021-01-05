/**
 * Is the value an empty string?
 * @param val
 */
export function isEmpty(val: unknown) {
    if (typeof val === 'string') {
        return val.length === 0;
    }

    return false;
}

/**
 * Is the value an empty string?
 * @param val
 */
export function isWhitespace(val: unknown) {
    if (typeof val === 'string') {
        return val.trim().length === 0;
    }

    return false;
}

/**
 * Is the value null, undefined or whitespace?
 * @param val
 */
export function isNullOrWhitespace(val: unknown) {
    return isWhitespace(val) || val === undefined || val === null;
}

/**
 * Turns "MyCamelCaseString" into "My Camel Case String"
 * @param val
 */
export function splitCamelCase(val: unknown) {
    if (typeof val === 'string') {
        return val.replace(/([a-z])([A-Z])/g, '$1 $2');
    }

    return val;
}