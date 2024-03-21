import {ModuleRegistryExtend} from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import { Tool, tool } from "cs2/bindings";
import mod from "../../../mod.json";
import { VanillaComponentResolver } from "../VanillaComponentResolver/VanillaComponentResolver";
import { useLocalization } from "cs2/l10n";
import clearAgesSrc from "./All.svg";
import brushSrc from "./Brush.svg";
import styles from "./treeController.module.scss";
import { Dispatch, SetStateAction, useState } from "react";
import { Icon } from "cs2/ui";

export enum Ages 
{
    None = 0,
    Child = 1,
    Teen = 2,
    Adult = 4,
    Elderly = 8,
    Dead = 16,
    All = 32,
}

export enum ToolMode 
{
    Plop = 0,
    Brush = 1,
    ChangeAge = 2,
    ChangeType = 3,
}

export enum Selection
{
    Single = 0,
    BuildingOrNet = 1,
    Radius = 2,
    Map = 3,
}

// These contain the coui paths to Unified Icon Library svg assets
export const couiStandard =                         "coui://uil/Standard/";
export const ageChangSrc =          couiStandard +  "ReplaceTreeAge.svg";
export const prefabChangeSrc =      couiStandard +  "Replace.svg";
export const buildingOrNetSrc =     couiStandard +  "HouseandNetwork.svg";
export const radiusSrc =            couiStandard +  "Circle.svg";
export const wholeMapSrc    =       couiStandard +  "MapGrid.svg";
export const childSrc =             couiStandard +  "TreeSapling.svg";
export const teenSrc =              couiStandard +  "TreeTeen.svg";
export const adultSrc =             couiStandard +  "TreeAdult.svg";
export const elderlySrc =           couiStandard +  "TreeElderly.svg";
export const deadSrc =              couiStandard +  "TreeDead.svg";
export const arrowDownSrc =         couiStandard +  "ArrowDownThickStroke.svg";
export const arrowUpSrc =           couiStandard +  "ArrowUpThickStroke.svg";
export const deciduousSrc =         couiStandard +  "TreesDeciduous.svg";
export const evergreenSrc =         couiStandard +  "TreesNeedle.svg";
export const bushesSrc =            couiStandard +  "Bushes.svg";
export const diskSaveSrc =          couiStandard +  "DiskSave.svg";
export const randomRotationSrc =    couiStandard +  "Dice.svg";

// These establishes the binding with C# side. Without C# side game ui will crash.

export const ToolMode$ =            bindValue<number> (mod.id, 'ToolMode');
export const SelectedAges$ =        bindValue<number> (mod.id, 'SelectedAges');
export const SelectionMode$ =       bindValue<number> (mod.id, 'SelectionMode');
/*
export const RadiusValue$ =        bindValue<number> (mod.id, 'RadiusValue');
export const MinDepthValue$ =      bindValue<number> (mod.id, 'MinDepthValue');
export const AmountLocaleKey$ =    bindValue<string> (mod.id, 'AmountLocaleKey');
export const AmountStep$ =         bindValue<number> (mod.id, 'AmountStep');
export const RadiusStep$ =         bindValue<number> (mod.id, 'RadiusStep');
export const MinDepthStep$ =       bindValue<number> (mod.id, 'MinDepthStep');
export const AmountScale$ =        bindValue<number> (mod.id, 'AmountScale');
export const RadiusScale$ =        bindValue<number> (mod.id, 'RadiusScale');
export const MinDepthScale$ =      bindValue<number> (mod.id, 'MinDepthScale');
export const ShowMinDepth$ =       bindValue<number> (mod.id, 'ShowMinDepth');
export const ToolMode$ =           bindValue<number> (mod.id, "ToolMode");
*/

