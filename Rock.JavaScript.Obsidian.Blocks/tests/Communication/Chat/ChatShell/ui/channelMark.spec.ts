// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
// A channel's mark, mounted. A channel with its own image shows that image, decorative since the
// name is always beside it; a channel without one, or whose image fails to load, shows the hash
// when it is public and the lock when it is private. The box is the caller's, so the mark never
// changes size between the two.
import { mount } from "@vue/test-utils";
import ChannelMark from "../../../../../src/Communication/Chat/ChatShell/ui/channelMark.partial.obs";
import { avatarHue } from "../../../../../src/Communication/Chat/ChatShell/ui/brand.partial";
import Icon from "../../../../../src/Communication/Chat/ChatShell/ui/icon.partial.obs";

const picture = "https://church.example/GetImage.ashx?guid=team";

describe("a channel's mark", () => {
    test("shows the channel's own image, with no alt text and hidden from screen readers", () => {
        const w = mount(ChannelMark, { props: { isPublic: true, imageUrl: picture } });

        const image = w.get("[data-testid='channel-mark-image']");
        expect(image.attributes("src")).toBe(picture);
        expect(image.attributes("alt")).toBe("");
        expect(w.attributes("aria-hidden")).toBe("true");
        expect(w.findAllComponents(Icon)).toHaveLength(0);
    });

    test.each([[true, "hash"], [false, "lock"]])("with no image, a channel that is public %s shows its %s", (isPublic, name) => {
        const w = mount(ChannelMark, { props: { isPublic, imageUrl: null } });

        expect(w.find("[data-testid='channel-mark-image']").exists()).toBe(false);
        expect(w.findAllComponents(Icon).map(icon => icon.props("name"))).toEqual([name]);
    });

    test("falls back to the hash or lock when the image fails to load, and tries a new image again", async () => {
        const w = mount(ChannelMark, { props: { isPublic: false, imageUrl: "https://church.example/missing.png" } });

        await w.get("[data-testid='channel-mark-image']").trigger("error");
        expect(w.find("[data-testid='channel-mark-image']").exists()).toBe(false);
        expect(w.findAllComponents(Icon).map(icon => icon.props("name"))).toEqual(["lock"]);

        await w.setProps({ imageUrl: picture });
        expect(w.get("[data-testid='channel-mark-image']").attributes("src")).toBe(picture);
    });

    test("draws the glyph at the size it is given", () => {
        const w = mount(ChannelMark, { props: { isPublic: true, imageUrl: null, iconSize: "sm" } });

        expect(w.getComponent(Icon).props("size")).toBe("sm");
    });

    test("keeps the caller's box class on its root, so the box's size is the caller's", () => {
        const w = mount(ChannelMark, { props: { isPublic: true, imageUrl: picture }, attrs: { class: "chat-header-ic" } });

        expect(w.classes()).toContain("chat-header-ic");
        expect(w.classes()).toContain("has-image");
    });

    // A tile is how the sidebar draws a channel: its image, else its first letter on its name's hue.
    // Off unless asked for, so the header, the intro and the details keep the hash and the lock.
    test("draws no letter unless given a tile name", () => {
        const w = mount(ChannelMark, { props: { isPublic: true, imageUrl: null } });

        expect(w.find("[data-testid='channel-mark-letter']").exists()).toBe(false);
        expect(w.classes()).not.toContain("is-letter");
    });

    test("with a tile name and no image, draws its first letter, upper case and whole, on its hue", () => {
        const w = mount(ChannelMark, { props: { isPublic: false, imageUrl: null, tileName: "  \u{1F600}smile team" } });

        expect(w.get("[data-testid='channel-mark-letter']").text()).toBe("\u{1F600}");
        expect(w.findAllComponents(Icon)).toHaveLength(0);
        expect(w.classes()).toContain("is-letter");
        expect((w.element as HTMLElement).style.getPropertyValue("--chat-av-hue")).toBe(String(avatarHue("  \u{1F600}smile team")));

        const plain = mount(ChannelMark, { props: { isPublic: true, imageUrl: null, tileName: "worship" } });
        expect(plain.get("[data-testid='channel-mark-letter']").text()).toBe("W");
    });

    test("with a tile name and an image, shows the image, and the letter once it fails", async () => {
        const w = mount(ChannelMark, { props: { isPublic: true, imageUrl: picture, tileName: "Greeters" } });

        expect(w.find("[data-testid='channel-mark-letter']").exists()).toBe(false);
        await w.get("[data-testid='channel-mark-image']").trigger("error");
        expect(w.get("[data-testid='channel-mark-letter']").text()).toBe("G");
    });
});
