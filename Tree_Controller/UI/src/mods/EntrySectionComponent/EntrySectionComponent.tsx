import classNames from "classnames";
import { Tooltip } from "cs2/ui";
import styles from "./EntrySectionComponent.module.scss";
import { getModule } from "cs2/modding";

type EntrySectionProps = {
    title?: string | null
    tooltip? : string | JSX.Element | null
    children? : JSX.Element | JSX.Element[]
}

const SectionStyle : any = getModule("game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.module.scss", "classes")

export const EntrySectionComponent = (props : EntrySectionProps) => {
    return (
        <div className={classNames(SectionStyle.item)}>
            <Tooltip tooltip={props.tooltip}>
                <div className={classNames(SectionStyle.label)}>{props.title}</div>
            </Tooltip>
            <div className={SectionStyle.content}>{props.children}</div>
        </div>
    );
}