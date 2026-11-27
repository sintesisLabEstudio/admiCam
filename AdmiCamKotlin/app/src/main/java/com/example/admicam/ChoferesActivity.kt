package com.example.admicam
import android.os.Bundle
import android.widget.ArrayAdapter
import android.widget.Button
import android.widget.EditText
import android.widget.ListView
import android.widget.Spinner
class ChoferesActivity : PantallaBase() {
    private var choferes = listOf<Chofer>()
    private lateinit var listado: ListView
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_choferes)
        listado = findViewById(R.id.listaRegistros)
        val nombre = findViewById<EditText>(R.id.txtNombre)
        val estado = findViewById<Spinner>(R.id.spEstado)
        estado.adapter = adaptadorSpinner(listOf("ACTIVO", "INACTIVO"))
        findViewById<Button>(R.id.btnVolver).setOnClickListener { finish() }
        val guardar = findViewById<Button>(R.id.btnGuardar)
        guardar.setOnClickListener {
            val texto = nombre.text.toString().trim()
            if (texto.isEmpty()) mensaje("Debe ingresar un nombre.")
            else if (texto.length > 50) mensaje("El nombre permite máximo 50 caracteres.")
            else consultar(guardar, { RetrofitCliente.ruta.crearChofer(Chofer(nombreChofer = texto, idEstado = estado.selectedItemPosition + 1)) }) { _, aviso ->
                mensaje(aviso)
                nombre.setText("")
                cargar()
            }
        }
        listado.setOnItemClickListener { _, _, posicion, _ ->
            val elegido = choferes[posicion]
            val siguiente = if (elegido.idEstado == 1) 2 else 1
            confirmar("¿Cambiar el estado de ${elegido.nombreChofer} a ${if (siguiente == 1) "ACTIVO" else "INACTIVO"}?") {
                consultar(llamada = { RetrofitCliente.ruta.estadoChofer(elegido.idChofer, Estado(siguiente)) }) { _, aviso ->
                    mensaje(aviso)
                    cargar()
                }
            }
        }
    }
    override fun onResume() {
        super.onResume()
        cargar()
    }
    private fun cargar() {
        consultar(llamada = { RetrofitCliente.ruta.listarChoferes() }) { datos, _ ->
            choferes = datos.orEmpty()
            listado.adapter = ArrayAdapter(this, android.R.layout.simple_list_item_1,
                choferes.map { "${it.idChofer} - ${it.nombreChofer}\n${if (it.idEstado == 1) "ACTIVO" else "INACTIVO"}" })
        }
    }
}