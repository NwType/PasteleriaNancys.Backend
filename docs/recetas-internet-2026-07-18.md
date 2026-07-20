# Recetas de internet adaptadas al modelo del sistema

**Fecha:** 2026-07-18 · **Preparado por:** Claude Code a pedido de Kevin
**Estado:** cargadas a la BD con **precio 0 + [PRECIO PENDIENTE]** — definir el precio real con la dueña antes de producirlas.

## Cómo leer este archivo

El sistema modela cada torta como **bizcocho en porciones (Intermedio, producido por la Horneada) + componentes de armado (cremas, jaleas, rellenos)**. La torta estándar es de **20 porciones = 2 bizcochos**. Por eso cada receta de internet se adapta en dos partes:

1. **Receta original (resumida y citada)** — lo que dice internet.
2. **Adaptación al sistema** — las líneas exactas cargadas en `Receta_Item`, por **1 torta**.

**Limitación honesta:** la masa auténtica de la Zanahoria y la Red Velvet es distinta a la batida estándar de la casa. El sistema hoy **solo produce los 2 bizcochos estándar** (la Horneada no fabrica otros intermedios — ver hallazgo "Preparados sin flujo de producción" del reporte de esta fecha). Mientras tanto, esas tortas se modelan como **bizcocho estándar + insumos de sabor en el armado**; cuando exista producción de "Preparados", se puede migrar a masa propia.

**Insumos nuevos creados sin stock** (registrar lote de compra antes de producir): Zanahoria, Canela Molida, Queso Crema, Colorante Rojo Comestible, Café Instantáneo. *Nota: "Zanahoria" quedó en categoría "Rellenos" porque la lista de categorías de materia prima es cerrada (Harinas y Secos / Lácteos y Cremas / Colorantes y Jaleas / Rellenos / Empaques) y es la menos mala; si se agrega una categoría "Frutas y Verduras" al catálogo, moverla.*

---

## 1. Torta de Durazno (solo insumos existentes)

