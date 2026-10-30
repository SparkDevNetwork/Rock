import assert = require("assert");
import axios from "axios";
import { postForBlob } from "../../Framework/Utility/http";

jest.mock("axios", () => {
    const axiosMock = jest.fn();
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (axiosMock as any).isAxiosError = (e: { isAxiosError?: boolean }) => e?.isAxiosError === true;

    return {
        __esModule: true,
        default: axiosMock
    };
});

const axiosMock = axios as unknown as jest.Mock;

/**
 * Sets up the axios mock to return a successful response with the given
 * content-disposition header.
 */
function mockSuccess(contentDisposition: string | undefined): Blob {
    const data = new Blob(["file content"]);

    axiosMock.mockResolvedValueOnce({
        data,
        status: 200,
        headers: contentDisposition === undefined ? {} : { "content-disposition": contentDisposition }
    });

    return data;
}

/**
 * Sets up the axios mock to fail with the given status and body text.
 */
function mockError(status: number, bodyText: string): void {
    axiosMock.mockRejectedValueOnce({
        isAxiosError: true,
        response: {
            status,
            data: { text: async () => bodyText }
        }
    });
}

describe("postForBlob Suite", () => {
    beforeEach(() => {
        axiosMock.mockReset();
    });

    it("Sends a POST that expects binary data", async () => {
        mockSuccess(undefined);

        await postForBlob("/api/v2/BlockActions/page/block/DownloadLogs", { key: "1" });

        const config = axiosMock.mock.calls[0][0];
        assert.strictEqual(config.method, "POST");
        assert.strictEqual(config.url, "/api/v2/BlockActions/page/block/DownloadLogs");
        assert.strictEqual(config.responseType, "blob");
        assert.deepStrictEqual(config.data, { key: "1" });
    });

    it("Sends an empty object when there is no data", async () => {
        mockSuccess(undefined);

        await postForBlob("/url");

        assert.deepStrictEqual(axiosMock.mock.calls[0][0].data, {});
    });

    it("Returns the data and file name on success", async () => {
        const data = mockSuccess("attachment; filename=RockLogs.zip");

        const result = await postForBlob("/url");

        assert.strictEqual(result.isSuccess, true);
        assert.strictEqual(result.statusCode, 200);
        assert.strictEqual(result.data, data);
        assert.strictEqual(result.fileName, "RockLogs.zip");
    });

    it("Reads the file name from the content-disposition header", async () => {
        const headers: [string | undefined, string | null][] = [
            ["attachment; filename=RockLogs.zip", "RockLogs.zip"],
            ["attachment; filename=\"Rock Logs.zip\"", "Rock Logs.zip"],
            ["attachment; filename*=UTF-8''Rock%20Logs%C3%A9.zip", "Rock Logsé.zip"],
            ["attachment; filename=fallback.zip; filename*=UTF-8''preferred.zip", "preferred.zip"],
            ["attachment; filename=\"=?utf-8?B?Um9ja0xvZ3MuemlwIA==?=\"", null],
            ["attachment", null],
            [undefined, null]
        ];

        for (const [header, expected] of headers) {
            mockSuccess(header);

            const result = await postForBlob("/url");

            assert.strictEqual(result.fileName, expected, String(header));
        }
    });

    it("Returns the server error message on failure", async () => {
        mockError(400, "{\"Message\":\"Unable to read the logs.\"}");

        const result = await postForBlob("/url");

        assert.strictEqual(result.isSuccess, false);
        assert.strictEqual(result.isError, true);
        assert.strictEqual(result.statusCode, 400);
        assert.strictEqual(result.errorMessage, "Unable to read the logs.");
        assert.strictEqual(result.data, null);
        assert.strictEqual(result.fileName, null);
    });

    it("Returns no error message when the error body is not JSON", async () => {
        mockError(405, "Method Not Allowed");

        const result = await postForBlob("/url");

        assert.strictEqual(result.isSuccess, false);
        assert.strictEqual(result.statusCode, 405);
        assert.strictEqual(result.errorMessage, null);
    });

    it("Returns a status code of 0 when there is no response", async () => {
        axiosMock.mockRejectedValueOnce(new Error("Network Error"));

        const result = await postForBlob("/url");

        assert.strictEqual(result.isSuccess, false);
        assert.strictEqual(result.statusCode, 0);
        assert.strictEqual(result.errorMessage, null);
    });
});
