# Sidebar Sportio - Design Spec

## Overview

Replace the existing placeholder sidebar in `DashBoard.Master` with a new Sportio-branded sidebar component. The sidebar uses a minimal light style with warm-toned role-based color accents, dynamic navigation items per user role, and responsive mobile behavior.

## Tech Stack

- **Platform**: ASP.NET Web Forms (.NET Framework 4.7.2)
- **Implementation**: HTML + CSS variables + vanilla JS in `DashBoard.Master`
- **CSS Framework**: Bootstrap 5.3.8 (already loaded via CDN)
- **Icons**: Bootstrap Icons 1.11.3 (already loaded)
- **Role data**: Passed from code-behind via `data-*` attributes on the sidebar element; JS reads them and applies CSS variables

## Visual Design

### Style: Minimal Claro

- White background panel with generous `border-radius: 16px`
- Role color appears only as accents: border-top on header, avatar gradient, active item highlight, logout button
- Inactive nav items in muted gray `#5a6c7d`
- Subtle box shadow: `0 4px 24px rgba(0,0,0,0.08)`

### Role Color Palette

All colors are in the warm pink-yellow spectrum to match the existing app theme (pink `#F4736B` on landing/login, yellow `rgba(249,199,79,0.6)` on cards).

| Role | Primary | Avatar Gradient | Active Item BG | Logout Button |
|------|---------|-----------------|----------------|---------------|
| Admin / Recepcion | `#F4736B` | `#F4736B → #F9C74F` | `rgba(244,115,107,0.12) → rgba(249,199,79,0.08)` | `#d63031` |
| Alumno | `#F9C74F` | `#F9C74F → #F4736B` | `rgba(249,199,79,0.15) → rgba(244,115,107,0.08)` | `#c0392b` |
| Entrenador | `#E8913A` | `#E8913A → #F9C74F` | `rgba(232,145,58,0.12) → rgba(249,199,79,0.08)` | `#b8601a` |
| Familiar | `#C77DBA` | `#C77DBA → #F4736B` | `rgba(199,125,186,0.12) → rgba(244,115,107,0.08)` | `#9b59b6` |

### CSS Variables

The sidebar uses CSS custom properties that JS sets based on the role:

```css
.sportio-sidebar {
  --role-primary: #F4736B;
  --role-primary-light: rgba(244,115,107,0.12);
  --role-primary-lighter: rgba(249,199,79,0.08);
  --role-gradient: linear-gradient(135deg, #F4736B, #F9C74F);
  --role-logout: #d63031;
}
```

JS reads `data-role` from the HTML element and sets these variables.

### Sidebar Structure (Desktop)

```
┌─────────────────────────┐
│  ┌───┐                  │  ← Header zone
│  │ AV │  User Name      │     Avatar (circular, gradient)
│  └───┘  Role Label      │     Role name in primary color
│─────────────────────────│  ← 3px border-bottom in primary color
│  · Alumnos      ← active│  ← Active: gradient bg + left border
│  · Entrenadores         │  ← Inactive: muted gray
│  · Asistencia            │
│  · Horarios              │
│  · Pagos                 │
│  · Clases                │
│                          │
│  ┌─────────────────────┐│
│  │   Cerrar sesión      ││  ← Logout: darker role color
│  └─────────────────────┘│
└─────────────────────────┘
```

Width: `15rem` expanded, `4rem` collapsed (icon-only mode).

### Avatar

- Circular, `3rem` diameter
- Background: role gradient
- Displays initials if no photo URL provided
- If `userAvatar` URL exists, shows the image instead

### Navigation Items per Role

| Role | Items |
|------|-------|
| admin | Alumnos, Entrenadores, Asistencia, Horarios, Pagos, Clases |
| alumno | Mi Rutina, Mi Progreso, Mis Pagos, Horarios, Competencias |
| entrenador | Mis Alumnos, Rutinas, Horarios, Asistencia |
| familiar | Alumnos, Pagos, Horarios |

Each item has a Bootstrap Icon and a bullet point (`·`) prefix.

### Active Item Styling

- Background: linear gradient from `--role-primary-light` to `--role-primary-lighter`
- Left border: `2.5px solid var(--role-primary)`
- Text color: `var(--role-primary)`
- Font weight: 500

### Hover Effect

- Background: `rgba(0,0,0,0.03)`
- Transition: `0.2s ease`

### Logout Button

- Pinned to bottom of sidebar with `margin-top: auto`
- Background: `var(--role-logout)` (darker shade of the role primary)
- Full width, `border-radius: 8px`
- White text, `font-weight: 500`

## Responsive Behavior

### Desktop (>= 768px)

- Sidebar visible on the left, `15rem` wide
- Collapse toggle button (chevron icon) at top
- Collapsed state: `4rem` wide, icons only, tooltip on hover

### Mobile (< 768px)

- Sidebar hidden off-screen by default
- **Bottom navigation bar** appears at the bottom of the viewport:
  - Shows 3-4 primary nav items as icon + short label
  - Last item is a hamburger (`☰`) that opens the full sidebar as an overlay
- Overlay sidebar covers content with a dark backdrop (`rgba(0,0,0,0.4)`)
- Tapping backdrop closes the overlay

### State Persistence

- Sidebar collapse state saved to `localStorage` key `sidebarCollapsed`
- Mobile overlay is ephemeral (no persistence)

## File Changes

### Modified Files

1. **`gymAppV2/DashBoard.Master`** — Replace sidebar HTML with Sportio structure
2. **`gymAppV2/DashBoard.Master.cs`** — Add code-behind logic to set `data-role`, `data-username`, `data-avatar` attributes from Session
3. **`gymAppV2/Content/dashboard.css`** — Replace current sidebar styles with Sportio styles, add CSS variables, add bottom nav styles, add responsive rules

### No New Files

All changes are in-place modifications to existing files. No new master pages, no new CSS files, no JS files.

## Data Flow

1. User logs in → code-behind stores role in Session
2. `DashBoard.Master.cs` Page_Load reads Session and sets `data-role="admin"`, `data-username="Admin User"`, `data-avatar="/path/photo.jpg"` on the sidebar `<nav>` element
3. Client-side JS reads these attributes on `DOMContentLoaded`
4. JS sets CSS custom properties on the sidebar element based on the role
5. JS also populates the nav items list based on the role
6. Bottom nav on mobile mirrors the first 3-4 items from the nav items list

## Interaction Details

- Clicking a nav item calls `onNavigate` (in Web Forms: navigates to the corresponding `~/Page` URL)
- Clicking logout calls `onLogout` (in Web Forms: `FormsAuthentication.SignOut()` + `Session.Abandon()` + redirect to `~/Inicio/Default.aspx`)
- Collapse toggle animates sidebar width with `0.3s ease` transition
- Mobile hamburger opens overlay with `0.3s ease` slide-in from left

## Accessibility

- All nav items are `<a>` tags with `href`
- Active item has `aria-current="page"`
- Sidebar has `role="navigation"` and `aria-label="Main navigation"`
- Logout button has `aria-label="Cerrar sesión"`
- Collapse toggle has `aria-label` and `aria-expanded`
- Mobile bottom nav has `role="navigation"` and `aria-label="Quick navigation"`