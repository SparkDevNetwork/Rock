import { Guid } from "../Util/Guid";
import AttributeValue from "./CodeGenerated/AttributeValueViewModel";

export default interface Entity {
    Id: number;
    Guid: Guid;
    Attributes: Record<string, AttributeValue> | null
}