/* ==============================
   TABLAS BASE
   ============================== */

IF OBJECT_ID('dbo.Roles','U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles (
        RolID INT IDENTITY(1,1) CONSTRAINT PK_Roles PRIMARY KEY,
        NombreRol NVARCHAR(50) NOT NULL UNIQUE
    );
END
GO

IF OBJECT_ID('dbo.Usuarios','U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios (
        UsuarioID INT IDENTITY(1,1) CONSTRAINT PK_Usuarios PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Apellido NVARCHAR(100) NOT NULL,
        CorreoElectronico NVARCHAR(100) NOT NULL,
        ContrasenaHash NVARCHAR(255) NOT NULL,
        FechaRegistro DATETIME2 DEFAULT SYSUTCDATETIME(),
        RolID INT NOT NULL,
        Activo BIT NOT NULL CONSTRAINT DF_Usuarios_Activo DEFAULT (1),
        RowVer ROWVERSION,
        CONSTRAINT UQ_Usuarios_Correo UNIQUE (CorreoElectronico),
        CONSTRAINT FK_Usuarios_Roles FOREIGN KEY (RolID) REFERENCES dbo.Roles(RolID)
    );
    CREATE INDEX IX_Usuarios_RolID ON dbo.Usuarios(RolID);
END
GO

