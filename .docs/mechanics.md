# Mecánicas

El videojuego se desarrolla en torno al colegio Saint Patrick. En particular la noche de fiesta de graduación de los alumnos del último año y su inmediato día hábil. Está separado en dos capítulos muy diferenciados.

## Capítulo 1

La noche de la fiesta de graduación del colegio Saint Patrick. Donde muere Sarah. En este capítulo Helena interactúa con los personajes y recoge objetos del entorno sin ningún propósito especial. Hasta este momento ella no sabía que Sarah iba a morir. Las evidencias servirán para el segundo capítulo donde se llevará a cabo la investigación por parte de Helena, la protagonista y amiga de Sarah.

Ante cada interacción con un personaje Helena puede decir la verdad o mentir. En función de cada decisión, los personajes pueden cerrarse o aportar información que será relevante para el próximo capítulo. La opción "correcta", la que expande la historia, no siempre es la de decir la verdad.

El final de este capítulo concluye con la muerte de Sarah en la fiesta. Aparentemente se había arrojado por el puente que conecta la entrada del colegio con el edificio.

## Capítulo 2

El día hábil siguiente, un lunes, donde Helena se entera que rápida y sospechosamente caratularon el caso como suicidio. El capítulo se inicia en el salón, donde Hudson da un discurso sobre la muerte de Sarah y menciona que la policía estará presente durante el día interrogando a alumnos y personal. Es ahí donde Helena decide investigar por su cuenta.

En este capítulo Helena vuelve a interactuar con los mismos personajes de la fiesta, en el mismo entorno escolar pero esta vez para averiguar qué fue lo que pasó realmente.

Ahora los roles se invierten: los mismos personajes que aparecieron en el capítulo 1 son los que pueden estar diciendo la verdad o mintiendo. Helena en cambio puede creer su versión o refutarla si y solo si tiene evidencia.

En función de las pruebas y testimonios que Helena reúna, la historia finalizará con un acusado. Hay un solo asesino y el resto son personajes que tenían motivación para hacerlo pero que no lo hicieron. Se generan tantos finales como personajes sospechosos hay en la historia.

El caso default, el que se consigue cuando hay falta de evidencia o cuando Sarah acumula más puntos que cualquier sospechoso, es el del suicidio. Ya que todos los personajes, de base, creen que Sarah tenía motivos para hacerlo producto de sus trastornos mentales. Por otro lado están los que les conviene que haya sido un suicidio, para no desenmascarar secretos más grandes. Por ejemplo vínculos entre profesores y alumnos, venta de drogas o relaciones homosexuales.

El capítulo cierra cuando la policía llama a Helena para interrogarla. Ella acusa automáticamente al sospechoso con más puntos que supere el umbral mínimo establecido. El capítulo finaliza con una escena no interactuable donde la policía se lleva detenido al acusado.

## Mecánica de tiempo

Ambos capítulos están gobernados por un timer visible que muestra la hora actual. Los eventos de la cronología ocurren en momentos fijos independientemente de lo que haga el jugador: los personajes se mueven, aparecen y desaparecen según la hora. Esto significa que Helena no puede interactuar con todos los personajes en una sola partida.

### Cómo avanza el tiempo

- **Moverse por el escenario no consume tiempo.** El jugador puede desplazarse libremente entre las zonas disponibles sin penalización.
- **Cada interacción consume 5 minutos.** Esto incluye iniciar una conversación, elegir una opción de diálogo, recoger un objeto o capturar un recuerdo. En una conversación larga, cada prompt suma 5 minutos adicionales.
- **El jugador puede retirarse de una conversación en cualquier momento**, salvo en los diálogos obligatorios. Las opciones disponibles en cada turno de diálogo son siempre: decir la verdad, mentir o irse (capítulo 1) y creer, refutar o irse (capítulo 2).

### Consecuencias del tiempo

Dado que las interacciones consumen tiempo y los eventos ocurren a horas fijas, el jugador debe elegir en qué personajes y objetos invertir su tiempo. Una conversación larga con un personaje puede significar no llegar a tiempo para interactuar con otro antes de que se mueva o desaparezca de la escena.

Los tiempos exactos que consume cada diálogo son un aspecto a refinar en una segunda etapa del diseño.

## Mecánica de diálogo

### Capítulo 1

En el capítulo 1, ante cada interacción con un personaje, Helena puede elegir decir la verdad o mentir. Esta decisión afecta lo que el personaje está dispuesto a revelar o no.

- La opción "correcta", la que expande la historia, no siempre es la de decir la verdad.
- Cuando una decisión cierra una puerta, esa puerta se cierra para siempre dentro de esa partida.
- Las decisiones no afectan el hecho de que Sarah haya sido asesinada esa noche. Solo afectan la conclusión final del capítulo 2: quién es el acusado.

