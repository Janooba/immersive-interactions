# Changelog

## v0.4.0-betas
- Completely rewrites flippable switch collision detection and resolving. Now reacts more accurately
- Adds Handle module to flippable switches
- Fix for switch rotations being world space under certain circumstances
- Updated example scene
- Fixed lever collision detection when min max rotations are swapped
- Added mesh gizmo for lever min and max position to help visualize extents
- Min and max angles are now applied to whatever rotation the button starts in

## v0.3.3
- Fixed edge case where disabling a button in certain states would freeze it when re-enabled

## v0.3.2
- Everything from 0.2.x-betas
- Fix bug with buttons that are set to start toggle on
- Fix inconsistent push registry

## v0.3.0
- Unity 2022.3 upgrade

## v0.2.6
- Release

## v0.2.6-betas
- Fixed dependancies so that VCC doesn't break your project
- Added warning for null udon receivers with a button to clean up the nulls for you
- Cleared up some tooltip language
- Fixed flippable switches not keeping their event strings
- Added "Limit Backpress" option to pressable buttons that can help stop buttons from being pushed from behind
- Raised head bone position so that it's not sunk into the neck so much
- Added box bones to hands without fingers
- Fixed 2 joint finger bones not having their full length
- Just-enabled buttons will not trigger immediately if they were left pressed when disabled
- Enabled buttons will not get stuck if disabled while pressed