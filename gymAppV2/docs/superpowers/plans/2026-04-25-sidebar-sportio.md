# Sidebar Sportio Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the existing sidebar in DashBoard.Master with a Sportio-branded, role-based sidebar with warm color palette, dynamic nav items, and responsive bottom navigation.

**Architecture:** Rewrite DashBoard.Master HTML with Sportio structure. CSS variables drive role-based theming. Vanilla JS reads `data-*` attributes from code-behind and applies CSS variables + populates nav items. Mobile uses a bottom nav bar + overlay sidebar.

**Tech Stack:** ASP.NET Web Forms 4.7.2, Bootstrap 5.3.8, Bootstrap Icons 1.11.3, vanilla JS, CSS custom properties

---

## File Structure

| File | Action | Responsibility |
|------|--------|----------------|
| `gymAppV2/DashBoard.Master` | Modify | Sportio sidebar HTML structure, inline JS for role logic |
| `gymAppV2/DashBoard.Master.cs` | Modify | Set data-role, data-username, data-avatar from Session |
| `gymAppV2/Content/dashboard.css` | Modify | Sportio styles, CSS variables, bottom nav, responsive rules |

No new files. All changes in-place.

---

### Task 1: Rewrite DashBoard.Master.cs — Role data attributes

**Files:**
- Modify: `gymAppV2/DashBoard.Master.cs`

- [ ] **Step 1: Add Session-based role attribute injection**

Replace the entire file with:

```csharp
using System;
using System.Web.UI;

namespace gymAppV2
{
    public partial class DashBoardMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string role = Session["UserRole"] as string ?? "admin";
            string userName = Session["UserName"] as string ?? "Usuario";
            string userAvatar = Session["UserAvatar"] as string ?? "";

            sidebar.Attributes["data-role"] = role;
            sidebar.Attributes["data-username"] = userName;
            sidebar.Attributes["data-avatar"] = userAvatar;
        }

        protected void LnkLogout_Click(object sender, EventArgs e)
        {
            System.Web.Security.FormsAuthentication.SignOut();
            Session.Abandon();
            Response.Redirect("~/Inicio/Default.aspx");
        }
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add gymAppV2/DashBoard.Master.cs
git commit -m "feat: add role data attributes to sidebar from Session"
```

---

### Task 2: Rewrite DashBoard.Master — Sportio sidebar HTML + JS

**Files:**
- Modify: `gymAppV2/DashBoard.Master`

- [ ] **Step 1: Replace entire DashBoard.Master with Sportio structure**

Replace the entire file with:

