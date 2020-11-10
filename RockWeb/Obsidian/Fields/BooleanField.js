Obsidian.Fields.registerField('1EDAFDED-DFE6-4334-B019-6EECBA89E05A', {
    name: 'BooleanField',
    props: {
        modelValue: {
            type: String,
            required: true
        }
    },
    computed: {
        safeValue() {
            return (this.modelValue || '').trim().toLowerCase();
        },
        valueIsNull() {
            return !this.safeValue
        },
        valueIsTrue() {
            return ['true', 'yes', 't', 'y', '1'].indexOf(this.safeValue) !== -1;
        },
        valueIsFalse() {
            return !this.valueIsTrue && !this.valueIsNull;
        },
        valueAsBooleanOrNull() {
            if (this.valueIsNull) {
                return null;
            }

            return this.valueIsTrue;
        },
        valueAsYesNoOrNull() {
            if (this.valueIsNull) {
                return null;
            }

            return this.valueIsTrue ? 'Yes' : 'No';
        }
    },
    template:
`<span>{{ valueAsYesNoOrNull }}</span>`
});
