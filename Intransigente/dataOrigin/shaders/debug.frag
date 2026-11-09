#version 400 core
#extension GL_EXT_gpu_shader4 : enable
//invalid shader

out vec4 FragColor;

uniform vec3 objectColor; //The color of the object.
in vec3 FragPos; //The fragment position.

void main()
{
        ivec2 size = ivec2(20,20);
        float total = floor(FragPos.x * float(size.x)) +
        floor(FragPos.y * float(size.y));
        bool isEven = mod(total, 2.0) == 0.0;
        vec4 col1 = vec4(objectColor, 1.0);
        if(!isEven)
        discard;
        FragColor = col1;
}