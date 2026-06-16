# Player Sprite Direction Sheet V1

Reference image: `docs/art/player_sprite_direction_sheet_v1.png`

## Intent

This direction sheet defines the player as a grounded DIY founder working out of a converted residential startup garage. The goal is to feel capable, practical, and slightly worn-in rather than heroic or exaggerated.

## Character Read

- Adult solo founder with a tired-but-focused expression.
- Garage-startup casual outfit: dark blue hoodie or light work jacket, muted olive tee, worn jeans, practical sneakers.
- Silhouette should stay broad and readable at game scale: hoodie mass at shoulders, straight denim leg shape, clear shoe contrast at the feet.
- Hair should be slightly messy and natural, not stylized into sharp anime spikes.

## Pose Direction

- Default stance should read relaxed but ready, with weight centered and hands simple enough to survive sprite reduction.
- Walk poses should preserve the hoodie bulk and readable shoe placement over dramatic limb swing.
- Interact/use-machine pose should angle the body toward a bench or workstation, with the upper body leaning forward just enough to suggest focused tinkering.

## Palette Guidance

Target the existing garage palette from `data/art/garage_art_foundation.json`:

- Floor anchor: `#8a755f`
- Wall anchor: `#b69a79`
- Trim anchor: `#4f392b`
- Shadow anchor: `#2d241d`
- Accent anchor: `#2f5b5d`

Character-specific palette direction:

- Keep the clothing in desaturated navy, charcoal, olive, and worn denim ranges.
- Use slightly stronger contrast than the garage floor so the character reads clearly in the center play space.
- Preserve warm amber highlights from the room lighting rather than introducing cold white highlights.
- Avoid bright hero colors, neon accents, or clean sci-fi materials.

## Production Notes For ART-07-2

- Preserve the head-to-body proportion and hoodie silhouette from the direction sheet when reducing to sprite scale.
- Keep the beard/stubble and hair shape only if they remain legible at the target frame size; simplify rather than smear.
- Prioritize readable boots/shoes and pant cuffs so the feet stay grounded on the concrete.
- When in doubt, choose clarity over extra clothing detail.
