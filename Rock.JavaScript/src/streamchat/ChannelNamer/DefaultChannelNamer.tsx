// DefaultChatChannelNamer.ts
import type { Channel } from 'stream-chat';
import type { DefaultStreamChatGenerics } from 'stream-chat-react';

/**
 * Generates a display title for a channel based on its assigned name or its members (for DMs).
 * - If the channel has an explicit name, it's returned.
 * - For "messaging" channels (DMs), it builds a name from other members:
 *   • One member: returns that member's name or ID
 *   • Two members: "Alice and Bob"
 *   • More: "Alice, Bob and 3 more" (configurable)
 * - Otherwise, returns null.
 *
 * @param channel - The Stream Chat channel instance
 * @param currentUserId - The ID of the current user (excluded from DM names)
 * @param maxMemberNames - Max number of member names to display in a DM
 * @param separator - Separator between names when listing multiple members
 * @returns A human-friendly title or null if none applies
 */
export function DefaultChatChannelNamer<SCG extends DefaultStreamChatGenerics = DefaultStreamChatGenerics>(
    channel: Channel<SCG>,
    directMessageChannelTypeKey: string,
    currentUserId?: string,
    maxMemberNames = 2,
    separator = ','
): string | null {
    // 1) Use explicit channel name
    const explicitName = channel.data?.name?.trim();
    if (explicitName) return explicitName;

    // 2) DM channels: build name from other members
    if (channel.type === directMessageChannelTypeKey) {
        // collect user objects, filter undefined
        const members = Object.values(channel.state.members ?? {})
            .map(m => m.user)
            .filter((u): u is NonNullable<typeof u> => u != null);

        // other members excluding current user
        const others = members.filter(u => u.id !== currentUserId);
        const names = others.map(u => u.name?.trim() || u.id).sort();

        if (names.length === 0) {
            // only current user present
            const me = members.find(u => u.id === currentUserId);
            return me ? (me.name?.trim() || me.id) : null;
        }
        if (names.length === 1) return names[0];
        if (names.length === 2) return `${names[0]} and ${names[1]}`;

        // more than 2 members
        const displayed = names.slice(0, maxMemberNames).join(`${separator} `);
        const extra = names.length - maxMemberNames;
        return extra > 0 ? `${displayed} and ${extra} more` : displayed;
    }

    // 3) Other channel types without explicit name
    return null;
}
