import type { ProjectTheme } from '../models/project'

export const THEME_PRESETS: Record<string, ProjectTheme> = {
    default: { primary: '#6c5ce7', secondary: '#db99c8', accent: '#cd6f88', preset: 'default', font: 'Helvetica' },
    ocean:   { primary: '#0ea5e9', secondary: '#06b6d4', accent: '#0284c7', preset: 'ocean',   font: 'Helvetica' },
    forest:  { primary: '#16a34a', secondary: '#4ade80', accent: '#84cc16', preset: 'forest',  font: 'Helvetica' },
    sunset:  { primary: '#ea580c', secondary: '#f59e0b', accent: '#ef4444', preset: 'sunset',  font: 'Helvetica' },
}

const FONT_FAMILIES: Record<string, string> = {
    'Helvetica':       '"Helvetica Neue", Helvetica, Arial, sans-serif',
    'Verdana':         'Verdana, Geneva, Tahoma, sans-serif',
    'Tahoma':          'Tahoma, Geneva, Verdana, sans-serif',
    'Georgia':         'Georgia, "Times New Roman", serif',
    'Times New Roman': '"Times New Roman", Times, serif',
}

export function applyTheme(theme: ProjectTheme | undefined): void {
    const t = theme ?? THEME_PRESETS.default
    const root = document.documentElement
    root.style.setProperty('--color-primary', t.primary)
    root.style.setProperty('--color-secondary', t.secondary)
    root.style.setProperty('--color-accent', t.accent)
    root.style.setProperty('--font-primary', FONT_FAMILIES[t.font] ?? FONT_FAMILIES['Helvetica'])
}
