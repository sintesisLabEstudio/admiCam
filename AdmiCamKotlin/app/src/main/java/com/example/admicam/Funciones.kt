package com.example.admicam
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.PUT
import retrofit2.http.Path
interface Funciones {
    @POST("api/Camiones/validaLogin")
    suspend fun validarLogin(@Body credenciales: Credenciales): Response<RespuestaApi<Usuario>>
    @GET("api/Camiones/listarUsuarios")
    suspend fun listarUsuarios(): Response<RespuestaApi<List<Usuario>>>
    @POST("api/Camiones/crearNuevoUsuario")
    suspend fun crearUsuario(@Body usuario: Usuario): Response<RespuestaApi<Usuario>>
    @PUT("api/Camiones/estadoUsuario/{usuario}")
    suspend fun estadoUsuario(@Path("usuario") usuario: String, @Body estado: Estado): Response<RespuestaApi<Usuario>>
    @GET("api/Camiones/choferes")
    suspend fun listarChoferes(): Response<RespuestaApi<List<Chofer>>>
    @GET("api/Camiones/choferesDisponibles")
    suspend fun choferesDisponibles(): Response<RespuestaApi<List<Chofer>>>
    @POST("api/Camiones/agregarNuevoChofer")
    suspend fun crearChofer(@Body chofer: Chofer): Response<RespuestaApi<Chofer>>
    @PUT("api/Camiones/estadoChofer/{idChofer}")
    suspend fun estadoChofer(@Path("idChofer") idChofer: Int, @Body estado: Estado): Response<RespuestaApi<Chofer>>
    @GET("api/Camiones/listarPlantas")
    suspend fun listarPlantas(): Response<RespuestaApi<List<Planta>>>
    @POST("api/Camiones/agregarNuevaPlanta")
    suspend fun crearPlanta(@Body planta: Planta): Response<RespuestaApi<Planta>>
    @GET("api/Camiones/listarCamiones")
    suspend fun listarCamiones(): Response<RespuestaApi<List<Camion>>>
    @GET("api/Camiones/camionesDisponibles/{idPlanta}")
    suspend fun camionesDisponibles(@Path("idPlanta") idPlanta: Int): Response<RespuestaApi<List<Camion>>>
    @POST("api/Camiones/agregarNuevoCamion")
    suspend fun crearCamion(@Body camion: Camion): Response<RespuestaApi<Camion>>
    @PUT("api/Camiones/estadoCamion/{codigo}")
    suspend fun estadoCamion(@Path("codigo") codigo: String, @Body estado: Estado): Response<RespuestaApi<Camion>>
    @GET("api/Camiones/salidas")
    suspend fun listarSalidas(): Response<RespuestaApi<List<Salida>>>
    @POST("api/Camiones/SalidaCamion")
    suspend fun registrarSalida(@Body salida: Salida): Response<RespuestaApi<Salida>>
    @PUT("api/Camiones/retornarCamion/{idSalida}")
    suspend fun retornarCamion(@Path("idSalida") idSalida: Int): Response<RespuestaApi<Salida>>
}