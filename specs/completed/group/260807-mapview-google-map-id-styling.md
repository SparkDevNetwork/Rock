---
author: Joshua Henninger
date_created: 2026-08-07
summary: >-
  Teach the shared Obsidian map view control to render a Rock Map Style through
  a Google cloud Map ID (native light/dark) when one is configured, falling back
  to the legacy Dynamic Map Style JSON path otherwise, and let the Map Style be
  the sole authority for POI/label visibility.
contributors: []
---

# Google Map ID Styling Support for the Obsidian Map View

## Summary

The shared map view control (`Rock.JavaScript.Obsidian/Framework/Controls/mapView.obs`) styles its map only through the legacy Dynamic Map Style JSON, which is a single appearance and cannot theme itself for dark mode. A Rock Map Style can also carry a Google cloud **Map ID** (`core_GoogleMapId`), and cloud Map IDs support native light/dark. This spec adds a Map ID rendering path alongside the existing JSON path: when a Map Style provides a Map ID, the control renders through it and lets Google handle light/dark; otherwise it uses the JSON path. It also makes the Map Style the sole authority for POI/label visibility by removing the control's `declutterBaseMap` prop.

## Motivation

A Map Style's legacy `DynamicMapStyle` is one JSON style array with no theme variants. Painting a light-tuned JSON style in dark mode previously produced a half-dark hybrid (a recently committed fix mitigates this by keeping the base map light while the JSON is painted). Google has deprecated JSON styling in favor of cloud-based styling addressed by a Map ID, which is where light/dark color-scheme support properly lives.

The plumbing to support Map IDs already exists end to end: the `core_GoogleMapId` attribute flows through `GeoPickerGetGoogleMapSettings` into `GeoPickerGoogleMapSettingsBag.GoogleMapId`, and `mapView` already receives it. The control simply ignores it today. `geoPickerMap.obs` already implements the Map ID pattern and is the reference to follow, so this is a control-local change with no server or bag work.

## Requirements

- The control MUST render a Map Style through its Google cloud Map ID when `core_GoogleMapId` is present, and let Google's color scheme drive light/dark.
- The control MUST fall back to the legacy Dynamic Map Style JSON path when no Map ID is present, and to Google's default base map when neither is present.
- When both a Map ID and JSON are configured on the same Map Style, the Map ID MUST win. (Google ignores the JSON `styles` once a `mapId` is set, so the two cannot blend.)
- The map's color scheme MUST derive from the Rock theme (`<html theme>`) on the Map ID path and the Google-default path. The JSON path MUST NOT override the scheme (the JSON governs its own look and stays light).
- A provided JSON style MUST always be painted; the `applyStyleInDarkMode` prop/gate MUST be removed.
- The Map Style (JSON rules or cloud style) MUST be the sole authority for POI/label visibility; the control MUST NOT inject its own decluttering. The `declutterBaseMap` prop MUST be removed.
- The control MUST keep POI clicks inert (always `clickableIcons: false`, not exposed as a prop) to protect its own click-to-clear-selection behavior, independent of styling.
- On the Map ID (vector) path, markers MUST render with `google.maps.marker.AdvancedMarkerElement`. The JSON and default paths MAY keep legacy `google.maps.Marker`.

## Design

### Styling source precedence (highest wins)

1. **Map ID** (`core_GoogleMapId`) → Google cloud style. Owns colors and light/dark.
2. **Dynamic Map Style JSON** (`DynamicMapStyle`) → `google.maps.StyledMapType`.
3. **Neither** → Google default base map.

### Color scheme resolver

Resolve from `document.documentElement.getAttribute("theme")`:

| `theme` value | `google.maps.ColorScheme` |
|---|---|
| `light` | `LIGHT` |
| `dark` | `DARK` |
| `system` or unset | Explicit `LIGHT` / `DARK`, resolved from the `prefers-color-scheme` media query |

This resolver replaces the current boolean `isRockDarkMode()` usage on the Map ID and default paths. The JSON path does not set a theme-driven scheme.

`system` (or an unset `theme`) resolves to an explicit `LIGHT` or `DARK` rather than `FOLLOW_SYSTEM`: on a vector (Map ID) map, `FOLLOW_SYSTEM` flashes a light base while the map loads before settling on the OS preference. Reading `prefers-color-scheme` up front avoids that flash, and `startThemeWatch` re-applies the scheme live when the OS preference flips.

### Per-path behavior

| Source | Color scheme | Styling applied | Markers | POI visibility |
|---|---|---|---|---|
| Map ID | Resolver (light/dark/system) | Google cloud style (via `mapId` map option) | `AdvancedMarkerElement` | Defined in the cloud style |
| JSON | Light (no theme override) | `StyledMapType` from the JSON | Legacy `Marker` | Defined in the JSON |
| Neither | Resolver (light/dark/system) | Google default base map | Legacy `Marker` | Google default |

