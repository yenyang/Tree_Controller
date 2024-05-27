import { getModule } from "cs2/modding";
import { Theme, FocusKey, UniqueFocusKey, Color } from "cs2/bindings";
import { bindValue, trigger, useValue } from "cs2/api";
import { ColorSet } from "mods/Domain/ColorSet";
import { useLocalization } from "cs2/l10n";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import { useState } from "react";

interface InfoSectionComponent {
	group: string;
	tooltipKeys: Array<string>;
	tooltipTags: Array<string>;
}

const uilStandard =                          "coui://uil/Standard/";
const uilColored =                           "coui://uil/Colored/";
const saveSrc =                     uilStandard +  "DiskSave.svg";

const CurrentColorSet$ = bindValue<ColorSet>(mod.id, "CurrentColorSet");

const InfoSectionTheme: Theme | any = getModule(
	"game-ui/game/components/selected-info-panel/shared-components/info-section/info-section.module.scss",
	"classes"
);

const InfoRowTheme: Theme | any = getModule(
	"game-ui/game/components/selected-info-panel/shared-components/info-row/info-row.module.scss",
	"classes"
)

const InfoSection: any = getModule( 
    "game-ui/game/components/selected-info-panel/shared-components/info-section/info-section.tsx",
    "InfoSection"
)

const InfoRow: any = getModule(
    "game-ui/game/components/selected-info-panel/shared-components/info-row/info-row.tsx",
    "InfoRow"
)

function handleClick(eventName : string) {
    // This triggers an event on C# side and C# designates the method to implement.
    trigger(mod.id, eventName);
}

function changeColor(channel : number, newColor : Color) {
    // This triggers an event on C# side and C# designates the method to implement.
    trigger(mod.id, "ChangeColor", channel, newColor);
}

const FocusDisabled$: FocusKey = getModule(
	"game-ui/common/focus/focus-key.ts",
	"FOCUS_DISABLED"
);

export const SIPcolorFieldsComponent = (componentList: any): any => {
    // I believe you should not put anything here.
	componentList["Tree_Controller.Systems.SelectedInfoPanelColorFieldsSystem"] = (e: InfoSectionComponent) => {
        // These get the value of the bindings.
        const CurrentColorSet = useValue(CurrentColorSet$);

        let [colorField0, updateColorField0] = useState<Color>(CurrentColorSet.Channel0);
        let [colorField1, updateColorField1] = useState<Color>(CurrentColorSet.Channel1);
        let [colorField2, updateColorField2] = useState<Color>(CurrentColorSet.Channel2);
        // translation handling. Translates using locale keys that are defined in C# or fallback string here.
        const { translate } = useLocalization();

        return 	<InfoSection focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED} disableFocus={true} className={InfoSectionTheme.infoSection}>
                        <InfoRow 
                            left={"Tree Controller"}
                            right={"Custom Color Variations"}
                            tooltip={"For choosing custom seasonal foliage colors."}
                            uppercase={true}
                            disableFocus={true}
                            subRow={false}
                            className={InfoRowTheme.infoRow}
                        ></InfoRow>
                        <InfoRow 
                            left={"Channel0"}
                            right=
                            {
                                <VanillaComponentResolver.instance.ColorField value={colorField0} onChange={(e) => {updateColorField0(e); changeColor(0, e);}}/>
                            }
                            uppercase={false}
                            disableFocus={true}
                            subRow={true}
                            className={InfoRowTheme.infoRow}
                        ></InfoRow>
                        <InfoRow 
                            left={"Channel1"}
                            right=
                            {
                                <VanillaComponentResolver.instance.ColorField value={colorField1} onChange={(e) => {updateColorField1(e); changeColor(1, e);}}/>
                            }
                            uppercase={false}
                            disableFocus={true}
                            subRow={true}
                            className={InfoRowTheme.infoRow}
                        ></InfoRow>
                        <InfoRow 
                            left={"Channel2"}
                            right=
                            {
                                <VanillaComponentResolver.instance.ColorField value={colorField2} onChange={(e) => {updateColorField2(e); changeColor(2, e);}}/>
                            }
                            uppercase={false}
                            disableFocus={true}
                            subRow={true}
                            className={InfoRowTheme.infoRow}
                        ></InfoRow>
                        <InfoRow 
                            left={"Save"}
                            right=
                            {
                                <VanillaComponentResolver.instance.ToolButton
                                    src={saveSrc}
                                    multiSelect = {false}   // I haven't tested any other value here
                                    disabled = {false}      // I haven't tested any other value here
                                    tooltip = {"Tooltip"}
                                    className = {VanillaComponentResolver.instance.toolButtonTheme.button}
                                    onSelect={() => handleClick("SaveColorSet")}
                                />
                            }
                            uppercase={false}
                            disableFocus={true}
                            subRow={true}
                            className={InfoRowTheme.infoRow}
                        ></InfoRow>
                </InfoSection>
				;
    }

	return componentList as any;
}

/*
      var t = e.icon
              , n = e.left
              , r = e.right
              , i = e.tooltip
              , o = e.link
              , a = e.uppercase
              , s = void 0 !== a && a
              , l = e.subRow
              , c = void 0 !== l && l
              , u = e.disableFocus
              , d = void 0 !== u && u
              , f = e.className;
              /*
/*


                        /*
/*
let VS = {
            "info-row": "info-row_QQ9 item-focused_FuT",
            infoRow: "info-row_QQ9 item-focused_FuT",
            "disable-focus-highlight": "disable-focus-highlight_I85",
            disableFocusHighlight: "disable-focus-highlight_I85",
            link: "link_ICj",
            tooltipRow: "tooltipRow_uIh",
            left: "left_RyE",
            hasIcon: "hasIcon_iZ3",
            right: "right_ZUb",
            icon: "icon_ugE",
            uppercase: "uppercase_f0y",
            subRow: "subRow_NJI"
        };

		 let fS = {
            "info-section": "info-section_I7V",
            infoSection: "info-section_I7V",
            content: "content_Cdk item-focused_FuT",
            column: "column_aPB",
            divider: "divider_rfM",
            "no-margin": "no-margin_K7I",
            noMargin: "no-margin_K7I",
            "disable-focus-highlight": "disable-focus-highlight_ik3",
            disableFocusHighlight: "disable-focus-highlight_ik3",
            "info-wrap-box": "info-wrap-box_Rt4",
            infoWrapBox: "info-wrap-box_Rt4"
        };
		*/