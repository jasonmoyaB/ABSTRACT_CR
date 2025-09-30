-- Script para agregar campos faltantes a la tabla PassResetTokens
-- Ejecutar este script en Azure SQL Database

-- Verificar si las columnas ya existen antes de agregarlas
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PassResetTokens') AND name = 'FechaExpiracion')
BEGIN
    ALTER TABLE PassResetTokens ADD FechaExpiracion datetime2 NOT NULL DEFAULT GETUTCDATE()
    PRINT 'Columna FechaExpiracion agregada'
END
ELSE
BEGIN
    PRINT 'Columna FechaExpiracion ya existe'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PassResetTokens') AND name = 'Usado')
BEGIN
    ALTER TABLE PassResetTokens ADD Usado bit NOT NULL DEFAULT 0
    PRINT 'Columna Usado agregada'
END
ELSE
BEGIN
    PRINT 'Columna Usado ya existe'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PassResetTokens') AND name = 'FechaUso')
BEGIN
    ALTER TABLE PassResetTokens ADD FechaUso datetime2 NULL
    PRINT 'Columna FechaUso agregada'
END
ELSE
BEGIN
    PRINT 'Columna FechaUso ya existe'
END

-- Actualizar la longitud máxima del campo Token si es necesario
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PassResetTokens') AND name = 'Token')
BEGIN
    DECLARE @currentLength int
    SELECT @currentLength = CHARACTER_MAXIMUM_LENGTH 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PassResetTokens' AND COLUMN_NAME = 'Token'
    
    IF @currentLength < 255
    BEGIN
        ALTER TABLE PassResetTokens ALTER COLUMN Token nvarchar(255) NOT NULL
        PRINT 'Longitud del campo Token actualizada a 255 caracteres'
    END
    ELSE
    BEGIN
        PRINT 'Longitud del campo Token ya es suficiente'
    END
END

PRINT 'Actualización de tabla PassResetTokens completada'