IF OBJECT_ID('dbo.PerfilesCliente','U') IS NULL
BEGIN
    CREATE TABLE dbo.PerfilesCliente (
        PerfilID INT IDENTITY(1,1) CONSTRAINT PK_PerfilesCliente PRIMARY KEY,
        UsuarioID INT NOT NULL UNIQUE,
        FechaNacimiento DATE NULL,
        Telefono NVARCHAR(20) NULL,
        DireccionEntrega NVARCHAR(255) NULL,
        CONSTRAINT FK_PerfilesCliente_Usuarios FOREIGN KEY (UsuarioID) REFERENCES dbo.Usuarios(UsuarioID) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID('dbo.RestriccionesAlimentarias','U') IS NULL
BEGIN
    CREATE TABLE dbo.RestriccionesAlimentarias (
        RestriccionID INT IDENTITY(1,1) CONSTRAINT PK_Restricciones PRIMARY KEY,
        UsuarioID INT NOT NULL,
        Descripcion NVARCHAR(MAX) NOT NULL,
        CONSTRAINT FK_Restricciones_Usuarios FOREIGN KEY (UsuarioID) REFERENCES dbo.Usuarios(UsuarioID) ON DELETE CASCADE
    );
    CREATE INDEX IX_Restricciones_UsuarioID ON dbo.RestriccionesAlimentarias(UsuarioID);
END
GO

IF OBJECT_ID('dbo.Recetarios','U') IS NULL
BEGIN
    CREATE TABLE dbo.Recetarios (
        RecetarioID INT IDENTITY(1,1) CONSTRAINT PK_Recetarios PRIMARY KEY,
        Titulo NVARCHAR(150) NOT NULL,
        Descripcion NVARCHAR(MAX) NULL,
        Precio DECIMAL(10,2) NULL,
        FechaPublicacion DATE NULL
    );
    CREATE UNIQUE INDEX UX_Recetarios_Titulo ON dbo.Recetarios(Titulo);
END
GO

IF OBJECT_ID('dbo.Recetas','U') IS NULL
BEGIN
    CREATE TABLE dbo.Recetas (
        RecetaID INT IDENTITY(1,1) CONSTRAINT PK_Recetas PRIMARY KEY,
        Titulo NVARCHAR(150) NOT NULL,
        Descripcion NVARCHAR(MAX) NULL,
        Instrucciones NVARCHAR(MAX) NULL,
        EsGratuita BIT NOT NULL CONSTRAINT DF_Recetas_Gratuita DEFAULT (1),
        EsParteDeEbook BIT NOT NULL CONSTRAINT DF_Recetas_Ebook DEFAULT (0),
        ChefID INT NOT NULL,
        RecetarioID INT NULL,
        RowVer ROWVERSION,
        CONSTRAINT FK_Recetas_Usuarios FOREIGN KEY (ChefID) REFERENCES dbo.Usuarios(UsuarioID),
        CONSTRAINT FK_Recetas_Recetarios FOREIGN KEY (RecetarioID) REFERENCES dbo.Recetarios(RecetarioID)
    );
    CREATE UNIQUE INDEX UX_Recetas_Titulo ON dbo.Recetas(Titulo);
    CREATE INDEX IX_Recetas_ChefID ON dbo.Recetas(ChefID);
    CREATE INDEX IX_Recetas_RecetarioID ON dbo.Recetas(RecetarioID);
END
GO

IF OBJECT_ID('dbo.ComentariosRecetas','U') IS NULL
BEGIN
    CREATE TABLE dbo.ComentariosRecetas (
        ComentarioID INT IDENTITY(1,1) CONSTRAINT PK_ComentariosRecetas PRIMARY KEY,
        RecetaID INT NOT NULL,
        UsuarioID INT NOT NULL,
        Comentario NVARCHAR(500) NOT NULL,
        FechaComentario DATETIME2 DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Comentarios_Recetas FOREIGN KEY (RecetaID) REFERENCES dbo.Recetas(RecetaID) ON DELETE CASCADE,
        CONSTRAINT FK_Comentarios_Usuarios FOREIGN KEY (UsuarioID) REFERENCES dbo.Usuarios(UsuarioID) ON DELETE CASCADE
    );
    CREATE INDEX IX_Comentarios_RecetaID ON dbo.ComentariosRecetas(RecetaID);
    CREATE INDEX IX_Comentarios_UsuarioID ON dbo.ComentariosRecetas(UsuarioID);
END
GO

IF OBJECT_ID('dbo.RecetasFavoritas','U') IS NULL
BEGIN
    CREATE TABLE dbo.RecetasFavoritas (
        RecetaFavoritaID INT IDENTITY(1,1) CONSTRAINT PK_RecetasFavoritas PRIMARY KEY,
        UsuarioID INT NOT NULL,
        RecetaID INT NOT NULL,
        FechaGuardado DATETIME2 DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Favoritas_Usuario FOREIGN KEY (UsuarioID) REFERENCES dbo.Usuarios(UsuarioID) ON DELETE CASCADE,
        CONSTRAINT FK_Favoritas_Receta FOREIGN KEY (RecetaID) REFERENCES dbo.Recetas(RecetaID) ON DELETE CASCADE,
        CONSTRAINT UQ_RecetaFavorita UNIQUE (UsuarioID, RecetaID)
    );
    CREATE INDEX IX_Favoritas_Usuario ON dbo.RecetasFavoritas(UsuarioID);
    CREATE INDEX IX_Favoritas_Receta ON dbo.RecetasFavoritas(RecetaID);
END
GO

IF OBJECT_ID('dbo.TiposMenu','U') IS NULL
BEGIN
    CREATE TABLE dbo.TiposMenu (
        TipoMenuID INT IDENTITY(1,1) CONSTRAINT PK_TiposMenu PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(MAX) NULL
    );
    CREATE UNIQUE INDEX UX_TiposMenu_Nombre ON dbo.TiposMenu(Nombre);
END
GO

IF OBJECT_ID('dbo.MenusSemanales','U') IS NULL
BEGIN
    CREATE TABLE dbo.MenusSemanales (
        MenuSemanalID INT IDENTITY(1,1) CONSTRAINT PK_MenusSemanales PRIMARY KEY,
        TipoMenuID INT NOT NULL,
        SemanaDel DATE NOT NULL,
        Descripcion NVARCHAR(MAX) NULL,
        RowVer ROWVERSION,
        CONSTRAINT FK_Menus_Tipos FOREIGN KEY (TipoMenuID) REFERENCES dbo.TiposMenu(TipoMenuID)
    );
    CREATE UNIQUE INDEX UX_Menus_Tipo_Semana ON dbo.MenusSemanales(TipoMenuID, SemanaDel);
    CREATE INDEX IX_Menus_Tipo ON dbo.MenusSemanales(TipoMenuID);
END
GO

IF OBJECT_ID('dbo.PlanesNutricionales','U') IS NULL
BEGIN
    CREATE TABLE dbo.PlanesNutricionales (
        PlanID INT IDENTITY(1,1) CONSTRAINT PK_Planes PRIMARY KEY,
        UsuarioID INT NOT NULL,
        NombrePlan NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(MAX) NULL,
        FechaCarga DATETIME2 DEFAULT SYSUTCDATETIME(),
        FechaVencimiento DATE NULL,
        DocumentoURL NVARCHAR(255) NULL,
        RowVer ROWVERSION,
        CONSTRAINT FK_Planes_Usuarios FOREIGN KEY (UsuarioID) REFERENCES dbo.Usuarios(UsuarioID) ON DELETE CASCADE
    );
    CREATE INDEX IX_Planes_UsuarioID ON dbo.PlanesNutricionales(UsuarioID);
    CREATE UNIQUE INDEX UX_Planes_Usuario_Nombre ON dbo.PlanesNutricionales(UsuarioID, NombrePlan);
END
GO

IF OBJECT_ID('dbo.MenusPorPlan','U') IS NULL
BEGIN
    CREATE TABLE dbo.MenusPorPlan (
        MenuID INT IDENTITY(1,1) CONSTRAINT PK_MenusPorPlan PRIMARY KEY,
        PlanID INT NOT NULL,
        DiaSemana NVARCHAR(20) NOT NULL,
        Descripcion NVARCHAR(MAX) NULL,
        CONSTRAINT FK_MenusPorPlan_Planes FOREIGN KEY (PlanID) REFERENCES dbo.PlanesNutricionales(PlanID) ON DELETE CASCADE
    );
    CREATE INDEX IX_MenusPorPlan_PlanID ON dbo.MenusPorPlan(PlanID);
END
GO

IF OBJECT_ID('dbo.Pedidos','U') IS NULL
BEGIN
    CREATE TABLE dbo.Pedidos (
        PedidoID INT IDENTITY(1,1) CONSTRAINT PK_Pedidos PRIMARY KEY,
        UsuarioID INT NOT NULL,
        FechaPedido DATETIME2 DEFAULT SYSUTCDATETIME(),
        DireccionEntrega NVARCHAR(255) NOT NULL,
        EstadoPedido NVARCHAR(50) NOT NULL, -- Creado, Pagado, EnPreparacion, Enviado, Entregado, Cancelado
        MontoTotal DECIMAL(10, 2) NOT NULL CONSTRAINT DF_Pedidos_Total DEFAULT (0),
        SuscripcionPausada BIT NOT NULL CONSTRAINT DF_Pedidos_SusP DEFAULT (0),
        RowVer ROWVERSION,
        CONSTRAINT FK_Pedidos_Usuarios FOREIGN KEY (UsuarioID) REFERENCES dbo.Usuarios(UsuarioID)
    );
    CREATE INDEX IX_Pedidos_UsuarioID ON dbo.Pedidos(UsuarioID);
    CREATE INDEX IX_Pedidos_Estado ON dbo.Pedidos(EstadoPedido);
END
GO

IF OBJECT_ID('dbo.DetallePedido','U') IS NULL
BEGIN
    CREATE TABLE dbo.DetallePedido (
        DetalleID INT IDENTITY(1,1) CONSTRAINT PK_DetallePedido PRIMARY KEY,
        PedidoID INT NOT NULL,
        MenuSemanalID INT NOT NULL,
        Cantidad INT NOT NULL,
        PrecioUnitario DECIMAL(10, 2) NOT NULL,
        CONSTRAINT FK_Detalle_Pedido FOREIGN KEY (PedidoID) REFERENCES dbo.Pedidos(PedidoID) ON DELETE CASCADE,
        CONSTRAINT FK_Detalle_Menu FOREIGN KEY (MenuSemanalID) REFERENCES dbo.MenusSemanales(MenuSemanalID)
    );
    CREATE INDEX IX_Detalle_PedidoID ON dbo.DetallePedido(PedidoID);
    CREATE INDEX IX_Detalle_MenuID ON dbo.DetallePedido(MenuSemanalID);
END
GO

IF OBJECT_ID('dbo.HistorialPedidos','U') IS NULL
BEGIN
    CREATE TABLE dbo.HistorialPedidos (
        HistorialID INT IDENTITY(1,1) CONSTRAINT PK_HistorialPedidos PRIMARY KEY,
        PedidoID INT NOT NULL,
        FechaEstado DATETIME2 DEFAULT SYSUTCDATETIME(),
        Estado NVARCHAR(50) NOT NULL,
        CONSTRAINT FK_Historial_Pedido FOREIGN KEY (PedidoID) REFERENCES dbo.Pedidos(PedidoID) ON DELETE CASCADE
    );
    CREATE INDEX IX_Historial_PedidoID ON dbo.HistorialPedidos(PedidoID);
END
GO

IF OBJECT_ID('dbo.PuntosUsuario','U') IS NULL
BEGIN
    CREATE TABLE dbo.PuntosUsuario (
        PuntoID INT IDENTITY(1,1) CONSTRAINT PK_PuntosUsuario PRIMARY KEY,
        UsuarioID INT NOT NULL,
        PuntosAcumulados INT NOT NULL CONSTRAINT DF_Puntos_0 DEFAULT (0),
        FechaActualizacion DATETIME2 DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Puntos_Usuario FOREIGN KEY (UsuarioID) REFERENCES dbo.Usuarios(UsuarioID) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX UX_Puntos_Usuario ON dbo.PuntosUsuario(UsuarioID);
END
GO

IF OBJECT_ID('dbo.EncuestasSatisfaccion','U') IS NULL
BEGIN
    CREATE TABLE dbo.EncuestasSatisfaccion (
        EncuestaID INT IDENTITY(1,1) CONSTRAINT PK_Encuestas PRIMARY KEY,
        UsuarioID INT NOT NULL,
        Calificacion INT NOT NULL,
        Sugerencias NVARCHAR(MAX) NULL,
        FechaEncuesta DATETIME2 DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Encuestas_Usuario FOREIGN KEY (UsuarioID) REFERENCES dbo.Usuarios(UsuarioID) ON DELETE CASCADE
    );
    CREATE INDEX IX_Encuestas_UsuarioID ON dbo.EncuestasSatisfaccion(UsuarioID);
END
GO

IF OBJECT_ID('dbo.Notificaciones','U') IS NULL
BEGIN
    CREATE TABLE dbo.Notificaciones (
        NotificacionID INT IDENTITY(1,1) CONSTRAINT PK_Notificaciones PRIMARY KEY,
        UsuarioID INT NOT NULL,
        Mensaje NVARCHAR(255) NULL,
        Tipo NVARCHAR(50) NULL,
        FechaEnvio DATETIME2 DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Notificaciones_Usuario FOREIGN KEY (UsuarioID) REFERENCES dbo.Usuarios(UsuarioID) ON DELETE CASCADE
    );
    CREATE INDEX IX_Notificaciones_UsuarioID ON dbo.Notificaciones(UsuarioID);
END
GO

IF OBJECT_ID('dbo.Recordatorios','U') IS NULL
BEGIN
    CREATE TABLE dbo.Recordatorios (
        RecordatorioID INT IDENTITY(1,1) CONSTRAINT PK_Recordatorios PRIMARY KEY,
        UsuarioID INT NOT NULL,
        Titulo NVARCHAR(100) NOT NULL,
        Mensaje NVARCHAR(255) NULL,
        FechaHora DATETIME2 NOT NULL,
        Estado BIT NOT NULL CONSTRAINT DF_Recordatorios_Estado DEFAULT (0),
        CONSTRAINT FK_Recordatorios_Usuario FOREIGN KEY (UsuarioID) REFERENCES dbo.Usuarios(UsuarioID) ON DELETE CASCADE
    );
    CREATE INDEX IX_Recordatorios_UsuarioID ON dbo.Recordatorios(UsuarioID);
    CREATE INDEX IX_Recordatorios_FechaHora ON dbo.Recordatorios(FechaHora);
END
GO

IF OBJECT_ID('dbo.Nutricionistas','U') IS NULL
BEGIN
    CREATE TABLE dbo.Nutricionistas (
        NutricionistaID INT IDENTITY(1,1) CONSTRAINT PK_Nutricionistas PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Correo NVARCHAR(100) NULL,
        Telefono NVARCHAR(20) NULL
    );
END
GO

IF OBJECT_ID('dbo.ContactoNutricionista','U') IS NULL
BEGIN
    CREATE TABLE dbo.ContactoNutricionista (
        ContactoID INT IDENTITY(1,1) CONSTRAINT PK_ContactoNutric PRIMARY KEY,
        UsuarioID INT NOT NULL,
        NutricionistaID INT NOT NULL,
        Mensaje NVARCHAR(MAX) NULL,
        FechaSolicitud DATETIME2 DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Contacto_Usuario FOREIGN KEY (UsuarioID) REFERENCES dbo.Usuarios(UsuarioID) ON DELETE CASCADE,
        CONSTRAINT FK_Contacto_Nutri FOREIGN KEY (NutricionistaID) REFERENCES dbo.Nutricionistas(NutricionistaID) ON DELETE CASCADE
    );
    CREATE INDEX IX_Contacto_UsuarioID ON dbo.ContactoNutricionista(UsuarioID);
    CREATE INDEX IX_Contacto_NutricionistaID ON dbo.ContactoNutricionista(NutricionistaID);
END
GO

IF OBJECT_ID('dbo.MetodosPago','U') IS NULL
BEGIN
    CREATE TABLE dbo.MetodosPago (
        MetodoPagoID INT IDENTITY(1,1) CONSTRAINT PK_MetodosPago PRIMARY KEY,
        UsuarioID INT NOT NULL,
        Tipo NVARCHAR(30) NOT NULL,
        Token NVARCHAR(200) NULL,
        Marca NVARCHAR(20) NULL,
        Ultimos4 CHAR(4) NULL,
        ExpiraMes TINYINT NULL,
        ExpiraAno SMALLINT NULL,
        Predeterminado BIT NOT NULL CONSTRAINT DF_MetodoPred DEFAULT(0),
        CONSTRAINT FK_MetodosPago_Usuario FOREIGN KEY (UsuarioID) REFERENCES dbo.Usuarios(UsuarioID) ON DELETE CASCADE
    );
    CREATE INDEX IX_MetodosPago_UsuarioID ON dbo.MetodosPago(UsuarioID);
END
GO

-- IMPORTANTE: NO CASCADE hacia Usuarios (como pediste)
IF OBJECT_ID('dbo.Pagos','U') IS NULL
BEGIN
    CREATE TABLE dbo.Pagos (
        PagoID INT IDENTITY(1,1) CONSTRAINT PK_Pagos PRIMARY KEY,
        PedidoID INT NULL,
        UsuarioID INT NOT NULL,
        MetodoPagoID INT NULL,
        Monto DECIMAL(10,2) NOT NULL,
        Moneda CHAR(3) NOT NULL CONSTRAINT DF_Pagos_CRC DEFAULT ('CRC'),
        Estado NVARCHAR(30) NOT NULL,      -- Autorizado, Confirmado, Fallido, Reembolsado
        TransaccionRef NVARCHAR(100) NULL,
        FechaPago DATETIME2 DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Pagos_Pedido  FOREIGN KEY (PedidoID)     REFERENCES dbo.Pedidos(PedidoID)         ON DELETE SET NULL,
        CONSTRAINT FK_Pagos_Usuario FOREIGN KEY (UsuarioID)    REFERENCES dbo.Usuarios(UsuarioID),      -- NO ACTION
        CONSTRAINT FK_Pagos_Metodo  FOREIGN KEY (MetodoPagoID) REFERENCES dbo.MetodosPago(MetodoPagoID) ON DELETE SET NULL
    );
    CREATE INDEX IX_Pagos_PedidoID  ON dbo.Pagos(PedidoID);
    CREATE INDEX IX_Pagos_UsuarioID ON dbo.Pagos(UsuarioID);
END
GO

IF OBJECT_ID('dbo.Suscripciones','U') IS NULL
BEGIN
    CREATE TABLE dbo.Suscripciones (
        SuscripcionID INT IDENTITY(1,1) CONSTRAINT PK_Suscripciones PRIMARY KEY,
        UsuarioID INT NOT NULL,
        Estado NVARCHAR(30) NOT NULL,      -- Activa, Pausada, Cancelada
        FechaInicio DATE NOT NULL,
        FechaFin DATE NULL,
        UltimaFacturacion DATE NULL,
        ProximaFacturacion DATE NULL,
        CONSTRAINT FK_Suscripciones_Usuario FOREIGN KEY (UsuarioID) REFERENCES dbo.Usuarios(UsuarioID) ON DELETE CASCADE
    );
    CREATE INDEX IX_Suscripciones_UsuarioID ON dbo.Suscripciones(UsuarioID);
END
GO

IF OBJECT_ID('dbo.AuditLog','U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLog (
        AuditID BIGINT IDENTITY(1,1) CONSTRAINT PK_AuditLog PRIMARY KEY,
        Entidad NVARCHAR(100) NOT NULL,
        EntidadID NVARCHAR(100) NOT NULL,
        Accion NVARCHAR(20) NOT NULL,          -- INSERT/UPDATE/DELETE
        UsuarioID INT NULL,
        Fecha DATETIME2 DEFAULT SYSUTCDATETIME(),
        Datos NVARCHAR(MAX) NULL
    );
    CREATE INDEX IX_Audit_Entidad ON dbo.AuditLog(Entidad, Fecha DESC);
END
GO

/* ==============================
   SEED BÁSICO
   ============================== */
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE NombreRol = N'Admin')
    INSERT INTO dbo.Roles(NombreRol) VALUES (N'Admin');
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE NombreRol = N'Cliente')
    INSERT INTO dbo.Roles(NombreRol) VALUES (N'Cliente');
