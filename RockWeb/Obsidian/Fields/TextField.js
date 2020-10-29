Obsidian.Fields.TextField = {
    name: 'TextField',
    props: {
        value: {
            type: String,
            required: true
        }
    },
    computed: {
        safeValue() {
            return (this.value || '').trim();
        },
        valueIsNull() {
            return !this.safeValue
        }
    },
    template:
`<span>{{ safeValue }}</span>`
};

Obsidian.Fields['9C204CD0-1233-41C5-818A-C5DA439445AA'] = Obsidian.Fields.TextField;
