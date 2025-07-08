import React, { createContext, useContext, useState, ReactNode } from 'react';
import type { UserResponse } from 'stream-chat';

/**
 * ChannelMemberListContextType
 *
 * Defines the shape of the context for managing the selected user in the channel member list.
 *
 * @property selectedUser - The currently selected user (or null if none is selected)
 * @property setSelectedUser - Function to update the selected user
 */
interface ChannelMemberListContextType {
    selectedUser: UserResponse | null;
    setSelectedUser: (user: UserResponse | null) => void;
}

/**
 * ChannelMemberListContext
 *
 * React context for sharing the selected user state and setter across the channel member list UI.
 * Initialized as undefined to enforce usage within a provider.
 */
const ChannelMemberListContext = createContext<ChannelMemberListContextType | undefined>(undefined);

/**
 * useChannelMemberListContext
 *
 * Custom hook to access the ChannelMemberListContext.
 * Throws an error if used outside of a ChannelMemberListProvider.
 *
 * @returns {ChannelMemberListContextType} The context value (selectedUser and setSelectedUser)
 * @throws {Error} If used outside of a ChannelMemberListProvider
 */
export const useChannelMemberListContext = () => {
    const context = useContext(ChannelMemberListContext);
    if (!context) {
        throw new Error('useChannelMemberListContext must be used within a ChannelMemberListProvider');
    }
    return context;
};

/**
 * ChannelMemberListProviderProps
 *
 * Props for the ChannelMemberListProvider component.
 * @property children - The React children to render within the provider
 */
interface ChannelMemberListProviderProps {
    children: ReactNode;
}

/**
 * ChannelMemberListProvider
 *
 * Provides the selected user state and setter to all descendants via context.
 * Wrap your component tree with this provider to enable member selection features.
 *
 * @param children - The React children to render within the provider
 * @returns {JSX.Element} The provider-wrapped children
 */
export const ChannelMemberListProvider: React.FC<ChannelMemberListProviderProps> = ({ children }) => {
    // State for the currently selected user (null if none selected)
    const [selectedUser, setSelectedUser] = useState<UserResponse | null>(null);

    return (
        <ChannelMemberListContext.Provider value={{ selectedUser, setSelectedUser: setSelectedUser }}>
            {children}
        </ChannelMemberListContext.Provider>
    );
};
