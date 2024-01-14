# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Ability to create Network Variables specific to a Network Object
- `[PublicNetworkVariable]` Attribute

### Changed
- Network Variables
  - Ownership
    - By default, the server is now the owner.
    - Using the `[PublicNetworkVariable]` Attribute, you can allow any client to modify the variable.
    - Client-owned variables can only be made by creating one specific to a Network Object.
      - A great choice is `PlayerControllerB`

### Removed
- LethalNetworkVariable.SetOwnership()
  - Removed in favor of the newly reworked Network Variable ownership system.

## [v1.1.3]

### Changed
- Updated NuGet Package License

## [v1.1.2]

### Changed
- Updated Changelog

## [v1.1.1]

### Changed
- Internal GitHub Workflow Shenanigans

## [v1.1.0]

### Changed
- Backend NetworkHandler Rework
  - May not be compatible with v1.0.0

## [v1.0.0] - ***Initial Release!***

### Added
- Network Messages
- Network Events
- Network Variables
- Network Utils/Extensions