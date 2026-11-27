package com.example.admicam
import android.app.AlertDialog
import android.os.Bundle
import android.view.View
import android.view.ViewGroup
import android.widget.ArrayAdapter
import android.widget.Button
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import androidx.core.view.ViewCompat
import androidx.core.view.WindowCompat
import androidx.core.view.WindowInsetsCompat
import androidx.lifecycle.lifecycleScope
import com.google.gson.JsonParser
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import retrofit2.Response
abstract class PantallaBase : AppCompatActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        Sesion.inicializar(applicationContext)
        WindowCompat.setDecorFitsSystemWindows(window, false)
        WindowCompat.getInsetsController(window, window.decorView).apply {
            isAppearanceLightStatusBars = false
            isAppearanceLightNavigationBars = false
        }
    }
    override fun setContentView(layoutResID: Int) {
        super.setContentView(layoutResID)
        findViewById<View>(R.id.txtMensaje)?.visibility = View.GONE
        val contenedor = findViewById<ViewGroup>(android.R.id.content)
        val vista = contenedor.getChildAt(0) ?: return
        val left = vista.paddingLeft
        val top = vista.paddingTop
        val right = vista.paddingRight
        val bottom = vista.paddingBottom
        ViewCompat.setOnApplyWindowInsetsListener(vista) { view, insets ->
            val barras = insets.getInsets(
                WindowInsetsCompat.Type.systemBars() or WindowInsetsCompat.Type.displayCutout()
            )
            view.setPadding(
                left + barras.left,
                top + barras.top,
                right + barras.right,
                bottom + barras.bottom
            )
            insets
        }
        ViewCompat.requestApplyInsets(vista)
    }
    protected fun mensaje(texto: String) {
        findViewById<TextView>(R.id.txtMensaje)?.apply {
            text = texto
            visibility = if (texto.isBlank()) View.GONE else View.VISIBLE
        }
    }
    protected fun <T> adaptadorSpinner(opciones: List<T>): ArrayAdapter<T> {
        val adaptador = ArrayAdapter(this, R.layout.spinner_item, opciones)
        adaptador.setDropDownViewResource(R.layout.spinner_dropdown_item)
        return adaptador
    }
    protected fun colorearBotones(dialogo: android.app.Dialog) {
        val color = resources.getColor(R.color.blanco, theme)
        for (id in intArrayOf(android.R.id.button1, android.R.id.button2, android.R.id.button3)) {
            dialogo.findViewById<Button>(id)?.setTextColor(color)
        }
    }
    protected fun confirmar(texto: String, aceptar: () -> Unit) {
        val dialogo = AlertDialog.Builder(this, R.style.Theme_AdmiCam_Dialogo)
            .setMessage(texto)
            .setPositiveButton("Sí") { _, _ -> aceptar() }
            .setNegativeButton("Cancelar", null)
            .show()
        colorearBotones(dialogo)
    }
    protected fun <T> consultar(
        boton: Button? = null,
        llamada: suspend () -> Response<RespuestaApi<T>>,
        correcto: (T?, String) -> Unit
    ) {
        boton?.isEnabled = false
        lifecycleScope.launch {
            var exito = false
            var datoFinal: T? = null
            var mensajeFinal = ""
            try {
                val respuesta = withContext(Dispatchers.IO) { llamada() }
                val contenido = respuesta.body()
                if (respuesta.isSuccessful && contenido?.resultado == true) {
                    exito = true
                    datoFinal = contenido.dato
                    mensajeFinal = contenido.mensaje
                } else {
                    var texto = contenido?.mensaje ?: "No se pudo completar la operación."
                    if (!respuesta.isSuccessful) {
                        val error = respuesta.errorBody()?.string()
                        try {
                            if (!error.isNullOrBlank()) {
                                val objeto = JsonParser().parse(error).asJsonObject
                                texto = objeto.get("mensaje")?.asString
                                    ?: objeto.get("Mensaje")?.asString ?: texto
                            }
                        } catch (_: Exception) { }
                    }
                    mensaje(texto)
                }
            } catch (ex: Exception) {
                val detalle = ex.localizedMessage?.takeIf { it.isNotBlank() } ?: ex.javaClass.simpleName
                val texto = when (ex) {
                    is javax.net.ssl.SSLHandshakeException ->
                        "La API redirigió a HTTPS con un certificado no confiable. Verifica que la API corra solo con el perfil http. $detalle"
                    is java.io.IOException ->
                        "No se pudo conectar con la API. Verifica la dirección y que esté encendida. $detalle"
                    is com.google.gson.JsonParseException ->
                        "La API respondió, pero el formato de la respuesta no es el esperado. $detalle"
                    else ->
                        "Ocurrió un error al procesar la respuesta de la API (${ex.javaClass.simpleName}). $detalle"
                }
                mensaje(texto)
            } finally {
                boton?.isEnabled = true
            }
            if (exito) {
                try {
                    correcto(datoFinal, mensajeFinal)
                } catch (ex: Exception) {
                    mensaje("Error al actualizar la pantalla: ${ex.localizedMessage ?: ex.javaClass.simpleName}")
                }
            }
        }
    }
}