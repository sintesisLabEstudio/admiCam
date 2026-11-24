package com.example.admicam

import com.google.gson.annotations.SerializedName

data class RespuestaApi<T>(
    @SerializedName(value = "resultado", alternate = ["Resultado"]) val resultado: Boolean = false,
    @SerializedName(value = "mensaje", alternate = ["Mensaje"]) val mensaje: String = "",
    @SerializedName(value = "dato", alternate = ["Dato"]) val dato: T? = null
)

// CAMBIO (24-11-2026): se aclara que NOM_PERSONA es el nombre con el que se hace
// login (el "username"). USUARIO es el ID de la cuenta y no viaja en este body.
data class Credenciales(
    @SerializedName("NOM_PERSONA") val nombrePersona: String,
    @SerializedName("PASSWORD") val password: String
)

data class Usuario(
    // CÓDIGO ORIGINAL (no eliminado, solo comentado):
    // @SerializedName("USUARIO") val usuario: String = "",
    // CAMBIO (24-11-2026): la columna USUARIO es el ID de la cuenta (debería ser
    // ID_USUARIO). Se renombra la propiedad a idUsuario para que el código no
    // repita la trampa. El SerializedName sigue siendo "USUARIO": el contrato
    // JSON con la API no se modifica.
    @SerializedName("USUARIO") val idUsuario: String = "",
    @SerializedName("NOM_PERSONA") val nombrePersona: String = "",
    @SerializedName("PASSWORD") val password: String? = null,
    @SerializedName("ID_ESTADO") val idEstado: Int = 1,
    @SerializedName("NOM_ESTADO") val nombreEstado: String? = null
)

data class Chofer(
    @SerializedName("ID_CHOFER") val idChofer: Int = 0,
    @SerializedName("NOM_CHOFER") val nombreChofer: String = "",
    @SerializedName("ID_ESTADO") val idEstado: Int = 1,
    @SerializedName("NOM_ESTADO") val nombreEstado: String? = null
)

data class Planta(
    @SerializedName("ID_PLANTA") val idPlanta: Int = 0,
    @SerializedName("NOM_PLANTA") val nombrePlanta: String = ""
)

data class Camion(
    @SerializedName("COD_CAMION") val codigoCamion: String = "",
    @SerializedName("ID_PLANTA") val idPlanta: Int = 0,
    @SerializedName("ID_ESTADO") val idEstado: Int = 1,
    @SerializedName("NOM_PLANTA") val nombrePlanta: String? = null,
    @SerializedName("NOM_ESTADO") val nombreEstado: String? = null
)

data class Estado(@SerializedName("ID_ESTADO") val idEstado: Int)

data class Salida(
    @SerializedName("ID_SALIDA") val idSalida: Int = 0,
    @SerializedName("FECHA_SALIDA") val fechaSalida: String = "",
    @SerializedName("ID_PLANTA") val idPlanta: Int = 0,
    @SerializedName("COD_CAMION") val codigoCamion: String = "",
    @SerializedName("ID_CHOFER") val idChofer: Int = 0,
    @SerializedName("ID_ESTADO_SALIDA") val idEstadoSalida: Int = 1,
    // CÓDIGO ORIGINAL (no eliminado, solo comentado):
    // @SerializedName("USUARIO") val usuario: String = "",
    // CAMBIO (24-11-2026): mismo criterio que en Usuario: USUARIO guarda el ID de
    // la cuenta que registró la salida, por eso se llama idUsuario en Kotlin.
    @SerializedName("USUARIO") val idUsuario: String = "",
    @SerializedName("NOM_PLANTA") val nombrePlanta: String? = null,
    @SerializedName("NOM_CHOFER") val nombreChofer: String? = null,
    @SerializedName("NOM_ESTADO_SALIDA") val nombreEstadoSalida: String? = null
)
