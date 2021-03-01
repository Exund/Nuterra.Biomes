# Nuterra.Biomes
A custom biome injection mod

# Docs
## Examples
You can find examples for each object type in the ["Examples"](Nuterra.Biomes/Examples) folder.

Each object type has its own JSON extension
- BiomeGroup: `*.group.json`
- Biome: `*.biome.json`
- MapGenerator: `*.generator.json`
- TerrainLayer: `*.layer.json`

Similarly to the Block Injector, the folder hierarchy is not important and assets are referenced by name (either filename for texture or the "name" field in JSON for objects) so just be sure to have a unique name for your assets.

Custom biomes files need to go in the `Custom Biomes` folder inside the TerraTech installation folder (will be automatically created when starting the game with the mod enabled)

## Visual Studio Code JSON Schemas config (JSON auto-completion)
In Visual Studio Code in the menubar go to `File > Preferences > Settings` <br/>
Then when in the settings go to `Extensions > JSON` <br/>
Then search for `Schemas` and click `Edit in settings.json` <br/>
Then copy/paste this at the end: <br/>

```json
"json.schemas": [
    {
        "fileMatch": [
            "*.biome.json"
        ],
        "url": "https://raw.githubusercontent.com/Exund/Nuterra.Biomes/master/SCHEMAS/Biome.schema.json"
    },
    {
        "fileMatch": [
            "*.generator.json"
        ],
        "url": "https://raw.githubusercontent.com/Exund/Nuterra.Biomes/master/SCHEMAS/MapGenerator.schema.json"
    },
    {
        "fileMatch": [
            "*.layer.json"
        ],
        "url": "https://raw.githubusercontent.com/Exund/Nuterra.Biomes/master/SCHEMAS/TerrainLayer.schema.json"
    },
    {
        "fileMatch": [
            "*.group.json"
        ],
        "url": "https://raw.githubusercontent.com/Exund/Nuterra.Biomes/master/SCHEMAS/BiomeGroup.schema.json"
    }
]
```