-- Script para verificar si la columna PermitirDescargaEbook existe en la tabla Usuarios
-- y agregarla si no existe

USE [AbstractDataBase]
GO

-- Verificar si la columna existe
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Usuarios' 
               AND COLUMN_NAME = 'PermitirDescargaEbook')
BEGIN
    PRINT 'La columna PermitirDescargaEbook NO existe. Agregándola...'
    
    -- Agregar la columna
    ALTER TABLE dbo.Usuarios 
    ADD PermitirDescargaEbook BIT NOT NULL DEFAULT 0
    
    PRINT 'Columna PermitirDescargaEbook agregada exitosamente con valor por defecto FALSE'
END
ELSE
BEGIN
    PRINT 'La columna PermitirDescargaEbook YA existe'
END

-- Mostrar la estructura actual de la tabla Usuarios
PRINT 'Estructura actual de la tabla Usuarios:'
SELECT 
    COLUMN_NAME as 'Columna',
    DATA_TYPE as 'Tipo',
    IS_NULLABLE as 'Permite_NULL',
    COLUMN_DEFAULT as 'Valor_Defecto'
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Usuarios' 
ORDER BY ORDINAL_POSITION

-- Mostrar los valores actuales de PermitirDescargaEbook para todos los usuarios
PRINT 'Valores actuales de PermitirDescargaEbook:'
SELECT 
    UsuarioID,
    NombreCompleto,
    PermitirDescargaEbook
FROM dbo.Usuarios
ORDER BY UsuarioID

-- Actualizar todos los usuarios para que tengan PermitirDescargaEbook = 0 (false) por defecto
UPDATE dbo.Usuarios 
SET PermitirDescargaEbook = 0
WHERE PermitirDescargaEbook IS NULL

PRINT 'Todos los usuarios actualizados para tener PermitirDescargaEbook = FALSE por defecto'
