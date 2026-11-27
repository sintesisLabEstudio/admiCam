package com.example.admicam
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
object RetrofitCliente {
    private const val URL_API = BuildConfig.URL_API
    val ruta: Funciones = Retrofit.Builder()
        .baseUrl(URL_API)
        .addConverterFactory(GsonConverterFactory.create())
        .build()
        .create(Funciones::class.java)
}