; BlueMeter Installer Script (NSIS)
; This script creates a professional Windows installer for BlueMeter

!include "MUI2.nsh"
!include "x64.nsh"

; Basic Installer Info
Name "BlueMeter"
OutFile "BlueMeter-Setup-1.2.1.exe"
InstallDir "$PROGRAMFILES\BlueMeter"

; Installer Appearance
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_LANGUAGE "English"

; Installer Sections
Section "Install BlueMeter"
    SetOutPath "$INSTDIR"

    ; Copy application files
    File /r "BlueMeter.WPF\bin\Release\net8.0-windows\*.*"

    ; Create Start Menu shortcuts
    CreateDirectory "$SMPROGRAMS\BlueMeter"
    CreateShortcut "$SMPROGRAMS\BlueMeter\BlueMeter.lnk" "$INSTDIR\BlueMeter.WPF.exe"
    CreateShortcut "$SMPROGRAMS\BlueMeter\Uninstall.lnk" "$INSTDIR\uninstall.exe"

    ; Create Desktop shortcut
    CreateShortcut "$DESKTOP\BlueMeter.lnk" "$INSTDIR\BlueMeter.WPF.exe"

    ; Write uninstaller
    WriteUninstaller "$INSTDIR\uninstall.exe"

    ; Write registry entries for Add/Remove Programs
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\BlueMeter" \
                     "DisplayName" "BlueMeter"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\BlueMeter" \
                     "UninstallString" "$INSTDIR\uninstall.exe"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\BlueMeter" \
                     "DisplayVersion" "1.2.1"
SectionEnd

; Uninstaller Section
Section "Uninstall"
    Delete "$SMPROGRAMS\BlueMeter\BlueMeter.lnk"
    Delete "$SMPROGRAMS\BlueMeter\Uninstall.lnk"
    RMDir "$SMPROGRAMS\BlueMeter"
    Delete "$DESKTOP\BlueMeter.lnk"
    RMDir /r "$INSTDIR"

    DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\BlueMeter"
SectionEnd
