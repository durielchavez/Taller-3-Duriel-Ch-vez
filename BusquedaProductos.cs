using System;
using System.Globalization;
using MySql.Data.MySqlClient;

namespace Northwind.Catalogo
{
    internal static class BusquedaProductos
    {
        public static void Ejecutar(MySqlConnection conexion)
        {
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine("BUSQUEDA DE PRODUCTOS POR PRECIO Y EXISTENCIAS");
            Console.WriteLine("============================================================");

            decimal precioMaximo = LeerDecimalNoNegativo("Ingrese el precio maximo por unidad: ");
            int existenciasMinimas = LeerEnteroNoNegativo("Ingrese las existencias minimas: ");

            string filtro =
                " FROM Products " +
                "WHERE UnitPrice <= @precioMaximo " +
                "AND UnitsInStock >= @existenciasMinimas " +
                "AND Discontinued = 0";

            string consulta =
                "SELECT ProductID, ProductName, UnitPrice, UnitsInStock, QuantityPerUnit" +
                filtro +
                " ORDER BY UnitPrice ASC, ProductID ASC;";

            int filas = 0;
            try
            {
                using (MySqlCommand comando = new MySqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@precioMaximo", MySqlDbType.Decimal).Value = precioMaximo;
                    comando.Parameters.Add("@existenciasMinimas", MySqlDbType.Int32).Value = existenciasMinimas;

                    using (MySqlDataReader lector = comando.ExecuteReader())
                    {
                        int colId = lector.GetOrdinal("ProductID");
                        int colNombre = lector.GetOrdinal("ProductName");
                        int colPrecio = lector.GetOrdinal("UnitPrice");
                        int colStock = lector.GetOrdinal("UnitsInStock");
                        int colPresentacion = lector.GetOrdinal("QuantityPerUnit");

                        while (lector.Read())
                        {
                            filas++;
                            int id = lector.GetInt32(colId);
                            string nombre = lector.GetString(colNombre);
                            decimal precio = lector.GetDecimal(colPrecio);
                            int stock = lector.GetInt32(colStock);
                            string presentacion = lector.IsDBNull(colPresentacion)
                                ? "Presentación sin registrar"
                                : lector.GetString(colPresentacion);

                            Console.WriteLine(
                                $"{id} | {nombre} | {precio.ToString("F2", CultureInfo.InvariantCulture)} | " +
                                $"{stock} | {presentacion}");
                        }
                    }
                }

                if (filas == 0)
                {
                    Console.WriteLine("No hay productos que cumplan los filtros");
                }
            }
            catch (MySqlException error)
            {
                Console.WriteLine("Error de MySQL durante la búsqueda: " + error.Message);
                return;
            }

            // El lector anterior ya fue liberado. Se ejecuta el conteo con los mismos filtros.
            try
            {
                string consultaConteo = "SELECT COUNT(*)" + filtro + ";";
                using (MySqlCommand comandoConteo = new MySqlCommand(consultaConteo, conexion))
                {
                    comandoConteo.Parameters.Add("@precioMaximo", MySqlDbType.Decimal).Value = precioMaximo;
                    comandoConteo.Parameters.Add("@existenciasMinimas", MySqlDbType.Int32).Value = existenciasMinimas;

                    long total = Convert.ToInt64(comandoConteo.ExecuteScalar());
                    Console.WriteLine("Total de productos que cumplen los filtros: " + total);
                    Console.WriteLine("Filas mostradas: " + filas);
                }
            }
            catch (MySqlException error)
            {
                Console.WriteLine("Error de MySQL durante el conteo: " + error.Message);
            }
        }

        public static void BuscarPorParteDelNombre(MySqlConnection conexion)
        {
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine("BUSQUEDA DE PRODUCTOS POR PARTE DEL NOMBRE");
            Console.WriteLine("============================================================");

            string fragmento;
            do
            {
                Console.Write("Ingrese parte del nombre del producto: ");
                fragmento = (Console.ReadLine() ?? string.Empty).Trim();

                if (fragmento.Length == 0)
                {
                    Console.WriteLine("Debe ingresar un texto. No se aceptan entradas vacías o con solo espacios.");
                }
            }
            while (fragmento.Length == 0);

            string consulta =
                "SELECT ProductID, ProductName, UnitPrice, UnitsInStock " +
                "FROM Products " +
                "WHERE Discontinued = 0 AND ProductName LIKE @patron " +
                "ORDER BY ProductName ASC, ProductID ASC;";

            int filas = 0;
            try
            {
                using (MySqlCommand comando = new MySqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@patron", MySqlDbType.VarChar).Value = "%" + fragmento + "%";

                    using (MySqlDataReader lector = comando.ExecuteReader())
                    {
                        int colId = lector.GetOrdinal("ProductID");
                        int colNombre = lector.GetOrdinal("ProductName");
                        int colPrecio = lector.GetOrdinal("UnitPrice");
                        int colStock = lector.GetOrdinal("UnitsInStock");

                        while (lector.Read())
                        {
                            filas++;
                            int id = lector.GetInt32(colId);
                            string nombre = lector.GetString(colNombre);
                            decimal precio = lector.GetDecimal(colPrecio);
                            int stock = lector.GetInt32(colStock);

                            Console.WriteLine(
                                $"{id} | {nombre} | {precio.ToString("F2", CultureInfo.InvariantCulture)} | {stock}");
                        }
                    }
                }

                if (filas == 0)
                {
                    Console.WriteLine("No hay productos activos que coincidan con el nombre");
                }
            }
            catch (MySqlException error)
            {
                Console.WriteLine("Error de MySQL durante la búsqueda por nombre: " + error.Message);
            }
        }

        private static decimal LeerDecimalNoNegativo(string mensaje)
        {
            while (true)
            {
                Console.Write(mensaje);
                string entrada = (Console.ReadLine() ?? string.Empty).Trim();

                if (decimal.TryParse(entrada, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal valor)
                    && valor >= 0)
                {
                    return valor;
                }

                Console.WriteLine("Entrada no válida. Escriba un número decimal mayor o igual que cero.");
            }
        }

        private static int LeerEnteroNoNegativo(string mensaje)
        {
            while (true)
            {
                Console.Write(mensaje);
                string entrada = (Console.ReadLine() ?? string.Empty).Trim();

                if (int.TryParse(entrada, NumberStyles.Integer, CultureInfo.CurrentCulture, out int valor)
                    && valor >= 0)
                {
                    return valor;
                }

                Console.WriteLine("Entrada no válida. Escriba un número entero mayor o igual que cero.");
            }
        }
    }
}
