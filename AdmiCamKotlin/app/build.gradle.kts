plugins {
    alias(libs.plugins.android.application)
}
android {
    namespace = "com.example.admicam"
    compileSdk = 37
    buildFeatures {
        buildConfig = true
    }
    defaultConfig {
        applicationId = "com.example.admicam"
        minSdk = 26
        targetSdk = 37
        versionCode = 1
        versionName = "1.0"
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