```aspx
<%@ Master Language="C#" AutoEventWireup="true" CodeBehind="DashBoard.Master.cs" Inherits="gymAppV2.DashBoardMaster" %>

<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title><%: Page.Title %> - Sportio</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
    <link href="https://fonts.googleapis.com/css2?family=Cherry+Bomb+One&display=swap" rel="stylesheet" />
    <link href="~/Content/dashboard.css" rel="stylesheet" />
    <asp:ContentPlaceHolder ID="HeadContent" runat="server" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="sidebar-overlay" id="sidebarOverlay"></div>

        <div class="dashboard-wrapper">
            <nav class="sportio-sidebar" id="sidebar" runat="server" role="navigation" aria-label="Main navigation">
                <div class="sidebar-header">
                    <button class="sidebar-toggle" type="button" id="sidebarToggle" aria-label="Colapsar menú" aria-expanded="true">
                        <i class="bi bi-chevron-left"></i>
                    </button>
                    <div class="sidebar-avatar" id="sidebarAvatar">
                        <span class="avatar-initials" id="avatarInitials"></span>
                        <img class="avatar-photo" id="avatarPhoto" alt="" style="display:none;" />
                    </div>
                    <div class="sidebar-user-info">
                        <div class="sidebar-username" id="sidebarUsername"></div>
                        <div class="sidebar-role" id="sidebarRole"></div>
                    </div>
                </div>

                <ul class="sidebar-nav" id="sidebarNav"></ul>

                <div class="sidebar-footer">
                    <asp:LinkButton ID="lnkLogout" runat="server" OnClick="LnkLogout_Click" CssClass="sidebar-logout-btn" aria-label="Cerrar sesión">
                        <i class="bi bi-box-arrow-right"></i>
                        <span class="menu-text">Cerrar sesión</span>
                    </asp:LinkButton>
                </div>
            </nav>

            <main class="main-content">
                <asp:ContentPlaceHolder ID="MainContent" runat="server" />
            </main>
        </div>

        <nav class="bottom-nav" id="bottomNav" role="navigation" aria-label="Quick navigation">
            <div class="bottom-nav-items" id="bottomNavItems"></div>
            <button class="bottom-nav-item bottom-nav-more" type="button" id="mobileToggle" aria-label="Abrir menú completo">
                <i class="bi bi-list"></i>
                <span>Más</span>
            </button>
        </nav>

        <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/js/bootstrap.bundle.min.js"></script>
        <script>
        (function () {
            var ROLES = {
                admin: {
                    primary: '#F4736B',
                    primaryLight: 'rgba(244,115,107,0.12)',
                    primaryLighter: 'rgba(249,199,79,0.08)',
                    gradient: 'linear-gradient(135deg, #F4736B, #F9C74F)',
                    logout: '#d63031',
                    label: 'Administrador',
                    items: [
                        { name: 'Alumnos', icon: 'bi-person', href: '~/Alumnos' },
                        { name: 'Entrenadores', icon: 'bi-star', href: '~/Entrenadores' },
                        { name: 'Asistencia', icon: 'bi-check2-square', href: '~/Asistencia' },
                        { name: 'Horarios', icon: 'bi-calendar', href: '~/Horarios' },
                        { name: 'Pagos', icon: 'bi-credit-card', href: '~/Pagos' },
                        { name: 'Clases', icon: 'bi-people', href: '~/Clases' }
                    ]
                },
                alumno: {
                    primary: '#F9C74F',
                    primaryLight: 'rgba(249,199,79,0.15)',
                    primaryLighter: 'rgba(244,115,107,0.08)',
                    gradient: 'linear-gradient(135deg, #F9C74F, #F4736B)',
                    logout: '#c0392b',
                    label: 'Alumno',
                    items: [
                        { name: 'Mi Rutina', icon: 'bi-list-check', href: '~/Rutinas' },
                        { name: 'Mi Progreso', icon: 'bi-graph-up', href: '~/Progreso' },
                        { name: 'Mis Pagos', icon: 'bi-credit-card', href: '~/Pagos' },
                        { name: 'Horarios', icon: 'bi-calendar', href: '~/Horarios' },
                        { name: 'Competencias', icon: 'bi-trophy', href: '~/Competencias' }
                    ]
                },
                entrenador: {
                    primary: '#E8913A',
                    primaryLight: 'rgba(232,145,58,0.12)',
                    primaryLighter: 'rgba(249,199,79,0.08)',
                    gradient: 'linear-gradient(135deg, #E8913A, #F9C74F)',
                    logout: '#b8601a',
                    label: 'Entrenador',
                    items: [
                        { name: 'Mis Alumnos', icon: 'bi-people', href: '~/Alumnos' },
                        { name: 'Rutinas', icon: 'bi-list-check', href: '~/Rutinas' },
                        { name: 'Horarios', icon: 'bi-calendar', href: '~/Horarios' },
                        { name: 'Asistencia', icon: 'bi-check2-square', href: '~/Asistencia' }
                    ]
                },
                familiar: {
                    primary: '#C77DBA',
                    primaryLight: 'rgba(199,125,186,0.12)',
                    primaryLighter: 'rgba(244,115,107,0.08)',
                    gradient: 'linear-gradient(135deg, #C77DBA, #F4736B)',
                    logout: '#9b59b6',
                    label: 'Familiar',
                    items: [
                        { name: 'Alumnos', icon: 'bi-person', href: '~/Alumnos' },
                        { name: 'Pagos', icon: 'bi-credit-card', href: '~/Pagos' },
                        { name: 'Horarios', icon: 'bi-calendar', href: '~/Horarios' }
                    ]
                }
            };

            var sidebar = document.getElementById('sidebar');
            var toggle = document.getElementById('sidebarToggle');
            var mobileToggle = document.getElementById('mobileToggle');
            var overlay = document.getElementById('sidebarOverlay');
            var bottomNav = document.getElementById('bottomNav');
            var bottomNavItems = document.getElementById('bottomNavItems');

            var role = sidebar.getAttribute('data-role') || 'admin';
            var userName = sidebar.getAttribute('data-username') || 'Usuario';
            var userAvatar = sidebar.getAttribute('data-avatar') || '';
            var config = ROLES[role] || ROLES.admin;

            // Apply CSS variables
            sidebar.style.setProperty('--role-primary', config.primary);
            sidebar.style.setProperty('--role-primary-light', config.primaryLight);
            sidebar.style.setProperty('--role-primary-lighter', config.primaryLighter);
            sidebar.style.setProperty('--role-gradient', config.gradient);
            sidebar.style.setProperty('--role-logout', config.logout);

            // Avatar
            var avatarInitials = document.getElementById('avatarInitials');
            var avatarPhoto = document.getElementById('avatarPhoto');
            if (userAvatar) {
                avatarPhoto.src = userAvatar;
                avatarPhoto.style.display = 'block';
                avatarInitials.style.display = 'none';
            } else {
                var parts = userName.trim().split(/\s+/);
                var initials = parts.length >= 2
                    ? (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
                    : userName.substring(0, 2).toUpperCase();
                avatarInitials.textContent = initials;
            }

            // User info
            document.getElementById('sidebarUsername').textContent = userName;
            document.getElementById('sidebarRole').textContent = config.label;

            // Nav items
            var navList = document.getElementById('sidebarNav');
            config.items.forEach(function (item) {
                var li = document.createElement('li');
                var a = document.createElement('a');
                a.href = item.href;
                a.innerHTML = '<i class="bi ' + item.icon + '"></i>' +
                    '<span class="menu-text"><span class="nav-bullet">·</span> ' + item.name + '</span>';
                li.appendChild(a);
                navList.appendChild(li);
            });

            // Bottom nav items (first 3)
            var maxBottomItems = 3;
            config.items.slice(0, maxBottomItems).forEach(function (item) {
                var btn = document.createElement('a');
                btn.href = item.href;
                btn.className = 'bottom-nav-item';
                btn.innerHTML = '<i class="bi ' + item.icon + '"></i><span>' + item.name + '</span>';
                bottomNavItems.appendChild(btn);
            });

            // Collapse toggle
            function applyCollapsed(collapsed) {
                if (collapsed) {
                    sidebar.classList.add('collapsed');
                    toggle.setAttribute('aria-expanded', 'false');
                } else {
                    sidebar.classList.remove('collapsed');
                    toggle.setAttribute('aria-expanded', 'true');
                }
                localStorage.setItem('sidebarCollapsed', collapsed);
            }

            var saved = localStorage.getItem('sidebarCollapsed');
            if (saved === 'true') {
                sidebar.classList.add('collapsed');
                toggle.setAttribute('aria-expanded', 'false');
            }

            toggle.addEventListener('click', function () {
                applyCollapsed(!sidebar.classList.contains('collapsed'));
            });

            // Mobile overlay
            mobileToggle.addEventListener('click', function () {
                sidebar.classList.add('mobile-open');
                overlay.classList.add('active');
            });

            overlay.addEventListener('click', function () {
                sidebar.classList.remove('mobile-open');
                overlay.classList.remove('active');
            });
        })();
        </script>
    </form>
</body>
</html>
```

