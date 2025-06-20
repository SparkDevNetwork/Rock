// CreateChannelModal.tsx
import React, { useState, useEffect, useRef, ChangeEvent } from "react";
import { useChatContext, Avatar } from "stream-chat-react";
import { XIcon } from "../ChannelListHeader/Icons";
import { useChatConfig } from '../Chat/ChatConfigContext';
import { UserResponse } from "stream-chat";

export interface CreateChannelModalProps {
    /** Called when the user clicks outside or on the close (×) button */
    onClose: () => void;
}

/**
 * A centered modal for searching users and creating a direct-message channel.
 * Selected users appear inline as removable chips within the input area.
 * Submit button is only enabled when at least one user is selected.
 * Search results include the user's avatar.
 */
const CreateChannelModal: React.FC<CreateChannelModalProps> = ({ onClose }) => {
    const { client, setActiveChannel } = useChatContext();
    const chatConfig = useChatConfig();

    const [searchTerm, setSearchTerm] = useState("");
    const [userResults, setUserResults] = useState<UserResponse[]>([]);
    const [selectedUsers, setSelectedUsers] = useState<UserResponse[]>([]);
    const inputRef = useRef<HTMLInputElement>(null);

    // Fetch matching users, excluding already selected
    useEffect(() => {
        if (!searchTerm) {
            setUserResults([]);
            return;
        }
        const fetchUsers = async () => {
            try {
                const { users } = await client.queryUsers(
                    { name: { $autocomplete: searchTerm }, rock_profile_public: true },
                    { id: 1, name: 1, image: 1 },
                    { limit: 10 }
                );

                // Filter out already selected users & the current user
                const currentUserId = client.userID;
                const filteredUsers = users.filter(u =>
                    u.id !== currentUserId && !selectedUsers.some(sel => sel.id === u.id)
                );

                setUserResults(filteredUsers);
            } catch (err) {
                console.error("Error fetching users:", err);
            }
        };
        fetchUsers();
    }, [searchTerm, client, selectedUsers]);

    const handleSearchChange = (e: ChangeEvent<HTMLInputElement>) => {
        setSearchTerm(e.target.value);
    };

    const handleSelectUser = (user: UserResponse) => {
        setSelectedUsers(prev => [...prev, user]);
        setSearchTerm("");
        setUserResults([]);
        inputRef.current?.focus();
    };

    const handleRemoveUser = (id: string) => {
        setSelectedUsers(prev => prev.filter(u => u.id !== id));
        inputRef.current?.focus();
    };

    const handleCreateChannel = async () => {
        if (!chatConfig?.directMessageChannelTypeKey || selectedUsers.length === 0) {
            return;
        }
        const userIds = selectedUsers.map(u => u.id);
        const members = [...userIds, client.userID!];

        const channel = client.channel(
            chatConfig.directMessageChannelTypeKey,
            null,
            { members }
        );
        await channel.create();
        onClose();
        setActiveChannel(channel);
    };

    const isSubmitDisabled = selectedUsers.length === 0;

    return (
        <div className="create-channel-modal-overlay str-chat" onClick={onClose}>
            <div className="create-channel-modal-content" onClick={e => e.stopPropagation()}>
                <header className="create-channel-modal-header">
                    <h2 className="create-channel-modal-header-title">New Direct Message</h2>
                    <button
                        className="create-channel-modal-close"
                        onClick={onClose}
                        aria-label="Close dialog"
                    >
                        <XIcon className="create-channel-modal-close-icon" />
                    </button>
                </header>

                <section className="create-channel-modal-body">
                    <label htmlFor="user-search">Select people to message</label>

                    <div className="create-channel-modal-input-container">
                        {selectedUsers.map(user => (
                            <div key={user.id} className="create-channel-modal-chip">
                                <span className="create-channel-modal-chip-text">
                                    {user.name || user.id}
                                </span>
                                <button
                                    type="button"
                                    className="create-channel-modal-chip-remove"
                                    onClick={() => handleRemoveUser(user.id)}
                                    aria-label={`Remove ${user.name || user.id}`}
                                >
                                    ×
                                </button>
                            </div>
                        ))}

                        <input
                            id="user-search"
                            ref={inputRef}
                            type="text"
                            value={searchTerm}
                            onChange={handleSearchChange}
                            placeholder={selectedUsers.length > 0 ? "" : "Search users..."}
                            autoComplete="off"
                            className="create-channel-modal-input"
                        />
                    </div>

                    {userResults.length > 0 && (
                        <ul className="create-channel-modal-search-results">
                            {userResults.map(u => (
                                <li
                                    key={u.id}
                                    className="create-channel-modal-search-item"
                                    onClick={() => handleSelectUser(u)}>
                                    <Avatar
                                        user={u}
                                        className="create-channel-modal-search-avatar"
                                        image={u.image}
                                        name={u.name || u.id}
                                    />
                                    <span className="create-channel-modal-search-text">
                                        {u.name || u.id}
                                    </span>
                                </li>
                            ))}
                        </ul>
                    )}
                </section>

                <footer className="create-channel-modal-footer">
                    <button onClick={onClose}>Cancel</button>
                    <button
                        className="create-channel-modal-create"
                        onClick={handleCreateChannel}
                        disabled={isSubmitDisabled}
                    >
                        Create
                    </button>
                </footer>
            </div>
        </div>
    );
};

export default CreateChannelModal;
