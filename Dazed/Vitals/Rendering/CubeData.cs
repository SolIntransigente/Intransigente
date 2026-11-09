namespace Dazed.Vitals.Rendering;

public static class CubeData
{
    public static readonly float[] VerticesAndTexCoord = {
        -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,  // A 0
        0.5f, -0.5f, -0.5f,  1.0f, 0.0f,  // B 1
        0.5f,  0.5f, -0.5f,  1.0f, 1.0f,  // C 2
        -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,  // D 3
        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,  // E 4
        0.5f, -0.5f,  0.5f,  1.0f, 0.0f,   // F 5
        0.5f,  0.5f,  0.5f,  1.0f, 1.0f,   // G 6
        -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,   // H 7
 
        -0.5f,  0.5f, -0.5f,  0.0f, 0.0f,  // D 8
        -0.5f, -0.5f, -0.5f,  1.0f, 0.0f,  // A 9
        -0.5f, -0.5f,  0.5f,  1.0f, 1.0f,  // E 10
        -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,  // H 11
        0.5f, -0.5f, -0.5f,  0.0f, 0.0f,   // B 12
        0.5f,  0.5f, -0.5f,  1.0f, 0.0f,   // C 13
        0.5f,  0.5f,  0.5f,  1.0f, 1.0f,   // G 14
        0.5f, -0.5f,  0.5f,  0.0f, 1.0f,   // F 15
 
        -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,  // A 16
        0.5f, -0.5f, -0.5f,  1.0f, 0.0f,   // B 17
        0.5f, -0.5f,  0.5f,  1.0f, 1.0f,   // F 18
        -0.5f, -0.5f,  0.5f,  0.0f, 1.0f,  // E 19
        0.5f,  0.5f, -0.5f,   0.0f, 0.0f,  // C 20
        -0.5f,  0.5f, -0.5f,  1.0f, 0.0f,  // D 21
        -0.5f,  0.5f,  0.5f,  1.0f, 1.0f,  // H 22
        0.5f,  0.5f,  0.5f,   0.0f, 1.0f,  // G 23
    };
    public static readonly int VerticesAndTexCoordSize = VerticesAndTexCoord.Length * sizeof(float);

    public static readonly float[] Vertices = [
        -0.5f, -0.5f, -0.5f,  // A 0
        0.5f, -0.5f, -0.5f,  // B 1
        0.5f,  0.5f, -0.5f,  // C 2
        -0.5f,  0.5f, -0.5f,  // D 3
        -0.5f, -0.5f,  0.5f,  // E 4
        0.5f, -0.5f,  0.5f,   // F 5
        0.5f,  0.5f,  0.5f,   // G 6
        -0.5f,  0.5f,  0.5f,   // H 7
 
        -0.5f,  0.5f, -0.5f,  // D 8
        -0.5f, -0.5f, -0.5f,  // A 9
        -0.5f, -0.5f,  0.5f,  // E 10
        -0.5f,  0.5f,  0.5f,  // H 11
        0.5f, -0.5f, -0.5f,   // B 12
        0.5f,  0.5f, -0.5f,   // C 13
        0.5f,  0.5f,  0.5f,   // G 14
        0.5f, -0.5f,  0.5f,   // F 15
 
        -0.5f, -0.5f, -0.5f,  // A 16
        0.5f, -0.5f, -0.5f,   // B 17
        0.5f, -0.5f,  0.5f,   // F 18
        -0.5f, -0.5f,  0.5f,  // E 19
        0.5f,  0.5f, -0.5f,  // C 20
        -0.5f,  0.5f, -0.5f,  // D 21
        -0.5f,  0.5f,  0.5f,  // H 22
        0.5f,  0.5f,  0.5f,  // G 23
    ];

    public static readonly int VerticesSize = Vertices.Length * sizeof(float);

    // index data
    public static readonly uint[] Indices = [
        // front and back
        0, 3, 2,
        2, 1, 0,
        4, 5, 6,
        6, 7 ,4,
        // left and right
        11, 8, 9,
        9, 10, 11,
        12, 13, 14,
        14, 15, 12,
        // bottom and top
        16, 17, 18,
        18, 19, 16,
        20, 21, 22,
        22, 23, 20
    ];
    public static readonly int IndicesSize = Indices.Length * sizeof(uint);
}