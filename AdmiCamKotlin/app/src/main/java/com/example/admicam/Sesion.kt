package com.example.admicam

import android.content.Context
import android.content.SharedPreferences

object Sesion {
    // ========================================================================
    // CÓDIGO ORIGINAL (no eliminado, solo comentado):
    // var usuario: String = ""
    // var nombre: String = ""
    // fun cerrar() {
    //     usuario = ""
    //     nombre = ""
    // }
    // ========================================================================

    // CAMBIO (24-11-2026): la sesión ahora se guarda en SharedPreferences.
    // Motivo: el object original vivía solo en memoria; al girar el celular o
    // recrear la activity se perdía y el usuario tenía que volver a entrar.
    // Las propiedades usuario/nombre conservan el mismo nombre para no tocar
    // ninguna de las activities que ya las usan.
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
        // CÓDIGO ORIGINAL (no eliminado, solo comentado):
        // usuario = ""
        // nombre = ""
        // CAMBIO (24-11-2026): se limpian las preferencias guardadas para que
        // un reinicio de la app no recupere una sesión ya cerrada.
        if (::prefs.isInitialized) prefs.edit().clear().apply()
    }
}
