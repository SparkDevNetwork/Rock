import { Guid } from '../../Util/Guid.js';
import Entity from './Entity.js';

export interface ConfigurationValue {
    Name: string;
    Description: string;
    Value: string;
}

export type ConfigurationValues = Record<string, ConfigurationValue>;

export default interface AttributeValue extends Entity {
    AttributeAbbreviatedName: string;
    AttributeFieldTypeGuid: Guid;
    AttributeKey: string;
    AttributeName: string;
    Value: string;
    AttributeDescription: string;
    AttributeIsRequired: boolean;
    AttributeQualifierValues: ConfigurationValues
}