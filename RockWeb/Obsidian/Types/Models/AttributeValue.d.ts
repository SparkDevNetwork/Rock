import { Guid } from '../../Util/Guid.js';
import Entity from './Entity.js';

export default interface AttributeValue extends Entity {
    AttributeAbbreviatedName: string;
    AttributeFieldTypeGuid: Guid;
    AttributeKey: string;
    AttributeName: string;
    Value: string;
    AttributeDescription: string;
}