GO

/* ==============================
   NORMALIZACIÓN 
   ============================== */

   -- 1) Normalización de correo: columna computada + índice único
IF COL_LENGTH('dbo.Usuarios', 'CorreoNorm') IS NULL
BEGIN
    ALTER TABLE dbo.Usuarios
    ADD CorreoNorm AS LOWER(LTRIM(RTRIM(CorreoElectronico))) PERSISTED;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Usuarios_CorreoNorm' AND object_id = OBJECT_ID('dbo.Usuarios'))
        CREATE UNIQUE INDEX UX_Usuarios_CorreoNorm ON dbo.Usuarios(CorreoNorm);
END
GO

-- 2) Reglas de dominio con CHECK
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name='CK_Pedidos_EstadoPedido' AND parent_object_id=OBJECT_ID('dbo.Pedidos'))
BEGIN
    ALTER TABLE dbo.Pedidos WITH NOCHECK
    ADD CONSTRAINT CK_Pedidos_EstadoPedido
    CHECK (EstadoPedido IN (N'Creado',N'Pagado',N'EnPreparacion',N'Enviado',N'Entregado',N'Cancelado'));
    ALTER TABLE dbo.Pedidos CHECK CONSTRAINT CK_Pedidos_EstadoPedido;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name='CK_Pagos_Estado' AND parent_object_id=OBJECT_ID('dbo.Pagos'))
