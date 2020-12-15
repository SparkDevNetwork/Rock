Obsidian.Controls.registerControl({
    name: 'GridRow',
    props: {
        rowContext: {
            type: Object,
            required: true
        }
    },
    provide() {
        return {
            rowContext: this.rowContext
        };
    },
    methods: {
        onRowClick() {
            if (!this.rowContext.isHeader) {
                this.$emit('click:body', this.rowContext);
            }
            else {
                this.$emit('click:header', this.rowContext);
            }
        }
    },
    template:
`<tr @click="onRowClick">
    <slot />
</tr>`
});
