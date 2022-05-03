shader_type canvas_item;

uniform sampler2D chars;
uniform sampler2D noise_tex; 
uniform vec4 matrixColor  : hint_color = vec4(.1,1,.35, 1);

vec2 rand(vec2 uv) {
    return floor(abs(mod(cos(
        uv * 652.6345 + uv.yx * 534.375 +
        TIME * 0.0000005 * dot(uv, vec2(0.364, 0.934))),
     0.001)) * 16000.0);
}

float text(vec2 fragCoord, vec2 tex)
{
	vec2 uv = fragCoord/tex;
	vec2 cells = vec2(64.0,30.0);
    
    vec2 pix = mod(uv, 1.0 / cells);
    vec2 cell = (uv - pix) * cells;
    pix *= cells * vec2(0.8, 1.0) + vec2(0.1, 0.0);
	
	vec2 charUv = (rand(cell) + pix) / 16.0;
    float c = texture(chars, charUv).x;
	return c;
}

vec3 rain(vec2 fragCoord, vec2 res)
{
	fragCoord.x -= mod(fragCoord.x, 16.);
    //fragCoord.y -= mod(fragCoord.y, 16.);
	
    float offset=sin(fragCoord.x*15.);
    float speed=cos(fragCoord.x*3.)*.3+.7;
   
    float y = fract(fragCoord.y/res.y + TIME * speed + offset);
    return matrixColor.rgb / (y*20.);
}

void fragment() {
	vec2 res = 	1.0 / SCREEN_PIXEL_SIZE;
	vec2 tex = 1.0 / TEXTURE_PIXEL_SIZE;
	
	COLOR = vec4(text(FRAGCOORD.xy, res)*rain(FRAGCOORD.xy, res),1.0);
}