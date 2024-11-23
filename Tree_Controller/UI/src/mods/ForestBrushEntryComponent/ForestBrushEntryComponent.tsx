
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
import { Ages, childSrc, teenSrc, adultSrc, elderlySrc, deadSrc, stumpSrc, clearAgesSrc, descriptionTooltip } from "mods/TreeControllerSections/treeControllerSections";
import locale from "../lang/en-US.json";

const uilStandard =                         "coui://uil/Standard/";
const minusSrc =            uilStandard + "Minus.svg"

const descriptionToolTipStyle = getModule("game-ui/common/tooltip/description-tooltip/description-tooltip.module.scss", "classes");

const SliderField : any = getModule("game-ui/editor/widgets/fields/number-slider-field.tsx", "FloatSliderField");
    
export const ForestBrushEntryComponent = (props: { entry : AdvancedForestBrushEntry }) => {
    const { translate } = useLocalization();

    const childTooltipTitle = translate("ToolOptions.TOOLTIP_DESCRIPTION[Sapling]");
    const teenTooltipTitle = translate("ToolOptions.TOOLTIP_DESCRIPTION[Young]");
    const adultTooltipTitle = translate("ToolOptions.TOOLTIP_DESCRIPTION[Mature]");
    const elderlyTooltipTitle = translate("ToolOptions.TOOLTIP_DESCRIPTION[Elderly]");
    const deadTooltipTitle = translate("YY_TREE_CONTROLLER[dead]",locale["YY_TREE_CONTROLLER[dead]"]);    
    const clearAgeTooltipTitle = translate("YY_TREE_CONTROLLER[clear-ages]",locale["YY_TREE_CONTROLLER[clear-ages]"]);
    const clearAgeTooltipDescription = translate("YY_TREE_CONTROLLER_DESCRIPTION[clear-ages]", locale["YY_TREE_CONTROLLER_DESCRIPTION[clear-ages]"]);

    let [seletectedAges, changeSelectedAge] = useState<Ages>(props.entry.Ages);
    let [sliderMinElev, changeMinElevation] = useState<Number>(props.entry.MinimumElevation);    
    let [sliderMaxElev, changeMaxElevation] = useState<Number>(props.entry.MaximumElevation);    
    let [sliderProbWeight, changeProbabilityWeight] = useState<Number>(props.entry.ProbabilityWeight);

    return (
        <div className={classNames(styles.rowGroup)}>
            <div className={classNames(styles.columnGroup, styles.centered)}>
                <Tooltip tooltip={"Hello"}>
                    <div className={VanillaComponentResolver.instance.assetGridTheme.item}>
                        <img src={props.entry.Src} className={VanillaComponentResolver.instance.assetGridTheme.thumbnail}></img>
                    </div>
                </Tooltip>
                <div className={classNames(styles.rowGroup, styles.centered)}>{translate(props.entry.LocaleKey)}</div>
                <div className={classNames(styles.rowGroup, styles.centered)}>
                    <VanillaComponentResolver.instance.ToolButton  tooltip={"Remove"}      onSelect={() => {}}        src={minusSrc}       focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>  
                </div>
            </div>
            <div className={styles.columnGroup}>
                <VanillaComponentResolver.instance.Section title={translate("YY_TREE_CONTROLLER[Age]",locale["YY_TREE_CONTROLLER[Age]"])}>
                    <VanillaComponentResolver.instance.ToolButton  selected={(seletectedAges & Ages.All) == Ages.All}         tooltip={descriptionTooltip(clearAgeTooltipTitle, clearAgeTooltipDescription)}      onSelect={() => changeSelectedAge(Ages.All)}        src={clearAgesSrc}       focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                    <VanillaComponentResolver.instance.ToolButton  selected={(seletectedAges & Ages.Child) == Ages.Child}     tooltip={childTooltipTitle}            onSelect={() => changeSelectedAge(Ages.Child)}      src={childSrc}           focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                    <VanillaComponentResolver.instance.ToolButton  selected={(seletectedAges & Ages.Teen) == Ages.Teen}       tooltip={teenTooltipTitle}              onSelect={() => changeSelectedAge(Ages.Teen)}       src={teenSrc}            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                    <VanillaComponentResolver.instance.ToolButton  selected={(seletectedAges & Ages.Adult) == Ages.Adult}     tooltip={adultTooltipTitle}            onSelect={() => changeSelectedAge(Ages.Adult)}      src={adultSrc}           focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                    <VanillaComponentResolver.instance.ToolButton  selected={(seletectedAges & Ages.Elderly) == Ages.Elderly} tooltip={elderlyTooltipTitle}        onSelect={() => changeSelectedAge(Ages.Elderly)}    src={elderlySrc}         focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                    <VanillaComponentResolver.instance.ToolButton  selected={(seletectedAges & Ages.Dead) == Ages.Dead}       tooltip={deadTooltipTitle}              onSelect={() => changeSelectedAge(Ages.Dead)}       src={deadSrc}            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
                </VanillaComponentResolver.instance.Section>
                <VanillaComponentResolver.instance.Section title={"Probability Weight"}>
                    <div className={styles.SliderFieldWidth}>
                        <SliderField value={sliderProbWeight} min={1} max={200} fractionDigits={0} onChange={(e: number) => {changeProbabilityWeight(e)}}></SliderField>
                    </div>
                </VanillaComponentResolver.instance.Section>
                <VanillaComponentResolver.instance.Section title={"Minimum Elevation"}>
                    <div className={styles.SliderFieldWidth}>
                        <SliderField value={sliderMinElev} min={0} max={4000} fractionDigits={0} onChange={(e: number) => {changeMinElevation(e)}}></SliderField>
                    </div>
                </VanillaComponentResolver.instance.Section>
                <VanillaComponentResolver.instance.Section title={"Maximum Elevation"}>
                    <div className={styles.SliderFieldWidth}>
                        <SliderField value={sliderMaxElev} min={0} max={4000} fractionDigits={0} onChange={(e: number) => {changeMaxElevation(e)}}></SliderField>
                    </div>
                </VanillaComponentResolver.instance.Section>
            </div>
        </div>
      );
}