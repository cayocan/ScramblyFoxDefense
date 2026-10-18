# Draws the tutorial hand sprites (Stage 2 grabbing-hand tutorial): an open hand and a closed grab.
# Run: python tools/draw-hands.py  ->  Assets/Art/UI/hand-open.png, hand-grab.png (64x64, palette colours).
# Drawn at 4x and downsampled for smooth edges. The grab is a plain fist with no raised finger.
from PIL import Image, ImageDraw

SCALE = 4
SIZE = 64 * SCALE
INK = (0x20, 0x13, 0x38, 255)
SKIN = (0xFF, 0xF6, 0xE8, 255)
CUFF = (0x78, 0x45, 0xD8, 255)
LINE = 9  # outline thickness at 4x


def shapes(draw, items, grow, colour):
    for kind, box in items:
        box = (box[0] - grow, box[1] - grow, box[2] + grow, box[3] + grow)
        if kind == "round":
            draw.rounded_rectangle(box, radius=(box[2] - box[0]) // 2 if box[2] - box[0] < box[3] - box[1] else (box[3] - box[1]) // 2, fill=colour)
        elif kind == "rect":
            draw.rounded_rectangle(box, radius=14, fill=colour)
        else:
            draw.ellipse(box, fill=colour)


def render(items, creases, name):
    image = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    cuff = [("rect", (78, 196, 182, 244))]
    shapes(draw, cuff + items, LINE, INK)       # outline pass
    shapes(draw, cuff, 0, CUFF)
    shapes(draw, items, 0, SKIN)                 # fill pass
    for a, b in creases:
        draw.line((a, b), fill=INK, width=7)
    image.resize((64, 64), Image.LANCZOS).save(f"Assets/Art/UI/{name}.png")


# Open hand, palm toward the viewer: four spread fingers and the thumb out to the left.
OPEN = [
    ("round", (70, 28, 100, 130)),    # index
    ("round", (104, 14, 134, 130)),   # middle
    ("round", (138, 24, 168, 130)),   # ring
    ("round", (170, 50, 196, 135)),   # little
    ("rect", (70, 100, 196, 204)),    # palm
    ("round", (30, 112, 62, 178)),    # thumb
    ("rect", (40, 140, 100, 196)),    # thumb base joining the palm
]

# Closed grab, back of the hand: knuckles on top, fingers curled under, thumb wrapped across the front.
GRAB = [
    ("rect", (62, 86, 194, 204)),     # fist
    ("oval", (62, 66, 102, 108)),     # knuckles
    ("oval", (96, 60, 136, 102)),
    ("oval", (130, 62, 170, 104)),
    ("oval", (160, 72, 196, 110)),
    ("round", (56, 136, 150, 172)),   # thumb across the front
]
GRAB_CREASES = [((99, 104), (99, 132)), ((133, 100), (133, 132)), ((165, 106), (165, 136))]

render(OPEN, [], "hand-open")
render(GRAB, GRAB_CREASES, "hand-grab")
print("wrote Assets/Art/UI/hand-open.png, hand-grab.png")
