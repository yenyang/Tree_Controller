import { ModRegistrar } from "cs2/modding";
import { TreeControllerComponent } from "mods/TreeControllerSections/treeControllerSections";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import mod from "../mod.json";
// import { ForestBrushMenuComponent } from "mods/ForestBrushMenu/forestBrushMenu";

const register: ModRegistrar = (moduleRegistry) => {
      // console.log('mr', moduleRegistry);

      // The vanilla component resolver is a singleton that helps extrant and maintain components from game that were not specifically exposed.
      VanillaComponentResolver.setRegistry(moduleRegistry);
      
     // This extends mouse tool options to include all tree controller sections.
     moduleRegistry.extend("game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx", 'MouseToolOptions', TreeControllerComponent);

     // moduleRegistry.append('Game', ForestBrushMenuComponent);

     // moduleRegistry.append('Editor', ForestBrushMenuComponent);
     
     // This is just to verify using UI console that all the component registriations was completed.
     console.log(mod.id + " UI module registrations completed.");
}

export default register;