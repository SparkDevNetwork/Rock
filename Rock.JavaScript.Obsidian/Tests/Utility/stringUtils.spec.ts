import { splitCase } from "@Obsidian/Utility/stringUtils";

describe("splitCase Function Tests", () => {
    it("should separate camelCase words into separate words", () => {
        expect(splitCase("MyCamelCaseString")).toBe("My Camel Case String");
    });

    it("should separate PascalCase words into separate words", () => {
        expect(splitCase("MyPascalCaseString")).toBe("My Pascal Case String");
    });
    it("should handle strings with consecutive uppercase letters correctly", () => {
        expect(splitCase("RESTKey")).toBe("REST Key");
    });

    it("should not add spaces before all uppercase sequences", () => {
        expect(splitCase("HTTPRequest")).toBe("HTTP Request");
    });

    it("should handle strings that start with lowercase letters", () => {
        expect(splitCase("xmlHttpRequest")).toBe("xml Http Request");
    });

    it("should return an empty string when provided an empty string", () => {
        expect(splitCase("")).toBe("");
    });

    it("should not change strings that are already separated", () => {
        expect(splitCase("Already Separated String")).toBe("Already Separated String");
    });

    it("should handle single-word strings correctly", () => {
        expect(splitCase("Word")).toBe("Word");
    });

    it("should not add a space at the beginning of the string", () => {
        expect(splitCase("CamelCaseString")).toBe("Camel Case String");
    });
});