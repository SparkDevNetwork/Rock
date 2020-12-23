export default {
    name: 'SecondaryBlock',
    computed: {
        isVisible() {
            return this.$store.state.areSecondaryBlocksShown;
        }
    },
    template:
`<div class="secondary-block">
    <slot v-if="isVisible" />
</div>`
}
