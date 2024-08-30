// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

import { getEmojiPattern, getSpecialCharacterPattern, getSpecialFontPattern } from "@Obsidian/Core/Utilities/regexPatterns";
import { ChildRequestBag, PersonRequestBag, PropertiesOfType } from "./types.partial";
import { CommunicationPreference, CommunicationPreferenceDescription } from "@Obsidian/Enums/Blocks/Crm/FamilyPreRegistration/communicationPreference";
import { Gender } from "@Obsidian/Enums/Crm/gender";
import { asBooleanOrNull } from "@Obsidian/Utility/booleanUtils";
import { getDay, getMonth, getYear } from "@Obsidian/Utility/dateKey";
import { emptyGuid } from "@Obsidian/Utility/guid";
import { toNumberOrNull } from "@Obsidian/Utility/numberUtils";
import { ValidationResult, ValidationRuleFunction } from "@Obsidian/ValidationRules";
import { FamilyPreRegistrationPersonBag } from "@Obsidian/ViewModels/Blocks/Crm/FamilyPreRegistration/familyPreRegistrationPersonBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { Ref, WritableComputedRef, computed, reactive, toRefs } from "vue";

/**
 * Helper that converts a person bag to a person request bag that can be used to pre-register the person.
 */
export function convertPersonToPersonRequest(person: FamilyPreRegistrationPersonBag | null | undefined): PersonRequestBag {
    const defaults = createPersonRequest();

    return {
        // Copy values from person bag.
        ...person,

        // Overwrite required fields.
        guid: person?.guid || emptyGuid,
        attributeValues: person?.attributeValues || defaults.attributeValues,
        communicationPreference: person?.communicationPreference ?? defaults.communicationPreference,
        email: person?.email || defaults.email,
        firstName: person?.firstName || defaults.firstName,
        gender: person?.gender || defaults.gender,
        isFirstNameReadOnly: person?.isFirstNameReadOnly || defaults.isFirstNameReadOnly,
        isLastNameReadOnly: person?.isLastNameReadOnly || defaults.isLastNameReadOnly,
        lastName: person?.lastName || defaults.lastName,
        mobilePhone: person?.mobilePhone || defaults.mobilePhone,
        mobilePhoneCountryCode: person?.mobilePhoneCountryCode || defaults.mobilePhoneCountryCode,
        isMessagingEnabled: person?.isMessagingEnabled || defaults.isMessagingEnabled,
    };
}

/**
 * Helper that converts a person bag to a child request bag that can be used to pre-register the child.
 */
export function convertPersonToChildRequest(person: FamilyPreRegistrationPersonBag | null | undefined): ChildRequestBag {
    const request = convertPersonToPersonRequest(person);
    const defaults = createChildRequest();

    return {
        // Copy values from person bag.
        ...request,
        familyRoleGuid: request.familyRoleGuid || defaults.familyRoleGuid
    };
}

/**
 * Helper that returns a new PersonRequestBag instance that can be used to pre-register the person.
 */
export function createPersonRequest(): PersonRequestBag {
    return {
        guid: emptyGuid,
        attributeValues: {},
        communicationPreference: CommunicationPreference.None,
        email: "",
        firstName: "",
        gender: Gender.Unknown,
        isFirstNameReadOnly: false,
        isLastNameReadOnly: false,
        lastName: "",
        mobilePhone: "",
        mobilePhoneCountryCode: "",
        isMessagingEnabled: false,
    };
}

/**
 * Helper that converts a PersonRequestBag into a person view model that can be used to edit the person.
 */
