#version 320 es

precision highp float;

in vec2 TexCoords;

out vec4 FragColor;

uniform sampler2D diffuse;

void main() {
    vec4 texColor = vec4(texture(diffuse, TexCoords));
    if(texColor.a < 0.1) {
        discard;
    }
    FragColor = texColor;
}