package com.example.admicam

import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory

object RetrofitCliente {
    // CÓDIGO ORIGINAL (no eliminado, solo comentado):
    // Emulador Android: 10.0.2.2 equivale al PC que ejecuta la API.
    // Celular físico: cambiar por la IP local del PC (misma red Wi-Fi).
    // private const val URL_API = "http://10.0.2.2:5000/"

    // CAMBIO (24-11-2026): la dirección se lee de gradle.properties (api.url),
    // inyectada por Gradle como BuildConfig.URL_API. Motivo: 10.0.2.2 solo existe
    // en el emulador; con el celular físico conectado por USB se usa 127.0.0.1
    // junto con `adb reverse tcp:5000 tcp:5000`, que redirige ese puerto del
    // teléfono al PC sin necesidad de modificar la API.
    private const val URL_API = BuildConfig.URL_API

    val ruta: Funciones = Retrofit.Builder()
        .baseUrl(URL_API)
        .addConverterFactory(GsonConverterFactory.create())
        .build()
        .create(Funciones::class.java)
}
