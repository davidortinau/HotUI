#!/usr/bin/env python3
"""Snapshot comparison: MAUI Reference vs Comet MVU — pixel-level analysis."""

import subprocess, time, os, sys
from PIL import Image, ImageDraw, ImageFont

SIM = "9B25DEC2-88C8-4612-B5E2-2B04C8313D4D"
MAUI_BUNDLE = "com.companyname.mauireference"
COMET_BUNDLE = "com.comet.projectmanager"
OUTPUT_DIR = "/tmp/snapshots"
COMET_DIR = "/Users/jfversluis/Documents/GitHub/Comet"

os.makedirs(OUTPUT_DIR, exist_ok=True)

def simctl(*args):
    return subprocess.run(["xcrun", "simctl"] + list(args),
                         capture_output=True, text=True, timeout=30, cwd=COMET_DIR)

def screenshot(name):
    path = f"{OUTPUT_DIR}/{name}.png"
    simctl("io", SIM, "screenshot", path)
    return path

def launch_app(bundle_id, extra_args=None):
    simctl("terminate", SIM, bundle_id)
    time.sleep(1)
    args = ["launch", SIM, bundle_id]
    if extra_args:
        args += ["--"] + extra_args
    simctl(*args)
    time.sleep(8)

def compare_region(img1, img2, y_start, y_end, name, tolerance=15):
    """Compare a specific region of two images. Returns (similarity%, diff_count, total)."""
    w = min(img1.width, img2.width)
    total = 0
    matching = 0
    diffs = []
    
    step = 2  # Every 2 pixels for balance of speed/accuracy
    for y in range(y_start, min(y_end, img1.height, img2.height), step):
        for x in range(0, w, step):
            total += 1
            r1, g1, b1 = img1.getpixel((x, y))[:3]
            r2, g2, b2 = img2.getpixel((x, y))[:3]
            if abs(r1-r2) <= tolerance and abs(g1-g2) <= tolerance and abs(b1-b2) <= tolerance:
                matching += 1
            else:
                diffs.append((x, y))
    
    sim = (matching / total * 100) if total > 0 else 0
    return sim, len(diffs), total, diffs

def analyze_page(maui_path, comet_path, page_name):
    """Detailed analysis of a page comparison."""
    maui = Image.open(maui_path)
    comet = Image.open(comet_path)
    
    print(f"\n{'='*50}")
    print(f"📊 {page_name} Analysis")
    print(f"{'='*50}")
    print(f"  MAUI size: {maui.size}, Comet size: {comet.size}")
    
    # Define regions (at 3x retina scale)
    # Nav bar: 0-400
    # Content: 400 - (height-300) 
    # Bottom: (height-300) - height
    h = min(maui.height, comet.height)
    
    regions = {
        "Nav Bar": (0, 400),
        "Content Top (chart)": (400, 900),
        "Content Mid (cards)": (900, 1600),
        "Content Bottom (tasks)": (1600, h - 300),
        "Bottom Bar": (h - 300, h),
    }
    
    overall_match = 0
    overall_total = 0
    all_diffs = []
    
    for region_name, (y_start, y_end) in regions.items():
        if y_start >= h or y_end <= y_start:
            continue
        sim, diff_count, total, diffs = compare_region(maui, comet, y_start, y_end, region_name)
        overall_match += (total - diff_count)
        overall_total += total
        all_diffs.extend(diffs)
        
        status = "✅" if sim >= 90 else "⚠️" if sim >= 75 else "❌"
        print(f"  {status} {region_name}: {sim:.1f}% ({diff_count} diffs / {total} pixels)")
    
    overall_sim = (overall_match / overall_total * 100) if overall_total > 0 else 0
    
    # Create visual diff
    w = min(maui.width, comet.width)
    diff_img = Image.new('RGBA', (w, h), (255, 255, 255, 255))
    # Blend the two images
    for y in range(0, h):
        for x in range(0, w, 2):
            r1, g1, b1 = maui.getpixel((x, y))[:3]
            r2, g2, b2 = comet.getpixel((x, y))[:3]
            if abs(r1-r2) > 15 or abs(g1-g2) > 15 or abs(b1-b2) > 15:
                diff_img.putpixel((x, y), (255, 0, 0, 128))
            else:
                avg = ((r1+r2)//2, (g1+g2)//2, (b1+b2)//2, 255)
                diff_img.putpixel((x, y), avg)
    diff_img.save(f"{OUTPUT_DIR}/diff_{page_name.lower().replace(' ', '_')}.png")
    
    # Side-by-side
    side = Image.new('RGB', (w*2 + 20, h), (255, 255, 255))
    side.paste(maui.crop((0, 0, w, h)), (0, 0))
    side.paste(comet.crop((0, 0, w, h)), (w + 20, 0))
    side.save(f"{OUTPUT_DIR}/side_{page_name.lower().replace(' ', '_')}.png")
    
    print(f"\n  📈 Overall: {overall_sim:.1f}% similar")
    return overall_sim

def main():
    print("🔍 SNAPSHOT COMPARISON: MAUI Reference vs Comet MVU")
    print("=" * 60)
    
    # 1. Build and install apps
    print("\n🔨 Building apps...")
    subprocess.run(["dotnet", "build", "-f", "net9.0-ios",
                    "sample/CometProjectManager/CometProjectManager.csproj",
                    "--no-restore", "-v", "q"], 
                   capture_output=True, cwd=COMET_DIR)
    
    simctl("install", SIM, 
           f"{COMET_DIR}/sample/CometProjectManager/bin/Debug/net9.0-ios/iossimulator-arm64/CometProjectManager.app")
    
    results = {}
    
    # 2. Dashboard comparison
    print("\n📸 Capturing Dashboard...")
    launch_app(MAUI_BUNDLE)
    maui_dash = screenshot("maui_dashboard")
    
    launch_app(COMET_BUNDLE, ["--page=dashboard"])
    comet_dash = screenshot("comet_dashboard")
    
    results["Dashboard"] = analyze_page(maui_dash, comet_dash, "Dashboard")
    
    # 3. Projects comparison 
    print("\n📸 Capturing Projects...")
    launch_app(COMET_BUNDLE, ["--page=projects"])
    comet_proj = screenshot("comet_projects")
    results["Projects"] = None  # MAUI Projects needs Shell navigation
    
    # 4. ManageMeta comparison
    print("\n📸 Capturing Manage Meta...")
    launch_app(COMET_BUNDLE, ["--page=manage"])
    comet_meta = screenshot("comet_manage")
    results["ManageMeta"] = None
    
    # Summary
    print("\n" + "=" * 60)
    print("📋 RESULTS SUMMARY")
    print("=" * 60)
    for name, sim in results.items():
        if sim is not None:
            status = "✅ PASS" if sim >= 95 else "⚠️ CLOSE" if sim >= 85 else "❌ FAIL"
            print(f"  {status} {name}: {sim:.1f}%")
        else:
            print(f"  ⏭️  {name}: Skipped (can't navigate MAUI Shell)")
    
    print(f"\nOutput: {OUTPUT_DIR}/")

if __name__ == "__main__":
    main()