export function createPersonViewModel(person: Ref<PersonRequestBag>): PersonRequestBag & {
    communicationPreferenceStringValue: string,
    ethnicityListItemBag: ListItemBag,
    genderStringValue: string,
    gradeListItemBag: ListItemBag,
    maritalStatusListItemBag: ListItemBag,
    profileImageListItemBag: ListItemBag,
    raceListItemBag: ListItemBag,
    suffixListItemBag: ListItemBag,
} {
    return reactive({
        ...toRefs(person.value),
        communicationPreferenceStringValue: computed<string>({
            get() {
                return person.value.communicationPreference?.toString();
            },
            set(newValue: string) {
                person.value.communicationPreference = CommunicationPreference[CommunicationPreferenceDescription[Number(newValue)]];
            }
        }),
        ethnicityListItemBag: createListItemBagWrapper(person, "ethnicityDefinedValueGuid"),
        genderStringValue: computed<string>({
            get() {
                return person.value.gender.toString();
            },
            set(newValue: string) {
                person.value.gender = Number(newValue) as Gender;
            }
        }),
        gradeListItemBag: createListItemBagWrapper(person, "gradeDefinedValueGuid"),
        maritalStatusListItemBag: createListItemBagWrapper(person, "maritalStatusDefinedValueGuid"),
        profileImageListItemBag: createListItemBagWrapper(person, "profilePhotoGuid"),
        raceListItemBag: createListItemBagWrapper(person, "raceDefinedValueGuid"),
        suffixListItemBag: createListItemBagWrapper(person, "suffixDefinedValueGuid"),
    });
}

/**
 * Helper that returns a new ChildRequestBag instance that can be used to pre-register the child.
 */
export function createChildRequest(): ChildRequestBag {
    return {
        guid: emptyGuid,
        attributeValues: {},
        communicationPreference: CommunicationPreference.None,
        email: "",
        familyRoleGuid: emptyGuid,
        firstName: "",
        gender: Gender.Unknown,
        isFirstNameReadOnly: false,
        isLastNameReadOnly: false,
        lastName: "",
        mobilePhone: "",
        mobilePhoneCountryCode: "",
        isMessagingEnabled: false,
    };
}

/**
 * Helper that returns a WritableComputedRef<ListItemBag> for a property of a ref.
 */
export function createListItemBagWrapper<
    TObj extends Record<string, unknown>,
    TKey extends keyof PropertiesOfType<TObj, string | undefined | null>>(r: Ref<TObj>, p: TKey): WritableComputedRef<ListItemBag> {
    return computed<ListItemBag>({
        get() {
            const value = r.value;

            return {
                value: value && value[p] && typeof value[p]["toString"] === "function" ? (value[p] as string).toString() : undefined
            };
        },
        set(newValue: ListItemBag) {
            try {
                const value = r.value;
                if (value) {
                    Object.assign(value, { [p]: newValue?.value });
                }
            }
            catch (e) {
                // The component which owns the ref was likely unmounted, so ignore the error.
            }
        }
    });
}

/**
 * Gets a function that will return a bootstrap column class, given the desired number of non-bootstrap columns in the parent container.
 *
 * @see {@link useGetBootstrapColumns} for more info.
 * @param columnsInContainer The number of columns in the container.
 * @returns The bootstrap column class.
 */
export function useGetBootstrapColumnClass(columnsInContainer: number): (columns: number) => string {
    const getColumn = useGetBootstrapColumns(columnsInContainer);

    return (columns: number): string => {
        return `col-sm-${getColumn(columns)}`;
    };
}

/**
 * Gets a function that will return the number of bootstrap columns, given the desired number of non-bootstrap of columns in the parent container.
 *
 * This logic is from the legacy Family Pre-Registration block.
 *
 * @example
 * const getBootstrapColumnsForTwoColumnForm = useGetBootstrapColumns(2);
 * getBootstrapColumnsForTwoColumnForm(1) // 1
 * getBootstrapColumnsForTwoColumnForm(2) // 2
 * getBootstrapColumnsForTwoColumnForm(3) // 6 - This is doubled
 * getBootstrapColumnsForTwoColumnForm(4) // 4
 * getBootstrapColumnsForTwoColumnForm(5) // 5
 * getBootstrapColumnsForTwoColumnForm(6) // 6
 * getBootstrapColumnsForTwoColumnForm(7) // 7
 * getBootstrapColumnsForTwoColumnForm(8) // 8
 * getBootstrapColumnsForTwoColumnForm(9) // 9
 * getBootstrapColumnsForTwoColumnForm(10) // 10
 * getBootstrapColumnsForTwoColumnForm(11) // 11
 * getBootstrapColumnsForTwoColumnForm(12) // 12
 *
 * @example
 * const getBootstrapColumnsForFourColumnForm = useGetBootstrapColumns(4);
 * getBootstrapColumnsForFourColumnForm(1) // 1
 * getBootstrapColumnsForFourColumnForm(2) // 2
 * getBootstrapColumnsForFourColumnForm(3) // 3
 * getBootstrapColumnsForFourColumnForm(4) // 4
 * getBootstrapColumnsForFourColumnForm(5) // 5
 * getBootstrapColumnsForFourColumnForm(6) // 6
 * getBootstrapColumnsForFourColumnForm(7) // 7
 * getBootstrapColumnsForFourColumnForm(8) // 8
 * getBootstrapColumnsForFourColumnForm(9) // 9
 * getBootstrapColumnsForFourColumnForm(10) // 10
 * getBootstrapColumnsForFourColumnForm(11) // 11
 * getBootstrapColumnsForFourColumnForm(12) // 12
 *
 * @param columnsInContainer The number of columns in the container.
 * @returns The bootstrap columns.
 */
