import React, { useState } from "react";

interface InsertLinkModalProps {
    selectedText: string;
    onInsert: (markdownLink: string) => void;
    onCancel: () => void;
}

export const InsertLinkModal: React.FC<InsertLinkModalProps> = ({
    selectedText,
    onInsert,
    onCancel,
}) => {
    const [text, setText] = useState(selectedText || '');
    const [url, setUrl] = useState('');

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (!url.trim()) return;

        const markdown = `[${text || url}](${url})`;
        onInsert(markdown);
    };

    return (
        <div>
            <div className="rock-add-link-modal-content">
                <fieldset className="rock-add-link-modal-fieldset" role="group">
                    <div className="rock-add-link-modal-field-row">
                        <label className="rock-add-link-modal-field-row-label" htmlFor="link-text">Text</label>
                        <span className="rock-add-link-modal-field-row-content">
                            <input
                                className="rock-add-link-modal-input"
                                type="text"
                                id="link-text"
                                name="link-url-text"
                                value={text}
                                onChange={(e) => setText(e.target.value)}
                            />
                        </span>
                    </div>

                    <div className="rock-add-link-modal-field-row">
                        <label className="rock-add-link-modal-field-row-label" htmlFor="link-url">URL</label>
                        <span className="rock-add-link-modal-field-row-content">
                            <input
                                className="rock-add-link-modal-input"
                                type="text"
                                id="link-url"
                                name="link-url"
                                value={url}
                                onChange={(e) => setUrl(e.target.value)}
                            />
                        </span>
                    </div>
                </fieldset>

                <div className="rock-add-link-modal-actions">
                    <button type="button" onClick={onCancel} className="rock-add-link-modal-button rock-add-link-modal-button-secondary">
                        Cancel
                    </button>
                    <button type="submit" onClick={handleSubmit} className="rock-add-link-modal-button rock-add-link-modal-button-primary">
                        Insert
                    </button>
                </div>
            </div>
        </div>
    );
};
