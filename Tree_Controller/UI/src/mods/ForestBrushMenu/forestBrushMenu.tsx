import { Button, Panel, Portal } from "cs2/ui";
import styles from "./forestBrushMenu.module.scss"
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import { bindValue, trigger, useValue } from "cs2/api";
import mod from "../../../mod.json";
import { getModule } from "cs2/modding";
import { game } from "cs2/bindings";
import { useLocalization } from "cs2/l10n";
import locale from "../lang/en-US.json";
import { ForestBrushEntryComponent } from "mods/ForestBrushEntryComponent/ForestBrushEntryComponent";
import { AdvancedForestBrushEntry } from "Domain/advancedForestBrushEntry";
import { Ages, evergreenTreesID, deciduousTreesID, wildBushesID, customSetID, descriptionTooltip, deciduousSrc, bushesSrc, evergreenSrc , changePrefabSet} from "mods/TreeControllerSections/treeControllerSections";

const uilStandard =                         "coui://uil/Standard/";

const closeSrc =         uilStandard +  "XClose.svg";
const arrowUpSrc =           uilStandard +  "ArrowUpThickStroke.svg";

// const ShowPanel$ = bindValue<boolean>(mod.id, "ShowForestBrushPanel");
const PrefabSet$ =           bindValue<string>(mod.id, 'PrefabSet');
const AdvancedForestBrushEntries$ = bindValue<AdvancedForestBrushEntry[]>(mod.id, 'AdvancedForestBrushEntries');

function handleClick(event: string) {
    trigger(mod.id, event);
}

const roundButtonHighlightStyle = getModule("game-ui/common/input/button/themes/round-highlight-button.module.scss", "classes");

