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

    // CAMBIO (24-11-2026): se guardan los objetos elegidos en los spinners.
    // Motivo: antes se usaba selectedItemPosition - 1, que se desalinea cuando
    // onResume recarga los adapters mientras el usuario está interactuando.
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
            // CÓDIGO ORIGINAL (no eliminado, solo comentado):
            // DatePickerDialog(this, { _, anio, mes, dia ->
            //     fechaElegida = String.format(Locale.US, "%04d-%02d-%02d", anio, mes + 1, dia)
            //     findViewById<TextView>(R.id.txtFecha).text = fechaElegida
            // }, hoy.get(Calendar.YEAR), hoy.get(Calendar.MONTH), hoy.get(Calendar.DAY_OF_MONTH)).show()
            // CAMBIO (24-11-2026): se pasa el estilo oscuro sobrio para que el
            // calendario no aparezca con fondo claro.
            DatePickerDialog(this, R.style.Theme_AdmiCam_Dialogo, { _, anio, mes, dia ->
                fechaElegida = String.format(Locale.US, "%04d-%02d-%02d", anio, mes + 1, dia)
                findViewById<TextView>(R.id.txtFecha).text = fechaElegida
            }, hoy.get(Calendar.YEAR), hoy.get(Calendar.MONTH), hoy.get(Calendar.DAY_OF_MONTH)).show()
        }

        // CÓDIGO ORIGINAL (no eliminado, solo comentado):
        // spPlanta.onItemSelectedListener = object : AdapterView.OnItemSelectedListener {
        //     override fun onNothingSelected(parent: AdapterView<*>?) { }
        //     override fun onItemSelected(parent: AdapterView<*>?, view: View?, position: Int, id: Long) {
        //         val planta = plantas.getOrNull(position - 1)
        //         ...
        //     }
        // }
        // CAMBIO (24-11-2026): se identifica la planta por el TEXTO del ítem y se
        // guarda en plantaSeleccionada, sin depender de la posición - 1.
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

        // CAMBIO (24-11-2026): lo mismo para camión y conductor. El texto del
        // spinner del chofer es "${it.idChofer} - ${it.nombreChofer}".
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
            // CÓDIGO ORIGINAL (no eliminado, solo comentado):
            // val planta = plantas.getOrNull(spPlanta.selectedItemPosition - 1)
            // val camion = camiones.getOrNull(spCamion.selectedItemPosition - 1)
            // val chofer = choferes.getOrNull(spChofer.selectedItemPosition - 1)
            // CAMBIO (24-11-2026): se usan los objetos guardados por los listeners.
            val planta = plantaSeleccionada
            val camion = camionSeleccionado
            val chofer = choferSeleccionado
            if (fechaElegida.isBlank() || planta == null || camion == null || chofer == null) {
                mensaje("Seleccione fecha, planta, camión y conductor.")
            } else if (Sesion.usuario.isBlank()) {
                mensaje("No hay una sesión activa. Vuelva a iniciar sesión.")
            } else {
                val salida = Salida(
                    // CÓDIGO ORIGINAL (no eliminado, solo comentado):
                    // fechaSalida = "${fechaElegida}T00:00:00",
                    // CAMBIO (24-11-2026): se envía solo la fecha (YYYY-MM-DD),
                    // como pide el requerimiento. La API la convierte con .Date.
                    fechaSalida = fechaElegida,
                    idPlanta = planta.idPlanta,
                    codigoCamion = camion.codigoCamion,
                    idChofer = chofer.idChofer,
                    idEstadoSalida = 1,
                    // CÓDIGO ORIGINAL (no eliminado, solo comentado):
                    // usuario = Sesion.usuario
                    // CAMBIO (24-11-2026): el modelo Salida ahora llama idUsuario al
                    // campo USUARIO; el JSON enviado a la API sigue siendo "USUARIO".
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
                        // CÓDIGO ORIGINAL (no eliminado, solo comentado):
                        // plantas.getOrNull(spPlanta.selectedItemPosition - 1)?.let { cargarCamiones(it.idPlanta) }
                        // CAMBIO (24-11-2026): se usa el objeto guardado por el listener.
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
            spPlanta.adapter = ArrayAdapter(this, android.R.layout.simple_spinner_dropdown_item,
                listOf("Seleccione una planta") + plantas.map { it.nombrePlanta })
        }
    }

    private fun cargarCamiones(idPlanta: Int) {
        consultar(llamada = { RetrofitCliente.ruta.camionesDisponibles(idPlanta) }) { datos, _ ->
            // CÓDIGO ORIGINAL (no eliminado, solo comentado):
            // if (plantas.getOrNull(spPlanta.selectedItemPosition - 1)?.idPlanta == idPlanta) {
            // CAMBIO (24-11-2026): comparación contra el objeto guardado; evita
            // descartar una respuesta válida por un cambio de posición del spinner.
            if (plantaSeleccionada?.idPlanta == idPlanta) {
                camiones = datos.orEmpty().filter { it.idEstado == 1 && it.idPlanta == idPlanta }
                camionSeleccionado = null
                llenarCamiones()
            }
        }
    }

    private fun llenarCamiones() {
        spCamion.adapter = ArrayAdapter(this, android.R.layout.simple_spinner_dropdown_item,
            listOf("Seleccione un camión") + camiones.map { it.codigoCamion })
    }

    private fun cargarChoferes() {
        consultar(llamada = { RetrofitCliente.ruta.choferesDisponibles() }) { datos, _ ->
            choferes = datos.orEmpty().filter { it.idEstado == 1 }
            spChofer.adapter = ArrayAdapter(this, android.R.layout.simple_spinner_dropdown_item,
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
                        // CÓDIGO ORIGINAL (no eliminado, solo comentado):
                        // "${if (it.idEstadoSalida == 1) "EN RUTA" else "RETORNO"} - Usuario: ${it.usuario}"
                        // CAMBIO (24-11-2026): se muestra idUsuario por el renombraje del modelo.
                        "${if (it.idEstadoSalida == 1) "EN RUTA" else "RETORNO"} - Usuario: ${it.idUsuario}"
                })
        }
    }
}
