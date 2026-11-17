# Prueba final del sistema (entrega 17/11/2026)

Recorrido de unos 35 minutos, ordenado como la demo. Lo nuevo de hoy está marcado con ★. Anotá el número de la prueba que falle y el mensaje que aparece.

## 0. Preparación (5 min)
1. `git pull` y compilar en Visual Studio: sin errores.
2. Backup desde Gestión de respaldo (se puede confirmar en el cartel propio de la página, sin el "localhost says").
3. Correr `scripts/correcciones-entrega-final.sql`. Mirar el último resultado: los alumnos con un peso fuera de 20–300 kg. Corregirlos en el paso 13.
4. Correr `scripts/webmaster-perfil-editable.sql`, si todavía no se corrió.
5. VerificacioDV: "Inicializar control" si aparecen tablas sin control, "Recalcular dígitos" y después "Verificar". Todo tiene que dar bien.
6. (Opcional) Correr `scripts/ver-usuarios-incompletos.sql` y anotar lo que falta.

## 1. Acceso (como Admin)
7. `/Inicio/Default` sin sesión lleva al login. ★
8. Login con la contraseña mal: mensaje claro. Login correcto: Dashboard sin el panel de check-in. ★
9. Cambiar contraseña con la actual mal: "contraseña incorrecta" (no un texto técnico). Con la actual bien: cierra la sesión y pide entrar con la nueva.

## 2. Permisos y usuarios
10. Permisos: el WebMaster tiene permisos. Editar otro perfil (Recepcionista) y guardar: funciona. Quitarle "Permisos" al Administrador: error. ★
11. Usuarios → Crear, sin contraseña: no aparece "Estado". Al guardar, la ventana muestra el usuario y la contraseña. Probar también un usuario repetido y la contraseña `abc`: mensajes claros. ★
12. Entrenadores → Crear: sin "Estado", con usuario, email y contraseña opcional, y la ventana de credenciales. Modificar: aparece "Estado" y no aparecen los datos de la cuenta. ★

## 3. Alumnos y cobros
13. Alumnos → corregir el peso de los alumnos del paso 3. Probar 2 kg (error) y 70 kg (bien). ★
14. Alumnos → Crear (persona nueva, sin "Estado"): si se generó una contraseña, primero se ve esa ventana y después "Primer mes del abono". Elegir clases y efectivo y tocar "Cobrar ahora": el abono queda al día en Pagos. ★
15. Otro alta → "Más tarde": aparece el aviso con "Cobrar primer mes". ★
16. Pagos (admin) → Cobrar a un alumno vencido: queda al día.
17. Pagos (admin) → editar el alias (por ejemplo `gym.sportio.mp`) y tocar "Guardar alias". Probar `ab` y `con espacio`: error. Guardar el mismo alias otra vez: "ya es el actual". ★

## 4. Cliente
18. Login como cliente → Pagos → Pagar → Tarjeta: el número y el código solo aceptan dígitos, y el vencimiento pone la barra solo. Pagar con `4111 1111 1111 1111`. ★
19. Con otro cliente → Pagar → Transferencia: se ve el **alias nuevo del paso 17**, se oculta la tarjeta y el botón dice "Informar transferencia". Después: aviso de pendiente, sin botón "Pagar". ★
20. Perfil → cargar un peso válido y uno inválido.
21. Rutinas → "Ver ejercicios" en una rutina asignada (solo lectura).
22. Entrar por URL a `/Permisos/Permisos.aspx`: Acceso denegado.

## 5. De vuelta como Admin
23. Pagos → "Transferencias a confirmar": aparece la del paso 19. "Confirmar": el abono del cliente queda al día. ★
24. Informar otra transferencia y rechazarla: se abre la ventana, tocar "Sí, rechazar" y la transferencia desaparece. ★
25. Registro público (desde el login) con Transferencia: la cuenta se crea, se ve el alias y la transferencia aparece en Pagos para confirmar. ★
26. Actividades, Aulas, Rutinas (agregar un ejercicio), Cálculo de RM y Precios de cuota (cambiar un precio y ver el banner): abrir cada una y hacer una acción.
27. Idioma → inglés: recorrer Pagos, Alumnos y Usuarios. Volver a español.
28. Bitácora: aparecen las acciones de hoy (pagos, transferencias, alias, altas). "Exportar CSV" con un filtro y abrirlo en Excel.
29. VerificacioDV → Verificar: **todo bien**.
30. Backup final, para llevar a la entrega.

## Si algo falla en la demo
- Si aparece una pantalla de error de ASP.NET, anotá la URL y el mensaje.
- Si es algo con ★ y no hay tiempo de arreglarlo, mostrá esa parte con datos ya cargados o saltéala.
- Antes de la demo, restaurá el backup del paso 30.
