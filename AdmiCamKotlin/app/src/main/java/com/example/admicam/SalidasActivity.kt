package com.example.admicam
import android.app.DatePickerDialog
import android.os.Bundle
import android.view.View
import android.widget.AdapterView
import android.widget.ArrayAdapter
import android.widget.Button
import android.widget.ListView
import android.widget.Spinner
import android.widget.TextView
import java.util.Calendar
import java.util.Locale
class SalidasActivity : PantallaBase() {
    private var plantas = listOf<Planta>()
    private var camiones = listOf<Camion>()
    private var choferes = listOf<Chofer>()
    private var salidas = listOf<Salida>()
    private var fechaElegida = ""
    private var plantaSeleccionada: Planta? = null
    private var camionSeleccionado: Camion? = null
    private var choferSeleccionado: Chofer? = null
    private lateinit var spPlanta: Spinner
    private lateinit var spCamion: Spinner
    private lateinit var spChofer: Spinner
    private lateinit var listado: ListView
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_salidas)
        spPlanta = findViewById(R.id.spPlanta)
        spCamion = findViewById(R.id.spCamion)
        spChofer = findViewById(R.id.spChofer)
        listado = findViewById(R.id.listaRegistros)
        findViewById<Button>(R.id.btnVolver).setOnClickListener { finish() }
        findViewById<Button>(R.id.btnFecha).setOnClickListener {
            val hoy = Calendar.getInstance()
            val dialogo = DatePickerDialog(this, R.style.Theme_AdmiCam_Dialogo, { _, anio, mes, dia ->
                fechaElegida = String.format(Locale.US, "%04d-%02d-%02d", anio, mes + 1, dia)
                findViewById<TextView>(R.id.txtFecha).text = fechaElegida
            }, hoy.get(Calendar.YEAR), hoy.get(Calendar.MONTH), hoy.get(Calendar.DAY_OF_MONTH))
            dialogo.show()
            colorearBotones(dialogo)
        }
        spPlanta.onItemSelectedListener = object : AdapterView.OnItemSelectedListener {
            override fun onNothingSelected(parent: AdapterView<*>?) {
                plantaSeleccionada = null
            }
            override fun onItemSelected(parent: AdapterView<*>?, view: View?, position: Int, id: Long) {
                val texto = parent?.getItemAtPosition(position) as? String ?: ""
                val planta = plantas.firstOrNull { it.nombrePlanta == texto }
                plantaSeleccionada = planta
                if (planta == null) {
                    camiones = emptyList()
                    camionSeleccionado = null
                    llenarCamiones()
                } else cargarCamiones(planta.idPlanta)
            }
        }
        spCamion.onItemSelectedListener = object : AdapterView.OnItemSelectedListener {
            override fun onNothingSelected(parent: AdapterView<*>?) { camionSeleccionado = null }
            override fun onItemSelected(parent: AdapterView<*>?, view: View?, position: Int, id: Long) {
                val texto = parent?.getItemAtPosition(position) as? String ?: ""
                camionSeleccionado = camiones.firstOrNull { it.codigoCamion == texto }
            }
        }
        spChofer.onItemSelectedListener = object : AdapterView.OnItemSelectedListener {
            override fun onNothingSelected(parent: AdapterView<*>?) { choferSeleccionado = null }
            override fun onItemSelected(parent: AdapterView<*>?, view: View?, position: Int, id: Long) {
                val texto = parent?.getItemAtPosition(position) as? String ?: ""
                choferSeleccionado = choferes.firstOrNull { "${it.idChofer} - ${it.nombreChofer}" == texto }
            }
        }
        val guardar = findViewById<Button>(R.id.btnGuardar)
        guardar.setOnClickListener {
            val planta = plantaSeleccionada
            val camion = camionSeleccionado
            val chofer = choferSeleccionado
            if (fechaElegida.isBlank() || planta == null || camion == null || chofer == null) {
                mensaje("Seleccione fecha, planta, camión y conductor.")
            } else if (Sesion.usuario.isBlank()) {
                mensaje("No hay una sesión activa. Vuelva a iniciar sesión.")
            } else {
                val salida = Salida(
                    fechaSalida = fechaElegida,
                    idPlanta = planta.idPlanta,
                    codigoCamion = camion.codigoCamion,
                    idChofer = chofer.idChofer,
                    idEstadoSalida = 1,
                    idUsuario = Sesion.usuario
                )
                consultar(guardar, { RetrofitCliente.ruta.registrarSalida(salida) }) { _, aviso ->
                    mensaje(aviso)
                    cargarCamiones(planta.idPlanta)
                    cargarChoferes()
                    cargarSalidas()
                }
            }
        }
        listado.setOnItemClickListener { _, _, posicion, _ ->
            val salida = salidas[posicion]
            if (salida.idEstadoSalida != 1) {
                mensaje("Solo se puede registrar el retorno de salidas EN RUTA.")
            } else {
                confirmar("¿Confirmar el retorno del camión ${salida.codigoCamion}, salida #${salida.idSalida}?") {
                    consultar(llamada = { RetrofitCliente.ruta.retornarCamion(salida.idSalida) }) { _, aviso ->
                        mensaje(aviso)
                        cargarSalidas()
                        cargarChoferes()
                        plantaSeleccionada?.let { cargarCamiones(it.idPlanta) }
                    }
                }
            }
        }
    }
    override fun onResume() {
        super.onResume()
        cargarPlantas()
        cargarChoferes()
        cargarSalidas()
    }
    private fun cargarPlantas() {
        consultar(llamada = { RetrofitCliente.ruta.listarPlantas() }) { datos, _ ->
            plantas = datos.orEmpty()
            spPlanta.adapter = adaptadorSpinner(
                listOf("Seleccione una planta") + plantas.map { it.nombrePlanta })
        }
    }
    private fun cargarCamiones(idPlanta: Int) {
        consultar(llamada = { RetrofitCliente.ruta.camionesDisponibles(idPlanta) }) { datos, _ ->
            if (plantaSeleccionada?.idPlanta == idPlanta) {
                camiones = datos.orEmpty().filter { it.idEstado == 1 && it.idPlanta == idPlanta }
                camionSeleccionado = null
                llenarCamiones()
            }
        }
    }
    private fun llenarCamiones() {
        spCamion.adapter = adaptadorSpinner(
            listOf("Seleccione un camión") + camiones.map { it.codigoCamion })
    }
    private fun cargarChoferes() {
        consultar(llamada = { RetrofitCliente.ruta.choferesDisponibles() }) { datos, _ ->
            choferes = datos.orEmpty().filter { it.idEstado == 1 }
            spChofer.adapter = adaptadorSpinner(
                listOf("Seleccione un conductor") + choferes.map { "${it.idChofer} - ${it.nombreChofer}" })
        }
    }
    private fun cargarSalidas() {
        consultar(llamada = { RetrofitCliente.ruta.listarSalidas() }) { datos, _ ->
            salidas = datos.orEmpty()
            listado.adapter = ArrayAdapter(this, android.R.layout.simple_list_item_1,
                salidas.map {
                    "#${it.idSalida}  ${it.fechaSalida.take(10)}  ${it.codigoCamion}\n" +
                        "${it.nombrePlanta ?: "Planta ${it.idPlanta}"} - ${it.nombreChofer ?: "Chofer ${it.idChofer}"}\n" +
                        "${if (it.idEstadoSalida == 1) "EN RUTA" else "RETORNO"} - Usuario: ${it.idUsuario}"
                })
        }
    }
}