export const ForestBrushMenuComponent = () => {
    // const ShowPanel = useValue(ShowPanel$);
    const isPhotoMode = useValue(game.activeGamePanel$)?.__Type == game.GamePanelType.PhotoMode;
    const AdvancedForestBrushEntries = useValue(AdvancedForestBrushEntries$);
    const PrefabSet = useValue(PrefabSet$);
    const { translate } = useLocalization();

    const deciduousTooltipTitle = translate("YY_TREE_CONTROLLER[wild-deciduous-trees]",locale["YY_TREE_CONTROLLER[wild-deciduous-trees]"]);
    const deciduousTooltipDescription = translate("YY_TREE_CONTROLLER_DESCRIPTION[wild-deciduous-trees]",locale["YY_TREE_CONTROLLER_DESCRIPTION[wild-deciduous-trees]"]);
    const evergreenTooltipTitle = translate("YY_TREE_CONTROLLER[evergreen-trees]" ,locale["YY_TREE_CONTROLLER[evergreen-trees]"]);
    const evergreenTooltipDescription = translate( "YY_TREE_CONTROLLER_DESCRIPTION[evergreen-trees]",locale["YY_TREE_CONTROLLER_DESCRIPTION[evergreen-trees]"]);
    const wildBushesTooltipTitle = translate("YY_TREE_CONTROLLER[wild-bushes]",locale["YY_TREE_CONTROLLER[wild-bushes]"]);
    const wildBushesTooltipDescription = translate("YY_TREE_CONTROLLER_DESCRIPTION[wild-bushes]",locale["YY_TREE_CONTROLLER_DESCRIPTION[wild-bushes]"]);
    const customSet1TooltipTitle = translate("YY_TREE_CONTROLLER[custom-set-1]",locale["YY_TREE_CONTROLLER[custom-set-1]"]);
    const customSet1TooltipDescription = translate("YY_TREE_CONTROLLER_DESCRIPTION[custom-set-1]",locale["YY_TREE_CONTROLLER_DESCRIPTION[custom-set-1]"]);
    const customSet2TooltipTitle = translate("YY_TREE_CONTROLLER[custom-set-2]",locale["YY_TREE_CONTROLLER[custom-set-2]"]);
    const customSet2TooltipDescription = translate("YY_TREE_CONTROLLER_DESCRIPTION[custom-set-2]",locale["YY_TREE_CONTROLLER_DESCRIPTION[custom-set-2]"]);
    const customSet3TooltipTitle = translate("YY_TREE_CONTROLLER[custom-set-3]",locale["YY_TREE_CONTROLLER[custom-set-3]"]);
    const customSet3TooltipDescription = translate("YY_TREE_CONTROLLER_DESCRIPTION[custom-set-3]",locale["YY_TREE_CONTROLLER_DESCRIPTION[custom-set-3]"]);
    const customSet4TooltipTitle = translate("YY_TREE_CONTROLLER[custom-set-4]",locale["YY_TREE_CONTROLLER[custom-set-4]"]);
    const customSet4TooltipDescription = translate("YY_TREE_CONTROLLER_DESCRIPTION[custom-set-4]",locale["YY_TREE_CONTROLLER_DESCRIPTION[custom-set-4]"]);
    const customSet5TooltipTitle = translate("YY_TREE_CONTROLLER[custom-set-5]",locale["YY_TREE_CONTROLLER[custom-set-5]"]);
    const customSet5TooltipDescription = translate("YY_TREE_CONTROLLER_DESCRIPTION[custom-set-5]",locale["YY_TREE_CONTROLLER_DESCRIPTION[custom-set-5]"]);
    
    const example : AdvancedForestBrushEntry = 
    {
        Name: "Grass",
        Src: uilStandard + "RoadUpgradeRetainingWall.svg",
        Ages: Ages.Adult,
        ProbabilityWeight: 100,
        MinimumElevation: 0,
        MaximumElevation: 4000,
    }
    return (
        <>
            {true && !isPhotoMode && (
                <Portal>
                    <Panel
                        className={styles.panel}
                        header={(
                            <VanillaComponentResolver.instance.Section title={"Forest Brush Advanced Options"}>
                                <Button className={roundButtonHighlightStyle.button} variant="icon" onSelect={() => handleClick("ToggleForestBrushPanel")} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}>
                                    <img src={closeSrc}></img>
                                </Button>
                            </VanillaComponentResolver.instance.Section>
                        )}>
                        <VanillaComponentResolver.instance.Section title={translate("YY_TREE_CONTROLLER[Sets]",locale["YY_TREE_CONTROLLER[Sets]"])}>
                            <VanillaComponentResolver.instance.ToolButton  selected={PrefabSet == deciduousTreesID}     tooltip={descriptionTooltip(deciduousTooltipTitle,deciduousTooltipDescription)}         onSelect={() => changePrefabSet(deciduousTreesID)}    src={deciduousSrc}                                                                                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                            <VanillaComponentResolver.instance.ToolButton  selected={PrefabSet == evergreenTreesID}     tooltip={descriptionTooltip(evergreenTooltipTitle, evergreenTooltipDescription)}        onSelect={() => changePrefabSet(evergreenTreesID)}    src={evergreenSrc}                                                                                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                            <VanillaComponentResolver.instance.ToolButton  selected={PrefabSet == wildBushesID}         tooltip={descriptionTooltip(wildBushesTooltipTitle, wildBushesTooltipDescription)}      onSelect={() => changePrefabSet(wildBushesID)}        src={bushesSrc}                                                                                       focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                            <VanillaComponentResolver.instance.ToolButton  selected={PrefabSet == customSetID+1}        tooltip={descriptionTooltip(customSet1TooltipTitle, customSet1TooltipDescription)}      onSelect={() => changePrefabSet(customSetID+1)}                             children={<div className={styles.ForestBrushMenuNumberedButton}>1</div>}       focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                            <VanillaComponentResolver.instance.ToolButton  selected={PrefabSet == customSetID+2}        tooltip={descriptionTooltip(customSet2TooltipTitle, customSet2TooltipDescription)}      onSelect={() => changePrefabSet(customSetID+2)}                             children={<div className={styles.ForestBrushMenuNumberedButton}>2</div>}       focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                            <VanillaComponentResolver.instance.ToolButton  selected={PrefabSet == customSetID+3}        tooltip={descriptionTooltip(customSet3TooltipTitle, customSet3TooltipDescription)}      onSelect={() => changePrefabSet(customSetID+3)}                             children={<div className={styles.ForestBrushMenuNumberedButton}>3</div>}       focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                            <VanillaComponentResolver.instance.ToolButton  selected={PrefabSet == customSetID+4}        tooltip={descriptionTooltip(customSet4TooltipTitle, customSet4TooltipDescription)}      onSelect={() => changePrefabSet(customSetID+4)}                             children={<div className={styles.ForestBrushMenuNumberedButton}>4</div>}       focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                            <VanillaComponentResolver.instance.ToolButton  selected={PrefabSet == customSetID+5}        tooltip={descriptionTooltip(customSet5TooltipTitle, customSet5TooltipDescription)}      onSelect={() => changePrefabSet(customSetID+5)}                             children={<div className={styles.ForestBrushMenuNumberedButton}>5</div>}       focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                        </VanillaComponentResolver.instance.Section>
                        { AdvancedForestBrushEntries.map((currentEntry) => (
                            <div className={styles.rowGroup}>
                                <ForestBrushEntryComponent entry={currentEntry}></ForestBrushEntryComponent>
                            </div>
                        ))}
                    </Panel>
                </Portal>
            )}
        </>
    );
}