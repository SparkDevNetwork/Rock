import { toCurrencyOrNull } from "@Obsidian/Utility/numberUtils";

describe("Currency Test", () => {
    it("should display the currency symbol whens provided", () => {
        expect(toCurrencyOrNull("5", {symbol: "£"})).toBe("£5.00");
    });

    it("should default to $ when no currency symbol is provided", () => {
        expect(toCurrencyOrNull("5")).toBe("$5.00");
    });

    it("should display the currency amount to the required decimal places when provided", () => {
        expect(toCurrencyOrNull("5", {decimalPlaces: 4})).toBe("$5.0000");
    });
});