- [ ] **Step 2: Commit**

```bash
git add gymAppV2/DashBoard.Master
git commit -m "feat: replace sidebar with Sportio role-based design"
```

---

### Task 3: Rewrite dashboard.css — Sportio styles + bottom nav + responsive

**Files:**
- Modify: `gymAppV2/Content/dashboard.css`

- [ ] **Step 1: Replace entire dashboard.css with Sportio styles**

Replace the entire file with:

```css
/* ===== Dashboard Layout ===== */
.dashboard-wrapper {
    display: flex;
    min-height: 100vh;
    background-color: #FAECE7;
}

/* ===== Sportio Sidebar ===== */
.sportio-sidebar {
    --role-primary: #F4736B;
    --role-primary-light: rgba(244,115,107,0.12);
    --role-primary-lighter: rgba(249,199,79,0.08);
    --role-gradient: linear-gradient(135deg, #F4736B, #F9C74F);
    --role-logout: #d63031;

    width: 15rem;
    min-height: 100vh;
    background: #ffffff;
    border-radius: 0 1rem 1rem 0;
    box-shadow: 0.25rem 0 1.5rem rgba(0, 0, 0, 0.08);
    display: flex;
    flex-direction: column;
    transition: width 0.3s ease;
    position: fixed;
    top: 0;
    left: 0;
    z-index: 1000;
    overflow: hidden;
}

.sportio-sidebar.collapsed {
    width: 4rem;
}

/* ===== Sidebar Header ===== */
.sidebar-header {
    padding: 1.25rem 0.75rem 1rem;
    text-align: center;
    border-bottom: 0.1875rem solid var(--role-primary);
}

.sidebar-toggle {
    background: none;
    border: none;
    color: #5a6c7d;
    font-size: 1rem;
    cursor: pointer;
    padding: 0.25rem;
    border-radius: 0.375rem;
    transition: background-color 0.2s ease, color 0.2s ease;
    position: absolute;
    top: 0.75rem;
    right: 0.5rem;
    z-index: 2;
}

.sidebar-toggle:hover {
    background-color: rgba(0, 0, 0, 0.05);
    color: var(--role-primary);
}

.sportio-sidebar.collapsed .sidebar-toggle {
    right: 50%;
    transform: translateX(50%);
}

.sportio-sidebar.collapsed .sidebar-toggle i {
    transform: rotate(180deg);
}

/* ===== Avatar ===== */
.sidebar-avatar {
    width: 3rem;
    height: 3rem;
    border-radius: 50%;
    background: var(--role-gradient);
    margin: 0 auto 0.5rem;
    display: flex;
    align-items: center;
    justify-content: center;
    overflow: hidden;
}

.avatar-initials {
    color: #ffffff;
    font-size: 1.125rem;
    font-weight: 600;
    line-height: 1;
}

.avatar-photo {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.sportio-sidebar.collapsed .sidebar-avatar {
    width: 2.25rem;
    height: 2.25rem;
    margin: 0 auto 0.25rem;
}

.sportio-sidebar.collapsed .avatar-initials {
    font-size: 0.8125rem;
}

/* ===== User Info ===== */
.sidebar-user-info {
    text-align: center;
}

.sidebar-username {
    color: #2c3e50;
    font-size: 0.875rem;
    font-weight: 600;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}

.sidebar-role {
    color: var(--role-primary);
    font-size: 0.6875rem;
    font-weight: 500;
}

.sportio-sidebar.collapsed .sidebar-user-info {
    display: none;
}

/* ===== Nav Items ===== */
.sidebar-nav {
    list-style: none;
    padding: 0.5rem 0;
    margin: 0;
    flex: 1;
    overflow-y: auto;
}

.sidebar-nav li {
    margin: 0;
}

.sidebar-nav a {
    display: flex;
    align-items: center;
    padding: 0.5rem 0.75rem;
    color: #5a6c7d;
    text-decoration: none;
    transition: background-color 0.2s ease, color 0.2s ease;
    white-space: nowrap;
    overflow: hidden;
    border-left: 0.15625rem solid transparent;
    margin: 0.0625rem 0.5rem;
    border-radius: 0 0.375rem 0.375rem 0;
    font-size: 0.8125rem;
}

.sidebar-nav a:hover {
    background-color: rgba(0, 0, 0, 0.03);
    color: #2c3e50;
}

.sidebar-nav a.active {
    background: linear-gradient(90deg, var(--role-primary-light), var(--role-primary-lighter));
    border-left-color: var(--role-primary);
    color: var(--role-primary);
    font-weight: 500;
}

.sidebar-nav a i {
    font-size: 1.125rem;
    min-width: 1.25rem;
    text-align: center;
    margin-right: 0.625rem;
}

.nav-bullet {
    margin-right: 0.25rem;
}

.sportio-sidebar.collapsed .sidebar-nav a {
    justify-content: center;
    padding: 0.5rem 0;
    margin: 0.0625rem 0.25rem;
    border-left-color: transparent;
    border-radius: 0.375rem;
}

.sportio-sidebar.collapsed .sidebar-nav a i {
    margin-right: 0;
}

.sportio-sidebar.collapsed .sidebar-nav a .menu-text {
    display: none;
}

/* ===== Sidebar Footer ===== */
.sidebar-footer {
    padding: 0.625rem 0.75rem 0.75rem;
    margin-top: auto;
}

.sidebar-logout-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    background: var(--role-logout);
    color: #ffffff;
    border: none;
    border-radius: 0.5rem;
    padding: 0.5rem 0.75rem;
    font-size: 0.8125rem;
    font-weight: 500;
    text-decoration: none;
    transition: opacity 0.2s ease;
    cursor: pointer;
    width: 100%;
}

.sidebar-logout-btn:hover {
    opacity: 0.85;
    color: #ffffff;
    text-decoration: none;
}

.sidebar-logout-btn i {
    margin-right: 0.5rem;
    font-size: 1rem;
}

.sportio-sidebar.collapsed .sidebar-logout-btn .menu-text {
    display: none;
}

.sportio-sidebar.collapsed .sidebar-logout-btn {
    padding: 0.5rem;
}

/* ===== Main Content ===== */
.main-content {
    margin-left: 15rem;
    flex: 1;
    padding: 1.25rem;
    transition: margin-left 0.3s ease;
    min-height: 100vh;
    padding-bottom: 4rem;
}

.sportio-sidebar.collapsed ~ .main-content {
    margin-left: 4rem;
}

/* ===== Mobile Overlay ===== */
.sidebar-overlay {
    display: none;
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.4);
    z-index: 999;
}

.sidebar-overlay.active {
    display: block;
}

/* ===== Bottom Navigation (Mobile) ===== */
.bottom-nav {
    display: none;
    position: fixed;
    bottom: 0;
    left: 0;
    right: 0;
    height: 3.5rem;
    background: #ffffff;
    border-top: 0.0625rem solid #eee;
    box-shadow: 0 -0.125rem 0.75rem rgba(0, 0, 0, 0.06);
    z-index: 998;
    padding: 0 0.25rem;
    align-items: center;
    justify-content: space-around;
    border-radius: 1rem 1rem 0 0;
}

.bottom-nav-items {
    display: flex;
    align-items: center;
    flex: 1;
    justify-content: space-around;
}

.bottom-nav-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    text-decoration: none;
    color: #5a6c7d;
    font-size: 0.625rem;
    padding: 0.375rem 0.25rem;
    border-radius: 0.5rem;
    transition: color 0.2s ease;
    min-width: 3.5rem;
}

.bottom-nav-item i {
    font-size: 1.25rem;
    margin-bottom: 0.125rem;
}

.bottom-nav-item.active {
    color: var(--role-primary, #F4736B);
}

.bottom-nav-more {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    background: none;
    border: none;
    color: #5a6c7d;
    font-size: 0.625rem;
    padding: 0.375rem 0.25rem;
    border-radius: 0.5rem;
    cursor: pointer;
    transition: color 0.2s ease;
    min-width: 3.5rem;
    font-family: inherit;
}

.bottom-nav-more i {
    font-size: 1.25rem;
    margin-bottom: 0.125rem;
}

.bottom-nav-more:hover {
    color: var(--role-primary, #F4736B);
}

/* ===== Responsive ===== */
@media screen and (max-width: 768px) {
    .sportio-sidebar {
        width: 15rem;
        transform: translateX(-100%);
        transition: transform 0.3s ease;
        border-radius: 0 1rem 1rem 0;
    }

    .sportio-sidebar.mobile-open {
        transform: translateX(0);
    }

    .sportio-sidebar.collapsed {
        width: 15rem;
        transform: translateX(-100%);
    }

    .sportio-sidebar.collapsed.mobile-open {
        transform: translateX(0);
    }

    .main-content {
        margin-left: 0;
        padding-bottom: 4.5rem;
    }

    .bottom-nav {
        display: flex;
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add gymAppV2/Content/dashboard.css
git commit -m "feat: Sportio sidebar styles with role-based CSS variables and bottom nav"
```

