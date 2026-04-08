# Bondi Simulator 2025 — Task Board

> Organización por módulos temáticos. Cada mono se hace cargo de un módulo completo, al completar una tarea, primero hacer PR a dev, no se trabaja sobre main.

> Si estas laburando en algo, reemplazar MONO con tu nombre para hacerselo a saber a los demás.

---

## Convenciones

- **[ ]** tarea pendiente · **[x]** completada · **[~]** en progreso
---

## Meta 1 — Prototipo de Física y Proyecto Base

> **Meta:** Repo configurado, proyecto Unity andando, un colectivo que se mueveeeee.

### Santy — Proyecto y repositorio
- [x] Definir estructura de carpetas en `Assets/_Project/`
- [ ] Configurar Input System (aceleración, freno, freno de mano, dirección, bocina, luces)
- [ ] Crear `InputActions.inputactions` con todos los bindings de PC
- [ ] Documentar en el README cómo clonar y abrir el proyecto

### Mono B — VehicleController
- [ ] Crear prefab base de colectivo con Rigidbody + WheelColliders (4 ruedas)
- [ ] Ajustar masa (~14,000 kg) y centro de masa bajo para anti-rollover
- [ ] Implementar `VehicleController.cs`: aceleración, dirección, freno, freno de mano
- [ ] Crear `VehicleData` ScriptableObject con campos: `maxMotorTorque`, `maxSteeringAngle`, `brakeTorque`, `topSpeedKmh`, `mass`, `centerOfMass`
- [ ] Completar SO con stats para el modelo OH 1618 Ugarte (modelo base)
- [ ] Escena de prueba de física: plano plano, rampas, giro 90°

### Mono C — GameManager y arquitectura de estados
- [ ] Implementar `GameManager.cs` como Singleton con FSM: `MainMenu → CountdownState → Racing → Results`
- [ ] Crear `GameEvents.cs` con los eventos globales del sistema
- [ ] Implementar `ServiceLocator.cs` básico
- [ ] Crear escena `MainMenu.unity` placeholder (solo un botón "Jugar")
- [ ] Crear escena `Route_620_SanJusto_CarlosCasares.unity` vacía con el colectivo cargado
- [ ] Conectar transición de escenas al FSM

---

## Meta 2 — Recorrido 620 Jugable (sin tráfico)

> **Meta:** El recorrido 620 es completable de punta a punta con cronómetro y minimapa.

### Mono A — RouteSystem y Waypoints
- [ ] Implementar `RouteSystem.cs`: secuencia de waypoints, detección de progreso del jugador
- [ ] Crear `RouteData` ScriptableObject con todos sus campos
- [ ] Completar SO del recorrido 620 (waypoints, paradas, tiempos medalla)
- [ ] Implementar límites de recorrido: zona de reposicionamiento si el jugador se desvía demasiado
- [ ] Implementar guía visual en el suelo
- [ ] Poblar la escena 620 con geometría básica de calles (ladrillardos grises va está bien)

### Mono B — HUD y Cronómetro
- [ ] Crear `UIManager.cs` y estructura Canvas del HUD
- [ ] Implementar velocímetro
- [ ] Implementar cronómetro con tiempo transcurrido y referencia al objetivo de medalla activo
- [ ] Implementar minimapa
- [ ] Implementar cuenta regresiva de 3 segundos antes del inicio
- [ ] Pantalla de resultados placeholder (tiempo final, medalla)

### Mono C — CameraController
- [ ] Integrar Cinemachine 3.x en el proyecto
- [ ] Configurar cámara de tercera persona siguiendo el colectivo
- [ ] Configurar cámara de primera persona (interior del colectivo, detrás del volante)
- [ ] Implementar `CameraController.cs`: toggle entre primera y tercera persona con blend suave (0.3s)
- [ ] Ajustar FOV y distancias para que el colectivo se sienta grande y pesado
- [ ] Probar las dos cámaras recorriendo el 620

---

## Meta 3 — Tráfico y Pasajeros

> **Meta:** El 620 tiene tráfico vial y paradas con pasajeros funcionales.

