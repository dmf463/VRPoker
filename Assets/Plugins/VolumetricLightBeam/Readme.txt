*******************
** Sample Scenes **
*******************
- demoScene: showcase multiple features
- demoStressTest: features 400 dynamic Volumetric Spotlights


*******************
** Configuration **
*******************
In your project file, look for a file named Config.asset under the folder Plugins/VolumetricLightBeam/Resources. In the inspector, you can configure the following properties:
- Geometry:
  - Layer: controls on which layer the beam geometry meshes will be created in.
- Global 3D Noise:
  - Scale: Global 3D Noise texture scaling. Higher scale make the noise more visible, but potentially less realistic.
  - Velocity: Global World Space direction and speed of the noise scrolling, simulating the fog/smoke movement.
- 3D Noise Texture Data:
  - Binary file: Binary file holding the 3D Noise texture data (a 3D array). Must be exactly Size * Size * Size bytes long.
  - Data dimension: Size (of one dimension) of the 3D Noise data. Must be power of 2. So if the binary file holds a 32x32x32 texture, this value must be 32.
- Assets:
  - Shader: Main shader


****************
** Properties **
****************
Basic:
- Spot Angle: Define the angle (in degrees) at the base of the beam's cone. [if attached to a Spotlight, check the toggle to get this value from it]
- Color: Use the color picker to set the color of the beam (takes account of the alpha value). [if attached to a Spotlight, check the toggle to get this value from it]
- Side Thickness: Thickness of the beam when looking at it from the side. 1 = the beam is fully visible (no difference between the center and the edges), but produces hard edges. Lower values produce softer transition at beam edges. If you set the lowest possible value and want to make the beam even thinner, just lower the 'Spot Angle' and/or the 'Truncated Radius' properties.
- Track Changes During Playtime: If true, the light beam will keep track of the changes of its own properties and the spotlight attached to it (if any) during playtime. This would allow you to modify the light beam in realtime from Script, Animator and/or Timeline. Enabling this feature is at very minor performance cost. So keep it disabled if you don't plan to modify this light beam during playtime.

Inside:
- Alpha: Modulate the opacity of the inside geometry of the beam. Is multiplied to Color's alpha.
- Glare Frontal: Boost intensity factor when looking at the beam from the inside directly at the source.

Outside:
- Alpha: Modulate the opacity of the outside geometry of the beam. Is multiplied to Color's alpha.
- Glare Behind: Boost intensity factor when looking at the beam from behind.

Fading Distances:
- Start: Distance from the light source (in units) the beam will start to fade out.
- End: Distance from the light source (in units) the beam is entirely faded out (alpha = 0, no more cone mesh).  [if attached to a Spotlight, check the toggle to get this value from it]

3D Noise:
- Enabled: Enable 3D Noise effect.
- Intensity: Higher intensity means the noise contribution is stronger and more visible.
- Scale: 3D Noise texture scaling. Higher scale make the noise more visible, but potentially less realistic. [if the toggle 'Use Global' is checked, it will use the Scale property set in Config.asset]
- Velocity: World Space direction and speed of the noise scrolling, simulating the fog/smoke movement. [if the toggle 'Use Global' is checked, it will use the Velocity property set in Config.asset]

Soft Intersections Blending Distances:
- Camera: Distance from the camera the beam will fade. 0 = hard intersection. Higher values produce soft intersection when the camera is near the cone triangles.
- Opaque Geometry: Distance from the world geometry the beam will fade. 0 = hard intersection Higher values produce soft intersection when the beam intersects other opaque geometry.

Cone Geometry:
- Truncated Radius: Radius (in units) at the beam's source (the top of the cone). 0 will generate perfect cone geometry. Higher values will generate truncated cones.
- Sides: Number of Sides of the cone. Higher values give better looking results, but require more memory and graphic performance.
- Cap Geom: Show the cap of the cone or not (only visible from the inside).


********************************************
** Soft intersection with opaque geometry **
********************************************
To support the "Soft intersection with opaque geometry" feature, your camera must use "DepthTextureMode.Depth". By default, the rendering camera will be forced to the proper DepthTextureMode value, just before rendering a LightBeam.
If you are sure that you camera is using the "DepthTextureMode.Depth" mode, you can disable this behavior for minor performance gain. To do so, comment the 1st line in BeamGeometry.cs:
// #define FORCE_CURRENT_CAMERA_DEPTH_TEXTURE_MODE


***********************
** Platform Specific **
***********************
- The Volumetric Light Beam shader is a 2 pass shader using a Shader Model as low as 3.0
- 3D Noise feature is not supported on mobile (because mobile platforms cannot handle 3D textures)
