import React, { useCallback } from 'react';

type SearchInputProps = {
    value: string;
    onChange: (value: string) => void;
    onSubmit?: () => void;
    placeholder?: string;
    autoFocus?: boolean;
};

export const SearchInput: React.FC<SearchInputProps> = ({
    value,
    onChange,
    onSubmit,
    placeholder = 'Search messages...',
    autoFocus = false,
}) => {
    const handleChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
        onChange(e.target.value);
    }, [onChange]);

    const handleKeyDown = useCallback((e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === 'Enter' && onSubmit) {
            onSubmit();
        }
    }, [onSubmit]);

    return (
        <div className="search-input-container">
            <label className="search-input-label">
                <input
                    type="text"
                    className="search-input"
                    placeholder={placeholder}
                    value={value}
                    onChange={handleChange}
                    onKeyDown={handleKeyDown}
                    autoFocus={autoFocus}
                />
                <i className="fas fa-search search-icon"></i>
            </label>
        </div>
    );
};
