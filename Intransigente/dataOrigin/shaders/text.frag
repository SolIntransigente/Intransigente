#version 330 core
//invalid shader
out vec4 FragColor;

uniform vec4 textColorMultiplier;
uniform vec2 texCoordsShift;
uniform vec2 texCoordsScale;
in vec2 TexCoords;

uniform sampler2D _mainTex;

void main()
{ 
    vec4 texColor = texture(_mainTex, (TexCoords * texCoordsScale) + texCoordsShift);
    if(texColor.a < 0.1)
        discard;
    FragColor = texColor * textColorMultiplier;
}