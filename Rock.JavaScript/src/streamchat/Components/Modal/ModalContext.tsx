// ModalContext.tsx
import React, { createContext, useCallback, useContext, useState } from 'react';

interface ModalContextValue {
    showModal: (options: ShowModalOptions) => void;
    hideModal: () => void;
}

interface ShowModalOptions {
    content: React.ReactNode;
    title: string;
}

const ModalContext = createContext<ModalContextValue | undefined>(undefined);

export const useModal = () => {
    const context = useContext(ModalContext);
    if (!context) {
        throw new Error("useModal must be used within a ModalProvider");
    }
    return context;
};

export const ModalProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [modalContent, setModalContent] = useState<React.ReactNode | null>(null);
    const [modalTitle, setModalTitle] = useState<string>('');

    const showModal = useCallback((options: ShowModalOptions) => {
        setModalContent(options.content);
        setModalTitle(options.title);
    }, []);

    const hideModal = useCallback(() => {
        setModalContent(null);
    }, []);

    return (
        <ModalContext.Provider value={{ showModal, hideModal }}>
            {children}
            {modalContent && (
                <div className="rock-chat-modal-backdrop" onClick={hideModal}>
                    <div className="rock-chat-modal-content str-chat" onClick={(e) => e.stopPropagation()}>
                        <header className="rock-chat-modal-header">
                            <h2 className="rock-chat-modal-title">{modalTitle}</h2>
                            <button
                                className="rock-chat-modal-close"
                                onClick={hideModal}
                                aria-label="Close dialog">
                                <i className="fas fa-times" />
                                {/* <XIcon className="create-channel-modal-close-icon" /> */}
                            </button>
                        </header>
                        {modalContent}
                    </div>
                </div>
            )}
        </ModalContext.Provider>
    );
};
