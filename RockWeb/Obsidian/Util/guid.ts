export type Guid = string;

/**
* Generates a new Guid
*/
export function newGuid(): Guid {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
        const r = Math.random() * 16 | 0;
        const v = c === 'x' ? r : r & 0x3 | 0x8;
        return v.toString(16);
    });
}

/**
 * Returns a normalized Guid that can be compared with string equality (===)
 * @param a
 */
export function normalize(a: Guid) {
    return a.toLowerCase();
}

/**
 * Are the guids equal?
 * @param a
 * @param b
 */
export function areEqual(a: Guid, b: Guid) {
    return normalize(a) === normalize(b);
}

