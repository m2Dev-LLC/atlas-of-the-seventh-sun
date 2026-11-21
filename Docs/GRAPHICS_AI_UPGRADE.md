# Graphics + AI Upgrade

## Gameplay AI

- Added a rival expedition with its own rust-and-ink map token.
- The rival evaluates terrain-weighted routes and advances toward an uncharted city every three player steps.
- A rival survey reduces that city's eventual trade reward, adding visible strategic pressure without removing the discovery.
- Added a cartographer advisor that continuously recalculates the lowest-provision route to the nearest uncharted city.
- The advisor reports direction and estimated provision cost in the HUD.

## Graphics

- Added animated city beacons and a seven-step gold route preview.
- Added a layered rival banner silhouette with idle motion and eased map movement.
- Added faint ambient wind strokes to make the parchment world feel alive.
- Updated the field guide to explain the route advisor and rival rules.
- Kept all new rendering inside the established ink, parchment, gold, and rust palette.

## Verification

- Unity 6.6 WebGL player build succeeded with no C# build errors.
- Artifact: `Builds/WebGL-Graphics-AI` (15,340,665 bytes).
- Browser play check confirmed the title screen, HUD, animated terrain, route preview, movement, and advisor cost updates.
