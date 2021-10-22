/* This file fixes some issues caused by the Microsoft Ajax library which we
 * don't have access to modify. It should be included early in the page. */


/* Fix the String.prototype.startsWith function. Microsoft Ajax library
 * loads in it's own version from an era before it was standard in browsers.
 * But it's a broken version. */
if (String.prototype.startsWith.toString().indexOf("function String$startsWith(") === 0) {
    String.prototype.startsWith = function (searchString) {
        var pos = parseInt(arguments.length > 1 ? arguments[1] : 0);

        return this.indexOf(searchString, pos) === 0;
    };
}