### Mono A — TrafficSystem
- [ ] Configurar NavMesh en la escena 620
- [ ] Implementar `TrafficManager.cs` con pool fijo de vehículos NPC (máx 20-25 activos)
- [ ] Implementar `TrafficSpawner.cs`: spawn/despawn en zonas fuera de la vista del jugador
- [ ] Implementar `VehicleAI.cs`: seguir carril, frenar ante obstáculos, respetar semáforos básico
- [ ] Crear 2-3 prefabs de autos NPC simples (blockout)
- [ ] Agregar colectivos NPC del carril contrario (con evento de saludo disponible)

### Mono B — PassengerSystem
- [ ] Implementar `PassengerManager.cs`: registra paradas, gestiona ocupación
- [ ] Implementar `BusStop.cs`: trigger de zona, spawn de pasajeros sprite billboard
- [ ] Lógica de subida de pasajeros: jugador frena en zona de parada → pasajeros suben
- [ ] Lógica de bajada de pasajeros: parada destino → jugador debe frenar completamente
- [ ] Implementar detección de pasajeros llamando desde lejos (gesto + ícono HUD)
- [ ] Poblar el recorrido 620 con paradas y pasajeros configurados

### Mono C — HornController y LightsController
- [ ] Implementar `HornController.cs`: bocina normal y bocina de aire con inputs separados
- [ ] Implementar `LightsController.cs`: toggle luces delanteras
- [ ] Agregar efectos de sonido placeholder para bocinas (pueden ser beeps temporales)
- [ ] Implementar detección de colectivos NPC del carril contrario para el saludo
- [ ] Implementar bus-spotters: spawn en puntos del recorrido, detección de reducción de velocidad
- [ ] Conectar estos controladores al `GameEvents.cs` para que el ScoreSystem los escuche

---

## Meta 4 — Score y Repeticiones

> **Meta:** El juego puntúa, graba la repetición y guarda el progreso en disco.

### Mono A — ScoreSystem
- [ ] Implementar `ScoreManager.cs`: puntaje base por tiempo restante * multiplicador por categoría
- [ ] Implementar Puntos de Estilo: Atender al público, Saludar al compañero, Posar para las cámaras
- [ ] Lógica de ventana de tiempo para el saludo (responder en N segundos)
- [ ] Implementar combo de acciones de estilo con multiplicador acumulativo
- [ ] Popup animado en HUD al ganar Style Points
- [ ] Pantalla de resultados completa: tiempo, medalla, desglose de puntaje

### Mono B — ReplaySystem
- [ ] Implementar `ReplayManager.cs`: grabación de estado del vehículo a 20 fps durante el recorrido
- [ ] Implementar `ReplayData` y `ReplayFrame` con serialización a JSON
- [ ] Implementar reproducción de repetición: mover el colectivo según frames grabados
- [ ] Crear escena `ReplayViewer.unity` con controles básicos (play, pausa, velocidad)
- [ ] Implementar exportación del archivo `.replayjson` a carpeta del usuario
- [ ] Implementar importación de un archivo de repetición externo para verlo

### Mono C — SaveSystem
- [ ] Implementar `SaveSystem.cs`: lectura y escritura de `progress.json` en `%AppData%/BondiSimulator/`
- [ ] Implementar `PlayerProgress` y `RouteRecord` con serialización JSON
- [ ] Implementar guardado manual desde el menú de pausa
- [ ] Implementar autoguardado al completar un recorrido (si está activado en opciones)
- [ ] Bloquear guardado durante un recorrido en curso
- [ ] Implementar carga de progreso al iniciar el juego

---

## Milestone 5 — Recorridos 622 y 378 + Los 3 Modelos

> **Meta:** Los 3 recorridos y los 3 colectivos están jugables.

### Mono A — Recorrido 622 (Cristania → Laferrere)
- [ ] Construir geometría de calles del 622 (angostas, giros 90°, tráfico denso)
- [ ] Configurar waypoints y RouteData SO del 622
- [ ] Poblar con paradas, pasajeros y tráfico ajustado a la densidad del recorrido
- [ ] Crear y asignar prefab del modelo OF 1621 Bimet Corwin Corbus
- [ ] Completar VehicleData SO del Corbus (alta maniobrabilidad, menor velocidad punta)
- [ ] Testear que el recorrido 622 sea completable y que la dificultad se sienta distinta al 620

