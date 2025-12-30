import React, { createContext, useContext, useState, ReactNode } from 'react';

interface DirectoryContextType {
    showDirectory: boolean;
    setShowDirectory: (show: boolean) => void;
    toggleShowDirectory: () => void;
}

const DirectoryContext = createContext<DirectoryContextType | undefined>(undefined);

export const useDirectoryContext = () => {
    const context = useContext(DirectoryContext);
    if (!context) {
        throw new Error('useDirectoryContext must be used within a DirectoryProvider');
    }
    return context;
};

interface DirectoryProviderProps {
    children: ReactNode;
}

export const DirectoryProvider: React.FC<DirectoryProviderProps> = ({ children }) => {
    const [showDirectory, setShowDirectory] = useState(false);
    const toggleShowDirectory = () => setShowDirectory(prev => !prev);
    return (
        <DirectoryContext.Provider value={{ showDirectory, setShowDirectory, toggleShowDirectory }}>
            {children}
        </DirectoryContext.Provider>
    );
};
