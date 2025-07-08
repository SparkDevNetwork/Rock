// ChannelRightPaneContext.tsx
//
// Provides context and state management for the right pane in the chat UI, which can display
// various views such as channel info, threads, search, mentions, members, or more options.
//
// Author: [Your Name]
// Date: [Today's Date]

import React, { createContext, useContext, useState, ReactNode } from 'react';

/**
 * ChannelPaneView
 *
 * Type representing all possible views for the right pane.
 * - 'info': Channel information view
 * - 'threads': Thread list or thread view
 * - 'search': Message search view
 * - 'mentions': Mentions list view
 * - 'members': Channel members list or details
 * - 'more': Additional options
 * - null: No pane is open
 */
type ChannelPaneView = 'info' | 'threads' | 'search' | 'mentions' | 'members' | 'more' | null;

/**
 * ChannelRightPaneContextType
 *
 * Defines the shape of the context for managing the right pane state.
 * @property activePane - The currently active pane view (or null if closed)
 * @property setActivePane - Function to set the active pane view
 * @property closePane - Function to close the pane (sets activePane to null)
 */
interface ChannelRightPaneContextType {
    activePane: ChannelPaneView;
    setActivePane: (view: ChannelPaneView) => void;
    closePane: () => void;
}

/**
 * ChannelRightPaneContext
 *
 * React context for sharing the right pane state and actions across the chat UI.
 * Initialized as undefined to enforce usage within a provider.
 */
const ChannelRightPaneContext = createContext<ChannelRightPaneContextType | undefined>(undefined);

/**
 * useChannelRightPane
 *
 * Custom hook to access the ChannelRightPaneContext.
 * Throws an error if used outside of a ChannelRightPaneProvider.
 *
 * @returns {ChannelRightPaneContextType} The context value (activePane, setActivePane, closePane)
 * @throws {Error} If used outside of a ChannelRightPaneProvider
 */
export const useChannelRightPane = () => {
    const ctx = useContext(ChannelRightPaneContext);
    if (!ctx) throw new Error("useChannelRightPane must be used within a ChannelRightPaneProvider");
    return ctx;
};

/**
 * ChannelRightPaneProvider
 *
 * Provides the right pane state and actions to all descendants via context.
 * Wrap your component tree with this provider to enable right pane features.
 *
 * @param children - The React children to render within the provider
 * @returns {JSX.Element} The provider-wrapped children
 */
export const ChannelRightPaneProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
    // State for the currently active pane (null if closed)
    const [activePane, setPane] = useState<ChannelPaneView>(null);

    /**
     * setActivePane
     *
     * Sets the active pane view. If the same view is selected again (except for 'threads' and 'members'),
     * closes the pane instead (toggles off). This allows for toggling the pane open/closed.
     *
     * @param view - The pane view to activate
     */
    const setActivePane = (view: ChannelPaneView) => {
        setPane(prev => (prev === view && view !== 'threads' && view !== 'members' ? null : view));
    };

    /**
     * closePane
     *
     * Closes the right pane by setting activePane to null.
     */
    const closePane = () => {
        setPane(null);
    };

    return (
        <ChannelRightPaneContext.Provider
            value={{
                activePane,
                setActivePane,
                closePane,
            }}
        >
            {children}
        </ChannelRightPaneContext.Provider>
    );
};
