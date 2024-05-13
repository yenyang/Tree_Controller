import { Portal, Panel } from "cs2/ui"
import styles from "./colorPicker.module.scss"
import { getModule } from "cs2/modding"
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver"
import { Theme } from "cs2/bindings"

const ColorPicker: any = getModule( 
    "game-ui/common/input/color-picker/color-picker/color-picker.tsx",
    "ColorPicker"
)

const ColorFieldTheme: Theme | any = getModule(
	"game-ui/common/input/color-picker/color-field/color-field.module.scss",
	"classes"
)


export const ColorPickerComponent = () => {
    return ( 
        <>
        <Portal>
            <Panel
                className = {styles.panel}
                header={(
                    <div className={styles.header}>
                        {"Seasonal Color Picker"}
                    </div>
                )}>
                <div className={styles.panelSection}>
                    <ColorPicker
                        focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                        color={}
                        className={ColorFieldTheme.colorField}
                        alpha={false}
                    >
                        
                    </ColorField>
                </div>
            </Panel>
        </Portal>
        </>
    )
}
/*
 t = e.focusKey
              , n = e.color
              , r = e.alpha
              , i = e.colorWheel
              , o = void 0 === i || i
              , a = e.sliderTextInput
              , s = void 0 === a || a
              , l = e.preview
              , c = void 0 === l ? Zq.None : l
              , u = e.mode
              , d = e.hexInput
              , f = void 0 === d || d
              , m = e.onChange
*/