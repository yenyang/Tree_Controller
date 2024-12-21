# Patch v1.6.0
* Added ability to select tree ages while plopping objects with tree subobjects such as buildings and round-a-bouts.
* Added Dead trees back to age selection with object tool, vanilla and modded line tool. (Does not work with street tree placement).
* Added Free Trees and Vegetation option. (Default Off)
* Added option to add Stumps to age selection with object tool, vanilla and modded line tool, and tree changing tools. (Default Off) (Does not work with street trees placement).
* Added option to constrain brush while adding trees and vegetation with object tool brush mode. (Default On).
* Age randomization is now tied to RandomSeeds and behaves more like current vanilla than old versions of Tree Controller.
* Added new random seeds to tree subobjects so trees inside a building will no longer have exactly same colors and their ages can be randomized.
* Added option for Faster Max Brush strength (default off).
* Added Limited Tree Anarchy option. Conflict checks approximately based on trunk instead of drip line. (default off). Be careful with this if settings are reset. But you can bring them back with Anarchy component tool if something disappears from being overriden.
* Added Advanced Set Control Panel that lets you set Age overrides, Probability weights, and minimum and maximum elevations for each prefab in a set. Age overrides and Probability weights work with object tool and line tools. Minimum and maximum elevations are only for brushing objects. 
* Sets can also be selected with single object placement.