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

import { defineComponent, PropType } from 'vue';
import { newGuid } from '../Util/Guid';

/** The data needed to represent an item in a ProgressTracker */
export interface ProgressTrackerItem
{
    Title: string;
    Subtitle: string;
    Key: string;
}

const ProgressTrackerItem = defineComponent( {
    name: 'ProgressTrackerItem',
    props: {
        isPast: {
            type: Boolean as PropType<boolean>,
            required: true
        },
        isPresent: {
            type: Boolean as PropType<boolean>,
            required: true
        },
        isFuture: {
            type: Boolean as PropType<boolean>,
            required: true
        },
        isLast: {
            type: Boolean as PropType<boolean>,
            required: true
        },
        item: {
            type: Object as PropType<ProgressTrackerItem>,
            required: true
        }
    },
    template: `
<li class="progress-step progress-tracker-priority">
    <div v-if="isPast" class="progress-step-link">
        <div class="progress-tracker-icon">
            <i class="fas fa-check"></i>
        </div>
        <div class="progress-tracker-details">
            <span class="progress-tracker-title text-truncate">{{item.Title}}</span>
            <p class="progress-tracker-subtitle text-truncate">{{item.Subtitle}}</p>
        </div>
    </div>
    <div v-else-if="isPresent" class="progress-step-link">
        <div class="progress-tracker-icon current"></div>
        <div class="progress-tracker-details">
            <span class="progress-tracker-title text-truncate">{{item.Title}}</span>
            <p class="progress-tracker-subtitle text-truncate">{{item.Subtitle}}</p>
        </div>
    </div>
    <div v-else-if="isFuture" class="progress-step-link">
        <div class="progress-tracker-icon upcoming"></div>
        <div class="progress-tracker-details">
            <span class="progress-tracker-title text-truncate">{{item.Title}}</span>
            <p class="progress-tracker-subtitle text-truncate">{{item.Subtitle}}</p>
        </div>
    </div>
    <div v-if="!isLast" class="progress-tracker-arrow">
        <svg viewBox="0 0 22 80" fill="none" preserveAspectRatio="none">
            <path d="M0 -2L20 40L0 82" vector-effect="non-scaling-stroke" stroke="currentcolor" stroke-linejoin="round" />
        </svg>
    </div>
</li>
`
} );

/** Displays a roadmap of successive steps that help the user understand where in a
 *  series of forms they currently are working. */
const ProgressTracker = defineComponent( {
    name: 'ProgressTracker',
    components: {
        ProgressTrackerItem
    },
    props: {
        currentIndex: {
            type: Number as PropType<number>,
            required: true
        },
        items: {
            type: Array as PropType<ProgressTrackerItem[]>,
            required: true
        }
    },
    data ()
    {
        return {
            guid: newGuid(),
            collapsedIndexes: [] as number[]
        };
    },
    computed: {
        /** Is the given index collapsed? */
        isCollapsed (): (index: number) => boolean
        {
            return ( index: number ) => this.collapsedIndexes.indexOf( index ) !== -1;
        },

        /** A list of indexes that should not be collapsed. These are not guaranteed to be valid or unique
         *  indexes, but rather a collection to check existance before collapsing a particular index. */
        doNotCollapseIndexes (): number[]
        {
            return [ 0, this.currentIndex - 1, this.currentIndex, this.currentIndex + 1, this.lastIndex ];
        },

        /** The last index of the items (prop) */
        lastIndex (): number
        {
            return this.items.length - 1;
        },

        /** The element id of the progress tracker (child) */
        progressTrackerElementId (): string
        {
            return `progress-tracker-${this.guid}`;
        },

        /** The element id of the progress tracker container (parent) */
        progressTrackerContainerElementId (): string
        {
            return `progress-tracker-container-${this.guid}`;
        },
    },
    methods: {
        /** Expand all items and then collapse some to fit if needed */
        expandAndCollapseItemsBecauseOfWidth ()
        {
            this.collapsedIndexes = [];
            this.$nextTick( () => this.collapseItemsBecauseOfWidth() );
        },

        /** Collapse some items if needed to make fit */
        collapseItemsBecauseOfWidth ()
        {
            // Using the DOM query getElementById because Vue refs were not conveying the changing width
            const progressTracker = document.getElementById( this.progressTrackerElementId );
            const progressTrackerContainer = document.getElementById( this.progressTrackerContainerElementId );

            const containerWidth = progressTrackerContainer?.clientWidth;
            const childWidth = progressTracker?.scrollWidth;

            if ( !containerWidth || !childWidth || childWidth <= containerWidth )
            {
                return;
            }

            // Collapse the furthest away index that can be collapsed
            const midPoint = this.lastIndex / 2;

            if ( this.currentIndex > midPoint )
            {
                for ( let i = 0; i <= this.lastIndex; i++ )
                {
                    if ( this.doNotCollapseIndexes.indexOf( i ) !== -1 )
                    {
                        continue;
                    }

                    if ( this.isCollapsed( i ) )
                    {
                        continue;
                    }

                    // After collapsing the first index that can be, then wait for the DOM to update (nexttick) and
                    // collapse more if needed
                    this.collapsedIndexes.push( i );
                    this.$nextTick( () => this.collapseItemsBecauseOfWidth() );
                    return;
                }
            }
            else
            {
                for ( let i = this.lastIndex; i >= 0; i-- )
                {
                    if ( this.doNotCollapseIndexes.indexOf( i ) !== -1 )
                    {
                        continue;
                    }

                    if ( this.isCollapsed( i ) )
                    {
                        continue;
                    }

                    // After collapsing the first index that can be, then wait for the DOM to update (nexttick) and
                    // collapse more if needed
                    this.collapsedIndexes.push( i );
                    this.$nextTick( () => this.collapseItemsBecauseOfWidth() );
                    return;
                }
            }
        }
    },
    watch: {
        currentIndex: {
            immediate: true,
            handler ()
            {
                this.expandAndCollapseItemsBecauseOfWidth();
            }
        }
    },
    template: `
<nav class="progress-tracker" style="margin: 20px auto; max-width: 1200px; width:100%">
    <div :id="progressTrackerContainerElementId" class="progress-tracker-container d-none d-md-block">
        <ul :id="progressTrackerElementId" class="progress-steps">
            <template v-for="(item, index) in items" :key="item.key">
                <li v-if="isCollapsed(index)" class="progress-step progress-tracker-more">
                    <div class="progress-step-link">
                        <i class="fas fa-ellipsis-v"></i>
                    </div>
                    <div class="progress-tracker-arrow">
                        <svg viewBox="0 0 22 80" fill="none" preserveAspectRatio="none">
                            <path d="M0 -2L20 40L0 82" vector-effect="non-scaling-stroke" stroke="currentcolor" stroke-linejoin="round" />
                        </svg>
                    </div>
                </li>
                <ProgressTrackerItem
                    v-else
                    :item="item"
                    :isPast="index < currentIndex"
                    :isPresent="index === currentIndex"
                    :isFuture="index > currentIndex"
                    :isLast="index === lastIndex" />
            </template>
        </ul>
    </div>
    <slot name="aside" />
</nav>`
} );

export default ProgressTracker;
