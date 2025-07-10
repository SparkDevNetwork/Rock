// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    /// <summary>
    /// Migration steps required for the new Payment Entry workflow action.
    /// We convert the existing Mobile Workflow Entry block to be a dual-purpose block
    /// used by Mobile and Obsidian. Then update the default structured content
    /// tool set to include the new alert tool.
    /// </summary>
    public partial class AddPaymentEntryWorkflowAction : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.RenameEntityType( "02d2dba8-5300-4367-b15b-e37dfb3f7d1e",
                "Rock.Blocks.Workflow.WorkflowEntry",
                "Workflow Entry",
                "Rock.Blocks.Workflow.WorkflowEntry, Rock.Blocks, Version=17.1.1.0, Culture=neutral, PublicKeyToken=null",
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityBlockType(
                "Workflow Entry",
                "Used to enter information for a workflow that has interactive elements.",
                "Rock.Blocks.Workflow.WorkflowEntry",
                "Workflow",
                "9116aad8-cf16-4bce-b0cf-5b4d565710ed" );

            RockMigrationHelper.UpdateDefinedValue(
                "e43ad92c-4dd4-4d78-9852-fcfaefdf52ca",
                "Default",
                @"{
    header: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Header,
        inlineToolbar: ['link'
        ],
        config: {
            placeholder: 'Header'
        },
        shortcut: 'CMD+SHIFT+H'
    },
    image: {
        class: Rock.UI.StructuredContentEditor.EditorTools.RockImage,
        inlineToolbar: ['link'
        ],
    },
    list: {
        class: Rock.UI.StructuredContentEditor.EditorTools.NestedList,
        inlineToolbar: true,
        shortcut: 'CMD+SHIFT+L'
    },
    checklist: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Checklist,
        inlineToolbar: true,
    },
    quote: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Quote,
        inlineToolbar: true,
        config: {
            quotePlaceholder: 'Enter a quote',
            captionPlaceholder: 'Quote\'s author',
        },
        shortcut: 'CMD+SHIFT+O'
    },
    alert: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Alert,
        inlineToolbar: ['bold', 'italic'],
        config: {
            alertTypes: ['info', 'success', 'warning', 'danger']
        }
    },
    warning: Rock.UI.StructuredContentEditor.EditorTools.Warning,
    marker: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Marker,
        shortcut: 'CMD+SHIFT+M'
    },
    code: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Code,
        shortcut: 'CMD+SHIFT+C'
    },
    raw: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Raw
    },
    delimiter: Rock.UI.StructuredContentEditor.EditorTools.Delimiter,
    inlineCode: {
        class: Rock.UI.StructuredContentEditor.EditorTools.InlineCode,
        shortcut: 'CMD+SHIFT+C'
    },
    embed: Rock.UI.StructuredContentEditor.EditorTools.Embed,
    table: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Table,
        config: {
            defaultHeadings: true
        },
        inlineToolbar: true,
        shortcut: 'CMD+ALT+T'
    }
}",
                "09b25845-b879-4e69-87e9-003f9380b8dd" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.UpdateDefinedValue(
                "e43ad92c-4dd4-4d78-9852-fcfaefdf52ca",
                "Default",
                @"{
    header: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Header,
        inlineToolbar: ['link'
        ],
        config: {
            placeholder: 'Header'
        },
        shortcut: 'CMD+SHIFT+H'
    },
    image: {
        class: Rock.UI.StructuredContentEditor.EditorTools.RockImage,
        inlineToolbar: ['link'
        ],
    },
    list: {
        class: Rock.UI.StructuredContentEditor.EditorTools.NestedList,
        inlineToolbar: true,
        shortcut: 'CMD+SHIFT+L'
    },
    checklist: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Checklist,
        inlineToolbar: true,
    },
    quote: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Quote,
        inlineToolbar: true,
        config: {
            quotePlaceholder: 'Enter a quote',
            captionPlaceholder: 'Quote\'s author',
        },
        shortcut: 'CMD+SHIFT+O'
    },
    warning: Rock.UI.StructuredContentEditor.EditorTools.Warning,
    marker: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Marker,
        shortcut: 'CMD+SHIFT+M'
    },
    code: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Code,
        shortcut: 'CMD+SHIFT+C'
    },
    raw: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Raw
    },
    delimiter: Rock.UI.StructuredContentEditor.EditorTools.Delimiter,
    inlineCode: {
        class: Rock.UI.StructuredContentEditor.EditorTools.InlineCode,
        shortcut: 'CMD+SHIFT+C'
    },
    embed: Rock.UI.StructuredContentEditor.EditorTools.Embed,
    table: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Table,
        config: {
            defaultHeadings: true
        },
        inlineToolbar: true,
        shortcut: 'CMD+ALT+T'
    }
}",
                "09b25845-b879-4e69-87e9-003f9380b8dd" );

            RockMigrationHelper.RenameEntityType( "02d2dba8-5300-4367-b15b-e37dfb3f7d1e",
                "Rock.Blocks.Types.Mobile.Cms.WorkflowEntry",
                "Workflow Entry",
                "Rock.Blocks.Types.Mobile.Cms.WorkflowEntry, Rock, Version=17.1.1.0, Culture=neutral, PublicKeyToken=null",
                false,
                false );

            RockMigrationHelper.AddOrUpdateEntityBlockType(
                "Workflow Entry",
                "Allows for filling out workflows from a mobile application.",
                "Rock.Blocks.Types.Mobile.Cms.WorkflowEntry",
                "Mobile > Cms",
                "9116aad8-cf16-4bce-b0cf-5b4d565710ed" );

        }
    }
}
