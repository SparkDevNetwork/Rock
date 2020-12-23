import { createCommonEntityPicker } from "../Store/controlGenerator.js";

export default createCommonEntityPicker(
    'Campus',
    store => store.getters['campuses/all'].map(c => ({
        key: c.Guid,
        value: c.Guid,
        text: c.Name
    }))
);
