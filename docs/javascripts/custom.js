/**
 * PortLog - Enhanced MkDocs Material JavaScript
 * Adds custom interactions and animations
 */

document.addEventListener('DOMContentLoaded', function() {
    
    // Enhanced smooth scrolling
    function initSmoothScrolling() {
        const links = document.querySelectorAll('a[href^="#"]');
        links.forEach(link => {
            link.addEventListener('click', function(e) {
                e.preventDefault();
                const targetId = this.getAttribute('href').substring(1);
                const targetElement = document.getElementById(targetId);
                
                if (targetElement) {
                    targetElement.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                    
                    // Add highlight effect
                    targetElement.style.background = 'rgba(21, 101, 192, 0.1)';
                    targetElement.style.transition = 'background 0.3s ease';
                    
                    setTimeout(() => {
                        targetElement.style.background = '';
                    }, 1500);
                }
            });
        });
    }

    // Progress bar for reading
    function initReadingProgress() {
        const progressBar = document.createElement('div');
        progressBar.className = 'reading-progress';
        progressBar.innerHTML = '<div class="reading-progress__fill"></div>';
        document.body.appendChild(progressBar);

        const progressFill = progressBar.querySelector('.reading-progress__fill');
        const content = document.querySelector('.md-content__inner');

        if (!content) return;

        window.addEventListener('scroll', function() {
            const scrollTop = window.pageYOffset;
            const docHeight = content.offsetHeight;
            const winHeight = window.innerHeight;
            const scrollPercent = scrollTop / (docHeight - winHeight);
            const scrollPercentRounded = Math.round(scrollPercent * 100);

            progressFill.style.width = Math.min(scrollPercentRounded, 100) + '%';
        });
    }

    // Enhanced code block interactions
    function initCodeBlockEnhancements() {
        const codeBlocks = document.querySelectorAll('pre code');
        
        codeBlocks.forEach((block, index) => {
            const pre = block.parentElement;
            
            // Add language label
            const language = block.className.match(/language-(\w+)/);
            if (language) {
                const label = document.createElement('div');
                label.className = 'code-language-label';
                label.textContent = language[1].toUpperCase();
                pre.appendChild(label);
            }

            // Add line numbers
            const lines = block.textContent.split('\n').length;
            if (lines > 3) {
                const lineNumbers = document.createElement('div');
                lineNumbers.className = 'line-numbers';
                
                for (let i = 1; i <= lines; i++) {
                    const lineNumber = document.createElement('span');
                    lineNumber.textContent = i;
                    lineNumbers.appendChild(lineNumber);
                }
                
                pre.style.position = 'relative';
                pre.appendChild(lineNumbers);
                block.style.paddingLeft = '3rem';
            }
        });
    }

    // Interactive navigation highlighting
    function initNavigationHighlighting() {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const id = entry.target.id;
                    const navLink = document.querySelector(`a[href="#${id}"]`);
                    
                    // Remove previous active states
                    document.querySelectorAll('.md-nav__link--active').forEach(link => {
                        link.classList.remove('md-nav__link--active');
                    });
                    
                    // Add active state to current link
                    if (navLink) {
                        navLink.classList.add('md-nav__link--active');
                    }
                }
            });
        }, {
            rootMargin: '-20% 0px -80% 0px'
        });

        // Observe all headings
        document.querySelectorAll('h1, h2, h3, h4, h5, h6').forEach(heading => {
            if (heading.id) {
                observer.observe(heading);
            }
        });
    }

    // Dynamic theme color based on scroll position
    function initDynamicThemeColor() {
        const header = document.querySelector('.md-header');
        if (!header) return;

        window.addEventListener('scroll', function() {
            const scrolled = window.pageYOffset;
            const rate = scrolled * -0.5;
            
            if (scrolled > 100) {
                header.style.background = `linear-gradient(135deg, 
                    hsl(${210 + rate * 0.1}, 70%, 45%) 0%, 
                    hsl(${195 + rate * 0.1}, 70%, 50%) 50%, 
                    hsl(${188 + rate * 0.1}, 70%, 55%) 100%)`;
            } else {
                header.style.background = '';
            }
        });
    }

    // Enhanced search functionality
    function initEnhancedSearch() {
        const searchInput = document.querySelector('.md-search__input');
        if (!searchInput) return;

        // Add search suggestions
        const suggestions = [
            'Installation guide',
            'API documentation',
            'User management',
            'Logging system',
            'Dashboard features',
            'Configuration setup'
        ];

        searchInput.addEventListener('focus', function() {
            this.placeholder = suggestions[Math.floor(Math.random() * suggestions.length)];
        });

        searchInput.addEventListener('blur', function() {
            this.placeholder = 'Search';
        });
    }

    // Interactive tooltips
    function initInteractiveTooltips() {
        const tooltipElements = document.querySelectorAll('[title]');
        
        tooltipElements.forEach(element => {
            const title = element.getAttribute('title');
            element.removeAttribute('title');
            
            element.addEventListener('mouseenter', function(e) {
                const tooltip = document.createElement('div');
                tooltip.className = 'custom-tooltip';
                tooltip.textContent = title;
                document.body.appendChild(tooltip);
                
                const rect = e.target.getBoundingClientRect();
                tooltip.style.left = rect.left + (rect.width / 2) - (tooltip.offsetWidth / 2) + 'px';
                tooltip.style.top = rect.top - tooltip.offsetHeight - 10 + 'px';
                
                setTimeout(() => tooltip.classList.add('show'), 10);
            });
            
            element.addEventListener('mouseleave', function() {
                const tooltip = document.querySelector('.custom-tooltip');
                if (tooltip) {
                    tooltip.classList.remove('show');
                    setTimeout(() => tooltip.remove(), 300);
                }
            });
        });
    }

    // Animated counters for statistics
    function initAnimatedCounters() {
        const counters = document.querySelectorAll('[data-count]');
        
        const animateCounter = (counter) => {
            const target = parseInt(counter.getAttribute('data-count'));
            const duration = 2000;
            const start = 0;
            const increment = target / (duration / 16);
            let current = start;
            
            const timer = setInterval(() => {
                current += increment;
                counter.textContent = Math.floor(current);
                
                if (current >= target) {
                    counter.textContent = target;
                    clearInterval(timer);
                }
            }, 16);
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    animateCounter(entry.target);
                    observer.unobserve(entry.target);
                }
            });
        });

        counters.forEach(counter => observer.observe(counter));
    }

    // Enhanced mobile menu
    function initMobileMenuEnhancements() {
        const navToggle = document.querySelector('.md-nav__toggle');
        const nav = document.querySelector('.md-nav--primary');
        
        if (navToggle && nav) {
            navToggle.addEventListener('change', function() {
                if (this.checked) {
                    nav.style.animation = 'slideInLeft 0.3s ease-out';
                } else {
                    nav.style.animation = 'slideOutLeft 0.3s ease-out';
                }
            });
        }
    }

    // Dynamic copyright year
    function initDynamicCopyright() {
        const copyrightElements = document.querySelectorAll('[data-dynamic-year]');
        const currentYear = new Date().getFullYear();
        
        copyrightElements.forEach(element => {
            element.textContent = element.textContent.replace(/\d{4}/, currentYear);
        });
    }

    // Enhanced keyboard navigation
    function initKeyboardNavigation() {
        document.addEventListener('keydown', function(e) {
            // Ctrl/Cmd + K to focus search
            if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
                e.preventDefault();
                const searchInput = document.querySelector('.md-search__input');
                if (searchInput) {
                    searchInput.focus();
                }
            }
            
            // Escape to close search
            if (e.key === 'Escape') {
                const searchInput = document.querySelector('.md-search__input');
                if (searchInput && document.activeElement === searchInput) {
                    searchInput.blur();
                }
            }
        });
    }

    // Page load animations
    function initPageLoadAnimations() {
        const animatedElements = document.querySelectorAll('.md-content__inner > *');
        
        animatedElements.forEach((element, index) => {
            element.style.opacity = '0';
            element.style.transform = 'translateY(30px)';
            element.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
            
            setTimeout(() => {
                element.style.opacity = '1';
                element.style.transform = 'translateY(0)';
            }, index * 100);
        });
    }

    // Theme transition effects
    function initThemeTransitions() {
        const themeToggles = document.querySelectorAll('[data-md-color-media]');
        
        themeToggles.forEach(toggle => {
            toggle.addEventListener('change', function() {
                document.body.style.transition = 'color 0.3s ease, background-color 0.3s ease';
                
                setTimeout(() => {
                    document.body.style.transition = '';
                }, 300);
            });
        });
    }

    // Enhanced table interactions
    function initTableEnhancements() {
        const tables = document.querySelectorAll('table');
        
        tables.forEach(table => {
            // Add wrapper for horizontal scrolling
            const wrapper = document.createElement('div');
            wrapper.className = 'table-wrapper';
            table.parentNode.insertBefore(wrapper, table);
            wrapper.appendChild(table);
            
            // Add hover effects to rows
            const rows = table.querySelectorAll('tbody tr');
            rows.forEach(row => {
                row.addEventListener('mouseenter', function() {
                    this.style.transform = 'scale(1.01)';
                    this.style.zIndex = '1';
                });
                
                row.addEventListener('mouseleave', function() {
                    this.style.transform = '';
                    this.style.zIndex = '';
                });
            });
        });
    }

    // Initialize all enhancements
    initSmoothScrolling();
    initReadingProgress();
    initCodeBlockEnhancements();
    initNavigationHighlighting();
    initDynamicThemeColor();
    initEnhancedSearch();
    initInteractiveTooltips();
    initAnimatedCounters();
    initMobileMenuEnhancements();
    initDynamicCopyright();
    initKeyboardNavigation();
    initPageLoadAnimations();
    initThemeTransitions();
    initTableEnhancements();

    console.log('🚢 PortLog UI Enhancements Loaded Successfully!');
});

