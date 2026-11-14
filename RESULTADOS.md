# Taller 3: consultas de productos con ADO.NET

**Estudiante:** Duriel Chávez  
**Asignatura:** Programación III  
**Proyecto base:** Northwind-Catalogo  
**Tecnologías:** C#, ADO.NET, MySQL

## 1. Exploración del tutorial (`AspectosAdoNet.cs`)

El archivo de referencia organiza el recorrido en recursos, parámetros y resultados, tipos y valores NULL, errores y transacciones. Las siguientes observaciones describen el comportamiento que debe verificarse al ejecutar cada variante. Completa las columnas de resultado observado con la salida de tu ejecución real antes de publicar el repositorio.

| Aspecto | Cambio/prueba indicada | Qué debe observarse | Resultado real |
|---|---|---|---|
| Recursos | Cambiar `LIMIT 2` por `LIMIT 5` | Se muestran cinco nombres si hay al menos cinco productos. El lector se libera antes que el comando y la conexión. | Completar tras ejecutar |
| Parámetros | Probar ID 1, 99999, 0 y texto | Un ID existente devuelve nombre; uno inexistente no devuelve producto; 0 y texto se rechazan por la validación. | Completar tras ejecutar |
| Tipos y NULL | Cambiar ID 5 a 1 y luego 99999 | Se comparan precio registrado, valor cero y `NULL`. `IsDBNull` permite detectar ausencia de valor; cero no es NULL. | Completar tras ejecutar |
| Error de consulta | Ejecutar con tabla inexistente y luego restaurar el nombre | MySQL genera una excepción de consulta; la comprobación posterior `SELECT 1` permite observar que la conexión sigue abierta. | Completar tras ejecutar |
| Error de conexión | Intentar conexión al puerto local 1 | Se espera un error al abrir la conexión, antes de ejecutar SQL. | Completar tras ejecutar |
| Clave duplicada | Repetir la inserción de ID 1 en tabla temporal | La segunda inserción viola la clave primaria y genera error de duplicado. | Completar tras ejecutar |
| Transacciones | Ejecutar transferencia correcta y transferencia con error | `Commit` conserva ambos cambios; `Rollback` revierte el descuento si falla el segundo paso. Los saldos esperados del tutorial son 80.00 y 70.00 después del caso correcto y permanecen iguales tras el rollback. | Completar tras ejecutar |

## 2. Implementación de las consultas

Se agregó `Northwind-Catalogo/BusquedaProductos.cs`, dentro del namespace `Northwind.Catalogo`.

### Búsqueda por precio máximo y existencias mínimas

La consulta filtra `UnitPrice <= @precioMaximo`, `UnitsInStock >= @existenciasMinimas` y `Discontinued = 0`. Los resultados se ordenan por precio ascendente y después por `ProductID`. Se muestran ProductID, ProductName, UnitPrice, UnitsInStock y QuantityPerUnit. El precio se lee como `decimal` y se presenta con dos decimales. Antes de leer QuantityPerUnit se utiliza `IsDBNull`; cuando no hay dato se muestra “Presentación sin registrar”.

### Conteo

Después de liberar el lector se ejecuta `SELECT COUNT(*)` con exactamente los mismos filtros y parámetros. `ExecuteScalar` devuelve el total. Ese número debe coincidir con la cantidad de filas impresas.

### Búsqueda por parte del nombre

La tercera consulta es independiente de los filtros numéricos. Solicita un fragmento, aplica `Trim()` y vuelve a pedirlo si está vacío o contiene solo espacios. Utiliza `LIKE @patron`, asignando `%fragmento%` al parámetro, filtra `Discontinued = 0` y ordena por `ProductName` y `ProductID`, ambos ascendentemente.

## 3. Explicación técnica y seguridad

Los parámetros separan la instrucción SQL de los valores que ingresa la persona. Por ejemplo, `@precioMaximo` y `@existenciasMinimas` son marcadores; sus valores se asignan mediante `Parameters.Add`. Esto evita concatenar entradas dentro del SQL y reduce el riesgo de inyección SQL. La validación previa también impide enviar números negativos, texto o existencias decimales cuando se requiere un entero.

Cada `using` administra la vida de un recurso. El `using` de `MySqlCommand` libera el comando y el `using` de `MySqlDataReader` libera el lector al terminar de recorrer las filas, incluso si ocurre una excepción. El lector debe cerrarse antes de ejecutar el conteo en la misma conexión. La conexión recibida por los métodos permanece abierta y es administrada por `Program`.

Es importante diferenciar tres situaciones. Cero coincidencias significa que la consulta se ejecutó correctamente, pero no encontró filas; no es un error. `NULL` es un valor ausente en una columna de una fila existente y debe comprobarse con `IsDBNull` antes de leerlo. Una `MySqlException` indica un problema de conexión o de ejecución SQL y debe manejarse por separado.

## 4. Casos de prueba requeridos

