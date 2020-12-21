<template>
  <v-row class="fill-height">
    <v-col>
      <v-sheet height="64">
        <v-toolbar
          flat
        >
          <v-btn
            outlined
            class="mr-4"
            color="grey darken-2"
            @click="setToday"
          >
            Today
          </v-btn>
         <v-spacer></v-spacer>
            <h4>{{focus.toLocaleString('en-us', { month: 'long' })}} {{focus.getFullYear()}}</h4>
          <v-spacer></v-spacer>
          <v-menu
            bottom
            right
          >
            <template v-slot:activator="{ on, attrs }">
              <v-btn
                outlined
                color="grey darken-2"
                v-bind="attrs"
                v-on="on"
              >
                <span>{{ typeToLabel[type] }}</span>
                <v-icon right>
                  mdi-menu-down
                </v-icon>
              </v-btn>
            </template>
            <v-list>
              <v-list-item @click="changeType('day')">
                <v-list-item-title>Day</v-list-item-title>
              </v-list-item>
              <v-list-item @click="changeType('week')">
                <v-list-item-title>Week</v-list-item-title>
              </v-list-item>
              <v-list-item @click="changeType('month')">
                <v-list-item-title>Month</v-list-item-title>
              </v-list-item>
            </v-list>
          </v-menu>
        </v-toolbar>
      </v-sheet>
      <v-sheet height="600">
        <v-calendar
          ref="calendar"
          v-model="focus"
          color="primary"
          :events="processEvents"
          :event-color="getEventColor"
          :type="type"
          @click:event="showEvent"
          @click:more="viewDay"
          @click:date="viewDay"
          
        ></v-calendar>
        <v-menu
          v-model="selectedOpen"
          :close-on-content-click="false"
          :activator="selectedElement"
          offset-x
        >
          <v-card
            color="grey lighten-4"
            min-width="350px"
            flat
          >
            <v-toolbar
              :color="selectedEvent.color"
              dark
            >
              <v-btn icon>
                <v-icon>mdi-pencil</v-icon>
              </v-btn>
              <v-toolbar-title v-html="selectedEvent.name"></v-toolbar-title>
              <v-spacer></v-spacer>
              <v-btn icon>
                <v-icon>mdi-heart</v-icon>
              </v-btn>
              <v-btn icon>
                <v-icon>mdi-dots-vertical</v-icon>
              </v-btn>
            </v-toolbar>
            <v-card-text>
              <span v-html="selectedEvent.details"></span>
            </v-card-text>
            <v-card-actions>
              <v-btn
                text
                color="secondary"
                @click="selectedOpen = false"
              >
                Cancel
              </v-btn>
            </v-card-actions>
          </v-card>
        </v-menu>
      </v-sheet>
    </v-col>
  </v-row>
</template>
<script>
export default {
    props: {
      Events: {
        type:Array,
        required:true,
      },
      focus: {
        type: Date
      },
      type:{
        type: String
      }
    },
    data: () => ({
      typeToLabel: {
        month: 'Month',
        week: 'Week',
        day: 'Day',
      },
      selectedEvent: {},
      selectedElement: null,
      selectedOpen: false,
      colors: ['blue', 'indigo', 'deep-purple', 'cyan', 'green', 'orange', 'grey darken-1'],
    }),
    watch:{
      type: function(val) {
        this.$emit('CalendarType',{type: val})
      }
    },
    mounted () {
      this.$refs.calendar.checkChange()
    },
    methods: {
      changeType(val){
        this.$emit('CalendarType',{type: val})
      },
      viewDay ({ date }) {
        let newDate = date + 'T00:00:00.0000'
        this.$emit('setFocus',newDate)
        this.$emit('CalendarType',{type: 'day'})
      },
      getEventColor (event) {
        return event.color
      },
      setToday () {
        this.$emit('setFocus',null)
      },
      
      showEvent ({ nativeEvent, event }) {
        this.$emit('ShowModal',event.Event)
       

       
        nativeEvent.stopPropagation()
      },
      // updateRange ({ start, end }) {
      //   const events = []

      //   const min = new Date(`${start.date}T00:00:00`)
      //   const max = new Date(`${end.date}T23:59:59`)
      //   const days = (max.getTime() - min.getTime()) / 86400000
      //   const eventCount = this.rnd(days, days + 20)

      //   for (let i = 0; i < eventCount; i++) {
      //     const allDay = this.rnd(0, 3) === 0
      //     const firstTimestamp = this.rnd(min.getTime(), max.getTime())
      //     const first = new Date(firstTimestamp - (firstTimestamp % 900000))
      //     const secondTimestamp = this.rnd(2, allDay ? 288 : 8) * 900000
      //     const second = new Date(first.getTime() + secondTimestamp)

      //     events.push({
      //       name: this.names[this.rnd(0, this.names.length - 1)],
      //       start: first,
      //       end: second,
      //       color: this.colors[this.rnd(0, this.colors.length - 1)],
      //       timed: !allDay,
      //     })
      //   }

      //   this.events = events
      // },
      rnd (a, b) {
        return Math.floor((b - a + 1) * Math.random()) + a
      },
    },
     computed: {
      processEvents(){
        let Events = [];
        this.Events.forEach((e,index) => {
          let newEvent = {};
          newEvent.name = e.EventName;
          newEvent.start = e.EventNextStartDate.StartDateTime;
          newEvent.end = e.EventNextStartDate.EndDateTime;
          newEvent.color = this.colors[index % this.colors.length]
          newEvent.Event = e;
          Events.push(newEvent)
        })
        return Events;
      },
    },
  }
    
</script>
