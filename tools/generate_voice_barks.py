#!/usr/bin/env python3
import os
import subprocess
import wave
import struct

OUTPUT_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), "../Assets/Audio/VoiceBarks"))
os.makedirs(OUTPUT_DIR, exist_ok=True)

BARKS = [
    # Selection Barks
    ("voice_hajur_1", "हजुर!", 0.84),
    ("voice_hajur_2", "हजुर, भन्नुहोस्!", 0.84),
    ("voice_aadesh_1", "आदेश दिनुहोस्!", 0.82),
    ("voice_tayar_1", "तयार छु!", 0.85),
    ("voice_gorkhali_1", "गोर्खाली तयार छ!", 0.80),

    # Move / Confirmation Barks
    ("voice_hunchha_1", "हुन्छ!", 0.85),
    ("voice_aagadi_1", "अगाडि बढौँ!", 0.82),
    ("voice_jaandai_1", "म जाँदै छु!", 0.85),

    # Attack & Combat Barks
    ("voice_aakraman_1", "आक्रमण!", 0.80),
    ("voice_mahakali_1", "जय महाकाली!", 0.78),
    ("voice_ayogorkhali_1", "आयो गोर्खाली!", 0.78),
    ("voice_khukuri_1", "खुकुरी प्रहार!", 0.80),

    # Villager Gathering Barks
    ("voice_kaamsuru_1", "काम सुरु!", 0.90),
    ("voice_gather_wood_1", "काठ काट्न जाँदै छु!", 0.88),
    ("voice_gather_stone_1", "ढुङ्गा निकाल्दै छु!", 0.88),
    ("voice_gather_food_1", "अन्न बाली उठाउँदै छु!", 0.90),

    # Village Defense & Alert Barks
    ("voice_shatru_1", "शत्रु आयो! हतियार उठाऊ!", 0.78),
    ("voice_suraksha_1", "गाउँ रक्षा गरौँ!", 0.80),
]

def pitch_shift_resample(in_wav, out_wav, pitch_factor):
    with wave.open(in_wav, 'rb') as wf:
        params = wf.getparams()
        nchannels, sampwidth, framerate, nframes = params[:4]
        data = wf.readframes(nframes)
    
    samples = struct.unpack(f'<{nframes}h', data)
    new_len = int(len(samples) / pitch_factor)
    new_samples = []
    for i in range(new_len):
        orig_pos = i * pitch_factor
        idx = int(orig_pos)
        frac = orig_pos - idx
        if idx + 1 < len(samples):
            s = samples[idx] * (1.0 - frac) + samples[idx + 1] * frac
        else:
            s = samples[-1]
        new_samples.append(int(max(-32768, min(32767, s))))
    
    out_data = struct.pack(f'<{len(new_samples)}h', *new_samples)
    with wave.open(out_wav, 'wb') as wf:
        wf.setparams((nchannels, sampwidth, framerate, len(new_samples), 'NONE', 'not compressed'))
        wf.writeframes(out_data)

def main():
    print(f"Generating authentic Nepali voice barks to {OUTPUT_DIR}...")
    for filename, text, pitch_factor in BARKS:
        tmp_wav = f"/tmp/{filename}_raw.wav"
        out_wav = os.path.join(OUTPUT_DIR, f"{filename}.wav")

        # Synthesize via macOS speech engine
        cmd = ["say", "-v", "Lekha", "-r", "165", "-o", tmp_wav, "--data-format=LEI16@22050", text]
        subprocess.run(cmd, check=True)

        # Apply martial pitch shift for masculine Nepali warrior / commander presence
        pitch_shift_resample(tmp_wav, out_wav, pitch_factor)
        if os.path.exists(tmp_wav):
            os.remove(tmp_wav)

        print(f"  ✓ Generated {filename}.wav: '{text}' (pitch factor {pitch_factor})")

    print("All authentic Nepali voice barks generated successfully!")

if __name__ == "__main__":
    main()
