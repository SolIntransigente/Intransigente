#version 330
//invalid shader
out vec4 FragColor;
in vec2 TexCoords;
uniform sampler2D screenTexture;

void main() {
	vec4 texColor = texture2D(screenTexture, TexCoords);
	FragColor = texColor;
}