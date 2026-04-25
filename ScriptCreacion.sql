-- =============================================
-- GYM APP - Script de creación de base de datos
-- SQL Server
-- =============================================

USE master;
GO

CREATE DATABASE GymApp;
GO

USE GymApp;
GO

-- =============================================
-- USUARIOS
-- =============================================

CREATE TABLE USUARIOS (
    usr         VARCHAR(50)     NOT NULL,
    contra      VARCHAR(255)    NOT NULL,
    dvv         VARCHAR(50)     NOT NULL,
    dvh         VARCHAR(50)     NOT NULL,
    CONSTRAINT PK_USUARIOS PRIMARY KEY (usr)
);

CREATE TABLE USUARIO_Intentos (
    usr         VARCHAR(50)     NOT NULL,
    intentos    INT             NOT NULL DEFAULT 0,
    dvv         VARCHAR(50)     NOT NULL,
    dvh         VARCHAR(50)     NOT NULL,
    CONSTRAINT PK_USUARIO_Intentos PRIMARY KEY (usr),
    CONSTRAINT FK_UsuarioIntentos_Usuario FOREIGN KEY (usr) REFERENCES USUARIOS(usr)
);

CREATE TABLE USUARIO_Contras (
    usr         VARCHAR(50)     NOT NULL,
    contra      VARCHAR(255)    NOT NULL,
    dvv         VARCHAR(50)     NOT NULL,
    dvh         VARCHAR(50)     NOT NULL,
    CONSTRAINT PK_USUARIO_Contras PRIMARY KEY (usr, contra),
    CONSTRAINT FK_UsuarioContras_Usuario FOREIGN KEY (usr) REFERENCES USUARIOS(usr)
);

-- =============================================
-- PERFILES Y PERMISOS
-- =============================================

CREATE TABLE Familia (
    familiaId       INT             NOT NULL IDENTITY(1,1),
    nombre          VARCHAR(100)    NOT NULL,
    profundidad     BIT             NOT NULL DEFAULT 0,
    dvv             VARCHAR(50)     NOT NULL,
    dvh             VARCHAR(50)     NOT NULL,
    CONSTRAINT PK_Familia PRIMARY KEY (familiaId)
);

CREATE TABLE Permiso (
    permisoId       INT             NOT NULL IDENTITY(1,1),
    nombre          VARCHAR(100)    NOT NULL,
    dvv             VARCHAR(50)     NOT NULL,
    dvh             VARCHAR(50)     NOT NULL,
    CONSTRAINT PK_Permiso PRIMARY KEY (permisoId)
);

CREATE TABLE PermisoFamilia (
    permisoId       INT             NOT NULL,
    familiaId       INT             NOT NULL,
    dvv             VARCHAR(50)     NOT NULL,
    dvh             VARCHAR(50)     NOT NULL,
    CONSTRAINT PK_PermisoFamilia PRIMARY KEY (permisoId, familiaId),
    CONSTRAINT FK_PermisoFamilia_Permiso FOREIGN KEY (permisoId) REFERENCES Permiso(permisoId),
    CONSTRAINT FK_PermisoFamilia_Familia FOREIGN KEY (familiaId) REFERENCES Familia(familiaId)
);

CREATE TABLE Perfiles (
    idPerfil        INT             NOT NULL IDENTITY(1,1),
    nombrePerfil    VARCHAR(100)    NOT NULL,
    dvv             VARCHAR(50)     NOT NULL,
    dvh             VARCHAR(50)     NOT NULL,
    CONSTRAINT PK_Perfiles PRIMARY KEY (idPerfil)
);

CREATE TABLE Perfil_Permiso (
    idPerfil        INT             NOT NULL,
    idPermiso       INT             NOT NULL,
    dvv             VARCHAR(50)     NOT NULL,
    dvh             VARCHAR(50)     NOT NULL,
    CONSTRAINT PK_Perfil_Permiso PRIMARY KEY (idPerfil, idPermiso),
    CONSTRAINT FK_PerfilPermiso_Perfil FOREIGN KEY (idPerfil) REFERENCES Perfiles(idPerfil),
    CONSTRAINT FK_PerfilPermiso_Permiso FOREIGN KEY (idPermiso) REFERENCES Permiso(permisoId)
);

