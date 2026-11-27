package com.example.admicam
import android.content.Context
import android.content.SharedPreferences
object Sesion {
    private const val ARCHIVO = "admicam_sesion"
    private const val CLAVE_USUARIO = "usuario"
    private const val CLAVE_NOMBRE = "nombre"
    private lateinit var prefs: SharedPreferences
    fun inicializar(context: Context) {
        prefs = context.applicationContext.getSharedPreferences(ARCHIVO, Context.MODE_PRIVATE)
    }
    var usuario: String
        get() = if (::prefs.isInitialized) prefs.getString(CLAVE_USUARIO, "") ?: "" else ""
        set(value) {
            if (::prefs.isInitialized) prefs.edit().putString(CLAVE_USUARIO, value).apply()
        }
    var nombre: String
        get() = if (::prefs.isInitialized) prefs.getString(CLAVE_NOMBRE, "") ?: "" else ""
        set(value) {
            if (::prefs.isInitialized) prefs.edit().putString(CLAVE_NOMBRE, value).apply()
        }
    fun cerrar() {
        if (::prefs.isInitialized) prefs.edit().clear().apply()
    }
}