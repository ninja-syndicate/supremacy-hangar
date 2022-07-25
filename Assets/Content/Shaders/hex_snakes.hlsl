void hex_snakes_float(float snake_timeScale, float2 snake_uvScale, float hex_uvScale,
				  float3 fillColour, float3 borderColour, float3 snakeColour, 
				  float2 u, float snake_threshold, float time, float2 snake_uvOffset,
				  float snake_thickness, float numSnakes, float2 size, out float3 outColour) {
float2 iResolution = float2(4096, 4096);

const float2 s = float2(1.7320508, 1);
    
  // scale uvs
  u = ((u*size.xy)- size.xy*.5)/size.y;
    float2 p = u * hex_uvScale;
	
  // get hex info
    float4 hC = floor(float4(p, p - float2(1, .5))/s.xyxy) + .5;
    float4 hP = float4(p - hC.xy*s, p - (hC.zw + .5)*s);
    float4 h = dot(hP.xy, hP.xy) < dot(hP.zw, hP.zw) 
        ? float4(hP.xy, hC.xy) 
        : float4(hP.zw, hC.zw + .5);
    
  // distance to edge of hex
    float eDist = max(dot(abs(h.xy), s*.5), abs(h.xy).y);

  // scale snake uvs
    float2 snakeUv = snake_uvOffset + (h.zw/iResolution.xy) * snake_uvScale;
    
  // calculate snake
  float4 o = float4(0,0,0,0);
    for (float dt=0.; dt<1.; dt+= .005) {
        
        float t = time*snake_timeScale + dt;
        
    
    
    for(int snakeNum = 0; snakeNum < numSnakes; snakeNum++) {
      t=t + snakeNum * 5432;
    float n = 1.;
    float d = 4.;
    float x=round(t*d/n)*n/d;
      float2 p2 = float2( 0.03*cos(x*19.)+.5*sin(-2.7*x), .08*sin(19.2*x)+.5*cos(3.2*x) ) /1.5*dt*0.5;            
          
    float snakePLength = length(p2-snakeUv);
        o += 5.*smoothstep(.04*float4(1,1,1,0), float4(0,0,0,0), float4(snakePLength,snakePLength,snakePLength,snakePLength));
    }
    }
    float snakeRaw = o.r;
    
  // make snake mask
  float snakeV = snakeRaw;//smoothstep(0.4, 5., snakeRaw);
  //if(snakeV<.2)snakeV=0.;
  snakeV = abs(snakeV - .2);
  // fill based on snake mask
  float3 fill = lerp(fillColour, snakeColour, snakeV);
  
  // create borders
    float3 col = lerp(fill, borderColour, smoothstep(0., .03, eDist - .5 + .04));  
  
  outColour=col;
}