function useGetBootstrapColumns(columnsInContainer: number): (columns: number) => number {
    return (columns: number): number => {
        if ((columns != 3 && columns != 6) || columnsInContainer === 4) {
            return columns;
        }

        if (columns === 6) {
            return columns;
        }

        return columns * 2;
    };
}

/**
 * Returns the ordinal string representation of an integer.
 *
 * Not reliable for numbers in the quadrillions and greater.
 *
 * @example
 * getNumberAsOrdinalString(1)      // first
 * getNumberAsOrdinalString(2)      // second
 * getNumberAsOrdinalString(123456) // one hundred twenty-three thousand four hundred fifty-sixth
 * @param numb The number for which to get the ordinal string representation.
 * @returns "first", "second", ..., "one thousandth", ..., (up to the max number allowed for JS).
 */
export function getNumberAsOrdinalString(numb: number): string {
    const ordinalStrings = {
        1: "first",
        2: "second",
        3: "third",
        4: "fourth",
        5: "fifth",
        6: "sixth",
        7: "seventh",
        8: "eighth",
        9: "ninth",
        10: "tenth",
        11: "eleventh",
        12: "twelfth",
        13: "thirteenth",
        14: "fourteenth",
        15: "fifteenth",
        16: "sixteenth",
        17: "seventeenth",
        18: "eighteenth",
        19: "nineteenth",
        20: "twentieth",
        30: "thirtieth",
        40: "fortieth",
        50: "fiftieth",
        60: "sixtieth",
        70: "seventieth",
        80: "eightieth",
        90: "ninetieth",
        100: "one hundredth",
        1000: "one thousandth",
        1000000: "one millionth",
        1000000000: "one trillionth",
        1000000000000: "one quadrillionth"
    };
    const numberStrings = {
        1: "one",
        2: "two",
        3: "three",
        4: "four",
        5: "five",
        6: "six",
        7: "seven",
        8: "eight",
        9: "nine",
        10: "ten",
        11: "eleven",
        12: "twelve",
        13: "thirteen",
        14: "fourteen",
        15: "fifteen",
        16: "sixteen",
        17: "seventeen",
        18: "eighteen",
        19: "nineteen",
        20: "twenty",
        30: "thirty",
        40: "forty",
        50: "fifty",
        60: "sixty",
        70: "seventy",
        80: "eighty",
        90: "ninety",
        100: "one hundred",
        1000: "one thousand",
        1000000: "one million",
        1000000000: "one billion",
        1000000000000: "one trillion",
        1000000000000000: "one quadrillion"
    };

    // Store constants for these since it is hard to distinguish between them at larger numbers.
    const oneHundred = 100;
    const oneThousand = 1000;
    const oneMillion = 1000000;
    const oneBillion = 1000000000;
    const oneTrillion = 1000000000000;
    const oneQuadrillion = 1000000000000000;

    if (ordinalStrings[numb]) {
        return ordinalStrings[numb];
    }

    function getQuadrillionth(numb: number): string {
        const trillionth = getTrillionth(numb);
        if (numb >= oneQuadrillion) {
            const quadrillions = getHundredsString(Number(numb.toString().slice(-18, -15)));
            if (trillionth) {
                return `${quadrillions} quadrillion ${trillionth}`;
            }
            else {
                return `${quadrillions} quadrillionth`;
            }
        }
        return trillionth;
    }

    function getTrillionth(numb: number): string {
        numb = Number(numb.toString().slice(-15));
        const billionth = getBillionth(numb);
        if (numb >= oneTrillion) {
            const trillions = getHundredsString(Number(numb.toString().slice(-15, -12)));
            if (billionth) {
                return `${trillions} trillion ${billionth}`;
            }
            else {
                return `${trillions} trillionth`;
            }
        }
        return billionth;
    }

    function getBillionth(numb: number): string {
        numb = Number(numb.toString().slice(-12));
        const millionth = getMillionth(numb);
        if (numb >= oneBillion) {
            const billions = getHundredsString(Number(numb.toString().slice(-12, -9)));
            if (millionth) {
                return `${billions} billion ${millionth}`;
            }
            else {
                return `${billions} billionth`;
            }
        }
        return millionth;
    }

    function getMillionth(numb: number): string {
        numb = Number(numb.toString().slice(-9));
        const thousandths = getThousandths(numb);
        if (numb >= oneMillion) {
            const millions = getHundredsString(Number(numb.toString().slice(-9, -6)));
            if (thousandths) {
                return `${millions} million ${thousandths}`;
            }
            else {
                return `${millions} millionth`;
            }
        }
        return thousandths;
    }

    function getThousandths(numb: number): string {
        numb = Number(numb.toString().slice(-6));
        const hundredths = getHundredths(numb);
        if (numb >= oneThousand) {
            const thousands = getHundredsString(Number(numb.toString().slice(-6, -3)));
            if (hundredths) {
                return `${thousands} thousand ${hundredths}`;
            }
            else {
                return `${thousands} thousandths`;
            }
        }
        return hundredths;
    }

    function getHundredths(numb: number): string {
        numb = Number(numb.toString().slice(-3));

        if (ordinalStrings[numb]) {
            return ordinalStrings[numb];
        }

        const tenths = getTenths(numb);
        if (numb >= oneHundred) {
            const hundreds = Number(numb.toString().slice(-3, -2));
            if (tenths) {
                return `${numberStrings[hundreds]} hundred ${tenths}`;
            }
            else {
                return `${numberStrings[hundreds]} hundredth`;
            }
        }
        return tenths;
    }

    function getHundredsString(numb: number): string {
        numb = Number(numb.toString().slice(-3));

        if (numberStrings[numb]) {
            return numberStrings[numb];
        }

        const tens = getTensString(numb);

        if (numb >= oneHundred) {
            const hundreds = Number(numb.toString().slice(-3, -2));
            if (tens) {
                return `${numberStrings[hundreds]} hundred ${tens}`;
            }
            else {
                return `${numberStrings[hundreds]} hundred`;
            }
        }
        return tens;
    }

    function getTensString(numb: number): string {
        numb = Number(numb.toString().slice(-2));

        if (numberStrings[numb]) {
            return numberStrings[numb];
        }

        const ones = getOnesString(numb);

        if (numb >= 20) {
            const tens = Number(numb.toString().slice(-2, -1));

            if (ones) {
                return `${numberStrings[tens * 10]}-${ones}`;
            }
            else {
                return numberStrings[tens * 10];
            }
        }
        return ones;
    }

    function getTenths(numb: number): string {
        numb = Number(numb.toString().slice(-2));

        if (ordinalStrings[numb]) {
            return ordinalStrings[numb];
        }

        const oneths = getOneths(numb);

        if (numb >= 20) {
            const tens = Number(numb.toString().slice(-2, -1));

            if (oneths) {
                return `${numberStrings[tens * 10]}-${oneths}`;
            }
            else {
                return ordinalStrings[tens * 10];
            }
        }
        return oneths;
    }

    function getOneths(numb: number): string {
        numb = Number(numb.toString().slice(-1));

        return ordinalStrings[numb];
    }

    function getOnesString(numb: number): string {
        numb = Number(numb.toString().slice(-1));

        return numberStrings[numb];
    }

    return getQuadrillionth(numb);
}

