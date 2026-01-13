-- Script SQL para crear la tabla ComprobantesPago
-- Ejecutar este script directamente en SQL Server Management Studio

IF OBJECT_ID('dbo.ComprobantesPago','U') IS NULL
BEGIN
    CREATE TABLE dbo.ComprobantesPago (
        ComprobanteID INT IDENTITY(1,1) CONSTRAINT PK_ComprobantesPago PRIMARY KEY,
        UsuarioID INT NOT NULL,
        RutaArchivo NVARCHAR(500) NOT NULL,
        NombreArchivoOriginal NVARCHAR(255) NULL,
        TipoArchivo NVARCHAR(50) NULL,
        FechaSubida DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        Observaciones NVARCHAR(500) NULL,
        Estado NVARCHAR(50) NOT NULL DEFAULT 'Pendiente',
        CONSTRAINT FK_ComprobantesPago_Usuarios FOREIGN KEY (UsuarioID) REFERENCES dbo.Usuarios(UsuarioID) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_ComprobantesPago_UsuarioID ON dbo.ComprobantesPago(UsuarioID);
    
    PRINT 'Tabla ComprobantesPago creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla ComprobantesPago ya existe.';
END
GO

