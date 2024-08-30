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

/**
 * Regex patterns used for validation purposes
 */
export const regexPatterns = {
    // This regular expression pattern matches parentheses, curly braces, square brackets, and double quotes.
    specialCharacterPattern: /[({[\]})"]/,

    // Regular expression to match emojis and special Unicode characters.
    emojiPattern: /[\u{1F000}-\u{1F6FF}\u{1F900}-\u{1F9FF}\u{2600}-\u{26FF}\u{2700}-\u{27BF}\u{1F300}-\u{1F5FF}\u{1F680}-\u{1F6FF}\u{1F1E0}-\u{1F1FF}]/u,

    // Regular expression to match special font characters.
    specialFontPattern: /[\u{1D400}-\u{1D7FF}\u{1F100}-\u{1F1FF}]/u
};

/**
 * Returns a regular expression pattern for matching special characters.
 * This pattern can be used to identify or validate strings containing parentheses, curly braces, square brackets, and double quotes.
 * @returns {RegExp} A regular expression object for matching special characters.
 */
export const getSpecialCharacterPattern = (): RegExp => regexPatterns.specialCharacterPattern;

/**
 * Returns a regular expression pattern for matching emoji characters.
 * This pattern can be used to identify or validate strings containing emojis.
 * @returns {RegExp} A regular expression object for matching emoji characters.
 */
export const getEmojiPattern = (): RegExp => regexPatterns.emojiPattern;

/**
 * Returns a regular expression pattern for matching special font characters.
 * This pattern can be used to identify or validate strings containing characters from special fonts,
 * such as mathematical alphanumeric symbols or enclosed alphanumerics.
 * @returns {RegExp} A regular expression object for matching special font characters.
 */
export const getSpecialFontPattern = (): RegExp => regexPatterns.specialFontPattern;