BEGIN
    ALTER TABLE dbo.Pagos WITH NOCHECK
    ADD CONSTRAINT CK_Pagos_Estado
    CHECK (Estado IN (N'Autorizado',N'Confirmado',N'Fallido',N'Reembolsado'));
    ALTER TABLE dbo.Pagos CHECK CONSTRAINT CK_Pagos_Estado;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name='CK_Suscripciones_Estado' AND parent_object_id=OBJECT_ID('dbo.Suscripciones'))
BEGIN
    ALTER TABLE dbo.Suscripciones WITH NOCHECK
    ADD CONSTRAINT CK_Suscripciones_Estado
    CHECK (Estado IN (N'Activa',N'Pausada',N'Cancelada'));
    ALTER TABLE dbo.Suscripciones CHECK CONSTRAINT CK_Suscripciones_Estado;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name='CK_MenusPorPlan_DiaSemana' AND parent_object_id=OBJECT_ID('dbo.MenusPorPlan'))
BEGIN
    ALTER TABLE dbo.MenusPorPlan WITH NOCHECK
    ADD CONSTRAINT CK_MenusPorPlan_DiaSemana
    CHECK (DiaSemana IN (N'Lunes',N'Martes',N'Miércoles',N'Jueves',N'Viernes',N'Sábado',N'Domingo'));
    ALTER TABLE dbo.MenusPorPlan CHECK CONSTRAINT CK_MenusPorPlan_DiaSemana;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name='CK_Pagos_Moneda' AND parent_object_id=OBJECT_ID('dbo.Pagos'))
