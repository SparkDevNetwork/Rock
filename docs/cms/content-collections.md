---
title: Content Collections
last_updated: 2026-05-01
related_files:
  - Rock/Model/CMS/ContentCollection/ContentCollection.cs
  - Rock/Model/CMS/ContentCollection/ContentCollection.Logic.cs
  - Rock/Model/CMS/ContentCollection/ContentCollectionService.cs
  - Rock/Model/CMS/ContentCollectionSource/ContentCollectionSource.cs
  - Rock/Model/CMS/ContentCollectionSource/ContentCollectionSource.SaveHook.cs
  - Rock/Cms/ContentCollection/
---

# Content Collections

## Overview

Content Collections are Rock's cross-channel aggregation layer for site-wide search. A `ContentCollection` aggregates `ContentChannelItem`s from multiple `ContentChannel`s plus other content types. Items are indexed by an indexing job (Lucene-backed locally; Elasticsearch for high-volume sites). The Universal Search block consumes the collection, providing site-wide search across sermons + articles + ministry pages + events. Items in collections respect the channel's approved status; the indexing pipeline updates the search index when items change.

## Why It Exists

Site-wide search is a fundamental website feature; without it, visitors cannot find content across channels. Hardcoding a search index would have multiplied complexity; modeling collections as configuration with pluggable backends (Lucene / Elasticsearch) gives administrators flexibility without code.

