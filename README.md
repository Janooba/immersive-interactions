![Immersive Interactions Banner](https://github.com/Janooba/immersive-interactions/blob/main/Website/banner.png?raw=true)

[<img height='36' src='https://cdn.ko-fi.com/cdn/kofi5.png?v=2' alt='Support the project at ko-fi.com' />](https://ko-fi.com/Z8Z4PAAFY)

# Immersive Interactions

This project aims to provide the most comprehensive and physically accurate buttons and switches on the VRChat market.

Documentation for the Immersive Interactions package can be found [here](https://docs.google.com/document/d/1okMFHb6yietf2g4nO5SxbSJP94q_d9qgWeNA-ezVXew/edit?usp=sharing).

If you’d like to test out the system, join the following world: [Immersive Interactions](https://vrchat.com/home/world/wrld_925c0e94-a683-4100-8696-7b289becf392)

## Dependencies

- Udon Sharp
- TextMeshPro (for debugging)

## Installation

- Add this project to VCC with this url `https://Janooba.github.io/immersive-interactions/index.json` or by clicking the Add to VCC button here [Here](https://janooba.github.io/immersive-interactions/)

- Alternatively, check out the [Releases](https://github.com/Janooba/immersive-interactions/releases) page for old-school package access.

## Quick Start

1. Drag the PlayerSkeletonInfo prefab located in `Assets/Janooba/Immersive Interactions/PlayerSkeletonInfo` into your scene.
1. Ensure the Bone Prefab is set.
1. Drag one of the button prefabs located in `Assets/Janooba/Immersive Interactions/Button Prefabs` into your scene.
1. Once positioned where you’d like, select the child object with a **Pressable_Button** or **Flippable_Switch** component on it.
1. Click **Find Nearby Colliders To Ignore** under **// RUNTIME & DETECTION**. This will make sure the button doesn’t get stuck in the wall or objects behind it. If this still happens, you may have to manually add the collider to the **Ignored Colliders** list.
1. Scroll to the **// EVENTS** section and add something to the list of receivers.
1. Toggle on one of the Send toggles and note the events that will be sent below.
1. Change the event name to match your own script’s Events, or add a public event to your own Udon Behaviour that matches one of the events sent.
