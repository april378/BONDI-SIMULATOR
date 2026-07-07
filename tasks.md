# Bondi Simulator 2025 - de rutaaaaaaaaaaaaaaaaaaaaaaaaaaa

## Convenciones

- `[ ]` pendiente
- `[~]` en progreso
- `[x]` terminado
- Cada tarea debe terminar en una escena o flujo verificable dentro de Unity.
- No se trabaja sobre `main`; los cambios van por rama y PR hacia `dev`.
- Una fase no se considera cerrada si el juego entra en Play Mode con errores criticos de consola.

## Fase 0 - Base Del Proyecto

Objetivo: ordenar el proyecto Unity y dejar una base consistente para construir gameplay.

### Estructura y configuracion

- [x] Crear estructura base bajo `Assets/_Project/`.
- [x] Mover/crear escenas propias dentro de `Assets/_Project/Scenes/`.
- [x] Mantener `Assets/Scenes/SampleScene.unity` solo como escena temporal o eliminarla cuando haya escenas reales.
- [x] Crear `MainMenu.unity`.
- [x] Crear `Route_620_SanJusto_CarlosCasares.unity`.
- [x] Crear `PhysicsTest.unity` para pruebas aisladas del colectivo.
- [x] Completar `Assets/_Project/Settings/InputActions.inputactions`.
- [x] Agregar input `AirHorn`.
- [x] Definir bindings PC finales: A/D, W, S, Space, H, J, L, V, Escape.

### Arquitectura minima

- [x] Crear `GameManager.cs` con estados `MainMenu`, `Countdown`, `Racing`, `Results`.
- [x] Crear `GameEvents.cs` para eventos globales de gameplay.
- [x] Crear `PlayerInputReader.cs` como capa de lectura del Input System.
- [x] Crear `ServiceLocator.cs` solo para servicios globales inevitables.
- [x] Documentar en comentarios XML las clases publicas principales.

### Criterio de aceptacion

- [x] El proyecto abre en Unity sin errores criticos.
- [x] Las escenas base existen y cargan.
- [x] Los inputs PC minimos estan definidos sin conflictos.
- [x] `GameManager` puede cambiar de `MainMenu` a `Countdown`, `Racing` y `Results`.

## Fase 1 - Vertical Slice 620 Sin Trafico

Objetivo: tener un recorrido 620 completable de punta a punta con un colectivo controlable, cronometro y resultado.

### Vehiculo base

- [x] Crear prefab `VEH_OH1618_Ugarte_Base.prefab`.
- [x] Agregar `Rigidbody` con masa aproximada de 14.000 kg.
- [x] Agregar `WheelCollider` para ruedas principales.
- [x] Ajustar centro de masa bajo.
- [x] Implementar `VehicleData.cs`.
- [x] Crear `VehicleData_OH1618_Ugarte.asset`.
- [x] Implementar `VehicleController.cs`: aceleracion, freno, direccion y freno de mano.
- [x] Agregar limite suave de velocidad maxima.
- [x] Agregar asistencia arcade basica contra vuelcos excesivos.
- [x] Probar en `PhysicsTest.unity`: recta, frenado, curva cerrada y rampa.

### Recorrido 620

- [x] Implementar `RouteData.cs`.
- [x] Implementar `RouteSceneBinder.cs`.
- [x] Implementar `RouteSystem.cs` con progreso por waypoints.
- [x] Crear `RouteData_620.asset`.
- [x] Crear geometria blockout del recorrido 620.
- [x] Colocar waypoints del 620.
- [x] Definir spawn inicial y meta.
- [x] Implementar limites de recorrido y reposicionamiento simple.
- [x] Agregar guia visual tenue en el suelo.

### Camara y HUD minimo

- [x] Instalar/configurar Cinemachine 3.x si falta.
- [x] Crear camara tercera persona.
- [x] Crear camara primera persona.
- [x] Implementar `CameraController.cs` con cambio en `V`.
- [x] Crear HUD Canvas con velocimetro.
- [x] Crear cronometro.
- [x] Crear cuenta regresiva de 3 segundos.
- [x] Crear pantalla de resultados con tiempo final y medalla.

### Criterio de aceptacion

