import { blobToBase64 } from "./utils.partial";

/**
 * Convert the image data to a black and white representation. The original
 * data string should be a base-64 encoded string of the raw data.
 *
 * @param original The base-64 encoded string that contains the original image data.
 * @param brightness The brightness amount to apply.
 *
 * @returns A new string that is a base-64 representation of the new image.
 */
export async function convertImageDataToBlackAndWhite(original: string, brightness: number): Promise<string | undefined> {
    const image = document.createElement("img");
    const canvas = document.createElement("canvas");
    const ctx = canvas.getContext("2d");

    if (!ctx) {
        throw new Error("Could not process image.");
    }

    // Load the image and wait for it to complete.
    const imageLoaded = new Promise<void>((resolve, reject) => {
        image.addEventListener("load", () => resolve());
        image.addEventListener("error", () => reject("Unable to load image data."));
    });

    image.src = `data:image/png;base64,${original}`;
    await imageLoaded;

    // Configure the size of the canvas to match the image and then draw
    // the image onto the full canvas.
    canvas.width = image.naturalWidth;
    canvas.height = image.naturalHeight;
    ctx.drawImage(image, 0, 0);

    // Get the raw RGBA canvas data.
    const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);

    applyImageFilter(imageData.data, createGrayscaleBt601Matrix(1));

    if (brightness !== 1) {
        applyImageFilter(imageData.data, createBrightnessMatrix(brightness));
    }

    // Convert the image data to be black and transparent.
    for (let i = 0; i < imageData.data.length; i += 4) {
        if (imageData.data[i] < 128 && imageData.data[i + 3] >= 128) {
            imageData.data[i] = imageData.data[i + 1] = imageData.data[i + 2] = 0;
            imageData.data[i + 3] = 255;
        }
        else {
            imageData.data[i + 3] = 0;
        }
    }

    // Replace the contents of the canvas with the modified image data.
    ctx.putImageData(imageData, 0, 0, 0, 0, canvas.width, canvas.height);

    // Convert the canvas to a PNG image blob.
    const blob = await new Promise<Blob>((resolve, reject) => {
        canvas.toBlob(b => {
            if (b) {
                resolve(b);
            }
            else {
                reject(new Error("Unable to process image."));
            }
        }, "image/png");
    });

    return blobToBase64(blob);
}

/**
 * Simple class to make it easier to work with a 5x4 color matrix.
 */
class ColorMatrix {
    public m11: number = 0;
    public m12: number = 0;
    public m13: number = 0;
    public m14: number = 0;

    public m21: number = 0;
    public m22: number = 0;
    public m23: number = 0;
    public m24: number = 0;

    public m31: number = 0;
    public m32: number = 0;
    public m33: number = 0;
    public m34: number = 0;

    public m41: number = 0;
    public m42: number = 0;
    public m43: number = 0;
    public m44: number = 0;

    public m51: number = 0;
    public m52: number = 0;
    public m53: number = 0;
    public m54: number = 0;
}

/**
 * Creates a color matrix to apply a grayscale effect.
 *
 * @param amount The amount of grayscale adjustment to apply between 0 and 1.
 *
 * @returns The color matrix that will apply the effect.
 */
function createGrayscaleBt601Matrix(amount: number): ColorMatrix {
    amount = 1 - amount;

    const m = new ColorMatrix();

    m.m11 = 0.299 + (0.701 * amount);
    m.m21 = 0.587 - (0.587 * amount);
    m.m31 = 1 - (m.m11 + m.m21);

    m.m12 = 0.299 - (0.299 * amount);
    m.m22 = 0.587 + (0.2848 * amount);
    m.m32 = 1 - (m.m12 + m.m22);

    m.m13 = 0.299 - (0.299 * amount);
    m.m23 = 0.587 - (0.587 * amount);
    m.m33 = 1 - (m.m13 + m.m23);

    m.m44 = 1;

    return m;
}

/**
 * Creates a color matrix to apply a brightness effect.
 *
 * @param amount The amount of brightness adjustment to apply, greater than or equal to 0.
 *
 * @returns The color matrix that will apply the effect.
 */
function createBrightnessMatrix(amount: number): ColorMatrix {
    const m = new ColorMatrix();

    m.m11 = amount;
    m.m22 = amount;
    m.m33 = amount;
    m.m44 = 1;

    return m;
}

/**
 * Applies a color multiplication matrix filter to the image data.
 *
 * @param data The raw bitmap data in RGBA format.
 *
 * @param matrix The multiplication matrix.
 */
function applyImageFilter(data: Uint8ClampedArray, matrix: ColorMatrix): void {
    for (let i = 0; i < data.length; i += 4) {
        let x = data[i] / 255;
        let y = data[i + 1] / 255;
        let z = data[i + 2] / 255;
        let w = data[i + 3] / 255;

        x = (x * matrix.m11) + (y * matrix.m21) + (z * matrix.m31) + (w * matrix.m41) + matrix.m51;
        y = (x * matrix.m12) + (y * matrix.m22) + (z * matrix.m32) + (w * matrix.m42) + matrix.m52;
        z = (x * matrix.m13) + (y * matrix.m23) + (z * matrix.m33) + (w * matrix.m43) + matrix.m53;
        w = (x * matrix.m14) + (y * matrix.m24) + (z * matrix.m34) + (w * matrix.m44) + matrix.m54;

        data[i] = Math.min(255, Math.max(0, Math.round(x * 255)));
        data[i + 1] = Math.min(255, Math.max(0, Math.round(y * 255)));
        data[i + 2] = Math.min(255, Math.max(0, Math.round(z * 255)));
        data[i + 3] = Math.min(255, Math.max(0, Math.round(w * 255)));
    }
}
