using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using DAL;

namespace SERVICIOS
{
    /// <summary>
    /// Migra datos personales existentes a encriptación reversible AES-256.
    /// Sirve para aplicar encriptación sobre bases de datos legacy sin necesidad
    /// de borrar los datos existentes.
    /// </summary>
    public class CriptoMigracion
    {
        private readonly DalGeneral _dal;
        private readonly CriptoManager _criptoManager;

        public CriptoMigracion()
        {
            _dal = new DalGeneral();
            _criptoManager = new CriptoManager();
        }

        public class ResultadoMigracion
        {
            public string Tabla { get; set; }
            public string Campo { get; set; }
            public int TotalFilas { get; set; }
            public int Encriptadas { get; set; }
            public int YaEncriptadas { get; set; }
            public int LegacyReencriptadas { get; set; }
            public int Errores { get; set; }
            public string MensajeError { get; set; }
        }

        /// <summary>
        /// Encripta todos los campos sensibles de todas las tablas soportadas.
        /// Devuelve un resumen por tabla/campo.
        /// </summary>
        public System.Collections.Generic.List<ResultadoMigracion> EncriptarTodo()
        {
            var resultados = new System.Collections.Generic.List<ResultadoMigracion>();

            resultados.Add(EncriptarCampo("USUARIOS", "usr", false));      // usr NO se encripta (PK y FK)
            resultados.Add(EncriptarCampo("USUARIOS", "nombre", true));
            resultados.Add(EncriptarCampo("USUARIOS", "apellido", true));
            resultados.Add(EncriptarCampo("USUARIOS", "telefono", true));
            resultados.Add(EncriptarCampo("USUARIOS", "email", true));
            resultados.Add(EncriptarCampo("USUARIOS", "fechaNacimiento", true, true));

            resultados.Add(EncriptarCampo("PreguntasSeguridad", "pregunta", true));
            resultados.Add(EncriptarCampo("PreguntasSeguridad", "respuesta", true));

            // Tablas legacy que aún puedan tener datos personales
            if (ColumnaExiste("ALUMNOS", "nombre"))
                resultados.Add(EncriptarCampo("ALUMNOS", "nombre", true));
            if (ColumnaExiste("ALUMNOS", "apellido"))
                resultados.Add(EncriptarCampo("ALUMNOS", "apellido", true));
            if (ColumnaExiste("ALUMNOS", "telefono"))
                resultados.Add(EncriptarCampo("ALUMNOS", "telefono", true));
            if (ColumnaExiste("ALUMNOS", "fechaNacimiento"))
                resultados.Add(EncriptarCampo("ALUMNOS", "fechaNacimiento", true, true));

            if (ColumnaExiste("ENTRENADORES", "nombre"))
                resultados.Add(EncriptarCampo("ENTRENADORES", "nombre", true));
            if (ColumnaExiste("ENTRENADORES", "apellido"))
                resultados.Add(EncriptarCampo("ENTRENADORES", "apellido", true));
            if (ColumnaExiste("ENTRENADORES", "telefono"))
                resultados.Add(EncriptarCampo("ENTRENADORES", "telefono", true));
            if (ColumnaExiste("ENTRENADORES", "fechaNacimiento"))
                resultados.Add(EncriptarCampo("ENTRENADORES", "fechaNacimiento", true, true));

            return resultados;
        }

