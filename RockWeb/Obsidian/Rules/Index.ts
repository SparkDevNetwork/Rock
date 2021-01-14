import { isEmail } from '../Filters/Email.js';
import { isNullOrWhitespace } from '../Filters/String.js';
import { defineRule } from '../Vendor/VeeValidate/vee-validate.js';

export type ValidationRuleFunction = (value: unknown) => boolean | string | Promise<boolean | string>;

export function ruleStringToArray(rulesString: string) {
    return rulesString.split('|');
}

export function ruleArrayToString(rulesArray: string[]) {
    return rulesArray.join('|');
}

defineRule('required', (value => {
    if (isNullOrWhitespace(value)) {
        return 'is required';
    }

    return true;
}) as ValidationRuleFunction);

defineRule('email', (value => {
    // Field is empty, should pass
    if (isNullOrWhitespace(value)) {
        return true;
    }

    // Check if email
    if (!isEmail(value)) {
        return 'must be a valid email';
    }

    return true;
}) as ValidationRuleFunction);