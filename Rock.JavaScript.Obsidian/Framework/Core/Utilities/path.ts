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

/*
 * A bunch of utility functions for parsing out pieces of paths or joining them together. Practically
 * all of this functionality was adapted from Node.js's path module:
 * https://github.com/nodejs/node/blob/main/lib/path.js
 *
 * Some functions have been renamed to match C# or changed to work primarily/only with forward
 * slashes, etc.
 */

/* eslint-disable @typescript-eslint/naming-convention */
const CHAR_DOT = 46;
const CHAR_FORWARD_SLASH = 47;
const CHAR_BACKWARD_SLASH = 92;
const CHAR_COLON = 58;
const CHAR_UPPERCASE_A = 65;
const CHAR_LOWERCASE_A = 97;
const CHAR_UPPERCASE_Z = 90;
const CHAR_LOWERCASE_Z = 122;
/* eslint-enable @typescript-eslint/naming-convention */

/**
 * Pull the directory portion out of a path string. Adapted from Node.js's path.dirname()
 */
export function getDirectoryName(path: string): string {
    const len = path.length;
    if (len === 0)
        return "";
    let rootEnd = -1;
    let offset = 0;
    const code = path.charCodeAt(0);

    if (len === 1) {
        // `path` contains just a path separator, exit early to avoid unnecessary work or a slash.
        // NOTE: the original from Node.js gives a dot instead of a slash as the fallback, but a
        // slash is more in line with the C# version of this function
        return isPathSeparator(code) ? path : "/";
    }

    // Try to match a root
    if (isPathSeparator(code)) {
        // Possible UNC root
        rootEnd = offset = 1;

        if (isPathSeparator(path.charCodeAt(1))) {
            // Matched double path separator at beginning
            let j = 2;
            let last = j;
            // Match 1 or more non-path separators
            while (j < len &&
                !isPathSeparator(path.charCodeAt(j))) {
                j++;
            }
            if (j < len && j !== last) {
                // Matched!
                last = j;
                // Match 1 or more path separators
                while (j < len &&
                    isPathSeparator(path.charCodeAt(j))) {
                    j++;
                }
                if (j < len && j !== last) {
                    // Matched!
                    last = j;
                    // Match 1 or more non-path separators
                    while (j < len &&
                        !isPathSeparator(path.charCodeAt(j))) {
                        j++;
                    }
                    if (j === len) {
                        // We matched a UNC root only
                        return path;
                    }
                    if (j !== last) {
                        // We matched a UNC root with leftovers

                        // Offset by 1 to include the separator after the UNC root to
                        // treat it as a "normal root" on top of a (UNC) root
                        rootEnd = offset = j + 1;
                    }
                }
            }
        }
        // Possible device root
    }
    else if (isWindowsDeviceRoot(code) && path.charCodeAt(1) === CHAR_COLON) {
        rootEnd = len > 2 && isPathSeparator(path.charCodeAt(2)) ? 3 : 2;
        offset = rootEnd;
    }

    let end = -1;
    let matchedSlash = true;
    for (let i = len - 1; i >= offset; --i) {
        if (isPathSeparator(path.charCodeAt(i))) {
            if (!matchedSlash) {
                end = i;
                break;
            }
        }
        else {
            // We saw the first non-path separator
            matchedSlash = false;
        }
    }

    if (end === -1) {
        if (rootEnd === -1)
            return "";

        end = rootEnd;
    }
    return path.slice(0, end);
}

/**
 * Joins all given path segments together using the "/" as a delimiter, then normalizes the resulting path.
 * Adapted from Node.js's path.join()
 */
