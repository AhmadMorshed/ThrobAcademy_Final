import sys
import os
import subprocess
import whisper
from datetime import datetime

def log_message(message):
    with open("whisper_output.log", "a", encoding="utf-8") as f:
        f.write(f"{datetime.now()} - {message}\n")

def convert_to_wav(input_path):
    output_path = os.path.splitext(input_path)[0] + ".wav"
    try:
        subprocess.run(["ffmpeg", "-y", "-i", input_path, output_path], check=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        return output_path
    except subprocess.CalledProcessError as e:
        log_message(f"خطأ في تحويل ffmpeg: {e.stderr.decode()}")
        return None

def transcribe_with_whisper(audio_path):
    try:
        model = whisper.load_model("base")
        result = model.transcribe(audio_path)
        return result["text"]
    except Exception as e:
        log_message(f"خطأ في Whisper: {e}")
        return None

def main():
    if len(sys.argv) < 2:
        log_message("❌ لم يتم تمرير أي ملف كوسيط.")
        return

    input_path = sys.argv[1]
    
    if not os.path.exists(input_path):
        log_message(f"❌ الملف غير موجود: {input_path}")
        return

    # تحويل إلى wav
    log_message(f"🔄 جاري تحويل الملف: {input_path}")
    wav_path = convert_to_wav(input_path)
    if not wav_path:
        log_message("❌ فشل في تحويل الملف إلى WAV.")
        return

    # النسخ باستخدام Whisper
    log_message(f"🧠 جاري تفريغ الصوت باستخدام Whisper من: {wav_path}")
    text = transcribe_with_whisper(wav_path)

    if text:
        output_path = os.path.splitext(input_path)[0] + ".txt"
        with open(output_path, "w", encoding="utf-8") as f:
            f.write(text)
        log_message(f"✅ تم التفريغ بنجاح، النص محفوظ في: {output_path}")
    else:
        log_message("❌ فشل في استخراج النص.")

if __name__ == "__main__":
    main()
