<template>
    <transition name="slide-fade">
    <div class="tag-container"  :class="{added:this.pulled}" :ref="'tag' + tag.id" >
            <div class="tag">

                <div :class="`tag-side tag-${selectedColor.id}-side `">
                    <div :class="`tag-${selectedColor.id}-top`"></div>
                    <div :class="`tag-text tag-${selectedColor.id}-text`">
                        {{ tag.ageRange.description }} <br />
                        {{ tag.gender.description }}<br />
                        <div v-if="selectedColor.shape != ''" :class="selectedColor.rulesClass" v-html="selectedColor.shape"></div>
                        <div v-else class="rule-diagonal"></div>
                    </div>
                </div>

                <div :class="`tag-side tag-${selectedColor.id}-side is-back`">
                    <div :class="`tag-${selectedColor.id}-top`"></div>
                    <div :class="`tag-text tag-${selectedColor.id}-text`">
                        <span v-if="tag.description && tag.description.length > 0">
                            {{tag.description}}
                        </span>
                        <span v-else>
                            {{defaultMessage}}
                        </span>
                        <v-btn 
                         color="accent" 
                         elevation="2"
                         @click="addCard(tag, 'tag' + tag.id)">
                         Pull Tag
                         </v-btn>
                        <div v-if="selectedColor.shape != ''" class="rule-shape" v-html="selectedColor.shape"></div>
                        <div v-else class="rule-diagonal"></div>
                    </div>
                </div>

            </div>
        </div>
    </transition>
</template>

<script>
// Import the EventBus we just created.
import { EventBus } from '../modules/event-bus.js';
import { gsap } from "gsap";

export default {
    props:{
        tag: Object,
    },
      created () {
    window.addEventListener('scroll', this.handleScroll);
    },
    destroyed () {
        window.removeEventListener('scroll', this.handleScroll);
    },
    mounted(){
        this.tagColor(Math.floor(Math.random()*(4-0+1)+0) + this.tag.gender.id + this.tag.ageRange.id)
    },
    data(){
        return {
            selectedColor:{},
            pulled:false,
            colorOptions:colorOptions,
            defaultMessage: defaultMessage
        }
    },
    computed: {
      
  },
    methods: {
        addCard(tag,ref) {
            //Change the tag to pulled and emit event
            
            this.pulled = !this.pulled;
            
            //Get the location of the button
            let btn = document.querySelector('#tagButton')
            let btnviewportOffset = btn.getBoundingClientRect();
            let btntop = btnviewportOffset.top;
            let btnleft = btnviewportOffset.left;
            
            //Get the location fo the tag
            let el = this.$refs[ref];
            
            let elviewportOffset = el.getBoundingClientRect();
            let eltop = elviewportOffset.top;
            let elleft = elviewportOffset.left;


            // let elviewportOffset = el.getBoundingClientRect();
            // let eltop = el.getBoundingClientRect().top + 
            //     el.ownerDocument.defaultView.pageYOffset;
            // let elleft = elviewportOffset.left;
            el.style.top = eltop +'px';
            el.style.left = elleft + 'px';
            el.style.position = 'fixed';
            
            
            //Calculate Movement value and add transform
            let moveLeft = (elleft + btnleft + 60) * -1;
            let moveDown = ((eltop - btntop ) * -1 -150);
           
            var tl = gsap.timeline({onComplete:() =>{ emitValue(tag)}})
            // moveItem(el,elleft,eltop)
            
            tl.to(el, {x:moveLeft, y:moveDown, scale:.5})
            tl.to(el, {scale:0},">")
            
            tl.duration(1).play();
            

            //Fire off Emit to APP to remove add the tag to the pulled tag list
            function emitValue(tag) {
                EventBus.$emit('addTagToPulledList', tag); 
            }
            
        },
        tagColor(i) {
                
                 this.selectedColor = this.colorOptions[i % 3];
                 this.selectedColor.rulesClass = this.selectedColor.rules.join(' ');
        },
         handleScroll (event) {
        // Any code to be executed when the window is scrolled
        }

    }
}
</script>

<style scoped>

</style>