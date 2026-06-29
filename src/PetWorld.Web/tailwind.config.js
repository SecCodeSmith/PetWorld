/** @type {import('tailwindcss').Config} */
// Token vocabulary mirrors design-reference/DESIGN.md (and the original Stitch config),
// so class names like `bg-surface`, `p-md`, `text-headline-md`, `rounded-card` map 1:1.
module.exports = {
  content: ['./Components/**/*.{razor,html,cs}', './wwwroot/index.html'],
  theme: {
    extend: {
      colors: {
        primary: '#096444',
        'primary-container': '#2e7d5b',
        'primary-hover': '#176b4b',
        'on-primary': '#ffffff',
        'secondary-container': '#c2e9c9',
        'on-secondary-container': '#2c4e36',
        surface: '#f7faf5',
        'surface-low': '#f1f5ef',
        'surface-container': '#ebefea',
        'surface-high': '#e6e9e4',
        'surface-highest': '#e0e3df',
        white: '#ffffff',
        'on-surface': '#181d1a',
        'on-surface-variant': '#3f4943',
        outline: '#6f7a72',
        'outline-variant': '#bec9c1',
        error: '#ba1a1a',
        'error-container': '#ffdad6',
        'on-error-container': '#93000a',
      },
      spacing: {
        xs: '4px',
        sm: '12px',
        base: '8px',
        md: '24px',
        lg: '48px',
        xl: '80px',
        gutter: '24px',
      },
      maxWidth: {
        'container-max': '1280px',
        narrow: '820px',
      },
      borderRadius: {
        card: '16px',
        control: '10px',
        lg: '0.5rem',
        xl: '0.75rem',
      },
      boxShadow: {
        card: '0 4px 20px rgba(31, 36, 33, 0.05)',
        float: '0 8px 30px rgba(31, 36, 33, 0.08)',
      },
      fontFamily: {
        sans: ['"Plus Jakarta Sans"', 'system-ui', '-apple-system', 'Segoe UI', 'sans-serif'],
      },
      fontSize: {
        'display-lg': ['48px', { lineHeight: '1.2', letterSpacing: '-0.02em', fontWeight: '700' }],
        'headline-lg': ['32px', { lineHeight: '1.3', fontWeight: '700' }],
        'headline-md': ['24px', { lineHeight: '1.4', fontWeight: '600' }],
        'body-lg': ['18px', { lineHeight: '1.6' }],
        'label-md': ['14px', { lineHeight: '1.2', letterSpacing: '0.03em', fontWeight: '600' }],
      },
    },
  },
  plugins: [],
};
