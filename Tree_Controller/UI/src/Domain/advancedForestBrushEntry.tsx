import { Ages } from "mods/TreeControllerSections/treeControllerSections";

export interface AdvancedForestBrushEntry {
    Name: string,
    Src: string,
    SelectedAges: Ages,
    ProbabilityWeight: number,
    MinimumElevation: number,
    MaximumElevation: number,
}