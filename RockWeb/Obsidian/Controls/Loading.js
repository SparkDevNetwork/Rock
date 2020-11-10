Obsidian.Controls.registerControl({
    name: 'Loading',
    components: {
        LoadingIndicator: Obsidian.Elements.LoadingIndicator
    },
    props: {
        isLoading: {
            type: Boolean,
            required: true
        }
    },
    template:
`<div>
    <slot v-if="!isLoading" />
    <LoadingIndicator v-else />
</div>`
});
