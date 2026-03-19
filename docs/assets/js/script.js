function goToPage(pageId) {
    const sections = document.querySelectorAll('.page-section');
    const target = document.getElementById(pageId);

    if (!target) return;

    sections.forEach(sec => {
        sec.classList.remove('active');
    });

    target.classList.add('active');

    window.scrollTo(0, 0);
}