### Map creation

Follow the `geoPickerMap.obs` pattern: conditionally add the Map ID to the map options.

```ts
const mapId = settings.googleMapId ?? "";
const hasMapId = !isNullOrWhiteSpace(mapId);

map = new google.maps.Map(mapContainer.value, {
    // ...existing options...
    colorScheme: resolveColorScheme(),      // Map ID + default paths
    clickableIcons: false,                  // control invariant, always off (not a prop)
    ...(hasMapId && { mapId })
});

if (hasMapId) {
    // Cloud style governs; do not apply a StyledMapType (Google ignores styles when mapId is set).
}
else if (styles.length > 0) {
    // JSON path (color scheme stays light).
    const styledMap = new google.maps.StyledMapType(styles, { name: "Styled Map" });
    map.mapTypes.set("map_style", styledMap);
    map.setMapTypeId("map_style");
}
// else: Google default base map, color scheme from the resolver.
```

### Marker migration (main task, biggest risk)

`mapView` builds every marker with legacy `google.maps.Marker`: the group pins (custom icon + label), the hover dots, the marker-cluster markers, and the reference-location marker, plus the hover/selection wiring that mutates those markers. Legacy markers are deprecated on vector (Map ID) maps. The Map ID path MUST create these as `AdvancedMarkerElement` instead, which has a different construction and content model (a DOM element rather than an `icon`/`Symbol`).

**Decision: dual-mode, path-specific markers.** Keep two marker implementations, `AdvancedMarkerElement` on the Map ID path and legacy `google.maps.Marker` on the JSON and default paths. Do not migrate all paths wholesale. This limits the blast radius of the marker rewrite to Map ID maps and leaves the well-exercised legacy JSON path untouched, at the cost of two marker code paths to maintain. This dual-mode marker layer is the bulk of the implementation and the main source of uncertainty (clustering library compatibility, hover/selection restyling, z-ordering, click handling). `geoPickerMap.obs` shows the basic `AdvancedMarkerElement` construction to build on, but its marker needs are far simpler than the finder's.

## Cross-consumer impact

`mapView.obs` is a shared Framework control, but the Group Finder block is currently its only consumer (`geoPickerMap.obs` is a separate control). So removing `declutterBaseMap` and `applyStyleInDarkMode` is scoped to the Group Finder in practice: it is the one place to update. The change is still made in a shared control, so the surface is kept deliberately minimal (see the `clickableIcons` decision under Considered but Rejected) to avoid over-fitting to a single caller.

## Considered but Rejected

### Keep `declutterBaseMap` as a control-level overlay
Rejected. It fights the Map Style: on the JSON path it competed with the style's own POI rules, and on the Map ID path it cannot apply at all (cloud styling ignores injected `styles`). Making the Map Style the single source of truth for visibility is simpler and consistent across both paths.

### Keep `applyStyleInDarkMode` as an opt-in gate
Rejected. It existed to avoid the light-JSON-in-dark hybrid, which the committed color-scheme fix already resolves. With that fix a provided JSON style renders cleanly in both themes, so the gate is redundant.

### Encode dark styling inside the Dynamic Map Style JSON
Rejected. The legacy JSON `styles` array is a single appearance and has no light/dark variant mechanism; dark support lives on Google's cloud (Map ID) side. This is the reason the Map ID path is needed.

### Expose `clickableIcons` as a prop
Rejected for now. POI clicks are always disabled so they cannot swallow the map click that clears the current selection. The Group Finder is the only consumer today and does not need interactive POIs, so exposing a prop would be speculative surface area. Add the prop only when a real second consumer needs interactive POIs.

## Required Follow-up

The Group Finder block (`Rock.JavaScript.Obsidian.Blocks/src/Group/groupFinder.obs`) currently passes `declutterBaseMap: true` and `applyStyleInDarkMode`. Both props are being removed, so the block must drop them and instead rely on its configured Map Style to define POI-off (otherwise parks and other POIs reappear).

## Related

- Reference implementation: `Rock.JavaScript.Obsidian/Framework/Controls/geoPickerMap.obs` (Map ID option + `AdvancedMarkerElement`).
- Settings endpoint: `GeoPickerGetGoogleMapSettings` in `Rock.Rest/v2/ControlsController.cs` (already returns `GoogleMapId` from `core_GoogleMapId`).
- Control under change: `Rock.JavaScript.Obsidian/Framework/Controls/mapView.obs`.
- Consumer follow-up: `Rock.JavaScript.Obsidian.Blocks/src/Group/groupFinder.obs`.
