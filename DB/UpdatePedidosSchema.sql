/*
    Update script to align the Pedidos domain tables with the current
    Entity Framework Core model (Pedido / PedidoDetalle).

    Execute against the target database BEFORE browsing the "Mis pedidos"
    section if you created the schema using an older revision of
    DB/AbstractDataBase.sql.
*/

SET NOCOUNT ON;

PRINT N'--- Ajustando tabla dbo.Pedidos ---';
IF OBJECT_ID(N'dbo.Pedidos', N'U') IS NULL
BEGIN
    PRINT N'La tabla dbo.Pedidos no existe. No se realizan cambios.';
END
ELSE
BEGIN
    -- Renombrar columnas antiguas si todavía existen.
    IF COL_LENGTH(N'dbo.Pedidos', N'DireccionEntrega') IS NOT NULL
       AND COL_LENGTH(N'dbo.Pedidos', N'DireccionEnvio') IS NULL
    BEGIN
        EXEC sp_rename N'dbo.Pedidos.DireccionEntrega', N'DireccionEnvio', N'COLUMN';
        PRINT N'Se renombró DireccionEntrega -> DireccionEnvio.';
    END

    IF COL_LENGTH(N'dbo.Pedidos', N'EstadoPedido') IS NOT NULL
       AND COL_LENGTH(N'dbo.Pedidos', N'Estado') IS NULL
    BEGIN
        EXEC sp_rename N'dbo.Pedidos.EstadoPedido', N'Estado', N'COLUMN';
        PRINT N'Se renombró EstadoPedido -> Estado.';
    END

    IF COL_LENGTH(N'dbo.Pedidos', N'MontoTotal') IS NOT NULL
       AND COL_LENGTH(N'dbo.Pedidos', N'Total') IS NULL
    BEGIN
        EXEC sp_rename N'dbo.Pedidos.MontoTotal', N'Total', N'COLUMN';
        PRINT N'Se renombró MontoTotal -> Total.';
    END

    -- Eliminar columnas obsoletas.
    DECLARE @constraintName sysname;
    SELECT @constraintName = dc.name
      FROM sys.default_constraints dc
      JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
     WHERE dc.parent_object_id = OBJECT_ID(N'dbo.Pedidos')
       AND c.name = N'SuscripcionPausada';
    IF @constraintName IS NOT NULL
    BEGIN
        EXEC(N'ALTER TABLE dbo.Pedidos DROP CONSTRAINT ' + QUOTENAME(@constraintName) + ';');
    END
    IF COL_LENGTH(N'dbo.Pedidos', N'SuscripcionPausada') IS NOT NULL
    BEGIN
        ALTER TABLE dbo.Pedidos DROP COLUMN SuscripcionPausada;
        PRINT N'Se eliminó la columna SuscripcionPausada.';
    END

    IF COL_LENGTH(N'dbo.Pedidos', N'RowVer') IS NOT NULL
    BEGIN
        ALTER TABLE dbo.Pedidos DROP COLUMN RowVer;
        PRINT N'Se eliminó la columna RowVer.';
    END

    -- Asegurar columnas requeridas y tipos de datos.
    IF COL_LENGTH(N'dbo.Pedidos', N'MetodoPago') IS NULL
    BEGIN
        ALTER TABLE dbo.Pedidos ADD MetodoPago NVARCHAR(30) NULL;
        UPDATE dbo.Pedidos SET MetodoPago = N'TarjetaCredito' WHERE MetodoPago IS NULL;
        ALTER TABLE dbo.Pedidos ALTER COLUMN MetodoPago NVARCHAR(30) NOT NULL;
        PRINT N'Se agregó la columna MetodoPago.';
    END

    IF COL_LENGTH(N'dbo.Pedidos', N'Total') IS NOT NULL
    BEGIN
        SELECT @constraintName = dc.name
          FROM sys.default_constraints dc
          JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
         WHERE dc.parent_object_id = OBJECT_ID(N'dbo.Pedidos')
           AND c.name = N'Total';
        IF @constraintName IS NOT NULL
        BEGIN
            EXEC(N'ALTER TABLE dbo.Pedidos DROP CONSTRAINT ' + QUOTENAME(@constraintName) + ';');
        END
        ALTER TABLE dbo.Pedidos ALTER COLUMN Total DECIMAL(10,2) NOT NULL;
        ALTER TABLE dbo.Pedidos ADD CONSTRAINT DF_Pedidos_Total DEFAULT (0) FOR Total;
    END

    IF COL_LENGTH(N'dbo.Pedidos', N'Estado') IS NOT NULL
    BEGIN
        ALTER TABLE dbo.Pedidos ALTER COLUMN Estado NVARCHAR(20) NOT NULL;
    END

    IF COL_LENGTH(N'dbo.Pedidos', N'DireccionEnvio') IS NOT NULL
    BEGIN
        ALTER TABLE dbo.Pedidos ALTER COLUMN DireccionEnvio NVARCHAR(250) NULL;
    END

    -- Reconstruir índices según el nuevo esquema.
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Pedidos_EstadoPedido' AND object_id = OBJECT_ID(N'dbo.Pedidos'))
    BEGIN
        DROP INDEX IX_Pedidos_EstadoPedido ON dbo.Pedidos;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Pedidos_Estado' AND object_id = OBJECT_ID(N'dbo.Pedidos'))
    BEGIN
        CREATE INDEX IX_Pedidos_Estado ON dbo.Pedidos(Estado);
    END

    PRINT N'Tabla dbo.Pedidos actualizada.';
