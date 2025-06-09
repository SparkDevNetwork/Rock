import React from "react";
import { CirclePlusIcon, EditIcon } from "./Icons";
export interface ChannelListHeaderProps {
    /**
     * Callback fired when the "New DM" button is clicked
     */
    onNewMessage: () => void;
}

/**
 * A small header above the channel list with a right-aligned "New DM" icon button.
 */
const ChannelListHeader: React.FC<ChannelListHeaderProps> = ({ onNewMessage }) => (
    <div className="rock__channel-list-header">
        <button
            onClick={onNewMessage}
            aria-label="New DM"
        >
            <EditIcon
                className="rock__channel-list-header__new-message-icon"
            />
        </button>
    </div>
);

export default ChannelListHeader;