The 2025 fix wave addressed real reliability issues: oversized attribute values broke the entire indexing job (`3cfb2abcec`, Fixes #6385); modifying items during indexing threw exceptions (`3cf07ec652`, Fixes #6365); the Lucene backend was 10x slower than it could be (`7fcfa422da`, 2025-04-14). Each fix made the system production-ready for sites with large content libraries.

The Elasticsearch bulk-indexing addition (commit `23ec04fc1f`, 2025-06-26) brought Elasticsearch up to par with Lucene for high-volume operations. Sites with millions of indexable items can now use Elasticsearch efficiently.

## Mental Model

```mermaid
flowchart LR
    Channels[Multiple ContentChannels] -->|configured sources| Sources[ContentCollectionSource rows]
    Sources --> Collection[ContentCollection]
    Collection -->|indexing job| Index[Search index<br/>Lucene or Elasticsearch]
    Index --> Search[Universal Search block]
    Search --> Results[Cross-channel results]
```

A collection has many sources (each typically a ContentChannel, but the architecture allows other source types). The indexing job iterates the sources, extracts indexable text, and writes to the configured search backend. The Universal Search block queries the index and returns results across the collection.

## What You Need to Know

**Two backends: Lucene (default) and Elasticsearch.** Lucene is local and zero-config; Elasticsearch is for sites with the operational maturity to run a search cluster. Configuration determines which.

**Lucene is now ~10x faster on retrieval (since `7fcfa422da`).** 2025-04-14 fix; sites running older builds may see significantly different performance.

**Elasticsearch supports bulk indexing (since `23ec04fc1f`).** 2025-06-26 fix; high-volume operations are now efficient.

**Indexing fails per-item on oversized fields, not per-collection.** Pre-fix `3cfb2abcec` (Fixes #6385), one oversized item could fail the entire indexing run. The fix isolates failures.

**Modify-during-indexing is safe.** Pre-fix `3cf07ec652` (Fixes #6365), concurrent modifications during indexing threw ObjectDisposedException. The fix corrects the lifecycle.

**Approved-status filter applies to collections.** Pending items are not indexed (or are indexed and excluded from search results, depending on configuration). Pre-fix `559605a5d8` (2026-03-30), the Content Collection's approved-status check was inconsistent with the rest of Rock; the fix consolidates.

**Sources can be ContentChannel-typed today; the architecture allows future expansion.** Hypothetically: index Pages, GroupTypes, or other entities. Today the Channel is the dominant source.

**`ContentCollection.IsActive = false` disables a collection.** Indexing skips inactive collections; search via the Universal Search block returns empty.

**Collection schema attributes are searchable.** Per-source attributes that are flagged "Index" appear in the search index. Configuration is per-source.

**Indexing runs on a job schedule.** Re-indexing happens on a configurable cadence; manual re-index via the Job Administration UI.

**Universal Search block configures the collection to search.** A site can have multiple collections; the block picks one.

## Common Scenarios

**"Set up site-wide search across sermons + articles."** Define ContentCollection "Site Search". Add ContentCollectionSource rows for the Sermons and Articles channels. Configure indexed attributes per source. Run the indexing job. Place the Universal Search block on a search results page.

**"Switch from Lucene to Elasticsearch."** Configure the Elasticsearch backend (provider plus connection settings). Re-index. The Universal Search block queries the new backend transparently.

**"Add a new source channel to an existing collection."** ContentCollectionSource row for the new channel. Re-index the collection.

**"Diagnose a slow search."** Verify the search backend is healthy. Check the indexing job's recent runs. Lucene perf should be 10x faster since `7fcfa422da`; older builds show the legacy speed.

**"Temporarily exclude a channel from search."** Disable the ContentCollectionSource row OR remove it. The next index run drops the channel's items.

**"Index custom attributes."** Configure the attribute as Indexed on the source. The next indexing run includes it.

## Key Architectural Decisions

### Two backend choice (Lucene + Elasticsearch)

Different deployments have different scale and operational maturity. Pluggable backend is correct.

### Source-based aggregation

Each ContentCollectionSource is a configuration row. Sources can be enabled / disabled / reordered.

### Indexing job-driven

Real-time indexing on every save would multiply DB load. Job-driven is the right tradeoff.

### Per-item failure isolation

One bad item should not break the whole index. The fix codifies this.

### Cross-channel aggregation as a separate entity

Modeling at the channel level would force every consumer to query each channel. The aggregation layer simplifies consumers.

## Considered but Rejected

### Single-backend (Lucene-only)

Rejected. High-volume sites need Elasticsearch; locking out Elasticsearch would compromise them.

### Real-time indexing on every save

Rejected. Cost too high; job-driven is correct.

### Per-channel-search blocks (no aggregation)

Rejected. Site-wide search is the universal feature; aggregation supports it.

## Technical Reference

### Schema (relevant subset)

`ContentCollection`:
- `Name`, `Description`
- `IconCssClass`
- `IsActive`
- Configuration for the search backend

`ContentCollectionSource`:
- `ContentCollectionId`
- Source entity reference (typically ContentChannel)
- Configuration for indexed attributes

### Backends

- **Lucene** (default): local file-based index. Configuration in `Rock.config` and the Lucene component.
- **Elasticsearch**: requires an Elasticsearch cluster. Configuration via the Elasticsearch component.

### Indexing Job

`Rock/Jobs/UniversalSearchIndexer.cs` (or similar): the job that iterates collections and writes to the index.

### Affected Blocks

- **Public:** Universal Search.
- **Admin:** Content Collection Detail/List, Universal Search Control Panel.

### Related Docs

- [docs/cms/content-channels.md](content-channels.md)
- [docs/cms/cms-overview.md](cms-overview.md)

## Recent Impactful Changes

- **2026-03-30** ([commit `559605a5d8`](https://github.com/SparkDevNetwork/Rock/commit/559605a5d8)). Content Collections now determine approved status consistently with the rest of Rock.
- **2025-08-08** ([commit `09492b6867`](https://github.com/SparkDevNetwork/Rock/commit/09492b6867)). Universal Search Re-Index job for Person now runs efficiently on large datasets (Fixes #6406).
- **2025-07-30** ([commit `3cfb2abcec`](https://github.com/SparkDevNetwork/Rock/commit/3cfb2abcec)). Content Collection indexing fails per-item, not per-collection, on oversized values (Fixes #6385).
- **2025-07-02** ([commit `3cf07ec652`](https://github.com/SparkDevNetwork/Rock/commit/3cf07ec652)). Modify-during-indexing no longer throws ObjectDisposedException (Fixes #6365).
- **2025-06-26** ([commit `23ec04fc1f`](https://github.com/SparkDevNetwork/Rock/commit/23ec04fc1f)). Elasticsearch backend supports bulk indexing.
- **2025-04-14** ([commit `7fcfa422da`](https://github.com/SparkDevNetwork/Rock/commit/7fcfa422da)). Lucene backend ~10x faster on result retrieval.
