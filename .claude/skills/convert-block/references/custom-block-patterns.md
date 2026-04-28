# Custom Block Patterns

Patterns specific to Custom / Standalone block conversions. Load this file when the block is classified as **Custom**.

---

## Base Class

Use `RockBlockType` — no grid, no entity detail scaffolding.

**Reference:** `Rock.Blocks/Crm/PersonalDevices.cs` + `Rock.JavaScript.Obsidian.Blocks/src/Crm/personalDevices.obs`

---

## Custom Block Actions

Define block actions with `[BlockAction]` for any server interaction the frontend needs:

```csharp
[BlockAction]
public BlockActionResult GetDeviceData( string deviceId )
{
    // Custom logic
    return ActionOk( resultBag );
}
```

Frontend invocation:
```typescript
const invokeBlockAction = useInvokeBlockAction();
const result = await invokeBlockAction<DeviceDataBag>("GetDeviceData", { deviceId });
```

---

## When to Reconsider Classification

If during implementation you discover:
- The block manages a single entity with Save/Cancel/Delete → reclassify as **Detail**
- The block renders a grid of records → reclassify as **List**

Go back to Phase 2 and adjust the design before continuing.
