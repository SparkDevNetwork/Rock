<template>
  <v-app>
    <v-img
        contain
        max-height="250"
        src="./Assets/images/CWTC_Title_English.jpg"
      ></v-img>

     <v-sheet height="64"
     
     class="pt-8 mb-8 "
     
     >
        <v-toolbar flat :bottom="true">
          <v-select
            background-color="transparent"
              style="width:25%"
              class="personList"
              cols="4"
              :menu-props="{ top: false, offsetY: true }"
              v-model="selectedGenders"
              :items="genderOptions"
              item-text="description"
              item-value="id"
              :deletable-chips="true"
              label="Filter By Gender"
              multiple
              chips
            >
            </v-select>
       
        <v-spacer></v-spacer>
       
          <v-select
              style="width:25%"
              class="personList"
              cols="4"
              :menu-props="{ top: false, offsetY: true }"
              v-model="selectedAgeRanges"
              :items="ageRangeOptions"
              item-text="description"
              item-value="id"
              :deletable-chips="true"
              label="Filter By Age Ranges"
              multiple
              chips
            >
            </v-select>

        <v-spacer>
        </v-spacer>
        <v-select
            background-color="transparent"
              style="width:25%"
              class="personList"
              cols="4"
              :menu-props="{ top: false, offsetY: true }"
              v-model="selectedCampus"
              :items="campusOptions"
              item-text="description"
              item-value="id"
              :deletable-chips="true"
              label="Filter By Campus"

            >
            </v-select>
        </v-toolbar>
     </v-sheet>

    <v-main class="snowflake">

              <v-btn
                id="tagButton"
                @mouseover="showCount=false"
                @mouseleave="showCount=true"
                @click="showForm"
                class="accent"
                fixed
                x-large
                bottom
                left
                fab
                style="z-index:2000"
                

              >
            
               <span id="pulledCount" class="fa-stack" :class="{hide:!showCount}">
                   <!-- Create an icon wrapped by the fa-stack class -->
                
                    <!-- The icon that will wrap the number -->
                    <span class="fas fa-tags fa-stack-2x"></span>
                    <!-- a strong element with the custom content, in this case a number -->
                    <transition name="slide-fade" mode="out-in">
                        <strong :key="pulledCount" class="fa-stack-1x accent--text" style="font-weight: 900; padding-left:5px; transform:translate(-3px,1px) scale(1.2); font-size:13px;">
                            {{pulledCount}}    
                        </strong>
                    </transition>        
                
                </span>
            
              
               <span :class="{hide:showCount}"><i class="far fa-list-alt fa-2x"></i></span>
              
              </v-btn>
              
            
       <TagList 
        :selectedGenders="selectedGenders" 
        :selectedAgeRanges="selectedAgeRanges"
        :selectedCampus="selectedCampus"
        :pulledTags="pulledTags" 

        />
        <ContactForm
        class="vuemodal"
        :class="{showItem:this.showModal}"
        :hideForm="showModal"
        :tags="this.pulledTags"
         />

    </v-main>
  </v-app>
</template>

<script>
import TagList from "./components/taglist.vue";
import ContactForm from "./components/contactform.vue"
import { EventBus } from './modules/event-bus.js';
import { gsap } from "gsap";

export default {
  name: 'App',

  components: {
    TagList,
    ContactForm
  },
  
  created(){
     fetch('/Webhooks/Lava.ashx/BEMA/GetAgeRanges')
      .then(response => response.json())
      .then(data => this.ageRangeOptions = data)
      .catch(message => console.log(message));
    
  },

  data: () => ({
    hidden:true,
    showCount:true,
    selectedGenders:[],
    selectedAgeRanges:[],
    selectedCampus: personCampus,
    pulledTags:[],
    campusOptions:campuses,
    genderOptions: [{
            description:'Boy',
            id:1
          },{
            description:'Girl',
            id:2
          }],
    ageRangeOptions: [],
    showModal:false,

    //
  }),
   
  watch:{
     
     pulledTags: function(){
         localStorage.setItem('pulledTags',JSON.stringify(this.pulledTags.map(tag => tag.id)));
     }

  },
  computed:{
    pulledCount(){

        return this.pulledTags.length
    }


  },
  methods: {
    pullTag(tag){
        this.pulledTags.push(tag);
        
    },
    showForm() {
        this.showModal = !this.showModal;
        if(this.showModal === true ) {
            document.body.style.overflow = 'hidden';
        } else {
            document.body.style.overflow = 'inherit';
        }
    },
    deleteTag(id){
        var filteredTags = this.pulledTags.filter(tag => tag.id != id)
        
        this.pulledTags = filteredTags;
    }
  },
   mounted () {
    
    EventBus.$on('addTagToPulledList', (tag) => {
      this.pullTag(tag);
    });
    EventBus.$on('closeModal', () => {
      this.showModal = false;
    });

     EventBus.$on('deleteItem', (id) => {
      this.deleteTag(id)
    });

    EventBus.$on('deleteAllTags',() =>{
        this.pulledTags = [];
    });

   
    
  },
 
};
</script>


<style>
@import url('https://fonts.googleapis.com/css2?family=Lato:wght@400;700;900&display=swap');

.v-application {
   /* background-image: url("./assets/images/CWTC_BlueSnowBackground.jpg"); */
  background-repeat: repeat;
  border: 2rem solid var(--v-primary-base,green);
  padding-bottom:20px;
  font-family: 'Lato', sans-serif !important;
}
</style>
<style scoped>
*, *:after, *:before {
  box-sizing: border-box;
}


.btn {
    font-size:1.2rem;
}



/* Enter and leave animations can use different */
/* durations and timing functions.              */
.slide-fade-enter-active {
  transition: all .3s ease;
}
.slide-fade-leave-active {
  transition: all .8s cubic-bezier(1.0, 0.5, 0.8, 1.0);
}
.slide-fade-enter, .slide-fade-leave-to
/* .slide-fade-leave-active below version 2.1.8 */ {
  transform: translateX(10px);
  opacity: 0;
}
.vuemodal {
    position:fixed;
    bottom:0;
    left:0;
    height:100vh;
    width: 100vw;
    transform: scale(0);
    
    z-index:30000;
    transform-origin:bottom left;
    display:flex;
    flex-direction:column;
    justify-content:space-around;
    align-items:center;
    transition: all 500ms ease;
}
.vuemodal.showItem{
    transform:scale(1);
    transition: all 500ms ease;
}


</style>
