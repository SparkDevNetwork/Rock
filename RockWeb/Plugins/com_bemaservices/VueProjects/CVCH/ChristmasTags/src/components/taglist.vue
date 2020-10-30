<template>
  <transition-group tag="div" class="tags" name="slide-fade" >

      <VueTag 
        v-for="tag in filterTags" 
        :key="tag.id"
        :tag="tag"
        :gender="tag.gender" 
        :ageRange="tag.ageRange"
        
        />
        
 
  </transition-group> 
</template>

<script >
import Vue from "vue";
import VueTag from "../components/tag.vue";
import { gsap } from "gsap";
import {EventBus } from "../modules/event-bus.js"

export default {
  components: {
    VueTag
  },
  props:{
    selectedGenders: Array,
    selectedAgeRanges: Array,
    pulledTags: Array,
    selectedCampus: Number,


  },
  created() {
    window.addEventListener('scroll', () => {
      this.bottom = this.bottomVisible()
    })
    
    fetch('/Webhooks/Lava.ashx/BEMA/GetChristmasTags/0/20')
        .then(response => response.json())
        .then(data => {console.log(data); this.taglist = data})
        .catch(er => console.log(er));

    //gets any pulled tags out of local storage and adds them to the pulle tag list.
    let tagList = JSON.parse(localStorage.getItem('pulledTags'));
    if(tagList && tagList.length > 0){
        let pulledtags = this.taglist.filter(tag => tagList.includes(tag.id) == true);
        pulledtags.forEach((tag) => {
            
                EventBus.$emit('addTagToPulledList', tag); 
            
        })
    }
  },

  mounted(){
    
     EventBus.$on('TagsPulled', (data) =>{
        this.removeTagsFromList(data);
    });
  },

  watch: {
    bottom(bottom) {
      if (bottom) {
        // this.step ++;
        this.getMoreTags()
      }
    },
  },
  data() {
    return {
      taglist: [],
      start: 0,
      stepSize: 15,
      step:2,
      bottom: false,
    }
  },
  computed:{
    pulledIds(){
      return this.pulledTags.map(tag => tag.id);
    },
    filterTags(){
      let filteredList = this.taglist;
      
      if(this.pulledTags && this.pulledTags.length > 0 ) {
        filteredList = filteredList.filter(tag => this.pulledIds.includes(tag.id) == false)
      }

      if(this.selectedGenders && this.selectedGenders.length > 0){
        filteredList = filteredList.filter(tag => this.selectedGenders.includes(tag.gender.id) == true)
      }

      if(this.selectedAgeRanges && this.selectedAgeRanges.length > 0){
        filteredList = filteredList.filter(tag => this.selectedAgeRanges.includes(tag.ageRange.id) == true)
      }
      if(this.selectedCampus && this.selectedCampus> 0){
        filteredList = filteredList.filter(tag => this.selectedCampus == tag.campusId)
      }
      return filteredList.slice(0,this.step * this.stepSize);
      
    },


  },
  methods: {
    bottomVisible() {
      const scrollY = window.scrollY
      const visible = document.documentElement.clientHeight
      const pageHeight = document.documentElement.scrollHeight
      const bottomOfPage = visible + scrollY  >= pageHeight - 20
      return bottomOfPage || pageHeight < visible
    },
    getMoreTags() {
         fetch(`/Webhooks/Lava.ashx/BEMA/GetChristmasTags/${this.taglist.length}/20`,{
            
         })
        .then(response => response.json())
        .then(data => {
            let newItems = data.filter(item => this.taglist.includes(item) == false);
            this.taglist.push(newItems);
            
        })
        .catch(er => console.log(er));

    },
    removeTagsFromList(data){
          this.taglist = this.taglist.filter(tag => data.includes(tag.id) == false )
          EventBus.$emit('deleteAllTags');
    }
  } 

}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style  >
.tags {
    flex-wrap: wrap;
    -webkit-box-pack: center;
    justify-content: center;
    display: -webkit-box;
    display: flex;
}
.tags {
    flex-wrap: wrap;
    -webkit-box-pack: center;
    justify-content: center;
    display: -webkit-box;
    display: flex;
}
/* Shared */
.tag-container {
    width: 200px;
    height: 300px;
    margin: 20px;
    position: relative;
    -webkit-perspective: 800px;
    perspective: 800px;
    filter:none;
    transition: all 500ms ease;
}

