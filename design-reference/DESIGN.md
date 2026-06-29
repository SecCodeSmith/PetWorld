---
name: PetWorld Design System
colors:
  surface: '#f7faf5'
  surface-dim: '#d7dbd6'
  surface-bright: '#f7faf5'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f1f5ef'
  surface-container: '#ebefea'
  surface-container-high: '#e6e9e4'
  surface-container-highest: '#e0e3df'
  on-surface: '#181d1a'
  on-surface-variant: '#3f4943'
  inverse-surface: '#2d312e'
  inverse-on-surface: '#eef2ed'
  outline: '#6f7a72'
  outline-variant: '#bec9c1'
  surface-tint: '#176b4b'
  primary: '#096444'
  on-primary: '#ffffff'
  primary-container: '#2e7d5b'
  on-primary-container: '#d0ffe3'
  inverse-primary: '#88d6af'
  secondary: '#43664d'
  on-secondary: '#ffffff'
  secondary-container: '#c2e9c9'
  on-secondary-container: '#486a51'
  tertiary: '#515956'
  on-tertiary: '#ffffff'
  tertiary-container: '#6a716e'
  on-tertiary-container: '#eff6f2'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#a4f3ca'
  primary-fixed-dim: '#88d6af'
  on-primary-fixed: '#002113'
  on-primary-fixed-variant: '#005236'
  secondary-fixed: '#c5eccc'
  secondary-fixed-dim: '#aad0b1'
  on-secondary-fixed: '#00210e'
  on-secondary-fixed-variant: '#2c4e36'
  tertiary-fixed: '#dde4e0'
  tertiary-fixed-dim: '#c1c8c4'
  on-tertiary-fixed: '#161d1b'
  on-tertiary-fixed-variant: '#414846'
  background: '#f7faf5'
  on-background: '#181d1a'
  surface-variant: '#e0e3df'
typography:
  display-lg:
    fontFamily: Plus Jakarta Sans
    fontSize: 48px
    fontWeight: '700'
    lineHeight: '1.2'
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Plus Jakarta Sans
    fontSize: 32px
    fontWeight: '700'
    lineHeight: '1.3'
  headline-md:
    fontFamily: Plus Jakarta Sans
    fontSize: 24px
    fontWeight: '600'
    lineHeight: '1.4'
  body-lg:
    fontFamily: Plus Jakarta Sans
    fontSize: 18px
    fontWeight: '400'
    lineHeight: '1.6'
  body-md:
    fontFamily: Plus Jakarta Sans
    fontSize: 16px
    fontWeight: '400'
    lineHeight: '1.6'
  label-md:
    fontFamily: Plus Jakarta Sans
    fontSize: 14px
    fontWeight: '600'
    lineHeight: '1.2'
    letterSpacing: 0.05em
  headline-lg-mobile:
    fontFamily: Plus Jakarta Sans
    fontSize: 28px
    fontWeight: '700'
    lineHeight: '1.3'
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  base: 8px
  xs: 4px
  sm: 12px
  md: 24px
  lg: 48px
  xl: 80px
  container-max: 1280px
  gutter: 24px
---

## Brand & Style

The design system is built on the pillars of "Reliable Companionship" and "Modern Wellness." It targets pet owners who value high-quality nutrition and intelligent shopping experiences. The aesthetic is **Modern Minimalist with a Humanist touch**, blending the precision of an AI shopping assistant with the warmth of a pet-friendly environment.

The UI should evoke a sense of calm and competence. We achieve this through generous whitespace (breathing room), a soft color palette that avoids high-vibration "neon" tones, and a tactile quality that makes digital interactions feel friendly and safe.

**Key Stylistic Principles:**
- **Clarity over Clutter:** Every element must serve a purpose; prioritize empty space to reduce cognitive load during the shopping process.
- **Friendly Geometry:** Avoid sharp corners. Use soft, consistent radii to mirror the organic nature of pets.
- **Subtle Motif:** Use paw-print patterns as ultra-low-opacity backgrounds (2-3% opacity) or within "Success" state illustrations to reinforce the brand identity without distracting from the task.

## Colors

The palette is anchored by **Forest Green**, a color that signals health, nature, and stability. 