/**
 * Validates that a value is entered.
 *
 * This was copied from the "required" validation rule.
 */
export function required(value: unknown, params?: unknown[]): ValidationResult {
    // This needs to be changed. JSON is not safe in rules because of the
    // comma and pipe characters.
    const options = params && params.length >= 1 && typeof params[0] === "string" ? JSON.parse(params[0]) : {};

    if (typeof value === "string") {
        const allowEmptyString = !!(options.allowEmptyString);

        if (!allowEmptyString && !(value?.trim())) {
            return "is required";
        }

        return true;
    }

    if (typeof value === "number" && value === 0) {
        return "is required";
    }

    if (Array.isArray(value) && value.length === 0) {
        return "is required";
    }

    // Special case for booleans, required rule is ignored. Otherwise things
    // like checkbox and toggle would always require a True value.
    if (typeof value === "boolean") {
        return true;
    }

    if (!value) {
        return "is required";
    }

    return true;
}

/**
 * Validates whether a name can include special characters.
 */
export function noSpecialCharacters (value: unknown): ValidationResult {
    if (typeof value === "string") {
        // Checks if a string contains special characters
        if (getSpecialCharacterPattern().test(value)) {
            return "cannot contain special characters such as quotes, parentheses, etc.";
        }
    }

    return true;
}

