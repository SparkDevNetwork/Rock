Obsidian.Controls.RockField = {
    name: 'RockField',
    props: {
        modelValue: {
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
`<component :is="fieldComponent" v-model="modelValue" />`
};