CREATE TABLE Perfil_Familia (
    idPerfil        INT             NOT NULL,
    idFamilia       INT             NOT NULL,
    dvv             VARCHAR(50)     NOT NULL,
    dvh             VARCHAR(50)     NOT NULL,
    CONSTRAINT PK_Perfil_Familia PRIMARY KEY (idPerfil, idFamilia),
    CONSTRAINT FK_PerfilFamilia_Perfil FOREIGN KEY (idPerfil) REFERENCES Perfiles(idPerfil),
    CONSTRAINT FK_PerfilFamilia_Familia FOREIGN KEY (idFamilia) REFERENCES Familia(familiaId)
);

-- =============================================
-- ALUMNOS Y ENTRENADORES
-- =============================================

CREATE TABLE Alumnos (
    dni             INT             NOT NULL,
    nombre          VARCHAR(100)    NOT NULL,
    apellido        VARCHAR(100)    NOT NULL,
    telefono        INT             NULL,
    fechaNacimiento DATE            NOT NULL,
    peso            DECIMAL(5,2)    NULL,
    usr             VARCHAR(50)     NOT NULL,
    dvv             VARCHAR(50)     NOT NULL,
    dvh             VARCHAR(50)     NOT NULL,
    CONSTRAINT PK_Alumnos PRIMARY KEY (dni),
    CONSTRAINT FK_Alumnos_Usuario FOREIGN KEY (usr) REFERENCES USUARIOS(usr),
    CONSTRAINT CK_Alumnos_Peso CHECK (peso IS NULL OR (peso > 0 AND peso < 500))
);

CREATE TABLE Entrenadores (
    dni             INT             NOT NULL,
    nombre          VARCHAR(100)    NOT NULL,
    apellido        VARCHAR(100)    NOT NULL,
    fechaNacimiento DATE            NOT NULL,
    usr             VARCHAR(50)     NOT NULL,
    dvv             VARCHAR(50)     NOT NULL,
    dvh             VARCHAR(50)     NOT NULL,
    CONSTRAINT PK_Entrenadores PRIMARY KEY (dni),
    CONSTRAINT FK_Entrenadores_Usuario FOREIGN KEY (usr) REFERENCES USUARIOS(usr)
);

-- =============================================
-- ACTIVIDADES
-- =============================================

CREATE TABLE Actividades (
    codActividad    INT             NOT NULL IDENTITY(1,1),
    descripcion     VARCHAR(200)    NOT NULL,
    cantXSemana     INT             NOT NULL,
    costo           DECIMAL(10,2)   NOT NULL,
    valor           DECIMAL(10,2)   NOT NULL,
    dvv             VARCHAR(50)     NOT NULL,
    dvh             VARCHAR(50)     NOT NULL,
    CONSTRAINT PK_Actividades PRIMARY KEY (codActividad)
);

CREATE TABLE Actividad_Alumno (
    dni             INT             NOT NULL,
    codActividad    INT             NOT NULL,
    dvv             VARCHAR(50)     NOT NULL,
    dvh             VARCHAR(50)     NOT NULL,
    CONSTRAINT PK_Actividad_Alumno PRIMARY KEY (dni, codActividad),
    CONSTRAINT FK_ActividadAlumno_Alumno FOREIGN KEY (dni) REFERENCES Alumnos(dni),
    CONSTRAINT FK_ActividadAlumno_Actividad FOREIGN KEY (codActividad) REFERENCES Actividades(codActividad)
);

CREATE TABLE Actividad_Entrenador (
    codActividad    INT             NOT NULL,
    dniEntrenador   INT             NOT NULL,
    dvv             VARCHAR(50)     NOT NULL,
    dvh             VARCHAR(50)     NOT NULL,
    CONSTRAINT PK_Actividad_Entrenador PRIMARY KEY (codActividad, dniEntrenador),
    CONSTRAINT FK_ActividadEntrenador_Actividad FOREIGN KEY (codActividad) REFERENCES Actividades(codActividad),
    CONSTRAINT FK_ActividadEntrenador_Entrenador FOREIGN KEY (dniEntrenador) REFERENCES Entrenadores(dni)
);

