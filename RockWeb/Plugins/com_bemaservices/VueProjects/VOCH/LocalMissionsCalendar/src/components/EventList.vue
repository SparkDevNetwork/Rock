<template>
  
    <div id="eventCardList">
      <EventCard v-for="(event,index) in Events" :key="index" :Event="event" v-on="$listeners" />
    </div>
  
</template>

<script>
import EventCard from './EventCard'

export default {

components: {
EventCard,
},

props:{
  Events: {
    type: Array, 
    required: true,
  }
}  
}
</script>

<style >

#eventCardList{
  display:flex;
  flex-wrap:wrap;
  max-width:100%;
  flex-direction:row;
  position:relative;
  align-content:stretch;
  /* gap:20px;
  grid-auto-flow: row; */
}
/* 
@media (min-width: 768px) {
  #eventCardList {
    grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));

  }
 } */

</style>