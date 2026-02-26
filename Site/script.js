function goToPage(pageId) {
    const mainContent = document.getElementById('swup');
    const sections = document.querySelectorAll('.page-section');
    const targetSection = document.getElementById(pageId);

    mainContent.classList.add('is-animating');

    setTimeout(() => {
        sections.forEach(section => {
            section.classList.remove('active');
            section.style.display = 'none';
        });

        if (pageId === 'hero') {
            targetSection.style.display = 'flex';
        } else {
            targetSection.style.display = 'block';
        }
        targetSection.classList.add('active');

        window.scrollTo(0, 0);
        mainContent.classList.remove('is-animating');
    }, 400); 
}