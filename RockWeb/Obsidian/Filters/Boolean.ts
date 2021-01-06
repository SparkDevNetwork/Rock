/**
 * Transform the value into true, false, or null
 * @param val
 */
export function asBooleanOrNull(val: unknown) {
    if (val === undefined || val === null) {
        return null;
    }

    if (typeof val === 'boolean') {
        return val;
    }

    if (typeof val === 'string') {
        const asString = (val || '').trim().toLowerCase();

        if (!asString) {
            return null;
        }

        return ['true', 'yes', 't', 'y', '1'].indexOf(asString) !== -1;
    }

    if (typeof val === 'number') {
        return !!val;
    }

    return null;
}

/**
 * Transform the value into true or false
 * @param val
 */
export function asBoolean(val: unknown) {
    return !!asBooleanOrNull(val);
}

/** Transform the value into the strings "Yes", "No", or null */
export function asYesNoOrNull(val: unknown) {
    const boolOrNull = asBooleanOrNull(val);

    if (boolOrNull === null) {
        return null;
    }

    return boolOrNull ? 'Yes' : 'No';
}

/** Transform the value into the strings "True", "False", or null */
export function asTrueFalseOrNull(val: unknown) {
    const boolOrNull = asBooleanOrNull(val);

    if (boolOrNull === null) {
        return null;
    }

    return boolOrNull ? 'True' : 'False';
}