-- =============================================
-- RUTINAS
-- =============================================

CREATE TABLE Rutinas (
    codRutina       INT             NOT NULL IDENTITY(1,1),
    descripcion     VARCHAR(500)    NOT NULL,
    fecha           DATE            NOT NULL,
    dniAlumno       INT             NOT NULL,
    dniEntrenador   INT             NOT NULL,
    codActividad    INT             NOT NULL,
    dvv             VARCHAR(50)     NOT NULL,
    dvh             VARCHAR(50)     NOT NULL,
    CONSTRAINT PK_Rutinas PRIMARY KEY (codRutina),
    CONSTRAINT FK_Rutinas_Alumno FOREIGN KEY (dniAlumno) REFERENCES Alumnos(dni),
    CONSTRAINT FK_Rutinas_Entrenador FOREIGN KEY (dniEntrenador) REFERENCES Entrenadores(dni),
    CONSTRAINT FK_Rutinas_Actividad FOREIGN KEY (codActividad) REFERENCES Actividades(codActividad)
);

-- =============================================
-- EJERCICIOS DE FUERZA
-- =============================================

CREATE TABLE Ejercicio (
    codEjercicio    INT             NOT NULL IDENTITY(1,1),
    nombre          VARCHAR(100)    NOT NULL,
    grupoMuscular   VARCHAR(100)    NOT NULL,
    descripcion     VARCHAR(500)    NULL,
    dvv             VARCHAR(50)     NOT NULL,
    dvh             VARCHAR(50)     NOT NULL,
    CONSTRAINT PK_Ejercicio PRIMARY KEY (codEjercicio)
);

-- =============================================
-- DETALLE DE EJERCICIOS POR RUTINA
-- =============================================

CREATE TABLE RutinaEjercicio (
    codRutinaEjercicio  INT             NOT NULL IDENTITY(1,1),
    codRutina           INT             NOT NULL,
    codEjercicio        INT             NOT NULL,
    series              INT             NOT NULL,
    repeticiones        INT             NOT NULL,
    pesoLevantado       DECIMAL(6,2)   NOT NULL,    -- kg levantados en cada serie
    porcentaje1RM       DECIMAL(5,2)   NULL,         -- % del 1RM calculado automáticamente por trigger
    descansoSegundos    INT             NULL,         -- descanso entre series en segundos
    dvv                 VARCHAR(50)     NOT NULL,
    dvh                 VARCHAR(50)     NOT NULL,
    CONSTRAINT PK_RutinaEjercicio PRIMARY KEY (codRutinaEjercicio),
    CONSTRAINT FK_RutinaEjercicio_Rutina FOREIGN KEY (codRutina) REFERENCES Rutinas(codRutina),
    CONSTRAINT FK_RutinaEjercicio_Ejercicio FOREIGN KEY (codEjercicio) REFERENCES Ejercicio(codEjercicio),
    CONSTRAINT CK_RutinaEjercicio_Series CHECK (series > 0),
    CONSTRAINT CK_RutinaEjercicio_Repeticiones CHECK (repeticiones > 0),
    CONSTRAINT CK_RutinaEjercicio_Peso CHECK (pesoLevantado >= 0),
    CONSTRAINT CK_RutinaEjercicio_Porcentaje CHECK (porcentaje1RM IS NULL OR (porcentaje1RM > 0 AND porcentaje1RM <= 100)),
    CONSTRAINT CK_RutinaEjercicio_Descanso CHECK (descansoSegundos IS NULL OR descansoSegundos > 0),
    volumen AS (series * repeticiones * pesoLevantado)  -- columna calculada: volumen = series × reps × peso
);

-- =============================================
-- REGISTRO DE 1RM (REPETICION MÁXIMA) POR ALUMNO
-- =============================================