- [x] El jugador puede iniciar el 620 desde menu o escena de prueba.
- [x] La cuenta regresiva bloquea el movimiento hasta terminar.
- [x] El colectivo recorre el 620 de punta a punta.
- [x] El juego detecta llegada a meta.
- [x] La pantalla de resultados muestra tiempo final.
- [x] Primera y tercera persona funcionan durante el recorrido.

## Fase 2 - Loop Competitivo Basico

Objetivo: convertir la vertical slice en un loop rejugable con medallas, puntaje base, replay y guardado.

### Puntaje y medallas

- [ ] Implementar `ScoreManager.cs`.
- [ ] Calcular medalla segun tiempos `Gold`, `Silver`, `Bronze`.
- [ ] Calcular puntaje base por tiempo restante y categoria.
- [ ] Mostrar desglose simple en resultados.
- [ ] Registrar personal best por recorrido.

### Repeticiones

- [ ] Implementar `ReplayData.cs` y `ReplayFrame.cs`.
- [ ] Implementar grabacion de estado del vehiculo a 20 fps.
- [ ] Guardar replay en `%AppData%/BondiSimulator/Replays/`.
- [ ] Serializar replay como JSON versionado comprimido con GZip.
- [ ] Implementar reproduccion local simple del ultimo replay.
- [ ] Crear `ReplayViewer.unity` con controles basicos: play, pausa, velocidad.

### Guardado

- [ ] Implementar `SaveSystem.cs`.
- [ ] Implementar `PlayerProgress.cs` y `RouteRecord.cs`.
- [ ] Guardar `progress.json` en `%AppData%/BondiSimulator/`.
- [ ] Cargar progreso al iniciar.
- [ ] Bloquear guardado durante `Racing`.
- [ ] Agregar autoguardado al completar recorrido si la opcion esta activa.

### Criterio de aceptacion

- [ ] Completar el 620 guarda o actualiza el record local.
- [ ] Se genera un replay reproducible.
- [ ] Reiniciar el juego conserva progreso y records.
- [ ] Fallar el tiempo objetivo permite terminar, registra personal best y no desbloquea contenido.

## Fase 3 - Estilo, Pasajeros Y Trafico Minimo En 620

Objetivo: validar las mecanicas diferenciales del concepto sin expandir todavia a mas recorridos.

### Bocinas, luces y acciones

- [ ] Implementar `HornController.cs` con bocina normal.
- [ ] Implementar bocina de aire con input separado.
- [ ] Implementar `LightsController.cs`.
- [ ] Agregar SFX placeholder para bocinas.
- [ ] Emitir eventos de bocina y luces hacia `GameEvents`.

### Pasajeros y paradas

- [ ] Implementar `BusStop.cs`.
- [ ] Implementar `PassengerManager.cs`.
- [ ] Crear pasajeros placeholder como billboards o modelos simples.
- [ ] Detectar frenado correcto en parada.
- [ ] Otorgar puntos por atender publico.
- [ ] Mostrar indicador HUD de pasajeros esperando.
- [ ] Poblar el 620 con paradas minimas.

### Trafico y oportunidades de estilo

- [ ] Configurar NavMesh/AI Navigation en la escena 620.
- [ ] Implementar `TrafficManager.cs` con pool limitado.
- [ ] Implementar `TrafficSpawner.cs` fuera de vista.
- [ ] Implementar `VehicleAI.cs` simple: seguir carril y frenar ante obstaculos.
- [ ] Crear 2 prefabs blockout de autos NPC.
- [ ] Crear colectivo NPC de carril contrario.
- [ ] Implementar saludo de companero con ventana de respuesta.
- [ ] Implementar bus-spotter placeholder.
- [ ] Otorgar puntos por posar para camaras.

### Criterio de aceptacion

- [ ] El 620 puede jugarse con trafico sin bloquearse constantemente.
- [ ] El jugador puede ganar puntos por las tres acciones de estilo MVP.
- [ ] Los puntos de estilo aparecen en HUD y resultados.
- [ ] Pasajeros y trafico no son obligatorios para ganar, solo afectan puntaje.

## Fase 4 - Demo MVP Con 3 Recorridos Y 3 Modelos

Objetivo: ampliar el contenido manteniendo el loop ya probado.

### Recorrido 622

