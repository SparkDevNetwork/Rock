// ChannelRightPaneContext.tsx
import React, { createContext, useContext, useState, ReactNode } from 'react';

type ChannelPaneView = 'info' | 'threads' | 'search' | 'mentions' | 'members' | 'more' | null;

interface ChannelRightPaneContextType {
    activePane: ChannelPaneView;
    setActivePane: (view: ChannelPaneView) => void;
    closePane: () => void;
}

const ChannelRightPaneContext = createContext<ChannelRightPaneContextType | undefined>(undefined);

export const useChannelRightPane = () => {
    const ctx = useContext(ChannelRightPaneContext);
    if (!ctx) throw new Error("useChannelRightPane must be used within a ChannelRightPaneProvider");
    return ctx;
};

export const ChannelRightPaneProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
    const [activePane, setPane] = useState<ChannelPaneView>(null);

    const setActivePane = (view: ChannelPaneView) => {
        setPane(prev => (prev === view ? null : view));
    };

    const closePane = () => setPane(null);

    return (
        <ChannelRightPaneContext.Provider value={{ activePane: activePane, setActivePane, closePane }}>
            {children}
        </ChannelRightPaneContext.Provider>
    );
};