/**
 * Validates whether a name can emojis or special fonts.
 */
export function noEmojisOrSpecialFonts (value: unknown): ValidationResult {
    if (typeof value === "string") {
        // Checks if a string contains emojis or special fonts.
        if (getEmojiPattern().test(value) || getSpecialFontPattern().test(value)) {
            return "cannot contain emojis or special fonts.";
        }
    }

    return true;
}

/**
 * Validates whether a birthday with an optional year is valid.
 */
export function monthAndDayRequiredRule(value: unknown, _params?: unknown): ValidationResult {
    if (typeof value !== "string") {
        return true;
    }

    if (!getMonth(value)) {
        return "must have a month";
    }

    if (!getDay(value)) {
        return "must have a day";
    }

    return true;
}

/**
 * Validates whether a birthday with an optional year is valid.
 *
 * This was copied from the "datekey" validation rule.
 */
export function monthAndDayAndYearRequiredRule(value: unknown, _params?: unknown): ValidationResult {
    if (typeof value !== "string") {
        return true;
    }

    if (!getYear(value)) {
        return "must have a year";
    }

    if (!getMonth(value)) {
        return "must have a month";
    }

    if (!getDay(value)) {
        return "must have a day";
    }

    return true;
}

/**
 * Convert the string to a number
 * @param val
 */
function convertToNumber(value: unknown): number {
    if (typeof value === "number") {
        return value;
    }

    if (typeof value === "string") {
        return toNumberOrNull(value) || 0;
    }

    return 0;
}

/**
 * Is the value numeric?
 * '0.9' => true
 * 0.9 => true
 * '9a' => false
 * @param value
 */
function isNumeric(value: unknown): boolean {
    if (typeof value === "number") {
        return true;
    }

    if (typeof value === "string") {
        return toNumberOrNull(value) !== null;
    }

    return false;
}

/**
 * Validates that a value does not equal another value.
 */
export function createNotEqualRule(compare: unknown): ValidationRuleFunction {
    return (value: unknown, params?: unknown[]): ValidationResult => {
        if (isNumeric(value) && isNumeric(compare)) {
            if (convertToNumber(value) !== convertToNumber(compare)) {
                return true;
            }
        }
        else if (typeof value === "boolean") {
            if (value !== asBooleanOrNull(compare)) {
                return true;
            }
        }
        else if (value !== compare) {
            return true;
        }

        return `must not equal ${compare}`;
    };
}

/**
 * Returns a wrapper around a validation function which appends a suffix to validation error messages.
 */
export function createRuleWithSuffix(rule: ValidationRuleFunction, suffix: string): ValidationRuleFunction {
    return (value: unknown, params?: unknown[]): ValidationResult => {
        const result = rule(value, params);

        if (typeof result === "string" && result && suffix) {
            return `${result} ${suffix}`;
        }

        return result;
    };
}

/**
 * Returns a wrapper around a validation function which replaces a validation error message.
 */
export function createRuleWithReplacement(rule: ValidationRuleFunction, replacement: string): ValidationRuleFunction {
    return (value: unknown, params?: unknown[]): ValidationResult => {
        const result = rule(value, params);

        if (typeof result === "string" && result && replacement) {
            return replacement;
        }

        return result;
    };
}