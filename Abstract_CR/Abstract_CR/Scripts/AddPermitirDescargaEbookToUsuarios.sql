-- Script para agregar la columna PermitirDescargaEbook a la tabla Usuarios
-- Ejecutar este script en la base de datos de Azure

-- Verificar si la tabla existe
IF OBJECT_ID('dbo.Usuarios','U') IS NOT NULL
BEGIN
    PRINT 'Tabla Usuarios encontrada. Agregando columna PermitirDescargaEbook...'
    
    -- Agregar columna PermitirDescargaEbook si no existe
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Usuarios') AND name = 'PermitirDescargaEbook')
    BEGIN
        ALTER TABLE dbo.Usuarios 
        ADD PermitirDescargaEbook BIT NOT NULL CONSTRAINT DF_Usuarios_PermitirDescargaEbook DEFAULT (1);
        PRINT 'Columna PermitirDescargaEbook agregada exitosamente'
    END
    ELSE
    BEGIN
        PRINT 'Columna PermitirDescargaEbook ya existe'
    END
    
    PRINT 'Script ejecutado exitosamente.'
END
ELSE
BEGIN
    PRINT 'ERROR: La tabla Usuarios no existe. Verifica el nombre de la tabla.'
END
