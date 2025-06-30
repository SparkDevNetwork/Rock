// ChannelListControllerContext.tsx
import { createContext, useContext } from 'react';

interface ChannelListController {
    refresh: () => void;
}

export const ChannelListControllerContext = createContext<ChannelListController | null>(null);

export const useChannelListController = (): ChannelListController => {
    const ctx = useContext(ChannelListControllerContext);
    if (!ctx) {
        throw new Error('useChannelListController must be used within a ChannelListControllerProvider');
    }
    return ctx;
};
