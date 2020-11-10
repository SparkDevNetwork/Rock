Obsidian.Fields.registerField('9C204CD0-1233-41C5-818A-C5DA439445AA', {
    name: 'TextField',
    props: {
        modelValue: {
            type: String,
            required: true
        }
    },
    computed: {
        safeValue() {
            return (this.modelValue || '').trim();
        },
        valueIsNull() {
            return !this.safeValue
        }
    },
    template:
`<span>{{ safeValue }}</span>`
});
