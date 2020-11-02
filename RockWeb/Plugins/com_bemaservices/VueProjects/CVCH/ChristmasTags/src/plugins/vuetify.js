import Vue from 'vue';
import Vuetify from 'vuetify/lib';

Vue.use(Vuetify);

export default new Vuetify({

    

        theme: {
          themes: {
            light: {
              
              primary: '#143A29',
              secondary:'#D81E39',
              accent: '#961B1F',
            },

          },
          options: {
            customProperties: true
          },
          
        },
      
});