- [ ] Crear `Route_622_Cristania_Laferrere.unity`.
- [ ] Crear blockout de calles angostas y giros de 90 grados.
- [ ] Crear `RouteData_622.asset`.
- [ ] Configurar waypoints, spawn, meta y limites.
- [ ] Poblar paradas y pasajeros.
- [ ] Ajustar trafico denso.
- [ ] Testear que sea completable y se sienta distinto al 620.

### Recorrido 378

- [ ] Crear `Route_378_Laferrere_KM30.unity`.
- [ ] Crear blockout con elevacion, Independencia y conexion Ruta 3.
- [ ] Crear `RouteData_378.asset`.
- [ ] Configurar waypoints, spawn, meta y limites.
- [ ] Poblar paradas y pasajeros.
- [ ] Ajustar trafico y ritmo del recorrido.
- [ ] Testear que sea completable y se sienta distinto al 620 y 622.

### Modelos de colectivo

- [ ] Crear prefab OH 1618 Ugarte Europeo para 620/378.
- [ ] Crear prefab OH 1621 Nuovobus Citta para 96/variante de velocidad.
- [ ] Crear prefab OF 1621 Bimet Corwin Corbus para 622.
- [ ] Crear `VehicleData` para cada modelo.
- [ ] Ajustar diferencias: base equilibrado, Citta rapido, Corbus maniobrable.
- [ ] Implementar variantes visuales especiales de baja probabilidad sin ventaja de stats.

### Seleccion y menu

- [ ] Crear flujo de menu: empresa -> linea -> recorrido -> modelo.
- [ ] Crear pantalla de previa con mapa simple del recorrido.
- [ ] Mostrar distancia, categoria y tiempos objetivo.
- [ ] Mostrar stats del modelo elegido.
- [ ] Spawnear el prefab correcto segun seleccion.

### Criterio de aceptacion

- [ ] Los tres recorridos son completables.
- [ ] Los tres modelos se pueden seleccionar.
- [ ] Cada modelo se siente distinto sin romper el balance.
- [ ] El menu permite iniciar cualquier recorrido disponible.

## Fase 5 - Audio, Polish Y Build

Objetivo: cerrar una demo presentable para PC.

### Audio

- [ ] Implementar `AudioManager.cs`.
- [ ] Crear `AudioMixer.mixer` con grupos `Music`, `SFX_Vehicle`, `SFX_Environment`, `UI`.
- [ ] Integrar musica de menu.
- [ ] Integrar musica de gameplay.
- [ ] Implementar loop de motor con pitch segun velocidad.
- [ ] Agregar SFX de freno, rueda, bocina, aire y UI.
- [ ] Agregar ambiente urbano basico.

### UI final

- [ ] Crear pantalla de opciones: volumen, resolucion, autoguardado.
- [ ] Crear pantalla de pausa: continuar, reiniciar, guardar si permitido, salir.
- [ ] Mejorar resultados: tiempo, medalla, puntaje base, estilo, total.
- [ ] Agregar feedback visual consistente para puntos de estilo.
- [ ] Revisar legibilidad del HUD en 1080p.

### QA y build

- [ ] Configurar Player Settings: nombre, icono, Windows x64.
- [ ] Revisar escenas en Build Settings.
- [ ] Configurar compresion de texturas y audio.
- [ ] Agregar LODs basicos a vehiculos y entorno relevante.
- [ ] Habilitar Occlusion Culling donde aporte.
- [ ] Testear los tres recorridos completos.
- [ ] Crear `KNOWN_ISSUES.md`.
- [ ] Generar build final de demo.

### Criterio de aceptacion

- [ ] La build corre en Windows x64.
- [ ] Los tres recorridos se pueden completar en build.
- [ ] No hay errores criticos de consola durante una carrera normal.
- [ ] Existe lista de bugs conocidos antes de compartir la demo.

## Backlog Post-MVP

- [ ] Arbol completo de desbloqueos por punto B y ramales conectados.
- [ ] Mas recorridos y empresas.
- [ ] Mas modelos de colectivos.
- [ ] Recorridos construidos por secciones modulares.
- [ ] Leaderboards locales.
- [ ] Compartir repeticiones desde servidor o plataforma.
- [ ] Soporte formal de gamepad/volante.
- [ ] FMOD para audio avanzado.
- [ ] Semaforos con logica real.
- [ ] Penalizaciones opcionales para modos mas simulador.
