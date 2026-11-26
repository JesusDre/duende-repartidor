"""
Genera los modelos low-poly del juego y los exporta como FBX a Assets/Models.

Uso (desde la raíz del proyecto):
    blender --background --factory-startup --python Tools/Blender/generar_modelos.py

Convenciones:
- 1 unidad de Blender = 1 metro = 1 unidad de Unity.
- Cada modelo tiene su origen en el suelo (z = 0) y mira hacia -Y en Blender
  (el frente), que al importar en Unity queda mirando hacia +Z.
- Los nombres de material (M_Rojo, M_Verde, ...) los reconoce el constructor
  de escena de Unity para asignar materiales URP.
"""
import math
import os
import sys

import bpy

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
OUT_DIR = os.path.join(ROOT, "Assets", "Models")

COLORES = {
    "M_Rojo": (0.80, 0.08, 0.10),
    "M_Verde": (0.10, 0.55, 0.20),
    "M_VerdeOscuro": (0.05, 0.30, 0.10),
    "M_Piel": (0.96, 0.76, 0.62),
    "M_Blanco": (0.95, 0.95, 0.95),
    "M_Negro": (0.03, 0.03, 0.03),
    "M_Dorado": (0.95, 0.70, 0.10),
    "M_Cafe": (0.40, 0.24, 0.12),
    "M_Madera": (0.62, 0.42, 0.22),
    "M_Pared": (0.98, 0.88, 0.70),
    "M_Turquesa": (0.15, 0.70, 0.70),
    "M_Rosa": (0.95, 0.45, 0.60),
    "M_Naranja": (0.95, 0.45, 0.10),
    "M_Azul": (0.15, 0.35, 0.85),
    "M_Vidrio": (0.55, 0.80, 0.95),
}


# ---------------------------------------------------------------- utilidades
def limpiar_escena():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete()
    for coleccion in (bpy.data.meshes, bpy.data.materials):
        for item in list(coleccion):
            coleccion.remove(item)


def material(nombre):
    mat = bpy.data.materials.get(nombre)
    if mat is None:
        mat = bpy.data.materials.new(nombre)
        r, g, b = COLORES[nombre]
        mat.diffuse_color = (r, g, b, 1.0)
        mat.use_nodes = True
        bsdf = mat.node_tree.nodes.get("Principled BSDF")
        if bsdf:
            bsdf.inputs["Base Color"].default_value = (r, g, b, 1.0)
            bsdf.inputs["Roughness"].default_value = 0.8
    return mat


def terminar(obj, nombre, mat, parent):
    obj.name = nombre
    obj.data.name = nombre
    obj.data.materials.clear()
    obj.data.materials.append(material(mat))
    bpy.ops.object.shade_flat()
    obj.parent = parent
    return obj


def cubo(nombre, mat, parent, loc, escala, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc, rotation=rot)
    obj = bpy.context.active_object
    obj.scale = escala
    return terminar(obj, nombre, mat, parent)


