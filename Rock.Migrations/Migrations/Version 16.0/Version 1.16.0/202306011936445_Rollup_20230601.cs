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
    ///
    /// </summary>
    public partial class Rollup_20230601 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            FixLockedOutCaptionLoginBlockSettingsUp();
            UpdateRealTimeVisualizerThemesUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// JMH: Fixes the "Locked Out Caption" Login Block setting.
        /// </summary>
        private void FixLockedOutCaptionLoginBlockSettingsUp()
        {
            FixLockedOutCaptionLoginBlockSettingsUp( "7B83D513-1178-429E-93FF-E76430E038E4" ); // WebForms Login Block Type
            FixLockedOutCaptionLoginBlockSettingsUp( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3" ); // Obsidian Login Block Type
        }

        /// <summary>
        /// JMH: Fixes the "Locked Out Caption" Login Block setting for a specific BlockType.
        /// </summary>
        private void FixLockedOutCaptionLoginBlockSettingsUp( string blockTypeGuid )
        {
            Sql( $@"
DECLARE @BlockTypeId AS int
SELECT @BlockTypeId=Id FROM [BlockType] WHERE [Guid] = '{blockTypeGuid}'
DECLARE @AttributeId AS int
SELECT @AttributeId=Id FROM [Attribute] WHERE [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = @BlockTypeId AND [Key] = 'LockedOutCaption'
-- Update the default attribute value.
UPDATE [Attribute]
   SET [DefaultValue] = REPLACE([DefaultValue], 'assign phone = Global''', 'assign phone = ''Global''')
     , [IsDefaultPersistedValueDirty] = 1
 WHERE [Id] = @AttributeId
-- Update attribute values.
UPDATE [AttributeValue]
   SET [Value] = REPLACE([Value], 'assign phone = Global''', 'assign phone = ''Global''')
     , [IsPersistedValueDirty] = 1
WHERE [AttributeId] = @AttributeId" );
        }

        /// <summary>
        /// DH: Update RealTime Visualizer Themes
        /// </summary>
        public void UpdateRealTimeVisualizerThemesUp()
        {
            RockMigrationHelper.UpdateDefinedValueAttributeValue(
                "1D830402-3378-43C0-A8A7-088DBD3CE57B",
                "A0A3BDEE-89AF-462E-807D-8157CAD8D3A0",
                @".visualizer-container {
    display: flex;
    flex-direction: column;
    min-height: 400px;
    position: relative;
{% if Settings.fullscreen == ""true"" %}
    position: fixed;
    left: 0px;
    top: 0px;
    right: 0px;
    bottom: 0px;
{% endif %}
}

.visualizer-container > canvas {
    position: absolute;
    left: 0px;
    top: 0px;
    right: 0px;
    bottom: 0px;
}

.visualizer-container > .realtime-visualizer-item {
    height: 0px;
}

/* IN transition initial states. */
.visualizer-container > .realtime-visualizer-item.left-in {
    transform: translateX(calc(var(--slideAmount) * -1));
}

.visualizer-container > .realtime-visualizer-item.top-in {
    transform: translateY(calc(var(--slideAmount) * -1));
}

.visualizer-container > .realtime-visualizer-item.right-in {
    transform: translateX(var(--slideAmount));
}

.visualizer-container > .realtime-visualizer-item.bottom-in {
    transform: translateY(var(--slideAmount));
}

.visualizer-container > .realtime-visualizer-item.fade-in {
    opacity: 0;
}

/* IN transition final states. */
.visualizer-container > .realtime-visualizer-item.left-in.in,
.visualizer-container > .realtime-visualizer-item.top-in.in,
.visualizer-container > .realtime-visualizer-item.right-in.in,
.visualizer-container > .realtime-visualizer-item.bottom-in.in {
    transform: initial;
}

.visualizer-container > .realtime-visualizer-item.fade-in.in {
    opacity: 1;
}

/* OUT transition final states. */
.visualizer-container > .realtime-visualizer-item.left-out.out {
    transform: translateX(calc(var(--slideAmount) * -1));
}

.visualizer-container > .realtime-visualizer-item.top-out.out {
    transform: translateY(calc(var(--slideAmount) * -1));
}

.visualizer-container > .realtime-visualizer-item.right-out.out {
    transform: translateX(var(--slideAmount));
}

.visualizer-container > .realtime-visualizer-item.bottom-out.out {
    transform: translateY(var(--slideAmount));
}

.visualizer-container > .realtime-visualizer-item.fade-out.out {
    opacity: 0;
    overflow-y: initial;
}

/* Transition Timings. */
.visualizer-container > .realtime-visualizer-item.in {
    transition: height var(--animationDuration) ease-out, transform var(--animationDuration) ease-out, opacity var(--animationDuration) ease-out;
}

.visualizer-container > .realtime-visualizer-item.out {
    transition: opacity var(--animationDuration) ease-in, transform var(--animationDuration) ease-in, height var(--animationDuration) ease-in;
}
" );

            RockMigrationHelper.UpdateDefinedValueAttributeValue(
                "1D830402-3378-43C0-A8A7-088DBD3CE57B",
                "6F875F31-F43B-4C33-9183-33FD78EB21F7",
                @"let helper = undefined;

// Called one time to initialize everything before any
// calls to showItem() are made.
async function setup(container, settings) {
    const itemContainer = container.getElementsByClassName(""visualizer-container"")[0];
    const fingerprint = crypto.randomUUID ? crypto.randomUUID() : Math.random().toString();

    const Helper = (await import(`/Scripts/Rock/UI/realtimevisualizer/common.js?v=${fingerprint}`)).Helper;
    helper = new Helper(itemContainer);
}

// Shows a single visual item from the RealTime system.
async function showItem(content, container, settings) {
    if (!content) {
        return;
    }
    
    const itemContainer = container.getElementsByClassName(""visualizer-container"")[0];
    
    // Configure the item and it's content.
    const item = document.createElement(""div"");
    item.classList.add(""realtime-visualizer-item"");
    item.innerHTML = content;
    itemContainer.prepend(item);

    // Configure all the animation classes.
    if (settings.fade === ""true"") {
        item.classList.add(""fade-in"", ""fade-out"");
    }

    if (settings.slideInDirection) {
        item.classList.add(`${settings.slideInDirection}-in`);
    }

    if (settings.slideOutDirection) {
        item.classList.add(`${settings.slideOutDirection}-out`);
    }

    // Show the item.
    helper.setItemHeight(item);
    item.classList.add(""in"");

    // Start up all the extras.
    if (settings.playAudio === ""true"") {
        helper.playAudio(item, settings.defautlAudioUrl);
    }
    
    if (settings.confetti === ""true"") {
        helper.showConfetti();
    }
    
    if (settings.fireworks === ""true'"") {
        helper.startFireworks();
    }
    
    // Wait until this item should be removed and then start
    // the removal process.
    setTimeout(() => {
        item.classList.add(""out"");
        item.style.height = ""0px"";

        item.addEventListener(""transitionend"", () => {
            if (item.parentElement) {
                item.remove();

                if (settings.fireworks === ""true"") {
                    helper.stopFireworks();
                }
            }
        });
    }, parseInt(settings.duration) || 5000);
}
" );

            RockMigrationHelper.UpdateDefinedValueAttributeValue(
                "6262166B-B25A-458C-B531-2FD4768059AD",
                "A0A3BDEE-89AF-462E-807D-8157CAD8D3A0",
                @".visualizer-container {
    display: flex;
    flex-direction: column;
    min-height: 400px;
    position: relative;
{% if Settings.fullscreen == ""true"" %}
    position: fixed;
    left: 0px;
    top: 0px;
    right: 0px;
    bottom: 0px;
{% endif %}
}

.visualizer-container > canvas {
    position: absolute;
    left: 0px;
    top: 0px;
    right: 0px;
    bottom: 0px;
}

.visualizer-item.in {
  animation: 1.5s incoming both;
}
::view-transition-old(outgoing) {
  animation: 1.5s outgoing both;
}

@keyframes outgoing {
  0% {
    opacity: 1;
  }
  100% {
    opacity: 0;
  }
}

@keyframes incoming {
  0% {
    opacity: 0;
  }
  100% {
    opacity: 1;
  }
}
" );

            RockMigrationHelper.UpdateDefinedValueAttributeValue(
                "6262166B-B25A-458C-B531-2FD4768059AD",
                "6F875F31-F43B-4C33-9183-33FD78EB21F7",
                @"let helper = undefined;
let itemCount = 0;

// Called one time to initialize everything before any
// calls to showItem() are made.
async function setup(container, settings) {
    const itemContainer = container.getElementsByClassName(""visualizer-container"")[0];
    const fingerprint = crypto.randomUUID ? crypto.randomUUID() : Math.random().toString();

    const Helper = (await import(`/Scripts/Rock/UI/realtimevisualizer/common.js?v=${fingerprint}`)).Helper;
    helper = new Helper(itemContainer);
}

// Shows a single visual item from the RealTime system.
async function showItem(content, container, settings) {
    if (!content) {
        return;
    }
    
    const itemContainer = container.getElementsByClassName(""visualizer-container"")[0];
    
    // Configure the item and it's content.
    const item = document.createElement(""div"");
    item.classList.add(""visualizer-item"", ""in"");
    item.style.viewTransitionName = `visualizer-item-${itemCount++}`;
    item.innerHTML = content;

    // Prepare old items for removal.
    const oldItems = itemContainer.querySelectorAll("".visualizer-item"");
    for (let i = 0; i < oldItems.length; i++) {
        oldItems[i].classList.remove(""in"");
        oldItems[i].style.viewTransitionName = ""outgoing"";
    }

    if (document.startViewTransition) {
        document.startViewTransition(() => {
            itemContainer.prepend(item);
            for (let i = 0; i < oldItems.length; i++) {
                oldItems[i].remove();
            }
        });
    }
    else {
        itemContainer.prepend(item);
        for (let i = 0; i < oldItems.length; i++) {
            oldItems[i].remove();
        }
    }

    // Start up all the extras.
    if (settings.playAudio === ""true"") {
        helper.playAudio(item, settings.defautlAudioUrl);
    }
    
    if (settings.confetti === ""true"") {
        helper.showConfetti();
    }
}
" );
        }
    }
}