// These are strings that will be used for translations keys and event triggers.
/*
export const amountDownID =             "amount-down-arrow";
export const amountUpID =               "amount-up-arrow";
export const radiusDownID =             "radius-down-arrow";
export const radiusUpID =               "radius-up-arrow";
export const minDepthDownID =           "min-depth-down-arrow";
export const minDepthUpID =             "min-depth-up-arrow";
export const amountStepID =             "amount-rate-of-change";
export const radiusStepID =             "radius-rate-of-change";
export const minDepthStepID =           "min-depth-rate-of-change";
export const tooltipDescriptionPrefix = "YY_WATER_FEATURES_DESCRIPTION.";
export const sectionTitlePrefix =       "YY_WATER_FEATURES.";
export const elevationChangeID =        "ElevationChange";
export const placeWaterSourceID =       "PlaceWaterSource";
export const moveWaterSourceID =        "MoveWaterSource";
export const radiusChangeID =           "RadiusChange";
*/

// This functions trigger an event on C# side and C# designates the method to implement.
export function handleClick(eventName: string) 
{
    trigger(mod.id, eventName);
}

// This function triggers an event to change the tree controller tool mode to specified tool mode.
export function changeToolMode(toolMode: ToolMode) {
    trigger(mod.id, "ChangeToolMode", toolMode);
}

// This function triggers an event to change the tree controller tool mode to specified tool mode.
export function changeSelectedAge(age: Ages) {
    trigger(mod.id, "ChangeSelectedAge", age);
}

// This function triggers an event to change the tree controller tool mode to specified tool mode.
export function changeSelectionMode(selectionMode: Selection) {
    trigger(mod.id, "ChangeSelectionMode", selectionMode);
}

