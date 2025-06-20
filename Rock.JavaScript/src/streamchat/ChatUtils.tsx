import React from "react";
import { Channel } from "stream-chat";
import { ChatViewStyle } from "./ChatViewStyle";

/**
 * Returns a function for rendering channels grouped by pinned, shared, and DM.
 */
export function getRenderChannelsFn(
    chatViewStyle: ChatViewStyle,
    directMessageChannelTypeKey: string,
    sharedChannelTypeKey: string
) {
    return (
        channels: Channel[],
        channelPreview: (channel: Channel) => React.ReactNode
    ): React.ReactNode => {
        if (chatViewStyle === ChatViewStyle.Conversational) {
            return <>{channels.map(channel => channelPreview(channel))}</>;
        }

        // Get pinned channels using the hook
        const pinned: Channel[] = [];
        const notPinned: Channel[] = [];

        for (const channel of channels) {
            let isPinned = channel.state.membership?.pinned_at
            if (isPinned) {
                pinned.push(channel);
            } else {
                notPinned.push(channel);
            }
        }

        const sharedChannels = notPinned.filter(c => c.type != directMessageChannelTypeKey);
        const dmChannels = notPinned.filter(c => c.type == directMessageChannelTypeKey);

        console.log(channels)
        return (
            <div className="rock-channel-list-container">
                {pinned.length > 0 && (
                    <div className="rock-channel-group pinned-channels">
                        <h5 className="rock-channel-group-title">Favorites</h5>
                        <div className="rock-channel-group-items">
                            {pinned.map(channel => (
                                <div key={channel.cid}>
                                    {channelPreview(channel)}
                                </div>
                            ))}
                        </div>
                    </div>
                )}

                {sharedChannels.length > 0 && (
                    <div className="rock-channel-group shared-channels">
                        <h5 className="rock-channel-group-title">Channels</h5>
                        <div className="rock-channel-group-items">
                            {sharedChannels.map(channel => (
                                <div key={channel.cid}>
                                    {channelPreview(channel)}
                                </div>
                            ))}
                        </div>
                    </div>
                )}

                {dmChannels.length > 0 && (
                    <div className="rock-channel-group direct-messages">
                        <h5 className="rock-channel-group-title">Direct Messages</h5>
                        <div className="rock-channel-group-items">
                            {dmChannels.map(channel => (
                                <div key={channel.cid}>
                                    {channelPreview(channel)}
                                </div>
                            ))}
                        </div>
                    </div>
                )}
            </div>
        );
    };
}
