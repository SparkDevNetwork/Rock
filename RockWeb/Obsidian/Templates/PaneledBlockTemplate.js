// Provides a generic Rock Block structure
Obsidian.Templates.registerTemplate({
    name: 'PaneledBlockTemplate',
    template:
`<div class="panel panel-block">
    <div class="panel-heading">
        <h1 class="panel-title">
            <slot name="title" />
        </h1>
    </div>
    <div class="panel-body">
        <div class="block-content">
            <slot />
        </div>
    </div>
</div>`
});