BEGIN
    ALTER TABLE dbo.Pagos WITH NOCHECK
    ADD CONSTRAINT CK_Pagos_Moneda
    CHECK (Moneda IN (N'CRC',N'USD',N'EUR',N'MXN',N'COP',N'PEN',N'CLP',N'ARS',N'BRL'));
    ALTER TABLE dbo.Pagos CHECK CONSTRAINT CK_Pagos_Moneda;
END
GO

-- 3) Único método Predeterminado por Usuario
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_MetodosPago_DefaultPorUsuario' AND object_id=OBJECT_ID('dbo.MetodosPago'))
BEGIN
    CREATE UNIQUE INDEX UX_MetodosPago_DefaultPorUsuario
    ON dbo.MetodosPago(UsuarioID)
    WHERE Predeterminado = 1;
END
GO

-- 4) Índices de soporte adicionales
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Pedidos_Fecha' AND object_id=OBJECT_ID('dbo.Pedidos'))
    CREATE INDEX IX_Pedidos_Fecha ON dbo.Pedidos(FechaPedido);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Historial_Pedido_Fecha' AND object_id=OBJECT_ID('dbo.HistorialPedidos'))
    CREATE INDEX IX_Historial_Pedido_Fecha ON dbo.HistorialPedidos(PedidoID, FechaEstado DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Comentarios_Receta_Fecha' AND object_id=OBJECT_ID('dbo.ComentariosRecetas'))
    CREATE INDEX IX_Comentarios_Receta_Fecha ON dbo.ComentariosRecetas(RecetaID, FechaComentario DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MenusSemanales_Semana' AND object_id=OBJECT_ID('dbo.MenusSemanales'))
    CREATE INDEX IX_MenusSemanales_Semana ON dbo.MenusSemanales(SemanaDel);
GO


/*
   VISTAS
   */
IF OBJECT_ID('dbo.vwPedidosPorEstado','V') IS NOT NULL DROP VIEW dbo.vwPedidosPorEstado;
GO
CREATE VIEW dbo.vwPedidosPorEstado AS
SELECT EstadoPedido, COUNT(*) AS Cantidad, SUM(MontoTotal) AS TotalCRC
FROM dbo.Pedidos
GROUP BY EstadoPedido;
GO

IF OBJECT_ID('dbo.vwRecetasMasGuardadas','V') IS NOT NULL DROP VIEW dbo.vwRecetasMasGuardadas;
GO
CREATE VIEW dbo.vwRecetasMasGuardadas AS
SELECT r.RecetaID, r.Titulo, COUNT(f.RecetaFavoritaID) AS Favoritos
FROM dbo.Recetas r
LEFT JOIN dbo.RecetasFavoritas f ON f.RecetaID = r.RecetaID
GROUP BY r.RecetaID, r.Titulo;
GO


IF OBJECT_ID('dbo.vwPuntosPorUsuario','V') IS NOT NULL DROP VIEW dbo.vwPuntosPorUsuario;
GO
CREATE VIEW dbo.vwPuntosPorUsuario AS
SELECT u.UsuarioID, u.CorreoElectronico, p.PuntosAcumulados, p.FechaActualizacion
FROM dbo.Usuarios u
LEFT JOIN dbo.PuntosUsuario p ON p.UsuarioID = u.UsuarioID;
GO

/* ==============================
   PROCEDIMIENTOS ALMACENADOS
   ============================== */

   CREATE OR ALTER PROCEDURE dbo.spUsuario_Create
    @Nombre NVARCHAR(100),
    @Apellido NVARCHAR(100),
    @Correo NVARCHAR(100),
    @ContrasenaHash NVARCHAR(255),
    @RolNombre NVARCHAR(50) = N'Cliente'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RolID INT = (SELECT RolID FROM dbo.Roles WHERE NombreRol = @RolNombre);
    IF @RolID IS NULL
    BEGIN
        RAISERROR(N'Rol no existe: %s', 16, 1, @RolNombre);
        RETURN;
    END
    INSERT INTO dbo.Usuarios(Nombre, Apellido, CorreoElectronico, ContrasenaHash, RolID)
    VALUES (@Nombre, @Apellido, @Correo, @ContrasenaHash, @RolID);
    SELECT SCOPE_IDENTITY() AS UsuarioID;
END
GO

CREATE OR ALTER PROCEDURE dbo.spUsuario_Login
    @Correo NVARCHAR(100),
    @ContrasenaHash NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 UsuarioID, Nombre, Apellido, CorreoElectronico, RolID, Activo
    FROM dbo.Usuarios
    WHERE CorreoElectronico = @Correo AND ContrasenaHash = @ContrasenaHash AND Activo = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.spUsuario_UpdatePerfil
    @UsuarioID INT,
    @FechaNacimiento DATE = NULL,
    @Telefono NVARCHAR(20) = NULL,
    @DireccionEntrega NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.PerfilesCliente WHERE UsuarioID = @UsuarioID)
        UPDATE dbo.PerfilesCliente
        SET FechaNacimiento=@FechaNacimiento, Telefono=@Telefono, DireccionEntrega=@DireccionEntrega
        WHERE UsuarioID=@UsuarioID;
    ELSE
        INSERT INTO dbo.PerfilesCliente(UsuarioID, FechaNacimiento, Telefono, DireccionEntrega)
        VALUES (@UsuarioID, @FechaNacimiento, @Telefono, @DireccionEntrega);
END
GO

CREATE OR ALTER PROCEDURE dbo.spUsuario_SetRol
    @UsuarioID INT,
    @RolNombre NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RolID INT = (SELECT RolID FROM dbo.Roles WHERE NombreRol = @RolNombre);
    IF @RolID IS NULL
    BEGIN
        RAISERROR(N'Rol no existe: %s',16,1,@RolNombre);
        RETURN;
    END
    UPDATE dbo.Usuarios SET RolID = @RolID WHERE UsuarioID = @UsuarioID;
END
GO

CREATE OR ALTER PROCEDURE dbo.spReceta_Create
    @Titulo NVARCHAR(150),
    @Descripcion NVARCHAR(MAX) = NULL,
    @Instrucciones NVARCHAR(MAX) = NULL,
    @EsGratuita BIT = 1,
    @EsParteDeEbook BIT = 0,
    @ChefID INT,
    @RecetarioID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Recetas(Titulo, Descripcion, Instrucciones, EsGratuita, EsParteDeEbook, ChefID, RecetarioID)
    VALUES (@Titulo, @Descripcion, @Instrucciones, @EsGratuita, @EsParteDeEbook, @ChefID, @RecetarioID);
    SELECT SCOPE_IDENTITY() AS RecetaID;
END
GO

CREATE OR ALTER PROCEDURE dbo.spReceta_Update
    @RecetaID INT,
    @Titulo NVARCHAR(150),
    @Descripcion NVARCHAR(MAX) = NULL,
    @Instrucciones NVARCHAR(MAX) = NULL,
    @EsGratuita BIT = 1,
    @EsParteDeEbook BIT = 0,
    @RecetarioID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Recetas
    SET Titulo=@Titulo, Descripcion=@Descripcion, Instrucciones=@Instrucciones,
        EsGratuita=@EsGratuita, EsParteDeEbook=@EsParteDeEbook, RecetarioID=@RecetarioID
    WHERE RecetaID=@RecetaID;
END
GO

CREATE OR ALTER PROCEDURE dbo.spReceta_Delete
    @RecetaID INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.Recetas WHERE RecetaID=@RecetaID;
END
GO

CREATE OR ALTER PROCEDURE dbo.spReceta_Favorito_Toggle
    @UsuarioID INT,
    @RecetaID INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.RecetasFavoritas WHERE UsuarioID=@UsuarioID AND RecetaID=@RecetaID)
        DELETE FROM dbo.RecetasFavoritas WHERE UsuarioID=@UsuarioID AND RecetaID=@RecetaID;
    ELSE
        INSERT INTO dbo.RecetasFavoritas(UsuarioID, RecetaID) VALUES (@UsuarioID, @RecetaID);
