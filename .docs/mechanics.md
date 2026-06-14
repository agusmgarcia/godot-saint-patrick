# Mecánicas

El videojuego se desarrolla en torno al colegio Saint Patrick. En particular la noche de fiesta de graduación de los alumnos del último año y su inmediato día hábil. Está separado en dos capítulos muy diferenciados.

## Capítulo 1

La noche de la fiesta de graduación del colegio Saint Patrick. Donde muere Sarah. En este capítulo Helena interactúa con los personajes y recoge objetos del entorno sin ningún propósito especial. Hasta este momento ella no sabía que Sarah iba a morir. Las evidencias servirán para el segundo capítulo donde se llevará a cabo la investigación por parte de Helena, la protagonista y amiga de Sarah.

Ante cada interacción con un personaje Helena puede decir la verdad o mentir. En función de cada decisión, los personajes pueden cerrarse o aportar información que será relevante para el próximo capítulo. La opción “correcta”, la que expande la historia, no siempre es la de decir la verdad.

El final de este capítulo concluye con la muerte de Sarah en la fiesta. Aparentemente se había arrojado por el puente que conecta la entrada del colegio con el edificio.

## Capítulo 2

El día hábil siguiente, donde Helena, la protagonista y amiga de Sarah, se entera que rápida y sospechosamente caratularon el caso como suicidio. En este capítulo, Helena vuelve a interactuar con los mismos personajes de la fiesta, en el mismo entorno escolar pero esta vez para averiguar qué fue lo que pasó realmente.

Ahora los roles se invierten: los mismos personajes que aparecieron en el capítulo 1 son los que pueden estar diciendo la verdad o mintiendo. Helena en cambio puede creer su versión o refutarla si y solo si tiene evidencia.

En función de las pruebas y testimonios que Helena reúna, la historia finalizará con un acusado. Hay un solo asesino y el resto son personajes que tenían motivación para hacerlo pero que no lo hicieron. Se generan tantos finales como personajes sospechosos hay en la historia.

El caso default, el que se consigue cuando hay falta de evidencia o respuestas incorrectas, es el del suicidio. Ya que todos los personajes, de base, creen que Sarah tenía motivos para hacerlo producto de sus trastornos mentales. Por otro lado están los que les conviene que haya sido un suicidio, para no desenmascarar secretos más grandes. Por ejemplo vínculos entre profesores y alumnos, venta de drogas o relaciones homosexuales.

## Mecánica de tiempo

El capítulo 1 está gobernado por un timer visible que muestra la hora actual de la noche. Los eventos de la cronología ocurren en momentos fijos independientemente de lo que haga el jugador: los personajes se mueven, aparecen y desaparecen según la hora. Esto significa que Helena no puede interactuar con todos los personajes en una sola partida.

### Cómo avanza el tiempo

- **Moverse por el escenario no consume tiempo.** El jugador puede desplazarse libremente entre las zonas disponibles sin penalización.
- **Cada interacción consume 5 minutos.** Esto incluye iniciar una conversación, elegir una opción de diálogo, recoger un objeto o capturar un recuerdo. En una conversación larga, cada prompt suma 5 minutos adicionales.
- **El jugador puede retirarse de una conversación en cualquier momento**, salvo en los diálogos obligatorios. Las opciones disponibles en cada turno de diálogo son siempre: responder verdad, mentir o irse.

### Consecuencias del tiempo

Dado que las interacciones consumen tiempo y los eventos ocurren a horas fijas, el jugador debe elegir en qué personajes y objetos invertir su tiempo. Una conversación larga con un personaje puede significar no llegar a tiempo para interactuar con otro antes de que se mueva o desaparezca de la escena.

Los tiempos exactos que consume cada diálogo son un aspecto a refinar en una segunda etapa del diseño.

## Mecánica de diálogo

### Capitulo 1

En el capítulo 1, ante cada interacción con un personaje, Helena puede elegir decir la verdad o mentir. Esta decisión afecta lo que el personaje está dispuesto a revelar o no.

- La opción “correcta”, la que expande la historia, no siempre es la de decir la verdad.
- Cuando una decisión cierra una puerta, esa puerta se cierra para siempre dentro de esa partida.
- Las decisiones no afectan el hecho de que Sarah haya sido asesinada esa noche. Solo afectan la conclusión final del capítulo 2: quién es el acusado.

## Mecánica de inventario — Objetos y recuerdos

Helena puede recoger objetos físicos del entorno o capturar recuerdos. Los recuerdos son situaciones o detalles que le llaman la atención: no se lleva el objeto en sí, sino la memoria de haberlo visto o notado.

### Características de los objetos y recuerdos

- En el capítulo 1 todos parecen inocentes, llamativos o sin importancia. Ninguno puede incriminar a nadie en ese momento porque hasta entonces nadie ha muerto.
- En el capítulo 2 cobran otro significado a la luz de la investigación.
- Los personajes del capítulo 2 no reaccionan directamente a lo que Helena les dijo a otros personajes, sino a los objetos y recuerdos que Helena tiene en su inventario. El inventario es el puente entre conversaciones y partidas.

### Cómo se obtienen

Algunos objetos y recuerdos están disponibles simplemente explorando el escenario. Otros solo se desbloquean si Helena tomó ciertas decisiones en conversaciones previas o si tiene determinados ítems en su inventario. Esto hace que el inventario de cada jugador sea distinto al final del capítulo 1.

## Resultado

Los objetos y recuerdos conseguidos inclinan la balanza para uno de los sospechosos. Al final del juego, el sospechoso que tenga más puntos es el que finalmente termina acusado.

## Zonas del escenario

### Capitulo 1

Las zonas del capítulo 1 son las siguientes. Algunas se abren o se cierran a determinadas horas de la noche:

- **Garita** - Disponible desde el inicio.
- **Puente** — Disponible desde el inicio. Se vacía cuando Hudson llama a todos a ingresar.
- **Salón** — Se abre cuando Hudson hace la apertura oficial.
- **Baños** — Accesibles desde que el salón está abierto.
