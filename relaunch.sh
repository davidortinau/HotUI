#!/bin/bash
# Kill any running instance, clean everything, and rebuild+launch the Controls Gallery
pkill -f "CometControlsGallery" 2>/dev/null
pkill -f "Comet Controls Gallery" 2>/dev/null
sleep 1

# Clean all build outputs
rm -rf sample/CometControlsGallery/bin sample/CometControlsGallery/obj
rm -rf src/Comet/bin src/Comet/obj
rm -rf src/Comet.SourceGenerator/bin src/Comet.SourceGenerator/obj

# Build source generator first, then the app
dotnet build src/Comet.SourceGenerator/Comet.SourceGenerator.csproj -c Debug
dotnet build sample/CometControlsGallery/CometControlsGallery.csproj -f net10.0-maccatalyst -c Debug

# Launch it (app name has spaces)
open "sample/CometControlsGallery/bin/Debug/net10.0-maccatalyst/maccatalyst-arm64/Comet Controls Gallery.app"
