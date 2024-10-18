(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.firstNameTextBox = (function () {
        const exports = {
            initialize: function (options) {
                if (!options.id) {
                    throw new Error('id is required');
                }
                this.options = options;
            },
            clientValidate: function (validator, args) {
                const firstNameTextBox = document.getElementById(this.options.id);
                const notAllowedStrings = this.options.notAllowedStrings;
                let firstName = undefined;
                if (firstNameTextBox) {
                    firstName = firstNameTextBox.value.toUpperCase();
                }

                let isValid = true;
                let invalidString;

                if (firstName) {
                    for (const value of notAllowedStrings) {
                        if (firstName.includes(value.toUpperCase())) {
                            isValid = false;
                            invalidString = value;
                            break;
                        }
                    }
                }
                if (!isValid) {
                    const labelText = firstNameTextBox.dataset.itemLabel;
                    validator.errormessage = `${labelText} cannot contain: ${invalidString}`;

                    if (this.options.displayInlineValidationError) {
                        validator.innerHTML = validator.errormessage;
                    }
                }

                args.IsValid = isValid;
            }
        };

        return exports;
    }());
}(jQuery));
