package com.example.admicam
import android.os.Bundle
import android.widget.ArrayAdapter
import android.widget.Button
import android.widget.EditText
import android.widget.ListView
class PlantasActivity : PantallaBase() {
    private lateinit var listado: ListView
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_plantas)
        listado = findViewById(R.id.listaRegistros)
        val nombre = findViewById<EditText>(R.id.txtNombre)
        val guardar = findViewById<Button>(R.id.btnGuardar)
        findViewById<Button>(R.id.btnVolver).setOnClickListener { finish() }
        guardar.setOnClickListener {
            val texto = nombre.text.toString().trim()
            if (texto.isEmpty()) mensaje("Debe ingresar el nombre de la planta.")
            else if (texto.length > 50) mensaje("El nombre permite máximo 50 caracteres.")
            else consultar(guardar, { RetrofitCliente.ruta.crearPlanta(Planta(nombrePlanta = texto)) }) { _, aviso ->
                mensaje(aviso)
                nombre.setText("")
                cargar()
            }
        }
    }
    override fun onResume() {
        super.onResume()
        cargar()
    }
    private fun cargar() {
        consultar(llamada = { RetrofitCliente.ruta.listarPlantas() }) { datos, _ ->
            listado.adapter = ArrayAdapter(this, android.R.layout.simple_list_item_1,
                datos.orEmpty().map { it.nombrePlanta })
        }
    }
}