<template>
  <v-app>
      <v-main>
        
          <div class="row" style="dislay:flex; flex-direction:row; flex-wrap:no-wrap; align-items:baseline; justify-content:space-between; margin-bottom:10px; width:100%;">
             <div class="col-md-3" >
               
                  <div style="display:flex; flex-direction:row; flex-wrap:no-wrap; justify-content:space-around; align-items:baseline;">
                    <i class="fa fa-chevron-left"
                      v-if="this.focus > new Date()"
                        @click='prev'
                      ></i>
                    <h4>{{getDate }} </h4>
                    <i class="fa fa-chevron-right"
                      @click="next"></i>
               
                </div>
          </div>
            <div class="col-md-3">
              <multiselect v-model="SelectedCampus" :options="Campuses" :multiple="true" :close-on-select="false" :clear-on-select="false" :preserve-search="true" placeholder="Filter By Campus" label="CampusName" track-by="CampusName" :preselect-first="false">
                <template slot="selection" slot-scope="{ values, search, isOpen }"><span class="multiselect__single" v-if="values.length &amp;&amp; !isOpen">{{ values.length }} options selected</span></template>
              </multiselect>
            </div>
          <div class="col-md-3">
            <multiselect 
              v-model="SelectedCategory" 
              :options="Categories" 
              :multiple="true" 
              :close-on-select="false" 
              :clear-on-select="false" 
              :preserve-search="true"
              placeholder="Filter By Category" 
              label="Value" 
              track-by="Id" 
              :preselect-first="false"
              :allowEmpty="true"
              >
                <template slot="selection" slot-scope="{ values, search, isOpen }"><span class="multiselect__single" v-if="values.length &amp;&amp; !isOpen">{{ values.length }} options selected</span>
                </template>
            </multiselect>
            </div>
         <div class="col-md-1">
            <div class="btn btn-primary" @click="clearFilters">Clear Filters</div>
          
            </div>
          </div>
          <div class="row" style="dislay:flex; flex-direction:row; flex-wrap:no-wrap; align-items:baseline; justify-content:flex-end; margin-bottom:10px; width:100%;">
            <v-spacer></v-spacer>
                    <div class="col-md-2" style="text-align:right;">
          
              <v-btn-toggle v-model="SelectedView">
              <v-btn small>
                <i class="fa fa-th"></i>
              </v-btn>
              <v-btn small>
                <i class="fa fa-calendar"></i>
              </v-btn>
            </v-btn-toggle>
            </div>
          </div>
        
          
        
        <CalendarView v-if="SelectedView == '1'" :Events="filteredEvents" :focus="focus" :type="calendarType" @setFocus="setFocus" @ShowModal="showModal" @CalendarType="SetType" />

        <EventList 
          v-else
          :Events="filteredEvents"
          @ShowModal="showModal"
          @Previous="prev"
          @Next="next"
          @SetFocus="setFocus"
      
          />

         <transition name="fade">
          <EventModal v-if="focusEvent" @CloseModal="closeModal" :Event="focusEvent"/>
        </transition>
        </div>
    </v-main>
  </v-app>
</template>

