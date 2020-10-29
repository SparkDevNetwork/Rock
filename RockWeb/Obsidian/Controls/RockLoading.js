Obsidian.Controls.RockLoading = {
    name: 'RockLoading',
    components: {
        RockLoadingIndicator: Obsidian.Elements.RockLoadingIndicator
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
    <RockLoadingIndicator v-else />
</div>`
};
