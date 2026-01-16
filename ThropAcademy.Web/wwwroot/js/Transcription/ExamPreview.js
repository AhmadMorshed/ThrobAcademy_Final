let timeLeft = CONFIG.durationSeconds;
let cheatAttempts = 0;
const MAX_ATTEMPTS = 2;
let timerInterval;

function startExam() {
    if (CONFIG.isAdmin) return;

    // طلب ملء الشاشة
    forceFullScreen();

    document.getElementById('start-overlay').style.display = 'none';
    document.getElementById('exam-area').style.display = 'block';

    startTimer();
    setupStrictAntiCheat();
}

function setupStrictAntiCheat() {
    if (CONFIG.isAdmin) return;

    // 1. منع النسخ واللصق والقائمة اليمنى
    document.addEventListener('contextmenu', e => e.preventDefault());
    document.body.style.userSelect = 'none';

    // 2. اكتشاف الخروج من الصفحة (Tab Switch / Alt+Tab)
    document.addEventListener("visibilitychange", () => {
        if (document.hidden) {
            handleCheat("مغادرة نافذة الاختبار");
        }
    });

    // 3. اكتشاف فقدان التركيز وتعتيم الشاشة
    window.addEventListener('blur', () => {
        document.getElementById('exam-area').classList.add('blur-content');
        handleCheat("النقر خارج منطقة الاختبار");
    });

    window.addEventListener('focus', () => {
        document.getElementById('exam-area').classList.remove('blur-content');
    });

    // 4. منع اختصارات الكيبورد (F12, Ctrl+Shift+I, etc)
    document.addEventListener('keydown', e => {
        const forbiddenKeys = ['F12', 'u', 'i', 'j', 'p', 's'];
        if (forbiddenKeys.includes(e.key.toLowerCase()) && (e.ctrlKey || e.metaKey)) {
            e.preventDefault();
            handleCheat("محاولة الوصول لأدوات المطور");
        }
    });
}

function handleCheat(reason) {
    cheatAttempts++;
    if (cheatAttempts >= MAX_ATTEMPTS) {
        autoSubmit("تم إلغاء الاختبار بسبب تجاوز محاولات الغش: " + reason);
    } else {
        Swal.fire({
            title: 'تحذير غش!',
            text: `تم رصد: ${reason}. (محاولة ${cheatAttempts} من ${MAX_ATTEMPTS})`,
            icon: 'warning',
            confirmButtonText: 'العودة للاختبار'
        }).then(() => forceFullScreen());
    }
}

function startTimer() {
    timerInterval = setInterval(() => {
        let min = Math.floor(timeLeft / 60);
        let sec = timeLeft % 60;

        const display = document.getElementById("timer");
        display.innerHTML = `${min < 10 ? '0' + min : min}:${sec < 10 ? '0' + sec : sec}`;

        if (timeLeft <= 60) display.classList.add("timer-danger");
        else if (timeLeft <= 300) display.classList.add("timer-warning");

        if (timeLeft <= 0) {
            clearInterval(timerInterval);
            autoSubmit("انتهى وقت الاختبار!");
        }
        timeLeft--;
    }, 1000);
}

function updateProgress() {
    const answered = document.querySelectorAll('input[type="radio"]:checked').length;
    const percent = Math.round((answered / CONFIG.totalQuestions) * 100);
    document.getElementById('exam-progress').style.width = percent + '%';
    document.getElementById('progress-text').innerText = percent + '%';
}

function forceFullScreen() {
    const elem = document.documentElement;
    if (!document.fullscreenElement) {
        if (elem.requestFullscreen) elem.requestFullscreen();
        else if (elem.webkitRequestFullscreen) elem.webkitRequestFullscreen();
    }
}

function autoSubmit(msg) {
    window.onbeforeunload = null;
    Swal.fire({ title: 'تنبيه', text: msg, icon: 'info', timer: 2000, showConfirmButton: false })
        .then(() => document.getElementById("examForm").submit());
}

// منع إغلاق الصفحة العفوي
window.onbeforeunload = () => CONFIG.isAdmin ? null : "هل أنت متأكد؟ سيتم فقدان تقدمك.";
document.getElementById('examForm')?.addEventListener('submit', () => window.onbeforeunload = null);