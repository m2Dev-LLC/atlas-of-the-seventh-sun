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

## Natural Obstacles + Enemies

- Every expedition now combines a fresh random seed with two noise layers, producing a unique biome and obstacle layout.
- Procedural validation guarantees land routes to all four cities, Dawnwatch, and the rival spawn; an unreachable map automatically receives natural-looking passes.
- Mountain ridges are impassable to the player, rival, enemies, and cartographer pathfinding.
- Desert steps cost two provisions, consume two days, and use a slower movement cadence.
- Dense forest costs three provisions and reduces local reveal distance.
- Mountain chains, forest clusters, and dune fields now carry stronger silhouettes at gameplay zoom.
- Each map deploys three to five hostile patrols in reachable inland regions.
- Patrols pursue the player every two successful steps, avoid mountain tiles and occupied enemy tiles, and raid morale and provisions at close range.
- Added red patrol tokens, attack bursts, impact audio, camera feedback, journal events, and HUD patrol count.

### Latest build proof

- Unity 6.6 WebGL build: success.
- Artifact: `Builds/WebGL-Obstacles-Enemies` (15,340,608 bytes).
- Browser check confirmed the unique map code, mountain and dune rendering, route advisor, enemy count, and visible patrol tokens.
