﻿using System.Collections.Generic;

namespace Navmesh.Customizations;

[CustomizationTerritory(130)]
class Z130UldahStepsofNald : NavmeshCustomization
{
    public override int Version => 1;
    
    public override void CustomizeScene(SceneExtractor scene) 
    {
        //Force all instances of Mesh as unwalkable
        if (scene.Meshes.TryGetValue("bg/ffxiv/wil_w1/twn/common/collision/w1t0_f0_kadn1.pcb", out var mesh))
        {
            foreach (var instance in mesh.Instances)
            {
                instance.ForceSetPrimFlags |= SceneExtractor.PrimitiveFlags.ForceUnwalkable;
            }
        }
    }
    
    public Z130UldahStepsofNald()
    {
    }
}
