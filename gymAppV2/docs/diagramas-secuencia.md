# Diagramas de secuencia - gymAppV2

> Generados a partir del código fuente de `C:\Users\Danunu\Desktop\WEB-4to\gymAppV2`.

---

## 1. Login de usuario

```mermaid
sequenceDiagram
    autonumber
    actor U as Usuario
    participant P as Página Web Forms
    participant B as BLLUsuario
    participant M as MPPUsuario
    participant D as DalGeneral
    participant C as CriptoManager
    participant S as Singleton / Sesión
    participant E as BLLEvento

    U->>P: Ingresa usuario y contraseña
    P->>B: ValidarLogin(usuario, contrasena)

    alt campos vacíos
        B-->>P: Excepción (usuario/contraseña inválido)
        P-->>U: Mensaje de error
    else
        B->>M: ObtenerUsuario(usuario)
        M->>D: SELECT USUARIOS WHERE usr
        D-->>M: DataRow usuario
        M-->>B: BE.Usuario (datos desencriptados)

        alt usuario inactivo o no existe
            B-->>P: Excepción InvalidUsername
            P-->>U: Mensaje genérico
        else
            B->>M: UsuarioEstaBloqueado(usuario)
            M->>D: SELECT bloqueado
            D-->>M: bool
            M-->>B: bool

            alt bloqueado
                B->>E: RegistrarBloqueoUsuario(usuario)
                B-->>P: Excepción AccountLocked
                P-->>U: Redirige a preguntas de seguridad
            else
                B->>C: GenerarHashSHA256(contrasena)
                C-->>B: hashIngresado
                B->>M: ObtenerContrasena(usuario)
                M->>D: SELECT contra
                D-->>M: hashAlmacenado
                M-->>B: hashAlmacenado

                alt contraseña correcta
                    B->>M: ReestablecerIntentos(usuario)
                    M->>D: UPDATE intentos=0, bloqueado=0
                    B->>S: LogearUsuario(usuario)
                    S-->>B: OK
                    B->>E: RegistrarLogin(usuario)
                    B-->>P: true
                    P-->>U: Dashboard / primer login
                else
                    B->>B: RegistrarIntentoFallido(usuario)
                    B->>M: AgregarIntento(usuario)
                    M->>D: UPDATE intentos + 1
                    alt supera MAX_INTENTOS_LOGIN
                        B->>E: RegistrarBloqueoUsuario(usuario)
                        B-->>P: Excepción AccountLocked
                    else
                        B-->>P: Excepción InvalidPassword
                    end
                    P-->>U: Mensaje de error
                end
            end
        end
    end
```

---

## 2. Registro en bitácora (general)

```mermaid
sequenceDiagram
    autonumber
    actor O as Operador / Sistema
    participant B as BLL negocio
    participant E as BLLEvento
    participant M as MPPEvento
    participant D as DalGeneral

    O->>B: Ejecuta acción (ej: Alta alumno)
    B->>E: RegistrarEvento(tipo, usuario, acción, criticidad, modulo)

    E->>E: Validar criticidad entre 1 y 4
    E->>E: Validar usuario no vacío ni "sistema" no autorizado

    alt evento post-autenticación sin sesión activa
        E-->>B: Error "No hay sesión activa"
        B-->>O: Propaga excepción
    else
        E->>E: Crear BE.Evento con timestamp truncado a segundos
        E->>M: RegistrarEvento(evento, criticidad)
        M->>M: Calcular DVH/DVV del evento
        M->>D: INSERT INTO Evento (... dvv, dvh) + SCOPE_IDENTITY()
        D-->>M: codEvento generado
        M-->>E: codEvento
        E-->>B: OK
    end

    opt consulta de bitácora
        O->>E: ObtenerEventos(filtro, búsqueda, criticidad, modulo)
        E->>M: ObtenerEventos(...)
        M->>D: SELECT Evento con filtros
        D-->>M: DataTable eventos
        M-->>E: List~Evento~
        E-->>O: Eventos mostrados
    end
```

---

## 3. Backup y Restore (general)

```mermaid
sequenceDiagram
    autonumber
    actor A as Administrador
    participant UI as Página Admin
    participant M as MPPDigitoVerificador
    participant D as DalGeneral / SQL Server
    participant FS as Sistema de archivos

    rect rgb(240,248,255)
        Note over A,FS: FLUJO DE BACKUP
        A->>UI: Solicita backup y selecciona ruta .bak
        UI->>M: RealizarBackup(rutaDestino)
        M->>M: Validar extensión .bak y directorio
        M->>D: Conecta a master (GymAppConnection modificado)
        M->>D: BACKUP DATABASE [GymApp] TO DISK = @Ruta WITH INIT
        D->>FS: Escribe archivo .bak
        FS-->>D: Confirmación
        D-->>M: OK
        M-->>UI: Backup completado
        UI-->>A: Mensaje de éxito
    end

    rect rgb(255,248,240)
        Note over A,FS: FLUJO DE RESTORE
        A->>UI: Solicita restore y selecciona archivo .bak
        UI->>M: RestaurarBackup(rutaBackup)
        M->>M: Validar ruta y existencia del archivo
        M->>D: Conecta a master
        M->>D: RESTORE FILELISTONLY FROM DISK
        D-->>M: Nombres lógicos data/log
        M->>D: Limpiar connection pools + SINGLE_USER
        M->>D: RESTORE DATABASE con MOVE, REPLACE
        D->>FS: Sobrescribe archivos .mdf / .ldf
        FS-->>D: Confirmación
        M->>D: MULTI_USER
        D-->>M: OK
        M-->>UI: Restore completado
        UI-->>A: Mensaje de éxito
    end
```

