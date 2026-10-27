#!/bin/bash
DOTNET="/Applications/Unity/Hub/Editor/6000.6.6f1/Unity.app/Contents/Resources/Scripting/DotNetSdk/dotnet"
CSC="/Applications/Unity/Hub/Editor/6000.6.6f1/Unity.app/Contents/Resources/Scripting/DotNetSdk/sdk/8.0.318/Roslyn/bincore/csc.dll"
NETSTANDARD="/Applications/Unity/Hub/Editor/6000.6.6f1/Unity.app/Contents/Resources/Scripting/DotNetSdk/packs/NETStandard.Library.Ref/2.1.0/ref/netstandard2.1"
MANAGED="/Applications/Unity/Hub/Editor/6000.6.6f1/Unity.app/Contents/Resources/Scripting/Managed/UnityEngine"
EDITOR_MANAGED="/Applications/Unity/Hub/Editor/6000.6.6f1/Unity.app/Contents/Resources/Scripting/Managed"
SCRIPT_ASSEMBLIES="/Users/sanishmaharjanengineering/Downloads/free/game/unity-client/Library/ScriptAssemblies"
NUNIT="/Applications/Unity/Hub/Editor/6000.6.6f1/Unity.app/Contents/Resources/PackageManager/BuiltInPackages/com.unity.ext.nunit/net472/unity-custom"

python3 -c '
import os
scripts = []
for root, dirs, files in os.walk("/Users/sanishmaharjanengineering/Downloads/free/game/unity-client/Assets/Scripts"):
    for f in files:
        if f.endswith(".cs"):
            scripts.append(f"\"{os.path.abspath(os.path.join(root, f))}\"")
with open("/tmp/sources.txt", "w") as out:
    out.write("\n".join(scripts) + "\n")
'

$DOTNET "$CSC" -target:library -out:/tmp/TestGorkha.dll -define:ENABLE_INPUT_SYSTEM -nowarn:0649 \
  -lib:"$NETSTANDARD","$MANAGED","$SCRIPT_ASSEMBLIES","$EDITOR_MANAGED","$NUNIT" \
  -r:netstandard.dll \
  -r:mscorlib.dll \
  -r:System.dll \
  -r:System.Core.dll \
  -r:System.Runtime.dll \
  -r:UnityEngine.dll \
  -r:UnityEngine.CoreModule.dll \
  -r:UnityEngine.IMGUIModule.dll \
  -r:UnityEngine.PhysicsModule.dll \
  -r:UnityEngine.AudioModule.dll \
  -r:UnityEngine.ParticleSystemModule.dll \
  -r:UnityEngine.AnimationModule.dll \
  -r:UnityEngine.ScriptingModule.dll \
  -r:UnityEngine.TextRenderingModule.dll \
  -r:UnityEngine.ImageConversionModule.dll \
  -r:UnityEngine.InputLegacyModule.dll \
  -r:UnityEngine.UI.dll \
  -r:UnityEngine.UIModule.dll \
  -r:Unity.InputSystem.dll \
  -r:UnityEditor.dll \
  -r:UnityEditor.CoreModule.dll \
  -r:Unity.Cinemachine.dll \
  -r:Unity.RenderPipelines.Universal.Runtime.dll \
  -r:Unity.RenderPipelines.Core.Runtime.dll \
  -r:Unity.RenderPipelines.GPUDriven.Runtime.dll \
  -r:Unity.TextMeshPro.dll \
  -r:UnityEngine.VFXModule.dll \
  -r:Unity.VisualEffectGraph.Runtime.dll \
  -r:nunit.framework.dll \
  @/tmp/sources.txt