export function combine(...args: string[]): string {
    if (args.length === 0) {
        // NOTE: Node.js's implementation returns '.' instead of "/" here
        return "/";
    }

    let joined;
    let firstPart;
    for (let i = 0; i < args.length; ++i) {
        const arg = args[i];
        if (arg.length > 0) {
            if (joined === undefined) {
                joined = firstPart = arg;
            }
            else {
                joined += `\\${arg}`;
            }
        }
    }

    if (joined === undefined) {
        // NOTE: Node.js's implementation returns '.' instead of "/" here
        return "/";
    }

    // Make sure that the joined path doesn't start with two slashes, because
    // normalize() will mistake it for a UNC path then.
    //
    // This step is skipped when it is very clear that the user actually
    // intended to point at a UNC path. This is assumed when the first
    // non-empty string arguments starts with exactly two slashes followed by
    // at least one more non-slash character.
    //
    // Note that for normalize() to treat a path as a UNC path it needs to
    // have at least 2 components, so we don't filter for that here.
    // This means that the user can use join to construct UNC paths from
    // a server name and a share name; for example:
    //   path.join('//server', 'share') -> '\\\\server\\share\\')
    let needsReplace = true;
    let slashCount = 0;
    if (isPathSeparator(firstPart.charCodeAt(0))) {
        ++slashCount;
        const firstLen = firstPart.length;
        if (firstLen > 1 &&
            isPathSeparator(firstPart.charCodeAt(1))) {
            ++slashCount;
            if (firstLen > 2) {
                if (isPathSeparator(firstPart.charCodeAt(2)))
                    ++slashCount;
                else {
                    // We matched a UNC path in the first part
                    needsReplace = false;
                }
            }
        }
    }
    if (needsReplace) {
        // Find any more consecutive slashes we need to replace
        while (slashCount < joined.length && isPathSeparator(joined.charCodeAt(slashCount))) {
            slashCount++;
        }

        // Replace the slashes if needed
        if (slashCount >= 2) {
            joined = `\\${joined.slice(slashCount)}`;
        }
    }

    return normalize(joined);
}

/**
 * Normalizes the given path, resolving '..' and '.' segments. When multiple, sequential "/"
 * characters are found, they are replaced by a single instance of "/";
 */
export function normalize(path: string): string {
    const len = path.length;
    if (len === 0)
        return "";
    let rootEnd = 0;
    let device;
    let isAbsolute = false;
    const code = path.charCodeAt(0);

    // Try to match a root
    if (len === 1) {
        // `path` contains just a single char, exit early to avoid unnecessary work
        return path;
    }
    if (isPathSeparator(code)) {
        // If we started with a separator, we know we at least have an absolute path of some kind (UNC or otherwise)
        isAbsolute = true;

        if (isPathSeparator(path.charCodeAt(1))) {
            // Matched double path separator at beginning
            let j = 2;
            let last = j;
            // Match 1 or more non-path separators
            while (j < len &&
                !isPathSeparator(path.charCodeAt(j))) {
                j++;
            }
            if (j < len && j !== last) {
                const firstPart = path.slice(last, j);
                // Matched!
                last = j;
                // Match 1 or more path separators
                while (j < len &&
                    isPathSeparator(path.charCodeAt(j))) {
                    j++;
                }
                if (j < len && j !== last) {
                    // Matched!
                    last = j;
                    // Match 1 or more non-path separators
                    while (j < len &&
                        !isPathSeparator(path.charCodeAt(j))) {
                        j++;
                    }
                    if (j === len) {
                        // We matched a UNC root only
                        // Return the normalized version of the UNC root since there
                        // is nothing left to process
                        return `//${firstPart}/${path.slice(last)}/`;
                    }
                    if (j !== last) {
                        // We matched a UNC root with leftovers
                        device =
                            `//${firstPart}/${path.slice(last, j)}`;
                        rootEnd = j;
                    }
                }
            }
        }
        else {
            rootEnd = 1;
        }
    }
    else if (isWindowsDeviceRoot(code) &&
        path.charCodeAt(1) === CHAR_COLON) {
        // Possible device root
        device = path.slice(0, 2);
        rootEnd = 2;
        if (len > 2 && isPathSeparator(path.charCodeAt(2))) {
            // Treat separator following drive name as an absolute path
            // indicator
            isAbsolute = true;
            rootEnd = 3;
        }
    }

    let tail = rootEnd < len ? normalizeString(path.slice(rootEnd), !isAbsolute, "/") : "";
    if (tail.length === 0 && !isAbsolute) {
        tail = "";
    }
    if (tail.length > 0 && isPathSeparator(path.charCodeAt(len - 1))) {
        tail += "/";
    }
    if (device === undefined) {
        return isAbsolute ? `/${tail}` : tail;
    }
    return isAbsolute ? `${device}/${tail}` : `${device}${tail}`;
}

