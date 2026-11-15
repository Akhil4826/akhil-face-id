"""
Generates the complete original brand visual assets for AKHIL FACE ID:
- 512x512 Master Logo PNG
- Multi-resolution ICO (256, 128, 64, 48, 32, 16)
- System Tray Icon
- 32-bit BMP Tile for Windows Lock Screen
- Installer Wizard Banners (wizard.bmp, wizard_small.bmp, banner.jpg)
"""
import math
from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter

ROOT = Path(__file__).resolve().parents[1]
APP_DIR = ROOT / "app" / "WinFace"
ASSETS_DIR = ROOT / "assets"
APP_DIR.mkdir(parents=True, exist_ok=True)
ASSETS_DIR.mkdir(parents=True, exist_ok=True)

def draw_logo(size=512):
    # Render at 4x for high-quality downsampling / antialiasing
    scale = 4
    dim = size * scale
    im = Image.new("RGBA", (dim, dim), (0, 0, 0, 0))
    draw = ImageDraw.Draw(im)

    c = dim / 2
    r_outer = dim * 0.45
    r_inner = dim * 0.38

    # Dark background circle with sleek border
    bg_color = (11, 15, 25, 250)      # Deep obsidian navy #0B0F19
    border_color = (0, 240, 255, 180)  # Cyan #00F0FF

    # Outer subtle glow
    for i in range(12):
        glow_r = r_outer + (12 - i) * 6 * scale
        alpha = int(8 + i * 2)
        draw.ellipse([c - glow_r, c - glow_r, c + glow_r, c + glow_r], fill=(0, 240, 255, alpha))

    # Base disc
    draw.ellipse([c - r_outer, c - r_outer, c + r_outer, c + r_outer], fill=bg_color)
    draw.ellipse([c - r_outer, c - r_outer, c + r_outer, c + r_outer], outline=(0, 240, 255, 220), width=int(3 * scale))

    # Concentric biometric scanner track arcs
    draw.arc([c - r_inner, c - r_inner, c + r_inner, c + r_inner], start=30, end=150, fill=(0, 240, 255, 200), width=int(4 * scale))
    draw.arc([c - r_inner, c - r_inner, c + r_inner, c + r_inner], start=210, end=330, fill=(0, 240, 255, 200), width=int(4 * scale))

    # Corner framing brackets (tactical biometric viewfinder)
    b_len = dim * 0.12
    b_inset = dim * 0.20
    b_thick = int(5 * scale)
    cyan_bright = (0, 240, 255, 255)
    electric_blue = (14, 165, 233, 255)

    # Top-Left Bracket
    draw.line([(c - b_inset, c - b_inset), (c - b_inset + b_len, c - b_inset)], fill=cyan_bright, width=b_thick)
    draw.line([(c - b_inset, c - b_inset), (c - b_inset, c - b_inset + b_len)], fill=cyan_bright, width=b_thick)

    # Top-Right Bracket
    draw.line([(c + b_inset, c - b_inset), (c + b_inset - b_len, c - b_inset)], fill=cyan_bright, width=b_thick)
    draw.line([(c + b_inset, c - b_inset), (c + b_inset, c - b_inset + b_len)], fill=cyan_bright, width=b_thick)

    # Bottom-Left Bracket
    draw.line([(c - b_inset, c + b_inset), (c - b_inset + b_len, c + b_inset)], fill=cyan_bright, width=b_thick)
    draw.line([(c - b_inset, c + b_inset), (c - b_inset, c + b_inset - b_len)], fill=cyan_bright, width=b_thick)

    # Bottom-Right Bracket
    draw.line([(c + b_inset, c + b_inset), (c + b_inset - b_len, c + b_inset)], fill=cyan_bright, width=b_thick)
    draw.line([(c + b_inset, c + b_inset), (c + b_inset, c + b_inset - b_len)], fill=cyan_bright, width=b_thick)

    # Central Geometric Shield "A" (Akhil) + Biometric Node Constellation
    # Apex of "A"
    p_apex = (c, c - dim * 0.14)
    p_left_leg = (c - dim * 0.13, c + dim * 0.14)
    p_right_leg = (c + dim * 0.13, c + dim * 0.14)
    p_cross_left = (c - dim * 0.075, c + dim * 0.03)
    p_cross_right = (c + dim * 0.075, c + dim * 0.03)

    # Draw "A" silhouette lines
    draw.line([p_left_leg, p_apex], fill=(255, 255, 255, 240), width=int(5 * scale))
    draw.line([p_apex, p_right_leg], fill=(255, 255, 255, 240), width=int(5 * scale))
    draw.line([p_cross_left, p_cross_right], fill=cyan_bright, width=int(4 * scale))

    # Biometric Nodes (points of facial constellation)
    nodes = [
        p_apex,
        p_left_leg,
        p_right_leg,
        p_cross_left,
        p_cross_right,
        (c, c - dim * 0.02),                  # Nose bridge
        (c - dim * 0.055, c - dim * 0.06),    # Left Eye
        (c + dim * 0.055, c - dim * 0.06),    # Right Eye
        (c, c + dim * 0.09),                  # Chin node
    ]

    # Draw biometric connections between eyes and nose
    draw.line([(c - dim * 0.055, c - dim * 0.06), (c, c - dim * 0.02)], fill=(0, 240, 255, 140), width=int(2 * scale))
    draw.line([(c + dim * 0.055, c - dim * 0.06), (c, c - dim * 0.02)], fill=(0, 240, 255, 140), width=int(2 * scale))
    draw.line([(c, c - dim * 0.02), (c, c + dim * 0.09)], fill=(0, 240, 255, 140), width=int(2 * scale))

    for node in nodes:
        nr = 6 * scale
        draw.ellipse([node[0] - nr, node[1] - nr, node[0] + nr, node[1] + nr], fill=cyan_bright)
        nr_inner = 3 * scale
        draw.ellipse([node[0] - nr_inner, node[1] - nr_inner, node[0] + nr_inner, node[1] + nr_inner], fill=(255, 255, 255, 255))

    # Downsample with high-quality Lanczos resampling
    final = im.resize((size, size), Image.Resampling.LANCZOS)
    return final

