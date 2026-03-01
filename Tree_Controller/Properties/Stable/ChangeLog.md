# Patch v.1.7.1
* Reinstated Disable Tree Growth Setting. Now called Tree Growth Paused. Changed description. Added Pause and Resume buttons with warnings and confirmations.
* Tree Growth Paused setting now when toggled pauses all tree aging by enabling vanilla Decoration component on all trees other than Lumber.
* When Tree Growth is resumed or settings are reset, existing trees are not effected and will continue to have paused tree aging. Use Tree Controller Tool to resume aging for trees.
* Tree Controller tools can now enable/disable vanilla Decoration component depending on Preserve Age toggle without changing a trees age or prefab.
* Tree Controller Tool no longer enable/disable vanilla Decoration component for Lumber.
* Fixed Lumber system to detect trees with enabled Decoration component.
* Limited Tree Anarchy retired. Vanilla makes it essentially redundant, and vanilla is reliable.
* Preserve Age Toggle state saved as hidden setting so it will have same toggle state on reload.
* Updated Localization.