<template>
  <div id="Modal"  @click.self="closeModal">
    <div class="card">
        <div class="top">
          <i class="fal fa-times fa-2x" style="font-weight:normal." @click.stop="closeModal"></i>
        </div>
 <v-card
    class="mx-auto flex-Card"
    height="100%"
    max-width="100%"
    rounded
    elevation="0"
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
         <div class="font-weight-bold">Description:</div>
        <div v-html="Event.EventDescription"></div>
        <div class="font-weight-bold">Upcoming Dates:</div>
        <ul>
          <li v-for="date in Event.UpcomingDates" :key="date.StartDateTime.toString()">{{formatDate(date.StartDateTime) }} at {{ formatTime(date.StartDateTime) }} to {{formatDate(date.EndDateTime) != formatDate(date.StartDateTime) ? `${formatDate(date.EndDateTime)} at` : ''}} {{ formatTime(date.EndDateTime) }}</li>
        </ul>
        <div v-if="Event.RegistrationInformation.length > 0">
            <div class="font-weight-bold">Registration Information:</div>
            <ul>
              <li v-for="registration in Event.RegistrationInformation" :key="registration.RegistrationInstanceId">
                <span>{{registration.RegistrationPublicName}} - Register Between {{formatDate(registration.RegistrationStartDate) }} and {{formatDate(registration.RegistrationEndDate) }}</span><br />
                <span vif="!registrationSpotsRemaining(registration).LimitedSpots || registrationSpotsRemaining(registration).SpotsRemaining > 0">
                  <a :href="registration.RegistrationPublicSlug != '' ? `https://voxchurch.org/registration/${registration.RegistrationPublicSlug}` : `https://voxchurch.org/registration?RegistrationInstanceId=${registration.RegistrationInstanceId}`" class="btn btn-primary" style="color:white;">Register</a>
                </span>
              </li>
            </ul>
         </div>
      </div>
    </v-card-text>
    <v-card-actions>
      <v-btn
        color="#34aeb7"
        text
        @click="closeModal"
      >
        <span>Hide Details</span>
      </v-btn>

    </v-card-actions>
    </div>
  </v-card>
    </div>
  </div>
</template>

<script>
import Eventcard from './EventCard';

export default {
  components: {
    Eventcard,
  },
  props: {
    Event: {
      type: Object,
      required: true,
    },
  },
  data: () => ({
    showTitle: false,
  }),
  methods: {
    closeModal() {
      this.$emit('CloseModal');
    },
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

      if (this.Event.RegistrationInformation[0]) {
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
#Modal{
  background-color:rgba(0,0,0,.8);
  position: fixed;
  display:flex;
  flex-direction:column;
  justify-content:space-around;
  align-items:center;
  top:0;
  left:0;
  bottom:0;
  right:0;
  z-index:200;
}
#Modal .card {
  background-color:white;
  width: clamp(400px, 90vw, 1000px);
  overflow-y:scroll;
  min-height:90vh;
  max-height: 95vh;
  position:relative;
  padding: 0;

  /* left:50%;
  top:50%;
   right:50%;
  bottom:50%;
  position:absolute;
  transform:translate(-50%, -50%) */
}
.top {

  background-color:#34aeb7;
  color:white;
  padding:1vh 1vw;
  display:flex;
  flex-direction:row-reverse;
  align-items:center;
  position: -webkit-sticky; /* Safari */
  position: sticky;
box-shadow: 0 5px 3px rgba(0,0,0,.25);
  
    z-index: 101;
    top: 0;
    right:50%;
    left:0;
    width:100%;
    height:45px;

}
.top i {
  cursor:pointer;

}
.gradient {
   background-color:black;
 }
 .titleFont {
   font-size:clamp(1.5rem, 2vw, 3rem);
   /* padding-left:0; */
 }
 .gradient {
   background: rgb(255,255,255);
   background: linear-gradient(0deg, rgba(255,255,255,.9) 46%, rgba(52, 174, 183, 1)90%);
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
