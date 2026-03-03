local vidPlayerParallax = {}

vidPlayerParallax.name = "VidPlayer/VidPlayerParallax"

vidPlayerParallax.canBackground = true
vidPlayerParallax.canForeground = true
vidPlayerParallax.defaultData = {
    video = "",
    muted = true,
    --keepAspectRatio = true,
    volumeMult = 1,
    globalAlpha = 1,
    --centered = false,
    hires = false,
    chromaKey = "",
    chromaKeyBaseThr = 0,
    chromaKeyAlphaCorr = 0.1,
    chromaKeySpill = 0.1,
    unpausable = false,
    -- Parallax stuff
    blendmode = "alphablend",
    instantIn = false,
    instantOut = false,
    fadeIn = false,
    fadex = "",
    fadey = "",
    flipx = false,
    flipy = false,
    loopx = true,
    loopy = true,
    --    color = "FFFFFF", ????
    scrollx = 1.0,
    scrolly = 1.0,
    speedx = 0.0,
    speedy = 0.0,
    x = 0.0,
    y = 0.0,
    scale = 1.0,
}

vidPlayerParallax.fieldInformation = {
    volumeMult = {
        fieldType = "number",
        minimumValue = 0
    },
    globalAlpha = {
        fieldType = "number",
        minimumValue = 0,
        maximumValue = 1,
    },
    chromaKey = {
        fieldType = "color",
        allowEmpty = true,
    },
    chromaKeyBaseThr = {
        fieldType = "number",
    },
    chromaKeyAlphaCorr = {
        fieldType = "number",
        minimumValue = 0.000001, -- epsilon
    },
    chromaKeySpill = {
        fieldType = "number",
        minimumValue = 0.000001, -- epsilon
    },
    blendmode = {
        options = {
            "additive",
            "alphablend"
        },
        editable = false
    }, 
}

return vidPlayerParallax
