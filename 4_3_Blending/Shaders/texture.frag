#version 320 es

precision highp float;

in vec2 TexCoords;

out vec4 FragColor;

uniform sampler2D tex;

void main() {
    FragColor = vec4(texture(tex, TexCoords));
}