import React from 'react';
import { IconProps } from 'stream-chat-react/dist/types/types';

/**
 * ActionsIcon Component (Vertical)
 *
 * A vertical ellipsis icon (⋮), often used for action menus in vertical layouts.
 * Compatible with Stream's `IconProps`.
 *
 * @component
 * @example
 * <ActionsIcon className="custom-icon-class" />
 *
 * @param {string} [className] - Optional CSS class name to apply to the SVG element.
 *
 * @returns {JSX.Element} An SVG icon element representing a vertical menu.
 */
export const VerticalEllipsisIcon = ({ className = '' }: IconProps) => (
    <svg
        className={className}
        height="11"
        width="4"
        viewBox="0 0 4 11"
        xmlns="http://www.w3.org/2000/svg"
    >
        <path
            d="M3 1.5A1.5 1.5 0 1 1 0 1.5 1.5 1.5 0 0 1 3 1.5zm0 4A1.5 1.5 0 1 1 0 5.5 1.5 1.5 0 0 1 3 5.5zm0 4A1.5 1.5 0 1 1 0 9.5 1.5 1.5 0 0 1 3 9.5z"
            fillRule="nonzero"
        />
    </svg>
);
