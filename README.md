# API de Granja lechera en .NET con arquitectura en capas

## Tecnologías

- .NET 10 / ASP.NET Core Web API (controllers)
- Entity Framework Core 10 + SQLite, con migraciones
- AutoMapper (mapeo de entidades a DTOs)
- Swagger / OpenAPI (Swashbuckle)
- ProblemDetails para el manejo global de errores
- xUnit para las pruebas (SQLite en memoria)

## Endpoints

| Método | Ruta | Descripción | Respuestas |
|---|---|---|---|
| GET | `/api/granja` | Configuración (n, l) y parámetros por producto | 200 |
| PUT | `/api/granja` | Actualiza n, l, rendimiento, vida útil y precio (y ajusta las vacas activas a n) | 200, 400 |
| GET | `/api/vacas?incluirInactivas=&pagina=&tamanoPagina=` | Listado paginado de vacas con los litros producidos | 200, 400 |
| GET | `/api/vacas/{id}` | Consulta una vaca | 200, 404 |
| POST | `/api/produccion` | Registra la producción del día (`{ "fecha"? }`) | 201, 409 |
| GET | `/api/produccion?desde=&hasta=&pagina=&tamanoPagina=` | Producción por día (paginada) | 200, 400 |
| GET | `/api/produccion/{fecha}` | Producción de un día: producidos, transformados y disponibles | 200, 404 |
| POST | `/api/produccion/transformaciones` | Transforma leche en queso o yogurt y crea un lote | 201, 400, 404, 409 |
| GET | `/api/inventario?tipo=&estado=&soloDisponibles=&desde=&hasta=&fecha=&pagina=&tamanoPagina=` | Lotes del inventario (paginado, con filtros) | 200, 400 |
| GET | `/api/inventario/{id}` | Consulta un lote | 200, 404 |
| GET | `/api/inventario/resumen?fecha=` | Existencias vendibles, vencidas y dañadas por producto | 200 |
| GET | `/api/inventario/proximos-a-vencer?dias=3&fecha=` | Alerta de lotes próximos a vencer (extensión opcional) | 200 |
| POST | `/api/inventario/procesar-vencimientos` | Pasa a dañado el disponible de los lotes vencidos | 200 |
| POST | `/api/ventas` | Registra una venta FIFO | 201, 400, 409 |
| GET | `/api/ventas?desde=&hasta=&tipo=&cantidadMinima=&cantidadMaxima=&ordenarPor=&descendente=&pagina=&tamanoPagina=` | Lista paginada de ventas con filtros avanzados | 200, 400 |
| GET | `/api/ventas/{id}` | Consulta una venta con su detalle por lote | 200, 404 |

### Reportes

| Método | Ruta | Contenido |
|---|---|---|
| GET | `/api/reportes/ventas?desde=&hasta=&tipo=` | Ventas del período, totales por producto y total general |
| GET | `/api/reportes/leche-diaria?desde=&hasta=` | Leche producida por día, litros transformados y no aprovechados |
| GET | `/api/reportes/producto-diario?desde=&hasta=` | Producto elaborado por día y por tipo |
| GET | `/api/reportes/producto-total` | Por producto: producido, vendido, disponible, dañado e ingresos |
| GET | `/api/reportes/producto-danado?desde=&hasta=` | Producto dañado por lote y por tipo, con fecha de vencimiento |
| GET | `/api/reportes/vacas?desde=&hasta=` | Producción promedio, la vaca que más produce, la que menos y las que están por debajo del promedio |
| GET | `/api/reportes/{reporte}/csv?desde=&hasta=&tipo=` | Exporta cualquiera de los reportes anteriores a CSV (`ventas`, `leche-diaria`, `producto-diario`, `producto-total`, `producto-danado`, `vacas`) |

Las fechas usan el formato ISO `yyyy-MM-dd` y los productos se escriben como texto (`"Queso"`, `"Yogurt"`).

### Ejemplo de venta

```http
POST /api/ventas
{ "tipoProducto": "Queso", "cantidad": 12, "fecha": "2026-12-03" }
```

Respuesta `201 Created`:

```json
{
  "id": 1, "fecha": "2026-12-03", "tipoProducto": "Queso", "cantidad": 12,
  "precioUnitario": 28000, "total": 336000,
  "detalle": [
    { "loteId": 1, "fechaProduccion": "2026-12-01", "cantidad": 10 },
    { "loteId": 3, "fechaProduccion": "2026-12-02", "cantidad": 2 }
  ]
}
```

## Extensiones opcionales

### Alerta de lotes próximos a vencer

`GET /api/inventario/proximos-a-vencer?dias=3&fecha=2026-12-14` devuelve los lotes vendibles que vencen dentro de los próximos `dias` días.

### Exportación de reportes a CSV

`GET /api/reportes/{reporte}/csv` descarga el reporte como archivo (`Content-Disposition: attachment`). Acepta los mismos filtros que la versión JSON y reutiliza los mismos datos de `IReporteService`, así que ambas versiones siempre coinciden.

- Formato RFC 4180: separador coma, decimales con punto, fechas `yyyy-MM-dd` y codificación UTF-8 con BOM para que Excel muestre bien las tildes.
- Los reportes de ventas, leche diaria, producto dañado y vacas incluyen al final sus filas de totales.
- Un reporte desconocido devuelve 400.

```csv
Producto,Unidad,Producido,Vendido,Disponible,Danado,Ingresos
Queso,kg,25,12,13,0,336000
Yogurt,L,50,20,0,30,120000
```

### Paginación y filtros avanzados

Los listados de ventas, inventario, vacas y producción reciben `pagina` (desde 1) y `tamanoPagina` (de 1 a 100, por defecto 20), y responden así:

```json
{ "items": [ ... ], "pagina": 1, "tamanoPagina": 20, "totalRegistros": 2, "totalPaginas": 1, "tieneAnterior": false, "tieneSiguiente": false }
```

| Listado | Filtros |
|---|---|
| `/api/ventas` | `desde`, `hasta`, `tipo`, `cantidadMinima`, `cantidadMaxima`, `ordenarPor` (`Fecha`, `Cantidad` o `Total`), `descendente` |
| `/api/inventario` | `tipo`, `estado` (`Disponible`, `Agotado` o `Vencido`), `soloDisponibles`, `desde`/`hasta` (fecha de producción), `fecha` (para evaluar el estado) |
| `/api/vacas` | `incluirInactivas` |
| `/api/produccion` | `desde`, `hasta` |

- Las ventas se filtran, ordenan y paginan directamente en la base de datos (`IVentaRepository.BuscarPaginadoAsync` usa `Skip`/`Take`).
- En el inventario el estado de un lote depende de la fecha de evaluación y lo calcula el dominio, así que ese listado se pagina en memoria.

## Cómo ejecutar

Requisitos: SDK de .NET 10.

```bash
dotnet restore
dotnet build
dotnet run --project src/Granja.Web --launch-profile http
```

- Swagger: <http://localhost:5161/swagger>. La raíz `/` redirige allí.
- Al arrancar se aplican las migraciones y, si la base está vacía, se crean la granja y las n vacas. La base SQLite queda en `src/Granja.Web/granja.db`; bórrala para empezar de cero.


## Pruebas unitarias

```bash
dotnet test
```
