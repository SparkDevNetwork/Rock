/**
 * Adjust for the timezone offset so early morning times don't appear as the previous local day.
 * @param val
 */
function stripTimezone(val: Date) {
    const asUtc = new Date(val.getTime() + val.getTimezoneOffset() * 60000);
    return asUtc;
}

/**
 * Transform the value into a date or null
 * @param val
 */
export function asDateOrNull(val: unknown) {
    if (val === undefined || val === null) {
        return null;
    }

    if (val instanceof Date) {
        return val;
    }

    if (typeof val === 'string') {
        const ms = Date.parse(val);

        if (isNaN(ms)) {
            return null;
        }

        return stripTimezone(new Date(ms));
    }

    return null;
}

/**
 * Transforms the value into a string like '9/13/2001'
 * @param val
 */
export function asDateString(val: unknown) {
    const dateOrNull = asDateOrNull(val);

    if (!dateOrNull) {
        return '';
    }

    return dateOrNull.toLocaleDateString();
}