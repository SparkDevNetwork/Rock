<template>
  
    <div id="eventCardList">
      <Observer @intersect="$emit('SetFocus')"/>
      
        <EventCard v-for="(event,index) in Events" :key="index" :Event="event" v-on="$listeners"/>
      
      <Observer @intersect="$emit('Next')"/>
    </div>
  
</template>

<script>
  import EventCard from './EventCard'
  import Observer from './Observer'

export default {

components: {
EventCard,
Observer
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
.list-item {
  display: inline-block;
  margin-right: 10px;
}
.list-enter-active, .list-leave-active {
  transition: all 1s;
}
.list-enter, .list-leave-to /* .list-leave-active below version 2.1.8 */ {
  opacity: 0;
  transform: translateY(30px);
}
#eventCardList{
  display:flex;
  flex-wrap:wrap;
  max-width:100%;
  flex-direction:row;
  position:relative;
  
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