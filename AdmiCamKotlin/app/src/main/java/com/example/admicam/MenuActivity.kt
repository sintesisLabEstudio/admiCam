package com.example.admicam

import android.content.Intent
import android.os.Bundle
import android.widget.Button
import android.widget.TextView

class MenuActivity : PantallaBase() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        if (Sesion.usuario.isBlank()) {
            startActivity(Intent(this, LoginActivity::class.java))
            finish()
            return
        }
        setContentView(R.layout.activity_menu)
        findViewById<TextView>(R.id.txtBienvenida).text = "Bienvenido: ${Sesion.nombre}"

        findViewById<Button>(R.id.btnUsuarios).setOnClickListener { ir(UsuariosActivity::class.java) }
        findViewById<Button>(R.id.btnChoferes).setOnClickListener { ir(ChoferesActivity::class.java) }
        findViewById<Button>(R.id.btnPlantas).setOnClickListener { ir(PlantasActivity::class.java) }
        findViewById<Button>(R.id.btnCamiones).setOnClickListener { ir(CamionesActivity::class.java) }
        findViewById<Button>(R.id.btnSalidas).setOnClickListener { ir(SalidasActivity::class.java) }
        findViewById<Button>(R.id.btnCerrar).setOnClickListener {
            confirmar("¿Realmente desea cerrar sesión?") {
                Sesion.cerrar()
                startActivity(Intent(this, LoginActivity::class.java))
                finish()
            }
        }
    }

    private fun ir(destino: Class<*>) {
        startActivity(Intent(this, destino))
    }
}
