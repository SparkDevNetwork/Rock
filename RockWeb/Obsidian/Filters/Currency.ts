const formatter = new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD'
});

/**
 * Get a formatted currency string.
 * Ex: 1.2 => $1.20
 * @param num
 */
export function asFormattedString(num: number | null) {
    if (num === null) {
        return '';
    }

    return formatter.format(num);
}

/**
 * Get a number value from a formatted string.
 * Ex: $1.20 => 1.2
 * @param str
 */
export function toNumberOrNull(str: string | null) {
    if (str === null) {
        return null;
    }

    const replaced = str.replace(/[$,]/g, '');
    return Number(replaced) || 0;
}