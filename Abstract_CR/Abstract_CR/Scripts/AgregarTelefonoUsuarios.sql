-- Agrega columna de teléfono a Usuarios (Azure SQL / SQL Server)
-- Ejecutar en la base de datos correcta (ej. AbstractDataBase)

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns c
    INNER JOIN sys.tables t ON c.object_id = t.object_id
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = N'dbo'
      AND t.name = N'Usuarios'
      AND c.name = N'Telefono'
)
BEGIN
    ALTER TABLE dbo.Usuarios
    ADD Telefono NVARCHAR(50) NULL;

    PRINT 'Columna Telefono agregada a dbo.Usuarios.';
END
ELSE
BEGIN
    PRINT 'La columna Telefono ya existe en dbo.Usuarios.';
END
GO
