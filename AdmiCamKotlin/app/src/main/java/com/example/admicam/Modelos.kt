package com.example.admicam
import com.google.gson.annotations.SerializedName
data class RespuestaApi<T>(
    @SerializedName(value = "resultado", alternate = ["Resultado"]) val resultado: Boolean = false,
    @SerializedName(value = "mensaje", alternate = ["Mensaje"]) val mensaje: String = "",
    @SerializedName(value = "dato", alternate = ["Dato"]) val dato: T? = null
)
data class Credenciales(
    @SerializedName("NOM_PERSONA") val nombrePersona: String,
    @SerializedName("PASSWORD") val password: String
)
data class Usuario(
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
    @SerializedName("USUARIO") val idUsuario: String = "",
    @SerializedName("NOM_PLANTA") val nombrePlanta: String? = null,
    @SerializedName("NOM_CHOFER") val nombreChofer: String? = null,
    @SerializedName("NOM_ESTADO_SALIDA") val nombreEstadoSalida: String? = null
)