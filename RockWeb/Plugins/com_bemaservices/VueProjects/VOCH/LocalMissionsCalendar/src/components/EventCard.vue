<template>
    <div

      class="col-md-4"
      style="oveflow:hidden;"
      v-if="!calculateSpotsRemaining.LimitedSpots || calculateSpotsRemaining.SpotsRemaining > 0 "
    >
    <v-card
    class="mx-auto flex-Card"
    height="100%"
    max-width="100%"
    rounded
    elevation="2"
  >
  <div>
    <div style="position:relative">
      <v-img
        class="white--text align-end gradient"
        aspect-ratio="1.7778"
        
        @load="showTitle = false"
        @error="showTitle = true"
        width="100%"
      :src="Event.EventPhoto ? Event.EventPhoto.Url : ''"
      >
      
      </v-img>
      <div class="campusBox">{{formatDate(Event.EventNextStartDate.StartDateTime)}}</div>   
    </div>
  <v-card-title class="titleFont">{{Event.EventName}}</v-card-title>
    <v-card-subtitle class="pb-0" style="display:flex; flex-direction:row; flex-wrap:nowrap; justify-content:space-between; align-items:top; flex-shirnk:1">
        
        <div style="flex-shrink:1;">
          {{ formatDate(Event.EventNextStartDate.StartDateTime) }} at {{ formatTime(Event.EventNextStartDate.StartDateTime) }}
          <div>{{ Event.OccurrenceLocation }}</div>
          <div v-if="calculateSpotsRemaining.LimitedSpots && calculateSpotsRemaining.SpotsRemaining > 0 ">Spots Remaining: {{ calculateSpotsRemaining.SpotsRemaining }}</div><br/>
        </div>
        <!-- <div style="flex-grow:1; align-content:flex-end;  break-inside: avoid;" class="text-right">
            {{Event.CampusName}}
        </div> -->

    </v-card-subtitle>
    </div>
  <div class="flex-Card">
    <v-card-text class="text--primary">
        <div>
          <div>{{ Event.EventSummary}}</div>
        </div>
    </v-card-text>
    <v-card-actions>
      <v-btn
        color="#34aeb7"
        text
        @click="$emit('ShowModal',Event)"
      >
        <span v-if="!ShowDetails">View Details</span>
       
      </v-btn>

      <v-btn
        color="#34aeb7"
        text
        v-if="!!Event.RegistrationInformation && Event.RegistrationInformation.length == 1 && calculateSpotsRemaining.LimitedSpots && calculateSpotsRemaining.SpotsRemaining > 0"
        :href="!!registration.RegistrationPublicSlug ? 'https://voxchurch.org/registration/' + registration.RegistrationPublicSlug : 'https://voxchurch.org/registration?RegistrationInstance=' + registration.RegistrationInstanceId"
      >
        Register
      </v-btn>
      <v-btn
        color="Gray"
        disabled
        text
        v-else-if="!!Event.RegistrationInformation && Event.RegistrationInformation.length == 1 && calculateSpotsRemaining.LimitedSpots && calculateSpotsRemaining.SpotsRemaining <= 0"
      >
        Registration Full
      </v-btn>
    </v-card-actions>
    </div>
  </v-card>

  </div>
</template>

<script>
 
export default {
 
  props: {
    Event: {
      type: Object,
      required: true
    }
  },
  data: () => ({
    ShowDetails: false,
    showTitle:false,
      }),

  methods: {
    formatDate(dateItem) {
      const options = {
        year: 'numeric', month: 'long', day: 'numeric',
      };

      const newDate = new Date(dateItem);
      return newDate.toLocaleDateString('en-US', options);
    },
    formatTime(dateItem) {
      const options = {
        hour: 'numeric',
        minute: '2-digit',
        hour12: true,
      };
      const newDate = new Date(dateItem);
      return newDate.toLocaleTimeString('en-US', options);
    },
    registrationSpotsRemaining(registration) {
      let SpotsRemaining = null;
      let LimitedSpots = false;

      SpotsRemaining = registration.RegistrationMaxAttendees - registration.RegistrationTotalRegistrants;
      LimitedSpots = registration.RegistrationMaxAttendees > 0 ? registration.RegistrationMaxAttendees : false;

      return {
        LimitedSpots,
        SpotsRemaining,
      };
    },
  },
  computed: {
    calculateSpotsRemaining() {
      let SpotsRemaining = null;
      let LimitedSpots = false;

      if (!!this.Event.REgistrationInformtion && this.Event.RegistrationInformation[0]) {
        SpotsRemaining = this.Event.RegistrationInformation[0].RegistrationMaxAttendees - this.Event.RegistrationInformation[0].RegistrationTotalRegistrants;
        LimitedSpots = this.Event.RegistrationInformation[0].RegistrationMaxAttendees > 0 ? this.Event.RegistrationInformation[0].RegistrationMaxAttendees : false;
      }
      return {
        LimitedSpots,
        SpotsRemaining,
      };
    },
  },
};
</script>
<style scoped>
 .removeItem {
   position:absolute;
   width:100%;
   z-index:2;
 }
 .flex-Card {
   display:flex;
   flex-direction:column;
   justify-content:space-between;
    flex-grow:1;
 }
 .gradient {
   background: rgb(255,255,255);
   background: linear-gradient(0deg, rgba(255,255,255,.9) 46%, rgba(34,193,195,.8) 90%);
 }
 .titleFont {
   font-size:1.15rem;
   line-height:1.15rem;
   width:80%;
   /* font-size:30ch; */
    -webkit-hyphens: auto;
    -moz-hyphens: auto;
    hyphens: auto;
   
 }
 .campusBox {
    position:absolute; 
    right:0; 
    bottom:0; 
    background-color:rgba(0,0,0,.75); 
    color:white; 
    padding:5px 10px 5px 20px;
    
 }
</style>
