# 📦 Asset Export Notes

This document contains implementation notes for improving runtime asset export quality.

---

## 🎯 Goal

Export only relevant, scene-backed assets instead of all assets loaded into memory.

---

## 🧭 Recommended Approach

Use scene/object-based traversal instead of global resource scans.

- Preferred source: active and inactive scene objects
- Avoid broad `Resources.FindObjectsOfTypeAll` as the primary source for gameplay-focused exports
- Keep deduplication by asset name/id

---

## 🔄 Suggested Export Flow

1. Collect scene `GameObject` instances.
2. Export meshes referenced by `MeshFilter.sharedMesh`.
3. Export textures referenced by renderer materials (`mainTexture` + optional shader properties).
4. Export UI textures/sprites from visible UI components.
5. Write metadata summaries for traceability.

---

## ✅ Why This Improves Output Quality

- Reduces unrelated assets loaded internally by Unity.
- Keeps exported textures tied to actually used materials.
- Produces smaller and cleaner output folders.

---

## ⚙️ IL2CPP / Unity Constraints

- Mesh read access may fail if mesh data is not readable at runtime.
- Texture readback may require temporary render targets/copies.
- Prefer robust error handling and continue-on-failure per asset.

---

## 🧪 Validation Checklist

- Trigger export in representative scenes.
- Compare resulting asset count before/after filtering.
- Verify model-texture associations in output.
- Confirm metadata files are generated.

---

## 🚀 Future Extensions

- Optional audio clip export.
- Optional scriptable object metadata dump.
- Additional per-scene filtering rules.