<script>
import Multiselect from 'vue-multiselect'
import EventList from './components/EventList'
import CalendarView from './components/CalendarView'
import EventModal from './components/EventModal'
export default {
  name: 'App',

  components: {
    Multiselect,
    EventList,
    CalendarView,
    EventModal
  },

  data: () => ({
    CurrentPerson: {}, //
    SelectedCampus: null,
    SelectedCategory: null,
    SelectedView: 0,
    focus:null,
    focusEvent: null,
    calendarType:'month',
    loadStartDate: new Date(),
    Events: []
  }),
  async created() {
    this.focus = new Date();
    try {
      const response = await fetch('rock.voxchurch.org/api/People/GetCurrentPerson', {
        credentials: 'include', // include, *same-origin, omit
      });
      const user = await response.json();
      this.CurrentPerson = user;
    } catch (err) {
      //
      console.log('User Not Logged In');
    }

    try {
      let startDate = new Date();
      let endDate = new Date(startDate.getFullYear(), startDate.getMonth() + 1, 1).toISOString();
      const response = await fetch(`https://voxchurch.org/api/com_bemaservices/EventLink/GetCalendarItems?CalendarIds=5&startDateTime=${startDate.toISOString()}&endDateTime=${endDate}`, {
        credentials: 'include',
      });
      const events = await response.json();
      this.Events = events;
    } catch (err) {
      console.log('Error Downloading Events: ', err);
    }
  },
  watch: {
      focus: async function (val) {
      let daystoAdd = 30;
      let endDate = new Date(val.getFullYear(), val.getMonth() + 1, 1).toISOString();
      let startDate = new Date(val);
       switch(this.calendarType) {
        case 'day':
         endDate = new Date(startDate.setDate(startDate.getDate()+1)).toISOString()
         break;
        case 'week':
          endDate = new Date(startDate.setDate(startDate.getDate()+7)).toISOString()
          break;
      }
      try {
        
      const response = await fetch(`https://voxchurch.org/api/com_bemaservices/EventLink/GetCalendarItems?CalendarIds=5&startDateTime=${startDate.toISOString()}&endDateTime=${endDate}`, {
        credentials: 'include',
      });
      const events = await response.json();
      const newEvents = []
      await events.forEach(
        (newEvent) => {
         if(this.Events.findIndex(event => event.OccurrenceId == newEvent.OccurrenceId) < 0 ){
           newEvents.push(newEvent)
         }
        }

      )
      console.log(this.Events, newEvents)
      this.Events = this.Events.concat(newEvents)



      
      
    } catch (err) {
      console.log('Error Downloading Events: ', err);
    }
    },
  },
  methods: {
    SetType(payload) {
      this.calendarType = payload.type
    },
    showModal(payload){
      document.getElementById('app').classList.add("modal-open");
      this.focusEvent = payload;
    },
    closeModal(){
      document.getElementById('app').classList.remove("modal-open");
      this.focusEvent = null;
    },
    clearFilters() {
      this.SelectedCampus = null
      this.SelectedCategory = null;
    },
    prev(){
      switch(this.calendarType) {
        case 'day':
          if(this.focus){
            let currentfocus = new Date(this.focus)
            let newDate = currentfocus.setDate(currentfocus.getDate()-1);
             this.focus = new Date(newDate);
          } else {
            let currentfocus = new Date()
            this.focus =  new Date(currentfocus.setDate(currentfocus.getDate()-1));
          }    // code block
          break;
        case 'week':
          if(this.focus){
            let currentfocus = new Date(this.focus)
            let newDate = currentfocus.setDate(currentfocus.getDate()-7);
             this.focus = new Date(newDate);;
          } else {
            let currentfocus = new Date()
            let newDate = currentfocus.setDate(currentfocus.getDate()-7);
             this.focus = new Date(newDate);;
          }    // code block
          break;
        default:
          if(this.focus){
            let currentfocus = new Date(this.focus)
            this.focus = new Date(currentfocus.getFullYear(), currentfocus.getMonth() - 1, 1);
          } else {
            let today = new Date()
            this.focus = new Date( today.getFullYear(), today.getMonth() - 1, 1)
          }    // code block
      }
    },
    next(){
      switch(this.calendarType) {
        case 'day':
          if(this.focus){
            let currentfocus = new Date(this.focus)
            let newDate = currentfocus.setDate(currentfocus.getDate()+1);
             this.focus = new Date(newDate);
          } else {
            let currentfocus = new Date()
            this.focus =  new Date(currentfocus.setDate(currentfocus.getDate()+1));
          }    // code block
          break;
        case 'week':
          if(this.focus){
            let currentfocus = new Date(this.focus)
            let newDate = currentfocus.setDate(currentfocus.getDate()+7);
             this.focus = new Date(newDate);;
          } else {
            let currentfocus = new Date()
            let newDate = currentfocus.setDate(currentfocus.getDate()+7);
             this.focus = new Date(newDate);;
          }    // code block
          break;
        default:
          if(this.focus){
            let currentfocus = new Date(this.focus)
            this.focus = new Date(currentfocus.getFullYear(), currentfocus.getMonth() + 1, 1);
          } else {
            let today = new Date()
            this.focus = new Date( today.getFullYear(), today.getMonth() + 1, 1)
          }    // code block
      }
 
    },
    setFocus(val){
      
      if(val && val != null ) {
       
        this.focus = new Date(val);
      } else {
        this.focus = new Date();
      }
    }
  
  },
  computed: {
      getDate(){
      if(this.focus && this.focus != '' ){
        return `${new Date(this.focus).toLocaleString('en-us', { month: 'long' }) } ${new Date(this.focus).getFullYear()}`
      } else {
        return `${new Date().toLocaleString('en-us', { month: 'long' }) } ${new Date().getFullYear()}`
      }
    },
    filteredEvents(){
      
      let selectedCategories = this.SelectedCategory
      let filteredEvents = this.Events
      let startDate = new Date();
      var endDate = new Date(startDate.getFullYear(), startDate.getMonth() + 1, 0);
      
      if(this.SelectedCampus && this.SelectedCampus.length > 0 ){
        let selectedCampuses = this.SelectedCampus.map((e) => { return e.CampusId })
        filteredEvents = filteredEvents.filter( tag => selectedCampuses.includes(tag.CampusId) == true)

      }

      if(this.SelectedCategory && this.SelectedCategory.length > 0 ){
        let selectedCategories = this.SelectedCategory.map((e) => { return e.Id })
        
        filteredEvents = filteredEvents.filter( e => {
          let itemCategoriesList = e.Categories.map(category => { return category.Id })
          return itemCategoriesList.some(r=> selectedCategories.indexOf(r) >= 0)
        })
      }
      if(this.focus ) {
        startDate = this.focus
        endDate = new Date (startDate.getFullYear(), startDate.getMonth() + 1, 1)
      }
      
      filteredEvents = filteredEvents.filter ( e => {
          let nextStartDate = new Date(e.EventNextStartDate.StartDateTime).getTime()
          
          return nextStartDate < endDate.getTime() && nextStartDate >= new Date();
          
          })
      return filteredEvents.sort((a,b) => ((a.EventNextStartDate.StartDateTime > b.EventNextStartDate.StartDateTime ) ? 1 : -1));
    },
    Campuses() {
      const result = [];
      const map = new Map();
      this.Events.map((item) => {
        if (!map.has(item.CampusId)) {
          map.set(item.CampusId, true); // set any value to Map
          if (item.CampusId) {
            result.push({
              CampusId: item.CampusId,
              CampusName: item.CampusName,
            });
          }
        }
      });

      return result.sort((a, b) => ((a.CampusName > b.CampusName) ? 1 : -1));
    },
    Categories() {
      const currentDate = new Date()
      const categoryList = [];
      this.Events.map((item) => {
        if (item.Categories.length > 0 && item.EventNextStartDate.NextStartDateTime >= currentDate) {
          item.Categories.map((category) => {
            const newCategory = {};
            newCategory.Id = category.Id;
            newCategory.Value = category.Value;
            categoryList.push(newCategory);
          });
        }
      });

      const result = [];
      const map = new Map();
      categoryList.map((item) => {
        if (!map.has(item.Id)) {
          map.set(item.Id, true); // set any value to Map
          if (item.Id) {
            result.push({
              Id: item.Id,
              Value: item.Value,
            });
          }
        }
      });

      return result.sort((a, b) => ((a.CampusName > b.CampusName) ? 1 : -1));
    },
  },
};
</script>
<style>
.fade-enter-active, .fade-leave-active {
  transition: opacity .5s;
}
.fade-enter, .fade-leave-to /* .fade-leave-active below version 2.1.8 */ {
  opacity: 0;
}
</style>