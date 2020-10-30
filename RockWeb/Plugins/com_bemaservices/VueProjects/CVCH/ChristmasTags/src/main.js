import Vue from 'vue'
import App from './App.vue'
import vuetify from './plugins/vuetify';
import { EventBus } from './modules/event-bus.js';

// Event listener for warranty transaction success
window.addEventListener('message', function (e) {
  if (e.data.event === 'transactioncomplete') {
      EventBus.$emit('transactionComplete', e.data.data);
  }
});

let firstName = '{{CurrentPerson.NickName}}';
let lastName = 'CurrentPerson.LastName';
let email = '{{CurrentPerson.Email}}';


Vue.config.productionTip = true

new Vue({
  vuetify,
  render: h => h(App)
}).$mount('#app')
