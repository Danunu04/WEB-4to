# Vertical Navigation Bar Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create a collapsible vertical navigation bar in a new DashBoard.Master page, used by all internal/authenticated pages after login.

**Architecture:** New `DashBoard.Master` master page with a left sidebar containing Bootstrap Icons menu items, a toggle button for collapse/expand, and a main content area. Sidebar state persisted in localStorage. Existing pages (Inicio, Login) remain unchanged.

**Tech Stack:** ASP.NET Web Forms, Bootstrap 5.3.8 (CDN), Bootstrap Icons 1.11.3 (CDN), vanilla JS for toggle logic.

---

## File Structure

| Action | File | Responsibility |
|--------|------|----------------|
| Create | `gymAppV2/DashBoard.Master` | Master page with sidebar + content layout |
| Create | `gymAppV2/DashBoard.Master.cs` | Code-behind (logout handler) |
| Create | `gymAppV2/Content/dashboard.css` | Sidebar and layout styles |
| Modify | `gymAppV2/DashBoard/WebForm1.aspx` | Wire up to DashBoard.Master |
| Modify | `gymAppV2/DashBoard/WebForm1.aspx.cs` | Update namespace/inheritance |
| Modify | `gymAppV2/gymAppV2.csproj` | Register new files |

---

### Task 1: Create dashboard CSS styles

**Files:**
- Create: `gymAppV2/Content/dashboard.css`

- [ ] **Step 1: Create `dashboard.css` with sidebar and layout styles**

Create `gymAppV2/Content/dashboard.css`:

```css
/* ===== Dashboard Layout ===== */
.dashboard-wrapper {
    display: flex;
    min-height: 100vh;
    background-color: #FAECE7;
}

/* ===== Sidebar ===== */
.sidebar {
    width: 15rem;
    min-height: 100vh;
    background: rgba(255, 115, 107, 0.48);
    border-radius: 0 0.9375rem 0.9375rem 0;
    box-shadow: 0.25rem 0 1.875rem rgba(0, 0, 0, 0.1);
    display: flex;
    flex-direction: column;
    transition: width 0.3s ease;
    position: fixed;
    top: 0;
    left: 0;
    z-index: 1000;
    overflow: hidden;
}

.sidebar.collapsed {
    width: 4rem;
}

/* ===== Sidebar Brand ===== */
.sidebar-brand {
    font-family: 'Cherry Bomb One', cursive;
    color: #FAECE7;
    text-shadow: 0.15rem 0.15rem 1rem rgba(255, 255, 255, 0.5);
    font-size: 1.3rem;
    padding: 1.25rem 1rem;
    white-space: nowrap;
    overflow: hidden;
}

.sidebar.collapsed .sidebar-brand {
    text-align: center;
    padding: 1.25rem 0.5rem;
}

/* ===== Toggle Button ===== */
.sidebar-toggle {
    background: none;
    border: none;
    color: #FAECE7;
    font-size: 1.25rem;
    cursor: pointer;
    padding: 0.5rem;
    margin: 0 0.5rem 0.5rem auto;
    border-radius: 0.3125rem;
    transition: background-color 0.2s ease;
    display: block;
}

.sidebar-toggle:hover {
    background-color: rgba(255, 255, 255, 0.2);
}

.sidebar.collapsed .sidebar-toggle {
    margin: 0 auto 0.5rem auto;
}

/* ===== Menu Items ===== */
.sidebar-menu {
    list-style: none;
    padding: 0;
    margin: 0;
    flex: 1;
}

.sidebar-menu li {
    margin: 0;
}

.sidebar-menu a {
    display: flex;
    align-items: center;
    padding: 0.75rem 1rem;
    color: #FAECE7;
    text-decoration: none;
    transition: background-color 0.2s ease, color 0.2s ease;
    white-space: nowrap;
    overflow: hidden;
    border-radius: 0.3125rem;
    margin: 0.125rem 0.5rem;
}

.sidebar-menu a:hover,
.sidebar-menu a.active {
    background-color: rgba(255, 255, 255, 0.25);
    color: #fff;
}

.sidebar-menu a i {
    font-size: 1.25rem;
    min-width: 1.25rem;
    text-align: center;
    margin-right: 0.75rem;
}

.sidebar.collapsed .sidebar-menu a {
    justify-content: center;
    padding: 0.75rem 0;
    margin: 0.125rem 0.25rem;
}

.sidebar.collapsed .sidebar-menu a i {
    margin-right: 0;
}

.sidebar.collapsed .sidebar-menu a .menu-text {
    display: none;
}

/* ===== Divider ===== */
.sidebar-divider {
    border-top: 0.0625rem solid rgba(255, 255, 255, 0.3);
    margin: 0.5rem 1rem;
}

.sidebar.collapsed .sidebar-divider {
    margin: 0.5rem 0.25rem;
}

/* ===== Logout Item ===== */
.sidebar-logout {
    margin-top: auto;
    padding-bottom: 0.5rem;
}

/* ===== Main Content ===== */
.main-content {
    margin-left: 15rem;
    flex: 1;
    padding: 1.25rem;
    transition: margin-left 0.3s ease;
    min-height: 100vh;
}

.sidebar.collapsed ~ .main-content {
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
    background: rgba(0, 0, 0, 0.5);
    z-index: 999;
}

/* ===== Mobile Toggle (visible only on small screens) ===== */
.mobile-toggle {
    display: none;
    position: fixed;
    top: 0.625rem;
    left: 0.625rem;
    z-index: 998;
    background-color: #F4736B;
    color: white;
    border: none;
    border-radius: 0.3125rem;
    padding: 0.5rem;
    font-size: 1.25rem;
    cursor: pointer;
}

/* ===== Responsive ===== */
@media screen and (max-width: 768px) {
    .sidebar {
        width: 15rem;
        transform: translateX(-100%);
        transition: transform 0.3s ease;
    }

    .sidebar.mobile-open {
        transform: translateX(0);
    }

    .sidebar.collapsed {
        width: 15rem;
        transform: translateX(-100%);
    }

    .sidebar.collapsed.mobile-open {
        transform: translateX(0);
    }

    .main-content {
        margin-left: 0;
    }

    .mobile-toggle {
        display: block;
    }

    .sidebar-overlay.active {
        display: block;
    }
}
```

