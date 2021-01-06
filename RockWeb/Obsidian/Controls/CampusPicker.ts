import { CommonEntityOption, createCommonEntityPicker } from '../Store/Generators.js';
import store from '../Store/index.js';

export default createCommonEntityPicker(
    'Campus',
    () => store.getters['campuses/all'].map(c => ({
        Guid: c.Guid,
        Id: c.Id,
        Text: c.Name
    } as CommonEntityOption))
);