END
GO


CREATE OR ALTER PROCEDURE dbo.spReceta_Comentario_Add
    @RecetaID INT,
    @UsuarioID INT,
    @Comentario NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.ComentariosRecetas(RecetaID, UsuarioID, Comentario) VALUES (@RecetaID, @UsuarioID, @Comentario);
    SELECT SCOPE_IDENTITY() AS ComentarioID;
END
GO

CREATE OR ALTER PROCEDURE dbo.spTipoMenu_Upsert
    @Nombre NVARCHAR(100),
    @Descripcion NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.TiposMenu WHERE Nombre=@Nombre)
        UPDATE dbo.TiposMenu SET Descripcion=@Descripcion WHERE Nombre=@Nombre;
    ELSE
        INSERT INTO dbo.TiposMenu(Nombre, Descripcion) VALUES (@Nombre, @Descripcion);
END
GO

CREATE OR ALTER PROCEDURE dbo.spMenuSemanal_Upsert
    @TipoMenuID INT,
    @SemanaDel DATE,
    @Descripcion NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.MenusSemanales WHERE TipoMenuID=@TipoMenuID AND SemanaDel=@SemanaDel)
        UPDATE dbo.MenusSemanales SET Descripcion=@Descripcion WHERE TipoMenuID=@TipoMenuID AND SemanaDel=@SemanaDel;
    ELSE
        INSERT INTO dbo.MenusSemanales(TipoMenuID, SemanaDel, Descripcion) VALUES (@TipoMenuID, @SemanaDel, @Descripcion);
