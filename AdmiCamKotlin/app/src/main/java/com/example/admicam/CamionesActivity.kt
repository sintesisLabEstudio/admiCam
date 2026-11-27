package com.example.admicam
import android.os.Bundle
import android.view.View
import android.widget.AdapterView
import android.widget.ArrayAdapter
import android.widget.Button
import android.widget.EditText
import android.widget.ListView
import android.widget.Spinner
class CamionesActivity : PantallaBase() {
    private var camiones = listOf<Camion>()
    private var plantas = listOf<Planta>()
    private var plantaSeleccionada: Planta? = null
    private lateinit var listado: ListView
    private lateinit var spPlanta: Spinner
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_camiones)
        listado = findViewById(R.id.listaRegistros)
        spPlanta = findViewById(R.id.spPlanta)
        val codigo = findViewById<EditText>(R.id.txtCodigo)
        val estado = findViewById<Spinner>(R.id.spEstado)
        estado.adapter = adaptadorSpinner(listOf("ACTIVO", "INACTIVO"))
        findViewById<Button>(R.id.btnVolver).setOnClickListener { finish() }
        val guardar = findViewById<Button>(R.id.btnGuardar)
        guardar.setOnClickListener {
            val texto = codigo.text.toString().trim().uppercase()
            val planta = plantaSeleccionada
            if (texto.isEmpty() || planta == null) mensaje("Debe ingresar el código y seleccionar una planta.")
            else if (texto.length > 10 || !texto.all { it.isLetterOrDigit() && it.code < 128 }) {
                mensaje("Código: máximo 10 caracteres, solo letras y números.")
            } else consultar(guardar, { RetrofitCliente.ruta.crearCamion(Camion(texto, planta.idPlanta, estado.selectedItemPosition + 1)) }) { _, aviso ->
                mensaje(aviso)
                codigo.setText("")
                cargar()
            }
        }
        listado.setOnItemClickListener { _, _, posicion, _ ->
            val elegido = camiones[posicion]
            val nuevo = if (elegido.idEstado == 1) 2 else 1
            confirmar("¿Cambiar el camión ${elegido.codigoCamion} a ${if (nuevo == 1) "ACTIVO" else "INACTIVO"}?") {
                consultar(llamada = { RetrofitCliente.ruta.estadoCamion(elegido.codigoCamion, Estado(nuevo)) }) { _, aviso ->
                    mensaje(aviso)
                    cargar()
                }
            }
        }
    }
    override fun onResume() {
        super.onResume()
        cargarPlantas()
        cargar()
    }
    private fun cargarPlantas() {
        consultar(llamada = { RetrofitCliente.ruta.listarPlantas() }) { datos, _ ->
            plantas = datos.orEmpty()
            plantaSeleccionada = null
            spPlanta.adapter = adaptadorSpinner(
                listOf("Seleccione una planta") + plantas.map { it.nombrePlanta })
            spPlanta.onItemSelectedListener = object : AdapterView.OnItemSelectedListener {
                override fun onNothingSelected(parent: AdapterView<*>?) {
                    plantaSeleccionada = null
                }
                override fun onItemSelected(parent: AdapterView<*>?, view: View?, position: Int, id: Long) {
                    val texto = parent?.getItemAtPosition(position) as? String ?: ""
                    plantaSeleccionada = plantas.firstOrNull { it.nombrePlanta == texto }
                }
            }
        }
    }
    private fun cargar() {
        consultar(llamada = { RetrofitCliente.ruta.listarCamiones() }) { datos, _ ->
            camiones = datos.orEmpty()
            listado.adapter = ArrayAdapter(this, android.R.layout.simple_list_item_1,
                camiones.map { "${it.codigoCamion} - ${it.nombrePlanta ?: "Planta ${it.idPlanta}"}\n${if (it.idEstado == 1) "ACTIVO" else "INACTIVO"}" })
        }
    }
}