.tag {
    width: 100%;
    height: 100%;
    position: absolute;
    -webkit-transform: translate3d(0, 0, 0);
    transform: translate3d(0, 0, 0);
    -webkit-transform-style: preserve-3d;
    transform-style: preserve-3d;
    -webkit-transition: -webkit-transform 1s;
    transition: -webkit-transform 1s;
    transition: transform 1s;
    transition: transform 1s, -webkit-transform 1s;
    
}
.tag-container:hover {
    z-index:300;
    filter:drop-shadow(0 1px 5px rgba(0,0,0,.3));
    transition: all 500ms ease;
}
.tag-container:hover .tag {
    -webkit-transform: rotateY(180deg);
    transform: rotateY(180deg)

}

.tag-side {
    width: 100%;
    height: 100%;
    position: absolute;
    -webkit-transform: translate3d(0, 0, 0);
    transform: translate3d(0, 0, 0);
    -webkit-backface-visibility: hidden;
    backface-visibility: hidden;
    display: -webkit-box;
    display: -ms-flexbox;
    display: flex;
    -webkit-box-align: stretch;
    -ms-flex-align: stretch;
    align-items: stretch;
    
}

.tag-side.is-back {
    -webkit-transform: rotateY(180deg);
    transform: rotateY(180deg);
    z-index: 2;
}

.tag-text {
    width: 100%;
    padding: 0 20px;
    color: #222;
    font-family: 'Lato', sans-serif;
    font-size: 28px;
    text-align: center;
    display: -webkit-box;
    display: -ms-flexbox;
    display: flex;
    -webkit-box-align: center;
    -ms-flex-align: center;
    align-items: center;
    -webkit-box-pack: center;
    -ms-flex-pack: center;
    justify-content: center;
    -ms-flex-wrap: wrap;
    flex-wrap: wrap;
}
:root {
    --tag1-accent: #fff;
    --tag1-side1: #ede5d8;
    --tag1-side2: #e44f47;
    
    --tag2-accent: #fff;
    --tag2-side1:#b6dfde;
    --tag2-side2: #47ada0;

    --tag3-accent: #fff;
    --tag3-side1:#e44f47;
    --tag3-side2: #b6dfde;

}
/* Tag 1 */
.tag-1-side:before {
    content: " ";
    background: var(--tag1-accent);
    width: 15px;
    height: 15px;
    border-radius: 50%;
    position: absolute;
    top: 30px;
    left: 50%;
    z-index: 1;
    -webkit-transform: translate3d(-50%, 0, 0);
    transform: translate3d(-50%, 0, 0);
}

.tag-1-top {
    width: 100%;
    margin-top: -35px;
    position: absolute;
    top: 0;
    -webkit-transform: scale(0.775, 0.5) translate3d(0, 0, 0);
    transform: scale(0.775, 0.5) translate3d(0, 0, 0);
}

.tag-1-top:before {
    content: " ";
    background: var(--tag1-side1);
    padding-bottom: 200px;
    border-bottom-left-radius: 30px;
    border-top-left-radius: 10px;
    border-top-right-radius: 30px;
    display: block;
    -webkit-transform: rotate(45deg);
    transform: rotate(45deg);
}

.tag-1-side.is-back .tag-1-top:before {
    background: var(--tag1-side2);
}

.tag-1-text {
    background: var(--tag1-side1);
    margin-top: 65px;
    border-bottom-left-radius: 10px;
    border-bottom-right-radius: 10px;
    padding-top: 30px;
    position: relative;
    z-index: 1;
    -webkit-transform: translate3d(0, 0, 0);
    transform: translate3d(0, 0, 0);
}

.tag-1-side.is-back .tag-1-text {
    background: var(--tag1-side2);
    color: var(--tag1-accent);
}

