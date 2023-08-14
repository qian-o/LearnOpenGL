#version 320 es

precision highp float;

struct Material {
   sampler2D diffuse;
};

in vec2 TexCoords;

out vec4 FragColor;

uniform Material material;

void main() {
   FragColor = vec4(texture(material.diffuse, TexCoords));
}