export const TreeControllerComponent: ModuleRegistryExtend = (Component : any) => 
{
    // I believe you should not put anything here.
    return (props) => 
    {
        const {children, ...otherProps} = props || {};

        // These get the value of the bindings.
        const objectToolActive = useValue(tool.activeTool$).id == tool.OBJECT_TOOL;
        const treeControllerToolActive = useValue(tool.activeTool$).id == "Tree Controller Tool";
        const lineToolActive = useValue(tool.activeTool$).id == "Line Tool";
        const SelectionMode = useValue(SelectionMode$);
        const CurrentToolMode = useValue(ToolMode$);
        const SelectedAges = useValue(SelectedAges$) as Ages;
        /*
        const RadiusValue = useValue(RadiusValue$);
        const MinDepthValue = useValue(MinDepthValue$);
        const AmountLocaleKey = useValue(AmountLocaleKey$);
        const AmountStep = useValue(AmountStep$);
        const RadiusStep = useValue(RadiusStep$);
        const MinDepthStep = useValue(MinDepthStep$);
        const AmountScale = useValue(AmountScale$);
        const RadiusScale = useValue(RadiusScale$);
        const MinDepthScale = useValue(MinDepthScale$);
        const ShowMinDepth = useValue(ShowMinDepth$);
        const ToolMode = useValue(ToolMode$);
        */


        // These set up state variables for custom sets switching from number to save disk icon.
        const [isCustomSet1Hovered, setCustomSet1Hovered] = useState(false);
        const [isCustomSet2Hovered, setCustomSet2Hovered] = useState(false);
        const [isCustomSet3Hovered, setCustomSet3Hovered] = useState(false);
        const [isCustomSet4Hovered, setCustomSet4Hovered] = useState(false);
        const [isCustomSet5Hovered, setCustomSet5Hovered] = useState(false);

        // These functions generate a div with either a number inside or an icon. might be amore efficient way to do this.
        function GenerateCustomSetNumber1() : JSX.Element 
        {
            return (
                <div className = {styles.yyNumberedButton} 
                     onMouseEnter={(ev) => {if (ev.ctrlKey) setCustomSet1Hovered(true); else setCustomSet1Hovered(false)}}
                     onMouseLeave={() => setCustomSet1Hovered(false)}>
                    {isCustomSet1Hovered ? <Icon src={diskSaveSrc}></Icon> : 1}
                </div>
            );
        }

        function GenerateCustomSetNumber2() : JSX.Element 
        {
            return (
                <div className = {styles.yyNumberedButton} 
                     onMouseEnter={(ev) => {if (ev.ctrlKey) setCustomSet2Hovered(true); else setCustomSet2Hovered(false)}}
                     onMouseLeave={() => setCustomSet2Hovered(false)}>
                    {isCustomSet2Hovered ? <Icon src={diskSaveSrc}></Icon> : 2}
                </div>
            );
        }

        function GenerateCustomSetNumber3() : JSX.Element 
        {
            return (
                <div className = {styles.yyNumberedButton} 
                     onMouseEnter={(ev) => {if (ev.ctrlKey) setCustomSet3Hovered(true); else setCustomSet3Hovered(false)}}
                     onMouseLeave={() => setCustomSet3Hovered(false)}>
                    {isCustomSet3Hovered ? <Icon src={diskSaveSrc}></Icon> : 3}
                </div>
            );
        }

        function GenerateCustomSetNumber4() : JSX.Element 
        {
            return (
                <div className = {styles.yyNumberedButton} 
                     onMouseEnter={(ev) => {if (ev.ctrlKey) setCustomSet4Hovered(true); else setCustomSet4Hovered(false)}}
                     onMouseLeave={() => setCustomSet4Hovered(false)}>
                    {isCustomSet4Hovered ? <Icon src={diskSaveSrc}></Icon> : 4}
                </div>
            );
        }

        function GenerateCustomSetNumber5() : JSX.Element 
        {
            return (
                <div className = {styles.yyNumberedButton} 
                     onMouseEnter={(ev) => {if (ev.ctrlKey) setCustomSet5Hovered(true); else setCustomSet5Hovered(false)}}
                     onMouseLeave={() => setCustomSet5Hovered(false)}>
                    {isCustomSet5Hovered ? <Icon src={diskSaveSrc}></Icon> : 5}
                </div>
            );
        }
        
        
        // translation handling. Translates using locale keys that are defined in C# or fallback string here.
        const { translate } = useLocalization();
        /*
        const amountDownTooltip =       translate(tooltipDescriptionPrefix + amountDownID,      "Reduces the flow for Streams. Decreases the depth or elevation for rivers, seas, and lakes. Reduces the max depth for retention and detention basins.");
        const amountUpTooltip =         translate(tooltipDescriptionPrefix + amountUpID,        "Increases the flow for Streams. Increases the depth or elevation for rivers, seas, and lakes. Increases the max depth for retention and detention basins.");
        const radiusDownTooltip =       translate(tooltipDescriptionPrefix + radiusDownID,      "Reduces the radius.");
        const radiusUpTooltip =         translate(tooltipDescriptionPrefix + radiusUpID,        "Increases the radius.");
        const minDepthDownTooltip =     translate(tooltipDescriptionPrefix + minDepthDownID,    "Reduces the minimum depth.");
        const minDepthUpTooltip =       translate(tooltipDescriptionPrefix + minDepthUpID,      "Increases the minimum depth.");
        const amountStepTooltip =       translate(tooltipDescriptionPrefix + amountStepID,      "Changes the rate in which the increase and decrease buttons work for Flow, Depth and Elevation.");
        const radiusStepTooltip =       translate(tooltipDescriptionPrefix + radiusStepID,      "Changes the rate in which the increase and decrease buttons work for Radius.");
        const minDepthStepTooltip =     translate(tooltipDescriptionPrefix + minDepthStepID,    "Changes the rate in which the increase and decrease buttons work for minimum depth.");
        const minDepthSection =         translate(sectionTitlePrefix + "MinDepth",              "Min Depth");
        const radiusSection =           translate(sectionTitlePrefix + "Radius",                "Radius");
        const amountSection =           translate(AmountLocaleKey,                              "Depth");
        const elevationChangeTooltip =  translate(tooltipDescriptionPrefix + elevationChangeID, "Water Tool will change target elevations of existing water sources by hovering over existing water source, left clicking, holding, dragging and releasing at new elevation. Usually dragging out raises, and dragging in lowers, but it's really just releasing at the desired elevation. Keep the cursor within playable area for reliability. Right click to cancel.");
        const placeWaterSourceTooltip = translate(tooltipDescriptionPrefix + placeWaterSourceID,"Water Tool will place water sources with left click, and remove water sources with right click.");
        const moveWaterSourceTooltip =  translate(tooltipDescriptionPrefix + moveWaterSourceID, "Water Tool will move existing water sources. Target elevations of existing water sources will not change. Right click to cancel.");
        const radiusChangeTooltip =     translate(tooltipDescriptionPrefix + radiusChangeID,    "Water Tool will change radius of water sources. Right click to cancel.");
        const toolModeTitle =           translate("Toolbar.TOOL_MODE_TITLE", "Tool Mode");
        */

        var result = Component();
        
        if (objectToolActive || treeControllerToolActive || lineToolActive) 
        {
            result.props.children?.push
            (
                /* 
                Add a new section before other tool options sections with translated title based of localization key from binding. Localization key defined in C#.
                Adds up and down buttons and field with step button. All buttons have translated tooltips. OnSelect triggers C# events. Src paths are local imports.
                values must be decending. SelectedValue is from binding. 

                */
                <>
                    { ((objectToolActive && CurrentToolMode == ToolMode.Brush)|| (treeControllerToolActive && CurrentToolMode == ToolMode.ChangeType)) && (
                    <VanillaComponentResolver.instance.Section title={"Sets"}>
                        <VanillaComponentResolver.instance.ToolButton  selected={false}    tooltip={"deciduous"}        /*onSelect={() => handleClick("rotatoin")}*/    src={deciduousSrc}                                              focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                        <VanillaComponentResolver.instance.ToolButton  selected={false}    tooltip={"evergreen"}      /*onSelect={() => handleClick("rotatoin")}*/      src={evergreenSrc}                                              focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                        <VanillaComponentResolver.instance.ToolButton  selected={false}    tooltip={"bushes"}      /*onSelect={() => handleClick("rotatoin")}*/         src={bushesSrc}                                                 focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                        <VanillaComponentResolver.instance.ToolButton  selected={false}    tooltip={"custom 1"} /*onSelect={() => handleClick("rotatoin")}*/               children={GenerateCustomSetNumber1()}   focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                        <VanillaComponentResolver.instance.ToolButton  selected={false}    tooltip={"custom 2"}      /*onSelect={() => handleClick("rotatoin")}*/       children={GenerateCustomSetNumber2()}   focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                        <VanillaComponentResolver.instance.ToolButton  selected={false}    tooltip={"custom 3"}      /*onSelect={() => handleClick("rotatoin")}*/       children={GenerateCustomSetNumber3()}   focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                        <VanillaComponentResolver.instance.ToolButton  selected={false}    tooltip={"custom 4"}      /*onSelect={() => handleClick("rotatoin")}*/       children={GenerateCustomSetNumber4()}   focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                        <VanillaComponentResolver.instance.ToolButton  selected={false}    tooltip={"custom 5"}      /*onSelect={() => handleClick("rotatoin")}*/       children={GenerateCustomSetNumber5()}   focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                    </VanillaComponentResolver.instance.Section>
                    )}
                    <VanillaComponentResolver.instance.Section title={"Age"}>
                        <VanillaComponentResolver.instance.ToolButton  selected={(SelectedAges & Ages.All) == Ages.All}         tooltip={"clear"}       onSelect={() => changeSelectedAge(Ages.All)}        src={clearAgesSrc}       focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                        <VanillaComponentResolver.instance.ToolButton  selected={(SelectedAges & Ages.Child) == Ages.Child}     tooltip={"child"}       onSelect={() => changeSelectedAge(Ages.Child)}      src={childSrc}           focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                        <VanillaComponentResolver.instance.ToolButton  selected={(SelectedAges & Ages.Teen) == Ages.Teen}       tooltip={"teen"}        onSelect={() => changeSelectedAge(Ages.Teen)}       src={teenSrc}            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                        <VanillaComponentResolver.instance.ToolButton  selected={(SelectedAges & Ages.Adult) == Ages.Adult}     tooltip={"adult"}       onSelect={() => changeSelectedAge(Ages.Adult)}      src={adultSrc}           focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                        <VanillaComponentResolver.instance.ToolButton  selected={(SelectedAges & Ages.Elderly) == Ages.Elderly} tooltip={"elderly"}     onSelect={() => changeSelectedAge(Ages.Elderly)}    src={elderlySrc}         focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                        <VanillaComponentResolver.instance.ToolButton  selected={(SelectedAges & Ages.Dead) == Ages.Dead}       tooltip={"dead"}        onSelect={() => changeSelectedAge(Ages.Dead)}       src={deadSrc}            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                    </VanillaComponentResolver.instance.Section>
                    { treeControllerToolActive && (
                    <VanillaComponentResolver.instance.Section title={"Selection"}>
                        <VanillaComponentResolver.instance.ToolButton  selected={SelectionMode == Selection.Single}         tooltip={"single tree"}      onSelect={() => changeSelectionMode(Selection.Single)}             src={adultSrc}            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                        <VanillaComponentResolver.instance.ToolButton  selected={SelectionMode == Selection.BuildingOrNet}  tooltip={"building or net"}  onSelect={() => changeSelectionMode(Selection.BuildingOrNet)}      src={buildingOrNetSrc}    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                        <VanillaComponentResolver.instance.ToolButton  selected={SelectionMode == Selection.Radius}         tooltip={"radius"}           onSelect={() => changeSelectionMode(Selection.Radius)}             src={radiusSrc}           focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                        <VanillaComponentResolver.instance.ToolButton  selected={SelectionMode == Selection.Map}            tooltip={"whole map"}        onSelect={() => changeSelectionMode(Selection.Map)}                src={wholeMapSrc}         focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                    </VanillaComponentResolver.instance.Section>
                    )}
                    { treeControllerToolActive && SelectionMode == Selection.Radius && (
                    <VanillaComponentResolver.instance.Section title={"Radius"}>
                        <VanillaComponentResolver.instance.ToolButton tooltip={"radiusDownTooltip"} onSelect={() => handleClick("radius down")} src={arrowDownSrc} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED} className={VanillaComponentResolver.instance.mouseToolOptionsTheme.startButton}></VanillaComponentResolver.instance.ToolButton>
                        <div className={VanillaComponentResolver.instance.mouseToolOptionsTheme.numberField}>{ 10 /*RadiusValue.toFixed(RadiusScale)*/ + " m"}</div>
                        <VanillaComponentResolver.instance.ToolButton tooltip={"radiusUpTooltip"} onSelect={() => handleClick("radius up")} src={arrowUpSrc} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED} className={VanillaComponentResolver.instance.mouseToolOptionsTheme.endButton} ></VanillaComponentResolver.instance.ToolButton>
                    </VanillaComponentResolver.instance.Section>
                    )}
                    <VanillaComponentResolver.instance.Section title={"Change"}>
                            <VanillaComponentResolver.instance.ToolButton  selected={CurrentToolMode == ToolMode.ChangeAge}     tooltip={"change age"}     onSelect={() => changeToolMode(ToolMode.ChangeAge)}     src={ageChangSrc}      focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                            <VanillaComponentResolver.instance.ToolButton  selected={CurrentToolMode == ToolMode.ChangeType}    tooltip={"change prefab"}  onSelect={() => changeToolMode(ToolMode.ChangeType)}    src={prefabChangeSrc}  focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                    </VanillaComponentResolver.instance.Section>
                    { treeControllerToolActive && (
                    <VanillaComponentResolver.instance.Section title={"Tool Mode"}>
                            <VanillaComponentResolver.instance.ToolButton  selected={CurrentToolMode == ToolMode.Plop}    tooltip={"plop"}     onSelect={() => changeToolMode(ToolMode.Plop)}     src={adultSrc}      focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                            <VanillaComponentResolver.instance.ToolButton  selected={CurrentToolMode == ToolMode.Brush}    tooltip={"brush"}   onSelect={() => changeToolMode(ToolMode.Brush)}    src={brushSrc}      focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                    </VanillaComponentResolver.instance.Section>                
                    )}
                </>
            );
        }
        return result;
    };
}