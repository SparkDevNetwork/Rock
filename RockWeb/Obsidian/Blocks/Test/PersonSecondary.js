Obsidian.Blocks['Test.PersonSecondary'] = {
    name: 'Test.PersonSecondary',
    components: {
        PaneledBlockTemplate: Obsidian.Templates.PaneledBlockTemplate,
        SecondaryBlock: Obsidian.Controls.SecondaryBlock,
        RockTextBox: Obsidian.Elements.RockTextBox,
        RockButton: Obsidian.Elements.RockButton
    },
    data() {
        return {
            messageToPublish: '',
            receivedMessage: ''
        };
    },
    methods: {
        receiveMessage(message) {
            this.receivedMessage = message;
        },
        doPublish() {
            Obsidian.Bus.publish('PersonSecondary:Message', this.messageToPublish);
            this.messageToPublish = '';
        }
    },
    created() {
        Obsidian.Bus.subscribe('PersonDetail:Message', this.receiveMessage);
    },
    template:
`<SecondaryBlock>
    <PaneledBlockTemplate>
        <template slot="title">
            <i class="fa fa-flask"></i>
            Secondary Block
        </template>
        <template>
            <div class="row">
                <div class="col-sm-6">
                    <p>This is a secondary block. It respects the store's value indicating if secondary blocks are visible.</p>
                </div>
                <div class="col-sm-6">
                    <div class="well">
                        <RockTextBox label="Message" v-model="messageToPublish" />
                        <RockButton class="btn-primary btn-sm" @click="doPublish">Publish</RockButton>
                    </div>
                    <p>
                        <strong>Detail block says:</strong>
                        {{receivedMessage}}
                    </p>
                </div>
            </div>
        </template>
    </PaneledBlockTemplate>
</SecondaryBlock>`
};
