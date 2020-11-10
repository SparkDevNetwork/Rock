Obsidian.Elements.registerElement({
    name: 'RockButton',
    props: {
        isLoading: {
            type: Boolean,
            default: false
        },
        loadingText: {
            type: String,
            default: 'Loading...'
        }
    },
    emits: [
        'click'
    ],
    methods: {
        handleClick: function () {
            this.$emit('click');
        }
    },
    template:
`<button class="btn" :disabled="isLoading" @click.prevent="handleClick">
    <template v-if="isLoading">
        {{loadingText}}
    </template>
    <slot v-else />
</button>`
});
