# Creating a Release

This document explains how to create a new release of BlueMeter.

## Automatic Release Process

The project uses GitHub Actions to automatically build and publish releases when you create a version tag.

### Steps to Create a Release:

1. **Commit all changes and push to main branch**
   ```bash
   git add .
   git commit -m "Prepare for release vX.X.X"
   git push origin main
   ```

2. **Create and push a version tag**
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

3. **Wait for GitHub Actions to complete**
   - Go to https://github.com/caaatto/BlueMeter/actions
   - Wait for the "Create Release" workflow to finish
   - This will automatically create a release with the compiled ZIP file

4. **Edit the release on GitHub (optional)**
   - Go to https://github.com/caaatto/BlueMeter/releases
   - Click "Edit" on the newly created release
   - Add detailed changelog or release notes
   - Click "Update release"

## Version Numbering

Follow [Semantic Versioning](https://semver.org/):
- **MAJOR** version (v2.0.0): Breaking changes
- **MINOR** version (v1.1.0): New features, backwards compatible
- **PATCH** version (v1.0.1): Bug fixes, backwards compatible

## What Gets Released

The release workflow automatically:
- Builds the project in Release configuration
- Publishes the WPF application with all dependencies
- Creates a ZIP file: `BlueMeter-vX.X.X-windows-x64.zip`
- Uploads it to GitHub Releases

## Manual Release (if needed)

If you need to create a release manually:

```bash
# Build and publish
dotnet publish BlueMeter.WPF/BlueMeter.WPF.csproj -c Release -o publish

# Create ZIP
cd publish
Compress-Archive -Path * -DestinationPath ../BlueMeter-v1.0.0-windows-x64.zip

# Upload manually to GitHub Releases page
```