- [ ] **Step 2: Commit dashboard CSS**

```bash
git add gymAppV2/Content/dashboard.css
git commit -m "feat: add dashboard sidebar CSS styles"
```

---

### Task 2: Create DashBoard.Master page

**Files:**
- Create: `gymAppV2/DashBoard.Master`
- Create: `gymAppV2/DashBoard.Master.cs`

- [ ] **Step 1: Create `DashBoard.Master` with sidebar layout**

Create `gymAppV2/DashBoard.Master`:

```aspx
<%@ Master Language="C#" AutoEventWireup="true" CodeBehind="DashBoard.Master.cs" Inherits="gymAppV2.DashBoardMaster" %>

<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title><%: Page.Title %> - GymApp</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
    <link href="https://fonts.googleapis.com/css2?family=Cherry+Bomb+One&display=swap" rel="stylesheet" />
    <link href="~/Content/dashboard.css" rel="stylesheet" />
    <asp:ContentPlaceHolder ID="HeadContent" runat="server" />
</head>
<body>
    <form id="form1" runat="server">
        <button class="mobile-toggle" type="button" id="mobileToggle" aria-label="Abrir menú">
            <i class="bi bi-list"></i>
        </button>
        <div class="sidebar-overlay" id="sidebarOverlay"></div>
        <div class="dashboard-wrapper">
            <nav class="sidebar" id="sidebar" runat="server">
                <div class="sidebar-brand">GymApp</div>
                <button class="sidebar-toggle" type="button" id="sidebarToggle" aria-label="Colapsar menú">
                    <i class="bi bi-chevron-left"></i>
                </button>
                <ul class="sidebar-menu">
                    <li>
                        <a href="~/DashBoard" runat="server">
                            <i class="bi bi-house"></i>
                            <span class="menu-text">Dashboard</span>
                        </a>
                    </li>
                    <li>
                        <a href="~/Alumnos" runat="server">
                            <i class="bi bi-person"></i>
                            <span class="menu-text">Alumnos</span>
                        </a>
                    </li>
                    <li>
                        <a href="~/Entrenadores" runat="server">
                            <i class="bi bi-star"></i>
                            <span class="menu-text">Entrenadores</span>
                        </a>
                    </li>
                    <li>
                        <a href="~/Actividades" runat="server">
                            <i class="bi bi-calendar-event"></i>
                            <span class="menu-text">Actividades</span>
                        </a>
                    </li>
                    <li>
                        <a href="~/Rutinas" runat="server">
                            <i class="bi bi-list-check"></i>
                            <span class="menu-text">Rutinas</span>
                        </a>
                    </li>
                    <li>
                        <a href="~/Permisos" runat="server">
                            <i class="bi bi-shield-lock"></i>
                            <span class="menu-text">Permisos</span>
                        </a>
                    </li>
                </ul>
                <div class="sidebar-divider"></div>
                <ul class="sidebar-menu sidebar-logout">
                    <li>
                        <asp:LinkButton ID="lnkLogout" runat="server" OnClick="LnkLogout_Click">
                            <i class="bi bi-box-arrow-right"></i>
                            <span class="menu-text">Cerrar sesión</span>
                        </asp:LinkButton>
                    </li>
                </ul>
            </nav>
            <main class="main-content">
                <asp:ContentPlaceHolder ID="MainContent" runat="server" />
            </main>
        </div>
    </form>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/js/bootstrap.bundle.min.js"></script>
    <script>
        (function () {
            var sidebar = document.getElementById('sidebar');
            var toggle = document.getElementById('sidebarToggle');
            var mobileToggle = document.getElementById('mobileToggle');
            var overlay = document.getElementById('sidebarOverlay');

            function applyState(collapsed) {
                if (collapsed) {
                    sidebar.classList.add('collapsed');
                } else {
                    sidebar.classList.remove('collapsed');
                }
                localStorage.setItem('sidebarCollapsed', collapsed);
            }

            var saved = localStorage.getItem('sidebarCollapsed');
            if (saved === 'true') {
                sidebar.classList.add('collapsed');
            }

            toggle.addEventListener('click', function () {
                var isCollapsed = sidebar.classList.contains('collapsed');
                applyState(!isCollapsed);
            });

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
</body>
</html>
```

