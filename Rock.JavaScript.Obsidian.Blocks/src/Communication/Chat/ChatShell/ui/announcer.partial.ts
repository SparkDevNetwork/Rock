import { Ref } from "vue";
export type Announcer = { announce: (text: string, politeness?: "polite" | "assertive") => void };
export function provideAnnouncer(_root: Ref<HTMLElement | undefined>): void {
    return;
}
export function useAnnouncer(): Announcer {
    return { announce: () => undefined };
}
export function useAnnouncerModal(_element: Ref<HTMLElement | undefined>): void {
    return;
}
