// The channel store: the one home for finding a room's row.
export function rowOf(store: ChannelStore, channelId: string): SidebarRow | null {
    return store.rows.find(row => row.channel_id === channelId) ?? null;
}
