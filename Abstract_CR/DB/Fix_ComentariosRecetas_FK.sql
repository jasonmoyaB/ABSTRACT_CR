-- FIX: Eliminar FK_Comentarios_Recetas de ComentariosRecetas
ALTER TABLE ComentariosRecetas DROP CONSTRAINT FK_Comentarios_Recetas;

SELECT fk.name AS FK_Name, tp.name AS ParentTable, cp.name AS ParentColumn, tr.name AS ReferencedTable
FROM sys.foreign_keys fk
INNER JOIN sys.tables tp ON fk.parent_object_id = tp.object_id
INNER JOIN sys.tables tr ON fk.referenced_object_id = tr.object_id
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns cp ON fkc.parent_column_id = cp.column_id AND fkc.parent_object_id = cp.object_id
WHERE tp.name = 'ComentariosRecetas';
