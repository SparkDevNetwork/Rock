Obsidian.Controls.RockField = {
    name: 'RockField',
    props: {
        value: {
            type: String,
            required: true
        },
        fieldTypeGuid: {
            type: String,
            required: true
        }
    },
    computed: {
        fieldComponent() {
            return Obsidian.Fields[this.fieldTypeGuid.toUpperCase()] || Obsidian.Fields.TextField;
        }
    },
    template:
`<component :is="fieldComponent" :value="value" />`
};
