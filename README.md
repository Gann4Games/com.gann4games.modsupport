# Install
Copy the git url and paste it into the package manager
1. Open the package manager window
2. Click on the + button
3. Click on "Add package from git url..."
4. Paste `https://github.com/gann4life/com.gann4games.modsupport.git`

# Profile Settings
* Local.BuildPath: `[UnityEngine.Application.dataPath]/Mods/MOD_NAME/[BuildTarget]`
* Local.LocalPath: `{UnityEngine.Application.dataPath}/Mods/MOD_NAME/[BuildTarget]`

It is important now to keep MOD_NAME in the profiles since this is the way I found to load bundles properly.
You can rename the folder to whatever you want, this data is only important to keep inside the catalog (json) file.
The reason is that, when loading a mod, it searches for the MOD_NAME tag and replaces that string by the actual mod name.