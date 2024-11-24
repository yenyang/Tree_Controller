import { Ages } from "mods/TreeControllerSections/treeControllerSections";

export interface AdvancedForestBrushEntry {
    Name: string,
    Src: string,
    Ages: Ages,
    ProbabilityWeight: Number,
    MinimumElevation: Number,
    MaximumElevation: Number,
}