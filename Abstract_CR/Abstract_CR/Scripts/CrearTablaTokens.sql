-- Script para crear la tabla Tokens (recuperación de contraseña) si no existe
-- Ejecutar en la base de datos de Azure si la tabla no existe

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tokens')
BEGIN
    CREATE TABLE [dbo].[Tokens] (
        [Id] int NOT NULL IDENTITY(1,1),
        [UsuarioID] int NOT NULL,
        [Token] nvarchar(max) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        CONSTRAINT [PK_Tokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Tokens_Usuarios_UsuarioID] FOREIGN KEY ([UsuarioID]) 
            REFERENCES [dbo].[Usuarios] ([UsuarioID]) ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_Tokens_UsuarioID] ON [dbo].[Tokens] ([UsuarioID]);
    
    PRINT 'Tabla Tokens creada correctamente.';
END
ELSE
BEGIN
    PRINT 'La tabla Tokens ya existe.';
END
