import React from 'react';
import { ChannelHeaderProps, useChannelListContext, ChannelHeader as DefaultHeader, useChannelMembershipState, useChannelPreviewInfo, useChannelStateContext, useChatContext, useTranslationContext } from 'stream-chat-react';
import { DefaultChatChannelNamer } from '../ChannelNamer/DefaultChannelNamer';
import { useChatConfig } from '../Chat/ChatConfigContext';
import { ChatViewStyle } from '../ChatViewStyle';
import { Avatar as DefaultAvatar } from 'stream-chat-react';
import { useChannelListController } from '../ChannelList/ChannelListControllerContext';

interface RockChannelHeaderProps extends ChannelHeaderProps {
    chatViewStyle: ChatViewStyle
}

/**
 * Overrides the built-in ChannelHeader to use the DefaultChatChannelNamer
 * for displayTitle, guarding against a null channel.
 */
export const RockChannelHeader: React.FC<RockChannelHeaderProps> = (props: RockChannelHeaderProps) => {

    const chatConfig = useChatConfig();
    const { refresh } = useChannelListController();
    const {
        Avatar = DefaultAvatar,
        image: overrideImage,
        live,
        title: overrideTitle,
    } = props;

    const { channel, watcher_count } = useChannelStateContext('ChannelHeader');
    const { openMobileNav, client } = useChatContext('ChannelHeader');
    const { t } = useTranslationContext('ChannelHeader');
    const { displayImage, displayTitle, groupChannelDisplayInfo } = useChannelPreviewInfo({
        channel,
        overrideImage,
        overrideTitle,
    });

    const { member_count, subtitle } = channel?.data || {};

    // If there's no channel yet, render the default header without a custom title
    if (!channel) {
        return <DefaultHeader />;
    }

    // Use our namer, falling back to undefined if it returns null
    const title = DefaultChatChannelNamer(channel, chatConfig.directMessageChannelTypeKey!, client.userID!) ?? undefined;
    if (props.chatViewStyle == ChatViewStyle.Conversational) {
        return <DefaultHeader title={title} />;
    }

    const membershipState = useChannelMembershipState(channel);

    const MenuIcon = () => (
        <svg
            data-testid='menu-icon'
            fill='none'
            height='24'
            viewBox='0 0 24 24'
            width='24'
            xmlns='http://www.w3.org/2000/svg'
        >
            <path
                clipRule='evenodd'
                d='M3 8V6H21V8H3ZM3 13H21V11H3V13ZM3 18H21V16H3V18Z'
                fill='black'
                fillRule='evenodd'
            />
        </svg>
    );

    return (
        <div className='str-chat__channel-header'>
            <button
                aria-label={t('aria/Menu')}
                className='str-chat__header-hamburger'
                onClick={openMobileNav}
            >
                <MenuIcon />
            </button>
            <Avatar
                className='str-chat__avatar--channel-header'
                groupChannelDisplayInfo={groupChannelDisplayInfo}
                image={displayImage}
                name={displayTitle}
            />
            <div className='str-chat__channel-header-end'>
                <p className='str-chat__channel-header-title'>
                    {displayTitle}{' '}
                    {live && (
                        <span className='str-chat__header-livestream-livelabel'>{String(t('live'))}</span>
                    )}

                    <span
                        className='rock__favorite-icon'
                        style={{ cursor: 'pointer' }}
                        onClick={async () => {
                            try {
                                if (membershipState?.pinned_at) {
                                    await channel.unpin();
                                } else {
                                    await channel.pin();
                                }

                                refresh();
                            } catch (error) {
                                console.error("Failed to toggle pin state", error);
                            }
                        }}
                    >
                        {membershipState?.pinned_at ? (
                            <i className="fas fa-star" title="Unpin" />
                        ) : (
                            <i className="far fa-star" title="Pin" />
                        )}
                    </span>

                </p>
                {subtitle && <p className='str-chat__channel-header-subtitle'>{subtitle}</p>}
                <p className='str-chat__channel-header-info'>
                    {!live && !!member_count && member_count > 0 && (
                        <>
                            {t('{{ memberCount }} members', {
                                memberCount: member_count,
                            })}
                            ,{' '}
                        </>
                    )}
                    {String(t('{{ watcherCount }} online', { watcherCount: watcher_count }))}
                </p>
            </div>
        </div>
    );
};
