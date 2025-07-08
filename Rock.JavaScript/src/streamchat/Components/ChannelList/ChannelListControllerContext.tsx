// ChannelListControllerContext.tsx
import { createContext, useContext } from 'react';

/**
 * Interface for the ChannelListController context value.
 *
 * @property {() => void} refresh - Function to refresh/reload the channel list UI. Should be implemented by the provider.
 */
interface ChannelListController {
    refresh: () => void;
}

/**
 * React Context for ChannelListController.
 *
 * Provides access to channel list control functions (e.g., refresh) throughout the component tree.
 * Default value is null; must be provided by a ChannelListControllerProvider.
 */
export const ChannelListControllerContext = createContext<ChannelListController | null>(null);

/**
 * Custom hook to access the ChannelListController context.
 *
 * @returns {ChannelListController} The context value (controller functions).
 * @throws {Error} If used outside of a ChannelListControllerProvider.
 *
 * Usage:
 *   const { refresh } = useChannelListController();
 */
export const useChannelListController = (): ChannelListController => {
    const ctx = useContext(ChannelListControllerContext);
    if (!ctx) {
        // Enforce usage within a provider for safety and predictability
        throw new Error('useChannelListController must be used within a ChannelListControllerProvider');
    }
    return ctx;
};