// Add custom styles for JavaScript enhancements
const customStyles = `
    .reading-progress {
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 4px;
        z-index: 1000;
        background: rgba(21, 101, 192, 0.1);
    }

    .reading-progress__fill {
        height: 100%;
        background: linear-gradient(90deg, #1565C0, #00BCD4);
        transition: width 0.3s ease;
        width: 0%;
    }

    .code-language-label {
        position: absolute;
        top: 0.5rem;
        right: 0.5rem;
        background: rgba(21, 101, 192, 0.9);
        color: white;
        padding: 0.25rem 0.5rem;
        border-radius: 4px;
        font-size: 0.75rem;
        font-weight: 600;
        text-transform: uppercase;
        letter-spacing: 0.05em;
    }

    .line-numbers {
        position: absolute;
        left: 0;
        top: 0;
        bottom: 0;
        width: 2.5rem;
        background: rgba(21, 101, 192, 0.05);
        border-right: 1px solid rgba(21, 101, 192, 0.1);
        padding: 1rem 0.5rem;
        font-family: 'JetBrains Mono', monospace;
        font-size: 0.8rem;
        line-height: 1.5;
        color: rgba(21, 101, 192, 0.6);
        user-select: none;
    }

    .line-numbers span {
        display: block;
        text-align: right;
        padding-right: 0.5rem;
    }

    .custom-tooltip {
        position: absolute;
        background: rgba(21, 101, 192, 0.95);
        color: white;
        padding: 0.5rem 0.75rem;
        border-radius: 6px;
        font-size: 0.8rem;
        font-weight: 500;
        z-index: 1000;
        opacity: 0;
        transform: translateY(5px);
        transition: opacity 0.3s ease, transform 0.3s ease;
        pointer-events: none;
        white-space: nowrap;
        box-shadow: 0 4px 12px rgba(21, 101, 192, 0.3);
    }

    .custom-tooltip.show {
        opacity: 1;
        transform: translateY(0);
    }

    .custom-tooltip::after {
        content: '';
        position: absolute;
        top: 100%;
        left: 50%;
        transform: translateX(-50%);
        border: 5px solid transparent;
        border-top-color: rgba(21, 101, 192, 0.95);
    }

    .table-wrapper {
        overflow-x: auto;
        margin: 1rem 0;
        border-radius: 12px;
        box-shadow: 0 8px 32px rgba(21, 101, 192, 0.1);
    }

    @keyframes slideOutLeft {
        from {
            opacity: 1;
            transform: translateX(0);
        }
        to {
            opacity: 0;
            transform: translateX(-100%);
        }
    }

    /* Enhanced mobile responsiveness */
    @media screen and (max-width: 768px) {
        .reading-progress {
            height: 3px;
        }
        
        .code-language-label {
            font-size: 0.7rem;
            padding: 0.2rem 0.4rem;
        }
        
        .line-numbers {
            width: 2rem;
            font-size: 0.7rem;
        }
    }

    /* Performance optimization */
    .md-content__inner * {
        will-change: transform;
    }
`;

// Inject custom styles
const styleSheet = document.createElement('style');
styleSheet.textContent = customStyles;
document.head.appendChild(styleSheet);