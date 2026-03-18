-- Script para agregar las columnas faltantes a la tabla EbookEdicion
-- Ejecutar este script en la base de datos de Azure

-- Verificar si la tabla existe
IF OBJECT_ID('dbo.EbookEdicion','U') IS NOT NULL
BEGIN
    PRINT 'Tabla EbookEdicion encontrada. Agregando columnas...'
    
    -- Agregar columna PermitirDescarga si no existe
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.EbookEdicion') AND name = 'PermitirDescarga')
    BEGIN
        ALTER TABLE dbo.EbookEdicion 
        ADD PermitirDescarga BIT NOT NULL CONSTRAINT DF_EbookEdicion_PermitirDescarga DEFAULT (1);
        PRINT 'Columna PermitirDescarga agregada exitosamente'
    END
    ELSE
    BEGIN
        PRINT 'Columna PermitirDescarga ya existe'
    END
    
    -- Agregar columna NombreArchivo si no existe
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.EbookEdicion') AND name = 'NombreArchivo')
    BEGIN
        ALTER TABLE dbo.EbookEdicion 
        ADD NombreArchivo NVARCHAR(255) NULL;
        PRINT 'Columna NombreArchivo agregada exitosamente'
    END
    ELSE
    BEGIN
        PRINT 'Columna NombreArchivo ya existe'
    END
    
    -- Agregar columna RutaArchivo si no existe
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.EbookEdicion') AND name = 'RutaArchivo')
    BEGIN
        ALTER TABLE dbo.EbookEdicion 
        ADD RutaArchivo NVARCHAR(500) NULL;
        PRINT 'Columna RutaArchivo agregada exitosamente'
    END
    ELSE
    BEGIN
        PRINT 'Columna RutaArchivo ya existe'
    END
    
    -- Agregar columna TamañoArchivo si no existe
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.EbookEdicion') AND name = 'TamañoArchivo')
    BEGIN
        ALTER TABLE dbo.EbookEdicion 
        ADD TamañoArchivo BIGINT NULL;
        PRINT 'Columna TamañoArchivo agregada exitosamente'
    END
    ELSE
    BEGIN
        PRINT 'Columna TamañoArchivo ya existe'
    END
    
    -- Agregar columna TipoMime si no existe
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.EbookEdicion') AND name = 'TipoMime')
    BEGIN
        ALTER TABLE dbo.EbookEdicion 
        ADD TipoMime NVARCHAR(100) NULL;
        PRINT 'Columna TipoMime agregada exitosamente'
    END
    ELSE
    BEGIN
        PRINT 'Columna TipoMime ya existe'
    END
    
    -- Agregar columna FechaSubida si no existe
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.EbookEdicion') AND name = 'FechaSubida')
    BEGIN
        ALTER TABLE dbo.EbookEdicion 
        ADD FechaSubida DATETIME2 NULL;
        PRINT 'Columna FechaSubida agregada exitosamente'
    END
    ELSE
    BEGIN
        PRINT 'Columna FechaSubida ya existe'
    END
    
    PRINT 'Script ejecutado exitosamente. Todas las columnas han sido agregadas.'
END
ELSE
BEGIN
    PRINT 'ERROR: La tabla EbookEdicion no existe. Verifica el nombre de la tabla.'
END
