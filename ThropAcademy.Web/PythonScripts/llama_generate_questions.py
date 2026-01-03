import requests
import sys

API_KEY = "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"  # مفتاحك هنا

def generate_questions(text):
    prompt = f"""
    أنت مساعد ذكي. استخرج من النص التالي مجموعة من أسئلة الامتحان:

    النص:
    {text}

    الرجاء توليد:
    - 3 أسئلة اختيار من متعدد مع 4 خيارات، وضع الإجابة الصحيحة.
    - 2 أسئلة صح أو خطأ، مع بيان الإجابة الصحيحة.
    """

    data = {
        "model": "meta-llama/llama-3.3-70b-instruct",
        "messages": [{"role": "user", "content": prompt}],
        "temperature": 0.7,
        "max_tokens": 512
    }

    headers = {
        "Authorization": f"Bearer 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",
        "Content-Type": "application/json"
    }

    response = requests.post("https://openrouter.ai/api/v1/chat/completions", headers=headers, json=data)
    if response.status_code != 200:
        return f"❌ خطأ في API: {response.status_code} - {response.text}"
    return response.json()["choices"][0]["message"]["content"]


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("❌ يرجى تمرير النص كوسيط.")
        sys.exit(1)

    text_input = sys.argv[1]
    questions = generate_questions(text_input)
    print(questions)