### Capítulo 2

En el capítulo 2, ante cada interacción con un personaje, Helena escucha su testimonio y decide qué hacer con él. Las opciones disponibles en cada turno son:

- **Creer:** Helena acepta el testimonio como verdadero y lo incorpora a su inventario como recuerdo. Este recuerdo puede ser verdadero o falso; Helena no lo sabe en ese momento. Si es falso, puede sumar puntos al sospechoso equivocado e inclinar la acusación final hacia un final incorrecto.
- **Refutar:** Helena desafía el testimonio presentando una prueba concreta de su inventario (un recuerdo u objeto adquirido previamente, ya sea en el capítulo 1 o en el mismo capítulo 2).
  - Si la prueba es correcta, se desbloquea un nuevo recuerdo verdadero que suma puntos al sospechoso correcto.
  - Si la prueba es incorrecta, el personaje se cierra y no hay más información disponible por ese camino.
- **Irse:** Helena abandona la conversación sin obtener información.

## Mecánica de inventario — Objetos y recuerdos

Helena puede recoger objetos físicos del entorno o capturar recuerdos. Los recuerdos son situaciones o detalles que le llaman la atención: no se lleva el objeto en sí, sino la memoria de haberlo visto o notado.

### Características de los objetos y recuerdos

- En el capítulo 1 todos parecen inocentes, llamativos o sin importancia. Ninguno puede incriminar a nadie en ese momento porque hasta entonces nadie ha muerto.
- En el capítulo 2 cobran otro significado a la luz de la investigación. Además Helena puede obtener nuevos recuerdos y objetos durante la investigación, que también suman puntos a los sospechosos.
- Los personajes del capítulo 2 no reaccionan directamente a lo que Helena les dijo a otros personajes, sino a los objetos y recuerdos que Helena tiene en su inventario. El inventario es el puente entre conversaciones y partidas.

### Cómo se obtienen

Algunos objetos y recuerdos están disponibles simplemente explorando el escenario. Otros solo se desbloquean si Helena tomó ciertas decisiones en conversaciones previas o si tiene determinados ítems en su inventario. Esto hace que el inventario de cada jugador sea distinto al final del capítulo 1 y a lo largo del capítulo 2.

## Sistema de puntos y acusación

Cada recuerdo u objeto del inventario suma puntos a un sospechoso específico, incluyendo a Sarah. El valor de cada evidencia depende de su contundencia, no del capítulo donde se encuentre, en una escala de 1 a 3:

- **1 punto** — Evidencia circunstancial o comportamiento sospechoso
- **2 puntos** — Evidencia que implica directamente al sospechoso pero que puede tener otra explicación
- **3 puntos** — Evidencia directa y difícilmente refutable

Al finalizar el capítulo 2, cuando la policía llama a Helena para interrogarla, ella acusa automáticamente al sospechoso con más puntos que supere el umbral mínimo establecido. Si ningún sospechoso supera ese umbral, o si Sarah es quien acumula más puntos, el final es el del suicidio.

### Sospechosos y finales

- **Elijah** — El asesino real. El final correcto y el más difícil de conseguir, ya que su motivación está completamente oculta y Helena tiene un sesgo emocional para no sospecharlo.
- **Abraham** — Culpable de abuso pero no de asesinato. Acusarlo es un final incorrecto.
- **Christian** — Culpable de violencia pero no de asesinato. Acusarlo es un final incorrecto.
- **Thomas** — Culpable de venta de drogas pero no de asesinato. Acusarlo es un final incorrecto.
- **Stephanie** — Tenía motivos pero no lo hizo. Acusarla es un final incorrecto.
- **Hudson** — Encubridor pero no asesino. Acusarlo es un final incorrecto.
- **Suicidio** — Final default por falta de evidencia o si Sarah acumula más puntos que cualquier sospechoso.

## Zonas del escenario

### Capítulo 1

- **Garita** — Disponible desde el inicio.
- **Puente** — Disponible desde el inicio. Se vacía cuando Hudson llama a todos a ingresar.
- **Salón** — Se abre cuando Hudson hace la apertura oficial.

### Capítulo 2

- **Garita** — Disponible desde el inicio.
- **Puente** — Disponible desde el inicio.
- **Salón** — Disponible desde el inicio.
- **Pasillo con lockers** — Disponible desde el inicio.
- **Aula** — Disponible desde el inicio.
- **Estacionamiento** — Accesible bajo condiciones a definir.
- **Despacho de Hudson** — Accesible bajo condiciones a definir.
