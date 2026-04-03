/* ═══════════════════════════════════════════════
   Aurora Background Effect
   Uses OGL library for WebGL rendering as ES Module
   ═══════════════════════════════════════════════ */

import { Renderer, Program, Mesh, Color, Triangle } from 'https://unpkg.com/ogl';

const VERT = `#version 300 es
in vec2 position;
void main() {
    gl_Position = vec4(position, 0.0, 1.0);
}
`;

const FRAG = `#version 300 es
precision highp float;

uniform float uTime;
uniform float uAmplitude;
uniform vec3 uColorStops[3];
uniform vec2 uResolution;
uniform float uBlend;

out vec4 fragColor;

vec3 permute(vec3 x) {
    return mod(((x * 34.0) + 1.0) * x, 289.0);
}

float snoise(vec2 v){
    const vec4 C = vec4(0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439);
    vec2 i  = floor(v + dot(v, C.yy));
    vec2 x0 = v - i + dot(i, C.xx);
    vec2 i1 = (x0.x > x0.y) ? vec2(1.0, 0.0) : vec2(0.0, 1.0);
    vec4 x12 = x0.xyxy + C.xxzz;
    x12.xy -= i1;
    i = mod(i, 289.0);
    vec3 p = permute(permute(i.y + vec3(0.0, i1.y, 1.0)) + i.x + vec3(0.0, i1.x, 1.0));
    vec3 m = max(0.5 - vec3(dot(x0, x0), dot(x12.xy, x12.xy), dot(x12.zw, x12.zw)), 0.0);
    m = m*m;
    m = m*m;
    vec3 x = 2.0 * fract(p * C.www) - 1.0;
    vec3 h = abs(x) - 0.5;
    vec3 ox = floor(x + 0.5);
    vec3 a0 = x - ox;
    m *= 1.79284291400159 - 0.85373472095314 * (a0*a0 + h*h);
    vec3 g;
    g.x = a0.x * x0.x + h.x * x0.y;
    g.yz = a0.yz * x12.xz + h.yz * x12.yw;
    return 130.0 * dot(m, g);
}

struct ColorStop {
    vec3 color;
    float position;
};

void main() {
    vec2 uv = gl_FragCoord.xy / uResolution;
    ColorStop colors[3];
    colors[0] = ColorStop(uColorStops[0], 0.0);
    colors[1] = ColorStop(uColorStops[1], 0.5);
    colors[2] = ColorStop(uColorStops[2], 1.0);

    vec3 rampColor = colors[0].color;
    float factor = uv.y;
    for (int i = 0; i < 2; i++) {
        ColorStop current = colors[i];
        ColorStop next = colors[i+1];
        if (factor >= current.position && factor <= next.position) {
            float t = (factor - current.position) / (next.position - current.position);
            rampColor = mix(current.color, next.color, t);
        }
    }

    float height = snoise(vec2(uv.x * 2.0 + uTime * 0.1, uTime * 0.25)) * 0.5 * uAmplitude;
    height = exp(height);
    height = (uv.y * 2.0 - height + 0.2);
    float intensity = 0.6 * height;
    float midPoint = 0.20;
    float auroraAlpha = smoothstep(midPoint - uBlend * 0.5, midPoint + uBlend * 0.5, intensity);
    vec3 auroraColor = intensity * rampColor;
    fragColor = vec4(auroraColor * auroraAlpha, auroraAlpha);
}
`;

function initAurora() {
    console.log("Initializing Aurora Background via ES Module...");
    const ctn = document.getElementById('aurora-bg');
    if (!ctn) {
        console.error("Aurora container not found: #aurora-bg");
        return;
    }

    console.log("Setting up renderer...");
    
    // In React snippet: [r, g, b] array per color instead of Color instances
    // We convert hex to Color, then to float arrays [r,g,b]
    const hexStops = ['#252322', '#842e54', '#6f626b'];
    const flatColors = hexStops.map(hex => {
        const c = new Color(hex);
        return [c.r, c.g, c.b];
    });

    const amplitude = 1.0;
    const blend = 0.44;
    const speed = 1.0;

    const renderer = new Renderer({ alpha: true, premultipliedAlpha: true, antialias: true });
    const gl = renderer.gl;
    gl.clearColor(0, 0, 0, 0);
    gl.enable(gl.BLEND);
    gl.blendFunc(gl.ONE, gl.ONE_MINUS_SRC_ALPHA);
    ctn.appendChild(gl.canvas);

    const geometry = new Triangle(gl);
    const program = new Program(gl, {
        vertex: VERT,
        fragment: FRAG,
        uniforms: {
            uTime: { value: 0 },
            uAmplitude: { value: amplitude },
            uColorStops: { value: flatColors }, // Array of float arrays
            uResolution: { value: [0, 0] },
            uBlend: { value: blend }
        }
    });

    const mesh = new Mesh(gl, { geometry, program });
    let animateId = 0;

    const resize = () => {
        const width = ctn.offsetWidth;
        const height = ctn.offsetHeight;
        renderer.setSize(width, height);
        program.uniforms.uResolution.value = [width, height];
    };
    window.addEventListener('resize', resize);
    resize();

    const update = t => {
        animateId = requestAnimationFrame(update);
        program.uniforms.uTime.value = t * 0.0001 * speed;
        renderer.render({ scene: mesh });
    };
    animateId = requestAnimationFrame(update);

    window.addEventListener('beforeunload', () => {
        cancelAnimationFrame(animateId);
        window.removeEventListener('resize', resize);
        gl.getExtension('WEBGL_lose_context')?.loseContext();
    });
}

// Auto-initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initAurora);
} else {
    initAurora();
}