---

## 4. Control de Dígitos Verificadores (DVH/DVV) - Verificación general

```mermaid
sequenceDiagram
    autonumber
    actor A as Administrador
    participant UI as Página Verificación DV
    participant M as MPPDigitoVerificador
    participant MS as MPP especializado (Usuario/Evento/Pregunta)
    participant DV as DigitoVerificadorManager
    participant D as DalGeneral
    participant DB as Base de datos

    A->>UI: Solicita verificar integridad
    UI->>M: VerificarIntegridadGlobal()

    M->>D: SELECT tablas registradas en DigitoVerificador
    D->>DB: SELECT nombreTabla FROM DigitoVerificador
    DB-->>D: lista de tablas
    D-->>M: List~string~

    loop por cada tabla con control
        M->>M: VerificarIntegridadTabla(nombreTabla)
        M->>D: Obtener control de la tabla
        D->>DB: SELECT dvhTabla, dvvTabla FROM DigitoVerificador WHERE nombreTabla
        DB-->>D: hashes almacenados
        D-->>M: DataRow control

        M->>D: SELECT * FROM tabla
        D->>DB: lectura de filas
        DB-->>D: DataTable filas
        D-->>M: DataTable

        M->>M: CalcularHashTabla(...)
        Note over M,DV: Concatena todos los dvh/dvv de las filas y los hashea
        M->>DV: GenerarHashSHA256(dvhConcat) / GenerarHashSHA256(dvvConcat)
        DV-->>M: dvhTablaCalculado, dvvTablaCalculado

        alt hash de tabla coincide
            M-->>UI: ResultadoVerificacionDV válido
        else no coincide
            M->>M: VerificarFilasTabla(nombreTabla)
            loop por cada fila corrupta
                M->>DV: CalcularAmbos(valores fila)
                DV-->>M: dvhCalculado, dvvCalculado
                alt no coinciden
                    M-->>UI: ResultadoVerificacionDV inválido por fila/campo
                end
            end
            MS-->>M: Resultados específicos (para tablas encriptadas)
        end
    end

    M-->>UI: List~ResultadoVerificacionDV~ completo
    UI-->>A: Muestra estado de integridad
```

---

## 5. Control de Dígitos Verificadores (DVH/DVV) - Recálculo general

```mermaid
sequenceDiagram
    autonumber
    actor A as Administrador
    participant UI as Página Recalcular DV
    participant M as MPPDigitoVerificador
    participant MS as MPP especializado (MPPUsuario, MPPEvento, etc.)
    participant DV as DigitoVerificadorManager
    participant D as DalGeneral
    participant DB as Base de datos

    A->>UI: Solicita recalcular dígitos verificadores
    UI->>M: RecalcularDigitosGlobal()

    M->>D: SELECT tablas con control
    D->>DB: SELECT nombreTabla FROM DigitoVerificador
    DB-->>D: lista de tablas
    D-->>M: List~string~

    loop por cada tabla
        M->>M: RecalcularDigitosTabla(nombreTabla)

        alt tabla con datos encriptados
            M->>MS: RecalcularDigitosTodos...() / VerificarIntegridad...()
            MS->>D: SELECT filas
            D->>DB: lectura
            DB-->>D: DataTable
            D-->>MS: filas
            MS->>DV: CalcularAmbos(valores en texto plano)
            DV-->>MS: dvh, dvv por fila
            MS->>D: UPDATE dvh/dvv por fila
            D->>DB: UPDATE
            DB-->>D: OK
            D-->>MS: OK
        else tabla genérica
            M->>D: SELECT * FROM tabla
            D->>DB: lectura de filas
            DB-->>D: DataTable
            D-->>M: DataTable
            loop por cada fila
                M->>DV: CalcularAmbos(valores fila)
                DV-->>M: dvh, dvv
                M->>D: UPDATE dvh/dvv por clave primaria
                D->>DB: UPDATE fila
                DB-->>D: OK
            end
        end

        M->>M: Calcular hash de tabla
        M->>DV: GenerarHashSHA256(dvhConcat) / GenerarHashSHA256(dvvConcat)
        DV-->>M: dvhTabla, dvvTabla
        M->>D: GuardarControl(nombreTabla, dvhTabla, dvvTabla)
        D->>DB: UPDATE/INSERT DigitoVerificador
        DB-->>D: OK
        D-->>M: OK
    end

    M-->>UI: Recálculo completado
    UI-->>A: Mensaje de éxito
```

---

## Notas

- Los diagramas son **simples y generales**: se omiten detalles de excepciones secundarias, logging de fallback y casos extremos para facilitar la comprensión del flujo principal.
- En los diagramas de DVH/DVV se destaca que las tablas con datos encriptados (`USUARIOS`, `PreguntasSeguridad`, `Evento`) requieren que el MPP especializado desencripte los valores antes de calcular los dígitos verificadores.
- El flujo de login muestra el camino completo: validación, bloqueo, intentos fallidos, hash de contraseña y registro de sesión.