        /// <summary>
        /// Encripta un campo específico de una tabla. Los valores en texto plano se encriptan,
        /// los valores en formato legacy se re-encriptan con IV aleatorio, y los valores ya
        /// en formato nuevo se saltan.
        /// </summary>
        public ResultadoMigracion EncriptarCampo(string tabla, string campo, bool encriptar, bool esFecha = false)
        {
            var resultado = new ResultadoMigracion
            {
                Tabla = tabla,
                Campo = campo,
                TotalFilas = 0,
                Encriptadas = 0,
                YaEncriptadas = 0,
                LegacyReencriptadas = 0,
                Errores = 0
            };

            try
            {
                if (!TablaExiste(tabla))
                {
                    resultado.MensajeError = $"La tabla {tabla} no existe. Se omite.";
                    return resultado;
                }

                if (!ColumnaExiste(tabla, campo))
                {
                    resultado.MensajeError = $"La columna {campo} no existe en {tabla}. Se omite.";
                    return resultado;
                }

                string clavePrimaria = ObtenerClavePrimaria(tabla);
                if (string.IsNullOrEmpty(clavePrimaria))
                {
                    clavePrimaria = campo; // fallback, aunque no debería pasar
                }

                string consulta = $@"SELECT [{clavePrimaria}], [{campo}] FROM [GymApp].[dbo].[{tabla}] WHERE [{campo}] IS NOT NULL";
                DataTable dt = _dal._686DPConsultar(consulta, new List<SqlParameter>());

                resultado.TotalFilas = dt.Rows.Count;

                foreach (DataRow row in dt.Rows)
                {
                    object id = row[clavePrimaria];
                    object valorOriginal = row[campo];

                    if (valorOriginal == null || valorOriginal == DBNull.Value)
                    {
                        resultado.YaEncriptadas++;
                        continue;
                    }

                    string texto = valorOriginal.ToString();
                    if (string.IsNullOrWhiteSpace(texto))
                    {
                        resultado.YaEncriptadas++;
                        continue;
                    }

                    string valorPlano;
                    bool esNuevoFormato = _criptoManager.EsFormatoNuevo(texto);
                    bool esFormatoLegacy = !esNuevoFormato && _criptoManager.EsFormatoLegacy(texto);

                    // Si ya está en el formato nuevo (IV aleatorio), no lo tocamos.
                    if (esNuevoFormato)
                    {
                        resultado.YaEncriptadas++;
                        continue;
                    }

                    // Si está en el formato legacy (IV fijo), lo desencriptamos para
                    // volver a encriptarlo con IV aleatorio.
                    if (esFormatoLegacy)
                    {
                        valorPlano = _criptoManager.DesencriptarAES256(texto);
                        resultado.LegacyReencriptadas++;
                    }
                    else
                    {
                        valorPlano = texto;
                    }

                    string encriptado;
                    if (esFecha)
                    {
                        // Las fechas se guardan como string yyyy-MM-dd encriptado
                        DateTime fecha;
                        if (DateTime.TryParse(valorPlano, out fecha))
                            encriptado = _criptoManager.EncriptarAES256(fecha.ToString("yyyy-MM-dd"));
                        else
                            encriptado = _criptoManager.EncriptarAES256(valorPlano);
                    }
                    else
                    {
                        encriptado = _criptoManager.EncriptarAES256(valorPlano);
                    }

                    string update = $@"UPDATE [GymApp].[dbo].[{tabla}] SET [{campo}] = @Valor WHERE [{clavePrimaria}] = @Id";
                    List<SqlParameter> parametros = new List<SqlParameter>
                    {
                        new SqlParameter("@Valor", encriptado),
                        new SqlParameter("@Id", id)
                    };

                    _dal._686DPEscribir(update, parametros);
                    resultado.Encriptadas++;
                }
            }
            catch (Exception ex)
            {
                resultado.Errores++;
                resultado.MensajeError = ex.Message;
            }

            return resultado;
        }

        /// <summary>
        /// Detecta si un valor ya fue encriptado con AES-256, tanto en el formato nuevo
        /// (IV aleatorio) como en el formato legacy (IV fijo).
        /// </summary>
        private bool EsValorEncriptado(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return false;

            return _criptoManager.EsFormatoNuevo(valor) || _criptoManager.EsFormatoLegacy(valor);
        }

        private bool TablaExiste(string tabla)
        {
            string consulta = @"
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @Tabla";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                new SqlParameter("@Tabla", tabla)
            };

            object resultado = _dal._686DPEscalar(consulta, parametros);
            return resultado != null && Convert.ToInt32(resultado) > 0;
        }

        private bool ColumnaExiste(string tabla, string columna)
        {
            string consulta = @"
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = 'dbo'
                  AND TABLE_NAME = @Tabla
                  AND COLUMN_NAME = @Columna";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                new SqlParameter("@Tabla", tabla),
                new SqlParameter("@Columna", columna)
            };

            object resultado = _dal._686DPEscalar(consulta, parametros);
            return resultado != null && Convert.ToInt32(resultado) > 0;
        }

        private string ObtenerClavePrimaria(string tabla)
        {
            string consulta = @"
                SELECT COLUMN_NAME
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') = 1
                  AND TABLE_SCHEMA = 'dbo'
                  AND TABLE_NAME = @Tabla";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                new SqlParameter("@Tabla", tabla)
            };

            object resultado = _dal._686DPEscalar(consulta, parametros);
            return resultado != null && resultado != DBNull.Value ? resultado.ToString() : string.Empty;
        }
    }
}
