/*
import { Button, Panel, Portal } from "cs2/ui";
import styles from "./partialAnarchyMenu.module.scss"
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import { bindValue, trigger, useValue } from "cs2/api";
import mod from "../../../mod.json";
import { getModule } from "cs2/modding";
import { game } from "cs2/bindings";
import { useLocalization } from "cs2/l10n";
import classNames from "classnames";

const uilStandard =                         "coui://uil/Standard/";

const closeSrc =         uilStandard +  "XClose.svg";
const arrowUpSrc =           uilStandard +  "ArrowUpThickStroke.svg";

const ShowPanel$ = bindValue<boolean>(mod.id, "ShowForestBrushPanel");

function handleClick(event: string) {
    trigger(mod.id, event);
}

const roundButtonHighlightStyle = getModule("game-ui/common/input/button/themes/round-highlight-button.module.scss", "classes");

export const ForestBrushMenuComponent = () => {
    const ShowPanel = useValue(ShowPanel$);
    const isPhotoMode = useValue(game.activeGamePanel$)?.__Type == game.GamePanelType.PhotoMode;
    const { translate } = useLocalization();

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
                        <div className={styles.rowGroup}>
                            
                        </div>
                    </Panel>
                </Portal>
            )}
        </>
    );
}*/
export{}