# Completed Specs Index

This index lists every spec that has been moved into `specs/completed/`. It is maintained by the `spec` skill, please do not edit by hand.

| Spec | Domain | Author | Summary | Commit |
|------|--------|--------|---------|--------|
| [Lava → Fluid Bridge: Performance and Allocation Improvements](lava/260501-lava-fluid-bridge-perf-improvements.md) | Lava | Jon Edmiston | Performance and allocation review of the Lava → Fluid bridge introduced when Rock moved from DotLiquid to Fluid. Catalogs hot-path reflection, redundant per-render parsing, async-over-sync allocations, and a few latent thread safety bugs, with a checkbox per finding so reviewers can pick what to act on. | `8525c2c2da3cab13b113e79866cefb09dab42f74` |
| [Lava Engine Abstraction: Performance and Allocation Improvements](lava/260501-lava-engine-abstraction-perf-improvements.md) | Lava | Jon Edmiston | Performance and allocation review of the engine-agnostic Lava abstraction that sits between Rock and the active Lava engine (LavaService, LavaHelper, LavaEngineBase, WebsiteLavaTemplateCacheService, ResolveMergeFields, and related plumbing). Catalogs reflection-per-call hotspots, redundant pre-flight checks, per-render allocation, mutable-input bugs, and a few thread-safety issues, each as a checkbox so reviewers can pick what to act on. | `19d5369f8984e4fe71daf8fddae5c67cd2bdd290` |
