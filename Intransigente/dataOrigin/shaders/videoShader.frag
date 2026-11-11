#version 330
out vec4 FragColor;
in vec2 TexCoords;
uniform sampler2D videoTexture;

void main() {
	vec4 texColor = texture2D(videoTexture, TexCoords);
	FragColor = vec4(texColor.z, texColor.y, texColor.z, 1);
}