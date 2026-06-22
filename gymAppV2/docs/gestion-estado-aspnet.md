# Gestion de Estado en ASP.NET Web Forms

## El problema

HTTP es sin estado (stateless). Cada postback o navegacion a otra pagina crea una **nueva instancia** de la clase Page. Todo lo que tengas en variables de instancia se pierde.

```csharp
// ESTO SE PIERDE en cada postback:
private int Intentos;        // Se reinicia a 0
private string usuario;      // Se vuelve null

// ESTO sobrevive:
ViewState["Intentos"]        // Sobrevive postbacks en la MISMA pagina
Session["Usuario"]           // Sobrevive entre paginas distintas
```

---

## Opciones disponibles

### 1. ViewState — Misma pagina, entre postbacks

Guarda datos en un campo oculto dentro del HTML de la pagina.

```csharp
// Guardar
ViewState["Intentos"] = 3;

// Leer (devuelve object, hay que castear)
int intentos = ViewState["Intentos"] != null ? (int)ViewState["Intentos"] : 0;
```

**Cuando usarlo:** Contadores, datos del formulario que necesitas conservar al hacer click en un boton de la misma pagina.

**Limitaciones:** Solo funciona en la misma pagina. Al navegar a otra, se pierde.

---

### 2. Session — Entre paginas, por usuario

Cada usuario tiene su propia Session. No se comparte entre usuarios.

```csharp
// Guardar (en LogIn.aspx.cs)
Session["Usuario"] = usuario;

// Leer (en cualquier otra pagina)
BE._686DP_Usuario usuario = (BE._686DP_Usuario)Session["Usuario"];

// Verificar si esta logueado
if (Session["Usuario"] == null)
    Response.Redirect("~/LogIn/LogIn.aspx");

// Cerrar sesion
Session.Abandon();
```

**Cuando usarlo:** Datos del usuario logueado, carrito de compras, preferencias.

**Limitaciones:** Se pierde si el servidor reinicia (por defecto se guarda en memoria). Expira despues de 20 min de inactividad (configurable en Web.config).

---

### 3. Cookies — Entre visitas, en el navegador

```csharp
// Guardar
HttpCookie cookie = new HttpCookie("preferenciaIdioma", "es");
cookie.Expires = DateTime.Now.AddDays(30);  // Dura 30 dias
Response.Cookies.Add(cookie);

// Leer
HttpCookie cookie = Request.Cookies["preferenciaIdioma"];
string idioma = cookie?.Value;  // "es"
```

**Cuando usarlo:** Recordar preferencias, "Recordarme" en login, tema oscuro/claro.

**Limitaciones:** El usuario puede desactivarlas. Max ~4KB por cookie.

---

### 4. QueryString — Pasar datos por la URL

```csharp
// Al redirigir
Response.Redirect("~/DashBoard/WebForm1.aspx?id=" + usuarioId);

// Al recibir
int id = Convert.ToInt32(Request.QueryString["id"]);
```

**Cuando usarlo:** Pasar identificadores simples entre paginas (id de alumno, id de rutina).

**Limitaciones:** Visible en la URL. No guardar datos sensibles (contrasenas).

---

### 5. Application State — Compartido entre todos los usuarios

```csharp
// Guardar
Application["TotalVisitas"] = (int)Application["TotalVisitas"] + 1;

// Leer
int visitas = (int)Application["TotalVisitas"];
```

**Cuando usarlo:** Contadores globales, configuracion compartida.

**Limitaciones:** Se comparte entre TODOS los usuarios. Se pierde al reiniciar el servidor.

---

### 6. Cache — Similar a Application, con expiracion automatica

```csharp
// Guardar con expiracion de 10 minutos
Cache.Insert("listaActividades", actividades, null,
    DateTime.Now.AddMinutes(10), Cache.NoSlidingExpiration);

// Leer
var lista = Cache["listaActividades"];
```

**Cuando usarlo:** Datos costosos de consultar que no cambian seguido (listas de actividades, provincias).

---

## Resumen rapido

| Metodo       | Sobrevive postback | Sobrevive entre paginas | Sobrevive entre visitas | Por usuario |
|-------------|:------------------:|:-----------------------:|:----------------------:|:-----------:|
| ViewState   | Si                 | No                      | No                     | Si          |
| Session     | Si                 | Si                      | No*                    | Si          |
| Cookies     | Si                 | Si                      | Si                     | Si          |
| QueryString | No (es navigate)   | Si                      | Si (si en URL)         | Si          |
| Application | Si                 | Si                      | No*                    | No (global) |
| Cache       | Si                 | Si                      | No*                    | No (global) |

*Se pierde al reiniciar el servidor o al expirar.

---

## En tu proyecto GymApp

### Login (lo que ya tenes)

```csharp
// LogIn.aspx.cs — al loguearse
Session["Usuario"] = user;
FormsAuthentication.SetAuthCookie(user.USUARIO_Nombre, false);

// DashBoard.Master.cs — al desloguearse
FormsAuthentication.SignOut();
Session.Abandon();
```

### Verificar sesion en cada pagina protegida

```csharp
// En el Page_Load de cualquier pagina protegida
protected void Page_Load(object sender, EventArgs e)
{
    if (Session["Usuario"] == null)
        Response.Redirect("~/LogIn/LogIn.aspx");
}
```

### El problema con el Singleton en Web

```csharp
// PROBLEMA: En Web, _instancia es COMPARTIDA entre todos los usuarios
_686DP_Singleton.Instancia._686DPLogIN(usuario);

// Si el usuario A se loguea, y despues el usuario B se loguea,
// _instancia._usuario pasa a ser el usuario B para TODOS.
```

**Solucion:** Usar `Session` para datos por usuario. El Singleton solo para cosas globales (como configuracion de la app).

### Contador de intentos en Login

```csharp
// Ya implementado con ViewState:
private int Intentos
{
    get { return ViewState["Intentos"] != null ? (int)ViewState["Intentos"] : 0; }
    set { ViewState["Intentos"] = value; }
}
```

---

## Configurar timeout de Session en Web.config

```xml
<system.web>
    <sessionState timeout="30" />
    <!-- 30 minutos de inactividad antes de expirar -->
</system.web>
```

Ya tenes `<authentication><forms timeout="30" />` configurado, que controla la cookie de autenticacion. El `sessionState timeout` controla la Session. Podes ponerlo en el mismo Web.config.