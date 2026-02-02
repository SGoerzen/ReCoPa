# 1) Publish
dotnet publish ReCoPa.Desktop/ReCoPa.Desktop.csproj -c Release -r osx-arm64

# 2) .app Struktur anlegen
PUBLISH_DIR="ReCoPa.Desktop/bin/Release/net10.0/osx-arm64/publish"
APP_NAME="ReCoPa"
APP_DIR="dist/$APP_NAME.app"

mkdir -p "dist"

mkdir -p "$APP_DIR/Contents/MacOS"
mkdir -p "$APP_DIR/Contents/Resources"

# 3) Binary & DLLs kopieren
cp "$PUBLISH_DIR/ReCoPa.Desktop" "$APP_DIR/Contents/MacOS/$APP_NAME"
chmod +x "$APP_DIR/Contents/MacOS/$APP_NAME"

# Kopiere den Rest daneben (dlls, deps, runtimeconfig, native libs)
cp "$PUBLISH_DIR"/*.dll "$APP_DIR/Contents/MacOS/" 2>/dev/null || true
cp "$PUBLISH_DIR"/*.json "$APP_DIR/Contents/MacOS/" 2>/dev/null || true
cp "$PUBLISH_DIR"/*.dylib "$APP_DIR/Contents/MacOS/" 2>/dev/null || true

# 4) Info.plist erstellen
cat > "$APP_DIR/Contents/Info.plist" <<'PLIST'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
  <dict>
    <key>CFBundleName</key><string>ReCoPa</string>
    <key>CFBundleDisplayName</key><string>ReCoPa</string>
    <key>CFBundleIdentifier</key><string>de.goerzen.recopa</string>
    <key>CFBundleVersion</key><string>2.0.0</string>
    <key>CFBundleShortVersionString</key><string>2.0.0</string>
    <key>CFBundlePackageType</key><string>APPL</string>
    <key>CFBundleExecutable</key><string>ReCoPa</string>
    <key>LSMinimumSystemVersion</key><string>12.0</string>
    <key>NSHighResolutionCapable</key><true/>
    <key>CFBundleIconFile</key><string>ReCoPa</string>
  </dict>
</plist>
PLIST

# 5) Icon
cp ReCoPa/Assets/app-icon.icns "$APP_DIR/Contents/Resources/ReCoPa.icns"

# 6) Starten
echo "Open $APP_DIR"
open "$APP_DIR"
