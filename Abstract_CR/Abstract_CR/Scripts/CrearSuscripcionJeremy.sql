-- Script para crear suscripción para Jeremy (UsuarioID 14)
-- Ejecutar este script en la base de datos de Azure

-- Verificar si Jeremy ya tiene suscripción
IF EXISTS (SELECT 1 FROM dbo.Suscripciones WHERE UsuarioID = 14)
BEGIN
    PRINT 'Jeremy ya tiene una suscripción:'
    SELECT * FROM dbo.Suscripciones WHERE UsuarioID = 14;
END
ELSE
BEGIN
    PRINT 'Jeremy NO tiene suscripción. Creando una...'
    
    -- Crear suscripción para Jeremy usando la estructura correcta
    INSERT INTO dbo.Suscripciones (
        UsuarioID,
        Estado,
        FechaInicio,
        FechaFin,
        UltimaFacturacion,
        ProximaFacturacion
    ) VALUES (
        14,  -- UsuarioID de Jeremy
        'Activa',
        GETDATE(),
        DATEADD(MONTH, 1, GETDATE()), -- 1 mes de suscripción
        GETDATE(),
        DATEADD(MONTH, 1, GETDATE())
    );
    
    PRINT 'Suscripción creada exitosamente para Jeremy (UsuarioID 14) por 1 mes'
    
    -- Verificar la suscripción creada
    SELECT * FROM dbo.Suscripciones WHERE UsuarioID = 14;
END

-- Mostrar todas las suscripciones para referencia
PRINT 'Todas las suscripciones en el sistema:'
SELECT s.*, u.Nombre, u.Apellido, u.CorreoElectronico 
FROM dbo.Suscripciones s
LEFT JOIN dbo.Usuarios u ON s.UsuarioID = u.UsuarioID;
