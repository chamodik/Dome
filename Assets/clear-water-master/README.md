# Clear Water 3

Clear Water 3 is a water shader for Unity's Built-in Render Pipeline. It works on PC and mobile platforms, such as Meta Quest.

The shader uses physical light rules for refraction, light absorption, and subsurface scattering. It includes wave simulation, baked reflection, screen-space reflection, mirror reflection, edge foam, caustics, and VRC Light Volume support.

---

## Features

- Custom lighting model with subsurface scattering.
- Bends light and absorbs colors based on water depth.
- Creates foam near shorelines and objects.
- Supports geometry distortion with procedural Gerstner waves and heightmap displacement.
- Projects fake caustics onto underwater surfaces.
- Screen-space reflections. (PC-only)
- VRC Mirror Reflection support. 
- Supports Bakery lightmaps and VRC Light Volumes.
- Includes an optimized fallback shader pass for Meta Quest and mobile devices.

---

## Installation

Copy the `Clear Water` folder into your project `Packages` directory:
   ```text
   Packages/
   └── Clear Water/
       ├── CW3/
       ├── Assets/
       └── package.json
   ```

---

## Quick Start

1. Create a new material in Unity.
2. Set the shader to `Silent/Clear Water 3`.
3. Add a Plane to your scene, and expand it to fill the area you want your water to cover. 
4. Assign the material to the mesh.
5. Add a Directional Light to your scene to enable the camera depth texture.

---

## Included Shaders

| Shader Name | Used for... | Description |
| :--- | :--- | :--- |
| `Silent/Clear Water 3` | Water surface | Main surface shader for the water plane. |
| `Silent/Clear Water 3 Caustics Projector` | Area above water | Projects bounced caustics onto room walls, ceilings, and poolside floors. |
| `Silent/Clear Water 3 Water Volume Box` | Underwater area | Renders fog, light absorption, and caustics when the camera is inside it. |
| `Silent/Clear Water 3 Extras/Camera Depth to Alpha` | Mirror helper | Helper shader for mirror depth calculation in VRChat. Sandwich against the mirror surface to allow it to pass depth from the mirror render pass. |
| `Silent/Clear Water 2 ...` | Legacy | Old Clear Water 2 shaders. |
| `Silent/Clear Water` | Legacy | Ancient Clear Water shaders. |

---

## Recommended Setup Overview

For complete water setups, use the three main shaders together:

1. **Water Surface (`Silent/Clear Water 3`):** <br> Assign to the water mesh plane for surface waves, reflections, and refraction.
2. **Caustics Projector (`Silent/Clear Water 3 Caustics Projector`):** <br> Assign to a volume mesh around above-water area to project bounced caustics onto nearby walls, ceilings, and floors.
3. **Underwater Volume (`Silent/Clear Water 3 Water Volume Box`):** <br> Assign to a box mesh under the water surface to render fog, absorption, and caustics when the view is underneath the water. 

```text
       [ Ceiling & Room Walls ]   <-- Caustics Projector (renders scattered caustics)
                  ^
                  |
     ~~~~~~~~~~~~~~~~~~~~~~~~~~~~ <-- Water Surface Shader
                  |
                  v
       [ Pool Basin / Seabed ]    <-- Water Volume Box (renders fog and caustics when underwater)
```

---

## Caustics Projector Shader

The **Caustics Projector** shader projects caustics onto above-water objects surrounding the water. It can simulate light bouncing off the water surface. Place it on a cube or tetrahedron mesh and cover the above-water area to use it.

The shader reads the camera depth texture and calculates projection vectors from the Directional Light. It does not require a Unity Projector component.

### How to Use

1. Create a box mesh around the room or area above the water.
2. Assign a material that uses the `Silent/Clear Water 3 Caustics Projector` shader.
3. Assign a caustics texture to **Caustics Texture**.
4. Assign a scrolling normal map to **Distortion Map Texture (optional)** for moving wave distortion.

