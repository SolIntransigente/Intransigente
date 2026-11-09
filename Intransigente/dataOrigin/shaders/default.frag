#version 330 core
out vec4 FragColor;

uniform lowp sampler2D mainTex; //exposed

uniform lowp vec3 objectColor; //exposed
uniform lowp vec3 ambientColor;

in lowp vec2 TexCoord;
in lowp vec3 FragPos;
in lowp vec3 Normal;

struct DirLight {
    lowp vec3 direction;
  
    lowp vec3 color;
    lowp float intensity;
};  

struct PointLight {    
    lowp vec3 position;
    lowp float radius;

    lowp vec3 color;
    lowp float intensity;
};  
struct SpotLight {
    lowp vec3 position;
    lowp float radius;
    lowp vec3 direction;
    lowp float cutOff;
    lowp float outerCutOff;

    lowp vec3 color;
    lowp float intensity;
};
#define MAX_LIGHT_COUNT 8
uniform DirLight dirLight;

uniform PointLight pointLights[MAX_LIGHT_COUNT];
uniform lowp float pointLightCount;

uniform SpotLight spotLights[MAX_LIGHT_COUNT];
uniform lowp float spotLightCount;

vec3 CalcDirLight(DirLight light, vec3 normal);
vec3 CalcPointLight(PointLight light);
vec3 CalcSpotLight(SpotLight light);

void main()
{
    float ambientStrength = 1;
    vec3 ambient = ambientStrength * ambientColor;
    vec3 norm = normalize(Normal);

    vec3 lightColor = CalcDirLight(dirLight, norm);
    for(int i = 0; i < pointLightCount; i++)
        lightColor += CalcPointLight(pointLights[i]);
    for(int s = 0; s < spotLightCount; s++)
        lightColor += CalcSpotLight(spotLights[s]);
    vec3 texColor = texture(mainTex, TexCoord).rgb;
    FragColor = vec4(ambient * objectColor * texColor + lightColor * texColor, 1.0f);
}


vec3 CalcDirLight(DirLight light, vec3 normal)
{
    vec3 lightDir = normalize(-light.direction);
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 lColor = light.color * diff * light.intensity;
    return lColor;
}
vec3 CalcPointLight(PointLight light)
{
    float distance = length(light.position - FragPos);
    if (distance < 1 + light.radius)
    {
        float attenuation = 1 / (1 + light.radius * distance + 0.092 * (distance * distance));
        vec3 lColor = light.color * light.intensity * attenuation;
        return lColor;
    }
    else
        return vec3(0,0,0);
}
vec3 CalcSpotLight(SpotLight light)
{
    vec3 lightDir = normalize(light.position - FragPos);
    
    float theta = dot(lightDir, normalize(-light.direction)); 
    float epsilon = light.cutOff - light.outerCutOff;
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
    
    float distance    = length(light.position - FragPos);
    float attenuation = 1.0 / (1 + light.radius * distance + 0.092 * (distance * distance));

    vec3 lColor = light.color * light.intensity * attenuation * intensity;
    return lColor;
}