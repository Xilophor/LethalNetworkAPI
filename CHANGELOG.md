# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [v3.0.2]

### Fixed
- Some built-in types not serializing properly.
- NGO `InvalidOperationException` during disconnecting if a `LethalNetworkVariable` is used.

## [v3.0.1]

### Fixed
- `InvalidOperationException: Invalid binary data stream` error. 

## [v3.0.0]

### Added
- `LNetworkMessage`
  -  Simplifies sending/receiving messages between the server and clients by using a single class instead of multiple classes
  -  Replaces `LethalServerMessage` and `LethalClientMessage`
- `LNetworkEvent`
  -  Simplifies sending/receiving events between the server and clients by using a single class instead of multiple classes
  -  Replaces `LethalServerEvent` and `LethalClientEvent`
- Added `LNetworkUtils`

### Changed
- Reworked the entire API
  -  Now uses Unity's new `CustomMessagingManager` to send messages
    -  This allows for vanilla compatibility, though it is not recommended
  -  Added `LNetworkMessage`
  -  Added `LNetworkEvent`

### Deprecated
- The `LethalClientMessage` and `LethalServerMessage` classes
  -  Use `LNetworkMessage` instead
- The `LethalClientEvent` and `LethalServerEvent` classes
  -  Use `LNetworkEvent` instead
- The `LethalNetworkVariable` class
  -  Currently, there is no replacement for this class. If you need to use it immediately, this class still will work but will receive no updates

## [v2.1.7]

### Added
- `ClearSubscriptions` method for messages & events

## [v2.1.7]

### Changed
- Added warning when using incorrect build - only for devs

### Fixed
- Incompatibility with latest (preview) LLL versions

## [v2.1.6]

### Changed
- Reverted Serialization Changes from v2.1.5

### Fixed
- Network Variable ownership ignored when joining a lobby
- Null Variables breaking loading into a server

## [v2.1.5]

### Changed
- Serialization of NetworkObjects/Behaviours
  - Should now support collections

### Fixed
- Error when setting a Network Variable's value before joining a lobby.

## [v2.1.4]

### Added
- NuGet Package Tags

### Fixed
- Actually fixed issues with re-hosting.

## [v2.1.3]

### Changed
- Increased Harmony Patch priority

## [v2.1.2]

### Changed
- Minor internal refactoring.
- Better error messages.

## [v2.1.1]

### Fixed
- Issue with Network Variables causing error when re-hosting

## [v2.1.0]

### Added
- Ability to create network messages and events with a method to run instead of having to subscribe separately.

### Changed
- Minor internal refactoring.

### Fixed
- Issue with networking Network Variables

## [v2.0.2]

### Added
- Stack-traces to identifier errors. 

## [v2.0.1]

### Fixed
- Extensions were not accessible.
- Identifiers were not mod-specific.

## [v2.0.0]

### Added
- Ability to create Network Variables specific to a Network Object.
- `[PublicNetworkVariable]` Attribute

### Changed
- Network Variables
  - Ownership
    - By default, the server is now the owner.
    - Using the `[PublicNetworkVariable]` Attribute, you can allow any client to modify the variable.
    - Client-owned variables can only be made by creating one specific to a Network Object.
      - A great choice is `PlayerControllerB`
- Internal Refactoring

### Removed
- LethalNetworkVariable.SetOwnership()
  - Removed in favor of the newly reworked Network Variable ownership system.
- `[LethalNetworkProtected]` Attribute
  - Removed in favor of the newly reworked Network Variable ownership system.

### Fixed
- Errors when re-hosting a server

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

## [v1.0.0]

### Added
- Network Messages
- Network Events
- Network Variables
- Network Utils/Extensions
