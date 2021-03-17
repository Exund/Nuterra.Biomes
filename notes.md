﻿# Generation
BiomeMap handles biome map (splat map) generation and then heightmap generation.
BiomeGroupDatabase is used to get biomes.


# MapGenerator 
## GenerationContext
### Variables
```
Vector2 origin_offset
float total_weight
Matrix2x2[] scale_inverted_rotation_matrix
Vector2[] pre_trans_off_vector
```

### Setup
```
origin_offset = Vector2(random_int & 65535, random_int >> 16)
local rotation_adjust = random_between(0, 2 * PI)
total_weight = 0

for each layer in generator.m_Layers
    local l_cos = cos(layer.rotation / 2 + rotation_adjust)
    local l_sin = sin(layer.rotation / 2 + rotation_adjust)

    if(layer.scaleX == 0 && layer.scaleY == 0)
        layer.scaleY = 2^(layer.scale + generator.m_ScaleAll)
        layer.scaleX = layer.scaleY

    local invert_scaleX = 1 / layer.scaleX
    local invert_scaleY = 1 / layer.scaleY
    if(generator.m_UseLegacy)
        invert_scaleX = 2^(-layer.scale - generator.m_ScaleAll)
        invert_scaleY = invert_scaleX

    scale_inverted_rotation_matrix[layer_index] =
        | (l_cos * invert_scaleX) (-l_sin * invert_scaleX) |
        | (l_sin * invert_scaleY) (l_cos * invert_scaleY)  |

    local layer_offset = layer.offset * 10
    pre_trans_off_vector[layer_index] = scale_inverted_rotation_matrix[layer_index] * layer_offset // Matrix multiplication
    
    total_weight += layer.weight

total_weight = 1 / total_weight
```
*The ***generator*** variable is the generator currently being processed*

## Algorithms
### Point generation algo
```
function GeneratePoint(GenerationContext context, Vector2 input_position)
    local generated_point = 0
    local position = input_position + context.origin_offset

    for each layer in generator.m_Layers
        local layer_value = 0

        if(layer.applyOperation.code == "Modify")
            layer_value = generated_point
        else
            let rotated_position = context.scale_inverted_rotation_matrix[layer_index] * position // Matrix multiplication
            rotated_position += context.pre_trans_off_vector[layer_index]
            layer_value = layer.function(rotated_position.x, rotated_position.y)

        for each operation in layer.operations
            if(operation.code == "Store")
                context.variable_buffer[operation.index] = layer_value
            else
                local param = 0;
                if(operation.buffered)
                    param = context.variable_buffer[operation.index];
                else
                    param = operation.param

                layer_value = operation.eval(layer_value, param)

        generated_point = layer.applyOperation.eval(generated_point, layer_value)

    clamp generated_point between -1 and 1
    return generated_point
```
*The ***generator*** variable is the generator currently being processed*

### Legacy point generation algo
```
function GeneratePointLegacy(GenerationContext context, Vector2 input_position)
    local generated_point = 0
    local position = input_position + context.origin_offset

    for each layer in generator.m_Layers
        let rotated_position = context.scale_inverted_rotation_matrix[layer_index] * position
        rotated_position += context.pre_trans_off_vector[layer_index]

        local layer_value = layer.function() * layer.amplitude + layer.bias
        layer_value *= layer.weight
        if layer.invert
            layer_value *= -1

        clamp layer_value between -1 and 1
        generated_point += layer_value

    generated_point *= context.total_layers_weight
    return generated_point
```
*The ***generator*** variable is the generator currently being processed*


# PrefabGroup weighting tags
Exctracted from BiomeMap.m_SceneryDistributionWeights

* trees
* luxite
* plumbite
* titanite
* carbite
* rodite
* oleite
* erudite
* celestite