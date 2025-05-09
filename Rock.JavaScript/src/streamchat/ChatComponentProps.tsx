export interface ChatComponentProps {
    primaryColor: string;
    apiKey: string;
    userId: string;
    userToken: string;
    filterSharedChannelByCampus: boolean;
    currentCampusId: string | null;
    sharedChannelTypeKey?: string;
    directMessageChannelTypeKey?: string;
    cid?: string;
    jumpToMessageId?: string;
}