- **Primary (#2E7D5B):** Used for primary actions, navigation headers, and critical brand touchpoints. It represents growth and vitality.
- **Secondary (#84A98C):** A desaturated sage used for secondary buttons, icon backgrounds, and decorative accents.
- **Neutral Background (#F7F8F6):** A warm, off-white grey that prevents eye strain and provides a softer canvas than pure white.
- **Surface White (#FFFFFF):** Reserved for "lifted" elements like cards, modals, and input fields to create clear separation from the background.
- **Text (#1F2421):** A deep charcoal. Avoid pure black to maintain a premium, softer editorial feel.

**Semantic Colors:**
- **Success:** #43A047 (Nature Green)
- **Warning:** #FFB300 (Golden Retriever Gold)
- **Error:** #D32F2F (Deep Rose)

## Typography

The design system utilizes **Plus Jakarta Sans** for all levels. This typeface offers a clean, modern geometric structure with soft terminals that align perfectly with our friendly yet professional brand persona.

**Usage Guidelines (Polish Language):**
- Polish characters (ą, ć, ę, ł, ń, ó, ś, ź, ż) must maintain consistent vertical alignment; avoid tight line heights that may cause descenders and ascenders to clash.
- **Headlines:** Use Bold (700) weight for clear hierarchy.
- **Body Text:** Use Regular (400) for long-form content to ensure maximum readability.
- **AI Assistant Text:** Utilize `body-md` in Medium (500) weight to distinguish the AI's "voice" from static interface text.

## Layout & Spacing

The layout follows a **8px linear scale**, ensuring consistent vertical and horizontal rhythm. 

- **Desktop:** A 12-column fluid grid with a maximum container width of `1280px`. Gutters are set to `24px`.
- **Tablet:** 8-column grid with `16px` margins.
- **Mobile:** 4-column grid with `16px` margins.

**AI Assistant Integration:**
The AI assistant should occupy a persistent "drawer" or "floating bubble" on the bottom right. On desktop, when active, the main content container should shrink horizontally (reflow) rather than being covered by the assistant, maintaining a clear "co-pilot" relationship.

## Elevation & Depth

This design system uses **Tonal Layering** combined with **Soft Ambient Shadows** to create a sense of organized depth.

- **Level 0 (Background):** Used for the main page canvas (`#F7F8F6`). No shadows.
- **Level 1 (Cards/Surface):** Used for product cards and main content blocks. White surface with a very soft, diffused shadow: `0px 4px 20px rgba(31, 36, 33, 0.05)`.
- **Level 2 (Navigation/Floating):** Used for sticky headers and the AI chat bubble. Higher elevation with a more pronounced shadow: `0px 8px 30px rgba(31, 36, 33, 0.08)`.
- **Level 3 (Modals):** Highest priority. Uses a background blur (12px) on the overlay to focus the user’s attention, with a sharp contrast against the modal content.

## Shapes

The shape language is defined by **Rounded (10px)** corners. This specific radius strikes the balance between the "bubbly" look of children's apps and the "sharp" look of corporate finance.

- **Standard Elements:** Buttons, Input Fields, and Checkboxes use the base `0.5rem` (8px-10px) radius.
- **Large Elements:** Product imagery and Container cards use `1rem` (16px) for a more pronounced "friendly" container feel.
- **Buttons:** Primary buttons are rounded-lg, while secondary navigational chips may use pill-shaped (rounded-full) styling to distinguish them from actionable buttons.

## Components

**Buttons:**
- **Primary:** Forest Green background, White text. 10px corner radius. High-contrast and bold.
- **Secondary:** Surface White with a 1px Forest Green border.
- **AI Action:** Sage Green (`#84A98C`) with a subtle shimmer effect to denote "intelligent" processing.

**Input Fields:**
- Background: Surface White.
- Border: 1px subtle grey, turning Primary Green on focus.
- Placeholder text: `body-md` in a lighter tint of the text color.

**Product Cards:**
- Surface White. Soft shadow (Level 1).
- Image at the top with `1rem` top-corner radius.
- Title in `headline-md`.
- Price in `headline-md` with Primary Green color to draw the eye.

**AI Assistant Bubble:**
- Circular or pill-shaped.
- Features a small "pulsing" animation when processing a request.
- Background: Primary Green. Icon: White.

**Checkboxes & Radio Buttons:**
- Always use the 10px rounded approach (softened corners for checkboxes).
- Active state: Primary Green fill with a white checkmark.