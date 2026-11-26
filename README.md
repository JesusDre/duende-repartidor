# 🎁 Duende Repartidor

Prototipo 3D en Unity con temática de **una navidad distinta**: Santa se enfermó y un duende tiene que repartir todos los regalos por un pueblo tropical antes de que amanezca.

## Cómo se juega

- **WASD / flechas**: moverse · **Espacio**: saltar · **Mouse**: girar la cámara
- Recoge un regalo en el **taller** y llévalo a la casa que **brilla**.
- Entrega todos los regalos antes de que se acabe el tiempo.

## Requisitos

- Unity **6000.3.27f1** (Unity 6.3) con URP
- Input System (nuevo)
- Blender 5.x (opcional, solo para regenerar los modelos low-poly de `Tools/Blender`)

## Estructura

```
Assets/
  Scenes/        Escenas (MainMenu, Game)
  Scripts/       C# del juego (Player, Gameplay, UI, Core)
  Editor/        Herramientas de editor (constructor de escena)
  Models/        Modelos FBX exportados desde Blender
  Materials/     Materiales URP
Tools/Blender/   Scripts de Python que generan los modelos
```

## Desarrollo

El trabajo está dividido en issues de GitHub, que se resuelven uno por uno, cada uno en su propio commit.
