# Vertical Navigation Bar Design

## Overview

Create a collapsible vertical navigation bar for internal pages (after login) in the Gym-APP Web Forms application, using a dedicated Master Page.

## Architecture

### New Master Page: `DashBoard.Master`

A new master page at `gymAppV2/DashBoard.Master` replaces `Site.Master` for all internal/authenticated pages. It contains:

- A collapsible sidebar on the left (~15rem expanded, ~4rem collapsed)
- A main content area on the right with `<asp:ContentPlaceHolder ID="MainContent">`
- A hamburger toggle button to collapse/expand the sidebar
- localStorage persistence for sidebar state (expanded/collapsed)
- Responsive behavior: on mobile, sidebar hides completely and toggles via hamburger button

### Pages Using DashBoard.Master

- `DashBoard/WebForm1.aspx` - updated to use DashBoard.Master
- All future internal pages (Alumnos, Entrenadores, Actividades, Rutinas, Permisos/Perfiles)

### Pages NOT Affected

- `Inicio/Default.aspx` - remains standalone (no master page)
- `LogIn/LogIn.aspx` - remains standalone (no master page)
- `About.aspx`, `Contact.aspx` - remain using Site.Master (not part of internal app)

## Menu Items

| # | Label | Icon | Link |
|---|-------|------|------|
| 1 | Dashboard | house | ~/DashBoard |
| 2 | Alumnos | person | ~/Alumnos |
| 3 | Entrenadores | star | ~/Entrenadores |
| 4 | Actividades | calendar | ~/Actividades |
| 5 | Rutinas | list-check | ~/Rutinas |
| 6 | Permisos/Perfiles | shield-lock | ~/Permisos |
| 7 | Cerrar sesión | box-arrow-right | (logout action) |

Icons use Bootstrap Icons (already available via CDN or local).

Item 7 (Cerrar sesión) is separated at the bottom with a visual divider.

## Visual Style

- Sidebar background: `#FAECE7` (light pink, matching existing Inicio/Login pages)
- Active/hover accent: `#F4736B` (pink, matching existing `.titulo` and button styles)
- Text color: dark for readability on light background
- Sidebar card with rounded corners (`border-radius`) and soft shadow (`box-shadow`)
- Font: consistent with existing pages, 'Cherry Bomb One' for the app title/logo
- All spacing/sizing in `rem` units per CLAUDE.md guidelines
- Smooth CSS transitions for expand/collapse animation

## Collapsible Behavior

- Toggle button: hamburger icon, visible at top of sidebar
- Expanded state: icon + text label for each menu item
- Collapsed state: icon only, narrower sidebar
- State saved to `localStorage` so it persists across page navigations
- On mobile (< 768px): sidebar completely hidden, toggle shows it as overlay

## Implementation Details

### Files to Create

1. `gymAppV2/DashBoard.Master` - new master page with sidebar layout
2. `gymAppV2/DashBoard.Master.cs` - code-behind (logout handler)
3. `gymAppV2/Content/dashboard.css` - sidebar and layout styles

### Files to Modify

1. `DashBoard/WebForm1.aspx` - change MasterPageFile to `~/DashBoard.Master`
2. `DashBoard/WebForm1.aspx.cs` - update if needed for new master page

### Dependencies

- Bootstrap 5.3.8 (already loaded via CDN in existing pages; DashBoard.Master will include it)
- Bootstrap Icons (new CDN dependency for menu icons)
- No additional NuGet packages required