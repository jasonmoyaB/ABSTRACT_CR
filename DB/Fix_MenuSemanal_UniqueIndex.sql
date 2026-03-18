-- Script para corregir el índice único de MenusSemanales
-- Permite tener múltiples platillos por semana (uno por cada día)

-- Eliminar el índice único existente que solo incluye TipoMenuID y SemanaDel
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'UX_Menus_Tipo_Semana' AND object_id = OBJECT_ID('dbo.MenusSemanales'))
BEGIN
    DROP INDEX UX_Menus_Tipo_Semana ON dbo.MenusSemanales;
    PRINT 'Índice único UX_Menus_Tipo_Semana eliminado';
END
GO

-- Crear un nuevo índice único que incluya también DiaSemana
-- Esto permite tener un platillo diferente para cada día de la semana
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UX_Menus_Tipo_Semana_Dia' AND object_id = OBJECT_ID('dbo.MenusSemanales'))
BEGIN
    CREATE UNIQUE INDEX UX_Menus_Tipo_Semana_Dia 
    ON dbo.MenusSemanales(TipoMenuID, SemanaDel, DiaSemana);
    PRINT 'Nuevo índice único UX_Menus_Tipo_Semana_Dia creado';
END
GO

PRINT 'Script ejecutado correctamente. Ahora puedes tener múltiples platillos por semana (uno por día).';
GO