**Fuentes:** [Torta rellena de durazno y crema](https://mundoreceta.blogspot.com/2018/11/torta-rellena-de-durazno-y-crema.html) · [Torta de duraznos y crema — Zesty Bake](https://zestybake.com/recetas/torta-de-duraznos-y-crema/) · [Torta de duraznos en almíbar — La Nación](https://www.lanacion.com.ar/recetas/tortas/torta-de-duraznos-en-almibar-nid03022023/)

**Original (resumen):** bizcocho de vainilla en dos capas, humedecido, relleno y cubierto con crema chantilly y duraznos en almíbar en cubos; decorado con gajos de durazno.

**Adaptación cargada (1 torta, 20 porciones):**

| Componente | Código | Cantidad |
|---|---|---|
| Bizcocho de Vainilla | PI-BIZC-001 | 20 porciones |
| Crema Mascrean | MP-CREM-002 | 0.40 kg |
| Relleno de Durazno | MP-RELL-001 | 0.30 kg |

---

## 2. Torta de Frutos Rojos (solo insumos existentes)

**Fuentes:** [Torta de frutos rojos — Cocineros Argentinos](http://cocinerosargentinos.com/pasteleria/torta-de-frutos-rojos) · [Torta blanca con frutos rojos — Isabel Vermal](https://isabelvermal.com/torta-blanca-con-frutos-rojos/)

**Original (resumen):** capas de bizcocho de vainilla con almíbar, crema (750 g de crema + azúcar impalpable en la receta de referencia), mermelada/frutos rojos frescos encima.

**Adaptación cargada (1 torta, 20 porciones):**

| Componente | Código | Cantidad |
|---|---|---|
| Bizcocho de Vainilla | PI-BIZC-001 | 20 porciones |
| Crema Mascrean | MP-CREM-002 | 0.40 kg |
| Relleno de Frutos Rojos | MP-RELL-002 | 0.30 kg |
| Jalea Roja de Frutilla (brillo/decoración) | MP-COLR-002 | 0.10 kg |

---

## 3. Torta Marmolada (solo insumos existentes)

**Fuentes:** [La mejor Torta Marmoleada — Anna's Pastelería](https://annaspasteleria.com/post/la-mejor-torta-marmoleada-del-mundo-vainilla-y-chocolate) · [Bizcochuelo Marmolado — Paulina Cocina](https://www.paulinacocina.net/bizcochuelo-marmolado/28289) · [Pastel o Torta Marmoleada — Sweet y Salado](https://sweetysalado.com/pastel-torta-marmoleada/)

**Original (resumen):** una sola masa dividida en dos; a una mitad se le agrega cacao; se colocan cucharadas alternadas de ambas y se marmolea con un palillo.

**Adaptación cargada (1 torta, 20 porciones):** la casa no mezcla masas en el molde — se aproxima alternando porciones de ambos bizcochos estándar en el armado.

| Componente | Código | Cantidad |
|---|---|---|
| Bizcocho de Vainilla | PI-BIZC-001 | 10 porciones |
| Bizcocho de Chocolate | PI-BIZC-002 | 10 porciones |
| Crema Mascrean | MP-CREM-002 | 0.40 kg |
| Jalea de Chocolate (veteado/decoración) | MP-COLR-001 | 0.10 kg |

**TipoMasa:** Mixto (entra a la recomendación de sabores de la Proyección de Horneado como las mixtas).

---

## 4. Torta de Nuez (solo insumos existentes)

**Fuentes:** [Torta de Nuez con Dulce de Leche y Chantilly](https://luciapaula.com/walnut-dulce-de-leche-cake-with-chantilly-cream/) · [Torta de nuez — Paulina Cocina](https://www.paulinacocina.net/torta-de-nuez/8716)

**Original (resumen):** bizcochuelo con nueces trituradas, relleno cremoso de nuez cocido con leche condensada y mantequilla, cubierta de crema.

**Adaptación cargada (1 torta, 20 porciones):**

| Componente | Código | Cantidad |
|---|---|---|
| Bizcocho de Vainilla | PI-BIZC-001 | 20 porciones |
| Crema Mascrean | MP-CREM-002 | 0.35 kg |
| Relleno de Nuez | MP-RELL-003 | 0.30 kg |
| Leche Condensada Mococa 395gr (relleno estilo dulce de leche de nuez) | MP-LACT-003 | 1 unidad |

---

## 5. Torta de Galleta — sin horno (solo insumos existentes; pendiente tuyo desde el 13/07)

**Fuentes:** [Tarta de galletas María sin horno — Directo al Paladar](https://www.directoalpaladar.com/postres/tarta-de-galletas-maria-receta-facil-sin-horno) · [Tarta de galletas en flor con leche condensada — Nestlé Cocina](https://www.nestlecocina.es/receta/tarta-de-galletas-en-flor-con-leche-condensada)

**Original (resumen):** capas de galletas María mojadas en leche alternadas con crema (leche condensada + queso/crema batida), 5–6 pisos, refrigerar mínimo 1 hora. **No lleva bizcocho ni horno.**

**Adaptación cargada (1 torta, 20 porciones):**

| Componente | Código | Cantidad |
|---|---|---|
| Galleta de Vainilla | MP-GALL-002 | 0.75 kg |
| Crema Pil | MP-CREM-003 | 0.40 kg |
| Leche Condensada Mococa 395gr | MP-LACT-003 | 2 unidades |
| Leche Pil 500ml (para mojar las galletas) | MP-LACT-002 | 1 unidad |

**Ojo:** MP-GALL-002, MP-LACT-002 y MP-LACT-003 están con **stock 0** — registrar compra antes de producirla. Sigue pendiente la **tabla de precios por tamaño** que quedó del 13/07.

---

## 6. Torta de Zanahoria (insumos nuevos: Zanahoria, Canela, Queso Crema)

**Fuentes:** [Torta o Pastel de Zanahoria — Sweet y Salado](https://sweetysalado.com/torta-o-pastel-de-zanahoria/) · [La mejor torta de zanahoria — Anna's Pastelería](https://annaspasteleria.com/post/la-mejor-torta-de-zanahoria) · [Torta de zanahoria — Paulina Cocina](https://www.paulinacocina.net/torta-de-zanahoria-super-facil/7289)

**Original (resumen, para ~20 cm):** 250 g harina, 250 g zanahoria rallada, 3 huevos, 250 g azúcar morena, 225 g aceite, 2 cdtas canela, 100 g nueces, 50 g pasas, polvo de hornear + bicarbonato. Cobertura: queso crema + mantequilla + azúcar glas.

**Adaptación cargada (1 torta, 20 porciones)** — *compromiso: masa estándar de la casa + sabor en el armado, hasta que exista producción de masas propias:*

| Componente | Código | Cantidad |
|---|---|---|
| Bizcocho de Vainilla | PI-BIZC-001 | 20 porciones |
| Zanahoria (rallada) | **nuevo** | 0.25 kg |
| Canela Molida | **nuevo** | 0.01 kg |
| Relleno de Nuez | MP-RELL-003 | 0.10 kg |
| Relleno de Pasas | MP-RELL-004 | 0.05 kg |
| Queso Crema (cobertura) | **nuevo** | 0.35 kg |

---

## 7. Torta Red Velvet (insumos nuevos: Colorante Rojo, Queso Crema)

**Fuentes:** [Tarta Red Velvet — Bon Viveur](https://bonviveur.com/es/recetas/red-velvet) · [Tarta Red Velvet — Directo al Paladar](https://www.directoalpaladar.com/postres/tarta-red-velvet-receta-clasico-pastel-terciopelo-rojo-ideal-para-celebrar-cualquier-ocasion-feliz)

**Original (resumen):** bizcocho de mantequilla con buttermilk, un toque de cacao (15 g) y colorante rojo; frosting de queso crema (150 g mantequilla + 300 g azúcar glas + 150 g queso crema).

**Adaptación cargada (1 torta, 20 porciones)** — *el sistema no tiene cacao en polvo como insumo (el sabor chocolate de la casa va por caramelina), así que el "toque de cacao" se aproxima con caramelina:*

| Componente | Código | Cantidad |
|---|---|---|
| Bizcocho de Vainilla | PI-BIZC-001 | 20 porciones |
| Colorante Rojo Comestible | **nuevo** | 0.02 kg |
| Caramelina (toque estilo cacao) | MP-CARA-001 | 0.005 kg |
| Queso Crema (frosting) | **nuevo** | 0.40 kg |

---

## 8. Torta Moka (insumo nuevo: Café Instantáneo)

**Fuentes:** [Torta Moka — La Cocina Chilena de Pilar Hernández](https://cocinachilena.cl/torta-moka-chocolate-y-cafe-y-los-cupcakes-de-navidad/) · [Torta moka — Isabel Vermal](https://isabelvermal.com/torta-moka/) · [Crema moka — Cocinatis](https://www.cocinatis.com/receta/crema-moka.html)

**Original (resumen):** capas de bizcocho de chocolate, crema de moka (crema batida + café + azúcar; variantes con cacao y mantequilla).

**Adaptación cargada (1 torta, 20 porciones):**

| Componente | Código | Cantidad |
|---|---|---|
| Bizcocho de Chocolate | PI-BIZC-002 | 20 porciones |
| Café Instantáneo | **nuevo** | 0.02 kg |
| Crema Mascrean | MP-CREM-002 | 0.45 kg |
| Jalea de Chocolate (decoración) | MP-COLR-001 | 0.10 kg |

---

## Checklist para producir estas tortas de verdad

1. **Ponerles precio real** (hoy Bs 0 + "[PRECIO PENDIENTE]" en la descripción) — sin esto no deben venderse.
2. **Comprar/registrar lotes** de: Huevo (bloquea TODA horneada), Zanahoria, Canela, Queso Crema, Colorante Rojo, Café, Galletas, Leche Pil, Leche Condensada.
3. Las tortas **no aparecen en la vitrina pública ni en el POS mientras tengan stock 0** — aparecerán solas al confirmarse su primer Viaje.
4. Cuando exista producción de "Preparados"/masas propias, migrar Zanahoria y Red Velvet a masa auténtica.
