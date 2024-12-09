
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
import { EntrySectionComponent } from "mods/EntrySectionComponent/EntrySectionComponent";

const uilStandard =                         "coui://uil/Standard/";
const minusSrc =            uilStandard + "Minus.svg";
const resetSrc =            uilStandard + "Reset.svg";


const MaxElevation$ =            bindValue<number> (mod.id, 'MaxElevation');
const SeaLevel$ =            bindValue<number> (mod.id, 'SeaLevel');
const PrefabSet$ =           bindValue<string>(mod.id, 'PrefabSet');

const descriptionToolTipStyle = getModule("game-ui/common/tooltip/description-tooltip/description-tooltip.module.scss", "classes");

const SliderField : any = getModule("game-ui/editor/widgets/fields/number-slider-field.tsx", "FloatSliderField");
   
function changeValue(event:string, name:string, value : number) {
    trigger(mod.id, event, name, value);
}

export const ForestBrushEntryComponent = (props: { entry : AdvancedForestBrushEntry, numberOfEntries : number}) => {

    const MaxElevation = useValue(MaxElevation$);
    const SeaLevel = useValue(SeaLevel$);
    const PrefabSet = useValue(PrefabSet$);

    const { translate } = useLocalization();

    const childTooltipTitle = translate("ToolOptions.TOOLTIP_DESCRIPTION[Sapling]");
    const teenTooltipTitle = translate("ToolOptions.TOOLTIP_DESCRIPTION[Young]");
    const adultTooltipTitle = translate("ToolOptions.TOOLTIP_DESCRIPTION[Mature]");
    const elderlyTooltipTitle = translate("ToolOptions.TOOLTIP_DESCRIPTION[Elderly]");
    const deadTooltipTitle = translate("YY_TREE_CONTROLLER[dead]",locale["YY_TREE_CONTROLLER[dead]"]);    
    const clearAgeTooltipTitle = translate("YY_TREE_CONTROLLER[clear-ages]",locale["YY_TREE_CONTROLLER[clear-ages]"]);
    const clearAgeTooltipDescription = translate("YY_TREE_CONTROLLER_DESCRIPTION[clear-ages]", locale["YY_TREE_CONTROLLER_DESCRIPTION[clear-ages]"]);
    let isDefault : boolean = (props.entry.ProbabilityWeight == 100 && (props.entry.SelectedAges == Ages.Adult || (props.entry.SelectedAges & Ages.Hide) == Ages.Hide) && props.entry.MinimumElevation == 0 && props.entry.MaximumElevation == MaxElevation)
    let removeable : boolean = ((PrefabSet.includes("custom") || PrefabSet == "") && props.numberOfEntries > 2);

    return (
        <div className={classNames(styles.rowGroup)}>
            <div className={classNames(styles.columnGroup, styles.centered, styles.PrefabThumbnailWidth)}>
                <div className={classNames(styles.rowGroup, styles.centered)}>
                    <Tooltip tooltip={translate("Assets.DESCRIPTION["+props.entry.Name+"]")}>
                        <div className={classNames(VanillaComponentResolver.instance.assetGridTheme.item)}>
                            <img src={props.entry.Src} className={classNames(VanillaComponentResolver.instance.assetGridTheme.thumbnail)}></img>
                        </div>
                    </Tooltip>
                </div>
                <div className={classNames(styles.rowGroup, styles.centered, styles.text)}>{translate("Assets.NAME["+props.entry.Name+"]")}</div>
                <div className={classNames(styles.rowGroup, styles.centered)}>
                    {removeable && (
                        <VanillaComponentResolver.instance.ToolButton  tooltip={descriptionTooltip(translate("Tree_Controller.TOOLTIP_TITLE[RemoveAsset]", locale["Tree_Controller.TOOLTIP_TITLE[RemoveAsset]"]), translate("Tree_Controller.TOOLTIP_DESCRIPTION[RemoveAsset]", locale["Tree_Controller.TOOLTIP_DESCRIPTION[RemoveAsset]"]))}      onSelect={() => {trigger(mod.id, "RemoveEntry", props.entry.Name)}}        src={minusSrc}       focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>  
                    )}
                    {(!isDefault) && (                    
                        <VanillaComponentResolver.instance.ToolButton  tooltip={translate("Tree_Controller.TOOLTIP_TITLE[ResetAssetEntry]",locale["Tree_Controller.TOOLTIP_TITLE[ResetAssetEntry]"])}      onSelect={() => {trigger(mod.id, "ResetEntry", props.entry.Name)}}        src={resetSrc}       focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>  
                    )}
                </div>
            </div>
            <div className={styles.columnGroup}>
                
                {( (props.entry.SelectedAges & Ages.Hide) != Ages.Hide &&                     
                    <EntrySectionComponent
                         title={translate("YY_TREE_CONTROLLER[Age]",locale["YY_TREE_CONTROLLER[Age]"])}
                         tooltip={descriptionTooltip(translate("Tree_Controller.TOOLTIP_TITLE[OverrideAge]", locale["Tree_Controller.TOOLTIP_TITLE[OverrideAge]"]), translate("Tree_Controller.TOOLTIP_DESCRIPTION[OverrideAge]", locale["Tree_Controller.TOOLTIP_DESCRIPTION[OverrideAge]"]))}
                    >
                        <>
                            <VanillaComponentResolver.instance.ToolButton  selected={(props.entry.SelectedAges & Ages.OverrideAge) == Ages.OverrideAge}         tooltip={descriptionTooltip(translate("Tree_Controller.TOOLTIP_TITLE[OverrideAge]", locale["Tree_Controller.TOOLTIP_TITLE[OverrideAge]"]), translate("Tree_Controller.TOOLTIP_DESCRIPTION[OverrideAge]", locale["Tree_Controller.TOOLTIP_DESCRIPTION[OverrideAge]"]))}      onSelect={() => {changeValue("SetEntryAge", props.entry.Name ,Ages.OverrideAge); console.log("yes")}}        src={ageChangSrc}       focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
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
                    </EntrySectionComponent>
                )}
                <EntrySectionComponent 
                    title={translate("Tree_Controller.TOOLTIP_TITLE[ProbabilityWeight]", locale["Tree_Controller.TOOLTIP_TITLE[ProbabilityWeight]"])}
                    tooltip={translate("Tree_Controller.TOOLTIP_DESCRIPTION[ProbabilityWeight]", locale["Tree_Controller.TOOLTIP_DESCRIPTION[ProbabilityWeight]"])}
                >
                    <div className={styles.SliderFieldWidth}>
                        <SliderField value={props.entry.ProbabilityWeight} min={1} max={200} fractionDigits={0} onChange={(e: number) => {changeValue("SetProbabilityWeight", props.entry.Name ,e)}}></SliderField>
                    </div> 
                </EntrySectionComponent>

                <EntrySectionComponent
                        title={translate("Tree_Controller.TOOLTIP_TITLE[MinimumElevation]", locale["Tree_Controller.TOOLTIP_TITLE[MinimumElevation]"])}
                        tooltip={translate("Tree_Controller.TOOLTIP_DESCRIPTION[MinimumElevation]", locale["Tree_Controller.TOOLTIP_DESCRIPTION[MinimumElevation]"])}>
                        <div className={styles.SliderFieldWidth}>
                            <SliderField value={props.entry.MinimumElevation-SeaLevel} min={-SeaLevel} max={MaxElevation-SeaLevel} fractionDigits={0} onChange={(e: number) => {changeValue("SetMinimumElevation", props.entry.Name ,e+SeaLevel)}}></SliderField>
                        </div>
                </EntrySectionComponent>
                <EntrySectionComponent 
                    title={translate("Tree_Controller.TOOLTIP_TITLE[MaximumElevation]", locale["Tree_Controller.TOOLTIP_TITLE[MaximumElevation]"])} 
                    tooltip={translate("Tree_Controller.TOOLTIP_DESCRIPTION[MaximumElevation]", locale["Tree_Controller.TOOLTIP_DESCRIPTION[MaximumElevation]"])}
                >                    
                    <div className={styles.SliderFieldWidth}>
                        <SliderField value={props.entry.MaximumElevation-SeaLevel} min={-SeaLevel} max={MaxElevation-SeaLevel} fractionDigits={0} onChange={(e: number) => {changeValue("SetMaximumElevation", props.entry.Name ,e+SeaLevel)}}></SliderField>
                    </div>                    
                </EntrySectionComponent>
                
            </div>
        </div>
      );
}