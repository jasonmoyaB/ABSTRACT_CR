# Scripts de base de datos

El directorio contiene utilidades SQL para inicializar o sincronizar el esquema de la aplicación.

## `AbstractDataBase.sql`
Crea toda la estructura base (tablas, índices, procedimientos almacenados y disparadores). Úsalo cuando necesites provisionar una base de datos desde cero.

## `UpdatePedidosSchema.sql`
Actualiza tablas y objetos relacionados con pedidos para que coincidan con el modelo Entity Framework Core (`Pedido` y `PedidoDetalle`).

Ejecuta este script cuando obtengas errores como **"Invalid column name 'Estado'"** o **"Invalid object name 'PedidoDetalles'"** al abrir la sección **Mis pedidos**. El script se encarga de:

- Renombrar columnas antiguas (`DireccionEntrega`, `EstadoPedido`, `MontoTotal`) a los nombres vigentes.
- Eliminar columnas obsoletas (`SuscripcionPausada`, `RowVer`).
- Crear la columna `MetodoPago` y asegurar los tipos/cantidades de caracteres esperados.
- Recrear los índices necesarios.
- Reemplazar la tabla `DetallePedido` por `PedidoDetalles` con la estructura correcta.

### Cómo ejecutarlo

```bash
sqlcmd -S <SERVIDOR> -d <BASE_DE_DATOS> -U <USUARIO> -P <CONTRASEÑA> -i DB/UpdatePedidosSchema.sql
```

También puedes abrir el archivo en Azure Data Studio o SQL Server Management Studio y ejecutarlo directamente.

> **Nota:** asegúrate de contar con un respaldo antes de ejecutar scripts en producción.
