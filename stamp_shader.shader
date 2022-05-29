shader_type canvas_item;
render_mode blend_mix;

uniform vec2 stampPos;

uniform sampler2D stampMark;
uniform vec2 stampMarkSize;

void fragment () {
	
	vec2 mainSize = 1.0 / TEXTURE_PIXEL_SIZE;
	vec2 diffScale = mainSize / stampMarkSize;
	
	if (texture(TEXTURE, UV).a > 0.0)
		COLOR = texture(stampMark, UV * diffScale - stampPos / stampMarkSize);
	else
		COLOR = vec4(0, 0, 0, 0);
	
}