---

### Task 4: Verify in browser

**Files:** None (verification only)

- [ ] **Step 1: Build and run the project**

Open the solution in Visual Studio and press F5, or run:
```bash
msbuild gymAppV2/gymAppV2.csproj /p:Configuration=Debug
```

Navigate to the dashboard page. Verify:

1. Sidebar shows white background with colored accents
2. Avatar displays initials with gradient background
3. Nav items show with bullet points (·)
4. Active item has gradient background + left border
5. Hover effect works on nav items
6. Collapse toggle works and persists in localStorage
7. Logout button shows in role color at bottom
8. On mobile (< 768px): bottom nav appears with icons
9. Mobile hamburger opens sidebar as overlay
10. Clicking overlay closes sidebar

- [ ] **Step 2: Test role switching**

In `DashBoard.Master.cs`, temporarily change the default role to test each:
- `data-role="admin"` → pink coral accents, 6 nav items
- `data-role="alumno"` → yellow accents, 5 nav items
- `data-role="entrenador"` → orange accents, 4 nav items
- `data-role="familiar"` → lilac accents, 3 nav items

Verify colors, avatars, and nav items change per role.

- [ ] **Step 3: Commit final verification (if any fixes were needed)**

```bash
git add -A
git commit -m "fix: adjust Sportio sidebar after browser verification"
```

---

## Self-Review

**Spec coverage check:**
- Visual style (Minimal Claro): Task 3 CSS
- Role color palette (4 roles): Task 2 JS (ROLES object) + Task 3 CSS variables
- CSS variables: Task 3 CSS
- Avatar (initials + photo): Task 2 JS + Task 3 CSS
- Nav items per role: Task 2 JS (ROLES object)
- Active item styling: Task 3 CSS
- Hover effect: Task 3 CSS
- Logout button: Task 2 HTML + Task 3 CSS
- Desktop sidebar (15rem/4rem): Task 3 CSS
- Mobile bottom nav: Task 2 HTML + Task 3 CSS
- Mobile overlay: Task 2 JS + Task 3 CSS
- localStorage persistence: Task 2 JS
- Accessibility (aria attributes): Task 2 HTML + JS
- Code-behind data attributes: Task 1 CS

**Placeholder scan:** No TBD, TODO, or placeholder patterns found.

**Type consistency:** ROLES object keys match `data-role` values. CSS variable names in JS (`setProperty`) match CSS (`var(--role-primary)`, etc.). Element IDs in JS match HTML `id` attributes.