- [ ] **Step 2: Create `DashBoard.Master.cs` code-behind**

Create `gymAppV2/DashBoard.Master.cs`:

```csharp
using System;

namespace gymAppV2
{
    public partial class DashBoardMaster : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
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

- [ ] **Step 3: Commit master page**

```bash
git add gymAppV2/DashBoard.Master gymAppV2/DashBoard.Master.cs
git commit -m "feat: add DashBoard.Master with collapsible sidebar navigation"
```

---

### Task 3: Update DashBoard WebForm1 to use DashBoard.Master

**Files:**
- Modify: `gymAppV2/DashBoard/WebForm1.aspx`
- Modify: `gymAppV2/DashBoard/WebForm1.aspx.cs`

- [ ] **Step 1: Update `WebForm1.aspx` to reference DashBoard.Master**

Replace the entire content of `gymAppV2/DashBoard/WebForm1.aspx` with:

```aspx
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="gymAppV2.DashBoard.WebForm1" MasterPageFile="~/DashBoard.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Dashboard</title>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Dashboard</h1>
    <p>Bienvenido al panel de gestión.</p>
</asp:Content>
```

- [ ] **Step 2: Verify `WebForm1.aspx.cs` is unchanged**

The existing code-behind at `gymAppV2/DashBoard/WebForm1.aspx.cs` already has the correct namespace (`gymAppV2.DashBoard`) and class name (`WebForm1`). No changes needed — it inherits from `Page` which is correct for a content page.

- [ ] **Step 3: Commit DashBoard WebForm1 update**

```bash
git add gymAppV2/DashBoard/WebForm1.aspx
git commit -m "feat: wire DashBoard WebForm1 to DashBoard.Master"
```

---

### Task 4: Register new files in the .csproj

**Files:**
- Modify: `gymAppV2/gymAppV2.csproj`

- [ ] **Step 1: Add new files to .csproj**

In `gymAppV2/gymAppV2.csproj`, add these entries to the `<ItemGroup>` containing `<Content>` elements (around line 102-188):

Add after line 121 (`<Content Include="Content\Site.css" />`):

```xml
    <Content Include="Content\dashboard.css" />
```

Add after line 184 (`<Content Include="Site.Master" />`):

```xml
    <Content Include="DashBoard.Master" />
```

Add after line 207-209 (after the `DashBoard\WebForm1.aspx.cs` Compile entry):

```xml
    <Compile Include="DashBoard.Master.cs">
      <DependentUpon>DashBoard.Master</DependentUpon>
      <SubType>ASPXCodeBehind</SubType>
    </Compile>
```

- [ ] **Step 2: Commit .csproj update**

```bash
git add gymAppV2/gymAppV2.csproj
git commit -m "feat: register DashBoard.Master and dashboard.css in project"
```

---

### Task 5: Verify and test

- [ ] **Step 1: Build the solution**

Run: `msbuild gymAppV2.sln`
Expected: Build succeeds with no errors.

- [ ] **Step 2: Visual verification in browser**

Open the app in a browser, navigate to `/DashBoard/WebForm1`. Verify:
- Sidebar appears on the left with pink background
- All 7 menu items are visible with icons and text
- Clicking the collapse toggle shrinks the sidebar to icon-only
- Clicking again expands it back
- On mobile (narrow viewport), sidebar is hidden and hamburger toggle appears
- Logout link redirects to Inicio/Default.aspx
- Content area shifts when sidebar collapses/expands

- [ ] **Step 3: Final commit if any fixes needed**

```bash
git add -A
git commit -m "fix: adjust sidebar styles after visual verification"
```