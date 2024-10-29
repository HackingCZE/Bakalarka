UI Blur - URP > Material > ui_blur_mat = in URP settings you need to set "Opaque Texture" to TRUE (works for UI elements and gameobjects in scene)

UI Blur - URP > Material > ui_blur_mat_transparent = you need to create a second camera in the scene from the MainCamera that has the "UI" option in "Culling Mask" turned off (works just for gameobjects in scene)