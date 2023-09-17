[<img src='https://github.com/Janooba/immersive-interactions/blob/main/Website/banner.png?raw=true'/>](#)

[<img height='36' src='https://cdn.ko-fi.com/cdn/kofi5.png?v=2' alt='Support the project at ko-fi.com' />](https://ko-fi.com/Z8Z4PAAFY)

# Immersive Interactions

This project aims to provide the most comprehensive and physically accurate buttons and switches on the VRChat market.

## Dependencies

- Udon Sharp
- TextMeshPro (for debugging)

## Installation

- Add this project to VCC with this url `https://Janooba.github.io/immersive-interactions/index.json` or by clicking the Add to VCC button here [Here](https://janooba.github.io/immersive-interactions/)

- Alternatively, check out the [Releases](https://github.com/Janooba/immersive-interactions/releases) page for old-school package access.

## Accurately tracked hands!
https://github.com/Janooba/immersive-interactions/assets/17034238/4ae79dc1-2825-4e8b-9085-6d35e09e4d92

## Physics enabled!
https://github.com/Janooba/immersive-interactions/assets/17034238/e33fb93e-ba72-4b69-b77c-51325777a50f

## Flippable switches!
https://github.com/Janooba/immersive-interactions/assets/17034238/2b94e1f5-01ff-4731-8154-41bc1e7ca1ea

## Pushable buttons!
https://github.com/Janooba/immersive-interactions/assets/17034238/40ce9fa2-2134-44a7-9257-7540197a9bc1

## Buttons on physics objects!?
https://github.com/Janooba/immersive-interactions/assets/17034238/bf1fea51-ae7a-4470-8f29-b3bc9d6ec2ec

## Comes with a variety of prefabs
https://github.com/Janooba/immersive-interactions/assets/17034238/1714cb18-a66b-480f-8a4a-daf4bb6b926e

Online documentation for the Immersive Interactions package can be found [here](https://docs.google.com/document/d/1okMFHb6yietf2g4nO5SxbSJP94q_d9qgWeNA-ezVXew/edit?usp=sharing).

If you’d like to test out the system yourself, join the following world: [Immersive Interactions](https://vrchat.com/home/world/wrld_925c0e94-a683-4100-8696-7b289becf392)

## Quick Start

1. Drag the PlayerSkeletonInfo prefab located in `Packages/Immersive Interactions/Runtime/Immersive Interactions/PlayerSkeletonInfo` into your scene.
1. Ensure the Bone Prefab is set.
1. Drag one of the button prefabs located in `Packages/Immersive Interactions/Runtime/Immersive Interactions/Button Prefabs` into your scene.
1. Once positioned where you’d like, select the child object with a **Pressable_Button** or **Flippable_Switch** component on it.
1. Click **Find Nearby Colliders To Ignore** under **// RUNTIME & DETECTION**. This will make sure the button doesn’t get stuck in the wall or objects behind it. If this still happens, you may have to manually add the collider to the **Ignored Colliders** list.
1. Scroll to the **// EVENTS** section and add something to the list of receivers.
1. Toggle on one of the Send toggles and note the events that will be sent below.
1. Change the event name to match your own script’s Events, or add a public event to your own Udon Behaviour that matches one of the events sent.