END
GO


CREATE OR ALTER PROCEDURE dbo.spPlan_Create
    @UsuarioID INT,
    @NombrePlan NVARCHAR(100),
    @Descripcion NVARCHAR(MAX) = NULL,
    @FechaVencimiento DATE = NULL,
    @DocumentoURL NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.PlanesNutricionales(UsuarioID, NombrePlan, Descripcion, FechaVencimiento, DocumentoURL)
    VALUES (@UsuarioID, @NombrePlan, @Descripcion, @FechaVencimiento, @DocumentoURL);
    SELECT SCOPE_IDENTITY() AS PlanID;
END
GO

CREATE OR ALTER PROCEDURE dbo.spMenusPorPlan_AddDay
    @PlanID INT,
    @DiaSemana NVARCHAR(20),
    @Descripcion NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.MenusPorPlan(PlanID, DiaSemana, Descripcion) VALUES (@PlanID, @DiaSemana, @Descripcion);
    SELECT SCOPE_IDENTITY() AS MenuID;
END
GO

CREATE OR ALTER PROCEDURE dbo.spPedido_Create
    @UsuarioID INT,
    @DireccionEntrega NVARCHAR(255),
    @EstadoPedido NVARCHAR(50) = N'Creado'
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Pedidos(UsuarioID, DireccionEntrega, EstadoPedido) VALUES (@UsuarioID, @DireccionEntrega, @EstadoPedido);
    DECLARE @PedidoID INT = SCOPE_IDENTITY();
    INSERT INTO dbo.HistorialPedidos(PedidoID, Estado) VALUES (@PedidoID, @EstadoPedido);
    SELECT @PedidoID AS PedidoID;
END
GO

CREATE OR ALTER PROCEDURE dbo.spPedido_AddDetalle
    @PedidoID INT,
    @MenuSemanalID INT,
    @Cantidad INT,
    @PrecioUnitario DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.DetallePedido(PedidoID, MenuSemanalID, Cantidad, PrecioUnitario)
    VALUES (@PedidoID, @MenuSemanalID, @Cantidad, @PrecioUnitario);
    EXEC dbo.spPedido_RecalcularTotal @PedidoID=@PedidoID;
END
GO


CREATE OR ALTER PROCEDURE dbo.spPedido_RecalcularTotal
    @PedidoID INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE p
    SET MontoTotal = d.SumImporte
    FROM dbo.Pedidos p
    CROSS APPLY (
        SELECT SUM(CAST(Cantidad AS DECIMAL(10,2)) * PrecioUnitario) AS SumImporte
        FROM dbo.DetallePedido
        WHERE PedidoID = p.PedidoID
    ) d
    WHERE p.PedidoID = @PedidoID;
END
GO



CREATE OR ALTER PROCEDURE dbo.spPedido_CambiarEstado
    @PedidoID INT,
    @NuevoEstado NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Pedidos SET EstadoPedido = @NuevoEstado WHERE PedidoID = @PedidoID;
    INSERT INTO dbo.HistorialPedidos(PedidoID, Estado) VALUES (@PedidoID, @NuevoEstado);
END
GO


CREATE OR ALTER PROCEDURE dbo.spMetodoPago_Upsert
    @UsuarioID INT,
    @Tipo NVARCHAR(30),
    @Token NVARCHAR(200) = NULL,
    @Marca NVARCHAR(20) = NULL,
    @Ultimos4 CHAR(4) = NULL,
    @ExpiraMes TINYINT = NULL,
    @ExpiraAno SMALLINT = NULL,
    @Predeterminado BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @MetodoPagoID INT =
        (SELECT TOP 1 MetodoPagoID FROM dbo.MetodosPago
         WHERE UsuarioID=@UsuarioID AND Tipo=@Tipo AND (@Ultimos4 IS NULL OR Ultimos4=@Ultimos4));

    IF @MetodoPagoID IS NULL
    BEGIN
        INSERT INTO dbo.MetodosPago(UsuarioID, Tipo, Token, Marca, Ultimos4, ExpiraMes, ExpiraAno, Predeterminado)
        VALUES (@UsuarioID, @Tipo, @Token, @Marca, @Ultimos4, @ExpiraMes, @ExpiraAno, @Predeterminado);
        SET @MetodoPagoID = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.MetodosPago
        SET Token=@Token, Marca=@Marca, Ultimos4=@Ultimos4, ExpiraMes=@ExpiraMes, ExpiraAno=@ExpiraAno, Predeterminado=@Predeterminado
        WHERE MetodoPagoID=@MetodoPagoID;
    END

    IF @Predeterminado = 1
        UPDATE dbo.MetodosPago SET Predeterminado = CASE WHEN MetodoPagoID=@MetodoPagoID THEN 1 ELSE 0 END
        WHERE UsuarioID=@UsuarioID;

    SELECT @MetodoPagoID AS MetodoPagoID;
END
GO



CREATE OR ALTER PROCEDURE dbo.spPago_Registrar
    @PedidoID INT = NULL,
    @UsuarioID INT,
    @MetodoPagoID INT = NULL,
    @Monto DECIMAL(10,2),
    @Moneda CHAR(3) = 'CRC',
    @Estado NVARCHAR(30),
    @TransaccionRef NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Pagos(PedidoID, UsuarioID, MetodoPagoID, Monto, Moneda, Estado, TransaccionRef)
    VALUES (@PedidoID, @UsuarioID, @MetodoPagoID, @Monto, @Moneda, @Estado, @TransaccionRef);
    SELECT SCOPE_IDENTITY() AS PagoID;
END
GO



