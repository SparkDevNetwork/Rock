import Vue from 'vue';
import App from './App.vue';
import vuetify from './plugins/vuetify';

const calendar = 4;
Vue.config.productionTip = false;

new Vue({
  vuetify,
  render: (h) => h(App),
}).$mount('#app');