def cilindro(nombre, mat, parent, loc, radio, alto, lados=8, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cylinder_add(vertices=lados, radius=radio, depth=alto, location=loc, rotation=rot)
    return terminar(bpy.context.active_object, nombre, mat, parent)


def cono(nombre, mat, parent, loc, r1, r2, alto, lados=8, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cone_add(vertices=lados, radius1=r1, radius2=r2, depth=alto, location=loc, rotation=rot)
    return terminar(bpy.context.active_object, nombre, mat, parent)


def esfera(nombre, mat, parent, loc, radio, escala=(1, 1, 1)):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=radio, location=loc)
    obj = bpy.context.active_object
    obj.scale = escala
    return terminar(obj, nombre, mat, parent)


def prisma(nombre, mat, parent, centro, ancho, profundo, alto):
    """Techo a dos aguas: prisma triangular con la cumbrera a lo largo de Y."""
    cx, cy, z = centro
    a, p = ancho / 2, profundo / 2
    verts = [(cx - a, cy - p, z), (cx + a, cy - p, z), (cx, cy - p, z + alto),
             (cx - a, cy + p, z), (cx + a, cy + p, z), (cx, cy + p, z + alto)]
    caras = [(0, 1, 2), (5, 4, 3), (0, 3, 4, 1), (1, 4, 5, 2), (2, 5, 3, 0)]
    malla = bpy.data.meshes.new(nombre)
    malla.from_pydata(verts, [], caras)
    malla.update()
    obj = bpy.data.objects.new(nombre, malla)
    bpy.context.scene.collection.objects.link(obj)
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    return terminar(obj, nombre, mat, parent)


def raiz(nombre):
    bpy.ops.object.empty_add(type="PLAIN_AXES", location=(0, 0, 0))
    obj = bpy.context.active_object
    obj.name = nombre
    return obj


def exportar(nombre):
    bpy.ops.object.select_all(action="SELECT")
    # Aplica escala y rotación para que Unity reciba transformaciones limpias.
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    ruta = os.path.join(OUT_DIR, nombre + ".fbx")
    bpy.ops.export_scene.fbx(
        filepath=ruta,
        use_selection=False,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_ALL",
        bake_space_transform=True,
        object_types={"EMPTY", "MESH"},
        axis_forward="-Z",
        axis_up="Y",
        add_leaf_bones=False,
        bake_anim=False,
    )
    print(f"[modelos] Exportado {ruta}")


# ------------------------------------------------------------------ modelos
def modelo_duende():
    r = raiz("Duende")
    # Piernas y botas
    for lado, x in (("I", -0.09), ("D", 0.09)):
        cilindro(f"Pierna{lado}", "M_Rojo", r, (x, 0, 0.17), 0.055, 0.26)
        cubo(f"Bota{lado}", "M_Cafe", r, (x, -0.04, 0.04), (0.11, 0.18, 0.08))
    # Cuerpo (túnica) y cinturón
    cono("Tunica", "M_Verde", r, (0, 0, 0.48), 0.24, 0.15, 0.40)
    cilindro("Cinturon", "M_Negro", r, (0, 0, 0.40), 0.205, 0.05)
    cubo("Hebilla", "M_Dorado", r, (0, -0.20, 0.40), (0.07, 0.02, 0.05))
    # Brazos
    for lado, x in (("I", -0.22), ("D", 0.22)):
        cilindro(f"Brazo{lado}", "M_Verde", r, (x, 0, 0.50), 0.045, 0.28, rot=(0, math.radians(15 if x > 0 else -15), 0))
        esfera(f"Mano{lado}", "M_Piel", r, (x * 1.12, 0, 0.35), 0.05)
    # Cabeza, nariz, ojos y orejas puntiagudas
    esfera("Cabeza", "M_Piel", r, (0, 0, 0.82), 0.17)
    esfera("Nariz", "M_Rosa", r, (0, -0.17, 0.80), 0.035)
    for lado, x in (("I", -0.06), ("D", 0.06)):
        esfera(f"Ojo{lado}", "M_Negro", r, (x, -0.15, 0.86), 0.025)
        cono(f"Oreja{lado}", "M_Piel", r, (x * 3.2, 0, 0.86), 0.04, 0.0, 0.16, lados=4,
             rot=(0, math.radians(-70 if x < 0 else 70), 0))
    # Gorro con borla
    cilindro("BordeGorro", "M_Blanco", r, (0, 0, 0.96), 0.175, 0.06, lados=10)
    cono("Gorro", "M_Rojo", r, (0, 0.04, 1.14), 0.165, 0.02, 0.34, lados=10, rot=(math.radians(15), 0, 0))
    esfera("Borla", "M_Blanco", r, (0, 0.10, 1.31), 0.05)
    exportar("Duende")


def modelo_regalo():
    r = raiz("Regalo")
    cubo("Caja", "M_Rojo", r, (0, 0, 0.2), (0.4, 0.4, 0.4))
    cubo("ListonX", "M_Dorado", r, (0, 0, 0.2), (0.42, 0.08, 0.42))
    cubo("ListonY", "M_Dorado", r, (0, 0, 0.2), (0.08, 0.42, 0.42))
    for i, ang in enumerate((45, -45)):
        esfera(f"Mono{i}", "M_Dorado", r, (0.06 * (1 if ang > 0 else -1), 0, 0.43), 0.06, escala=(1.4, 0.6, 0.8))
    exportar("Regalo")


def modelo_casa():
    r = raiz("Casa")
    cubo("Base", "M_Madera", r, (0, 0, 0.1), (4.2, 4.2, 0.2))
    cubo("Paredes", "M_Pared", r, (0, 0, 1.5), (3.6, 3.6, 2.6))
    # Techo a dos aguas
    prisma("Techo", "M_Turquesa", r, (0, 0, 2.8), 4.4, 4.4, 1.5)
    cubo("Puerta", "M_Cafe", r, (0, -1.81, 1.1), (0.9, 0.06, 1.8))
    esfera("Perilla", "M_Dorado", r, (0.3, -1.85, 1.1), 0.05)
    for lado, x in (("I", -1.15), ("D", 1.15)):
        cubo(f"Ventana{lado}", "M_Vidrio", r, (x, -1.81, 1.7), (0.7, 0.05, 0.7))
        cubo(f"Marco{lado}", "M_Blanco", r, (x, -1.80, 1.7), (0.8, 0.04, 0.8))
    # Corona navideña en la puerta
    bpy.ops.mesh.primitive_torus_add(major_radius=0.22, minor_radius=0.06, location=(0, -1.88, 2.25),
                                     rotation=(math.radians(90), 0, 0), major_segments=10, minor_segments=4)
    terminar(bpy.context.active_object, "Corona", "M_VerdeOscuro", r)
    esfera("MonoCorona", "M_Rojo", r, (0, -1.9, 2.05), 0.06)
    exportar("Casa")


def modelo_palmera():
    r = raiz("Palmera")
    alto_seg = 0.7
    for i in range(6):
        cilindro(f"Tronco{i}", "M_Madera", r, (0.05 * i, 0, alto_seg * (i + 0.5)), 0.18 - i * 0.015, alto_seg * 0.95, lados=6)
    copa_z = alto_seg * 6
    cx = 0.05 * 5
    for i in range(7):
        ang = i * (360 / 7)
        hoja = cono(f"Hoja{i}", "M_Verde", r, (cx, 0, copa_z), 0.35, 0.02, 2.2, lados=4)
        hoja.scale = (1.0, 0.25, 1.0)
        hoja.rotation_euler = (math.radians(70), 0, math.radians(ang))
        # Desplaza la hoja hacia afuera según su dirección
        hoja.location = (cx + math.sin(math.radians(ang)) * -0.9, math.cos(math.radians(ang)) * 0.9, copa_z - 0.25)
    for i in range(3):
        ang = math.radians(i * 120)
        esfera(f"Coco{i}", "M_Cafe", r, (cx + math.cos(ang) * 0.2, math.sin(ang) * 0.2, copa_z - 0.2), 0.13)
    exportar("Palmera")


def modelo_taller():
    r = raiz("Taller")
    cubo("Base", "M_Madera", r, (0, 0, 0.1), (7.0, 6.0, 0.2))
    cubo("Paredes", "M_Rojo", r, (0, 0.5, 2.0), (6.0, 4.5, 3.6))
    prisma("Techo", "M_Blanco", r, (0, 0.5, 3.8), 6.8, 5.2, 1.8)
    cubo("Porton", "M_Cafe", r, (0, -1.76, 1.4), (2.4, 0.08, 2.6))
    # Columnas de bastón de caramelo
    for lado, x in (("I", -2.9), ("D", 2.9)):
        for i in range(8):
            cilindro(f"Baston{lado}{i}", "M_Rojo" if i % 2 == 0 else "M_Blanco", r,
                     (x, -2.5, 0.2 + 0.4 * i + 0.2), 0.16, 0.4, lados=8)
        esfera(f"Tope{lado}", "M_Rojo", r, (x, -2.5, 3.55), 0.2)
    cubo("Letrero", "M_VerdeOscuro", r, (0, -2.5, 3.6), (5.4, 0.15, 0.7))
    cubo("LetreroBorde", "M_Dorado", r, (0, -2.45, 3.6), (5.6, 0.1, 0.85))
    # Pila de regalos de adorno junto al portón
    for i, (x, y, s, m) in enumerate(((-2.0, -2.2, 0.5, "M_Azul"), (-1.6, -2.4, 0.35, "M_Verde"),
                                      (2.0, -2.2, 0.55, "M_Rosa"), (1.9, -2.3, 0.3, "M_Naranja"))):
        cubo(f"RegaloAdorno{i}", m, r, (x, y, 0.2 + s / 2 + (0.5 if i in (1, 3) else 0)), (s, s, s))
    exportar("Taller")


def modelo_cangrejo():
    r = raiz("Cangrejo")
    esfera("Caparazon", "M_Naranja", r, (0, 0, 0.18), 0.22, escala=(1.3, 1.0, 0.55))
    for lado, x in (("I", -1), ("D", 1)):
        for i in range(3):
            pata = cilindro(f"Pata{lado}{i}", "M_Rojo", r, (x * 0.30, -0.1 + i * 0.1, 0.08), 0.02, 0.25, lados=5)
            pata.rotation_euler = (0, math.radians(60 * x), 0)
        cilindro(f"BrazoPinza{lado}", "M_Rojo", r, (x * 0.2, -0.25, 0.15), 0.03, 0.2, lados=5,
                 rot=(math.radians(70), 0, math.radians(-30 * x)))
        esfera(f"Pinza{lado}", "M_Rojo", r, (x * 0.25, -0.36, 0.18), 0.08, escala=(0.8, 1.2, 0.6))
        cilindro(f"Antena{lado}", "M_Rojo", r, (x * 0.07, -0.15, 0.32), 0.012, 0.14, lados=4)
        esfera(f"Ojo{lado}", "M_Negro", r, (x * 0.07, -0.15, 0.40), 0.035)
    # Gorrito navideño
    cono("Gorro", "M_Rojo", r, (0, 0.02, 0.43), 0.09, 0.0, 0.2, lados=8)
    esfera("Borla", "M_Blanco", r, (0, 0.02, 0.54), 0.03)
    exportar("Cangrejo")


def main():
    os.makedirs(OUT_DIR, exist_ok=True)
    modelos = [modelo_duende, modelo_regalo, modelo_casa, modelo_palmera, modelo_taller, modelo_cangrejo]
    for modelo in modelos:
        limpiar_escena()
        modelo()
    print("[modelos] Listo.")


if __name__ == "__main__":
    try:
        main()
    except Exception as exc:  # noqa: BLE001
        import traceback
        traceback.print_exc()
        sys.exit(1)
