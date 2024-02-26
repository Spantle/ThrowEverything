# ThrowEverything Changelog

## v0.4.1
- Fixed a bug causing ThrowEverything to sometimes not appear in the "Remap Controls" menu
- Updated InputUtils to v0.6.3

## v0.4.0
- Fixed a bug causing missing tooltips when some (poorly implemented) modded scrap did not have tooltips
- Fixed a bug preventing some items from being able to be switched from
- Fixed a bug causing an inconsistency where items would be slightly offset when colliding with a surface (as opposed to not colliding with a surface)
- Improved the logic for throwing an item to prevent items from clipping into surfaces
- Made the throw tooltip not appear when an item already has the maximum amount of tooltips
- Made items have a minimum and maximum hitbox size to prevent items from being too small or too large (and also to prevent small items from clipping into surfaces)
- Updated InputUtils to v0.6.1

## v0.3.0
- Added a preview of where the item will land
- Added a proper way to cancel throwing an item (scrolling the mouse wheel/switching items)
- Fixed a bug where items could be thrown when they shouldn't be (e.g. when the player is in a menu or the terminal)
- Updated the logic to make it so charging a throw means you cannot perform some actions (such as picking up other items, using emotes, etc.) until you stop charging your throw

## v0.2.1
- Fix the animation of the item travelling through the air not playing for other players

## v0.2.0
- Clean up and refactor the code
- Fix various bugs and issues
- Make the mod open source on GitHub

## v0.1.0
- Initial release (in beta)