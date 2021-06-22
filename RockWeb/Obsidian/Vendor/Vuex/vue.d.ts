/**
 * Extends interfaces in Vue.js
 */

import { ComponentCustomOptions } from 'vue';
import { Store } from './Index';

declare module '@vue/runtime-core' {
  interface ComponentCustomOptions {
    store?: Store<any>;
  }
}
