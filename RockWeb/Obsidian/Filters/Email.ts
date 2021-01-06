/**
 * Is the value a valid email address?
 * @param val
 */
export function isEmail(val: unknown) {
    if (typeof val === 'string') {
        const re = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        return re.test(val.toLowerCase());
    }

    return false;
}