/* Tag 2 */
.tag-2-side:before,
.tag-2-side:after {
    content: " ";
    background: var(--tag2-side1);
    height: 50px;
    position: absolute;
    top: 0;
    left: 50px;
    right: 50px;
    -webkit-transform: skew(-45deg) translate3d(0, 0, 0);
    transform: skew(-45deg) translate3d(0, 0, 0);
    -webkit-transform-origin: 0 0;
    transform-origin: 0 0;
}

.tag-2-side.is-back:before,
.tag-2-side.is-back:after {
    background: var(--tag2-side2);
}

.tag-2-side:after {
    -webkit-transform: skew(45deg);
    transform: skew(45deg);
}

.tag-2-text:before {
    content: " ";
    background: var( --tag2-accent);
    width: 27px;
    height: 27px;
    border: 6px solid var(--tag2-side2);
    border-radius: 50%;
    position: absolute;
    top: 20px;
    left: 50%;
    z-index: 1;
    -webkit-transform: translate3d(-50%, 0, 0);
    transform: translate3d(-50%, 0, 0);
}

.tag-2-side.is-back .tag-2-text:before {
    border-color: var(--tag2-side1);
}

.tag-2-text {
    background: var(--tag2-side1);
    margin-top: 50px;
    padding-bottom: 30px;
}

.tag-2-side.is-back .tag-2-text {
    background: var(--tag2-side2);
}

/* Tag 3 */
.tag-3-side {
    margin-top: 20px;
    padding-top: 20px;
    display: -webkit-box;
    display: flex;
}

.tag-3-side:before {
    content: " ";
    background: var(--tag3-side1);
    width: 150px;
    height: 100%;
    border-radius: 20px;
    position: absolute;
    top: 0;
    left: 50%;
    -webkit-transform: translate3d(-50%, 0, 0);
    transform: translate3d(-50%, 0, 0);
}

.tag-3-side.is-back:before {
    background: var(--tag3-side2);
}

.tag-3-side:after {
    content: " ";
    background: #fff;
    width: 45px;
    height: 45px;
    border: 15px solid var(--tag3-side1);
    border-radius: 50%;
    position: absolute;
    top: 0;
    left: 50%;
    z-index: 1;
    -webkit-transform: translate3d(-50%, -50%, 0);
    transform: translate3d(-50%, -50%, 0);
}

.tag-3-side.is-back:after {
    border-color: var(--tag3-side2);
}

.tag-3-text {
    background: var(--tag3-side1);
    width: 100%;
    border-radius: 20px;
    padding-top: 60px;
    color: #fff;
    z-index: 1;
}

.tag-3-side.is-back .tag-3-text {
    background: var(--tag3-side2);
    color: #222;
}

/* Extras */
.rule-shape {
    width: 100%;
    color: #fff;
    font-size: 34px;
    display: -webkit-box;
    display: flex;
    -webkit-box-align: center;
    align-items: center;
    align-self: flex-end;
}

.rule-shape:before,
.rule-shape:after {
    content: " ";
    background: #fff;
    height: 1px;
    margin-bottom: 8px;
    display: block;
    -webkit-box-flex: 2;
    flex-grow: 2;
}

.rule-shape:before {
    margin-right: 6.25px;
}

.rule-shape:after {
    margin-left: 6.25px;
}

.rule-red {
    color: #e44f47;
}

.rule-red:before,
.rule-red:after {
    background: #e44f47;
}

.rule-diagonal {
    background: -webkit-repeating-linear-gradient(
        45deg,
        #e44f47,
        #e44f47 7px,
        transparent 7px,
        transparent 14px,
        #fff 14px,
        #fff 21px,
        transparent 21px,
        transparent 28px
    );
    background: repeating-linear-gradient(
        45deg,
        #e44f47,
        #e44f47 7px,
        transparent 7px,
        transparent 14px,
        #fff 14px,
        #fff 21px,
        transparent 21px,
        transparent 28px
    );
    width: 100%;
    height: 30px;
    position: absolute;
    bottom: 0;
    left: 0;
}
.is-back .tag-text{
    font-size:20px;

}
.added {
    z-index: 3000;
}
    
</style>
