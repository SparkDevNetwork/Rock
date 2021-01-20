import { defineComponent, PropType } from '../Vendor/Vue/vue.js';
import AttributeValue from '../ViewModels/CodeGenerated/AttributeValueViewModel.js';
import Entity from '../ViewModels/Entity.js';
import AttributeValuesContainer from './AttributeValuesContainer';

export default defineComponent({
    name: 'EntityAttributeValuesContainer',
    components: {
        AttributeValuesContainer
    },
    props: {
        entity: {
            type: Object as PropType<Entity>,
            required: true
        },
        categoryName: {
            type: String as PropType<string>,
            default: ''
        }
    },
    computed: {
        attributeValues(): AttributeValue[] {
            const attributes = this.entity.Attributes || {};
            const attributeValues: AttributeValue[] = [];

            for (const key in attributes) {
                const attributeValue = attributes[key];
                const attribute = attributeValue.Attribute;

                if (this.categoryName && !attribute) {
                    continue;
                }

                if (this.categoryName && !attribute?.CategoryNames.includes(this.categoryName)) {
                    continue;
                }

                attributeValues.push(attributeValue);
            }

            attributeValues.sort((a, b) => (a.Attribute?.Order || 0) - (b.Attribute?.Order || 0));
            return attributeValues;
        }
    },
    template: `
<AttributeValuesContainer :attributeValues="attributeValues">`
});
