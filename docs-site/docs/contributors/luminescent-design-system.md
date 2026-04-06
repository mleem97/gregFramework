---
sidebar_label: Luminescent design system
description: Visual and interaction guidelines for the docs site (Luminescent Architect).
---

The **Frika Mod Framework** (FMF) is the product name for all Docusaurus branding (site title, navbar, page titles). **Luminescent Architect** names this visual design system only.

# Design System Specification: The Luminescent Architect

## 1. Overview & Creative North Star

**The Creative North Star: "The Luminescent Architect"**  
This design system moves away from the "flat-and-boxy" utility of standard modding sites. It treats code and community interaction as an architectural feat. We achieve a high-end editorial feel through **Tonal Depth** and **Intentional Asymmetry**. Instead of rigid grids that feel like a spreadsheet, we use overlapping layers and "light-bleed" to guide the user’s eye. The goal is a digital environment that feels like a high-end laboratory: sterile, precise, yet pulsing with the energy of the teal primary accent.

## 2. Color & Atmospheric Theory

We do not use color simply to decorate; we use it to define space.

### The "No-Line" Rule

**Explicit Instruction:** Prohibit 1px solid borders for sectioning.  
Boundaries are defined solely through background color shifts. For example, a `surface-container-low` section sitting on a `surface` background creates a natural edge. This "Editorial Bleed" makes the interface feel expansive and premium rather than boxed-in.

### Surface Hierarchy & Nesting

Treat the UI as physical layers of "Synthetic Glass."

- **Layer 0 (Base):** `surface` (#001110) - The deep abyss.
- **Layer 1 (Sub-sections):** `surface-container-low` (#001715).
- **Layer 2 (Cards/Containers):** `surface-container` (#001E1C).
- **Layer 3 (Modals/Popovers):** `surface-container-high` (#002422).

### The Glass & Gradient Rule

Floating elements (Navigation, Tooltips) must use **Glassmorphism**.

- **Formula:** `surface-container` at 80% opacity + `backdrop-filter: blur(12px)`.
- **Signature Texture:** Primary CTAs must utilize a subtle linear gradient from `primary` (#61F4D8) to `primary-container` (#08C1A6) at a 135-degree angle. This prevents "flatness" and gives the button a machined, metallic quality.

Implementation reference: `docs-site/src/css/custom.css` (`@theme` tokens, `.btn-primary`, `.glass-card`, `.navbar`).

## 3. Typography: The Editorial Edge

The contrast between the technical precision of **Inter** and the geometric authority of **Space Grotesk** defines the brand.

- **Display & Headlines (Space Grotesk):** Use these for hero sections and documentation titles. High-contrast and slightly wider tracking (+2%) creates an authoritative, "tech-brochure" aesthetic. Tailwind / utility: `font-headline`.
- **Body & Labels (Inter):** Reserved for technical data and long-form wiki content. Inter’s tall x-height ensures readability against the dark `background`. Default body uses `font-sans`.
- **Code Blocks:** Must use a monospaced font (JetBrains Mono) nested in `surface-container-highest` to differentiate logic from documentation. Utility: `font-mono` / Infima monospace variables.

## 4. Elevation & Depth

We abandon the traditional drop-shadow. Depth is achieved via **Tonal Layering**.

- **The Layering Principle:** To lift a card, place a `surface-container-lowest` card inside a `surface-container-low` section. The "inverse lift" creates a sophisticated, recessed look.
- **Ambient Glows:** For "floating" primary elements, use a shadow with the color `primary` at 10% opacity, a blur of `32px`, and a spread of `-4px`. This mimics the glow of a physical LED.
- **The Ghost Border Fallback:** If a border is required for accessibility, use the `outline-variant` token at **15% opacity**. This creates a "suggestion" of a line that disappears into the background.

## 5. Component Architecture

### Buttons: The Kinetic Core

- **Primary:** Gradient fill (`primary` to `primary-container`), `on-primary` text. Add a 2px outer glow of `primary` on hover.
- **Secondary (Outlined):** No fill. A "Ghost Border" of `outline-variant` at 40%. On hover, the border opacity jumps to 100%.
- **Tertiary:** Pure text in `secondary`. Used for low-priority actions in the Sidebar.

### Navigation: Structural Fixed Points

- **Top Bar (Global):** Height: 72px. Background: `surface` (80% opacity) with `backdrop-blur`. No bottom border; use a subtle transition to `surface-container-low` for the content area.
- **Wiki Sidebar (Contextual):** Sticky position. Uses `surface-container-low`. Active links use a left-aligned 4px vertical pill in `primary` and high-contrast `on-surface` text.

### Cards & Information Architecture

- **Rule:** Forbid divider lines within cards.
- **Implementation:** Use 24px vertical padding (from the spacing scale) to separate the header from the body. Use `surface-variant` for metadata badges (e.g., "v1.2.0") to provide a "recessed" look.

### Code Blocks & Documentation

- **Container:** `surface-container-highest` with a `md` (0.75rem) corner radius.
- **Syntax Highlighting:** Use `tertiary` (#64D0FF) for functions, `secondary` (#1CEDE1) for strings, and `error_dim` (#D7383B) for keywords.

## 6. Do’s and Don’ts

### Do:

- **Do** use `primary_fixed_dim` for icons to ensure they don't overpower the text.
- **Do** allow for generous "negative space." High-end editorial design requires breathing room to feel premium.
- **Do** use `surface_bright` as a very subtle "top-light" gradient on large containers to simulate overhead lighting.

### Don’t:

- **Don’t** use pure white (#FFFFFF) for text. Always use `on_surface` (#C0FCF6) to reduce eye strain in dark mode.
- **Don’t** use 1px solid borders to separate sidebar items; use vertical spacing or a subtle background hover state.
- **Don’t** use standard "drop shadows" (Black/Grey). Always tint your shadows with the background or accent color to maintain tonal harmony.

## 7. Token map (implementation)

Semantic colors are defined as Tailwind v4 `@theme` variables in `docs-site/src/css/custom.css` (e.g. `--color-primary`, `--color-on-surface`, `--color-surface-container-low`). Prefer those utilities in React pages (`bg-background`, `text-on-surface`, `border-outline-variant/15`) so the site stays aligned with this spec.
