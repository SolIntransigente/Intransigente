#version 330
out vec4 FragColor;
in vec2 TexCoords;
uniform sampler2D screenTexture;
uniform sampler2D dataTexture;
/*
const int colortex0Format = RGBA32F;
const int colortex4Format = R32F;
const int colortex5Format = R32F;
const int colortex6Format = R32F;
const int colortex7Format = R32F;
*/

uniform vec2 screenDimensions;

// ccolor space transformation
// 0 - RGB
// 1 - YCbCr
// 2 - CMYK
#define CST 0 // [ 0 1 2 ]

// color post-quantization
// 0 - off
// 1 - 256 levels
// 2 - 64 levels
// 3 - 32 levels
// 4 - 24 levels
// 5 - 16 levels
// 6 - 12 levels
// 7 - 8 levels
// 8 - 6 levels
// 9 - 5 levels
// 10 - 4 level
// 11 - 3 levels
// 12 - 2 levels
#define CRQ1 3 // [ 0 1 2 3 4 5 6 7 8 9 10 11 12 ]
#define CRQ2 3 // [ 0 1 2 3 4 5 6 7 8 9 10 11 12 ]
#define CRQ3 0 // [ 0 1 2 3 4 5 6 7 8 9 10 11 12 ]
#define CRQ4 6 // [ 0 1 2 3 4 5 6 7 8 9 10 11 12 ]
#if !(CRQ1 == 0 && CRQ2 == 0 && CRQ3 == 0 && CRQ4 == 0)
	const vec4 crqf = vec4(
		#if CRQ1 == 0
			1024.0
		#elif CRQ1 == 1
			256.0
		#elif CRQ1 == 2
			64.0
		#elif CRQ1 == 3
			32.0
		#elif CRQ1 == 4
			24.0
		#elif CRQ1 == 5
			16.0
		#elif CRQ1 == 6
			12.0
		#elif CRQ1 == 7
			8.0
		#elif CRQ1 == 8
			6.0
		#elif CRQ1 == 9
			5.0
		#elif CRQ1 == 10
			4.0
		#elif CRQ1 == 11
			3.0
		#elif CRQ1 == 12
			2.0
		#endif
		,
		#if CRQ2 == 0
			1024.0
		#elif CRQ2 == 1
			256.0
		#elif CRQ2 == 2
			64.0
		#elif CRQ2 == 3
			32.0
		#elif CRQ2 == 4
			24.0
		#elif CRQ2 == 5
			16.0
		#elif CRQ2 == 6
			12.0
		#elif CRQ2 == 7
			8.0
		#elif CRQ2 == 8
			6.0
		#elif CRQ2 == 9
			5.0
		#elif CRQ2 == 10
			4.0
		#elif CRQ2 == 11
			3.0
		#elif CRQ2 == 12
			2.0
		#endif
		,
		#if CRQ3 == 0
			1024.0
		#elif CRQ3 == 1
			256.0
		#elif CRQ3 == 2
			64.0
		#elif CRQ3 == 3
			32.0
		#elif CRQ3 == 4
			24.0
		#elif CRQ3 == 5
			16.0
		#elif CRQ3 == 6
			12.0
		#elif CRQ3 == 7
			8.0
		#elif CRQ3 == 8
			6.0
		#elif CRQ3 == 9
			5.0
		#elif CRQ3 == 10
			4.0
		#elif CRQ3 == 11
			3.0
		#elif CRQ3 == 12
			2.0
		#endif
		,
		#if CRQ4 == 0
			1024.0
		#elif CRQ4 == 1
			256.0
		#elif CRQ4 == 2
			64.0
		#elif CRQ4 == 3
			32.0
		#elif CRQ4 == 4
			24.0
		#elif CRQ4 == 5
			16.0
		#elif CRQ4 == 6
			12.0
		#elif CRQ4 == 7
			8.0
		#elif CRQ4 == 8
			6.0
		#elif CRQ4 == 9
			5.0
		#elif CRQ4 == 10
			4.0
		#elif CRQ4 == 11
			3.0
		#elif CRQ4 == 12
			2.0
		#endif
	);
#endif
const vec4 crqo = sign(vec4(CRQ1, CRQ2, CRQ3, CRQ4));

// discrete cosine transform stuff
vec4 dcta;
vec2 dctci;
vec2 dctcj;
vec2 dctcci;
vec2 dctci1i;
const vec2 dctcc1i = vec2(0.0, 0.125); // vec to the next value of cosine  aka width of a pixel in the map
int dctii;

// local variables
vec4 col1;
float val1;

void main() {
	dctcj = floor(TexCoords * screenDimensions); // <0;lb-px;dims/spx>  quantize to full super-pixel beginning (lobo)
	dctci = floor(dctcj / 8.0) * 8.0;
	dctcci = vec2(0.0625 + 0.125 * (dctcj.y - dctci.y), 0.0625); // setup initial coords for getting cosine values for y
	dctci = vec2(dctcj.x + 0.5, dctci.y + 0.5); // offset to the center of the first pixel of the dct square (mid)
	dctci = dctci / screenDimensions; // <0;mid;1> transform back to og texture coords (mid)
	dctci1i = vec2(0.0, 1.0 / screenDimensions.y); // vec to the next pixel in x dir  aka the width of a pixel
	// do tha DCT on x axis
	dcta = vec4(0.0);
	for(dctii=0;dctii<8;dctii++) {
		dcta += texture2D(screenTexture, dctci) * texture2D(dataTexture, dctcci).rrrr;
		dctci += dctci1i;
		dctcci += dctcc1i;
	}
	col1 = vec4(0.5) + dcta * 0.5;
	// apply color post-quantization if necessary
	#if (CRQ1 == 0 && CRQ2 == 0 && CRQ3 == 0 && CRQ4 == 0)
	#else
		col1 = mix( col1, floor( vec4(0.5) + col1*crqf ) / crqf, crqo );
	#endif
	// transform the color if requested
	#if CST == 0
	#elif CST == 1
		col1 = vec4( 0.0 , -0.5 , -0.5 , 0.0 ) + col1;
		col1 =
				mat4(
						1.0   ,  1.0      , 1.0   , 0.0 ,
						0.0   , -0.344136 , 1.772 , 0.0 ,
						1.402 , -0.714136 , 0.0   , 0.0 ,
						0.0   ,  0.0      , 0.0   , 1.0
				)
				*
				col1;
	#elif CST == 2
		col1 = vec4( 1.0 ) - col1;
		val1 = col1.a;
		col1 = col1 * vec4( val1 );
		col1.a = 1.0;
	#endif
	vec4 texColor = texture2D(screenTexture, TexCoords);
	float fraction = 1;
	FragColor = texColor + 0.1 * vec4(floor(col1.r * fraction), floor(col1.g* fraction), floor(col1.b* fraction), floor(col1.a* fraction));
	FragColor = FragColor * 0.75;
}