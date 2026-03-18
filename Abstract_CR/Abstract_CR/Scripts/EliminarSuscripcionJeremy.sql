-- Script para eliminar la suscripción de Jeremy y probar la funcionalidad del admin
-- Ejecutar este script en la base de datos de Azure

-- Eliminar suscripción de Jeremy si existe
IF EXISTS (SELECT 1 FROM dbo.Suscripciones WHERE UsuarioID = 14)
BEGIN
    DELETE FROM dbo.Suscripciones WHERE UsuarioID = 14;
    PRINT 'Suscripción de Jeremy eliminada. Ahora puedes probar la funcionalidad del admin.'
END
ELSE
BEGIN
    PRINT 'Jeremy no tiene suscripción. Puedes probar la funcionalidad del admin.'
END

-- Mostrar estado actual de todas las suscripciones
PRINT 'Estado actual de suscripciones:'
SELECT s.*, u.Nombre, u.Apellido, u.CorreoElectronico 
FROM dbo.Suscripciones s
LEFT JOIN dbo.Usuarios u ON s.UsuarioID = u.UsuarioID;