END

PRINT N'--- Ajustando tabla de detalles ---';
IF OBJECT_ID(N'dbo.PedidoDetalles', N'U') IS NULL
BEGIN
    IF OBJECT_ID(N'dbo.DetallePedido', N'U') IS NOT NULL
    BEGIN
        IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Detalle_Pedido')
            ALTER TABLE dbo.DetallePedido DROP CONSTRAINT FK_Detalle_Pedido;
        IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Detalle_Menu')
            ALTER TABLE dbo.DetallePedido DROP CONSTRAINT FK_Detalle_Menu;
        DROP TABLE dbo.DetallePedido;
        PRINT N'Se eliminó la tabla antigua dbo.DetallePedido.';
    END

    CREATE TABLE dbo.PedidoDetalles (
        PedidoDetalleID INT IDENTITY(1,1) CONSTRAINT PK_PedidoDetalles PRIMARY KEY,
        PedidoID INT NOT NULL,
        Descripcion NVARCHAR(200) NOT NULL,
        Cantidad INT NOT NULL,
        PrecioUnitario DECIMAL(10,2) NOT NULL,
        CONSTRAINT FK_PedidoDetalles_Pedido FOREIGN KEY (PedidoID) REFERENCES dbo.Pedidos(PedidoID) ON DELETE CASCADE
    );
    CREATE INDEX IX_PedidoDetalles_PedidoID ON dbo.PedidoDetalles(PedidoID);
    PRINT N'Tabla dbo.PedidoDetalles creada.';
END
ELSE
BEGIN
    -- Asegurar columnas en caso de haber migrado manualmente.
    IF COL_LENGTH(N'dbo.PedidoDetalles', N'Descripcion') IS NULL
    BEGIN
        RAISERROR(N'La tabla dbo.PedidoDetalles existe pero no contiene la columna Descripcion. Revise manualmente.', 16, 1);
    END

    IF COL_LENGTH(N'dbo.PedidoDetalles', N'PrecioUnitario') IS NOT NULL
    BEGIN
        ALTER TABLE dbo.PedidoDetalles ALTER COLUMN PrecioUnitario DECIMAL(10,2) NOT NULL;
    END

    PRINT N'Tabla dbo.PedidoDetalles validada.';
END

PRINT N'--- Fin del script de actualización ---';
GO
