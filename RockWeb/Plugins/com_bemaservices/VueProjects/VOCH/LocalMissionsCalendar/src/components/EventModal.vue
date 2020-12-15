<template>
  <div id="Modal">
    <div class="card">
        <div class="top">
          <i class="fa fa-times fa-2x" @click="closeModal"></i>
        </div>
      <v-card
    class="mx-auto flex-Card"
    elevation="0"
    max-width="100%"
  >
  <div>
    <v-img
      class="white--text align-end gradient"
      height="275px"
      
      width="100%"
     :src="Event.EventPhoto ? Event.EventPhoto.Url : ''"
    >
      <v-card-title :color="Event.EventPhoto ? 'White' : 'Black'">{{Event.EventName}}</v-card-title>
    </v-img>
  
    <v-card-subtitle class="pb-0" style="display:flex; flex-direction:row; flex-wrap:nowrap; justify-content:space-between; align-items:top; flex-shirnk:1">

        <div style="flex-shrink:1;">
          {{ formatDate(Event.EventNextStartDate.StartDateTime) }} at {{ formatTime(Event.EventNextStartDate.StartDateTime) }}
          <div>{{ Event.OccurrenceLocation }}</div>
          <div v-if="calculateSpotsRemaining.LimitedSpots && calculateSpotsRemaining.SpotsRemaining > 0 ">Spots Remaining: {{ calculateSpotsRemaining.SpotsRemaining }}</div><br/>
        </div>
        <div style="flex-grow:1; align-content:flex-end;  break-inside: avoid;" class="text-right">
            {{Event.CampusName}}
        </div>

    </v-card-subtitle>
    </div>
  <div class="flex-Card">
    <v-card-text class="text--primary">
      <div>
         <div class="font-weight-bold">Description:</div>
        <div v-html="Event.EventDescription"></div>
        <div class="font-weight-bold">Upcoming Dates:</div>
        <ul>
          <li v-for="date in Event.UpcomingDates" :key="date">{{formatDate(date.StartDateTime) }} at {{ formatTime(date.StartDateTime) }} to {{formatDate(date.EndDateTime) != formatDate(date.StartDateTime) ? `${formatDate(date.EndDateTime)} at` : ''}} {{ formatTime(date.EndDateTime) }}</li>
        </ul>
        <div v-if="Event.RegistrationInformation.length > 0">
            <div class="font-weight-bold">Registration Information:</div>
            <ul>
              <li v-for="registration in Event.RegistrationInformation" :key="registration.RegistrationInstanceId">
                <span>{{registration.RegistrationPublicName}} - Register Between {{formatDate(registration.RegistrationStartDate) }} and {{formatDate(registration.RegistrationEndDate) }}</span><br />
                <span vif="!registrationSpotsRemaining(registration).LimitedSpots || registrationSpotsRemaining(registration).SpotsRemaining > 0">
                  <a :href="registration.RegistrationPublicSlug != '' ? `https://voxchurch.org/registration/${registration.RegistrationPublicSlug}` : `https://voxchurch.org/registration?RegistrationInstance=${registration.RegistrationInstanceId}`" class="btn btn-primary" style="color:white;">Register</a>
                </span>
              </li>
            </ul>
         </div>
      </div>
    </v-card-text>
    <v-card-actions>
      <v-btn
        color="orange"
        text
        @click="closeModal"
      >
        <span>Hide Details</span>
      </v-btn>

      <v-btn
        color="orange"
        text
        v-if="Event.RegistrationInformation.length > 0 && calculateSpotsRemaining.LimitedSpots && calculateSpotsRemaining.SpotsRemaining > 0"
      >
        Register
      </v-btn>
    </v-card-actions>
    </div>
  </v-card>
    </div>
  </div>
</template>

<script>
export default {
  props:{
    Event:{
      type:Object,
      required:true,
    }
  },
  methods:{
    closeModal(){
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
}
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
  z-index:20;
}
.card {
  background-color:white;
  width:90vw;
  overflow-y:scroll;
  height:90vh;
 
  /* left:50%;
  top:50%;
   right:50%;
  bottom:50%;
  position:absolute;
  transform:translate(-50%, -50%) */
}
.top {
  width:100%;
  background-color:teal;
  color:white;
  padding:1vh 2vw;
  display:flex;
  flex-direction:row-reverse;
  position: fixed;
    z-index: 101;
    top: 0;
    width: 90vw;

}


</style>
<style>
.modal-open {
  overflow: hidden;
  height: 100vh;
}
</style>