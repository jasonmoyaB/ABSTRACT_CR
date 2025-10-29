-- Script para verificar y crear suscripción para Jeremy (UsuarioID 14)
-- Ejecutar este script en la base de datos de Azure

-- 1. Verificar si existe la tabla Suscripciones
IF OBJECT_ID('dbo.Suscripciones','U') IS NOT NULL
BEGIN
    PRINT 'Tabla Suscripciones encontrada.'
    
    -- 2. Verificar si Jeremy tiene suscripción
    IF EXISTS (SELECT 1 FROM dbo.Suscripciones WHERE UsuarioID = 14)
    BEGIN
        PRINT 'Jeremy ya tiene suscripción:'
        SELECT * FROM dbo.Suscripciones WHERE UsuarioID = 14;
    END
    ELSE
    BEGIN
        PRINT 'Jeremy NO tiene suscripción. Creando una...'
        
        -- 3. Crear suscripción para Jeremy
        INSERT INTO dbo.Suscripciones (
            UsuarioID,
            PlanID,
            Estado,
            FechaInicio,
            FechaFin,
            FechaRegistro
        ) VALUES (
            14,  -- UsuarioID de Jeremy
            1,   -- PlanID (asumiendo que existe un plan con ID 1)
            'Activa',
            GETDATE(),
            DATEADD(YEAR, 1, GETDATE()), -- 1 año de suscripción
            GETDATE()
        );
        
        PRINT 'Suscripción creada exitosamente para Jeremy (UsuarioID 14)'
        
        -- 4. Verificar la suscripción creada
        SELECT * FROM dbo.Suscripciones WHERE UsuarioID = 14;
    END
END
ELSE
BEGIN
    PRINT 'ERROR: La tabla Suscripciones no existe.'
    PRINT 'Verifica el nombre de la tabla o créala primero.'
END

-- 5. Mostrar todas las suscripciones para referencia
PRINT 'Todas las suscripciones en el sistema:'
SELECT s.*, u.Nombre, u.Apellido, u.CorreoElectronico 
FROM dbo.Suscripciones s
LEFT JOIN dbo.Usuarios u ON s.UsuarioID = u.UsuarioID;