def generate_assets():
    print("Generating Master Logo...")
    master = draw_logo(512)

    # 1. PNG Logos
    master.save(APP_DIR / "akhilfaceid.png", "PNG")
    master.save(APP_DIR / "winface.png", "PNG")  # maintain compatibility
    master.save(ASSETS_DIR / "akhilfaceid.png", "PNG")

    # 2. Multi-Resolution ICO
    print("Generating Multi-resolution ICO...")
    sizes = [(256, 256), (128, 128), (64, 64), (48, 48), (32, 32), (16, 16)]
    ico_images = [master.resize(s, Image.Resampling.LANCZOS) for s in sizes]
    ico_images[0].save(APP_DIR / "akhilfaceid.ico", format="ICO", sizes=sizes)
    ico_images[0].save(APP_DIR / "winface.ico", format="ICO", sizes=sizes)
    ico_images[0].save(ASSETS_DIR / "akhilfaceid.ico", format="ICO", sizes=sizes)

    # 3. Windows Lock Screen Tile Bitmap (192x192 24/32-bit BMP)
    print("Generating Lock Screen Tile BMP...")
    tile = draw_logo(192)
    tile_bg = Image.new("RGB", (192, 192), (16, 22, 34))
    tile_bg.paste(tile, (0, 0), tile)
    tile_bg.save(ASSETS_DIR / "tile.bmp", "BMP")
    tile_bg.save(ASSETS_DIR / "akhilfaceid_tile.bmp", "BMP")

    # 4. Installer Wizard Artwork
    print("Generating Installer Wizard Banners...")
    # wizard.bmp: 164x314 left sidebar for Inno Setup
    w_side = Image.new("RGB", (164, 314), (11, 15, 25))
    w_draw = ImageDraw.Draw(w_side)
    # Background gradient effect
    for y in range(314):
        g = int(15 + (y / 314.0) * 20)
        b = int(25 + (y / 314.0) * 35)
        w_draw.line([(0, y), (164, y)], fill=(11, g, b))
    # Place scaled logo
    scaled_logo = master.resize((120, 120), Image.Resampling.LANCZOS)
    w_side.paste(scaled_logo, (22, 45), scaled_logo)
    w_draw.text((22, 180), "AKHIL", fill=(255, 255, 255))
    w_draw.text((22, 198), "FACE ID", fill=(0, 240, 255))
    w_draw.text((22, 225), "Secure Biometrics\nWindows 11 Edition", fill=(160, 175, 200))
    w_side.save(ASSETS_DIR / "wizard.bmp", "BMP")

    # wizard_small.bmp: 55x55 header icon for Inno Setup
    w_small = Image.new("RGB", (55, 55), (11, 15, 25))
    s_logo = master.resize((48, 48), Image.Resampling.LANCZOS)
    w_small.paste(s_logo, (4, 4), s_logo)
    w_small.save(ASSETS_DIR / "wizard_small.bmp", "BMP")

    # banner.jpg: 1200x400 header banner
    banner = Image.new("RGB", (1200, 400), (10, 14, 23))
    b_draw = ImageDraw.Draw(banner)
    for x in range(1200):
        t = x / 1200.0
        r = int(10 + t * 5)
        g = int(14 + t * 15)
        b = int(23 + t * 45)
        b_draw.line([(x, 0), (x, 400)], fill=(r, g, b))

    b_logo = master.resize((260, 260), Image.Resampling.LANCZOS)
    banner.paste(b_logo, (80, 70), b_logo)
    b_draw.text((380, 130), "AKHIL FACE ID", fill=(255, 255, 255))
    b_draw.text((380, 180), "Professional AI Facial Recognition for Windows 11", fill=(0, 240, 255))
    b_draw.text((380, 220), "On-Device Neural Biometrics • TPM 2.0 Vault • Anti-Spoof Protection", fill=(180, 195, 220))
    banner.save(ASSETS_DIR / "banner.jpg", "JPEG", quality=95)

    print("All branding assets generated successfully.")

if __name__ == "__main__":
    generate_assets()

