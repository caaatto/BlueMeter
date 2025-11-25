# Release Notes - Installer & Offline Usage Improvements

## Fixed Issues

### 1. Installer No Longer Attempts to Re-Install Npcap

**Problem**: The installer tried to download and install Npcap even when it was already installed on the system.

**Root Cause**: The installer only checked for Npcap in `HKLM\SOFTWARE\Npcap`, but some installations may register in different locations (32-bit registry on 64-bit systems, legacy WinPcap registrations, or only through installed DLLs).

**Solution**: Enhanced Npcap detection to check multiple locations:
- 64-bit registry: `HKLM\SOFTWARE\Npcap`
- 32-bit registry (on 64-bit systems): `HKLM\SOFTWARE\WOW6432Node\Npcap`
- Legacy WinPcap registry: `HKLM\SOFTWARE\WinPcap`
- DLL in System32: `%SystemRoot%\System32\Npcap\wpcap.dll`
- Legacy DLL: `%SystemRoot%\System32\wpcap.dll`

**File Changed**: `setup/CodeDependencies.iss:778-827`

### 2. New Pre-Built Portable Version for Offline/VM Usage

**Problem**: The portable version (`BlueMeter-vX.X.X-windows-x64.zip`) required building from source, which needs internet connection to download NuGet packages. This was a blocker for users wanting to run BlueMeter in isolated VMs or offline environments.

**Error Encountered**:
```
error NU1301: Unable to load the service index for source https://api.nuget.org/v3/index.json
```

**Solution**: Created a new build script (`create-portable-release.bat`) that generates a truly portable, pre-built version:
- Self-contained with embedded .NET 8.0 runtime
- No building required by end user
- No internet connection needed (after initial download)
- Includes simple `BlueMeter.bat` launcher
- Only requires Npcap (one-time installation)

**Files Added**:
- `create-portable-release.bat` - Script to create portable releases
- Future releases will include `BlueMeter-vX.X.X-portable.zip` package

### 3. Documentation Improvements

**Added**:
- Clear distinction between developer portable version (requires building) and pre-built portable version (ready to run)
- Dedicated "Offline & VM Usage" section in README
- Step-by-step guide for using BlueMeter in isolated/offline environments
- Security testing use case documentation
- Internet requirement warnings where applicable

**Files Changed**:
- `README.md:47-147`

## Usage Recommendations

### For Most Users
Use **Option 1: Professional Installer** - Automatically handles all dependencies including .NET 8.0 and Npcap.

### For Developers
Use **Option 2: Portable Build (For Developers)** - Build from source with `setup.bat`. Requires internet for initial NuGet restore.

### For Offline/VM/Security Testing
Use **Option 3: Pre-Built Portable** - Download once, run anywhere. Perfect for:
- Testing in VMs with limited network access
- Secure/isolated environments
- Offline usage scenarios
- Security auditing/testing

## Building the Pre-Built Portable Version

Maintainers can create the portable release using:
```bash
.\create-portable-release.bat
```

This will:
1. Build a self-contained Release with embedded .NET runtime
2. Create a simple `BlueMeter.bat` launcher
3. Include usage documentation
4. Generate a ZIP file ready for distribution

Output location: `release-portable/BlueMeter-vX.X.X-portable.zip`

## Testing Notes

To verify the installer fix:
1. Install Npcap manually from npcap.com
2. Run the installer - it should detect existing Npcap installation
3. Installer should skip Npcap download/installation step

To verify the portable version:
1. Build portable release with `create-portable-release.bat`
2. Copy to a machine without .NET installed
3. Ensure Npcap is installed
4. Run `BlueMeter.bat` - should launch without any errors

## Feedback from Community

This fix addresses feedback from Reddit user Neynae:
- Installer attempting to download Npcap when already installed ✅ FIXED
- Portable version requiring internet for NuGet packages ✅ FIXED with new pre-built option
- Offline VM usage scenario ✅ DOCUMENTED with dedicated guide

## Next Steps

For the next release:
1. Test the enhanced Npcap detection on various Windows configurations
2. Generate and upload the pre-built portable ZIP to Releases
3. Consider adding a checkbox in installer to skip Npcap installation (even if not detected)
