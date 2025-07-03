import React, { createContext, useContext, useState, ReactNode } from 'react';
import type { ChannelMemberResponse } from 'stream-chat';

interface ChannelMemberListContextType {
    selectedMember: ChannelMemberResponse | null;
    setSelectedMember: (member: ChannelMemberResponse | null) => void;
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
    const [selectedMember, setSelectedMember] = useState<ChannelMemberResponse | null>(null);

    return (
        <ChannelMemberListContext.Provider value={{ selectedMember, setSelectedMember }}>
            {children}
        </ChannelMemberListContext.Provider>
    );
};
