# CMS Documentation

CMS is Rock's content, page, and site management. The most surface-area-heavy domain in Rock: `Site` -> `Layout` -> `Page` -> `Block` is the rendering hierarchy; `ContentChannel` -> `ContentChannelItem` is the content model; `LavaApplication` is the URL-routable Lava endpoint surface; `PersonalizationSegment` and `AdaptiveMessage` add audience targeting; `MediaElement` handles audio/video; `ContentCollection` indexes for site-wide search.

If you are new, start with [cms-overview.md](cms-overview.md). Sub-topics worth their own docs (Pages and Routing, Content Channels, Content Collections, Personalization, Lava Applications, Adaptive Messages, Media, Universal Search backends) will be added as separate files.

## Files in this directory

| Doc | Summary |
|---|---|
| [CMS Domain Overview](cms-overview.md) | Three orthogonal hierarchies (rendering, content, personalization), how they join at the page/block layer, and the universal-search backends. |
| [Content Channels](content-channels.md) | Channel/Type/Item hierarchy, attribute-based schema, approval, item caching. |
| [Content Collections](content-collections.md) | Cross-channel aggregation, Lucene vs Elasticsearch backends, indexing-job resilience. |
| [Lava Applications](lava-applications.md) | URL-routable Lava endpoints, Helix reactivity, Body merge fields, RenderLavaEndpoint composition. |
| [Media Elements](media-elements.md) | Account/Folder/Element hierarchy, external provider integration, sync jobs, mediaplayer shortcode. |
| [Obsidian Content](obsidian-content.md) | Prototype (unmerged). In-page Vue authoring, out-of-process server compile, and the MCP tools that drive it from a chat. |
| [Pages and Routing](pages-and-routing.md) | Site/Layout/Page/Block four-layer hierarchy, friendly URLs, page short links with expiration and inbound UTM fallback, IP geolocation. |
| [Personalization and Segments](personalization-and-segments.md) | PersonalizationSegment + RequestFilter + AdaptiveMessage, persistent membership, anonymous + authenticated personalization. |
