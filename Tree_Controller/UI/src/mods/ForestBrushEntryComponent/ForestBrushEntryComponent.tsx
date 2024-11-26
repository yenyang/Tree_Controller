
import { getModule } from "cs2/modding";
import styles from "./ForestBrushEntryComponent.module.scss";
import { Button, Tooltip } from "cs2/ui";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import { bindValue, trigger, useValue } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import { useState } from "react";
import classNames from "classnames";
import { AdvancedForestBrushEntry } from "Domain/advancedForestBrushEntry";
import { Ages, childSrc, teenSrc, adultSrc, elderlySrc, deadSrc, stumpSrc, clearAgesSrc, descriptionTooltip, ageChangSrc } from "mods/TreeControllerSections/treeControllerSections";
import locale from "../lang/en-US.json";

const uilStandard =                         "coui://uil/Standard/";
const minusSrc =            uilStandard + "Minus.svg"

const descriptionToolTipStyle = getModule("game-ui/common/tooltip/description-tooltip/description-tooltip.module.scss", "classes");

const SliderField : any = getModule("game-ui/editor/widgets/fields/number-slider-field.tsx", "FloatSliderField");
   
function changeValue(event:string, name:string, value : number) {
    trigger(mod.id, event, name, value);
}

export const ForestBrushEntryComponent = (props: { entry : AdvancedForestBrushEntry }) => {
    const { translate } = useLocalization();

    const childTooltipTitle = translate("ToolOptions.TOOLTIP_DESCRIPTION[Sapling]");
    const teenTooltipTitle = translate("ToolOptions.TOOLTIP_DESCRIPTION[Young]");
    const adultTooltipTitle = translate("ToolOptions.TOOLTIP_DESCRIPTION[Mature]");
    const elderlyTooltipTitle = translate("ToolOptions.TOOLTIP_DESCRIPTION[Elderly]");
    const deadTooltipTitle = translate("YY_TREE_CONTROLLER[dead]",locale["YY_TREE_CONTROLLER[dead]"]);    
    const clearAgeTooltipTitle = translate("YY_TREE_CONTROLLER[clear-ages]",locale["YY_TREE_CONTROLLER[clear-ages]"]);
    const clearAgeTooltipDescription = translate("YY_TREE_CONTROLLER_DESCRIPTION[clear-ages]", locale["YY_TREE_CONTROLLER_DESCRIPTION[clear-ages]"]);
    return (
        <div className={classNames(styles.rowGroup)}>
            <div className={classNames(styles.columnGroup, styles.centered, styles.PrefabThumbnailWidth)}>
                <div className={classNames(styles.rowGroup, styles.centered)}>
                    <Tooltip tooltip={"Hello"}>
                        <div className={classNames(VanillaComponentResolver.instance.assetGridTheme.item)}>
                            <img src={props.entry.Src} className={classNames(VanillaComponentResolver.instance.assetGridTheme.thumbnail)}></img>
                        </div>
                    </Tooltip>
                </div>
                <div className={classNames(styles.rowGroup, styles.centered, styles.text)}>{translate("Assets.NAME["+props.entry.Name+"]")}</div>
                <div className={classNames(styles.rowGroup, styles.centered)}>
                    <VanillaComponentResolver.instance.ToolButton  tooltip={"Remove"}      onSelect={() => {}}        src={minusSrc}       focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>  
                </div>
            </div>
            <div className={styles.columnGroup}>
                
                {( (props.entry.SelectedAges & Ages.Hide) != Ages.Hide &&                     
                    <VanillaComponentResolver.instance.Section title={translate("YY_TREE_CONTROLLER[Age]",locale["YY_TREE_CONTROLLER[Age]"])}>
                        <>
                            <VanillaComponentResolver.instance.ToolButton  selected={(props.entry.SelectedAges & Ages.OverrideAge) == Ages.OverrideAge}         tooltip={"Match gloabl"}      onSelect={() => {changeValue("SetEntryAge", props.entry.Name ,Ages.OverrideAge); console.log("yes")}}        src={ageChangSrc}       focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                            {( (props.entry.SelectedAges & Ages.OverrideAge) == Ages.OverrideAge && 
                                <>
                                    <VanillaComponentResolver.instance.ToolButton  selected={(props.entry.SelectedAges & Ages.All) == Ages.All}         tooltip={descriptionTooltip(clearAgeTooltipTitle, clearAgeTooltipDescription)}      onSelect={() => changeValue("SetEntryAge", props.entry.Name ,Ages.All)}        src={clearAgesSrc}       focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                                    <VanillaComponentResolver.instance.ToolButton  selected={(props.entry.SelectedAges & Ages.Child) == Ages.Child}     tooltip={childTooltipTitle}            onSelect={() => changeValue("SetEntryAge", props.entry.Name ,Ages.Child)}      src={childSrc}           focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                                    <VanillaComponentResolver.instance.ToolButton  selected={(props.entry.SelectedAges & Ages.Teen) == Ages.Teen}       tooltip={teenTooltipTitle}              onSelect={() => changeValue("SetEntryAge", props.entry.Name ,Ages.Teen)}       src={teenSrc}            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                                    <VanillaComponentResolver.instance.ToolButton  selected={(props.entry.SelectedAges & Ages.Adult) == Ages.Adult}     tooltip={adultTooltipTitle}            onSelect={() => changeValue("SetEntryAge", props.entry.Name ,Ages.Adult)}      src={adultSrc}           focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                                    <VanillaComponentResolver.instance.ToolButton  selected={(props.entry.SelectedAges & Ages.Elderly) == Ages.Elderly} tooltip={elderlyTooltipTitle}        onSelect={() => changeValue("SetEntryAge", props.entry.Name ,Ages.Elderly)}    src={elderlySrc}         focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                                    <VanillaComponentResolver.instance.ToolButton  selected={(props.entry.SelectedAges & Ages.Dead) == Ages.Dead}       tooltip={deadTooltipTitle}              onSelect={() =>changeValue("SetEntryAge", props.entry.Name ,Ages.Dead)}       src={deadSrc}            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                                </>
                            )}
                            {( false && 
                                <VanillaComponentResolver.instance.ToolButton  selected={(props.entry.SelectedAges & Ages.Stump) == Ages.Stump} tooltip={translate("Tree_Controller.TOOLTIP_TITLE[Stump]", locale["Tree_Controller.TOOLTIP_TITLE[Stump]"])}     onSelect={() => console.log("stump")}        src={stumpSrc}       focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                            )}
                        </>
                    </VanillaComponentResolver.instance.Section>
                )}
                <VanillaComponentResolver.instance.Section title={"Probability Weight"}>
                    <div className={styles.SliderFieldWidth}>
                        <SliderField value={props.entry.ProbabilityWeight} min={1} max={200} fractionDigits={0} onChange={(e: number) => {changeValue("SetProbabilityWeight", props.entry.Name ,e)}}></SliderField>
                    </div>
                </VanillaComponentResolver.instance.Section>
                <VanillaComponentResolver.instance.Section title={"Minimum Elevation"}>
                    <div className={styles.SliderFieldWidth}>
                        <SliderField value={props.entry.MinimumElevation} min={0} max={4000} fractionDigits={0} onChange={(e: number) => {changeValue("SetMinimumElevation", props.entry.Name ,e)}}></SliderField>
                    </div>
                </VanillaComponentResolver.instance.Section>
                <VanillaComponentResolver.instance.Section title={"Maximum Elevation"}>
                    <div className={styles.SliderFieldWidth}>
                        <SliderField value={props.entry.MaximumElevation} min={0} max={4000} fractionDigits={0} onChange={(e: number) => {changeValue("SetMaximumElevation", props.entry.Name ,e)}}></SliderField>
                    </div>
                </VanillaComponentResolver.instance.Section>
            </div>
        </div>
      );
}