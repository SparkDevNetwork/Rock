import React, { createContext, useContext, useState, ReactNode } from 'react';
import type { ChannelMemberResponse, UserResponse } from 'stream-chat';

interface ChannelMemberListContextType {
    selectedUser: UserResponse | null;
    setSelectedUser: (user: UserResponse | null) => void;
}

const ChannelMemberListContext = createContext<ChannelMemberListContextType | undefined>(undefined);

export const useChannelMemberListContext = () => {
    const context = useContext(ChannelMemberListContext);
    if (!context) {
        throw new Error('useChannelMemberListContext must be used within a ChannelMemberListProvider');
    }
    return context;
};

interface ChannelMemberListProviderProps {
    children: ReactNode;
}

export const ChannelMemberListProvider: React.FC<ChannelMemberListProviderProps> = ({ children }) => {
    const [selectedUser, setSelectedUser] = useState<UserResponse | null>(null);

    return (
        <ChannelMemberListContext.Provider value={{ selectedUser, setSelectedUser: setSelectedUser }}>
            {children}
        </ChannelMemberListContext.Provider>
    );
};