### Mono B — Recorrido 378 (Laferrere → Plaza KM30)
- [ ] Construir geometría del 378: elevación, centro de transbordo Independencia, Ruta 3
- [ ] Configurar waypoints y RouteData SO del 378
- [ ] Poblar con paradas, pasajeros y tráfico
- [ ] Crear y asignar prefab del modelo OH 1618 Ugarte Europeo (modelo estándar del 378)
- [ ] Testear el recorrido completo con el modelo asignado

### Mono C — Modelo OH 1621 Nuovobus Cittá + selección de vehículos
- [ ] Crear prefab del modelo OH 1621 Nuovobus Cittá con materiales y colores del 96
- [ ] Completar VehicleData SO del Cittá (alta aceleración y velocidad, menor maniobra)
- [ ] Implementar variantes visuales (versión especial de baja probabilidad) para los 3 modelos
- [ ] Implementar pantalla de selección de modelo en el menú (con stats visibles)
- [ ] Implementar lógica de modelo especial: probabilidad baja al seleccionar, sin ventaja de stats
- [ ] Conectar selección de modelo al spawn del prefab correcto en cada escena

---

## Meta 6 — Audio, UI Final y Polish

> **Meta:** MVP presentable: menús terminados, audio completo, build de PC estable.

### Mono A — AudioManager y Soundtrack
- [ ] Implementar `AudioManager.cs` con AudioMixer (grupos: Music, SFX_Vehicle, SFX_Environment, UI)
- [ ] Integrar 2 tracks de música: uno para menús, uno para gameplay
- [ ] Implementar efectos de sonido del motor (loop con pitch variable según velocidad)
- [ ] Implementar SFX de bocinas, frenos, ruedas en curva
- [ ] Implementar SFX de ambiente urbano (tráfico lejano, ciudad)
- [ ] Agregar SFX de UI (navegación de menús, selección, resultados)

### Mono B — Menú Principal y Flujo Completo
- [ ] Construir menú principal completo: selección de empresa → línea → recorrido → modelo
- [ ] Implementar mapa del recorrido en la pantalla de previa (imagen o render simple)
- [ ] Mostrar distancia, categoría (Sprint/Short/etc.) y los 3 tiempos objetivo
- [ ] Implementar pantalla de opciones: volumen, resolución, autoguardado on/off
- [ ] Implementar pantalla de pausa con opciones de guardar, reiniciar y salir
- [ ] Conectar el flujo completo de UI con el FSM del GameManager

### Mono C — Build, QA y README técnico
- [ ] Testear los 3 recorridos completos de punta a punta
- [ ] Configurar Player Settings para build PC (Windows 64-bit, ícono, nombre del juego)
- [ ] Aplicar compresión de texturas (DXT5/BC7) y compresión de audio (Vorbis)
- [ ] Configurar LOD Groups en los 3 modelos de colectivos y en elementos del entorno
- [ ] Habilitar Occlusion Culling en las 3 escenas de recorrido
- [ ] Generar build final y documentar bugs conocidos en un archivo `KNOWN_ISSUES.md` o con git issues

---

## Backlog (post-MVP)

Estas tareas no son necesarias para el MVP pero están documentadas para no perderlas.

- [ ] Sistema de desbloqueo de recorridos en árbol
- [ ] Más recorridos y modelos de colectivos
- [ ] Integración de FMOD para audio más avanzado
- [ ] Leaderboards locales por recorrido
- [ ] Sistema de compartir repeticiones via servidor o plataforma
- [ ] Soporte para gamepad/joystick
- [ ] Recorridos por secciones modulares para agilizar la producción de contenido nuevo
- [ ] Semáforos con lógica real y penalización de puntos de estilo

---

_Última actualización: 5-4 - Santy: CREO EL ARCHIVO 