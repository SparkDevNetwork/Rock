# Mobile Documentation

Mobile is Rock's native mobile-app block surface, distinct from the Obsidian web blocks. Mobile blocks live under `Rock/Blocks/Types/Mobile/` (organized by domain) and render through the Rock Mobile shell. Data and services are shared with web; the rendering surface is mobile-specific.

If you are new, start with [mobile-overview.md](mobile-overview.md). Sub-topics worth their own docs (Mobile Block Type Bases, Push Notifications, PersonalDevice, Outreach Toolbox internals, Mobile Check-in flows) will be added as separate files.

## Files in this directory

| Doc | Summary |
|---|---|
| [Mobile Block Type Bases](block-type-bases.md) | `RockMobileBlockType` hierarchy, `[BlockAction]` round-trips, bag-based responses, configuration via attributes. |
| [Mobile Domain Overview](mobile-overview.md) | Server-side mobile block infrastructure, web-vs-mobile parity expectations, the bag-based response shape, and the shared-services + separate-blocks model. |
| [Mobile Push Notifications](push-notifications.md) | CommunicationType.Push routing, multi-device fan-out, fallback to email/SMS, token rotation. |
| [Personal Device and Registration](personal-device-and-registration.md) | `PersonalDevice` model, push tokens, MAC-presence detection, multi-device per Person. |
