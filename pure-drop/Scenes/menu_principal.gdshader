shader_type canvas_item;

// Velocidade que você ajusta no Inspetor
uniform vec2 velocidade = vec2(0.05, 0.05); 

void fragment() {
    // fract faz com que o número volte para 0 quando chega em 1
    // Isso cria o loop infinito perfeito
    vec2 uv_infinita = fract(UV + (TIME * velocidade));
    
    COLOR = texture(TEXTURE, uv_infinita);
}