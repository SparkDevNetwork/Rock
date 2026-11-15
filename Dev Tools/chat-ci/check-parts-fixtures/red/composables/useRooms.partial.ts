// A composable that reads the viewport, pages, counts generations, finds elements by test id
// and looks rooms up in the rows for itself: each of these has a shared home.
let searchGeneration = 0;

export function useRooms(rootElement: Ref<HTMLElement | null>, channels: ChannelStore): void {
    const isTouch = window.matchMedia?.("(hover: none)").matches ?? false;

    const observer = new IntersectionObserver(entries => {
        entries.forEach(entry => entry.isIntersecting && loadMore());
    });

    const place = rootElement.value?.querySelector("[data-testid='frame-main']");

    const row = channels.rows.find(r => r.channel_id === channelId);

    const generation = ++searchGeneration;
}
