Obsidian.Elements.RockButton = {
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
    methods: {
        handleClick: function() {
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
};
