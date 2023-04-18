import { IDocVisitor, XmlDocMember } from "../../../src/Utility/RealTimeApiBrowser/xmlDoc.partial";

const testXml = `<member name="M:Math.Add(System.Double,System.Double)">
<summary>
          Adds <c>two</c> doubles <paramref name="a" /> and <paramref name="b" />
          and returns the result.
          </summary>
<returns>
          The sum of two doubles.
          </returns>
<example>
  <code>
          double c = Math.Add(4.5, 5.4);
          if (c &gt; 10)
          {
              Console.WriteLine(c);
          }
          </code>
</example>
<exception cref="T:System.OverflowException">
          Thrown when one parameter is max and the other
          is greater than 0.</exception>
          See <see cref="!:Math.Add(int, int)" /> to add integers.
          <seealso cref="!:Math.Subtract(double, double)" />
          <seealso cref="!:Math.Multiply(double, double)" />
          <seealso cref="!:Math.Divide(double, double)" />
          <param name="a">A double precision number.</param>
          <param name="b">A double precision number.</param>
</member>`;

describe("XmlDocMember", () => {
    it("getDoc find named node", () => {
        const member = new XmlDocMember(testXml);
        const summary = member.getDoc("summary");

        expect(summary).not.toBeNull();
        expect(summary.nodeName).toStrictEqual("summary");
    });

    it("getDoc find named node with attribute", () => {
        const member = new XmlDocMember(testXml);
        const paramB = member.getDoc("param", { name: "b" });

        expect(paramB).not.toBeNull();
        expect(paramB.nodeName).toStrictEqual("param");
        expect(paramB.attributes.name).toStrictEqual("b");
    });

    it("text with paramref parses correctly", () => {
        const member = new XmlDocMember(testXml);
        const summary = member.getDoc("summary");

        expect(summary).not.toBeNull();
        expect(summary.toString()).toStrictEqual("Adds two doubles a and b and returns the result.");
    });
});

describe("IDocVisitor", () => {
    it("summary does not descend when false returned by visitor", () => {
        const member = new XmlDocMember(testXml);
        const summary = member.getDoc("summary");
        const visitor: IDocVisitor = {
            visitSummary() {
                summaryVisited = true;
                return false;
            },
            visitText() {
                otherVisited = true;
            },
            visitParamRef() {
                otherVisited = true;
            }
        };
        let summaryVisited = false;
        let otherVisited = false;

        summary.accept(visitor);

        expect(summaryVisited).toStrictEqual(true);
        expect(otherVisited).toStrictEqual(false);
    });
});

describe("IDocVisitor", () => {
    it("summary does descend when true returned by visitor", () => {
        const member = new XmlDocMember(testXml);
        const summary = member.getDoc("summary");
        const visitor: IDocVisitor = {
            visitSummary() {
                summaryVisited = true;
                return true;
            },
            visitText() {
                otherVisited = true;
            },
            visitParamRef() {
                otherVisited = true;
            }
        };
        let summaryVisited = false;
        let otherVisited = false;

        summary.accept(visitor);

        expect(summaryVisited).toStrictEqual(true);
        expect(otherVisited).toStrictEqual(true);
    });

    it("HTML visitor works", () => {
        const member = new XmlDocMember(testXml);
        const summary = member.getDoc("summary");
        const visitor: IDocVisitor = {
            visitText(text) {
                segments.push(text.toString());
            },
            visitParamRef(paramRef) {
                segments.push(`<a href="#param_${paramRef.name}">${paramRef.toString()}</a>`);
            },
            visitInlineCode(inlineCode) {
                segments.push(`<b>${inlineCode.toString()}</b>`);
            }
        };
        const segments: string[] = [];

        summary.accept(visitor);
        const summaryText = segments.map(s => s.trim()).join(" ");

        expect(summaryText).toStrictEqual(`Adds <b>two</b> doubles <a href="#param_a">a</a> and <a href="#param_b">b</a> and returns the result.`);
    });
});
