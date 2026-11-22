
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Ev3Api.Data;
using Ev3Api.Models;

namespace Ev3Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CamionesController : ControllerBase
    {
        private readonly ConexionBD _conexionBD;
        public CamionesController(ConexionBD conexionBD)
        {
            _conexionBD = conexionBD;
        }

        // USUARIOS
        [HttpPost("crearNuevoUsuario")]
        public async Task<IActionResult> CrearNuevoUsuario(
            [FromBody] Usuario datos)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (string.IsNullOrWhiteSpace(datos.UsuarioId) ||
                string.IsNullOrWhiteSpace(datos.NombreCompleto) ||
                string.IsNullOrWhiteSpace(datos.Password))
                return BadRequest(new
                {
                    mensaje = "Todos los campos son obligatorios."
                });
            try
            {
                using SqlConnection conexion = _conexionBD.ObtenerConexion();
                await conexion.OpenAsync();
                string consulta = @"
            IF NOT EXISTS (
                SELECT 1
                FROM ESTADO
                WHERE ID_ESTADO = @Estado
            )
                THROW 50001, 'El estado indicado no existe.', 1;
            INSERT INTO USUARIOS
                (USUARIO, NOM_PERSONA, PASSWORD, ID_ESTADO)
            VALUES
                (@Usuario, @Nombre, @Password, @Estado);
            SELECT
                U.USUARIO,
                U.ID_ESTADO,
                E.NOM_ESTADO
            FROM USUARIOS U
            INNER JOIN ESTADO E
                ON E.ID_ESTADO = U.ID_ESTADO
            WHERE U.USUARIO = @Usuario;";
                using SqlCommand comando = new SqlCommand(consulta, conexion);
                comando.Parameters.Add("@Usuario", SqlDbType.VarChar, 20)
                    .Value = datos.UsuarioId.Trim();
                comando.Parameters.Add("@Nombre", SqlDbType.VarChar, 50)
                    .Value = datos.NombreCompleto.Trim();
                comando.Parameters.Add("@Password", SqlDbType.VarChar, 20)
                    .Value = datos.Password.Trim();
                comando.Parameters.Add("@Estado", SqlDbType.Int)
                    .Value = datos.IdEstado;
                using SqlDataReader lector = await comando.ExecuteReaderAsync();
                if (!await lector.ReadAsync())
                    return StatusCode(500, new
                    {
                        mensaje = "No fue posible recuperar el usuario creado."
                    });
                return Ok(new
                {
                    mensaje = "Usuario creado correctamente.",
                    usuario = lector["USUARIO"].ToString(),
                    idEstado = Convert.ToInt32(lector["ID_ESTADO"]),
                    nombreEstado = lector["NOM_ESTADO"].ToString()
                });
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                return Conflict(new { mensaje = "El usuario ya existe." });
            }
            catch (SqlException ex) when (ex.Number == 50001)
            {
                return BadRequest(new
                {
                    mensaje = "El estado indicado no existe en la tabla ESTADO."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al crear el usuario.",
                    detalle = ex.Message
                });
            }
        }


        // VALIDA LOGIN
        [HttpPost("validaLogin")]
        public async Task<IActionResult> ValidarAcceso(
            [FromBody] Acceso datos)
        {
            if (!ModelState.IsValid || !datos.TieneCredenciales())
                return BadRequest(new
                {
                    mensaje = "Debe ingresar nombre de persona y contraseña."
                });
            try
            {
                using SqlConnection conexion = _conexionBD.ObtenerConexion();
                await conexion.OpenAsync();
                string consulta = @"
            SELECT U.USUARIO, U.NOM_PERSONA, U.PASSWORD, U.ID_ESTADO
            FROM USUARIOS U
            WHERE U.NOM_PERSONA = @NombrePersona";
                using SqlCommand comando = new SqlCommand(consulta, conexion);
                comando.Parameters.Add("@NombrePersona", SqlDbType.VarChar, 50)
                    .Value = datos.Usuario.Trim();
                using SqlDataReader lector = await comando.ExecuteReaderAsync();
                if (!await lector.ReadAsync())
                    return Unauthorized(new { mensaje = "La persona no existe." });
                string passwordBD = lector["PASSWORD"].ToString() ?? "";
                int idEstado = Convert.ToInt32(lector["ID_ESTADO"]);
                if (passwordBD != datos.Password.Trim())
                    return Unauthorized(new { mensaje = "La contraseña es incorrecta." });
                if (idEstado != Estado.Activo)
                    return Unauthorized(new { mensaje = "La persona se encuentra INACTIVA." });
                return Ok(new
                {
                    mensaje = "Acceso autorizado.",
                    usuario = lector["USUARIO"].ToString(),
                    nombreCompleto = lector["NOM_PERSONA"].ToString(),
                    estado = "ACTIVO"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al validar el acceso.",
                    detalle = ex.Message
                });
            }
        }


        // ObtenerUsuario
        [HttpGet("usuarios")]
        public async Task<IActionResult> ObtenerUsuarios()
        {
            try
            {
                using SqlConnection conexion = _conexionBD.ObtenerConexion();
                await conexion.OpenAsync();

                string consulta = @"
                    SELECT U.USUARIO, U.NOM_PERSONA, U.ID_ESTADO
                    FROM USUARIOS U
                    WHERE U.USUARIO <> 'ADMIN'
                    ORDER BY U.USUARIO";

                using SqlCommand comando = new SqlCommand(consulta, conexion);
                using SqlDataReader lector = await comando.ExecuteReaderAsync();

                List<Usuario> usuarios = new List<Usuario>();

                while (await lector.ReadAsync())
                {
                    usuarios.Add(new Usuario
                    {
                        UsuarioId = lector["USUARIO"].ToString() ?? "",
                        NombreCompleto = lector["NOM_PERSONA"].ToString() ?? "",
                        Password = null,
                        IdEstado = Convert.ToInt32(lector["ID_ESTADO"])
                    });
                }

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al obtener los usuarios.",
                    detalle = ex.Message
                });
            }
        }

        //CambiarEstadoUsuario
        [HttpPut("estadoUsuario/{usuario}")]
        public async Task<IActionResult> CambiarEstadoUsuario(
            string usuario,
            [FromBody] Estado datos)
        {
            if (!ModelState.IsValid || !datos.EsEstadoValido())
                return BadRequest(new { mensaje = "Estado no válido." });

            if (usuario.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new
                {
                    mensaje = "No se puede modificar el estado de ADMIN."
                });

            try
            {
                using SqlConnection conexion = _conexionBD.ObtenerConexion();
                await conexion.OpenAsync();

                string consulta = @"
                    UPDATE USUARIOS
                    SET ID_ESTADO = @Estado
                    WHERE USUARIO = @Usuario";

                using SqlCommand comando = new SqlCommand(consulta, conexion);

                comando.Parameters.Add("@Estado", SqlDbType.Int).Value = datos.IdEstado;
                comando.Parameters.Add("@Usuario", SqlDbType.VarChar, 20).Value = usuario.Trim();

                int filas = await comando.ExecuteNonQueryAsync();

                if (filas == 0)
                    return NotFound(new { mensaje = "El usuario no existe." });

                return Ok(new
                {
                    mensaje = "Estado del usuario actualizado.",
                    usuario,
                    estado = datos.IdEstado == Estado.Activo ? "ACTIVO" : "INACTIVO"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al modificar el usuario.",
                    detalle = ex.Message
                });
            }
        }

        // agregarNuevaPlanta
        [HttpPost("agregarNuevaPlanta")]
        public async Task<IActionResult> CrearNuevaPlanta(
            [FromBody] Planta datos)
        {
            if (!ModelState.IsValid || !datos.TieneNombreValido())
                return BadRequest(new { mensaje = "Nombre de planta no válido." });
            try
            {
                using SqlConnection conexion = _conexionBD.ObtenerConexion();
                await conexion.OpenAsync();
                string consulta = @"
            IF EXISTS (
                SELECT 1
                FROM PLANTA
                WHERE UPPER(LTRIM(RTRIM(NOM_PLANTA))) =
                      UPPER(LTRIM(RTRIM(@Nombre)))
            )
                SELECT 0;
            ELSE
            BEGIN
                INSERT INTO PLANTA (NOM_PLANTA)
                OUTPUT INSERTED.ID_PLANTA
                VALUES (@Nombre);
            END";
                using SqlCommand comando = new SqlCommand(consulta, conexion);
                comando.Parameters.Add("@Nombre", SqlDbType.VarChar, 50)
                    .Value = datos.NombrePlanta.Trim();
                object resultado = await comando.ExecuteScalarAsync();
                int idPlanta = Convert.ToInt32(resultado);
                if (idPlanta == 0)
                    return Conflict(new
                    {
                        mensaje = "Ya existe una planta con ese nombre."
                    });
                datos.IdPlanta = idPlanta;
                return Ok(new
                {
                    mensaje = "Planta creada correctamente.",
                    planta = datos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al crear la planta.",
                    detalle = ex.Message
                });
            }
        }


        //ObtenerPlantas
        [HttpGet("plantas")]
        public async Task<IActionResult> ObtenerPlantas()
        {
            try
            {
                using SqlConnection conexion = _conexionBD.ObtenerConexion();
                await conexion.OpenAsync();
                string consulta = @"
                    SELECT ID_PLANTA, NOM_PLANTA
                    FROM PLANTA
                    ORDER BY NOM_PLANTA";
                using SqlCommand comando = new SqlCommand(consulta, conexion);
                using SqlDataReader lector = await comando.ExecuteReaderAsync();
                List<Planta> plantas = new List<Planta>();
                while (await lector.ReadAsync())
                {
                    plantas.Add(new Planta
                    {
                        IdPlanta = Convert.ToInt32(lector["ID_PLANTA"]),
                        NombrePlanta = lector["NOM_PLANTA"].ToString() ?? ""
                    });
                }
                return Ok(plantas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al obtener las plantas.",
                    detalle = ex.Message
                });
            }
        }

        //agregarNuevoCamion


        // agregarNuevoCamion
        [HttpPost("agregarNuevoCamion")]
        public async Task<IActionResult> CrearNuevoCamion(
            [FromBody] Camion datos)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    mensaje = "Los datos del camión no son válidos.",
                    errores = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                        )
                });

            if (!datos.TieneCodigoValido())
                return BadRequest(new
                {
                    mensaje = "El código del camión no es válido."
                });

            if (!datos.EsEstadoValido())
                return BadRequest(new
                {
                    mensaje = "El estado del camión no es válido."
                });

            try
            {
                using SqlConnection conexion = _conexionBD.ObtenerConexion();
                await conexion.OpenAsync();

                string verificarPlanta = @"
            SELECT COUNT(*)
            FROM PLANTA
            WHERE ID_PLANTA = @IdPlanta";

                using (SqlCommand validar = new SqlCommand(verificarPlanta, conexion))
                {
                    validar.Parameters.Add("@IdPlanta", SqlDbType.Int)
                        .Value = datos.IdPlanta;

                    if (Convert.ToInt32(await validar.ExecuteScalarAsync()) == 0)
                        return BadRequest(new
                        {
                            mensaje = "La planta no existe."
                        });
                }

                string consulta = @"
            INSERT INTO CAMION
                (COD_CAMION, ID_PLANTA, ID_ESTADO)
            VALUES
                (@Codigo, @Planta, @Estado)";

                using SqlCommand comando = new SqlCommand(consulta, conexion);

                comando.Parameters.Add("@Codigo", SqlDbType.VarChar, 10)
                    .Value = datos.NormalizarCodigo();

                comando.Parameters.Add("@Planta", SqlDbType.Int)
                    .Value = datos.IdPlanta;

                comando.Parameters.Add("@Estado", SqlDbType.Int)
                    .Value = datos.IdEstado;

                await comando.ExecuteNonQueryAsync();

                datos.CodigoCamion = datos.NormalizarCodigo();

                return Ok(new
                {
                    mensaje = "Camión registrado correctamente.",
                    camion = datos
                });
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                return Conflict(new
                {
                    mensaje = "El código del camión ya existe."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al registrar el camión.",
                    detalle = ex.Message
                });
            }
        }


        //btenerCamiones
        [HttpGet("camiones")]
        public async Task<IActionResult> ObtenerCamiones()
        {
            try
            {
                using SqlConnection conexion = _conexionBD.ObtenerConexion();
                await conexion.OpenAsync();
                string consulta = @"
                    SELECT C.COD_CAMION, C.ID_PLANTA, P.NOM_PLANTA,
                           C.ID_ESTADO, E.NOM_ESTADO
                    FROM CAMION C
                    INNER JOIN PLANTA P ON C.ID_PLANTA = P.ID_PLANTA
                    INNER JOIN ESTADO E ON C.ID_ESTADO = E.ID_ESTADO
                    ORDER BY C.COD_CAMION";
                using SqlCommand comando = new SqlCommand(consulta, conexion);
                using SqlDataReader lector = await comando.ExecuteReaderAsync();
                List<Camion> camiones = new List<Camion>();
                while (await lector.ReadAsync())
                {
                    camiones.Add(new Camion
                    {
                        CodigoCamion = lector["COD_CAMION"].ToString() ?? "",
                        IdPlanta = Convert.ToInt32(lector["ID_PLANTA"]),
                        NombrePlanta = lector["NOM_PLANTA"].ToString(),
                        IdEstado = Convert.ToInt32(lector["ID_ESTADO"]),
                        NombreEstado = lector["NOM_ESTADO"].ToString()
                    });
                }
                return Ok(camiones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al obtener los camiones.",
                    detalle = ex.Message
                });
            }
        }
        //ObtenerCamionesDisponible
        [HttpGet("camionesDisponibles/{idPlanta}")]
        public async Task<IActionResult> ObtenerCamionesDisponibles(int idPlanta)
        {
            try
            {
                using SqlConnection conexion = _conexionBD.ObtenerConexion();
                await conexion.OpenAsync();
                string consulta = @"
                    SELECT C.COD_CAMION, C.ID_PLANTA, P.NOM_PLANTA,
                           C.ID_ESTADO
                    FROM CAMION C
                    INNER JOIN PLANTA P ON C.ID_PLANTA = P.ID_PLANTA
                    WHERE C.ID_PLANTA = @Planta
                      AND C.ID_ESTADO = @Activo
                      AND NOT EXISTS (
                          SELECT 1 FROM SALIDA S
                          WHERE S.COD_CAMION = C.COD_CAMION
                            AND S.ID_ESTADO_SALIDA = @EnRuta
                      )
                    ORDER BY C.COD_CAMION";
                using SqlCommand comando = new SqlCommand(consulta, conexion);
                comando.Parameters.Add("@Planta", SqlDbType.Int).Value = idPlanta;
                comando.Parameters.Add("@Activo", SqlDbType.Int).Value = Estado.Activo;
                comando.Parameters.Add("@EnRuta", SqlDbType.Int).Value = Estado.EnRuta;
                using SqlDataReader lector = await comando.ExecuteReaderAsync();
                List<Camion> camiones = new List<Camion>();
                while (await lector.ReadAsync())
                {
                    camiones.Add(new Camion
                    {
                        CodigoCamion = lector["COD_CAMION"].ToString() ?? "",
                        IdPlanta = Convert.ToInt32(lector["ID_PLANTA"]),
                        NombrePlanta = lector["NOM_PLANTA"].ToString(),
                        IdEstado = Convert.ToInt32(lector["ID_ESTADO"])
                    });
                }
                return Ok(camiones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al obtener los camiones disponibles.",
                    detalle = ex.Message
                });
            }
        }

        //CambiarEstadoCamion
        [HttpPut("estadoCamion/{codigo}")]
        public async Task<IActionResult> CambiarEstadoCamion(
            string codigo,
            [FromBody] Estado datos)
        {
            if (!ModelState.IsValid || !datos.EsEstadoValido())
                return BadRequest(new { mensaje = "Estado no válido." });
            try
            {
                using SqlConnection conexion = _conexionBD.ObtenerConexion();
                await conexion.OpenAsync();
                string consulta = @"
                    UPDATE CAMION
                    SET ID_ESTADO = @Estado
                    WHERE COD_CAMION = @Codigo";

                using SqlCommand comando = new SqlCommand(consulta, conexion);
                comando.Parameters.Add("@Estado", SqlDbType.Int).Value = datos.IdEstado;
                comando.Parameters.Add("@Codigo", SqlDbType.VarChar, 10)
                    .Value = codigo.Trim().ToUpperInvariant();
                int filas = await comando.ExecuteNonQueryAsync();
                if (filas == 0)
                    return NotFound(new { mensaje = "El camión no existe." });
                return Ok(new
                {
                    mensaje = "Estado del camión actualizado.",
                    codigo,
                    estado = datos.IdEstado == Estado.Activo ? "ACTIVO" : "INACTIVO"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al cambiar el estado del camión.",
                    detalle = ex.Message
                });
            }
        }

        // agregarNuevoChofer"
        [HttpPost("agregarNuevoChofer")]
        public async Task<IActionResult> AgregarNuevoChofer([FromBody] Chofer datos)
        {
            // VALIDAR DATOS RECIBIDOS
            if (!ModelState.IsValid)
            {
                var errores = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                return BadRequest(new
                {
                    mensaje = "No se puede registrar el chofer.",
                    errores
                });
            }

            // VALIDAR NOMBRE
            if (!datos.TieneNombreValido())
            {
                return BadRequest(new
                {
                    mensaje = "El nombre del chofer es obligatorio y debe tener máximo 50 caracteres."
                });
            }

            // VALIDAR ESTADO
            if (datos.IdEstado != Estado.Activo && datos.IdEstado != Estado.Inactivo)
            {
                return BadRequest(new
                {
                    mensaje = "El estado del chofer no es válido."
                });
            }

            try
            {
                using SqlConnection conexion = _conexionBD.ObtenerConexion();
                await conexion.OpenAsync();

                // VERIFICAR SI EL CHOFER YA EXISTE
                string verificar = @"
            SELECT COUNT(*)
            FROM CHOFER
            WHERE UPPER(LTRIM(RTRIM(NOM_CHOFER))) =
                  UPPER(LTRIM(RTRIM(@NombreChofer)))";

                using SqlCommand comandoVerificar = new SqlCommand(verificar, conexion);

                comandoVerificar.Parameters.Add("@NombreChofer", SqlDbType.VarChar, 50)
                    .Value = datos.NombreChofer.Trim();

                int existe = Convert.ToInt32(await comandoVerificar.ExecuteScalarAsync());

                if (existe > 0)
                {
                    return Conflict(new
                    {
                        mensaje = "Ya existe un chofer registrado con ese nombre."
                    });
                }

                // VERIFICAR QUE EL ESTADO EXISTA
                string verificarEstado = @"
            SELECT COUNT(*)
            FROM ESTADO
            WHERE ID_ESTADO = @IdEstado";

                using SqlCommand comandoEstado = new SqlCommand(verificarEstado, conexion);

                comandoEstado.Parameters.Add("@IdEstado", SqlDbType.Int)
                    .Value = datos.IdEstado;

                int estadoExiste = Convert.ToInt32(await comandoEstado.ExecuteScalarAsync());

                if (estadoExiste == 0)
                {
                    return BadRequest(new
                    {
                        mensaje = "El estado seleccionado no existe."
                    });
                }

                // REGISTRAR CHOFER
                string insertar = @"
            INSERT INTO CHOFER (NOM_CHOFER, ID_ESTADO)
            OUTPUT INSERTED.ID_CHOFER
            VALUES (@NombreChofer, @IdEstado)";

                using SqlCommand comandoInsertar = new SqlCommand(insertar, conexion);

                comandoInsertar.Parameters.Add("@NombreChofer", SqlDbType.VarChar, 50)
                    .Value = datos.NombreChofer.Trim();

                comandoInsertar.Parameters.Add("@IdEstado", SqlDbType.Int)
                    .Value = datos.IdEstado;

                datos.IdChofer = Convert.ToInt32(await comandoInsertar.ExecuteScalarAsync());

                // OBTENER NOMBRE DEL ESTADO
                string consultar = @"
            SELECT NOM_ESTADO
            FROM ESTADO
            WHERE ID_ESTADO = @IdEstado";

                using SqlCommand comandoConsultar = new SqlCommand(consultar, conexion);

                comandoConsultar.Parameters.Add("@IdEstado", SqlDbType.Int)
                    .Value = datos.IdEstado;

                datos.NombreEstado = Convert.ToString(await comandoConsultar.ExecuteScalarAsync());

                return Ok(new
                {
                    mensaje = "Chofer registrado correctamente.",
                    chofer = datos
                });
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                return Conflict(new
                {
                    mensaje = "El chofer ya se encuentra registrado."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error interno al registrar el chofer.",
                    detalle = ex.Message
                });
            }
        }


        //ObtenerChoferes
        [HttpGet("choferes")]
        public async Task<IActionResult> ObtenerChoferes()
        {
            try
            {
                using SqlConnection conexion = _conexionBD.ObtenerConexion();
                await conexion.OpenAsync();
                string consulta = @"
                    SELECT C.ID_CHOFER, C.NOM_CHOFER,
                           C.ID_ESTADO, E.NOM_ESTADO
                    FROM CHOFER C
                    INNER JOIN ESTADO E ON C.ID_ESTADO = E.ID_ESTADO
                    ORDER BY C.ID_CHOFER";
                using SqlCommand comando = new SqlCommand(consulta, conexion);
                using SqlDataReader lector = await comando.ExecuteReaderAsync();
                List<Chofer> choferes = new List<Chofer>();

                while (await lector.ReadAsync())
                {
                    choferes.Add(new Chofer
                    {
                        IdChofer = Convert.ToInt32(lector["ID_CHOFER"]),
                        NombreChofer = lector["NOM_CHOFER"].ToString() ?? "",
                        IdEstado = Convert.ToInt32(lector["ID_ESTADO"]),
                        NombreEstado = lector["NOM_ESTADO"].ToString()
                    });
                }
                return Ok(choferes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al obtener los choferes.",
                    detalle = ex.Message
                });
            }
        }
        //ObtenerChoferesDisponibles
        [HttpGet("choferesDisponibles")]
        public async Task<IActionResult> ObtenerChoferesDisponibles()
        {
            try
            {
                using SqlConnection conexion = _conexionBD.ObtenerConexion();
                await conexion.OpenAsync();
                string consulta = @"
                    SELECT C.ID_CHOFER, C.NOM_CHOFER, C.ID_ESTADO
                    FROM CHOFER C
                    WHERE C.ID_ESTADO = @Activo
                      AND NOT EXISTS (
                          SELECT 1 FROM SALIDA S
                          WHERE S.ID_CHOFER = C.ID_CHOFER
                            AND S.ID_ESTADO_SALIDA = @EnRuta
                      )
                    ORDER BY C.NOM_CHOFER";
                using SqlCommand comando = new SqlCommand(consulta, conexion);
                comando.Parameters.Add("@Activo", SqlDbType.Int).Value = Estado.Activo;
                comando.Parameters.Add("@EnRuta", SqlDbType.Int).Value = Estado.EnRuta;
                using SqlDataReader lector = await comando.ExecuteReaderAsync();
                List<Chofer> choferes = new List<Chofer>();
                while (await lector.ReadAsync())
                {
                    choferes.Add(new Chofer
                    {
                        IdChofer = Convert.ToInt32(lector["ID_CHOFER"]),
                        NombreChofer = lector["NOM_CHOFER"].ToString() ?? "",
                        IdEstado = Convert.ToInt32(lector["ID_ESTADO"])
                    });
                }
                return Ok(choferes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al obtener los choferes disponibles.",
                    detalle = ex.Message
                });
            }
        }
        //CambiarEstadoChofer
        [HttpPut("estadoChofer/{id}")]
        public async Task<IActionResult> CambiarEstadoChofer(
            int id,
            [FromBody] Estado datos)
        {
            if (!ModelState.IsValid || !datos.EsEstadoValido())
                return BadRequest(new { mensaje = "Estado no válido." });
            try
            {
                using SqlConnection conexion = _conexionBD.ObtenerConexion();
                await conexion.OpenAsync();
                string consulta = @"
                    UPDATE CHOFER
                    SET ID_ESTADO = @Estado
                    WHERE ID_CHOFER = @Id";
                using SqlCommand comando = new SqlCommand(consulta, conexion);
                comando.Parameters.Add("@Estado", SqlDbType.Int).Value = datos.IdEstado;
                comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                int filas = await comando.ExecuteNonQueryAsync();
                if (filas == 0)
                    return NotFound(new { mensaje = "El chofer no existe." });
                return Ok(new
                {
                    mensaje = "Estado del chofer actualizado.",
                    idChofer = id,
                    estado = datos.IdEstado == Estado.Activo ? "ACTIVO" : "INACTIVO"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al cambiar el estado del chofer.",
                    detalle = ex.Message
                });
            }
        }

        //RegistrarSalida
        [HttpPost("SalidaCamion")]
        public async Task<IActionResult> RegistrarSalida(
            [FromBody] Salida datos)
        {
            if (!ModelState.IsValid || !datos.TieneFechaValida())
                return BadRequest(new { mensaje = "Debe indicar datos válidos para la salida." });

            if (string.IsNullOrWhiteSpace(datos.CodigoCamion) ||
                string.IsNullOrWhiteSpace(datos.Usuario))
                return BadRequest(new { mensaje = "Debe indicar camión y usuario." });

            using SqlConnection conexion = _conexionBD.ObtenerConexion();

            try
            {
                await conexion.OpenAsync();
                using SqlTransaction transaccion =
                    (SqlTransaction)await conexion.BeginTransactionAsync(
                        IsolationLevel.Serializable);
                try
                {
                    // Verificar usuario activo.
                    string consultaUsuario = @"
                        SELECT COUNT(*) FROM USUARIOS
                        WHERE USUARIO = @Usuario AND ID_ESTADO = @Activo";
                    using (SqlCommand comando = new SqlCommand(
                        consultaUsuario, conexion, transaccion))
                    {
                        comando.Parameters.Add("@Usuario", SqlDbType.VarChar, 20)
                            .Value = datos.Usuario.Trim();
                        comando.Parameters.Add("@Activo", SqlDbType.Int)
                            .Value = Estado.Activo;
                        if (Convert.ToInt32(await comando.ExecuteScalarAsync()) == 0)
                        {
                            await transaccion.RollbackAsync();
                            return BadRequest(new { mensaje = "El usuario no existe o está INACTIVO." });
                        }
                    }
                    // Verificar camión activo y perteneciente a la planta.
                    string consultaCamion = @"
                        SELECT COUNT(*) FROM CAMION
                        WHERE COD_CAMION = @Codigo
                          AND ID_PLANTA = @Planta
                          AND ID_ESTADO = @Activo";
                    using (SqlCommand comando = new SqlCommand(
                        consultaCamion, conexion, transaccion))
                    {
                        comando.Parameters.Add("@Codigo", SqlDbType.VarChar, 10)
                            .Value = datos.CodigoCamion.Trim().ToUpperInvariant();
                        comando.Parameters.Add("@Planta", SqlDbType.Int)
                            .Value = datos.IdPlanta;
                        comando.Parameters.Add("@Activo", SqlDbType.Int)
                            .Value = Estado.Activo;
                        if (Convert.ToInt32(await comando.ExecuteScalarAsync()) == 0)
                        {
                            await transaccion.RollbackAsync();
                            return BadRequest(new { mensaje = "El camión no existe, está INACTIVO o no pertenece a la planta." });
                        }
                    }
                    // Verificar chofer activo.
                    string consultaChofer = @"
                        SELECT COUNT(*) FROM CHOFER
                        WHERE ID_CHOFER = @IdChofer AND ID_ESTADO = @Activo";
                    using (SqlCommand comando = new SqlCommand(
                        consultaChofer, conexion, transaccion))
                    {
                        comando.Parameters.Add("@IdChofer", SqlDbType.Int)
                            .Value = datos.IdChofer;
                        comando.Parameters.Add("@Activo", SqlDbType.Int)
                            .Value = Estado.Activo;
                        if (Convert.ToInt32(await comando.ExecuteScalarAsync()) == 0)
                        {
                            await transaccion.RollbackAsync();
                            return BadRequest(new { mensaje = "El chofer no existe o está INACTIVO." });
                        }
                    }
                    // Verificar que el camión y el chofer estén disponibles.
                    string consultaDisponibilidad = @"
                        SELECT
                            CASE WHEN EXISTS (
                                SELECT 1 FROM SALIDA
                                WHERE COD_CAMION = @Codigo
                                  AND ID_ESTADO_SALIDA = @EnRuta
                            ) THEN 1 ELSE 0 END AS CamionOcupado,
                            CASE WHEN EXISTS (
                                SELECT 1 FROM SALIDA
                                WHERE ID_CHOFER = @IdChofer
                                  AND ID_ESTADO_SALIDA = @EnRuta
                            ) THEN 1 ELSE 0 END AS ChoferOcupado";
                    using (SqlCommand comando = new SqlCommand(
                        consultaDisponibilidad, conexion, transaccion))
                    {
                        comando.Parameters.Add("@Codigo", SqlDbType.VarChar, 10)
                            .Value = datos.CodigoCamion.Trim().ToUpperInvariant();
                        comando.Parameters.Add("@IdChofer", SqlDbType.Int)
                            .Value = datos.IdChofer;
                        comando.Parameters.Add("@EnRuta", SqlDbType.Int)
                            .Value = Estado.EnRuta;
                        using SqlDataReader lector = await comando.ExecuteReaderAsync();
                        if (await lector.ReadAsync())
                        {
                            bool camionOcupado = Convert.ToInt32(lector["CamionOcupado"]) == 1;
                            bool choferOcupado = Convert.ToInt32(lector["ChoferOcupado"]) == 1;

                            if (camionOcupado || choferOcupado)
                            {
                                await lector.CloseAsync();
                                await transaccion.RollbackAsync();
                                return Conflict(new
                                {
                                    mensaje = camionOcupado
                                        ? "El camión ya tiene una salida EN RUTA."
                                        : "El chofer ya tiene una salida EN RUTA."
                                });
                            }
                        }
                    }
                    // Registrar salida.
                    string insertar = @"
                        INSERT INTO SALIDA
                        (FECHA_SALIDA, ID_PLANTA, COD_CAMION,
                         ID_CHOFER, ID_ESTADO_SALIDA, USUARIO)
                        OUTPUT INSERTED.ID_SALIDA
                        VALUES
                        (@Fecha, @Planta, @Codigo,
                         @Chofer, @EstadoSalida, @Usuario)";
                    using SqlCommand comandoInsertar = new SqlCommand(
                        insertar, conexion, transaccion);
                    comandoInsertar.Parameters.Add("@Fecha", SqlDbType.Date)
                        .Value = datos.ObtenerFechaSoloFecha();
                    comandoInsertar.Parameters.Add("@Planta", SqlDbType.Int)
                        .Value = datos.IdPlanta;
                    comandoInsertar.Parameters.Add("@Codigo", SqlDbType.VarChar, 10)
                        .Value = datos.CodigoCamion.Trim().ToUpperInvariant();
                    comandoInsertar.Parameters.Add("@Chofer", SqlDbType.Int)
                        .Value = datos.IdChofer;
                    comandoInsertar.Parameters.Add("@EstadoSalida", SqlDbType.Int)
                        .Value = Estado.EnRuta;
                    comandoInsertar.Parameters.Add("@Usuario", SqlDbType.VarChar, 20)
                        .Value = datos.Usuario.Trim();
                    datos.IdSalida = Convert.ToInt32(
                        await comandoInsertar.ExecuteScalarAsync());
                    datos.IdEstadoSalida = Estado.EnRuta;
                    datos.NombreEstadoSalida = "EN RUTA";
                    await transaccion.CommitAsync();
                    return Ok(new
                    {
                        mensaje = "Salida registrada correctamente.",
                        salida = datos
                    });
                }
                catch
                {
                    try
                    {
                        await transaccion.RollbackAsync();
                    }
                    catch
                    {
                        // La transacción pudo haberse cerrado.
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al registrar la salida.",
                    detalle = ex.Message
                });
            }
        }
        
        //salidas
        [HttpGet("salidas")]
        public async Task<IActionResult> ObtenerSalidas()
        {
            try
            {
                using SqlConnection conexion = _conexionBD.ObtenerConexion();
                await conexion.OpenAsync();
                string consulta = @"
                    SELECT
                        S.ID_SALIDA,
                        S.FECHA_SALIDA,
                        S.ID_PLANTA,
                        P.NOM_PLANTA,
                        S.COD_CAMION,
                        S.ID_CHOFER,
                        C.NOM_CHOFER,
                        S.ID_ESTADO_SALIDA,
                        CASE
                            WHEN S.ID_ESTADO_SALIDA = 2 THEN 'RETORNO'
                            ELSE ES.NOM_ESTADO_SALIDA
                        END AS NOM_ESTADO_SALIDA,
                        S.USUARIO,
                        U.NOM_PERSONA
                    FROM SALIDA S
                    INNER JOIN PLANTA P ON S.ID_PLANTA = P.ID_PLANTA
                    INNER JOIN CAMION CA ON S.COD_CAMION = CA.COD_CAMION
                    INNER JOIN CHOFER C ON S.ID_CHOFER = C.ID_CHOFER
                    INNER JOIN ESTADO_SALIDA ES
                        ON S.ID_ESTADO_SALIDA = ES.ID_ESTADO_SALIDA
                    INNER JOIN USUARIOS U ON S.USUARIO = U.USUARIO
                    ORDER BY S.ID_SALIDA DESC";
                using SqlCommand comando = new SqlCommand(consulta, conexion);
                using SqlDataReader lector = await comando.ExecuteReaderAsync();
                List<Salida> salidas = new List<Salida>();
                while (await lector.ReadAsync())
                {
                    salidas.Add(new Salida
                    {
                        IdSalida = Convert.ToInt32(lector["ID_SALIDA"]),
                        FechaSalida = Convert.ToDateTime(lector["FECHA_SALIDA"]),
                        IdPlanta = Convert.ToInt32(lector["ID_PLANTA"]),
                        NombrePlanta = lector["NOM_PLANTA"].ToString(),
                        CodigoCamion = lector["COD_CAMION"].ToString() ?? "",
                        IdChofer = Convert.ToInt32(lector["ID_CHOFER"]),
                        NombreChofer = lector["NOM_CHOFER"].ToString(),
                        IdEstadoSalida = Convert.ToInt32(lector["ID_ESTADO_SALIDA"]),
                        NombreEstadoSalida = lector["NOM_ESTADO_SALIDA"].ToString(),
                        Usuario = lector["USUARIO"].ToString() ?? "",
                        NombreUsuario = lector["NOM_PERSONA"].ToString()
                    });
                }
                return Ok(salidas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al obtener las salidas.",
                    detalle = ex.Message
                });
            }
        }
        
        //retornarCamion
        [HttpPut("retornarCamion/{id}")]
        public async Task<IActionResult> RegistrarRetorno(int id)
        {
            try
            {
                using SqlConnection conexion = _conexionBD.ObtenerConexion();
                await conexion.OpenAsync();
                string consulta = @"
                    UPDATE SALIDA
                    SET ID_ESTADO_SALIDA = @Retorno
                    WHERE ID_SALIDA = @Id
                      AND ID_ESTADO_SALIDA = @EnRuta";
                using SqlCommand comando = new SqlCommand(consulta, conexion);
                comando.Parameters.Add("@Retorno", SqlDbType.Int)
                    .Value = Estado.Retorno;
                comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                comando.Parameters.Add("@EnRuta", SqlDbType.Int)
                    .Value = Estado.EnRuta;
                int filas = await comando.ExecuteNonQueryAsync();
                if (filas == 0)
                    return BadRequest(new
                    {
                        mensaje = "La salida no existe o no está EN RUTA."
                    });
                return Ok(new
                {
                    mensaje = "Retorno registrado correctamente.",
                    idSalida = id,
                    estado = "RETORNO"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al registrar el retorno.",
                    detalle = ex.Message
                });
            }
        }
    }
}
