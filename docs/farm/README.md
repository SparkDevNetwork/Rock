# Farm Documentation

Farm (release-note name) / WebFarm (namespace) is Rock's multi-node deployment coordination layer. It propagates cache invalidation across nodes, supports leader election for jobs that should run on exactly one node, and uses a pluggable Bus (Redis, Azure Service Bus, in-process) for inter-node messaging. The subsystem is small (three core files) because its scope is narrow on purpose.

If you are new, start with [farm-overview.md](farm-overview.md).

## Files in this directory

| Doc | Summary |
|---|---|
| [Farm Domain Overview](farm-overview.md) | Cache invalidation broadcast, leader election per IntervalAction, pluggable Bus, and the eventual-consistency tradeoff. |