CREATE OR ALTER PROCEDURE dbo.spPedido_Pagar
    @PedidoID INT,
    @UsuarioID INT,
    @MetodoPagoID INT,
    @TransaccionRef NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Monto DECIMAL(10,2) = (SELECT MontoTotal FROM dbo.Pedidos WHERE PedidoID=@PedidoID);
    IF @Monto IS NULL
    BEGIN
        RAISERROR(N'Pedido no existe',16,1);
        RETURN;
    END
    EXEC dbo.spPago_Registrar
        @PedidoID=@PedidoID,
        @UsuarioID=@UsuarioID,
        @MetodoPagoID=@MetodoPagoID,
        @Monto=@Monto,
        @Moneda='CRC',
        @Estado='Confirmado',
        @TransaccionRef=@TransaccionRef;
    EXEC dbo.spPedido_CambiarEstado @PedidoID=@PedidoID, @NuevoEstado=N'Pagado';
END
GO







CREATE OR ALTER PROCEDURE dbo.spSuscripcion_Crear
    @UsuarioID INT,
    @FechaInicio DATE,
    @ProximaFacturacion DATE,
    @Estado NVARCHAR(30) = N'Activa'
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Suscripciones(UsuarioID, Estado, FechaInicio, ProximaFacturacion)
    VALUES (@UsuarioID, @Estado, @FechaInicio, @ProximaFacturacion);
    SELECT SCOPE_IDENTITY() AS SuscripcionID;
END
GO



CREATE OR ALTER PROCEDURE dbo.spSuscripcion_Pausar
    @SuscripcionID INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Suscripciones SET Estado='Pausada' WHERE SuscripcionID=@SuscripcionID;
END
GO



CREATE OR ALTER PROCEDURE dbo.spSuscripcion_Reanudar
    @SuscripcionID INT, @ProximaFacturacion DATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Suscripciones SET Estado='Activa', ProximaFacturacion=@ProximaFacturacion WHERE SuscripcionID=@SuscripcionID;
END
GO


CREATE OR ALTER PROCEDURE dbo.spPuntos_Sumar
    @UsuarioID INT,
    @Delta INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.PuntosUsuario WHERE UsuarioID=@UsuarioID)
        UPDATE dbo.PuntosUsuario SET PuntosAcumulados = PuntosAcumulados + @Delta, FechaActualizacion = SYSUTCDATETIME()
        WHERE UsuarioID=@UsuarioID;
    ELSE
        INSERT INTO dbo.PuntosUsuario(UsuarioID, PuntosAcumulados) VALUES (@UsuarioID, @Delta);
END
GO



CREATE OR ALTER PROCEDURE dbo.spEncuesta_Registrar
    @UsuarioID INT,
    @Calificacion INT,
    @Sugerencias NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.EncuestasSatisfaccion(UsuarioID, Calificacion, Sugerencias) VALUES (@UsuarioID, @Calificacion, @Sugerencias);
    SELECT SCOPE_IDENTITY() AS EncuestaID;
END
GO




CREATE OR ALTER PROCEDURE dbo.spNotificacion_Enviar
    @UsuarioID INT,
    @Mensaje NVARCHAR(255),
    @Tipo NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Notificaciones(UsuarioID, Mensaje, Tipo) VALUES (@UsuarioID, @Mensaje, @Tipo);
END
GO



CREATE OR ALTER PROCEDURE dbo.spRecordatorio_Programar
    @UsuarioID INT,
    @Titulo NVARCHAR(100),
    @Mensaje NVARCHAR(255) = NULL,
    @FechaHora DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Recordatorios(UsuarioID, Titulo, Mensaje, FechaHora) VALUES (@UsuarioID, @Titulo, @Mensaje, @FechaHora);
END
GO



CREATE OR ALTER PROCEDURE dbo.spAudit_Write
    @Entidad NVARCHAR(100),
    @EntidadID NVARCHAR(100),
    @Accion NVARCHAR(20),
    @UsuarioID INT = NULL,
    @Datos NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.AuditLog(Entidad, EntidadID, Accion, UsuarioID, Datos)
    VALUES (@Entidad, @EntidadID, @Accion, @UsuarioID, @Datos);
END
GO


/* ==============================
   TRIGGER DE AUDITORÍA (Pedidos)
   ============================== */
IF OBJECT_ID('dbo.trg_Pedidos_Audit','TR') IS NOT NULL DROP TRIGGER dbo.trg_Pedidos_Audit;
GO
CREATE TRIGGER dbo.trg_Pedidos_Audit
ON dbo.Pedidos
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- INSERT
    INSERT INTO dbo.AuditLog(Entidad, EntidadID, Accion, UsuarioID, Datos)
    SELECT 'Pedidos', CAST(i.PedidoID AS NVARCHAR(100)), 'INSERT', NULL,
           CONCAT('Estado=', i.EstadoPedido, '; Total=', i.MontoTotal)
    FROM inserted i
    LEFT JOIN deleted d ON 1=0;

    -- UPDATE
    INSERT INTO dbo.AuditLog(Entidad, EntidadID, Accion, UsuarioID, Datos)
    SELECT 'Pedidos', CAST(i.PedidoID AS NVARCHAR(100)), 'UPDATE', NULL,
           CONCAT('EstadoOld=', d.EstadoPedido, ' -> EstadoNew=', i.EstadoPedido,
                  '; TotalOld=', d.MontoTotal, ' -> TotalNew=', i.MontoTotal)
    FROM inserted i
    JOIN deleted d ON d.PedidoID = i.PedidoID;

    -- DELETE
    INSERT INTO dbo.AuditLog(Entidad, EntidadID, Accion, UsuarioID, Datos)
    SELECT 'Pedidos', CAST(d.PedidoID AS NVARCHAR(100)), 'DELETE', NULL,
           CONCAT('Estado=', d.EstadoPedido, '; Total=', d.MontoTotal)
    FROM deleted d
    LEFT JOIN inserted i ON 1=0;
END
GO






