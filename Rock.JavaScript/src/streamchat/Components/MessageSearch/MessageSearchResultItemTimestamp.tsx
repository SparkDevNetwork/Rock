import React, { useMemo } from 'react';

export interface MessageSearchResultItemTimestampProps {
    date: Date | string | number;
    customClass?: string;
}

function formatDate(date: Date) {
    // M/D/YY H:MM AM/PM
    const m = date.getMonth() + 1;
    const d = date.getDate();
    const yy = String(date.getFullYear()).slice(-2);
    let hours = date.getHours();
    const minutes = String(date.getMinutes()).padStart(2, '0');
    const ampm = hours >= 12 ? 'PM' : 'AM';
    hours = hours % 12;
    hours = hours ? hours : 12;
    return `${m}/${d}/${yy} ${hours}:${minutes} ${ampm}`;
}

export const MessageSearchResultItemTimestamp: React.FC<MessageSearchResultItemTimestampProps> = ({ date, customClass }) => {
    const d = useMemo(() => new Date(date), [date]);
    const formatted = useMemo(() => formatDate(d), [d]);
    const iso = d.toISOString();
    return (
        <time className={customClass} dateTime={iso} title={iso}>
            {formatted}
        </time>
    );
};
