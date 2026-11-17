# Errores encontrados (versión para personas)

Revisión general del código del 17/11/2026. **No se cambió nada del código:** esto es solo una lista de lo que llamó la atención.
Cada número coincide con el mismo número de `Herroes_IA.md`, que tiene el detalle técnico y cómo arreglarlo.

**Gravedad:** 🔴 alta (puede romper algo o es un agujero de seguridad) · 🟠 media · 🟡 baja (prolijidad o casos raros).

**Para la demo de mañana, lo que más conviene mirar:** el 6 (el peso se borra al modificar un alumno) y el 12 (cualquier error dice "Acceso denegado").

---

## 🔴 Gravedad alta

### 1. Las pantallas siguen funcionando aunque digan "acceso denegado"
Cuando alguien entra a una pantalla para la que no tiene permiso, el sistema lo manda a "Acceso denegado", pero la pantalla igual se termina de ejecutar por detrás. Alguien con conocimientos técnicos podría ver los datos de esa pantalla, o tocar sus botones, sin tener permiso. Pasa lo mismo si no inició sesión.

### 2. Al recuperar la contraseña, la persona entra al sistema sin cambiarla
En "Olvidé mi contraseña", con solo responder bien la pregunta de seguridad la persona ya queda adentro con su sesión iniciada. Puede salir de la pantalla de cambio de contraseña y usar el sistema con la cuenta todavía bloqueada. También pasa con cuentas desactivadas.

### 3. Cualquiera puede bloquear la cuenta de otra persona
En la pantalla de la pregunta de seguridad se puede escribir el usuario de cualquiera, por ejemplo el del administrador. Con una respuesta incorrecta, esa cuenta queda bloqueada. Además se puede probar respuestas sin límite hasta adivinar.

### 4. Si un Restore falla, la base puede quedar trabada
Antes de restaurar, el sistema pone la base en modo "un solo usuario". Si la restauración falla a mitad de camino (por ejemplo, con un archivo dañado), la base queda en ese modo y la aplicación deja de funcionar hasta que alguien la destrabe desde SQL Server.

### 5. Importar un .bacpac puede borrar la base
Para importar un .bacpac, el sistema **primero borra la base** y recién después busca el programa que hace la importación (SqlPackage). Si ese programa no está instalado, la base queda borrada y no se importa nada.

### 6. El peso de un alumno se borra al modificarlo (o se lee mal)
En la pantalla Alumnos:
- Al abrir "Modificar", el peso se carga como `70,00` (con coma), pero el campo solo acepta punto. El campo aparece vacío y, al guardar, **el peso del alumno se borra**.
- Si se escribe `70.5`, el sistema lo lee como 705 y da error de peso inválido.

Perfil, Rutinas y Cálculo de RM leen bien los decimales: el problema está solo en Alumnos.

---

## 🟠 Gravedad media

### 7. Destildar "Estado" en Modificar alumno no es una baja de verdad
Destildar "Estado" desactiva al alumno, pero no hace lo que hace el botón de baja: no desactiva su cuenta ni la del familiar. Al revés, si se lo vuelve a activar tildando "Estado", su cuenta sigue desactivada y no puede entrar.

### 8. Usuarios con datos inventados
Al crear un Administrador o Recepcionista sin algunos datos, el sistema rellena solo un DNI falso (999999991, 999999992...), el teléfono "0000-0000", nacimiento 1/1/1990 y el nombre "Empleado". Por eso esos usuarios parecen completos aunque no lo estén, y el script de usuarios incompletos no los detecta.

### 9. Cambiar el DNI de un entrenador o cliente borra cosas
En Usuarios → Modificar, si se cambia el DNI de un entrenador, el sistema borra su ficha de entrenador (y con ella sus rutinas y clases). Si es un cliente, borra su ficha de alumno. Además lo hace en varios pasos sueltos: si uno falla, lo anterior ya quedó hecho.

### 10. El sistema se va a ir poniendo lento
En cada pantalla que se abre, el sistema revisa la integridad de **toda la bitácora** y de todos los usuarios, desencriptando fila por fila. Hoy no se nota, pero cada acción agrega filas a la bitácora y cada página va a tardar un poco más.

### 11. Las claves de encriptación están a la vista
La clave que encripta los datos personales está escrita en `Web.config`, y ese archivo se sube a GitHub. Además, las contraseñas se guardan con SHA-256 sin "sal": dos usuarios con la misma contraseña tienen el mismo hash. Ya figuraba como pendiente de seguridad.

### 12. Cualquier error muestra "Acceso denegado"
Cuando algo falla, el sistema manda a la página de "Acceso denegado", que no tiene nada que ver con el problema. Lo mismo pasa si alguien escribe un `<` en cualquier campo (por ejemplo `<3`): ASP.NET lo rechaza por seguridad y aparece "Acceso denegado". Además, el sitio está en modo depuración (`debug="true"`).

---

## 🟡 Gravedad baja

### 13. Algunos textos se muestran sin "limpiar"
En varias grillas, los nombres, apellidos, descripciones de actividades y parentescos se muestran tal cual. Hoy ASP.NET bloquea los textos peligrosos al guardarlos (eso es lo del punto 12), así que el riesgo es bajo, pero conviene mostrarlos codificados.

### 14. Doble clic puede cobrar dos veces
Si el cliente toca dos veces rápido "Confirmar pago", o dos administradores confirman la misma transferencia al mismo tiempo, se pueden registrar dos abonos.

### 15. La pantalla de dígitos verificadores confía en lo que se ve
Cualquier usuario con sesión iniciada puede entrar a la pantalla de dígitos verificadores. Los botones de recalcular, restaurar e inicializar no controlan del lado del servidor que quien los usa sea administrador: dependen de que el panel esté oculto.

### 16. Lo que ve cada uno depende del tipo de cuenta, no del perfil
Varias pantallas, como Pagos y el Dashboard, deciden qué mostrar según el tipo de cuenta (Cliente, Entrenador...) y no según los permisos del perfil. Si a un entrenador se le diera el permiso de Pagos, vería la vista de administración completa.

### 17. La sesión no se renueva al iniciar sesión
Al entrar, el sistema reutiliza el mismo identificador de sesión que tenía antes del login. Es una buena práctica de seguridad generar uno nuevo.

### 18. Quedó código que ya no se usa
Las alertas de check-in (tabla, clases y estilos), las páginas `Default.aspx` de la raíz, `WebForm1.aspx`, `About`, `Contact` y `VerificacionDV.aspx` (una copia vacía), y el CSS del inicio viejo. No molestan, pero ensucian el proyecto.

### 19. Registrarse cierra la sesión de quien ya estaba adentro
Si alguien con la sesión iniciada (por ejemplo el admin, probando) usa "Registrarse" en el mismo navegador, al terminar el registro se le cierra su propia sesión.

### 20. Renombrar un usuario dejaría datos colgados
Si se cambiara el nombre de usuario de una cuenta, no se actualizarían su pregunta de seguridad ni su historial de contraseñas. Hoy la pantalla no deja cambiar el nombre de usuario, así que no pasa, pero el código lo permitiría.
