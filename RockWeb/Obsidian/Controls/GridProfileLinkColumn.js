Obsidian.Controls.registerControl({
    name: 'GridProfileLinkColumn',
    components: {
        GridColumn: Obsidian.Controls.GridColumn
    },
    inject: [
        'rowContext'
    ],
    props: {
        property: {
            type: String,
            default: 'PersonId'
        },
        urlTemplate: {
            type: String,
            default: '/person/{id}'
        }
    },
    computed: {
        personId() {
            return this.rowContext.rowData[this.property];
        },
        url() {
            return this.urlTemplate.replace('{id}', this.personId);
        }
    },
    template:
`<GridColumn :rowContext="rowContext" class="grid-columncommand" align="center">
    <a @click.stop class="btn btn-default btn-sm" :href="url">
        <i class="fa fa-user"></i>
    </a>
</GridColumn>`
});
