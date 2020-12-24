import { createCommonEntityPicker } from "../Store/generators.js";
import store from '../Store/index.js';

export default createCommonEntityPicker(
    'Campus',
    () => store.getters['campuses/all'].map(c => ({
        key: c.Guid,
        value: c.Guid,
        text: c.Name
    }))
);
