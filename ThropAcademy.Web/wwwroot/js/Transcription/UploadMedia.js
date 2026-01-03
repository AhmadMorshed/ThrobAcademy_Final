/**
 * Throb Academy - Media Upload & Recording Logic
 * تم التعديل ليتناسب مع الواجهة العصرية الجديدة
 */

let mediaRecorder;
let recordedChunks = [];

// العناصر الأساسية
const themeToggle = document.getElementById('themeToggle');
const courseSelect = document.getElementById('courseSelect');
const goToBankBtn = document.getElementById('goToBankBtn');
const hiddenCourseId = document.getElementById('hiddenCourseId');
const mainFileInput = document.getElementById('mainFileInput');
const fileNameDisplay = document.getElementById('fileNameDisplay');
const loadingIndicator = document.getElementById('loading');

// 1. تبديل المظهر (Dark/Light Mode)
themeToggle.onclick = () => {
    const isDark = document.body.dataset.theme === 'dark';
    const newTheme = isDark ? 'light' : 'dark';
    document.body.dataset.theme = newTheme;
    themeToggle.textContent = newTheme === 'dark' ? '☀️' : '🌙';
    localStorage.setItem('selected-theme', newTheme);
};

// تحميل الثيم المفضل عند فتح الصفحة
window.addEventListener('DOMContentLoaded', () => {
    const savedTheme = localStorage.getItem('selected-theme');
    if (savedTheme) {
        document.body.dataset.theme = savedTheme;
        themeToggle.textContent = savedTheme === 'dark' ? '☀️' : '🌙';
    }
});

// 2. تحديث الروابط ومنطق اختيار الكورس
courseSelect.onchange = function () {
    const id = this.value;
    // تحديث القيمة في نموذج الرفع التقليدي ونموذج التسجيل
    hiddenCourseId.value = id;

    if (id) {
        goToBankBtn.href = '/Transcription/QuestionBank?courseId=' + id;
        goToBankBtn.style.display = 'inline-block';
        // إضافة تأثير حركي بسيط
        goToBankBtn.classList.add('animate__animated', 'animate__fadeIn');
    } else {
        goToBankBtn.style.display = 'none';
    }
};

// 3. تفاعل منطقة الرفع (File Input Display)
mainFileInput.onchange = function () {
    if (this.files && this.files.length > 0) {
        const fileName = this.files[0].name;
        fileNameDisplay.textContent = "الملف المختار: " + fileName;
        fileNameDisplay.classList.replace('text-muted', 'text-primary');
        fileNameDisplay.classList.add('fw-bold');
    }
};

// 4. منطق التسجيل المباشر
async function startRecording(constraints, type) {
    try {
        const stream = await navigator.mediaDevices.getUserMedia(constraints);
        const preview = type === 'video' ? document.getElementById('preview') : document.getElementById('audioPreview');

        // تجهيز الواجهة للتسجيل
        preview.srcObject = stream;
        preview.style.display = 'block';
        document.getElementById('recordingControls').style.display = 'block';
        document.getElementById('startRecording').disabled = true;
        document.getElementById('startAudioRecording').disabled = true;

        mediaRecorder = new MediaRecorder(stream);
        recordedChunks = [];

        mediaRecorder.ondataavailable = e => {
            if (e.data.size > 0) recordedChunks.push(e.data);
        };

        mediaRecorder.onstop = () => {
            const mimeType = type === 'video' ? 'video/webm' : 'audio/webm';
            const blob = new Blob(recordedChunks, { type: mimeType });

            // تحويل الـ Blob إلى ملف لإرساله عبر الفورم
            const file = new File([blob], `recorded_${type}.webm`, { type: mimeType });
            const dt = new DataTransfer();
            dt.items.add(file);

            const recordedFileInput = document.getElementById('recordedFile');
            recordedFileInput.files = dt.files;

            // عرض زر الرفع النهائي
            const uploadBtn = document.getElementById('uploadRecorded');
            uploadBtn.classList.remove('d-none');
            uploadBtn.disabled = false;

            // معاينة التسجيل النهائي
            preview.srcObject = null;
            preview.src = URL.createObjectURL(blob);
            preview.controls = true;

            // إغلاق الكاميرا/الميكروفون بعد الانتهاء
            stream.getTracks().forEach(track => track.stop());
        };

        mediaRecorder.start();
        document.getElementById('stopRecording').disabled = false;

    } catch (err) {
        console.error("Error accessing media devices:", err);
        alert("فشل الوصول إلى الكاميرا أو الميكروفون. يرجى التحقق من الأذونات.");
    }
}

// أزرار التحكم في التسجيل
document.getElementById('startRecording').onclick = (e) => {
    e.preventDefault();
    startRecording({ video: true, audio: true }, 'video');
};

document.getElementById('startAudioRecording').onclick = (e) => {
    e.preventDefault();
    startRecording({ audio: true }, 'audio');
};

document.getElementById('stopRecording').onclick = () => {
    if (mediaRecorder && mediaRecorder.state !== 'inactive') {
        mediaRecorder.stop();
        document.getElementById('stopRecording').disabled = true;
        document.getElementById('recordingControls').style.display = 'none';
        document.getElementById('startRecording').disabled = false;
        document.getElementById('startAudioRecording').disabled = false;
    }
};

// 5. شاشة التحميل عند إرسال أي نموذج
document.querySelectorAll('form').forEach(form => {
    form.onsubmit = (e) => {
        // التحقق من اختيار الكورس أولاً
        if (!courseSelect.value) {
            e.preventDefault();
            alert("يرجى اختيار الكورس أولاً.");
            return;
        }
        loadingIndicator.style.display = 'block';
        // تمرير الصفحة للأسفل لرؤية مؤشر التحميل
        loadingIndicator.scrollIntoView({ behavior: 'smooth' });
    };
});