# Installer and uninstaller configuration
!define PRODUCT_NAME "VXMusic"
!define PRODUCT_VERSION "0.6.0.0"
!define PRODUCT_PUBLISHER "Virtual Xtensions"
!define PRODUCT_WEB_SITE "https://github.com/Soapwood"

# Set the relative path for the source files
!define SOURCE_PATH ".."

# Define the name of the installer and the output file
OutFile "VXMusicInstaller.exe"
InstallDir $PROGRAMFILES\VXMusic

# Installer sections
Section "MainSection" SEC01
  # Define the installation directory
  SetOutPath $INSTDIR
  # Include your files
  File /r "${SOURCE_PATH}\Publish\x64\*.*"
  # Create a shortcut on the Desktop
  CreateShortcut "$DESKTOP\VXMusic.lnk" "$INSTDIR\VXMusicDesktop.exe"
  
   # Write registry entries for Add/Remove Programs
   WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" "DisplayName" "${PRODUCT_NAME}"
   WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" "UninstallString" "$INSTDIR\UninstallVXMusic.exe"
   WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" "InstallLocation" "$INSTDIR"
   WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" "DisplayIcon" "$INSTDIR\VXMusicDesktop.exe"
   WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" "Publisher" "${PRODUCT_PUBLISHER}"
   WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" "DisplayVersion" "${PRODUCT_VERSION}"
   WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
   WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" "NoModify" 1
   WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" "NoRepair" 1
   
   # Create an uninstaller
   WriteUninstaller "$INSTDIR\UninstallVXMusic.exe"
SectionEnd

# Uninstaller section
Section "Uninstall"
  # Remove files
  Delete "$INSTDIR\*.*"
  # Remove the installation directory
  RMDir /r "$INSTDIR"
  # Remove the desktop shortcut
  Delete "$DESKTOP\VXMusicInstaller.lnk"
  
  # Remove registry entries
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
SectionEnd

# MUI (Modern User Interface) configuration
!include MUI2.nsh

Caption "VXMusic Installer"

# MUI Settings
!define MUI_ABORTWARNING
!define MUI_ICON "${SOURCE_PATH}\VXMusicDesktop\Images\VXLogoIcon.ico"
!define MUI_UNICON "${SOURCE_PATH}\VXMusicDesktop\Images\VXLogoIcon.ico"

!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "${SOURCE_PATH}\VXMusicDesktop\Images\InstallerBranding.bmp"
!define MUI_HEADERIMAGE_UNBITMAP "${SOURCE_PATH}\VXMusicDesktop\Images\InstallerBranding.bmp"
!define MUI_HEADERIMAGE_BITMAP_STRETCH AspectFitHeight
!define MUI_HEADERIMAGE_UNBITMAP_STRETCH AspectFitHeight

!define MUI_BGCOLOR 1F212D
!define MUI_TEXTCOLOR FFFFFF
!define MUI_INSTFILESPAGE_COLORS "FFFFFF 1F212D"
!define MUI_FINISHPAGE_LINK_COLOR FFFFFF

# Welcome page settings
!define MUI_WELCOMEPAGE_TITLE ""
!define MUI_WELCOMEFINISHPAGE_TITLE "Welcome to the VXMusic Setup Wizard"
!define MUI_WELCOMEFINISHPAGE_TEXT "This wizard will guide you through the installation of VXMusic. Click Next to continue."
!define MUI_WELCOMEFINISHPAGE_BITMAP "${SOURCE_PATH}\VXMusicDesktop\Images\InstallerBranding.bmp"
!define MUI_WELCOMEFINISHPAGE_BITMAP_NOSTRETCH

# Welcome page settings
#!define MUI_WELCOMEFINISHPAGE_BITMAP "${SOURCE_PATH}\VXMusicDesktop\Images\InstallerBranding.bmp"
#!define MUI_WELCOMEFINISHPAGE_TEXT "Welcome to VXMusic. Please follow the instructions to install VXMusic on your computer."

# Finish page settings
!define MUI_FINISHPAGE_TITLE "Installation Complete"
!define MUI_UNWELCOMEFINISHPAGE_TITLE "Installation Complete"
!define MUI_UNWELCOMEFINISHPAGE_TEXT "VXMusic has been successfully installed on your computer. Click Finish to exit the installer."
!define MUI_UNWELCOMEFINISHPAGE_BITMAP "${SOURCE_PATH}\VXMusicDesktop\Images\InstallerBranding.bmp"
!define MUI_UNWELCOMEFINISHPAGE_UNBITMAP "${SOURCE_PATH}\VXMusicDesktop\Images\InstallerBranding.bmp"
!define MUI_UNWELCOMEFINISHPAGE_BITMAP_NOSTRETCH

!define MUI_PRODUCT "VXMusic"
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "${SOURCE_PATH}\license.txt"
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_INSTFILES

# Language files
!insertmacro MUI_LANGUAGE "English"
# Version information
VIProductVersion "${PRODUCT_VERSION}"
VIAddVersionKey "ProductName" "${PRODUCT_NAME}"
VIAddVersionKey "CompanyName" "${PRODUCT_PUBLISHER}"
VIAddVersionKey "FileDescription" "${PRODUCT_NAME}"
VIAddVersionKey "FileVersion" "${PRODUCT_VERSION}"
VIAddVersionKey "LegalCopyright" "Copyright (C) 2024"
VIAddVersionKey "ProductVersion" "${PRODUCT_VERSION}"
VIAddVersionKey "OriginalFilename" "$$"
VIAddVersionKey "Comments" "${PRODUCT_WEB_SITE}"