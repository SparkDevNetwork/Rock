/**
 * Get a formatted string.
 * Ex: 10001.2 => 10,001.2
 * @param num
 */
export function asFormattedString(num: number | null, digits = 2) {
    if (num === null) {
        return '';
    }

    return num.toLocaleString(
        'en-US',
        {
            minimumFractionDigits: digits,
            maximumFractionDigits: digits
        }
    );
}

/**
 * Get a number value from a formatted string.
 * Ex: $1,000.20 => 1000.2
 * @param str
 */
export function toNumberOrNull(str: string | null) {
    if (str === null) {
        return null;
    }

    const replaced = str.replace(/[$,]/g, '');
    return Number(replaced) || 0;
}