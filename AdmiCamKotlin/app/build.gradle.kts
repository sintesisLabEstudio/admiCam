plugins {
    alias(libs.plugins.android.application)
}

android {
    namespace = "com.example.admicam"
    compileSdk = 37

    // CAMBIO (24-11-2026): se habilita BuildConfig para poder leer la dirección
    // de la API desde gradle.properties. Motivo: dejar la URL como constante en
    // RetrofitCliente.kt obligaba a recompilar código para cambiar de emulador a
    // celular físico. A partir de AGP 8 BuildConfig viene desactivado por defecto.
    buildFeatures {
        buildConfig = true
    }

    defaultConfig {
        applicationId = "com.example.admicam"
        minSdk = 26
        targetSdk = 37
        versionCode = 1
        versionName = "1.0"

        // CAMBIO (24-11-2026): se expone la propiedad api.url como BuildConfig.URL_API.
        // Si no se define en gradle.properties se mantiene 127.0.0.1:5000, que es
        // lo que necesita el celular por USB con `adb reverse tcp:5000 tcp:5000`.
        val urlApi = project.findProperty("api.url")?.toString()?.takeIf { it.isNotBlank() }
            ?: "http://127.0.0.1:5000/"
        buildConfigField("String", "URL_API", "\"$urlApi\"")
    }
    buildTypes {
        release {
            isMinifyEnabled = false
            proguardFiles(getDefaultProguardFile("proguard-android-optimize.txt"), "proguard-rules.pro")
        }
    }
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_11
        targetCompatibility = JavaVersion.VERSION_11
    }
}

dependencies {
    implementation("com.squareup.retrofit2:retrofit:2.9.0")
    implementation("com.squareup.retrofit2:converter-gson:2.9.0")
    implementation("org.jetbrains.kotlinx:kotlinx-coroutines-android:1.10.2")
    implementation("androidx.lifecycle:lifecycle-runtime-ktx:2.9.3")
    implementation(libs.androidx.activity.ktx)
    implementation(libs.androidx.appcompat)
    implementation(libs.androidx.constraintlayout)
    implementation(libs.androidx.core.ktx)
    implementation(libs.material)
}