/**
 * Returns the extension of the path, from the last occurrence of the . character to end of string in the last portion
 * of the path. If there is no . in the last portion of the path, or if there are no . characters other than the first
 * character of the basename of path, an empty string is returned.
 *
 * Adapted from Node.js's path.extname
 */
export function getExtension(path: string): string {
    let start = 0;
    let startDot = -1;
    let startPart = 0;
    let end = -1;
    let matchedSlash = true;
    // Track the state of characters (if any) we see before our first dot and
    // after any path separator we find
    let preDotState = 0;

    // Check for a drive letter prefix so as not to mistake the following
    // path separator as an extra separator at the end of the path that can be
    // disregarded

    if (path.length >= 2 && path.charCodeAt(1) === CHAR_COLON && isWindowsDeviceRoot(path.charCodeAt(0))) {
        start = startPart = 2;
    }

    for (let i = path.length - 1; i >= start; --i) {
        const code = path.charCodeAt(i);
        if (isPathSeparator(code)) {
            // If we reached a path separator that was not part of a set of path
            // separators at the end of the string, stop now
            if (!matchedSlash) {
                startPart = i + 1;
                break;
            }
            continue;
        }
        if (end === -1) {
            // We saw the first non-path separator, mark this as the end of our
            // extension
            matchedSlash = false;
            end = i + 1;
        }
        if (code === CHAR_DOT) {
            // If this is our first dot, mark it as the start of our extension
            if (startDot === -1) {
                startDot = i;
            }
            else if (preDotState !== 1) {
                preDotState = 1;
            }
        }
        else if (startDot !== -1) {
            // We saw a non-dot and non-path separator before our dot, so we should
            // have a good chance at having a non-empty extension
            preDotState = -1;
        }
    }

    if (startDot === -1 ||
        end === -1 ||
        // We saw a non-dot character immediately before the dot
        preDotState === 0 ||
        // The (right-most) trimmed path component is exactly '..'
        (preDotState === 1 &&
            startDot === end - 1 &&
            startDot === startPart + 1)) {
        return "";
    }
    return path.slice(startDot, end);
}

/**
 * Returns the last portion of a path, similar to the Unix basename command. Trailing directory separators are ignored.
 * Unlike the C# equivalent, if there is a trailing slash, this will still return the last part that comes before the trailing
 * slash, whereas C# will treat it as a folder and will return an empty string. Also, unlike the C# version, this function
 * allows you to specify an suffix to strip off the end of the result.
 *
 * Adapted from Node.js's path.basename()
 */
export function getFileName(path: string, suffix?: string): string {
    let start = 0;
    let end = -1;
    let matchedSlash = true;

    // Check for a drive letter prefix so as not to mistake the following
    // path separator as an extra separator at the end of the path that can be
    // disregarded
    if (path.length >= 2 && isWindowsDeviceRoot(path.charCodeAt(0)) && path.charCodeAt(1) === CHAR_COLON) {
        start = 2;
    }

    if (suffix !== undefined && suffix.length > 0 && suffix.length <= path.length) {
        if (suffix === path) {
            return "";
        }
        let extIdx = suffix.length - 1;
        let firstNonSlashEnd = -1;
        for (let i = path.length - 1; i >= start; --i) {
            const code = path.charCodeAt(i);
            if (isPathSeparator(code)) {
                // If we reached a path separator that was not part of a set of path
                // separators at the end of the string, stop now
                if (!matchedSlash) {
                    start = i + 1;
                    break;
                }
            }
            else {
                if (firstNonSlashEnd === -1) {
                    // We saw the first non-path separator, remember this index in case
                    // we need it if the extension ends up not matching
                    matchedSlash = false;
                    firstNonSlashEnd = i + 1;
                }
                if (extIdx >= 0) {
                    // Try to match the explicit extension
                    if (code === suffix.charCodeAt(extIdx)) {
                        if (--extIdx === -1) {
                            // We matched the extension, so mark this as the end of our path
                            // component
                            end = i;
                        }
                    }
                    else {
                        // Extension does not match, so our result is the entire path
                        // component
                        extIdx = -1;
                        end = firstNonSlashEnd;
                    }
                }
            }
        }

        if (start === end) {
            end = firstNonSlashEnd;
        }
        else if (end === -1) {
            end = path.length;
        }
        return path.slice(start, end);
    }
    for (let i = path.length - 1; i >= start; --i) {
        if (isPathSeparator(path.charCodeAt(i))) {
            // If we reached a path separator that was not part of a set of path
            // separators at the end of the string, stop now
            if (!matchedSlash) {
                start = i + 1;
                break;
            }
        }
        else if (end === -1) {
            // We saw the first non-path separator, mark this as the end of our
            // path component
            matchedSlash = false;
            end = i + 1;
        }
    }

    if (end === -1) {
        return "";
    }
    return path.slice(start, end);
}