CREATE TABLE AlumnoRM (
    codRM           INT             NOT NULL IDENTITY(1,1),
    dni             INT             NOT NULL,
    codEjercicio    INT             NOT NULL,
    pesoRM          DECIMAL(6,2)    NOT NULL,         -- peso máximo levantado para 1 rep
    fechaRM         DATE            NOT NULL,         -- fecha en que se registró el 1RM
    dvv             VARCHAR(50)     NOT NULL,
    dvh             VARCHAR(50)     NOT NULL,
    CONSTRAINT PK_AlumnoRM PRIMARY KEY (codRM),
    CONSTRAINT FK_AlumnoRM_Alumno FOREIGN KEY (dni) REFERENCES Alumnos(dni),
    CONSTRAINT FK_AlumnoRM_Ejercicio FOREIGN KEY (codEjercicio) REFERENCES Ejercicio(codEjercicio),
    CONSTRAINT CK_AlumnoRM_Peso CHECK (pesoRM > 0)
);

-- =============================================
-- HISTORIAL DE PESO DEL ALUMNO
-- =============================================

CREATE TABLE PesoHistorial (
    codPeso         INT             NOT NULL IDENTITY(1,1),
    dni             INT             NOT NULL,
    peso            DECIMAL(5,2)    NOT NULL,
    fecha           DATE            NOT NULL,
    dvv             VARCHAR(50)     NOT NULL,
    dvh             VARCHAR(50)     NOT NULL,
    CONSTRAINT PK_PesoHistorial PRIMARY KEY (codPeso),
    CONSTRAINT FK_PesoHistorial_Alumno FOREIGN KEY (dni) REFERENCES Alumnos(dni),
    CONSTRAINT CK_PesoHistorial_Peso CHECK (peso > 0 AND peso < 500)
);

-- =============================================
-- BITÁCORA / EVENTO
-- =============================================

CREATE TABLE Evento (
    codEvento       INT             NOT NULL IDENTITY(1,1),
    usr             VARCHAR(50)     NOT NULL,
    fecha           DATETIME        NOT NULL DEFAULT GETDATE(),
    modulo          VARCHAR(100)    NOT NULL,
    descripcion     VARCHAR(500)    NOT NULL,
    criticidad      INT             NOT NULL,
    dvv             VARCHAR(50)     NOT NULL,
    dvh             VARCHAR(50)     NOT NULL,
    CONSTRAINT PK_Evento PRIMARY KEY (codEvento),
    CONSTRAINT FK_Evento_Usuario FOREIGN KEY (usr) REFERENCES USUARIOS(usr),
    CONSTRAINT CK_Evento_Criticidad CHECK (criticidad IN (1, 2, 3, 4))
    -- 1=Info, 2=Advertencia, 3=Error, 4=Crítico
);

GO

-- =============================================
-- ÍNDICES RECOMENDADOS
-- =============================================

CREATE INDEX IX_Alumnos_usr ON Alumnos(usr);
CREATE INDEX IX_Entrenadores_usr ON Entrenadores(usr);
CREATE INDEX IX_Actividad_Alumno_codActividad ON Actividad_Alumno(codActividad);
CREATE INDEX IX_Rutinas_dniAlumno ON Rutinas(dniAlumno);
CREATE INDEX IX_Rutinas_dniEntrenador ON Rutinas(dniEntrenador);
CREATE INDEX IX_Evento_usr ON Evento(usr);
CREATE INDEX IX_Evento_fecha ON Evento(fecha);
CREATE INDEX IX_RutinaEjercicio_Rutina ON RutinaEjercicio(codRutina);
CREATE INDEX IX_RutinaEjercicio_Ejercicio ON RutinaEjercicio(codEjercicio);
CREATE INDEX IX_AlumnoRM_Alumno ON AlumnoRM(dni);
CREATE INDEX IX_AlumnoRM_Ejercicio ON AlumnoRM(codEjercicio);
CREATE INDEX IX_AlumnoRM_AlumnoEjercicio ON AlumnoRM(dni, codEjercicio);
CREATE INDEX IX_PesoHistorial_Alumno ON PesoHistorial(dni);
CREATE INDEX IX_PesoHistorial_Fecha ON PesoHistorial(fecha);

GO

PRINT 'GymApp - Base de datos creada exitosamente.';

-- =============================================
-- FUNCIONES ESCALARES
-- =============================================

