package com.example.admicam

import android.app.AlertDialog
import android.os.Bundle
import android.view.ViewGroup
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
        // CAMBIO (24-11-2026): se inicializa la sesión persistente para TODAS las
        // activities (todas heredan de PantallaBase). Motivo: antes Sesion era un
        // object en memoria y al rotar la pantalla o matar el proceso se perdía el
        // login y MenuActivity redirigía al LoginActivity.
        Sesion.inicializar(applicationContext)
        WindowCompat.setDecorFitsSystemWindows(window, false)
        WindowCompat.getInsetsController(window, window.decorView).apply {
            // CÓDIGO ORIGINAL (no eliminado, solo comentado):
            // isAppearanceLightStatusBars = true
            // isAppearanceLightNavigationBars = true
            // CAMBIO (24-11-2026): las barras ahora son oscuras (@color/barra),
            // así que los iconos deben ser claros (false). Antes eran oscuros
            // porque los fondos eran claros y casi invisibles sobre la barra nueva.
            isAppearanceLightStatusBars = false
            isAppearanceLightNavigationBars = false
        }
    }

    override fun setContentView(layoutResID: Int) {
        super.setContentView(layoutResID)
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
        findViewById<TextView>(R.id.txtMensaje)?.text = texto
    }

    protected fun confirmar(texto: String, aceptar: () -> Unit) {
        // CÓDIGO ORIGINAL (no eliminado, solo comentado):
        // AlertDialog.Builder(this)
        // CAMBIO (24-11-2026): se aplica el estilo oscuro sobrio para que el
        // diálogo no quede claro cuando el sistema está en modo claro.
        AlertDialog.Builder(this, R.style.Theme_AdmiCam_Dialogo)
            .setMessage(texto)
            .setPositiveButton("Sí") { _, _ -> aceptar() }
            .setNegativeButton("Cancelar", null)
            .show()
    }

    protected fun <T> consultar(
        boton: Button? = null,
        llamada: suspend () -> Response<RespuestaApi<T>>,
        correcto: (T?, String) -> Unit
    ) {
        /*
         * =====================================================================
         * CÓDIGO ORIGINAL (no eliminado, solo comentado).
         * Motivo del cambio: el try incluía la llamada a correcto(...), así que
         * un error de la propia pantalla (índice de lista, adapter, etc.) caía
         * en el catch y se mostraba como "No se pudo conectar con la API".
         * Ahora el try cubre SOLO la red y la interpretación de la respuesta;
         * el callback se ejecuta después, con su propio mensaje de error.
         *
         * boton?.isEnabled = false
         * lifecycleScope.launch {
         *     try {
         *         val respuesta = withContext(Dispatchers.IO) { llamada() }
         *         val contenido = respuesta.body()
         *         if (respuesta.isSuccessful && contenido?.resultado == true) {
         *             correcto(contenido.dato, contenido.mensaje)
         *         } else {
         *             var texto = contenido?.mensaje ?: "No se pudo completar la operación."
         *             if (!respuesta.isSuccessful) {
         *                 val error = respuesta.errorBody()?.string()
         *                 try {
         *                     if (!error.isNullOrBlank()) {
         *                         val objeto = JsonParser().parse(error).asJsonObject
         *                         texto = objeto.get("mensaje")?.asString
         *                             ?: objeto.get("Mensaje")?.asString ?: texto
         *                     }
         *                 } catch (_: Exception) { }
         *             }
         *             mensaje(texto)
         *         }
         *     } catch (ex: Exception) {
         *         val detalle = ex.localizedMessage?.takeIf { it.isNotBlank() } ?: ex.javaClass.simpleName
         *         val texto = when (ex) { ... }
         *         mensaje(texto)
         *     } finally {
         *         boton?.isEnabled = true
         *     }
         * }
         * =====================================================================
         */
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
                                // CAMBIO (24-11-2026): se intentó JsonParser.parseString,
                                // pero la Gson que trae Retrofit 2.9 es 2.8.5 y no tiene
                                // ese método estático (no compila). Se conserva la forma
                                // original; en esta versión NO está deprecado.
                                // CÓDIGO ORIGINAL (no eliminado, solo comentado temporalmente):
                                // val objeto = JsonParser.parseString(error).asJsonObject
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
            // CAMBIO (24-11-2026): el callback se ejecuta fuera del try de red.
            // Si falla la actualización de la pantalla, el mensaje lo dice
            // explícitamente en vez de simular un problema de conexión.
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