/**
 * Returns the same thing as `getFileName`, but doesn't require you to know the extension and pass it in in order to
 * exclude the extension.
 */
export function getFileNameWithoutExtension(path: string): string {
    const ext = getExtension(path);
    return getFileName(path, ext);
}

/**
 * Determine if given character code is a path separator.
 */
function isPathSeparator(code: number): boolean {
    return code === CHAR_FORWARD_SLASH || code === CHAR_BACKWARD_SLASH;
}

/**
 * Determine if the given character code could be a Windows device root. Adapted from Node.js's path module:
 * https://github.com/nodejs/node/blob/main/lib/path.js#L60
 */
function isWindowsDeviceRoot(code: number): boolean {
    return (code >= CHAR_UPPERCASE_A && code <= CHAR_UPPERCASE_Z) ||
        (code >= CHAR_LOWERCASE_A && code <= CHAR_LOWERCASE_Z);
}

/**
 * Resolves . and .. elements in a path with directory names
 */
function normalizeString(path: string, allowAboveRoot: boolean, separator: string): string {
    let res = "";
    let lastSegmentLength = 0;
    let lastSlash = -1;
    let dots = 0;
    let code = 0;
    for (let i = 0; i <= path.length; ++i) {
        if (i < path.length) {
            code = path.charCodeAt(i);
        }
        else if (isPathSeparator(code)) {
            break;
        }
        else {
            code = CHAR_FORWARD_SLASH;
        }

        if (isPathSeparator(code)) {
            if (lastSlash === i - 1 || dots === 1) {
                // NOOP
            }
            else if (dots === 2) {
                if (res.length < 2 || lastSegmentLength !== 2 ||
                    res.charCodeAt(res.length - 1) !== CHAR_DOT ||
                    res.charCodeAt(res.length - 2) !== CHAR_DOT) {
                    if (res.length > 2) {
                        const lastSlashIndex = res.lastIndexOf(separator);
                        if (lastSlashIndex === -1) {
                            res = "";
                            lastSegmentLength = 0;
                        }
                        else {
                            res = res.slice(0, lastSlashIndex);
                            lastSegmentLength =
                                res.length - 1 - res.lastIndexOf(separator);
                        }
                        lastSlash = i;
                        dots = 0;
                        continue;
                    }
                    else if (res.length !== 0) {
                        res = "";
                        lastSegmentLength = 0;
                        lastSlash = i;
                        dots = 0;
                        continue;
                    }
                }
                if (allowAboveRoot) {
                    res += res.length > 0 ? `${separator}..` : "..";
                    lastSegmentLength = 2;
                }
            }
            else {
                if (res.length > 0) {
                    res += `${separator}${path.slice(lastSlash + 1, i)}`;
                }
                else {
                    res = path.slice(lastSlash + 1, i);
                }
                lastSegmentLength = i - lastSlash - 1;
            }
            lastSlash = i;
            dots = 0;
        }
        else if (code === CHAR_DOT && dots !== -1) {
            ++dots;
        }
        else {
            dots = -1;
        }
    }
    return res;
}