**Importante:** los resultados exactos dependen de la base Northwind cargada en el equipo. Esta tabla es una guía de ejecución, no una afirmación de pruebas ya ejecutadas. Registra la salida real de la consola en cada caso.

| N.º | Entrada o prueba | Criterio de aceptación | Resultado observado |
|---:|---|---|---|
| 1 | Precio máximo 20; stock mínimo 10 | Todas las filas cumplen ambos límites y están activas; conteo igual al número de filas. | Pendiente de ejecutar |
| 2 | Precio máximo 0; stock mínimo 0 | Mostrar el mensaje de ausencia de coincidencias si el catálogo inicial no contiene productos con precio cero. | Pendiente de ejecutar |
| 3 | Precio máximo 1000; stock mínimo 10000 | Si no hay coincidencias, mostrar el mensaje correspondiente; no tratarlo como error SQL o de conexión. | Pendiente de ejecutar |
| 4 | Precio negativo o stock negativo | Rechazar la entrada y volver a solicitarla antes de ejecutar SQL. | Pendiente de ejecutar |
| 5 | Introducir texto en un campo numérico | Rechazar la entrada sin cerrar inesperadamente el programa. | Pendiente de ejecutar |
| 6 | Introducir stock con decimales | Rechazarlo porque las existencias deben ser un entero. | Pendiente de ejecutar |
| 7 | Usar el precio y stock exactos de un producto activo como límites | El producto debe aparecer porque los operadores son inclusivos (`<=` y `>=`). | Pendiente de ejecutar |
| 8 | Probar `CAST(NULL AS CHAR)` como presentación en una consulta de prueba | La fila debe mostrar “Presentación sin registrar”; después restaurar la consulta normal. | Pendiente de ejecutar |
| 9 | Introducir un fragmento de un producto activo | Debe aparecer el producto; todos los resultados activos y ordenados por nombre e ID. | Pendiente de ejecutar |
| 10 | Introducir `zzzzsincoincidencias999` | Mostrar “No hay productos activos que coincidan con el nombre”, sin tratarlo como error. | Pendiente de ejecutar |
| 11 | Presionar Enter y luego introducir solo espacios | Ambas entradas deben rechazarse y solicitarse nuevamente antes de ejecutar SQL. | Pendiente de ejecutar |

## 5. Comparación del orden

Para contrastar el orden, cambia temporalmente `ORDER BY UnitPrice ASC, ProductID ASC` por `ORDER BY UnitPrice DESC, ProductID ASC`. El conjunto de productos debe ser el mismo porque los filtros no cambian; cambia la posición de las filas según el precio. Para la entrega se debe restaurar el orden ascendente solicitado.

**Resultado real de la comparación:** completar después de ejecutar ambas versiones y conservar la salida o captura correspondiente.

## 6. Integración en `Program.cs`

La llamada debe realizarse mientras la conexión esté abierta. En el `Main` del proyecto de partida, conserva la comprobación inicial de conexión y cantidad de productos. Para probar el taller por separado, comenta temporalmente la llamada a `AspectosAdoNet.Ejecutar(cadenaConexion)` y agrega el bloque siguiente dentro del `try`, después de abrir la conexión:

```csharp
using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
{
    conexion.Open();
    Console.WriteLine("Conexión abierta correctamente.");

    BusquedaProductos.Ejecutar(conexion);
    BusquedaProductos.BuscarPorParteDelNombre(conexion);
}
```

La cadena de conexión debe contener los datos locales del estudiante, pero antes de publicar el repositorio se deben sustituir por `TU_USUARIO` y `TU_CONTRASENA`. Nunca se debe subir una contraseña real a GitHub.

## 7. Pasos para completar y publicar

1. Descargar o clonar el repositorio original: https://github.com/jaime-oteima/northwind-adonet
2. Copiar `BusquedaProductos.cs` dentro de `Northwind-Catalogo`.
3. Integrar las llamadas indicadas en `Program.cs`.
4. Ejecutar el tutorial y documentar los resultados reales en esta hoja.
5. Ejecutar los once casos de prueba y completar la columna “Resultado observado”.
6. Restaurar el orden ascendente y cualquier variante temporal del tutorial.
7. Colocar este archivo `RESULTADOS.md` en la raíz del repositorio.
8. Compilar y ejecutar el proyecto. Publicar los cambios en un repositorio de GitHub propio y entregar el enlace en Moodle.

## 8. Verificación pendiente antes de entregar

- [ ] Se ejecutó el proyecto con la base Northwind y se confirmó la conexión.
- [ ] Se probó la búsqueda por precio y existencias.
- [ ] El total de `COUNT(*)` coincidió con las filas del listado.
- [ ] Se probó la búsqueda por fragmento del nombre.
- [ ] Se completaron los resultados reales de los once casos.
- [ ] Se completó la exploración del tutorial con observaciones de ejecución.
- [ ] Se restauró el orden ascendente.
- [ ] Se revisó que no haya contraseñas reales en los archivos o commits.
- [ ] Se publicó el repositorio y se copió su enlace en Moodle.