-- Fórmula de Epley: 1RM = peso × (1 + repeticiones / 30)
-- Calcula la repetición máxima estimada a partir del peso levantado y las repeticiones realizadas.
CREATE FUNCTION fn_Calcular1RM(
    @peso       DECIMAL(6,2),
    @repeticiones INT
)
RETURNS DECIMAL(6,2)
AS
BEGIN
    DECLARE @rm DECIMAL(6,2);

    IF @repeticiones = 1
        SET @rm = @peso;
    ELSE IF @repeticiones > 1 AND @repeticiones <= 30
        SET @rm = ROUND(@peso * (1.0 + CAST(@repeticiones AS DECIMAL(6,2)) / 30.0), 2);
    ELSE
        SET @rm = NULL;

    RETURN @rm;
END;
GO

-- Obtiene el 1RM registrado más reciente para un alumno y ejercicio.
-- Si no existe registro, devuelve NULL (se puede usar COALESCE con fn_Calcular1RM como fallback).
CREATE FUNCTION fn_Obtener1RMAlumno(
    @dni           INT,
    @codEjercicio  INT
)
RETURNS DECIMAL(6,2)
AS
BEGIN
    DECLARE @pesoRM DECIMAL(6,2);

    SELECT TOP 1 @pesoRM = pesoRM
    FROM AlumnoRM
    WHERE dni = @dni AND codEjercicio = @codEjercicio
    ORDER BY fechaRM DESC;

    RETURN @pesoRM;
END;
GO

-- =============================================
-- TRIGGERS
-- =============================================

-- Calcula porcentaje1RM automáticamente al insertar o actualizar una RutinaEjercicio.
-- Busca el 1RM registrado del alumno (vía Rutinas → dniAlumno) y el ejercicio.
-- Si no hay 1RM registrado, estima usando la fórmula de Epley con los datos de la serie.
CREATE TRIGGER trg_RutinaEjercicio_Porcentaje1RM
ON RutinaEjercicio
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Solo procesar si pesoLevantado o codEjercicio cambiaron
    IF UPDATE(pesoLevantado) OR UPDATE(codEjercicio)
    BEGIN
        UPDATE re
        SET re.porcentaje1RM = CASE
            WHEN rm.rm1RM IS NOT NULL AND rm.rm1RM > 0
                THEN ROUND((i.pesoLevantado / rm.rm1RM) * 100, 2)
            ELSE
                -- Sin 1RM registrado: estimar con Epley y calcular % sobre ese estimado
                -- Si el alumno hizo X reps con Y kg, su 1RM estimado = Y*(1+X/30)
                -- El porcentaje de ese 1RM estimado es: Y / [Y*(1+X/30)] * 100
                -- Simplificado: 100 / (1 + X/30)
                ROUND(100.0 / (1.0 + CAST(r.repeticiones AS DECIMAL(6,2)) / 30.0), 2)
        END
        FROM RutinaEjercicio re
        INNER JOIN inserted i ON re.codRutinaEjercicio = i.codRutinaEjercicio
        INNER JOIN Rutinas r ON i.codRutina = r.codRutina
        CROSS APPLY (
            SELECT dbo.fn_Obtener1RMAlumno(r.dniAlumno, i.codEjercicio) AS rm1RM
        ) AS rm
        WHERE i.pesoLevantado > 0;
    END
END;
GO

-- Actualiza Alumnos.peso con el último registro de PesoHistorial
-- cada vez que se inserta un nuevo peso.
CREATE TRIGGER trg_PesoHistorial_ActualizarPesoAlumno
ON PesoHistorial
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE a
    SET a.peso = i.peso
    FROM Alumnos a
    INNER JOIN inserted i ON a.dni = i.dni;
END;
GO

-- Actualiza AlumnoRM: al registrar un nuevo 1RM, recalcula el porcentaje1RM
-- de todas las RutinaEjercicio del alumno para ese ejercicio.
CREATE TRIGGER trg_AlumnoRM_RecalcularPorcentaje
ON AlumnoRM
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE re
    SET re.porcentaje1RM = ROUND((re.pesoLevantado / i.pesoRM) * 100, 2)
    FROM RutinaEjercicio re
    INNER JOIN Rutinas r ON re.codRutina = r.codRutina
    INNER JOIN inserted i ON r.dniAlumno = i.dni AND re.codEjercicio = i.codEjercicio
    WHERE re.pesoLevantado > 0
      AND i.pesoRM > 0;
END;
GO