### Property Reference

| Property Display Name | Type | Description |
| :--- | :--- | :--- |
| **Caustics Texture** | 2D | Caustics pattern texture. |
| **Caustics Speed** | Float | Movement speed of the caustics layers. |
| **Caustics Scale** | Float | Tiling size of the caustics pattern. |
| **Caustics Strength** | Float | Brightness of the projected caustics. |
| **Edge Fade Radius** | Float | Distance of the box boundary edge fade. |
| **Edge Fade Offset** | Float | Start position offset for the boundary fade. |
| **Distortion Map Texture (optional)** | 2D | Normal map texture used to distort the caustics. |
| **Distortion Power** | Vector4 | `XY`: Scroll speed. `ZW`: Distortion strength. |

---

## Water Volume Box Shader

The **Water Volume Box** shader renders underwater fog and caustics when the camera is inside it. It's designed to pair with the main water surface shader, which can only render the fog/caustics when the camera looks through it. 

It is only visible when the camera is inside it. Use this shader for a pool basin or ocean depths where the player's viewpoint can go inside the water. 

### How to Use

1. Create a box mesh surrounding the underwater area.
2. Assign a material that uses the `Silent/Clear Water 3 Water Volume Box` shader.
3. Transfer material settings from the water surface material. 

---

## VRChat Setup

### Depth Buffer 

Depth fog, edge foam, and refraction require a depth buffer.

Make sure your scene contains at least one **Directional Light** with shadows enabled. VRChat enables the depth texture automatically when shadow casting lights or depth features are active. 

If your scene uses baked lighting, you can get the depth buffer without actually lighting objects in the scene. On the directional light, disable all layers except for an unused one, like TransparentFX. 

### Depth-Aware Mirror Setup

VRChat mirrors do not store depth data. You can improve the accuracy of water reflection distortion by adding a Camera Depth to Alpha plane above the mirror. It will capture the depth data and pass it through to the mirror reflection texture. 

To use this feature:

1. Duplicate your water surface mesh.
2. Place the duplicate mesh directly over the water surface. 
3. Assign a material with the `Silent/Clear Water 3 Extras/Camera Depth to Alpha` shader to the duplicate mesh.
4. On your main water material, enable **Mirror Reflections** and **Mirror Contains Depth**.


### VRC Light Volumes

Clear Water 3 supports VRC Light Volumes. Using them, CW3 can receive indirect lighting and light directionality without being lightmapped. It applies this lighting to the water surface, wave crests, and underwater fog.

### Meta Quest and Mobile Support

Clear Water 3 includes an optimized fallback shader pass for Meta Quest and mobile GPUs.

Mobile GPUs do not support screen-space depth reconstruction or GrabPass refraction efficiently. When rendered on Meta Quest or Android:
- The shader disables GrabPass refraction and screen-space reflections.
- It uses basic surface rendering to maintain high frame rates.
- It keeps procedural waves, normal mapping, and basic lighting active.

---

## License and Credits

- **Author:** [Silent](https://github.com/s-ilent)
- **License:** GNU General Public License v2.0 or later ([GPL2](https://opensource.org/license/gpl-2.0)) OR Creative Commons Attribution-ShareAlike 4.0 International ([CC BY-SA 4.0](http://creativecommons.org/licenses/by-sa/4.0/))

When using this shader in a VRchat world, please provide attribution. 

### Third-Party Credits

- **`tanoise`**: Noise generation library by [cnlohr](https://github.com/cnlohr/shadertrixx) (MIT License).
- **VRC Light Volumes:** Developed by RED_SIM.
- **Oblique Projection and Frustum Correction:** Dj Lukis.LT ([VRCUnityStuffs](https://github.com/lukis101/VRCUnityStuffs)).
- **Mirror Depth Technique:** Written by Lyuma.
- **Refraction Model:** Adapted from Google Filament.
- **Sample Textures:** Adapted from UnionBytes and Achim Menzel.