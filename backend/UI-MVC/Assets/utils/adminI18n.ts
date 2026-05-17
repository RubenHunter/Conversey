declare global {
    interface Window {
        __AdminI18n?: { lang?: string; strings?: Record<string, string> }
    }
}

export function t(key: string, fallback?: string): string {
    const dict = window.__AdminI18n?.strings;
    if (dict && Object.prototype.hasOwnProperty.call(dict, key)) {
        return dict[key];
    }
    return fallback ?? key;
}

export function getLang(): string | undefined {
    return window.__AdminI18n?.lang;
}

